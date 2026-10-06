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
            }
        }
        //End : Device Count

    }
}
