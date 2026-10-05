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
        //Group 1 : Status
        private sealed class DeviceRuntimeState
        {
            public int Busy = 0;
            public ERROR_CODE ErrorCode = ERROR_CODE.NO_ERROR;
            public string ErrorMessage = string.Empty;
            public INTERNAL_STATUS InternalStatus = INTERNAL_STATUS.INVALID;
        }
        //End : Group 1 : Status
        //Group 2 : Flags & Counters
        //class for flags
        private sealed class DeviceRuntimeFlags
        {
            public bool NeedRefresh { get; set; }
            public bool NeedSimChange { get; set; }
            public bool LowCredit { get; set; }
            public bool AutoRefreshEnabled { get; set; }
            public bool AutoSmsSendEnabled { get; set; }
            public bool AutoInboxEnabled { get; set; }
        }
        //End : class for flags -> Group 1 : Status
        //class for counters -> continues failures
        private sealed class DeviceRuntimeFailureCounters
        {
            public int Select { get; set; }
            public int Refresh { get; set; }
            public int SmsSend { get; set; }
        }
        //End : class for counters -> continues failures
        //class for Device information
        private sealed class DeviceRuntimeInfo
        {
            public DateTime LastRefreshTime { get; set; }
            public DateTime LastSmsSendTime { get; set; }
            public DateTime LastInboxReadTime { get; set; }
            public DateTime CreateDateTime { get; set; }
            public DateTime LastRecoveryTime { get; set; }
        }
        //End : class for Device information
        //End : Group 2 : Flags & Counters

        //Error Manager
        private void ErrorReset()
        {
            this._state.ErrorCode = ERROR_CODE.NO_ERROR;
            this._state.ErrorMessage = string.Empty;
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
            this._state.ErrorCode = ERROR_CODE.DLL_WRAPPER;
            this._state.ErrorMessage = error;
        }
        private bool ErrorSet(ERROR_CODE ErrorCode, string ErrorMessage)
        {
            this._state.ErrorCode = ErrorCode;
            this._state.ErrorMessage = ErrorMessage;
            return false;
        }
        private bool BaseErrorCheck()
        {
            if (this._state.ErrorCode == ERROR_CODE.DLL_WRAPPER || this._state.ErrorCode == ERROR_CODE.INTERNAL_STATUS || this._state.ErrorCode == ERROR_CODE.INVALID_INPUT)
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
            return Interlocked.CompareExchange(ref this._state.Busy, 1, 0) == 1;
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
            Interlocked.Exchange(ref this._state.Busy, 0);
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
    }
}
