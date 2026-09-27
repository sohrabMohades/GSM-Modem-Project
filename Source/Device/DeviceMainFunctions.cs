using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        //DeviceManager -> Discovery Scope
        //Device Identity
        public bool DeviceIdentity(int DeviceIndex, ref int DeviceNumber, ref string IMEI)
        {
            bool result = false;
            if (this.TrySetBusy())
            {
                result = false;
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceIdentity));
                return result;
            }
            try
            {
                result = this.DeviceIdentityWorkFlow(DeviceIndex, ref DeviceNumber, ref IMEI);
                return result;
            }
            finally
            {
                this.DeviceBusyDeactive(nameof(this.DeviceIdentity), result);
            }
        }
        private bool DeviceIdentityWorkFlow(int DeviceIndex, ref int DeviceNumber, ref string IMEI)
        {
            bool result = true;
            string imei = string.Empty;
            if (result)
            {
                result = this.DeviceSelectByIndexWorkflow(DeviceIndex, true, true);
                //if result => this._deviceNumber fill by correct value
            }
            if (result)
            {
                result = this.GSMIsConnectWorkflow(false);
                if (result)
                {
                    result = this.IMEIGetWorkflow(ref imei, true);
                    //if result => this._imei fill by correct value
                }
                else
                {
                    result = this.DeviceRestart();
                    if (result)
                    {
                        result = this.GSMIsConnectWorkflow(false);
                        if (result)
                        {
                            result = this.IMEIGetWorkflow(ref imei, true);
                            //if result => this._imei fill by correct value
                        }
                    }
                }
            }
            this.DeviceIdentitySetValue(this.DeviceNumber, this.IMEI, ref DeviceNumber, ref IMEI);
            return result;
        }
        private void DeviceIdentitySetValue(int DeviceNumberNewValue, string IMEINewValue, ref int DeviceNumber, ref string IMEI)
        {
            DeviceNumber = DeviceNumberNewValue;
            IMEI = IMEINewValue;
        }
        //End : Device Identity
        //End : DeviceManager -> Discovery Scope

        //DeviceManager -> Device wroking scope
        //Device Number Change
        public bool DeviceNumberChange(int DeviceNumberNew)
        {
            bool result = false;
            if (this.TrySetBusy())
            {
                result = false;
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceNumberChange));
                return result;
            }
            if (!this.DeviceNumberChangeAllow(nameof(DeviceNumberChange)))
            {
                result = false;
                return result;
            }
            try
            {
                result = this.DeviceNumberChangeWorkFlow(DeviceIndex, DeviceNumberNew);
                if (result)
                {
                    this.DeviceNumberChanged();
                }
                return result;
            }
            finally
            {
                this.DeviceBusyDeactive(nameof(this.DeviceNumberChange) , result);
            }
        }
        private bool DeviceNumberChangeWorkFlow(int DeviceIndex, int DeviceNumber)
        {
            bool result = true;
            if (result)
            {
                result = this.DeviceSelectByIndexWorkflow(DeviceIndex, true, true);
                //if result => this._deviceNumber fill by correct value
            }
            if (result)
            {
                if(this.DeviceNumber == DeviceNumber)
                {
                    return true;
                }
                result = this.DeviceNumberSetWorkflow(DeviceNumber , true , true);
            }
            return result;
        }
        //Device Number Change
        //Device Index Unselect
        public bool DeviceUnselect()
        {
            bool result = false;
            if (this.TrySetBusy())
            {
                result = false;
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceUnselect));
                return result;
            }
            try
            {
                result = this.DeviceUnselectWorkFlow();
                return result;
            }
            finally
            {
                this.DeviceBusyDeactive(nameof(this.DeviceUnselect), result);
            }
        }
        private bool DeviceUnselectWorkFlow()
        {
            bool result = true;
            if (result)
            {
                result = this.DeviceSelectByIndexWorkflow(DeviceDefinitions.DEVICE_INDEX_MAX, false, false);
                //if result = false => this._deviceNumber is invalid and DLL unselect this index
            }
            return !result;
        }
        //End : Device Index Unselect
    }
}
