using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GSM.Core.Source.Common.DeviceDefinitions;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        //Send Configuration Set
        private FUNC_RESULT SendConfigurationSetComplexWorkflow(DeviceDefinitions.SMS_CHAR_MODE CharMode, DeviceDefinitions.SMS_TYPE SmsType, int ATCommandDelay)
        {
            bool internalValueSet = true;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            if (result == FUNC_RESULT.SUCCESS && CharMode != this._runtimeWorkingConfig.SmsCharMode) result = this.CharModeSetWorkflow(CharMode, internalValueSet);
            if (result == FUNC_RESULT.SUCCESS && SmsType != this._runtimeWorkingConfig.SmsType) result = this.SmsTypeSetWorkflow(SmsType, internalValueSet);
            if (result == FUNC_RESULT.SUCCESS && ATCommandDelay != this._runtimeWorkingConfig.AtCommandDelay) result = this.ATCommandDelaySetWorkflow(ATCommandDelay, internalValueSet);
            return result;
        }
        private FUNC_RESULT SendConfigurationSetComplexWorkflow(DeviceDefinitions.SMS_CHAR_MODE CharMode, DeviceDefinitions.SMS_TYPE SmsType)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            if (result == FUNC_RESULT.SUCCESS && CharMode != this._runtimeWorkingConfig.SmsCharMode) result = this.CharModeSetWorkflow(CharMode, true);
            if (result == FUNC_RESULT.SUCCESS && SmsType != this._runtimeWorkingConfig.SmsType) result = this.SmsTypeSetWorkflow(SmsType, true);
            return result;
        }
        //End : Send Configuration Set

        //Char Mode Set
        private FUNC_RESULT CharModeSetWorkflow(DeviceDefinitions.SMS_CHAR_MODE CharMode, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.CharModeSetExecute(result, CharMode, (int)OPERATION_DELAY_MS.NORMAL_SHORT);
            this.XXXSetValue(result, CharMode, DeviceDefinitions.SMS_CHAR_MODE.UNKHOWN, ref CharMode, ref this._runtimeWorkingConfig.SmsCharMode, InternalValueSet);
            return result;
        }
        private FUNC_RESULT CharModeSetExecute(FUNC_RESULT Result, DeviceDefinitions.SMS_CHAR_MODE CharMode, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.CharModeSetBase(CharMode);
        }
        private FUNC_RESULT CharModeSetBase(DeviceDefinitions.SMS_CHAR_MODE CharMode)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.CharModeSetBaseInputCheck(result, CharMode);
            result = this.CharModeSetBaseDll(result, CharMode);
            return result;
        }
        private FUNC_RESULT CharModeSetBaseInputCheck(FUNC_RESULT Result, DeviceDefinitions.SMS_CHAR_MODE CharMode)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (CharMode == DeviceDefinitions.SMS_CHAR_MODE.UNKHOWN)
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "CharMode = " + CharMode.ToString()); return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT CharModeSetBaseDll(FUNC_RESULT Result, DeviceDefinitions.SMS_CHAR_MODE CharMode)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            this._dllWrapper.SmsCharModeSet(CharMode);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : Char Mode Set

        //Sms Type Set
        private FUNC_RESULT SmsTypeSetWorkflow(DeviceDefinitions.SMS_TYPE SmsType, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SmsTypeSetExecute(result, SmsType, (int)OPERATION_DELAY_MS.NORMAL_SHORT);
            this.XXXSetValue(result, SmsType, DeviceDefinitions.SMS_TYPE.UNKHOWN, ref SmsType, ref this._runtimeWorkingConfig.SmsType, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SmsTypeSetExecute(FUNC_RESULT Result, DeviceDefinitions.SMS_TYPE SmsType, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.SmsTypeSetBase(SmsType);
        }
        private FUNC_RESULT SmsTypeSetBase(DeviceDefinitions.SMS_TYPE SmsType)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SmsTypeSetBaseInputCheck(result, SmsType);
            result = this.SmsTypeSetBaseDll(result, SmsType);
            return result;
        }
        private FUNC_RESULT SmsTypeSetBaseInputCheck(FUNC_RESULT Result, DeviceDefinitions.SMS_TYPE SmsType)
        {
            if (Result != FUNC_RESULT.SUCCESS)  return Result;
            if (SmsType == DeviceDefinitions.SMS_TYPE.UNKHOWN)
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "SmsType = " + SmsType.ToString()); return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT SmsTypeSetBaseDll(FUNC_RESULT Result, DeviceDefinitions.SMS_TYPE SmsType)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            this._dllWrapper.SmsPDUModeSet(SmsType);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : Sms Type Set

        //AT Command Delay Set
        private FUNC_RESULT ATCommandDelaySetWorkflow(int ATCommandDelay, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.ATCommandDelaySetExecute(result, ATCommandDelay, (int)OPERATION_DELAY_MS.NORMAL_SHORT);
            this.XXXSetValue(result, ATCommandDelay, DeviceDefinitions.NUMBER_INVALID, ref ATCommandDelay, ref this._runtimeWorkingConfig.AtCommandDelay, InternalValueSet);
            return result;
        }
        private FUNC_RESULT ATCommandDelaySetExecute(FUNC_RESULT Result, int ATCommandDelay, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.ATCommandDelaySetBase(ATCommandDelay);
        }
        private FUNC_RESULT ATCommandDelaySetBase(int ATCommandDelay)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.ATCommandDelaySetBaseCheck(result, ATCommandDelay);
            result = this.ATCommandDelaySetBaseDll(result, ATCommandDelay);
            return result;
        }
        private FUNC_RESULT ATCommandDelaySetBaseCheck(FUNC_RESULT Result, int ATCommandDelay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (ATCommandDelay < DeviceDefinitions.AT_COMMAND_DELAY_MIN || ATCommandDelay > DeviceDefinitions.AT_COMMAND_DELAY_MAX) 
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, $"ATCommandDelay = {ATCommandDelay}"); return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT ATCommandDelaySetBaseDll(FUNC_RESULT Result, int ATCommandDelay)
        {
            if (Result != FUNC_RESULT.SUCCESS)
                return Result;
            this._dllWrapper.ATCommandDelaySet(ATCommandDelay);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : AT Command Delay Set

        //Buffer
        //End : Buffer
    }
}
