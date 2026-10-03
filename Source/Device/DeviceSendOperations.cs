using GSM.Core.Source.Common;
using GSM.Core.Source.StringAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static GSM.Core.Source.Common.DeviceDefinitions;
using static GSM.Core.Source.StringAnalyzer.StringAnalyzer;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        //SMS Send
        public bool SmsSend(string PhoneNumber, string SmsText, ref List<string> SMSReference)
        {
            bool result = false;
            if (this.TrySetBusy())
            {
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.SmsSend));
                return false;
            }
            if (!this.InternalStatusCheck(INTERNAL_STATUS.READY, nameof(this.SmsSend)))
            {
                return false;
            }
            try
            {
                DeviceDefinitions.SmsSendData smsSendData = null;
                result = this.SmsSendDataInitial(ref smsSendData, PhoneNumber, SmsText);
                if(!result)
                {
                    this.ErrorSet(ERROR_CODE.SEND_SMS_INPUT, nameof(this.SmsSend) + " - Phone Number = " + PhoneNumber + " - SMS Text = " + SmsText);
                }
                if (result)
                {
                    result = this.SendConfigurationSetWorkflow(SMS_CHAR_MODE.UCS2 , SMS_TYPE.Persian);
                }
                if (result)
                {
                    result = this.SMSSendWorkflow(smsSendData, ref SMSReference);
                }
                return result;
            }
            finally
            {
                this.DeviceBusyDeactive(nameof(this.SmsSend), result);
            }
        }
        private bool SMSSendWorkflow(DeviceDefinitions.SmsSendData smsSendData, ref List<string> SMSReference)
        {
            bool result = true;
            if (result)
            {
                result = this.SMSSendExecute(smsSendData, DELAY_NORMAL1_MS, ref SMSReference);
            }
            return result;
        }
        private bool SMSSendExecute(DeviceDefinitions.SmsSendData smsSendData, int Delay, ref List<string> SMSReference)
        {
            Thread.Sleep(Delay);
            return this.SMSSendBase(smsSendData , ref SMSReference);
        }
        private bool SMSSendBase(DeviceDefinitions.SmsSendData smsSendData, ref List<string> SMSReference)
        {
            bool result = true;
            string smsSendResultRaw = string.Empty;
            List<string> smsReferenceList = new List<string>();
            this.ErrorReset();
            if (result)
            {
                result = this.SMSSendBaseDll(smsSendData, ref smsSendResultRaw);
            }
            if (result)
            {
                result = this.SMSSendBaseCheck(smsSendResultRaw, ref smsReferenceList);
            }
            this.SMSSendBaseSetValue(result, smsReferenceList, ref SMSReference);
            return result;
        }
        private void SMSSendBaseSetValue(bool result, List<string> smsReferenceList, ref List<string> SMSReference)
        {
            if (result)
            {
                SMSReference = smsReferenceList;
            }
            else
            {
                SMSReference = null;
            }
        }
        private bool SMSSendBaseDll(DeviceDefinitions.SmsSendData smsSendData, ref string ResultStr)
        {
            try
            {
                this._dllWrapper.SmsSend(out ResultStr, smsSendData.PhoneNumber, smsSendData.SmsText, smsSendData.DeliveryReport, smsSendData.SendInterval,
                                         smsSendData.RetryTimes, (int)smsSendData.SmsValidity, smsSendData.SmsReference, smsSendData.SmsCenter);
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"SMSSendBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool SMSSendBaseCheck(string SmsSendResultRaw, ref List<string> SMSReference)
        {
            var result = StringAnalyzer.StringAnalyzer.ParseSmsSendResult(SmsSendResultRaw);
            if(!result.Result && result.ErrorCode != StringAnalyzer.StringAnalyzer.ERROR_CODE.NO_ERROR)
            {
                this.ErrorSet(ERROR_CODE.SEND_SMS_RESULT , result.ErrorMessage);
            }
            else
            {
                SMSReference = result.Data;
            }
            return result.Result;
        }
        //End : SMS Send

        //AT Command Send
        private bool ATCommandSendWorkflow(string ATCommand, ref string ATCommandResult)
        {
            bool result = true;
            if (result)
            {
                result = this.ATCommandSendExecute(ATCommand, DELAY_NORMAL1_MS, ref ATCommandResult);
            }
            return result;
        }
        private bool ATCommandSendExecute(string ATCommand, int Delay, ref string ATCommandResult)
        {
            Thread.Sleep(Delay);
            return this.ATCommandSendBase(ATCommand, ref ATCommandResult);
        }
        private bool ATCommandSendBase(string ATCommand, ref string ATCommandResult)
        {
            bool result = true;
            int resultLen = 0;
            string atCommandSendResultRaw = string.Empty;
            this.ErrorReset();
            if (result)
            {
                result = this.ATCommandSendBaseDll(ATCommand, ref atCommandSendResultRaw, ref resultLen);
            }
            if (result)
            {
                result = this.ATCommandSendBaseCheck(atCommandSendResultRaw, resultLen);
            }
            this.ATCommandSendBaseSetValue(result, atCommandSendResultRaw, ref ATCommandResult);
            return result;
        }
        private bool ATCommandSendBaseDll(string ATCommand, ref string ATCommandResult , ref int ResultLen)
        {
            try
            {
                this._dllWrapper.ATCommandSend(ATCommand, ref ResultLen, out ATCommandResult);
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"ATCommandtBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool ATCommandSendBaseCheck(string ATCommandResult, int ResultLen)
        {
            if (string.IsNullOrWhiteSpace(ATCommandResult) || ResultLen <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void ATCommandSendBaseSetValue(bool result, string atCommandSendResultRaw, ref string ATCommandResult)
        {
            if(result)
            {
                ATCommandResult = atCommandSendResultRaw;
            }
            else
            {
                ATCommandResult = null;
            }
        }
        //End : AT Command Send


        //Helper Functions
        private bool SmsSendDataInitial(ref DeviceDefinitions.SmsSendData smsSendData, string PhoneNumber, string SmsText)
        {
            bool result = true;
            string phoneNumber = string.Empty;
            this.SmsSendDataDefault(ref smsSendData);
            if (result)
            {
                result = this.SmsSendNormalizePhoneNumber(PhoneNumber, this._smsSendCountryCode, ref phoneNumber);
            }
            if (result)
            {
                result = this.SmsSendSmsTextCheck(SmsText);
            }
            if (result)
            {
                this.SmsSendSetValue(ref smsSendData, phoneNumber, SmsText);
            }
            return result;
        }
        private void SmsSendDataDefault(ref DeviceDefinitions.SmsSendData smsSendData)
        {
            smsSendData = new DeviceDefinitions.SmsSendData();
            smsSendData.PhoneNumber = DeviceDefinitions.STRING_INVALID;
            smsSendData.SmsText = DeviceDefinitions.STRING_INVALID;
            smsSendData.DeliveryReport = this.SmsSendDeliveryReport;
            smsSendData.SendInterval = this.SmsSendSendInterval;
            smsSendData.RetryTimes = this.SmsSendRetryTimes;
            smsSendData.SmsValidity = this.SmsSendSmsValidity;
            smsSendData.SmsReference = this._smsSendSmsReference;
            smsSendData.SmsCenter = this.SMSCenter;
        }
        private bool SmsSendNormalizePhoneNumber(string PhoneNumber, string countryCodeStr, ref string resultStr)
        {
            resultStr = null;
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                return false;
            }
            PhoneNumber = PhoneNumber.Trim();
            switch (PhoneNumber.Length)
            {
                // 9121234567 -> +989121234567
                case 10:
                    if (PhoneNumber.All(char.IsDigit) && PhoneNumber[0] != '0')
                    {
                        resultStr = countryCodeStr + PhoneNumber;
                        return true;
                    }
                    break;
                // 09121234567 -> +989121234567
                case 11:
                    if (PhoneNumber.All(char.IsDigit) && PhoneNumber[0] == '0')
                    {
                        resultStr = countryCodeStr + PhoneNumber.Substring(1);
                        return true;
                    }
                    break;
                // 989121234567 -> +989121234567
                case 12:
                    if (PhoneNumber.All(char.IsDigit))
                    {
                        resultStr = "+" + PhoneNumber;
                        return true;
                    }
                    break;
                // +989121234567 -> +989121234567
                case 13:
                    if (PhoneNumber[0] == '+' && PhoneNumber.Skip(1).All(char.IsDigit))
                    {
                        resultStr = PhoneNumber;
                        return true;
                    }
                    break;
                // 00989121234567 -> +989121234567
                case 14:
                    if (PhoneNumber.StartsWith("00") && PhoneNumber.Skip(2).All(char.IsDigit))
                    {
                        resultStr = "+" + PhoneNumber.Substring(2);
                        return true;
                    }
                    break;
            }
            return false;
        }
        private bool SmsSendSmsTextCheck(string SmsText)
        {
            if (!string.IsNullOrEmpty(SmsText))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void SmsSendSetValue(ref DeviceDefinitions.SmsSendData smsSendData, string PhoneNumber, string SmsText)
        {
            smsSendData.PhoneNumber = PhoneNumber;
            smsSendData.SmsText = SmsText;
        }
        //End : Helper Functions
    }
}
