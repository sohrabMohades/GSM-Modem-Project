using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        //Error Manager
        private void ErrorReset()
        {
            this._errorCode = ERROR_CODE.NO_ERROR;
            this._errorMessage = string.Empty;
        }
        private bool DllWrapperErrorCheck()
        {
            try
            {
                if (this._dllWrapper.ErrorCode == DllWrapper.ERROR_CODE.NO_ERROR)
                {
                    return true;
                }
                else
                {
                    this.DllWrapperErrorCheckSetValue(this._dllWrapper.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue(ex.Message);
                return false;
            }
        }
        private void DllWrapperErrorCheckSetValue(string error)
        {
            this._errorCode = ERROR_CODE.DLL_WRAPPER;
            this._errorMessage = error;
        }
        private bool ErrorSet(ERROR_CODE ErrorCode, string ErrorMessage)
        {
            this._errorCode = ErrorCode;
            this._errorMessage = ErrorMessage;
            return false;
        }
        private bool BaseErrorCheck()
        {
            if (this._errorCode == ERROR_CODE.DLL_WRAPPER || this._errorCode == ERROR_CODE.INTERNAL_STATUS || this._errorCode == ERROR_CODE.INVALID_INPUT)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //Error Manager
        //Busy Flag manager
        private bool TrySetBusy()
        {
            return Interlocked.CompareExchange(ref this._busyFlag, 1, 0) == 1;
        }
        private void DeviceBusyDeactive(string FuncName, bool FuncResult)
        {
            DeviceDefinitions.DEVICE_OPERATION operation = DeviceDefinitions.DEVICE_OPERATION.UNKNOWN;
            switch (FuncName)
            {
                case nameof(this.DeviceCountGet):
                    operation = DeviceDefinitions.DEVICE_OPERATION.DEVICE_COUNT;
                    break;

                case nameof(this.DeviceIdentity):
                    operation = DeviceDefinitions.DEVICE_OPERATION.DEVICE_IDENTITY;
                    break;

                case nameof(this.DeviceNumberChange):
                    operation = DeviceDefinitions.DEVICE_OPERATION.DEVICE_NUMBER_CHANGE;
                    break;

                case nameof(this.DeviceUnselect):
                    operation = DeviceDefinitions.DEVICE_OPERATION.DEVICE_UNSELECT;
                    break;

                case nameof(this.DeviceRefresh):
                    operation = DeviceDefinitions.DEVICE_OPERATION.DEVICE_REFRESH;
                    break;

                case nameof(this.SmsSend):
                    operation = DeviceDefinitions.DEVICE_OPERATION.SMS_SEND;
                    break;

                default:
                    operation = DeviceDefinitions.DEVICE_OPERATION.UNKNOWN;
                    break;
            }
            Interlocked.Exchange(ref this._busyFlag, 0);
            this.OnOperationCompleted(operation, FuncResult);
        }
        //Busy Flag manager
        //Timeout
        private void TimeOutStart()
        {
            this._timeOutCounter = DateTime.Now;
        }
        private bool TimeOutEnd()
        {
            if ((DateTime.Now - this._timeOutCounter).TotalMilliseconds >= TIME_OUT_MAX_MS)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //End : Timeout
        //Device Number Change Allow
        private bool DeviceNumberChangeAllow(string FuncName)
        {
            if (!this._deviceNumberChangeAllow)
            {
                return this.ErrorSet(ERROR_CODE.DEVICE_NUMBER_CHANGE_NOT_ALLOW, FuncName);
            }
            return true;
        }
        private void DeviceNumberChanged()
        {
            this._deviceNumberChangeAllow = false;
        }
        //End : Device Number Change Allow
    }
}
