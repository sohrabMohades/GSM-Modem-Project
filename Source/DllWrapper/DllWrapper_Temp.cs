/*
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
        //Variable for functions

        //Common Main function
        internal SMS_Device_Count_Delegete _smsDeviceCount;
        //End : Common Main function

        //Device connection
        private SMS_Select_Device_Delegete _smsSelectDevice;
        private SMS_SelectDevice_ByIndex_Delegete _smsSelectDeviceByIndex;
        //End : Device connection

        //Device Hardware setting
        private GSM_Set_Device_NO_Delegete _gsmSetDeviceNO;
        private GSM_Get_Device_NO_Delegete _gsmGetDeviceNO;
        private R_IMEI_Delegete _rIMEI;
        private GSM_ON_Delegete _gsmON;
        private GSM_OFF_Delegete _gsmOFF;
        private R_GSM_Pow_Delegete _rgsmPow;
        private GSM_Ready_Delegete _gsmReady;
        private GSM_Test_Connect_Delegete _gsmTestConnect;
        private SMS_Dev_INFO_Delegete _smsDevInfo;
        private SMS_Soft_Ver_Delegete _smsSoftVer;
        private Get_GSM_Message_Delegete _getGSMMessage;
        private R_GSM_Date_Time_Delegete _rgsmDateTime;
        private Set_GSM_Date_Time_Delegete _setGSMDateTime;
        //End : Device Hardware setting

        //Device Network setting
        private R_Natttional_Mobile_ID_Delegete _rNatttionalMobileID;
        private R_SIM_ID_Delegete _rSimID;
        private Selected_Network_Delegete _selectedNetwork;
        private R_SIM_PIN_Delegete _rsimPin;
        private GSM_Signal_Delegete _gsmSignal;
        private Select_SIM_Network_Delegete _selectSIMNetwork;
        private Insert_PIN_Delegete _insertPIN;
        private R_SMS_Center_Delegete _rsmsCenter;
        private Change_SMS_Center_Delegete _changesmsCenter;
        private SMS_PDU_Mode_Delegete _smsPDUMode;
        private SMS_Char_Mode_Delegete _smsCharMode;
        //End : Device Network setting

        //SMS & AT Command setting
        private Res_AT_Delay_Delegete _resATDelay;
        //End : SMS & AT Command setting

        //SMS Send
        private S_SMS3_Delegete _sSMS3;
        //Send_Text_SMS_Delegete -> NOT WORK
        private Send_Text_SMS_Delegete _sendTextSMS;
        private R_SMS_Ref_Delegete _rSMSRef;
        private Send_SMS_Error_Code_Delegete _sendSMSErrorCode;
        //End : SMS Send

        //AT Command
        private GSM_Send_AT_Delegete _gsmSendAT;
        //End : AT Command

        //Buffer Manager
        private Last_Buf_Index_Delegete _lastBufIndex;
        private GSM_MSG_Delegete _gsmMSG;
        private Clear_Buf_Delegete _clearBuf;
        //End : Buffer Manager

        //SMS Inbox
        //New SMS Inbox Event , ACtive/DeActive , IndexOfSMS/SMS Number
        private Active_New_SMS_Event_Delegete _activeNewSMSEvent;
        private Recieved_New_SMS_Delegete _recievedNewSMS;
        //End : New SMS Inbox Event , ACtive/DeActive , IndexOfSMS/SMS Number
        //Inbox Storage
        private Get_SMS_NO_Delegete _getSMSNO;
        private Get_SMS_Space_Delegete _getSMSSpace;
        //End : Inbox Storage
        //SMS Inbox Delete
        private SMS_Delete_Delegete _smsDelete;
        //End : SMS Inbox Delete
        //SMS Inbox raed
        private R_SMS4_Delegete _rSMS4;
        //End : SMS Inbox raed
        //End : SMS Inbox

        /*
        //Group SMS
        private Group_SMS_Setting_Delegete _groupSMSSetting;
        private Add_Group_SMS_Buffer_Delegete _addGroupSMSBuffer;
        private Get_SMS_Buf_Status_Delegete _getSMSBufStatus;
        private SMS_Buf_Pause_Delegete _smsBufPause;
        private Get_SMS_Buf_Sended_Delegete _getSMSBufSended;
        private Get_SMS_Buf_Indx_Delegete _getSMSBufIndx;
        private SMS_Buf_Start_Delegete _smsBufStart;
        private SMS_Buf_Clear_Delegete _smsBufClear;
        //End : Group SMS
        */

        /*
        //Offline SMS -> Not Work
        private Total_Oflline_SMS_NO_Delegete _totalOfllineSMSNO;
        private Show_Oflline_SMS_Delegete _showOfllineSMS;
        private Clear_Oflline_SMS_NO_Delegete _clearOfllineSMSNO;
        private G_Offline_SMS_Active_Delegete _gOfflineSMSActive;
        private Offline_SMS_Active_Delegete _offlineSMSActive;
        //End : Offline SMS -> Not Work
        */

        /*
        //Call Functions
        private Disconnect_Voice_Call_Delegete _disconnectVoiceCall;
        private GSM_Answer_Call_Delegete _gsmAnswerCall;
        private GSM_Dial_NO_Delegete _gsmDialNO;
        private CallerID_Enable_Delegete _callerIDEnable;
        private CallIn_Number_Delegete _callInNumber;
        //End : Call Functions
        */
/*
        //Variable for manage class
        internal STATUS _status = STATUS.INVALID;
        internal ERROR_CODE _errorCode = ERROR_CODE.UNKNOWN_ERROR;
        internal string _errorMessage = "";
        internal int _dllTimeoutMs = 10000;
        //End : Variable for manage class

        //Set & Get Property
        public STATUS Status
        {
            get
            {
                return this._status;
            }
        }
        public ERROR_CODE ErrorCode
        {
            get
            {
                return this._errorCode;
            }
        }
        public string ErrorMessage
        {
            get
            {
                return this._errorMessage;
            }
        }
        private void ErrorReset()
        {
            this._errorCode = ERROR_CODE.NO_ERROR;
            this._errorMessage = "";
        }
        //End : Set & Get Property

        //Private Functions
        private bool StringCheck(string str)
        {
            try
            {
                if (str == null || str.Length == 0 || str == "" || str == " ")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool StringIsInt64(string str, ref Int64 Number)
        {
            Number = -1;
            try
            {
                if (str == null || str.Length == 0 || str == "" || str == " ")
                {
                    return false;
                }
                else
                {
                    if (Convert.ToInt64(str) >= -1)
                    {
                        if (Convert.ToInt64(str) == -1)
                        {
                            return (false);
                        }
                        else
                        {
                            Number = Convert.ToInt64(str);
                            return (true);
                        }
                    }
                    else
                    {
                        if (Convert.ToInt64(str.Substring(1)) >= 0)
                        {
                            Number = Convert.ToInt64(str.Substring(1));
                            return (true);
                        }
                        else
                        {
                            return (false);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return (false);
            }
        }
        //End : Private Functions

        //Public function

        //Common Main function
        public Int16 DeviceCount()
        {
            try
            {
                this.ErrorReset();
                return this._smsDeviceCount();
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "DeviceCount - " + ex.Message;
                return 0;
            }
        }
        /*
         * Note : return Device Number connected to computer
        */
        //End : Common Main function
/*
        //Device connection
        public void SelectByDeviceNumber(Int16 DeviceNumber)
        {
            try
            {
                this.ErrorReset();
                this._smsSelectDevice(DeviceNumber);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "DeviceSelectByDeviceNumber - " + ex.Message;
            }
        }
        /*
         * Important Note :
         * 1. if 2 devices have equal Device Number when choose this Device Number UnKnown situation happen
         * 2. for confirmation select must use read Device Number
        */
/*
        public void SelectByDeviceIndex(Int16 DeviceIndex)
        {
            try
            {
                this.ErrorReset();
                this._smsSelectDeviceByIndex(DeviceIndex);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "DeviceSelectByIndex - " + ex.Message;
            }
        }
        /*
         * Important Note :
         * 1. index sorted by device hardware connections and always start from (0) to (total device number - 1)
         * 2. when any device DC it is sorted automatic such as 1. -> 0 , 1 , 2 , 3 , 4 then remove 2 -> new result = 0 , 1 , 2 , 3
         * 3. for confirmation select must use read Device Number
        */
        //End : Device connection
/*
        //Device Hardware setting
        public void DeviceNumberSet(int DeviceNumber)
        {
            try
            {
                this.ErrorReset();
                this._gsmSetDeviceNO(DeviceNumber);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "DeviceNumberSet - " + ex.Message;
            }
        }
        /*
         * Important Note:
         * 1. DeviceNumberSet has bool result but not work and is True every times
         * so i remove this invalid output
        */
/*
        public bool DeviceNumberGet(ref int DeviceNumber)
        {
            try
            {
                this.ErrorReset();
                DeviceNumber = this._gsmGetDeviceNO();
                if (DeviceNumber == -1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "DeviceNumberGet - " + ex.Message;
                DeviceNumber = -1;
                return false;
            }
        }
        /*
         * Important Note :
         * 1. if DeviceNumber == -1 means is False read Device Number
        */
/*
        public bool IMEI(out string IMEI)
        {
            try
            {
                this.ErrorReset();
                this._rIMEI(out IMEI);
                if (this.StringCheck(IMEI))
                {
                    return true;
                }
                else
                {
                    IMEI = "";
                    return false;
                }
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GSM_IMEI - " + ex.Message;
                IMEI = "";
                return false;
            }
        }
        /*
         * Important Note :
         * 1. if IMEI == null means is False to read Device IMEI
        */
/*
        public void GsmPowerSet(bool Power)
        {
            if (Power)
            {
                try
                {
                    this.ErrorReset();
                    this._gsmON();
                }
                catch (Exception ex)
                {
                    this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                    this._errorMessage = "GSM_ON - " + ex.Message;
                }
            }
            else
            {
                try
                {
                    this.ErrorReset();
                    this._gsmOFF();
                }
                catch (Exception ex)
                {
                    this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                    this._errorMessage = "GSM_OFF - " + ex.Message;
                }
            }
        }
        /*
         * Important Notes :
         * 1. when adaptor is DC from modem this functions maybe work or not wor
         * 2. GSM_OFF if modem is ON then this is OFF and if modem is OFF then this is ON
         * so the bool result of GSM_OFF not trustable , you must use GsmPowerGet() for modem power status
         * 3. Important : after change SIM Card i must Power OFF then Power ON -> reset for identify new SIM Card
        */
/*
        public bool GsmPowerGet()
        {
            try
            {
                this.ErrorReset();
                return this._rgsmPow();
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GSM_Status - " + ex.Message;
                return false;
            }
        }
        /*
         * note : GSM Power is ON = true or is OFF = False or NO response = False
        */
/*
        public bool GsmIsReady()
        {
            try
            {
                this.ErrorReset();
                return this._gsmReady();
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GSMIsReady - " + ex.Message;
                return false;
            }
        }
        /*
         * note : for extra check
        */
/*
        public bool GsmIsConnect()
        {
            try
            {
                this.ErrorReset();
                return this._gsmTestConnect();
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GSMIsConnect - " + ex.Message;
                return false;
            }
        }
        /*
         * note : for extra check
        */
/*
        public bool DeviceInfo(out string DeviceInfo)
        {
            try
            {
                this.ErrorReset();
                this._smsDevInfo(out DeviceInfo);
                return this.StringCheck(DeviceInfo);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "DeviceInfo - " + ex.Message;
                DeviceInfo = "";
                return false;
            }
        }
        /*
         * Important Note :
         * 1. if DeviceInfo == null means is False to read DeviceInfo
        */
/*
        public bool SoftwareVersion(out string SofteareVersion)
        {
            try
            {
                this.ErrorReset();
                this._smsSoftVer(out SofteareVersion);
                return this.StringCheck(SofteareVersion);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SoftwareVersion - " + ex.Message;
                SofteareVersion = "";
                return false;
            }
        }
        /*
         * Important Note :
         * 1. if SofteareVersion == null means is False to read SofteareVersion
        */
/*
        private int GsmStatus(out string TextMessage)
        {
            try
            {
                this.ErrorReset();
                this._getGSMMessage(out GSM_Message_Struct result);
                TextMessage = result.Message_Text;
                return result.Result;
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GetGSMMessage - " + ex.Message;
                TextMessage = "";
                return -1;
            }
        }
        /*
         * Important : NOT TRUSTABLE OUTPUT -> modem is disconnect but return last status = 0 , modem is connect but return last status = -1
         * Important Note : NO help available for this function
         * 1. result.Result = -1 , result.Message_Text = null -> NOT Ready
         * 2. result.Result = 2 , result.Message_Text = "MODEM:STARTUP"
         * 3. result.Result = 0 , result.Message_Text = "+PBREADY"
        */
/*
        public bool GsmDateTimeGet(out string GsmDateTime)
        {
            try
            {
                this.ErrorReset();
                this._rgsmDateTime(out GsmDateTime);
                return this.StringCheck(GsmDateTime);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GSM_DateTime - " + ex.Message;
                GsmDateTime = "";
                return false;
            }
        }
        /*
         * Note : get GSM DateTime , what is this use ?
         * 1. if GsmDateTime == null means is False to read GsmDateTime
        */
/*
        public void GSMDateTimeSet(Int16 GSM_Year, Int16 GSM_Month, Int16 GSM_Day, Int16 GSM_Hour, Int16 GSM_Minute, Int16 GSM_Second)
        {
            try
            {
                this.ErrorReset();
                this._setGSMDateTime(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SetGSMDateTime - " + ex.Message;
            }
        }
        /*
         * Note : set GSM DateTime , what is this use ?
         * Important Note : DLL function has bool result but NOT work and it always is True 
        */
        //End : Device Hardware setting
/*
        //Device Network setting
        public bool SimID(out string SimID)
        {
            try
            {
                this.ErrorReset();
                this._rSimID(out SimID);
                if (this.StringCheck(SimID))
                {
                    return true;
                }
                else
                {
                    SimID = "";
                    return false;
                }
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SimID - " + ex.Message;
                SimID = "";
                return false;
            }
        }
        /*
         * Important Note :
         * 1. it is number show SIM Card ID
         * 2. if SimID == null means is False to read SimID
        */
/*
        public bool NationalMobileID(out string NMobileID)
        {
            try
            {
                this.ErrorReset();
                this._rNatttionalMobileID(out NMobileID);
                if (this.StringCheck(NMobileID))
                {
                    return true;
                }
                else
                {
                    NMobileID = "";
                    return false;
                }
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "NatttionalMobileID - " + ex.Message;
                NMobileID = "";
                return false;
            }
        }
        /*
         * Important Note :
         * 1. it is number show Special Number about SIM Card and Network
         * 2. if NationalMobileID == null means is False to read NationalMobileID
        */
/*
        public bool SimStatus(ref SIM_STATUS SimStatus)
        {
            try
            {
                Int64 value = 0;
                this.ErrorReset();
                this._rsimPin(out string result);
                if (this.StringIsInt64(result, ref value))
                {
                    if (value == 1)
                    {
                        SimStatus = SIM_STATUS.READY;
                        return true;
                    }
                    else
                    {
                        if (value == 2 || value == 3 || value == 4 || value == 5)
                        {
                            SimStatus = SIM_STATUS.PIN_PUK_ERROR;
                            return false;
                        }
                        else
                        {
                            SimStatus = SIM_STATUS.UNKHOWN_ERROR;
                            return false;
                        }
                    }
                }
                else
                {
                    SimStatus = SIM_STATUS.IDENTITY_ERROR;
                    return false;
                }
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SimPinStatus - " + ex.Message;
                SimStatus = SIM_STATUS.IDENTITY_ERROR;
                return false;
            }
        }
        /*
         * Note : _rsimPin(result) -> result = -1 => SIM Identity ERROR , result = 1 => SIM Identity Correct , result = 2 => SIM PIN ERROR , result = 3 => SIM PIN2 ERROR , 
         *                            result = 4 => SIM PUK ERROR , result = 5 => SIM PUK2 ERROR , 
        */
/*
        public bool ModemSignal(ref int SignalValue)
        {
            try
            {
                Int64 value = 0;
                this.ErrorReset();
                this._gsmSignal(out string result);
                if (this.StringIsInt64(result, ref value))
                {
                    SignalValue = Convert.ToInt16(value);
                    return true;
                }
                else
                {
                    SignalValue = 0;
                    return false;
                }
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "ModemSignal - " + ex.Message;
                SignalValue = 0;
                return false;
            }
        }
        /*
         * Important Note :
         * 1. SignalValue output is number from 0 to 100 
         * 2. if internal SignalValue(string) == null means is False to read SignalValue
        */
/*
        public bool SimNetworkGet(out string SimNetwork)
        {
            try
            {
                this.ErrorReset();
                this._selectedNetwork(out SimNetwork);
                if (this.StringCheck(SimNetwork))
                {
                    return true;
                }
                else
                {
                    SimNetwork = "";
                    return false;
                }
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SimNetwork - " + ex.Message;
                SimNetwork = "";
                return false;
            }
        }
        /*
         * Important Note :
         * 1. it is number or string show network name
         * 2. if SimNetwork == null means is False to read SimNetwork
        */
/*
        public void SIMNetworkSelect(bool Enable)
        {
            try
            {
                this.ErrorReset();
                this._selectSIMNetwork();
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SelectSIMNetwork - " + ex.Message;
            }
        }
        /*
         * Important Note :
         * 1. it select Network automatic
         * 2. it must check work!?
         * 3. SelectSIMNetwork has bool result but NOT work and always is True
        */
/*
        public bool SimPINSet(string SimPIN)
        {
            try
            {
                this.ErrorReset();
                return this._insertPIN(SimPIN);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "InsertSimPIN - " + ex.Message;
                return false;
            }
        }
        /*
         * Note : SimPINSet , is it work ? , output check > is it work or always is TRUE ?
        */
/*
        public bool SmsCenterGet(out string SmsCenter)
        {
            try
            {
                this.ErrorReset();
                this._rsmsCenter(out SmsCenter);
                return this.StringCheck(SmsCenter);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SMS_CenterGet - " + ex.Message;
                SmsCenter = "";
                return false;
            }
        }
        /*
         * Important Note :
         * 1. it is number for SMS Center number
         * 2. if SmsCenter == null means is False to read SmsCenter
        */
/*
        public void SmsCenterSet(string SMS_Center)
        {
            try
            {
                this.ErrorReset();
                this._changesmsCenter(SMS_Center);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SMS_CenterSet - " + ex.Message;
            }
        }
        /*
         * Important Note :
         * 1. SMS_CenterSet return bool result but it is NOT work and return always TRUE
         * 2. it is number for SMS Center number
        */
/*
        public void SmsPDUModeSet(SMS_TYPE SMS_Type)
        {
            try
            {
                bool _PDU = false;
                if (SMS_Type == SMS_TYPE.ENGLISH)
                {
                    _PDU = false;
                }
                else
                {
                    if (SMS_Type == SMS_TYPE.Persian)
                    {
                        _PDU = true;
                    }
                    else
                    {
                        return;
                    }
                }
                this.ErrorReset();
                this._smsPDUMode(_PDU);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SMS_PDU_Mode - " + ex.Message;
            }
        }
        /*
         * note : this function set SMS PDU mode for send English text or Multi language text
        */
/*
        public void SmsCharModeSet(SMS_CHAR_MODE SMS_Char_Mode)
        {
            try
            {
                int CharMode = (int)SMS_Char_Mode;
                this.ErrorReset();
                this._smsCharMode(CharMode);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SMS_Char_Mode - " + ex.Message;
            }
        }
        /*
         * note : Char Mode can be : GSM = 1 , HEX = 2 , IRA = 3 , PCCP437 = 4 , 8859-1 = 5 , UCS2 = 6
         * that types very important for USSD code result and SMS Send and SMS Recieved
         * Need more information for correct working
        */
        //End : Device Network setting
/*
        //SMS & AT Command setting
        public void ATCommandDelaySet(Int32 Delay)
        {
            try
            {
                this.ErrorReset();
                this._resATDelay(Delay);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SetATCommandDelay - " + ex.Message;
            }
        }
        /*
         * Importan Note :
         * 1. we need function to get this value for check it set correctly
         * 2. BIG PROBLEM : some times it is NOT work
        */
        //End : SMS & AT Command setting
/*
        //SMS Send
        public void SmsSend(out string Result, string Phone_NO, string SMS_Text, bool Request_Delivery_Report, int Send_Interval, int Retry_Times, int SMS_Validity,
                            int SMS_Reference, string SMS_Center)
        {
            try
            {
                this.ErrorReset();
                this._sSMS3(out Result, Phone_NO, SMS_Text, Request_Delivery_Report, Send_Interval, Retry_Times, SMS_Validity, SMS_Reference, SMS_Center);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SendSMS - " + ex.Message;
                Result = "";
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
/*
        private bool SmsTextSend(bool PDU, string Country_Code, string Phone_NO, string SMS_TXT)
        {
            try
            {
                this.ErrorReset();
                return this._sendTextSMS(PDU, Country_Code, Phone_NO, SMS_TXT);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SendTextSMS - " + ex.Message;
                return false;
            }
        }
        /*
         * IMPORTANT NOTE : this function NOT WORK
        */
/*
        public string SmsRefrenceGet()
        {
            try
            {
                this.ErrorReset();
                this._rSMSRef(out string result);
                return result;
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GetSMSRefrence - " + ex.Message;
                return "";
            }
        }
        /*
         * IMPORTANT NOTE : it is return last succeed Send SMS refrence Code -> first load = NULL , then 110 , 111 , 112 , ... for each new succeed send X+1
        */
/*
        private string SMSSendedErrorCode()
        {
            try
            {
                this.ErrorReset();
                this._sendSMSErrorCode(out string result);
                return result;
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SMSSendedErrorCode - " + ex.Message;
                return "";
            }
        }
        /*
         * MPORTANT NOTE : it is return last Send SMS Error code in Persian Language message : "خطا در ارسال پيامک1" , "پيامک با موفقيت ارسال شد" -> Not Useful
        */
        //End : SMS Send
/*
        //AT Command
        public void ATCommandSend(string AT_Input, ref int AtOutput_Length, out string AT_OutPut)
        {
            try
            {
                this.ErrorReset();
                this._gsmSendAT(AT_Input, ref AtOutput_Length, out AT_OutPut);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SendATCommand - " + ex.Message;
                AT_OutPut = "";
                AtOutput_Length = 0;
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
/*
        public Int16 BufferLastIndexGet()
        {
            try
            {
                this.ErrorReset();
                return this._lastBufIndex();
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GetBufferLastIndex - " + ex.Message;
                return -1;
            }
        }
        /*
         * Note : result = -1 -> error happen , result = 0 -> Buffer empty , result = 1 to X -> X is last buffer index carry modem response
        */
/*
        public void GsmMessage(int BufferIndex, out string Result, ref int ResultLen)
        {
            try
            {
                this.ErrorReset();
                this._gsmMSG(out Result, ref BufferIndex, ref ResultLen);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GSMMessage - " + ex.Message;
                Result = "";
                ResultLen = 0;
            }
        }
        /*
         * Important Note : in DLL layer they use "ref int GSM_CMD" insted of "int BufferIndex"
         * if buffer[BufferIndex] is empty or read previously -> Result = "" and GSM_Len = 0
        */
/*
        public void ClearBuffer()
        {
            try
            {
                this.ErrorReset();
                this._clearBuf();
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "ClearBuffer - " + ex.Message;
            }
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
/*
        //SMS Inbox
        //New SMS Inbox Event , ACtive/DeActive , IndexOfSMS/SMS Number
        public void SmsInboxAlarm(bool Active)
        {
            try
            {
                this.ErrorReset();
                this._activeNewSMSEvent(Active);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "ActiveNewSMSInboxAlarm - " + ex.Message;
            }
        }
        /*
         * Important Note :
         * 1. bool result of this._activeNewSMSEvent(Active) is true every time
         * 2. if Active = True alarm event for new SMS is active
        */
/*
        public bool SmsNewRecieved(ref int SMS_Index, ref int SMS_Number)
        {
            try
            {
                this.ErrorReset();
                this._recievedNewSMS(ref SMS_Index, ref SMS_Number);
                if (SMS_Index > -1 && SMS_Number > -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "RecievedNewSMS - " + ex.Message;
                SMS_Index = -1;
                SMS_Number = -1;
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
/*
        public bool SmsInboxNumber(ref int SMS_Number)
        {
            try
            {
                this.ErrorReset();
                SMS_Number = this._getSMSNO();
                if (SMS_Number < 0)
                {
                    SMS_Number = -1;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SMSInboxNumber - " + ex.Message;
                SMS_Number = -1;
                return false;
            }
        }
        /*
         * Important Note:
         * 1. if result = -1 => read fail -> result = false
        */
/*
        public bool SmsInboxStatus(ref int SMSInboxTotalSize, ref int SMSInboxFreeSize)
        {
            try
            {
                this.ErrorReset();
                this._getSMSSpace(out SMS_Space_Struct result);
                if (result.Total_Size < 0 || result.Free_Space < 0)
                {
                    SMSInboxTotalSize = -1;
                    SMSInboxFreeSize = -1;
                    return false;
                }
                else
                {
                    SMSInboxTotalSize = result.Total_Size;
                    SMSInboxFreeSize = result.Free_Space;
                    return true;
                }
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SMSInboxNumber - " + ex.Message;
                SMSInboxTotalSize = -1;
                SMSInboxFreeSize = -1;
                return false;
            }
        }
        /*
         * Important Note:
         * 1. if SMSInboxTotalSize = -1 or SMSInboxFreeSize = -1 => read fail -> result = false
        */
        //End : Inbox Storage
        //SMS Inbox Delete
/*
        public void SmsDelete(int Index, SMS_DELETE_TYPE SmsDeleteType)
        {
            try
            {
                this.ErrorReset();
                this._smsDelete(Index, (int)SmsDeleteType);
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "SMSDelete - " + ex.Message;
            }
        }
        /*
         * Important Note:
         * 1. _smsDelete result always is True => bool output NOT Work
        */
        //End : SMS Inbox Delete
/*
        //SMS Inbox raed
        public bool SmsInboxGet(int SMS_Index, out string SMS_Text, out string SMS_Center, out string SMS_Type, out string Tel_Number,
                                out string Time_stamp, out string S_Discharge, out string TP_PID, out string TP_DCS,
                                out string TP_VP, out string TP_ST, out string S_Reference, out string All_Part, out string Part_NO)
        {
            try
            {
                bool PDU = true;
                string Result = "";
                this.ErrorReset();
                return this._rSMS4(out Result, PDU, SMS_Index, out SMS_Center, out SMS_Type, out Tel_Number, out Time_stamp, out S_Discharge, out SMS_Text,
                                   out TP_PID, out TP_DCS, out TP_VP, out TP_ST, out S_Reference, out All_Part, out Part_NO);
            }
            catch (Exception ex)
            {
                SMS_Text = "";
                this._errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = "GetSMS - " + ex.Message;
                SMS_Center = "";
                SMS_Type = "";
                Tel_Number = "";
                Time_stamp = "";
                S_Discharge = "";
                TP_PID = "";
                TP_DCS = "";
                TP_VP = "";
                TP_ST = "";
                S_Reference = "";
                All_Part = "";
                Part_NO = "";
                return false;
            }
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
/*
    }
}
*/