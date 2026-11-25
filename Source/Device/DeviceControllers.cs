using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GSM.Core.Source.Common.DeviceDefinitions;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        //Device Count
        public bool DeviceCountGet()
        {
            int deviceCount = DeviceDefinitions.NUMBER_INVALID;
            return this.DeviceCountGet(ref deviceCount, true);
        }
        public bool DeviceCountGet(ref int DeviceCount)
        {
            return this.DeviceCountGet(ref DeviceCount, true);
        }
        public bool DeviceCountGet(ref int DeviceCount, bool InternalValueSet)
        {
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            try
            {
                if (!this.TrySetBusy()) return this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceCountGet));
                if (!this.InternalStatusCheck(INTERNAL_STATUS.BASIC_LEVEL, nameof(this.DeviceCountGet))) return this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.DeviceCountGet));
                this.ErrorReset();
                funcResult = this.DeviceCountGetWorkflow(ref DeviceCount, InternalValueSet);
                ////////_runtimeFlags Updates
                ////////_runtimeInfo Updates
                this.InternalStatusUpdate();
                return this.FuncResult(funcResult);
            }
            finally
            {
                this.DeviceBusyDeactive();
                //this.OnOperationCompleted(DEVICE_OPERATION.UNKNOWN, funcResult);
            }
        }
        //End : Device Count

        //Device Identity
        public bool DeviceIdentity(int DeviceIndex, ref int DeviceNumber, ref string IMEI)
        {
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            try
            {
                if (!this.TrySetBusy()) return this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceIdentity));
                if (!this.InternalStatusCheck(INTERNAL_STATUS.BASIC_LEVEL, nameof(this.DeviceIdentity))) return this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.DeviceIdentity));
                this.ErrorReset();
                funcResult = this.DeviceIdentityComplexWorkflow(DeviceIndex, ref DeviceNumber, ref IMEI);
                ////////_runtimeFlags Updates
                ////////_runtimeInfo Updates
                this.InternalStatusUpdate();
                return this.FuncResult(funcResult);
            }
            finally
            {
                this.DeviceBusyDeactive();
                //this.OnOperationCompleted(DEVICE_OPERATION.UNKNOWN, funcResult);
            }
        }
        //Device Identity

        //Device Number Change
        public bool DeviceNumberChange(int DeviceNumberNew)
        {
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            try
            {
                if (!this.TrySetBusy()) return this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceNumberChange));
                if (!this.InternalStatusCheck(INTERNAL_STATUS.BASIC_LEVEL, nameof(this.DeviceNumberChange))) return this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.DeviceNumberChange));
                this.ErrorReset();
                //funcResult = this.DeviceNumberChangeComplexWorkflow(DeviceIndex, DeviceNumberNew);
                ////////_runtimeFlags Updates
                ////////_runtimeInfo Updates
                this.InternalStatusUpdate();
                return this.FuncResult(funcResult);
            }
            finally
            {
                this.DeviceBusyDeactive();
                //this.OnOperationCompleted(DEVICE_OPERATION.UNKNOWN, funcResult);
            }
        }
        //End : Device Number Change

        //Device Index Unselect
        public bool DeviceUnselect()
        {
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            try
            {
                if (!this.TrySetBusy()) return this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceUnselect));
                if (!this.InternalStatusCheck(INTERNAL_STATUS.BASIC_LEVEL, nameof(this.DeviceUnselect))) return this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.DeviceUnselect));
                this.ErrorReset();
                funcResult = this.DeviceUnselectComplexWorkflow();
                ////////_runtimeFlags Updates
                ////////_runtimeInfo Updates
                this.InternalStatusUpdate();
                return this.FuncResult(funcResult);
            }
            finally
            {
                this.DeviceBusyDeactive();
                //this.OnOperationCompleted(DEVICE_OPERATION.UNKNOWN, funcResult);
            }
        }
        //End : Device Index Unselect
        
        //Refresh
        public bool DeviceRefresh()
        {
            return this.DeviceRefresh(true);
        }
        public bool DeviceRefresh(bool PowerRestartAllow)
        {
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            try
            {
                if (!this.TrySetBusy()) return this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceRefresh));
                if (!this.InternalStatusCheck(INTERNAL_STATUS.SINGLE_DEVICE, nameof(this.DeviceRefresh))) return this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.DeviceRefresh));
                funcResult = this.DeviceRefreshComplexWorkFlow(PowerRestartAllow);
                ////////_runtimeFlags Updates
                ////////_runtimeInfo Updates
                this.InternalStatusUpdate();
                return this.FuncResult(funcResult);
            }
            finally
            {
                this.DeviceBusyDeactive();
                //this.OnOperationCompleted(DEVICE_OPERATION.UNKNOWN, funcResult);
            }
        }
        //End : Refresh

        //SMS Send
        public bool SmsSend(string PhoneNumber, string SmsText, ref List<string> SMSReference)
        {
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            try
            {
                if (!this.TrySetBusy()) return this.ErrorSet(ERROR_CODE.BUSY, nameof(this.SmsSend));
                if (!this.InternalStatusCheck(INTERNAL_STATUS.SINGLE_DEVICE, nameof(this.SmsSend))) return this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.SmsSend));
                DeviceDefinitions.SmsSendData smsSendData = null;
                funcResult = this.SmsSendDataInitial(ref smsSendData, PhoneNumber, SmsText);
                if (funcResult == FUNC_RESULT.SUCCESS) funcResult = this.SendConfigurationSetComplexWorkflow(SMS_CHAR_MODE.UCS2, SMS_TYPE.PERSIAN);
                if (funcResult == FUNC_RESULT.SUCCESS) funcResult = this.SMSSendWorkflow(smsSendData, ref SMSReference);
                ////////_runtimeFlags Updates
                ////////_runtimeInfo Updates
                this.InternalStatusUpdate();
                return this.FuncResult(funcResult);
            }
            finally
            {
                this.DeviceBusyDeactive();
                //this.OnOperationCompleted(DEVICE_OPERATION.UNKNOWN, funcResult);
            }
        }
        //SMS Send

        //Helper
        private bool FuncResult(FUNC_RESULT funcResult)
        {
            if (funcResult == FUNC_RESULT.SUCCESS) return true;
            else return false;
        }
        //Helper
    }
}
