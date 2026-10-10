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
        private FUNC_RESULT SMSSendWorkflow(DeviceDefinitions.SmsSendData smsSendData, ref List<string> SMSReference)
        {
            return this.SMSSendExecute(smsSendData, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref SMSReference);
        }
        private FUNC_RESULT SMSSendExecute(DeviceDefinitions.SmsSendData smsSendData, int Delay, ref List<string> SMSReference)
        {
            Thread.Sleep(Delay);
            return this.SMSSendBase(smsSendData , ref SMSReference);
        }
        private FUNC_RESULT SMSSendBase(DeviceDefinitions.SmsSendData smsSendData, ref List<string> SMSReference)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            string smsSendResultRaw = string.Empty;
            List<string> smsReferenceList = new List<string>();
            result = this.SMSSendBaseDll(smsSendData, ref smsSendResultRaw);
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.SMSSendBaseCheck(smsSendResultRaw, ref smsReferenceList);
            }
            this.XXXBaseSetValue(result, smsReferenceList, null, ref SMSReference);
            return result;
        }
        private FUNC_RESULT SMSSendBaseDll(DeviceDefinitions.SmsSendData smsSendData, ref string ResultStr)
        {
            this._dllWrapper.SmsSend(out ResultStr, smsSendData.PhoneNumber, smsSendData.SmsText, smsSendData.DeliveryReport, smsSendData.SendInterval,
                                     smsSendData.RetryTimes, (int)smsSendData.SmsValidity, smsSendData.SmsReference, smsSendData.SmsCenter);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        private FUNC_RESULT SMSSendBaseCheck(string SmsSendResultRaw, ref List<string> SMSReference)
        {
            var result = StringAnalyzer.StringAnalyzer.ParseSmsSendResult(SmsSendResultRaw);
            if (result.Result)
            {
                SMSReference = result.Data; 
                return FUNC_RESULT.SUCCESS;
            }
            else
                if (result.ErrorCode != StringAnalyzer.StringAnalyzer.ERROR_CODE.NO_ERROR)
                {
                    this.ErrorSet(ERROR_CODE.SEND_SMS_RESULT, result.ErrorMessage);
                    return FUNC_RESULT.MAJOR_ERROR;
                }
                else
                    return FUNC_RESULT.FAIL;
        }
        //End : SMS Send

        //AT Command Send
        private FUNC_RESULT ATCommandSendWorkflow(string ATCommand, ref string ATCommandResult)
        {
            return this.ATCommandSendExecute(ATCommand, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref ATCommandResult);
        }
        private FUNC_RESULT ATCommandSendExecute(string ATCommand, int Delay, ref string ATCommandResult)
        {
            Thread.Sleep(Delay);
            return this.ATCommandSendBase(ATCommand, ref ATCommandResult);
        }
        private FUNC_RESULT ATCommandSendBase(string ATCommand, ref string ATCommandResult)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            int resultLen = 0;
            string atCommandSendResultRaw = string.Empty;
            result = this.ATCommandSendBaseDll(ATCommand, ref atCommandSendResultRaw, ref resultLen);
            if (result == FUNC_RESULT.SUCCESS)
                result = this.ATCommandSendBaseCheck(atCommandSendResultRaw, resultLen);
            this.XXXBaseSetValue(result, atCommandSendResultRaw, null, ref ATCommandResult);
            return result;
        }
        private FUNC_RESULT ATCommandSendBaseDll(string ATCommand, ref string ATCommandResult, ref int ResultLen)
        {
            this._dllWrapper.ATCommandSend(ATCommand, ref ResultLen, out ATCommandResult);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        private FUNC_RESULT ATCommandSendBaseCheck(string ATCommandResult, int ResultLen)
        {
            if (string.IsNullOrWhiteSpace(ATCommandResult) || ResultLen <= 0)
                return FUNC_RESULT.FAIL;
            else
                return FUNC_RESULT.SUCCESS;
        }
        //End : AT Command Send


        //Helper Functions
        private FUNC_RESULT SmsSendDataInitial(ref DeviceDefinitions.SmsSendData smsSendData, string PhoneNumber, string SmsText)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            string phoneNumber = string.Empty;
            this.SmsSendDataDefault(ref smsSendData);
            result = this.SmsSendNormalizePhoneNumber(PhoneNumber, this._runtimeSmsSendConfig.CountryCode, ref phoneNumber);
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.SmsSendSmsTextCheck(SmsText);
            }
            if (result == FUNC_RESULT.SUCCESS)
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
            smsSendData.DeliveryReport = this._runtimeSmsSendConfig.DeliveryReport;
            smsSendData.SendInterval = this._runtimeSmsSendConfig.SendInterval;
            smsSendData.RetryTimes = this._runtimeSmsSendConfig.RetryTimes;
            smsSendData.SmsValidity = this._runtimeSmsSendConfig.SmsValidity;
            smsSendData.SmsReference = this._runtimeSmsSendConfig.SmsReference;
            smsSendData.SmsCenter = this._runtimeNetwork.SMSCenter;
        }
        private FUNC_RESULT SmsSendNormalizePhoneNumber(string PhoneNumber, string countryCodeStr, ref string resultStr)
        {
            resultStr = null;
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                this.ErrorSet(ERROR_CODE.SEND_SMS_INPUT, nameof(this.SmsSend) + " - Phone Number = " + PhoneNumber + " . INVALID");
                return FUNC_RESULT.MAJOR_ERROR;
            }
            PhoneNumber = PhoneNumber.Trim();
            switch (PhoneNumber.Length)
            {
                // 9121234567 -> +989121234567
                case 10:
                    if (PhoneNumber.All(char.IsDigit) && PhoneNumber[0] != '0')
                    {
                        resultStr = countryCodeStr + PhoneNumber;
                        return FUNC_RESULT.SUCCESS;
                    }
                    break;
                // 09121234567 -> +989121234567
                case 11:
                    if (PhoneNumber.All(char.IsDigit) && PhoneNumber[0] == '0')
                    {
                        resultStr = countryCodeStr + PhoneNumber.Substring(1);
                        return FUNC_RESULT.SUCCESS;
                    }
                    break;
                // 989121234567 -> +989121234567
                case 12:
                    if (PhoneNumber.All(char.IsDigit))
                    {
                        resultStr = "+" + PhoneNumber;
                        return FUNC_RESULT.SUCCESS;
                    }
                    break;
                // +989121234567 -> +989121234567
                case 13:
                    if (PhoneNumber[0] == '+' && PhoneNumber.Skip(1).All(char.IsDigit))
                    {
                        resultStr = PhoneNumber;
                        return FUNC_RESULT.SUCCESS;
                    }
                    break;
                // 00989121234567 -> +989121234567
                case 14:
                    if (PhoneNumber.StartsWith("00") && PhoneNumber.Skip(2).All(char.IsDigit))
                    {
                        resultStr = "+" + PhoneNumber.Substring(2);
                        return FUNC_RESULT.SUCCESS;
                    }
                    break;
            }
            this.ErrorSet(ERROR_CODE.SEND_SMS_INPUT, nameof(this.SmsSend) + " - Phone Number = " + PhoneNumber + " . INVALID");
            return FUNC_RESULT.MAJOR_ERROR;
        }
        private FUNC_RESULT SmsSendSmsTextCheck(string SmsText)
        {
            if (!string.IsNullOrEmpty(SmsText))
                return FUNC_RESULT.SUCCESS;
            else
            {
                this.ErrorSet(ERROR_CODE.SEND_SMS_INPUT, nameof(this.SmsSend) + " - SMS Text = " + SmsText + " . INVALID");
                return FUNC_RESULT.MAJOR_ERROR;
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
