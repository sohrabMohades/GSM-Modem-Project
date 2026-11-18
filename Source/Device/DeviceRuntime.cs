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
        private void ErrorReset() { this._state.ErrorCode = ERROR_CODE.NO_ERROR; this._state.ErrorMessage = string.Empty; }
        private FUNC_RESULT DllWrapperHasError(FUNC_RESULT funcResult)
        {
            try
            {
                if (this._dllWrapper.ErrorCode == DllWrapper.ERROR_CODE.NO_ERROR) return funcResult;
                else { this.DllWrapperErrorCheckSetValue(this._dllWrapper.ErrorMessage); return FUNC_RESULT.MAJOR_ERROR; }
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue(ex.Message); return FUNC_RESULT.MAJOR_ERROR;
            }
        }
        private FUNC_RESULT DllWrapperHasError()
        {
            try
            {
                if (this._dllWrapper.ErrorCode == DllWrapper.ERROR_CODE.NO_ERROR) return FUNC_RESULT.SUCCESS;
                else { this.DllWrapperErrorCheckSetValue(this._dllWrapper.ErrorMessage); return FUNC_RESULT.MAJOR_ERROR; }
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue(ex.Message); return FUNC_RESULT.MAJOR_ERROR;
            }
        }
        private bool DllWrapperErrorCheckSetValue(string error, Exception exception = null)
        {
            this._state.ErrorCode = ERROR_CODE.DLL_WRAPPER;
            this._state.ErrorMessage = error;
            this._logger.LogError(this._state.ErrorCode, this._state.ErrorMessage, exception);
            return false;
        }
        private bool ErrorSet(ERROR_CODE ErrorCode, string ErrorMessage)
        {
            this._state.ErrorCode = ErrorCode;
            this._state.ErrorMessage = ErrorMessage;
            this._logger.LogError(this._state.ErrorCode, this._state.ErrorMessage);
            return false;
        }
        //Error Manager
        //Busy Flag manager
        private bool TrySetBusy() { return Interlocked.CompareExchange(ref this._state.Busy, 1, 0) == 0; }
        private void DeviceBusyDeactive() => Interlocked.Exchange(ref this._state.Busy, 0); 
        //Busy Flag manager
        //Timeout
        private void TimeOutStart() => this._runtimeInfo.TimeOutCounter = DateTime.Now;
        private bool TimeOutEnd()
        { if ((DateTime.Now - this._runtimeInfo.TimeOutCounter).TotalMilliseconds >= this._runtimeConfig.TimeoutMaxMs) return true; else return false; }
        //End : Timeout
        //Internal Status
        private bool InternalStatusCheck(INTERNAL_STATUS MinStatusNeed, string FuncName)
        {
            if (FuncName == nameof(this.DeviceNumberChange)) { if ((int)this._state.InternalStatus == (int)MinStatusNeed) return true; else return false; }
            if ((int)this._state.InternalStatus >= (int)MinStatusNeed) return true; else return false;
        }
        private void InternalStatusUpdate()
        {
        }
        //Internal Status
        /*
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
            this.OnOperationCompleted(operation, FuncResult);
        */
    }
}
