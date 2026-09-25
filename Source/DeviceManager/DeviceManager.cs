using GSM.Core.Source.Common;
using GSM.Core.Source.Device;
using GSM.Core.Source.FileManager;
using GSM.Core.Source.SMSManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GSM.Core.Source.Common.DeviceDefinitions;

namespace GSM.Core.Source.DeviceManager
{
    public partial class DeviceManager
    {
        const int DEVICE_WAIT_MS = 10000;
        private enum DEVICE_STATUS
        {
            NORMAL = 0,
            FAULTED = 1,
            WORKING = 2
        }
        private enum DEVICE_DETECTION_STATUS
        {
            FAIL = 0,
            IMEI_FAIL = 1,
            OKEY = 2
        }
        private enum SMS_STATUS
        {
            READY = 0,
            SENDING = 1,
            SEND_FAIL = 2,
            SENDED = 3
        }
        private class DeviceRuntime
        {
            public Device.Device Device { get; }
            public int RefreshFailCount { get; set; }
            public int SmsFailCount { get; set; }
            public int SmsSuccessCount { get; set; }
            public int RefreshConsecutiveFailCount { get; set; }
            public int SmsConsecutiveFailCount { get; set; }
            public int SmsConsecutiveSuccessCount { get; set; }
            public int SmsIndex { get; set; }
            public DEVICE_STATUS Status { get; set; }
            //for test and debug
            public DEVICE_OPERATION LastOperation { get; set; }
            public DeviceRuntime(Device.Device device)
            {
                Device = device;
                this.RefreshFailCount = 0;
                this.SmsFailCount = 0;
                this.SmsSuccessCount = 0;
                this.RefreshConsecutiveFailCount = 0;
                this.SmsConsecutiveFailCount = 0;
                this.SmsConsecutiveSuccessCount = 0;
                this.SmsIndex = -1;
                this.Status = DEVICE_STATUS.NORMAL;
                this.LastOperation = DEVICE_OPERATION.UNKNOWN;
            }
        }
        private class DeviceDiscoveryItem
        {
            public int DeviceIndex { get; set; }
            public int DeviceNumber { get; set; }
            public string IMEI { get; set; }
            public DEVICE_DETECTION_STATUS Status { get; set; }
            public DeviceDiscoveryItem()
            {
                this.DeviceIndex = DeviceDefinitions.NUMBER_INVALID;
                this.DeviceNumber = DeviceDefinitions.NUMBER_INVALID;
                this.IMEI = DeviceDefinitions.STRING_INVALID;
                this.Status = DEVICE_DETECTION_STATUS.FAIL;
            }
        }


        private volatile bool _workEnable = false;
        private bool _firstStart = true;
        private Device.Device _mainDevice;
        private Dictionary<Device.Device, DeviceRuntime> _devices = new Dictionary<Device.Device, DeviceRuntime>();
        private SMSManager.SMSManager _smsManager = new SMSManager.SMSManager();
        //if UI or Test want see devices result
        public IReadOnlyList<Device.Device> Devices => _devices.Keys.ToList();
        //Helper : List manger
        private DeviceRuntime GetDeviceRuntime(Device.Device device)
        {
            if (_devices.TryGetValue(device, out DeviceRuntime runtime))
            {
                return runtime;
            }
            return null;
        }
        //End : Helper : List manger

        private FileManager.FileManager _fileManage;

        private const int MAX_REFRESH_FAIL = 10;
        private const int MAX_SMS_FAIL = 5;


        public DeviceManager()
        {
        }

        public void StartWorks()
        {
            if (this._workEnable)
            {
                return;
            }
            this._workEnable = true;
            foreach (DeviceRuntime runtime in _devices.Values)
            {
                if (this._firstStart)
                {
                    this.DeviceRunOperation(runtime, runtime.Device, DEVICE_OPERATION.DEVICE_REFRESH);
                }
                else
                {
                    this.DeviceRunOperation(runtime, runtime.Device, runtime.LastOperation);
                }
            }
            this._firstStart = false;
        }
        public void StopWorks()
        {
            _workEnable = false;
        }
        private void DeviceOperationCompleted(Device.Device sender, DeviceDefinitions.DEVICE_OPERATION operation, bool result)
        {
            DeviceDefinitions.DEVICE_OPERATION nextOperation = DeviceDefinitions.DEVICE_OPERATION.UNKNOWN;
            if (sender == null)
                return;
            DeviceRuntime runtime = this.GetDeviceRuntime(sender);
            if (runtime == null)
            {
                return;
            }
            this.SmsDataUpdate(runtime, result);
            this.DeviceOperationCompletedSetValue(runtime, operation, result);
            nextOperation = this.NextOperation(runtime, operation, result);
            _ = this.DeviceRunOperationAsync(runtime, sender, nextOperation);
        }
        private void DeviceOperationCompletedSetValue(DeviceRuntime runtime, DeviceDefinitions.DEVICE_OPERATION LastOperation, bool OperationResult)
        {
            switch (LastOperation)
            {
                case DEVICE_OPERATION.DEVICE_REFRESH:
                    //Refresh Succeed
                    if (OperationResult)
                    {
                        runtime.RefreshConsecutiveFailCount = 0;
                        runtime.SmsConsecutiveFailCount = 0;
                        runtime.SmsConsecutiveSuccessCount = 0;
                    }
                    //Refresh Fail
                    else
                    {
                        runtime.RefreshConsecutiveFailCount++;
                    }
                    break;

                case DEVICE_OPERATION.SMS_SEND:
                    //SMS Send Succeed
                    if (OperationResult)
                    {
                        runtime.SmsSuccessCount++;
                        runtime.SmsConsecutiveFailCount = 0;
                        runtime.SmsConsecutiveSuccessCount++;
                    }
                    //SMS Send Fail
                    else
                    {
                        runtime.SmsFailCount++;
                        runtime.SmsConsecutiveFailCount++;
                        runtime.SmsConsecutiveSuccessCount = 0;
                    }
                    break;

                default:
                    break;
            }
        }
        private DeviceDefinitions.DEVICE_OPERATION NextOperation(DeviceRuntime runtime, DeviceDefinitions.DEVICE_OPERATION LastOperation, bool OperationResult)
        {
            if (runtime.Status == DEVICE_STATUS.FAULTED)
            {
                return DeviceDefinitions.DEVICE_OPERATION.DEVICE_STOP;
            }
            DeviceDefinitions.DEVICE_OPERATION result = DeviceDefinitions.DEVICE_OPERATION.UNKNOWN;
            switch (LastOperation)
            {
                case DEVICE_OPERATION.DEVICE_REFRESH:
                    {
                        //Refresh Succeed
                        if (OperationResult)
                        {
                            result = DeviceDefinitions.DEVICE_OPERATION.SMS_SEND;
                        }
                        //Refresh Fail
                        else
                        {
                            //Refresh Fail more than MAX
                            if (runtime.RefreshConsecutiveFailCount > MAX_REFRESH_FAIL)
                            {
                                runtime.Status = DEVICE_STATUS.FAULTED;
                                result = DeviceDefinitions.DEVICE_OPERATION.DEVICE_STOP;
                            }
                            //Refresh Fail less than MAX
                            else
                            {
                                result = DeviceDefinitions.DEVICE_OPERATION.DEVICE_REFRESH;
                            }
                        }
                        break;
                    }

                case DEVICE_OPERATION.SMS_SEND:
                    {
                        //SMS Send Succeed
                        if (OperationResult)
                        {
                            result = DeviceDefinitions.DEVICE_OPERATION.SMS_SEND;
                        }
                        //SMS Send Fail
                        else
                        {
                            if (runtime.SmsConsecutiveFailCount > MAX_SMS_FAIL)
                            {
                                result = DEVICE_OPERATION.DEVICE_REFRESH;
                            }
                            else
                            {
                                result = DEVICE_OPERATION.SMS_SEND;
                            }
                        }
                        break;
                    }

                default:
                    {
                        result = DeviceDefinitions.DEVICE_OPERATION.UNKNOWN;
                        break;
                    }
            }
            runtime.LastOperation = result;
            return result;
        }
        private async Task DeviceRunOperationAsync(DeviceRuntime runtime, Device.Device sender, DeviceDefinitions.DEVICE_OPERATION NextOperation)
        {
            if (this._workEnable)
            {
                switch (NextOperation)
                {
                    case DEVICE_OPERATION.DEVICE_REFRESH:
                        this.DeviceRefresh(sender);
                        break;

                    case DEVICE_OPERATION.SMS_SEND:
                        string phoneNumber = string.Empty, smsText = string.Empty;
                        if (this.SmsDataGet(runtime, ref phoneNumber, ref smsText))
                        {
                            this.SmsSend(sender, phoneNumber, smsText);
                        }
                        else
                        {
                            await DeviceWaitAsync(runtime, sender, DeviceDefinitions.DEVICE_OPERATION.SMS_SEND);
                        }
                        break;

                    default:
                        break;
                }
            }
        }
        private void DeviceRefresh(Device.Device sender)
        {
            Task.Run(() =>
            {
                try
                {
                    sender.DeviceRefresh();
                }
                catch (Exception)
                {
                }
            });
        }
        private void SmsSend(Device.Device sender, string PhoneNumber, string SmsText)
        {
            Task.Run(() =>
            {
                try
                {
                    sender.SmsSend(PhoneNumber, SmsText);
                }
                catch (Exception)
                {
                }
            });

        }
        private async Task DeviceWaitAsync(DeviceRuntime runtime, Device.Device sender, DeviceDefinitions.DEVICE_OPERATION NextOperation)
        {
            await Task.Delay(DEVICE_WAIT_MS);
            this.DeviceRunOperation(runtime, sender, NextOperation);
        }

        private bool SmsDataGet(DeviceRuntime runtime, ref string PhoneNumber, ref String SmsText)
        {
            bool result = false;
            int index = -1;
            result = this._smsManager.GetData(ref index, ref PhoneNumber, ref SmsText);
            runtime.SmsIndex = index;
            return result;
        }
        private void SmsDataUpdate(DeviceRuntime runtime, bool Result)
        {
            if (runtime.LastOperation == DEVICE_OPERATION.SMS_SEND)
            {
                this._smsManager.UpdateData(runtime.SmsIndex, Result);
            }
        }

        public bool DevicesConfig()
        {
            bool result = true;
            List<DeviceDiscoveryItem> deviceDiscoveryItem = new List<DeviceDiscoveryItem>();
            foreach (var item in _devices.Values)
            {
                item.Device.Dispose();
            }
            _devices = new Dictionary<Device.Device, DeviceRuntime>();
            if (result)
            {
                result = this.FileManagerCreate();
            }
            if (result)
            {
                result = this.MainDllFileName();
            }
            if (result)
            {
                result = this.MainDeviceCreate();
            }
            if (result)
            {
                result = this.DevicesCount();
            }
            //for test , must REMOVE
            Console.WriteLine("Devices Count = " + this._mainDevice.DeviceCount.ToString());
            //End : for test , must REMOVE
            if (result)
            {
                this.DevicesDiscoveryList(deviceDiscoveryItem);
            }
            if (result)
            {
                result = this.DevicesListCreation(deviceDiscoveryItem);
            }
            //this._mainDevice.Dispose();
            this._mainDevice.DeviceUnselect();
            //for test , must REMOVE
            Console.WriteLine("Devices Number Add = " + this._devices.Count.ToString());
            foreach (var item in this._devices)
            {
                Console.WriteLine("Device Index = " + item.Key.DeviceIndex + " - Device Number = " + item.Key.DeviceNumber);
            }
            //End : for test , must REMOVE
            return result;
        }
        private bool FileManagerCreate()
        {
            try
            {
                this._fileManage = new FileManager.FileManager();
                if (this._fileManage.Status != FileManager.FileManager.STATUS.READY)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                return false;
            }
        }
        private bool MainDllFileName()
        {
            if (this._fileManage.Status == FileManager.FileManager.STATUS.READY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool MainDeviceCreate()
        {
            this._mainDevice = new Device.Device(this._fileManage.MainFile);
            if (this._mainDevice.ErrorCode == Device.Device.ERROR_CODE.NO_ERROR)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool DevicesCount()
        {
            return this._mainDevice.DeviceCountGet();
        }
        private void DevicesDiscoveryList(List<DeviceDiscoveryItem> deviceDiscoveryItem)
        {
            bool result = false;
            int deviceNumber = 0, deviceCount = this._mainDevice.DeviceCount;
            string imei = string.Empty;
            DeviceDiscoveryItem temp;
            for (int index = 0; index < deviceCount; index++)
            {
                temp = new DeviceDiscoveryItem();
                //first try read data
                result = this._mainDevice.DeviceIdentity(index, ref deviceNumber, ref imei);
                if (!result)
                {
                    //second try read data
                    result = this._mainDevice.DeviceIdentity(index, ref deviceNumber, ref imei);
                    if (!result)
                    {
                        //third try read data
                        result = this._mainDevice.DeviceIdentity(index, ref deviceNumber, ref imei);
                    }
                }
                if (result)
                {
                    temp.DeviceIndex = index;
                    temp.DeviceNumber = deviceNumber;
                    temp.IMEI = imei;
                    temp.Status = DEVICE_DETECTION_STATUS.OKEY;
                }
                else
                {
                    if (deviceNumber == DeviceDefinitions.NUMBER_INVALID)
                    {
                        temp.DeviceIndex = index;
                        temp.DeviceNumber = DeviceDefinitions.NUMBER_INVALID;
                        temp.IMEI = DeviceDefinitions.STRING_INVALID;
                        temp.Status = DEVICE_DETECTION_STATUS.FAIL;
                    }
                    else
                    {
                        temp.DeviceIndex = index;
                        temp.DeviceNumber = deviceNumber;
                        temp.IMEI = DeviceDefinitions.STRING_INVALID;
                        temp.Status = DEVICE_DETECTION_STATUS.IMEI_FAIL;
                    }
                }
                deviceDiscoveryItem.Add(temp);
            }
        }
        private bool DevicesListCreation(List<DeviceDiscoveryItem> deviceDiscoveryItem)
        {
            foreach (var item in deviceDiscoveryItem)
            {
                bool result = false;
                string fileName = string.Empty;
                Device.Device device = null;
                if (item.Status == DEVICE_DETECTION_STATUS.OKEY || item.Status == DEVICE_DETECTION_STATUS.IMEI_FAIL)
                {
                    result = this.NewDllFileName(ref fileName);
                    if (result)
                    {
                        this.NewDeviceCreate(ref device, fileName, item.DeviceIndex);
                    }
                }
            }
            if (this._devices.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool NewDllFileName(ref string FileName)
        {
            if (this._fileManage.Status == FileManager.FileManager.STATUS.READY)
            {
                if (this._fileManage.CreateNewFile(out FileName))
                {
                    return true;
                }
                else
                {
                    FileName = DeviceDefinitions.STRING_INVALID;
                    return false;
                }
            }
            else
            {
                FileName = DeviceDefinitions.STRING_INVALID;
                return false;
            }
        }
        private void NewDeviceCreate(ref Device.Device device, string FileName, int DeviceIndex)
        {
            bool result = false;
            try
            {
                device = new Device.Device(FileName);
                device.DeviceIndex = DeviceIndex;
                result = device.DeviceNumberChange(DeviceIndex);
                if (result)
                {
                    this.DeviceAdd(device);
                }
                else
                {
                    //device.Dispose();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                device?.Dispose();
            }
        }
        private void DeviceAdd(Device.Device device)
        {
            if (device == null)
            {
                return;
            }
            if (_devices.ContainsKey(device))
            {
                return;
            }
            device.DeviceOperationCompleted += DeviceOperationCompleted;
            _devices.Add(device, new DeviceRuntime(device));
        }

        public void SmsListFill(List<(int ExternalIndex, string PhoneNumber, string SmsText)> smsList)
        {
            this._smsManager.AddData(smsList);
        }
    }
}
