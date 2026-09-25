using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GSM.Core.Source.DllWrapper
{
    //Enum
    //Status
    public enum STATUS
    {
        INVALID = 0,            //DLL & DLL Functions must load (DLL level)
        READY = 1               //Ready to send and recive (Working level)
    }
    //End : Status
    //ErrorCode
    public enum ERROR_CODE
    {
        NO_ERROR = 0,
        CONSTRUCTOR_FAIL = 1,   //Class construct fail
        DLL_LOAD_FAIL = 2,      //DLL load fail
        DLL_FUNC_LOAD_FAIL = 3, //DLL internal Function load fail
        DLL_FUNC_CALL_ERROR = 4,//DLL internal Function call error
        DLL_FUNC_TIMEOUT = 5,   //call DLL functions error
        UNKNOWN_ERROR = 1000    //UnKnown error 
    }
    //End : ErrorCode
    //Inbox SMS Type
    internal enum INBOX_SMS_TYPE
    {
        SMS_DELIVERED_SUCCEED = 0x00,
        FORWARDED_STATUS_UNKNOWN = 0x01,
        CONGESTION_TRYING = 0x20,
        RECIPIENT_TRYING = 0x21,
        NO_RESPONCE_RECIPIENT_TRYING = 0x22,
        SERVICE_REJECTED_TRYING = 0x23,
        QOS_NOT_AVAILABLE_TRYING = 0x24,
        RECIPIENT_ERROR_TRYING = 0x25,
        RPC_ERROR = 0x40,
        INCOMPATIBLE_DESTINATION = 0x41,
        CONNECTION_REJECTED = 0x42,
        NOT_OBTAINABLE = 0x43,
        QOS_NOT_AVAILABLE = 0x44,
        NO_INTERNETWORKING_AVAILABLE = 0x45,
        MESSAGE_EXPIRED = 0x46,
        MESSAGE_DELETED_BY_SENDER = 0x47,
        MESSAGE_DELETED_BY_SMSC = 0x48,
        NOT_EXIST = 0x49
    }
    //End : Inbox SMS Type
    //End : Enum

    //Internal DLL struct & variable definition
    [StructLayout(LayoutKind.Sequential)]
    internal struct GSM_Message_Struct
    {
        [MarshalAs(UnmanagedType.BStr)] public string Message_Text;
        public int Result;
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct Send_AT
    {
        public int Response_Len;
        [MarshalAs(UnmanagedType.BStr)] public string Result;
    };
    [StructLayout(LayoutKind.Sequential)]
    internal struct SMS_Space_Struct
    {
        public int Free_Space;
        public int Total_Size;
        public bool Result;
    };
    /*
    [StructLayout(LayoutKind.Sequential)]
    private struct OffLine_SMS_Struct
    {
        [MarshalAs(UnmanagedType.BStr)] public string SMS_Center;
        [MarshalAs(UnmanagedType.BStr)] public string SMS_Type;
        [MarshalAs(UnmanagedType.BStr)] public string Tel_Number;
        [MarshalAs(UnmanagedType.BStr)] public string Time_stamp;
        [MarshalAs(UnmanagedType.BStr)] public string S_Discharge;
        [MarshalAs(UnmanagedType.BStr)] public string TP_PID;
        [MarshalAs(UnmanagedType.BStr)] public string TP_DCS;
        [MarshalAs(UnmanagedType.BStr)] public string TP_VP;
        [MarshalAs(UnmanagedType.BStr)] public string TP_ST;
        [MarshalAs(UnmanagedType.BStr)] public string S_Reference;
        public int All_Part;
        public int Part_NO;
        [MarshalAs(UnmanagedType.BStr)] public string SMS_Text;
        public bool Result;
    };
    */
    /*
    [StructLayout(LayoutKind.Sequential)]
    private struct GSM_Dial_Struct
    {
        [MarshalAs(UnmanagedType.BStr)] public string Res_MSG;
        public bool Result;
    };
    */
    //End : Internal DLL struct & variable definition

    public partial class ManshoorGsmDllWrapper : IDisposable
    {

        //Dll Wrapper class -> helper
        //ATCommand function outputs
        private class AtCommandResult
        {
            public int OutputLength;
            public string Output;
        }
        //End : ATCommand function outputs
        //GsmMessageResult function outputs
        private class GsmMessageResult
        {
            public int Index;
            public int ResultLength;
            public string Message;
        }
        //End : GsmMessageResult function outputs
        //SmsNewReceivedResult function outputs
        private class SmsNewReceivedResult
        {
            public bool Result;
            public int SmsIndex;
            public int SmsNumber;
        }
        //End : SmsNewReceivedResult function outputs
        //SmsInboxMessage function outputs
        private class SmsInboxMessageResult
        {
            public bool Result;
            public string SmsText;
            public string SmsCenter;
            public string SmsType;
            public string TelNumber;
            public string TimeStamp;
            public string SDischarge;
            public string TpPid;
            public string TpDcs;
            public string TpVp;
            public string TpSt;
            public string SReference;
            public string AllPart;
            public string PartNo;
        }
        //End : SmsInboxMessage function outputs
        //Dll Wrapper class -> helper

        //Variable for call DLL definition
        private string _dllName = "DllManshoorGSM.dll";
        private IntPtr _handle = IntPtr.Zero;
        //End : Variable for call DLL definition

        //Variable for manage class
        private STATUS _status = STATUS.INVALID;
        private ERROR_CODE _errorCode = ERROR_CODE.UNKNOWN_ERROR;
        private string _errorMessage = string.Empty;
        private int _dllTimeoutMs = 30000;
        //End : Variable for manage class

        //Common Main function
        private SMS_Device_Count_Delegete _smsDeviceCount;
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
    }
}
