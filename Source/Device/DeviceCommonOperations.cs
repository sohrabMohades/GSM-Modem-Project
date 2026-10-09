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
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            if (result == FUNC_RESULT.SUCCESS && CharMode != this._runtimeWorkingConfig.SmsCharMode)
            {
                result = this.CharModeSetWorkflow(CharMode, true);
            }
            if (result == FUNC_RESULT.SUCCESS && SmsType != this._runtimeWorkingConfig.SmsType)
            {
                result = this.SmsTypeSetWorkflow(SmsType, true);
            }
            if (result == FUNC_RESULT.SUCCESS && ATCommandDelay != this._runtimeWorkingConfig.AtCommandDelay)
            {
                result = this.ATCommandDelaySetWorkflow(ATCommandDelay, true);
            }
            return result;
        }
        private FUNC_RESULT SendConfigurationSetComplexWorkflow(DeviceDefinitions.SMS_CHAR_MODE CharMode, DeviceDefinitions.SMS_TYPE SmsType)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            if (result == FUNC_RESULT.SUCCESS && CharMode != this._runtimeWorkingConfig.SmsCharMode)
            {
                result = this.CharModeSetWorkflow(CharMode, true);
            }
            if (result == FUNC_RESULT.SUCCESS && SmsType != this._runtimeWorkingConfig.SmsType)
            {
                result = this.SmsTypeSetWorkflow(SmsType, true);
            }
            return result;
        }
        //End : Send Configuration Set

        //Char Mode Set
        private FUNC_RESULT CharModeSetWorkflow(DeviceDefinitions.SMS_CHAR_MODE CharMode, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            DeviceDefinitions.SMS_CHAR_MODE charMode = DeviceDefinitions.SMS_CHAR_MODE.UNKHOWN;
            result = this.CharModeSetExecute(CharMode, (int)OPERATION_DELAY_MS.NORMAL_SHORT);
            this.XXXSetValue(result, CharMode, DeviceDefinitions.SMS_CHAR_MODE.UNKHOWN, ref charMode, ref this._runtimeWorkingConfig.SmsCharMode, InternalValueSet);
            return result;
        }
        private FUNC_RESULT CharModeSetExecute(DeviceDefinitions.SMS_CHAR_MODE CharMode, int Delay)
        {
            Thread.Sleep(Delay);
            return this.CharModeSetBase(CharMode);
        }
        private FUNC_RESULT CharModeSetBase(DeviceDefinitions.SMS_CHAR_MODE CharMode)
        {
            FUNC_RESULT result = this.CharModeSetBaseInputCheck(CharMode);
            if (result == FUNC_RESULT.SUCCESS)
                result = this.CharModeSetBaseDll(CharMode);
            return result;
        }
        private FUNC_RESULT CharModeSetBaseInputCheck(DeviceDefinitions.SMS_CHAR_MODE CharMode)
        {
            if (CharMode == DeviceDefinitions.SMS_CHAR_MODE.UNKHOWN)
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "CharMode = " + CharMode.ToString());
                return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT CharModeSetBaseDll(DeviceDefinitions.SMS_CHAR_MODE CharMode)
        {
            this._dllWrapper.SmsCharModeSet(CharMode);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : Char Mode Set

        //Sms Type Set
        private FUNC_RESULT SmsTypeSetWorkflow(DeviceDefinitions.SMS_TYPE SmsType, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            DeviceDefinitions.SMS_TYPE smsType = DeviceDefinitions.SMS_TYPE.UNKHOWN;
            result = this.SmsTypeSetExecute(SmsType, (int)OPERATION_DELAY_MS.NORMAL_SHORT);
            this.XXXSetValue(result, SmsType, DeviceDefinitions.SMS_TYPE.UNKHOWN, ref smsType, ref this._runtimeWorkingConfig.SmsType, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SmsTypeSetExecute(DeviceDefinitions.SMS_TYPE SmsType, int Delay)
        {
            Thread.Sleep(Delay);
            return this.SmsTypeSetBase(SmsType);
        }
        private FUNC_RESULT SmsTypeSetBase(DeviceDefinitions.SMS_TYPE SmsType)
        {
            FUNC_RESULT result = this.SmsTypeSetBaseInputCheck(SmsType);
            if (result == FUNC_RESULT.SUCCESS)
                result = this.SmsTypeSetBaseDll(SmsType);
            return result;
        }
        private FUNC_RESULT SmsTypeSetBaseInputCheck(DeviceDefinitions.SMS_TYPE SmsType)
        {
            if (SmsType == DeviceDefinitions.SMS_TYPE.UNKHOWN)
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "SmsType = " + SmsType.ToString());
                return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT SmsTypeSetBaseDll(DeviceDefinitions.SMS_TYPE SmsType)
        {
            this._dllWrapper.SmsPDUModeSet(SmsType);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : Sms Type Set

        //AT Command Delay Set
        private FUNC_RESULT ATCommandDelaySetWorkflow(int ATCommandDelay, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            int atCommand = DeviceDefinitions.NUMBER_INVALID;
            result = this.ATCommandDelaySetExecute(ATCommandDelay, (int)OPERATION_DELAY_MS.NORMAL_SHORT);
            this.XXXSetValue(result, ATCommandDelay, DeviceDefinitions.NUMBER_INVALID, ref atCommand, ref this._runtimeWorkingConfig.AtCommandDelay, InternalValueSet);
            return result;
        }
        private FUNC_RESULT ATCommandDelaySetExecute(int ATCommandDelay, int Delay)
        {
            Thread.Sleep(Delay);
            return this.ATCommandDelaySetBase(ATCommandDelay);
        }
        private FUNC_RESULT ATCommandDelaySetBase(int ATCommandDelay)
        {
            return this.ATCommandDelaySetBaseDll(ATCommandDelay);
        }
        private FUNC_RESULT ATCommandDelaySetBaseDll(int ATCommandDelay)
        {
            this._dllWrapper.ATCommandDelaySet(ATCommandDelay);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : AT Command Delay Set

        //Buffer
        //End : Buffer
    }
}
