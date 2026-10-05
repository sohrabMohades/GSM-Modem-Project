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
        //Send Configuration Set
        private bool SendConfigurationSetWorkflow(DeviceDefinitions.SMS_CHAR_MODE CharMode, DeviceDefinitions.SMS_TYPE SmsType, int ATCommandDelay)
        {
            bool result = true;
            if (result && CharMode != this._smsCharMode)
            {
                result = this.CharModeSetWorkflow(CharMode, true);
            }
            if (result && SmsType != this._smsType)
            {
                result = this.SmsTypeSetWorkflow(SmsType, true);
            }
            if (result && ATCommandDelay != this._atCommandDelay)
            {
                result = this.ATCommandDelaySetWorkflow(ATCommandDelay, true);
            }
            return result;
        }
        private bool SendConfigurationSetWorkflow(DeviceDefinitions.SMS_CHAR_MODE CharMode, DeviceDefinitions.SMS_TYPE SmsType)
        {
            bool result = true;
            if (result && CharMode != this._smsCharMode)
            {
                result = this.CharModeSetWorkflow(CharMode, true);
            }
            if (result && SmsType != this._smsType)
            {
                result = this.SmsTypeSetWorkflow(SmsType, true);
            }
            return result;
        }
        //End : Send Configuration Set

        //Char Mode Set
        private bool CharModeSetWorkflow(DeviceDefinitions.SMS_CHAR_MODE CharMode, bool InternalValueSet)
        {
            bool result = true;
            if (result)
            {
                result = this.CharModeSetExecute(CharMode, DELAY_NORMAL1_MS);
            }
            if (result)
            {
                this.CharModeSetSetValue(CharMode, InternalValueSet);
            }
            else
            {
                this.CharModeSetSetValue(DeviceDefinitions.SMS_CHAR_MODE.GSM, InternalValueSet);
            }
            return result;
        }
        private bool CharModeSetExecute(DeviceDefinitions.SMS_CHAR_MODE CharMode, int Delay)
        {
            Thread.Sleep(Delay);
            return this.CharModeSetBase(CharMode);
        }
        private void CharModeSetSetValue(DeviceDefinitions.SMS_CHAR_MODE CharMode, bool InternalValueSet)
        {
            if (InternalValueSet)
            {
                this._smsCharMode = CharMode;
            }
        }
        private bool CharModeSetBase(DeviceDefinitions.SMS_CHAR_MODE CharMode)
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.CharModeSetBaseInputCheck(CharMode);
            }
            if (result)
            {
                result = this.CharModeSetBaseDll(CharMode);
            }
            return result;
        }
        private bool CharModeSetBaseInputCheck(DeviceDefinitions.SMS_CHAR_MODE CharMode)
        {
            if (CharMode == DeviceDefinitions.SMS_CHAR_MODE.UNKHOWN)
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "CharMode = " + CharMode.ToString());
                return false;
            }
            return true;
        }
        private bool CharModeSetBaseDll(DeviceDefinitions.SMS_CHAR_MODE CharMode)
        {
            try
            {
                this._dllWrapper.SmsCharModeSet(CharMode);
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"CharModeSetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        //End : Char Mode Set

        //Sms Type Set
        private bool SmsTypeSetWorkflow(DeviceDefinitions.SMS_TYPE SmsType, bool InternalValueSet)
        {
            bool result = true;
            if (result)
            {
                result = this.SmsTypeSetExecute(SmsType, DELAY_NORMAL1_MS);
            }
            if (result)
            {
                this.SmsTypeSetSetValue(SmsType, InternalValueSet);
            }
            else
            {
                this.SmsTypeSetSetValue(DeviceDefinitions.SMS_TYPE.Persian, InternalValueSet);
            }
            return result;
        }
        private bool SmsTypeSetExecute(DeviceDefinitions.SMS_TYPE SmsType, int Delay)
        {
            Thread.Sleep(Delay);
            return this.SmsTypeSetBase(SmsType);
        }
        private void SmsTypeSetSetValue(DeviceDefinitions.SMS_TYPE SmsType, bool InternalValueSet)
        {
            if (InternalValueSet)
            {
                this._smsType = SmsType;
            }
        }
        private bool SmsTypeSetBase(DeviceDefinitions.SMS_TYPE SmsType)
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.SmsTypeSetBaseInputCheck(SmsType);
            }
            if (result)
            {
                result = this.SmsTypeSetBaseDll(SmsType);
            }
            return result;
        }
        private bool SmsTypeSetBaseInputCheck(DeviceDefinitions.SMS_TYPE SmsType)
        {
            if (SmsType == DeviceDefinitions.SMS_TYPE.UNKHOWN)
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "SmsType = " + SmsType.ToString());
                return false;
            }
            return true;
        }
        private bool SmsTypeSetBaseDll(DeviceDefinitions.SMS_TYPE SmsType)
        {
            try
            {
                this._dllWrapper.SmsPDUModeSet(SmsType);
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"SmsTypeSetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        //End : Sms Type Set

        //AT Command Delay Set
        private bool ATCommandDelaySetWorkflow(int ATCommandDelay, bool InternalValueSet)
        {
            bool result = true;
            if (result)
            {
                result = this.ATCommandDelaySetExecute(ATCommandDelay, DELAY_NORMAL1_MS);
            }
            if (result)
            {
                this.ATCommandDelaySetSetValue(ATCommandDelay, InternalValueSet);
            }
            else
            {
                this.ATCommandDelaySetSetValue(DeviceDefinitions.AT_COMMAND_DELAY_INVALID, InternalValueSet);
            }
            return result;
        }
        private bool ATCommandDelaySetExecute(int ATCommandDelay, int Delay)
        {
            Thread.Sleep(Delay);
            return this.ATCommandDelaySetBase(ATCommandDelay);
        }
        private void ATCommandDelaySetSetValue(int ATCommandDelay, bool InternalValueSet)
        {
            if (InternalValueSet)
            {
                this._atCommandDelay = ATCommandDelay;
            }
        }
        private bool ATCommandDelaySetBase(int ATCommandDelay)
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.ATCommandDelaySetBaseDll(ATCommandDelay);
            }
            return result;
        }
        private bool ATCommandDelaySetBaseDll(int ATCommandDelay)
        {
            try
            {
                this._dllWrapper.ATCommandDelaySet(ATCommandDelay);
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"ATCommandDelaySetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        //End : AT Command Delay Set

        //Buffer
        //End : Buffer
    }
}
