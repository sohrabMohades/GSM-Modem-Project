using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GSM.Core.Source.DllWrapper
{
    public partial class ManshoorGsmDllWrapper : IDisposable
    {
        //Set & Get Property
        public STATUS Status
        {
            get => this._status;
        }
        public ERROR_CODE ErrorCode
        {
            get => this._errorCode;
        }
        public string ErrorMessage
        {
            get => this._errorMessage;
        }
        private void ErrorReset()
        {
            this._errorCode = ERROR_CODE.NO_ERROR;
            this._errorMessage = string.Empty;
        }
        public int DllTimeOutMs
        {
            get => this._dllTimeoutMs;
            set
            {
                if (value > 0)
                {
                    this._dllTimeoutMs = value;
                }
            }
        }
        //End : Set & Get Property

        //Helper Functions
        private bool StringCheck(string str)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(str);
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool StringIsInt64(string str, ref Int64 Number)
        {
            Int64 resultValue;
            Number = DeviceDefinitions.NUMBER_INVALID;

            if (!Int64.TryParse(str, out resultValue))
            {
                return false;
            }

            if (resultValue <= DeviceDefinitions.NUMBER_INVALID)
            {
                return false;
            }

            Number = resultValue;
            return true;
        }
        //End : Helper Functions

        //Public function

        //Common Main function
        public Int16 DeviceCount()
        {
            Int16 result = ExecuteDllFunction(() => this._smsDeviceCount(), nameof(DeviceCount));
            return result;
        }
        /*
         * Note : return Device Number connected to computer
        */
        //End : Common Main function

        //Device connection
        public void SelectByDeviceNumber(Int16 DeviceNumber)
        {
            bool result = ExecuteDllFunction(() => { this._smsSelectDevice(DeviceNumber); return true; }, nameof(SelectByDeviceNumber));
        }
        /*
         * Important Note :
         * 1. if 2 devices have equal Device Number when choose this Device Number UnKnown situation happen
         * 2. for confirmation select must use read Device Number
        */
        public void SelectByDeviceIndex(Int16 DeviceIndex)
        {
            bool result = ExecuteDllFunction(() => { this._smsSelectDeviceByIndex(DeviceIndex); return true; }, nameof(SelectByDeviceIndex));
        }
        /*
         * Important Note :
         * 1. index sorted by device hardware connections and always start from (0) to (total device number - 1)
         * 2. when any device DC it is sorted automatic such as 1. -> 0 , 1 , 2 , 3 , 4 then remove 2 -> new result = 0 , 1 , 2 , 3
         * 3. for confirmation select must use read Device Number
        */
        //End : Device connection

        //Device Hardware setting
        public void DeviceNumberSet(int DeviceNumber)
        {
            bool result = ExecuteDllFunction(() => this._gsmSetDeviceNO(DeviceNumber), nameof(DeviceNumberSet));
        }
        /*
         * Important Note:
         * 1. DeviceNumberSet has bool result but not work and is True every times
         * so i remove this invalid output
        */
        public bool DeviceNumberGet(ref int DeviceNumber)
        {
            int result = ExecuteDllFunction(() => this._gsmGetDeviceNO(), nameof(DeviceNumberGet));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                DeviceNumber = DeviceDefinitions.NUMBER_INVALID;
                return false;
            }

            if (result <= DeviceDefinitions.NUMBER_INVALID)
            {
                DeviceNumber = DeviceDefinitions.NUMBER_INVALID;
                return false;
            }
            else
            {
                DeviceNumber = result;
                return true;
            }
        }
        /*
         * Important Note :
         * 1. if DeviceNumber == -1 means is False read Device Number
        */
        public bool IMEI(out string Imei)
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
                        {
                            string imei;
                            this._rIMEI(out imei);
                            return new DllCallResult<string> { Result = true, Value = imei };
                        }, nameof(IMEI));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                Imei = DeviceDefinitions.STRING_INVALID;
                return false;
            }

            if (this.StringCheck(result.Value))
            {
                Imei = result.Value;
                return true;
            }
            else
            {
                Imei = DeviceDefinitions.STRING_INVALID;
                return false;
            }
        }
        /*
         * Important Note :
         * 1. if IMEI == null means is False to read Device IMEI
        */
        public void GsmPowerSet(bool Power)
        {
            bool result = false;
            if (Power)
            {
                result = ExecuteDllFunction(() => this._gsmON(), nameof(GsmPowerSet));
            }
            else
            {
                result = ExecuteDllFunction(() => this._gsmOFF(), nameof(GsmPowerSet));
            }
        }
        /*
         * Important Notes :
         * 1. when adaptor is DC from modem this functions maybe work or not wor
         * 2. GSM_OFF if modem is ON then this is OFF and if modem is OFF then this is ON
         * so the bool result of GSM_OFF not trustable , you must use GsmPowerGet() for modem power status
         * 3. Important : after change SIM Card i must Power OFF then Power ON -> reset for identify new SIM Card
        */
        public bool GsmPowerGet()
        {
            bool result = ExecuteDllFunction(() => this._rgsmPow(), nameof(GsmPowerGet));
            return result;
        }
        /*
         * note : GSM Power is ON = true or is OFF = False or NO response = False
        */
        public bool GsmIsReady()
        {
            bool result = ExecuteDllFunction(() => this._gsmReady(), nameof(GsmIsReady));
            return result;
        }
        /*
         * note : for extra check
        */
        public bool GsmIsConnect()
        {
            bool result = ExecuteDllFunction(() => this._gsmTestConnect(), nameof(GsmIsConnect));
            return result;
        }
        /*
         * note : for extra check
        */
        public bool DeviceInfo(out string Deviceinfo)
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
            {
                string deviceInfo;
                this._smsDevInfo(out deviceInfo);
                return new DllCallResult<string> { Result = true, Value = deviceInfo };
            }, nameof(DeviceInfo));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                Deviceinfo = DeviceDefinitions.STRING_INVALID;
                return false;
            }

            if (this.StringCheck(result.Value))
            {
                Deviceinfo = result.Value;
                return true;
            }
            else
            {
                Deviceinfo = DeviceDefinitions.STRING_INVALID;
                return false;
            }
        }
        /*
         * Important Note :
         * 1. if DeviceInfo == null means is False to read DeviceInfo
        */
        public bool SoftwareVersion(out string Softeareversion)
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
            {
                string SofteareVersion;
                this._smsSoftVer(out SofteareVersion);
                return new DllCallResult<string> { Result = true, Value = SofteareVersion };
            }, nameof(SoftwareVersion));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                Softeareversion = DeviceDefinitions.STRING_INVALID;
                return false;
            }

            if (this.StringCheck(result.Value))
            {
                Softeareversion = result.Value;
                return true;
            }
            else
            {
                Softeareversion = DeviceDefinitions.STRING_INVALID;
                return false;
            }
        }
        /*
         * Important Note :
         * 1. if SofteareVersion == null means is False to read SofteareVersion
        */
        public bool GsmStatus(ref int Result , out string TextMessage)
        {
            GSM_Message_Struct result = ExecuteDllFunction(() => 
            {
                GSM_Message_Struct gsmMessage;
                this._getGSMMessage(out gsmMessage);
                return gsmMessage; 
            }, nameof(GsmStatus));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                TextMessage = string.Empty;
                Result = DeviceDefinitions.NUMBER_INVALID;
                return false;
            }
            else
            {
                TextMessage = result.Message_Text;
                Result = result.Result;
                return true;
            }
        }
        /*
         * Important : NOT TRUSTABLE OUTPUT -> modem is disconnect but return last status = 0 , modem is connect but return last status = -1
         * Important Note : NO help available for this function
         * 1. result.Result = -1 , result.Message_Text = null -> NOT Ready
         * 2. result.Result = 2 , result.Message_Text = "MODEM:STARTUP"
         * 3. result.Result = 0 , result.Message_Text = "+PBREADY"
        */
        public bool GsmDateTimeGet(out string GsmDateTime)
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
            {
                string gsmDateTime;
                this._rgsmDateTime(out gsmDateTime);
                return new DllCallResult<string> { Result = true, Value = gsmDateTime };
            }, nameof(GsmDateTimeGet));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                GsmDateTime = DeviceDefinitions.STRING_INVALID;
                return false;
            }
            else
            {
                if (this.StringCheck(result.Value))
                {
                    GsmDateTime = result.Value;
                    return true;
                }
                else
                {
                    GsmDateTime = DeviceDefinitions.STRING_INVALID;
                    return false;
                }
            }
        }
        /*
         * Note : get GSM DateTime , what is this use ?
         * 1. if GsmDateTime == null means is False to read GsmDateTime
        */
        public void GSMDateTimeSet(Int16 GSM_Year, Int16 GSM_Month, Int16 GSM_Day, Int16 GSM_Hour, Int16 GSM_Minute, Int16 GSM_Second)
        {
            bool result = ExecuteDllFunction(() =>
            this._setGSMDateTime(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second)
            , nameof(GSMDateTimeSet));
        }
        /*
         * Note : set GSM DateTime , what is this use ?
         * Important Note : DLL function has bool result but NOT work and it always is True 
        */
        //End : Device Hardware setting

        //Device Network setting
        public bool SimID(out string SimId)
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
            {
                string simID;
                this._rSimID(out simID);
                return new DllCallResult<string> { Result = true, Value = simID };
            }, nameof(SimID));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                SimId = DeviceDefinitions.STRING_INVALID;
                return false;
            }

            if (this.StringCheck(result.Value))
            {
                SimId = result.Value;
                return true;
            }
            else
            {
                SimId = DeviceDefinitions.STRING_INVALID;
                return false;
            }
        }
        /*
         * Important Note :
         * 1. it is number show SIM Card ID
         * 2. if SimID == null means is False to read SimID
        */
        public bool NationalMobileID(out string NMobileID)
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
            {
                string nMobileID;
                this._rNatttionalMobileID(out nMobileID);
                return new DllCallResult<string> { Result = true, Value = nMobileID };
            }, nameof(NationalMobileID));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                NMobileID = DeviceDefinitions.STRING_INVALID;
                return false;
            }

            if (this.StringCheck(result.Value))
            {
                NMobileID = result.Value;
                return true;
            }
            else
            {
                NMobileID = DeviceDefinitions.STRING_INVALID;
                return false;
            }
        }
        /*
         * Important Note :
         * 1. it is number show Special Number about SIM Card and Network
         * 2. if NationalMobileID == null means is False to read NationalMobileID
        */
        public bool SimStatus(ref DeviceDefinitions.SIM_STATUS simStatus)
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
            {
                string rSimPin;
                this._rsimPin(out rSimPin);
                return new DllCallResult<string> { Result = true, Value = rSimPin };
            }, nameof(SimStatus));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                simStatus = DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR;
                return false;
            }

            Int64 value = 0;
            if (this.StringIsInt64(result.Value, ref value))
            {
                if (value == 1)
                {
                    simStatus = DeviceDefinitions.SIM_STATUS.READY;
                    return true;
                }
                else
                {
                    if (value == 2 || value == 3 || value == 4 || value == 5)
                    {
                        simStatus = DeviceDefinitions.SIM_STATUS.PIN_PUK_ERROR;
                        return false;
                    }
                    else
                    {
                        simStatus = DeviceDefinitions.SIM_STATUS.UNKHOWN_ERROR;
                        return false;
                    }
                }
            }
            else
            {
                simStatus = DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR;
                return false;
            }
        }
        /*
         * Note : _rsimPin(result) -> result = -1 => SIM Identity ERROR , result = 1 => SIM Identity Correct , result = 2 => SIM PIN ERROR , result = 3 => SIM PIN2 ERROR , 
         *                            result = 4 => SIM PUK ERROR , result = 5 => SIM PUK2 ERROR , 
        */
        public bool ModemSignal(ref int SignalValue)
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
            {
                string signalValue;
                this._gsmSignal(out signalValue);
                return new DllCallResult<string> { Result = true, Value = signalValue };
            }, nameof(ModemSignal));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                SignalValue = DeviceDefinitions.NUMBER_INVALID;
                return false;
            }

            Int64 value = 0;
            if (this.StringIsInt64(result.Value, ref value))
            {
                SignalValue = Convert.ToInt16(value);
                return true;
            }
            else
            {
                SignalValue = DeviceDefinitions.NUMBER_INVALID;
                return false;
            }
        }
        /*
         * Important Note :
         * 1. SignalValue output is number from 0 to 100 
         * 2. if internal SignalValue(string) == null means is False to read SignalValue
        */
        public bool SimNetworkGet(out string SimNetwork)
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
            {
                string simNetwork;
                this._selectedNetwork(out simNetwork);
                return new DllCallResult<string> { Result = true, Value = simNetwork };
            }, nameof(SimNetworkGet));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                SimNetwork = DeviceDefinitions.STRING_INVALID;
                return false;
            }

            if (this.StringCheck(result.Value))
            {
                SimNetwork = result.Value;
                return true;
            }
            else
            {
                SimNetwork = DeviceDefinitions.STRING_INVALID;
                return false;
            }
        }
        /*
         * Important Note :
         * 1. it is number or string show network name
         * 2. if SimNetwork == null means is False to read SimNetwork
        */
        public void SIMNetworkSelect()
        {
            bool result = ExecuteDllFunction(() => this._selectSIMNetwork(), nameof(SIMNetworkSelect));
        }
        /*
         * Important Note :
         * 1. it select Network automatic
         * 2. it must check work!?
         * 3. SelectSIMNetwork has bool result but NOT work and always is True
        */
        public bool SimPINSet(string SimPIN)
        {
            bool result = ExecuteDllFunction(() => this._insertPIN(SimPIN), nameof(SimPINSet));
            return result;
        }
        /*
         * Note : SimPINSet , is it work ? , output check > is it work or always is TRUE ?
        */
        public bool SmsCenterGet(out string SmsCenter)
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
            {
                string smsCenter;
                this._rsmsCenter(out smsCenter);
                return new DllCallResult<string> { Result = true, Value = smsCenter };
            }, nameof(SmsCenterGet));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                SmsCenter = DeviceDefinitions.STRING_INVALID;
                return false;
            }

            if (this.StringCheck(result.Value))
            {
                SmsCenter = result.Value;
                return true;
            }
            else
            {
                SmsCenter = DeviceDefinitions.STRING_INVALID;
                return false;
            }
        }
        /*
         * Important Note :
         * 1. it is number for SMS Center number
         * 2. if SmsCenter == null means is False to read SmsCenter
        */
        public void SmsCenterSet(string SMS_Center)
        {
            bool result = ExecuteDllFunction(() => this._changesmsCenter(SMS_Center), nameof(SmsCenterSet));
        }
        /*
         * Important Note :
         * 1. SMS_CenterSet return bool result but it is NOT work and return always TRUE
         * 2. it is number for SMS Center number
        */
        public void SmsPDUModeSet(DeviceDefinitions.SMS_TYPE SMS_Type)
        {
            bool _PDU = false , result;

            if (SMS_Type == DeviceDefinitions.SMS_TYPE.ENGLISH)
            {
                _PDU = false;
            }
            else
            {
                if (SMS_Type == DeviceDefinitions.SMS_TYPE.PERSIAN)
                {
                    _PDU = true;
                }
                else
                {
                    return;
                }
            }
            
            result = ExecuteDllFunction(() => { this._smsPDUMode(_PDU); return true; }, nameof(SmsPDUModeSet));
        }
        /*
         * note : this function set SMS PDU mode for send English text or Multi language text
        */
        public void SmsCharModeSet(DeviceDefinitions.SMS_CHAR_MODE SMS_Char_Mode)
        {
            int CharMode = (int)SMS_Char_Mode;
            bool result = ExecuteDllFunction(() => this._smsCharMode(CharMode), nameof(SmsCharModeSet));
        }
        /*
         * note : Char Mode can be : GSM = 1 , HEX = 2 , IRA = 3 , PCCP437 = 4 , 8859-1 = 5 , UCS2 = 6
         * that types very important for USSD code result and SMS Send and SMS Recieved
         * Need more information for correct working
        */
        //End : Device Network setting

        //SMS & AT Command setting
        public void ATCommandDelaySet(Int32 Delay)
        {
            bool result = ExecuteDllFunction(() => { this._resATDelay(Delay); return true; }, nameof(ATCommandDelaySet));
        }
        /*
         * Importan Note :
         * 1. we need function to get this value for check it set correctly
         * 2. BIG PROBLEM : some times it is NOT work
        */
        //End : SMS & AT Command setting

        //SMS Send
        public void SmsSend(out string Result, string Phone_NO, string SMS_Text, bool Request_Delivery_Report, int Send_Interval, int Retry_Times, int SMS_Validity,
                            int SMS_Reference, string SMS_Center)
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
            {
                string smsResult;
                this._sSMS3(out smsResult, Phone_NO, SMS_Text, Request_Delivery_Report, Send_Interval, Retry_Times, SMS_Validity, SMS_Reference, SMS_Center);
                return new DllCallResult<string> { Result = true, Value = smsResult };
            }, nameof(SmsSend));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                Result = DeviceDefinitions.STRING_INVALID;
                return;
            }
            else
            {
                Result = result.Value;
            }
        }
        /*
         * Notes :
         * 1. for use this function first must -> SmsPDUModeSet(SMS_TYPE.Persian)
         * 2. SmsCharModeSet(SMS_Char_Mode) work correctly with all Char Mode both persian language and english language
         * 3. Result show Send Result , Result = NULL => Send Error , sample Result = "R1:22--" and next Send SMS => Result = "R1:23--" and Software Reset => Result = "R1:24--"
         * and USB ReConnect =>  Result = "R1:25--" and Modem full reset => "R1:26--" and Inbox Clear Result = "R1:27--"
         * 4. IMPORTAN: Result comes from Inbox SMS that SMS Type = "SMS STATUS REPORT"
         * 5. Phone_NO = "98" + "9154911200" => Okey Result = "R1:XX--" , Phone_NO = "9154911200" => Error Result = NULL , Phone_NO = "09154911200" => Okey Result = "R1:29--"
         * 6. SMS_Center is NOT work , SMS_Center = "" and SMS_Center = "0123456"
        */
        private bool SmsTextSend(bool PDU, string Country_Code, string Phone_NO, string SMS_TXT)
        {
            bool result = ExecuteDllFunction(() => this._sendTextSMS(PDU, Country_Code, Phone_NO, SMS_TXT), nameof(SmsTextSend));
            return result;
        }
        /*
         * IMPORTANT NOTE : this function NOT WORK
        */
        public string SmsRefrenceGet()
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
            {
                string smsRefrence;
                this._rSMSRef(out smsRefrence);
                return new DllCallResult<string> { Result = true, Value = smsRefrence };
            }, nameof(SmsRefrenceGet));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                return DeviceDefinitions.STRING_INVALID;
            }
            else
            {
                return result.Value;
            }
        }
        /*
         * IMPORTANT NOTE : it is return last succeed Send SMS refrence Code -> first load = NULL , then 110 , 111 , 112 , ... for each new succeed send X+1
        */
        private string SMSSendedErrorCode()
        {
            DllCallResult<string> result = ExecuteDllFunction(() =>
            {
                string smsErrorCode;
                this._sendSMSErrorCode(out smsErrorCode);
                return new DllCallResult<string> { Result = true, Value = smsErrorCode };
            }, nameof(SMSSendedErrorCode));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                return DeviceDefinitions.STRING_INVALID;
            }
            else
            {
                return result.Value;
            }
        }
        /*
         * MPORTANT NOTE : it is return last Send SMS Error code in Persian Language message : "خطا در ارسال پيامک1" , "پيامک با موفقيت ارسال شد" -> Not Useful
        */
        //End : SMS Send

        //AT Command
        public void ATCommandSend(string AT_Input, ref int AtOutput_Length, out string AT_OutPut)
        {
            AtCommandResult result = ExecuteDllFunction(() =>
            {
                int len = 0;
                string resultStr = string.Empty;
                AtCommandResult output = new AtCommandResult();
                this._gsmSendAT(AT_Input,ref len,out resultStr);
                output.OutputLength = len;
                output.Output = resultStr;
                return output;
            }, nameof(ATCommandSend));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                AT_OutPut = DeviceDefinitions.STRING_INVALID;
                AtOutput_Length = 0;
            }
            else
            {
                AT_OutPut = result.Output;
                AtOutput_Length = result.OutputLength;
            }
        }
        //End : AT Command

        //Buffer Manager
        /*
         * about Buffer :
         * 1. Buffer is queue start Index = 1 to BufferLastIndex
         * 2. it is auto distroyed after read, it is'nt depend on DLL read or user read in both it destroy
         * 3. IMPORTANT : befor some actions must clear buffer manually , because some command in DLL layer not get response but there is in memory and
         * All at once back in another command that need response
         * 4. buffer is in DLL layer (high probebility)
        */
        public Int16 BufferLastIndexGet()
        {
            Int16 result = ExecuteDllFunction(() => this._lastBufIndex(), nameof(BufferLastIndexGet));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                return DeviceDefinitions.NUMBER_INVALID;
            }
            else
            {
                return result;
            }
        }
        /*
         * Note : result = -1 -> error happen , result = 0 -> Buffer empty , result = 1 to X -> X is last buffer index carry modem response
        */
        public void GsmMessage(ref int BufferIndex, out string Result, ref int ResultLen)
        {
            int bufferIndex = BufferIndex;
            GsmMessageResult result = ExecuteDllFunction(() =>
            {
                GsmMessageResult output = new GsmMessageResult();
                output.Index = bufferIndex;
                this._gsmMSG(out output.Message, ref output.Index, ref output.ResultLength);
                return output;
            }, nameof(GsmMessage));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                Result = DeviceDefinitions.STRING_INVALID;
                ResultLen = DeviceDefinitions.NUMBER_INVALID;
                BufferIndex = DeviceDefinitions.NUMBER_INVALID;
            }
            else
            {
                Result = result.Message;
                ResultLen = result.ResultLength;
                BufferIndex = result.Index;
            }
        }
        /*
         * Important Note : in DLL layer they use "ref int GSM_CMD" insted of "int BufferIndex"
         * if buffer[BufferIndex] is empty or read previously -> Result = "" and GSM_Len = 0
        */
        public void ClearBuffer()
        {
            bool result = ExecuteDllFunction(() => { this._clearBuf(); return true; }, nameof(ClearBuffer));
        }
        /*
         * note : Clear buffer in DLL layer
        */
        //End : Buffer Manager

        /*
        //Group SMS
        public void SmsGroupSetting(bool SMS_Delivery, int SMS_Validity, int SMS_NO_Change_Sim, int Repeat_NO, int Send_Interval, string SMS_Center)
        {
            try
            {
                this.ErrorReset();
                this._groupSMSSetting(SMS_Delivery, SMS_Validity, SMS_NO_Change_Sim, Repeat_NO, Send_Interval, SMS_Center);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GetSMSRefrence - " + ex.Message;
            }
        }
        public void SmsGroupAddBuffer(string Phone_NO, string SMS_Context)
        {
            try
            {
                this.ErrorReset();
                this._addGroupSMSBuffer(Phone_NO, SMS_Context);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GroupSMSAddBuffer - " + ex.Message;
            }
        }
        public void SmsGroupSendPaus()
        {
            try
            {
                this.ErrorReset();
                this._smsBufPause();
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GroupSMSSendPaus - " + ex.Message;
            }
        }
        public int SmsGroupSendedNumber()
        {
            try
            {
                this.ErrorReset();
                return this._getSMSBufSended();
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GroupSMSSendedNumber - " + ex.Message;
                return -1;
            }
        }
        public int SmsGroupBufferNumber()
        {
            try
            {
                this.ErrorReset();
                return this._getSMSBufIndx();
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GroupSMSBufferNumber - " + ex.Message;
                return -1;
            }
        }
        public void SmsGroupSendStartStop(bool Start)
        {
            try
            {
                this.ErrorReset();
                this._smsBufStart(Start);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GroupSMSSendStartStop - " + ex.Message;
            }
        }
        public void SmsGroupBufferClear(bool Clear)
        {
            try
            {
                this.ErrorReset();
                this._smsBufClear(Clear);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GroupSMSBufferClear - " + ex.Message;
            }
        }
        //End : Group SMS
        */

        //SMS Inbox
        //New SMS Inbox Event , ACtive/DeActive , IndexOfSMS/SMS Number
        public void SmsInboxAlarm(bool Active)
        {
            bool result = ExecuteDllFunction(() => this._activeNewSMSEvent(Active), nameof(SmsInboxAlarm));
        }
        /*
         * Important Note :
         * 1. bool result of this._activeNewSMSEvent(Active) is true every time
         * 2. if Active = True alarm event for new SMS is active
        */
        public bool SmsNewRecieved(ref int SMS_Index, ref int SMS_Number)
        {
            SmsNewReceivedResult result = ExecuteDllFunction(() =>
            {
                SmsNewReceivedResult output = new SmsNewReceivedResult();
                output.Result = this._recievedNewSMS(ref output.SmsIndex, ref output.SmsNumber);
                return output;
            }, nameof(SmsNewRecieved));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                SMS_Index = DeviceDefinitions.NUMBER_INVALID;
                SMS_Number = DeviceDefinitions.NUMBER_INVALID;
                return false;
            }

            if (result.SmsIndex > DeviceDefinitions.NUMBER_INVALID && result.SmsNumber > DeviceDefinitions.NUMBER_INVALID)
            {
                SMS_Index = result.SmsIndex;
                SMS_Number = result.SmsNumber;
                return true;
            }
            else
            {
                SMS_Index = DeviceDefinitions.NUMBER_INVALID;
                SMS_Number = DeviceDefinitions.NUMBER_INVALID;
                return false;
            }
        }
        /*
         * Important Note:
         * 1. for use this function first must call SmsInboxAlarm(True)
         * 2. when Inbox Alarm is active and call this function if new Inbox recieved then SMS_Index is first new Inbox Index and SMS_Number is number of new inbox
         * 3. if SMS_Index = -1 or SMS_Number = -1 means NO New Inbox => result = False
        */
        //End : New SMS Inbox Event , ACtive/DeActive , IndexOfSMS/SMS Number
        //Inbox Storage
        public bool SmsInboxNumber(ref int SMS_Number)
        {
            int result = ExecuteDllFunction(() => this._getSMSNO(), nameof(SmsInboxNumber));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                SMS_Number = DeviceDefinitions.NUMBER_INVALID;
                return false;
            }
            if (result > DeviceDefinitions.NUMBER_INVALID)
            {
                SMS_Number = result;
                return true;
            }
            else
            {
                SMS_Number = DeviceDefinitions.NUMBER_INVALID;
                return false;
            }
        }
        /*
         * Important Note:
         * 1. if result = -1 => read fail -> result = false
        */
        public bool SmsInboxStatus(ref int SMSInboxTotalSize, ref int SMSInboxFreeSize)
        {
            SMS_Space_Struct result = ExecuteDllFunction(() =>
            {
                this._getSMSSpace(out SMS_Space_Struct smsSpace); return smsSpace;
            }, nameof(SmsInboxStatus));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                SMSInboxTotalSize = DeviceDefinitions.NUMBER_INVALID;
                SMSInboxFreeSize = DeviceDefinitions.NUMBER_INVALID;
                return false;
            }

            if (result.Total_Size > DeviceDefinitions.NUMBER_INVALID && result.Free_Space > DeviceDefinitions.NUMBER_INVALID)
            {
                SMSInboxTotalSize = result.Total_Size;
                SMSInboxFreeSize = result.Free_Space;
                return true;
            }
            else
            {
                SMSInboxTotalSize = DeviceDefinitions.NUMBER_INVALID;
                SMSInboxFreeSize = DeviceDefinitions.NUMBER_INVALID;
                return false;
            }
        }
        /*
         * Important Note:
         * 1. if SMSInboxTotalSize = -1 or SMSInboxFreeSize = -1 => read fail -> result = false
        */
        //End : Inbox Storage
        //SMS Inbox Delete
        public void SmsDelete(int Index, DeviceDefinitions.SMS_DELETE_TYPE SmsDeleteType)
        {
            bool result = ExecuteDllFunction(() => this._smsDelete(Index, (int)SmsDeleteType), nameof(SmsDelete));
        }
        /*
         * Important Note:
         * 1. _smsDelete result always is True => bool output NOT Work
        */
        //End : SMS Inbox Delete
        //SMS Inbox raed
        public bool SmsInboxGet(int SMS_Index, out string SMS_Text, out string SMS_Center, out string SMS_Type, out string Tel_Number,
                                out string Time_stamp, out string S_Discharge, out string TP_PID, out string TP_DCS,
                                out string TP_VP, out string TP_ST, out string S_Reference, out string All_Part, out string Part_NO)
        {
            SMS_Text = DeviceDefinitions.STRING_INVALID;
            SMS_Center = DeviceDefinitions.STRING_INVALID;
            SMS_Type = DeviceDefinitions.STRING_INVALID;
            Tel_Number = DeviceDefinitions.STRING_INVALID;
            Time_stamp = DeviceDefinitions.STRING_INVALID;
            S_Discharge = DeviceDefinitions.STRING_INVALID;
            TP_PID = DeviceDefinitions.STRING_INVALID;
            TP_DCS = DeviceDefinitions.STRING_INVALID;
            TP_VP = DeviceDefinitions.STRING_INVALID;
            TP_ST = DeviceDefinitions.STRING_INVALID;
            S_Reference = DeviceDefinitions.STRING_INVALID;
            All_Part = DeviceDefinitions.STRING_INVALID;
            Part_NO = DeviceDefinitions.STRING_INVALID;

            SmsInboxMessageResult result = ExecuteDllFunction(() =>
            {
                SmsInboxMessageResult output = new SmsInboxMessageResult();
                bool pDU = true;
                string stringResult = string.Empty;
                output.Result = this._rSMS4(out stringResult, pDU, SMS_Index, out output.SmsCenter, out output.SmsType, out output.TelNumber, 
                                            out output.TimeStamp , out output.SDischarge, out output.SmsText, out output.TpPid, out output.TpDcs,
                                            out output.TpVp, out output.TpSt, out output.SReference, out output.AllPart, out output.PartNo);

                return output;
            }, nameof(SmsInboxGet));

            if (this._errorCode != ERROR_CODE.NO_ERROR)
            {
                return false;
            }

            if (result.Result)
            {
                SMS_Text = result.SmsText;
                SMS_Center = result.SmsCenter;
                SMS_Type = result.SmsType;
                Tel_Number = result.TelNumber;
                Time_stamp = result.TimeStamp;
                S_Discharge = result.SDischarge;
                TP_PID = result.TpPid;
                TP_DCS = result.TpDcs;
                TP_VP = result.TpVp;
                TP_ST = result.TpSt;
                S_Reference = result.SReference;
                All_Part = result.AllPart;
                Part_NO = result.PartNo;
            }
            return result.Result;
        }
        //End : SMS Inbox raed
        /*
         * Important Note
         * 1. PDU NOT work, for recieve Persina language must use first SmsPDUModeSet() else maybe recieve SMS ="????" insted of persian character
         * 2. Result and SMS_Text always is same, so use one of this
         * IMPORTANT : 3. first set PDU = True else i recieved in pcket and code
         * sample : SMS_Type = SMS RECEIVED , Tel_Number = +989154911200 , Time_stamp = 2026/08/08-17:44:06-03.5 , SMS_Text = تست , TP_PID = 0 , TP_DCS = 8
         * sample PDU = False -> SMS_Text = " \"REC READ\",\"+989154911200\",\"\",\"26/08/08,17:44:06+20\"\r???\r\rOK\r"
         *                       SMS_Type = null , Tel_Number = null , Time_stamp = null , TP_PID = null , TP_DCS = null
         * 
        */
        //End : SMS Inbox

    }
}
