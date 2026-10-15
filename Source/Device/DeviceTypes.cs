using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        //Event definition
        private delegate void DeviceOperationCompletedEventHandler(Device sender, DeviceDefinitions.DEVICE_OPERATION operation, FUNC_RESULT result);
        private event DeviceOperationCompletedEventHandler DeviceOperationCompleted;
        private void OnOperationCompleted(DeviceDefinitions.DEVICE_OPERATION operation, FUNC_RESULT result)
        {
            this.DeviceOperationCompleted?.Invoke(this, operation, result);
        }
        //End : Event definition

        //Internal Device Status
        private enum INTERNAL_STATUS
        {
            INVALID = 0,            // Device cannot operate, must be destroyed and recreated
            BASIC_LEVEL = 1,        // Device is not associated with a valid selected modem. Only discovery and identity operations are allowed.
                                    // Device Number Change is only allowed in this status to prevent Device Number conflicts.
            SINGLE_DEVICE = 2,      // Device is associated with a modem (only Select By Device Number) and can perform refresh and device-level operations.
            WORKING_MODE = 3,       // Device can communicate with SIM and Network. SMS sending is not allowed.
            FULL_MODE = 4           // All operations are allowed, including Main Operation -> SMS Send
        }
        //End : Internal Device Status
        //External Device Status
        /*
        public enum STATUS
        {
            DISCONNECTED = 0,       //DEVICE_STATUS = INVALID , Device cannot continue and must be destroyed and recreated (INTERNAL_STATUS = INVALID)
            NOT_READY = 1,          //DEVICE_STATUS = HARDWARE_NOT_READY , SIM_NOT_READY , NETWORK_NOT_READY
            READY = 2,              //DEVICE_STATUS = READY
            BUSY = 3                //_busyFlag = True , Doing any operation wait operation end then internal operation change or NO change DEVICE_STATUS
        }
        */
        //End : External Device Status
        //Error Codes
        public enum ERROR_CODE
        {
            NO_ERROR = 0,
            DLL_WRAPPER = 3,        //any undef DllWrapper Layer
            INVALID_INPUT = 4,      //Invalid value for input parameters
            DEVICE_COUNT = 5,       //Device count error
            DEVICE_INDEX = 6,       //Device index invalid
            DEVICE_NUMBER_GET = 7,  //device number get has error
            DEVICE_NUMBER_SET = 8,  //device number set has error
            IMEI = 9,               //IMEI get has error
            SIM_STATUS = 13,        //SIM status get has error
            SIM_NUMBER = 14,        //SIM Number get has error
            SIM_ID = 15,            //SIM ID get has error
            GSM_SIGNAL = 16,        //GSM signal get has error
            SOFTWARE_VERSION = 17,  //software version get has error
            DEVICE_INFORMATION = 18,//device information get has error
            GSM_DATE_TIME_GET = 19, //GSM date time get has error
            GSM_DATE_TIME_SET = 20, //GSM date time set has error
            NETWORK_NAME_GET = 21,  //Network name get has error
            NETWORK_NAME_SET = 22,  //Network name auto select has error
            SMS_CENTER_GET = 23,    //SMS center get has error
            SMS_CENTER_SET = 24,    //SMS center set has error
            NATIONAL_MOBILE_ID = 25,//National mobile ID get has error
            SMS_TYPE_GET = 26,      //SMS type get error
            SMS_TYPE_SET = 27,      //SMS type set error
            CHAR_MODE_GET = 28,     //Char mode get error
            CHAR_MODE_SET = 29,     //Char mode set error
            AT_COMMAND_DELAY_SET = 30,//AT Command delay set error
            INBOX_PROPERTY = 31,    //Inbox total space get error
            INBOX_ALARM_SET = 34,   //Inbox alarm set error
            SEND_SMS_COMMON = 35,   //Send SMS common error
            SEND_SMS_RESULT = 36,   //Send SMS return invalid result string
            SEND_SMS_INPUT = 37,    //Send SMS invalid Input parameters
            SEND_AT_COMMON = 38,    //Send AT Command common error
            READ_INBOX_COMMON = 39, //Read Inbox common error
            DEVICE_SELECT_INDEX = 40,//Device Select By Index Error
            DEVICE_SELECT_NUMBER = 41,//Device Select By Device Number Error
            DEVICE_NUMBER = 42,     //Device Number out of range or error
            EXECUTE_TIMEOUT = 43,   //Execute DLL function timeout
            BUSY = 44,              //Device is Busy wait untill current work end
            INTERNAL_STATUS = 45,   //current Status NOT in correct Internal Status
            GSM_STATUS = 46,        //GSM Status fail
            DEVICE_NUMBER_CHANGE_NOT_ALLOW = 47,//Device Number not allow change
            UNKNOWN_ERROR = 1000    //Unknown error 
        }
        //End : Error Codes
        //Device Select Type
        private enum REFRESH_RESULT
        {
            UNKNOWN_ERROR = 0,
            DEVICE_NOT_SELECT = 1,
            DEVICE_NUMBER_CHANGED = 2,
            POWER_OFF = 3,
            RESTART_NEED = 4,
            SIM_NOT_READY = 5,
            SUCCEED = 6
        }
        //End : Refresh Result
        //Enum Function Result
        private enum FUNC_RESULT
        {
            UNKNOWN = 0,
            SUCCESS = 1,
            FAIL = 2,
            MAJOR_ERROR = 3
        }
        //End : Enum Function Result
        //Enum Operation execute Delay ms
        private enum OPERATION_DELAY_MS
        {
            BUFFER_UPDATE = 0,
            SHORT = 200,
            NORMAL_SHORT = 400,
            NORMAL_LONG = 600,
            LONG = 1000,
            AFTER_DEVICE_POWER_ON = 5000,
            AFTER_DEVICE_NUM_CHANGE = 10000
        }
        //End : Enum Operation execute Delay ms
        //Enum Try Read times
        private enum TRY_COUNT
        {
            IS_CONNECT = 2,
            FAST = 3,
            NORMAL = 5,
            POWER = 10,
            NETWORK_DATA = 30
        }
        //End : Enum Try Read times
        //Try Fail read limitation
        private enum FAIL_LIMIT
        {
            NUM_ERROR = 3,
            DEVICE_NUM_ERROR = 2,
        }
        //End : Try Fail read limitation
        //bool type
        private enum BOOL_TYPE
        {
            UNKNOWN = -1,
            FALSE = 0,
            TRUE = 1,
        }
        //End : bool type
        //Buffer
        //Buffer Action
        internal enum BUFFER_ACTION
        {
            NULL,                       //NULL
            UNKNOWN,                    //Response Message not obvious
            MODEM_STATUS,               //"MODEM:STARTUP" find in message , after this wait for +PBREADY
            MODEM_READY,                //"+PBREADY" find in message, befor this MODEM:STARTUP was appeare
            CHAR_MODE_SET,              //"AT+CSCS=" find in message -> GSM, HEX, IRA, PCCP437, 8859-1, UCS2
            PDU_MODE_SET,               //"AT+CMGF=" find in message -> 0 : UDU mode (Persian support) , 1 : Text Mode (NOT Persian support)
            GSM_MODEL,                  //"MS_SMS" find in message -> MS_SMS1402.12.01, Date maybe is another date
            GSM_DATE_TIME_GET,          //"AT+CCLK?" find in message -> request command and GSM response : +CCLK: "04/01/01,00:24:49" 
            GSM_CONNECTION_ATE1,        //"ATE1" find in message -> GSM Is Connect, phase 1 : ATE1 , phase2 (with delay) AT
            GSM_CONNECTION_AT,          //"AT" find in message -> GSM Is Connect, phase 1 : ATE1 , phase2 (with delay) AT
            GSM_SIGNAL,                 //"AT+CSQ" find in message -> GSM Signal request, response : +CSQ: 21,1
            USSD_REQUEST,               //"AT+CUSD=" find in message -> sample : AT+CUSD=1,"*140*11#",15 (USSD request)
            USSD_RESPONSE,              //"+CUSD:" find in message -> sample : +CUSD: 2, "...",72 (1 sample , there is some model respose)
            SMS_SEND_COMMAND,           //"AT+CMGS=" find in message -> sample : AT+CMGS=20
            SYMBOL_ARROW_RIGHT,         //">" find in message -> in SMS Send
            SMS_SEND_RESPONSE,          //"+CMGS:" find in message -> sample : +CMGS: 144
                                        //AT+CMGS=20 (Send start sample) , > , 06918919015000B1000C9189194519210000080B06062A0633 (part of Send data) , +CMGS: 144 (Send response sample)
            SMS_DELETE,                 //"AT+CMGD=" find in message -> sample : AT+CMGD=1,0
                                        //AT+CMGD=1,0 / AT+CMGD=X(Inbox slot),Y(0 to 4 -> Command type) (Delete request) , OK / ERROR (execute response, maybe delay)
            INBOX_READ_COMMAND,         //"AT+CMGR=" find in message -> sample : AT+CMGR=1
            INBOX_READ_RESPONSE,        //"+CMGR:" find in message -> sample : +CMGR: 1,,161
                                        //AT+CMGR=1 (Read command) , +CMGR: 1,,161 (response header sample) , 06918... (response Data)
            SMS_STORAGE_COMMAND,        //"AT+CPMS?" find in message -> sample : AT+CPMS?
            SMS_STORAGE_RESPONSE,       //"+CPMS:" find in message -> +CPMS: "SM",0,50,"SM",0,50,"SM",0,50 (sample response)
                                        //AT+CPMS? (command request) , +CPMS: "SM",0,50,"SM",0,50,"SM",0,50 (sample response)
            SMS_RECEIVED,               //"+SMSFLAG:"  find in message, after that is number of slot such : 1, 2, 3, ..., MAX
        }
        //End : Buffer Action
        //Buffer Action Status
        internal enum BUFFER_ACTION_STATUS
        {
            UNKNOWN = 0,
            SUCCEEDED = 1,
            FAIL = 2
        }
        //End : Buffer Action Status 
        //Buffer Update Source
        enum BUFFER_UPDATE_SOURCE
        {
            WORKFLOW = 1,
            TIMER = 2
        }
        //End : Buffer Update Source
        //End: Buffer
        //End : Constants

        //Group 1 : Status
        private sealed class DeviceRuntimeState
        {
            public int Busy = 0;
            public ERROR_CODE ErrorCode = ERROR_CODE.NO_ERROR;
            public string ErrorMessage = string.Empty;
            public INTERNAL_STATUS InternalStatus = INTERNAL_STATUS.INVALID;
        }
        //End : Group 1 : Status
        //Group 2 : Flags & Counters & information & configuration
        //class for flags
        private sealed class DeviceRuntimeFlags
        {
            public bool NeedRefresh = false;
            public bool NeedSimChange = false;
            public bool LowCredit = false;
            public bool AutoRefreshEnabled = true;
            public bool AutoSmsSendEnabled = true;
            public bool AutoInboxEnabled = true;
        }
        //End : class for flags -> Group 1 : Status
        //class for Device information
        private sealed class DeviceRuntimeInfo
        {
            public int RefreshFailConsecutive = 0;
            public int RefreshFailTotal = 0;
            public int SmsSendFailConsecutive = 0;
            public int SmsSendFailTotal = 0;
            public int SmsSendSuccessTotal = 0;
            public int TimeoutConsecutive = 0;
            public int TimeoutTotal = 0;

            public DateTime LastRefreshTime = DateTime.MinValue;
            public DateTime LastSmsSendTime = DateTime.MinValue;
            public DateTime LastInboxReadTime = DateTime.MinValue;
            public DateTime CreateDateTime = DateTime.MinValue;
            public DateTime LastRecoveryTime = DateTime.MinValue;

            public DeviceDefinitions.DEVICE_OPERATION LastOperation = DeviceDefinitions.DEVICE_OPERATION.UNKNOWN;
            public FUNC_RESULT LastOperationResult = FUNC_RESULT.UNKNOWN;
            public DateTime LastOperationTime = DateTime.MinValue;

            public DateTime TimeOutCounter = DateTime.MinValue;
        }
        //End : class for Device information
        //class Device Runtime Configuration
        private sealed class DeviceRuntimeConfiguration
        {
            public int SignalMinAcceptableValue = 10;
            public bool EnableDoubleReadValidation = false;
            public bool TimeoutCheck = true;
            public int TimeoutMaxMs = 30000;
            public bool PowerON2TimeCheck = true;
        }
        //End : class Device Runtime Configuration
        //End : Group 2 : Flags & Counters & information & configuration

        //Group 3 : Device Properties
        //identity
        private sealed class DeviceRuntimePropertiesIdentity
        {
            public int DeviceCount = DeviceDefinitions.NUMBER_INVALID;
            public int DeviceIndex = DeviceDefinitions.NUMBER_INVALID;
            public int DeviceNumber = DeviceDefinitions.NUMBER_INVALID;
        }
        //End : identity
        //Hardware
        private sealed class DeviceRuntimePropertiesHardware
        {
            public string IMEI = DeviceDefinitions.STRING_INVALID;
            public string SoftwareVersion = DeviceDefinitions.STRING_INVALID;
            public string DeviceInformation = DeviceDefinitions.STRING_INVALID;
            public BOOL_TYPE GsmPower = BOOL_TYPE.UNKNOWN;
            public BOOL_TYPE GsmIsConnect = BOOL_TYPE.UNKNOWN;
            public BOOL_TYPE GsmIsReady = BOOL_TYPE.UNKNOWN;
            public int GsmStatusValue = (int)DeviceDefinitions.GSM_STATUS.ERROR;
            public DeviceDefinitions.GSM_STATUS GsmStatus = DeviceDefinitions.GSM_STATUS.ERROR;
            public string GsmStatusMessage = DeviceDefinitions.STRING_INVALID;
        }
        //End :Hardware
        //SIM
        private sealed class DeviceRuntimePropertiesSIM
        {
            public DeviceDefinitions.SIM_STATUS SIMStatus = DeviceDefinitions.SIM_STATUS.UNKHOWN_ERROR;
            public string SIMID = DeviceDefinitions.STRING_INVALID;
            public string NationalMobileID = DeviceDefinitions.STRING_INVALID;
        }
        //End : SIM
        //Network
        private sealed class DeviceRuntimePropertiesNetwork
        {
            public int ModemSignal = DeviceDefinitions.NUMBER_INVALID;
            public string NetworkName = DeviceDefinitions.STRING_INVALID;
            public string SMSCenter = DeviceDefinitions.STRING_INVALID;
        }
        //End : Network
        //Operator
        private sealed class DeviceRuntimePropertiesOperator
        {
            public string MobileNumber = DeviceDefinitions.STRING_INVALID;
            public decimal RemainBalance = DeviceDefinitions.NUMBER_INVALID;
        }
        //End : Operator
        //Ungrouped Properties
        private sealed class DeviceRuntimePropertiesUngrouped
        {
            public string GsmDateTime = DeviceDefinitions.STRING_INVALID;
        }
        //End : Ungrouped Properties
        //End : Group 3 : Device Properties

        //Group 4 : Device Working
        //Working Configuration
        private sealed class DeviceRuntimeWorkingConfiguration
        {
            public int AtCommandDelay = DeviceDefinitions.AT_COMMAND_DELAY_INVALID;
            public DeviceDefinitions.SMS_TYPE SmsType = DeviceDefinitions.SMS_TYPE.UNKHOWN;
            public DeviceDefinitions.SMS_CHAR_MODE SmsCharMode = DeviceDefinitions.SMS_CHAR_MODE.UNKHOWN;
        }
        //End : Working Configuration
        //SMS Send Configuration
        private sealed class DeviceRuntimeSmsSendConfiguration
        {
            public bool DeliveryReport = DeviceDefinitions.SMS_SEND_DELIVERY_REPORT_DEFAULT;
            public int SendInterval = DeviceDefinitions.SMS_SEND_SEND_INTERVAL_DEFAULT;
            public int RetryTimes = DeviceDefinitions.SMS_SEND_RETRY_TIMES_DEFAULT;
            public DeviceDefinitions.SMS_VALIDITY SmsValidity = DeviceDefinitions.SMS_SEND_VALIDITY_DEFAULT;
            public int SmsReference = DeviceDefinitions.SMS_SEND_REFERENCE_DEFAULT;
            public string CountryCode = DeviceDefinitions.SMS_SEND_COUNTRY_CODE_DEFAULT;
        }
        //End : SMS Send Configuration
        //Inbox Property
        private sealed class DeviceRuntimePropertiesInbox
        {
            public int TotalSpace = DeviceDefinitions.NUMBER_INVALID;
            public int SMSNumber = DeviceDefinitions.NUMBER_INVALID;
        }
        //End : Inbox Property
        //Inbox Configuration
        private sealed class DeviceRuntimeInboxConfiguration
        {
            public bool NewInboxAlarm = false;
        }
        //End : Inbox Configuration
        //class Device Runtime Buffer
        private sealed class DeviceRuntimeBuffer
        {
            public bool BufferAutoUpdate = true;
            public bool BufferNewData = false;
            public int BufferLastIndex = DeviceDefinitions.NUMBER_INVALID;
            public string BufferMessage = DeviceDefinitions.STRING_INVALID;     //only for singel read or test
            public int BufferMessageLen = DeviceDefinitions.NUMBER_INVALID;     //only for singel read or test
            public DateTime BufferLastUpdate = DateTime.MinValue;
            public List<string> BufferDataRaw = new List<string>();             //RAW Message list, without any Analyze, ONLY REMOVE NULL string from this list
            public List<BufferDataGroup> BufferDataFinal = new List<BufferDataGroup>();   //this list is Analyze of BufferDataRaw list
            public bool NewInboxNotifiction = false;
            public bool ModemStartup = false;
            public bool ModemReady = false;
            public List<string> NewInboxSlot = new List<string>();
        }
        //End : class Device Runtime Buffer
        //Class Buffer Data -> Analyze RAW Message
        internal sealed class BufferDataGroup
        {
            public string SourceMessage = DeviceDefinitions.STRING_INVALID;
            public List<BufferData> BufferData = new List<BufferData>();
        }
        internal sealed class BufferData
        {
            public BUFFER_ACTION Action = BUFFER_ACTION.UNKNOWN;
            public BUFFER_ACTION_STATUS Status = BUFFER_ACTION_STATUS.UNKNOWN;
            public string Message = DeviceDefinitions.STRING_INVALID;
            public string MessageAnalyzed = DeviceDefinitions.STRING_INVALID;
            public string Detail = DeviceDefinitions.STRING_INVALID;
            public DateTime ReceivedTime = DateTime.MinValue;
        }
        //End : Class Buffer Data -> Analyze RAW Message
        //End : Group 4 : Device Working

        //Fields
        //Variable
        private readonly string _dllFileName;
        private DllWrapper.ManshoorGsmDllWrapper _dllWrapper;
        private bool _disposed;

        private DeviceRuntimeState _state = new DeviceRuntimeState(); 
        private DeviceRuntimeFlags _runtimeFlags = new DeviceRuntimeFlags();
        private DeviceRuntimeInfo _runtimeInfo = new DeviceRuntimeInfo();
        private DeviceRuntimeConfiguration _runtimeConfig = new DeviceRuntimeConfiguration();

        private DeviceRuntimePropertiesIdentity _runtimeIdentity = new DeviceRuntimePropertiesIdentity();
        private DeviceRuntimePropertiesHardware _runtimeHardware = new DeviceRuntimePropertiesHardware();
        private DeviceRuntimePropertiesSIM _runtimeSim = new DeviceRuntimePropertiesSIM();
        private DeviceRuntimePropertiesNetwork _runtimeNetwork = new DeviceRuntimePropertiesNetwork();
        private DeviceRuntimePropertiesOperator _runtimeOperator = new DeviceRuntimePropertiesOperator();
        private DeviceRuntimePropertiesUngrouped _runtimeUngroup = new DeviceRuntimePropertiesUngrouped();
        private DeviceRuntimeWorkingConfiguration _runtimeWorkingConfig = new DeviceRuntimeWorkingConfiguration();
        private DeviceRuntimeSmsSendConfiguration _runtimeSmsSendConfig = new DeviceRuntimeSmsSendConfiguration();
        private DeviceRuntimePropertiesInbox _runtimeInboxProperty = new DeviceRuntimePropertiesInbox();
        private DeviceRuntimeInboxConfiguration _runtimeInboxConfig = new DeviceRuntimeInboxConfiguration();
        private DeviceRuntimeBuffer _runtimeBuffer = new DeviceRuntimeBuffer();
        //End : Variable
    }
}
