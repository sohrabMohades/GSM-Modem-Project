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
            bool result = false;
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            if (!this.TrySetBusy())
            {
                result = false;
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceCountGet));
                return result;
            }
            if (!this.InternalStatusCheck(INTERNAL_STATUS.BASIC_LEVEL , nameof(this.DeviceCountGet)))
            {
                result = false;
                this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.DeviceCountGet));
                return result;
            }
            try
            {
                this.ErrorReset();
                funcResult = this.DeviceCountGetWorkflow(ref DeviceCount, InternalValueSet);
                if (funcResult == FUNC_RESULT.SUCCESS)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                ////////_runtimeFlags Updates
                ////////_runtimeInfo Updates
                this.InternalStatusUpdate();
                return result;
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
            bool result = false;
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            if (!this.TrySetBusy())
            {
                result = false;
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceIdentity));
                return result;
            }
            if (!this.InternalStatusCheck(INTERNAL_STATUS.BASIC_LEVEL, nameof(this.DeviceIdentity)))
            {
                result = false;
                this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.DeviceIdentity));
                return result;
            }
            try
            {
                this.ErrorReset();
                funcResult = this.DeviceIdentityComplexWorkflow(DeviceIndex, ref DeviceNumber, ref IMEI);
                if (funcResult == FUNC_RESULT.SUCCESS)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                ////////_runtimeFlags Updates
                ////////_runtimeInfo Updates
                this.InternalStatusUpdate();
                return result;
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
            bool result = false;
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            if (!this.TrySetBusy())
            {
                result = false;
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceNumberChange));
                return result;
            }
            if (!this.InternalStatusCheck(INTERNAL_STATUS.BASIC_LEVEL, nameof(this.DeviceNumberChange)))
            {
                result = false;
                this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.DeviceNumberChange));
                return result;
            }
            try
            {
                this.ErrorReset();
                funcResult = this.DeviceNumberChangeComplexWorkflow(DeviceIndex, DeviceNumberNew);
                if (funcResult == FUNC_RESULT.SUCCESS)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                ////////_runtimeFlags Updates
                ////////_runtimeInfo Updates
                this.InternalStatusUpdate();
                return result;
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
            bool result = false;
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            if (!this.TrySetBusy())
            {
                result = false;
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceUnselect));
                return result;
            }
            if (!this.InternalStatusCheck(INTERNAL_STATUS.BASIC_LEVEL, nameof(this.DeviceUnselect)))
            {
                result = false;
                this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.DeviceUnselect));
                return result;
            }
            try
            {
                this.ErrorReset();
                funcResult = this.DeviceUnselectComplexWorkflow();
                if (funcResult == FUNC_RESULT.SUCCESS)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                ////////_runtimeFlags Updates
                ////////_runtimeInfo Updates
                this.InternalStatusUpdate();
                return result;
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
            bool result = false;
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            if (!this.TrySetBusy())
            {
                result = false;
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceRefresh));
                return result;
            }
            if (!this.InternalStatusCheck(INTERNAL_STATUS.SINGLE_DEVICE, nameof(this.DeviceRefresh)))
            {
                result = false;
                this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.DeviceRefresh));
                return result;
            }
            try
            {
                funcResult = this.DeviceRefreshComplexWorkFlow(PowerRestartAllow);
                if (funcResult == FUNC_RESULT.SUCCESS)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                ////////_runtimeFlags Updates
                ////////_runtimeInfo Updates
                this.InternalStatusUpdate();
                return result;
            }
            finally
            {
                this.DeviceBusyDeactive();
                //this.OnOperationCompleted(DEVICE_OPERATION.UNKNOWN, funcResult);
            }
        }
        //End : Refresh

    }
}
