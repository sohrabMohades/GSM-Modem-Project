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
            NO_ERROR,
            DLL_WRAPPER,            //any undef DllWrapper Layer
            INVALID_INPUT,          //Invalid value for input parameters
            DEVICE_COUNT,           //Device count error
            DEVICE_INDEX,           //Device index invalid
            DEVICE_NUMBER_GET,      //device number get has error
            DEVICE_NUMBER_SET,      //device number set has error
            IMEI,                   //IMEI get has error
            SIM_STATUS,             //SIM status get has error
            SIM_NUMBER,             //SIM Number get has error
            SIM_ID,                 //SIM ID get has error
            GSM_SIGNAL,             //GSM signal get has error
            SOFTWARE_VERSION,       //software version get has error
            HARDWARE_MODEL,         //device information get has error
            GSM_DATE_TIME_GET,      //GSM date time get has error
            GSM_DATE_TIME_SET,      //GSM date time set has error
            NETWORK_NAME_GET,       //Network name get has error
            NETWORK_NAME_SET,       //Network name auto select has error
            SMS_CENTER_GET,         //SMS center get has error
            SMS_CENTER_SET,         //SMS center set has error
            NATIONAL_MOBILE_ID,     //National mobile ID get has error
            SMS_TYPE_GET,           //SMS type get error
            SMS_TYPE_SET,           //SMS type set error
            CHAR_MODE_GET,          //Char mode get error
            CHAR_MODE_SET,          //Char mode set error
            AT_COMMAND_DELAY_SET,   //AT Command delay set error
            INBOX_PROPERTY,         //Inbox total space get error
            INBOX_ALARM_SET,        //Inbox alarm set error
            SEND_SMS_COMMON,        //Send SMS common error
            SEND_SMS_RESULT,        //Send SMS return invalid result string
            SEND_SMS_INPUT,         //Send SMS invalid Input parameters
            SEND_AT_COMMON,         //Send AT Command common error
            READ_INBOX_COMMON,      //Read Inbox common error
            DEVICE_SELECT_INDEX,    //Device Select By Index Error
            DEVICE_SELECT_NUMBER,   //Device Select By Device Number Error
            DEVICE_NUMBER,          //Device Number out of range or error
            EXECUTE_TIMEOUT,        //Execute DLL function timeout
            BUSY,                   //Device is Busy wait untill current work end
            INTERNAL_STATUS,        //current Status NOT in correct Internal Status
            GSM_STATUS,             //GSM Status fail
            DEVICE_NUMBER_CHANGE_NOT_ALLOW,//Device Number not allow change
            BUFFER_LAST_INDEX_GET,  //Buffer last index is invalid
            BUFFER_ANALYZE_PATTERN, //Buffer Analyze pattern
            UNKNOWN_ERROR           //Unknown error 
        }
        //End : Error Codes
        //Device Select Type
        private enum REFRESH_RESULT
        {
            SUCCEED = 0,
            MINOR_ERROR = 1,
            POWER_OFF_AND_RESTART = 2,
            SIM_NOT_READY = 3,
            DEVICE_NOT_SELECT = 4,
            DEVICE_NUMBER_CHANGED = 5,
            UNKNOWN_ERROR = 5,
            UNKNOWN = 5
        }
        //End : Refresh Result
        //Enum Function Result
        internal enum FUNC_RESULT
        {
            UNKNOWN,
            SUCCESS,
            FAIL,
            MAJOR_ERROR
        }
        //End : Enum Function Result
        //Enum Function Type
        internal enum FUNC_TYPE
        {
            LOADER,                 //Device Loader layer functions
            CONTROLLER,             //Device Controller layer functions
            COMPLEX_WORKFLOW,       //Complex workflow layer functions
            WORKFLOW,               //Standard workflow layer functions
            COMPLEX_WORKFLOW_SUB,   //Complex workflow sub layers functions
            WORKFLOW_SUB,           //Standard workflow sub layers functions
            BASE,                   //Base layer functions
            BASE_DLL_CALL,          //Base call Dll functions
            BASE_SUB,               //Base sub layers functions
            BUFFER_UPDATE,          //Buffer Update functions
            BUFFER_ANALYZE,         //Buffer Analyze functions
            BUFFER_CHECK,           //Buffer Check functions
            UNCLASSIFIED            //Unclassified functions
        }
        //End : Enum Function Type
        //Enum Operation execute Delay ms
        private enum OPERATION_DELAY_MS
        {
            NO_DELAY = 0,
            BUFFER_UPDATE_CONTINUE = 0,
            BUFFER_UPDATE = 300,
            SHORT = 200,
            NORMAL_SHORT = 400,
            NORMAL_LONG = 600,
            LONG = 1000,
            AFTER_DEVICE_POWER_ON = 5000,
            AFTER_DEVICE_NUM_CHANGE = 10000,
        }
        //End : Enum Operation execute Delay ms
        //Enum Try Read times
        private enum TRY_COUNT
        {
            IS_CONNECT = 2,
            FAST = 3,
            NORMAL = 5,
            POWER = 10,
            NETWORK_DATA = 30,
            BUFFER_CHECK = 2,
            BUFFER_COUNT_TRY = 3
        }
        //End : Enum Try Read times
        //Try Fail read limitation
        private enum FAIL_LIMIT
        {
            NUM_ERROR = 3,
            DEVICE_NUM_ERROR = 2
        }
        //End : Try Fail read limitation
        private enum BUFFER_TIME_OUT_MS
        {
            MODEM_STARTUP = 10000,
            MODEM_READY = 30000,
            SHORT = 1000,
            NORMAL_SHORT = 2000,
            NORMAL = 4000,
            NORMAL_LONG = 6000,
            LONG = 8000,
            TIMEOUT_MAX_MS = 60000
        }
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
            NULL,                       //string is NULL or white space -> "" or "   "
            UNKNOWN,                    //Response Message not obvious what action is doing
            MODEM_STARTUP,              //Modem is startup -> "MODEM:STARTUP"
            MODEM_READY,                //Modem is ready -> "+PBREADY"
            DEVICE_NUMBER_GET,          //Device Number Get response -> "(0010)OK(DeviceNumber)"
            GSM_CONNECTION,             //GSM Is Connect first response -> "(0012)OK"
            GSM_CONNECTION_ECHO,        //GSM buffer return values to send command -> "ATE0" => echo OFF, "ATE1" => echo ON
            GSM_CONNECTION_AT_TEST,     //Modem AT connection is okey -> "AT"
            POWER_GET,                  //Power status get and Modem Is Ready (same reaction and same response) -> "(000D)OK(0000)" / "(000D)OK(0001)" / "(000D)OK(0002)"
            POWER_SET,                  //Power ON and Power OFF -> "(000A)OK"
            PDU_MODE_SET,               //PDU Mode set -> "AT+CMGF=0" => 0 : UDU mode (Persian support) , "AT+CMGF=1" => 1 : Text Mode (NOT Persian support)
            CHAR_TYPE_SET,              //Char type set -> "AT+CSCS="XXX"" ,XXX => GSM, HEX, IRA, PCCP437, 8859-1, UCS2
            HARDWARE_MODEL,             //Hardware model -> "MS_SMS1402.12.01" => hardwareModel = "1402.12.01"

            GSM_DATE_TIME_GET,          //"AT+CCLK?" find in message -> request command and GSM response : +CCLK: "04/01/01,00:24:49" 
            GSM_SIGNAL_GET,             //"AT+CSQ" find in message -> GSM Signal request, response : +CSQ: 21,1
            GSM_SIGNAL_RESPONSE,        //"+CSQ:" find in message -> GSM Signal request, response : +CSQ: 21,1
            USSD_REQUEST,               //"AT+CUSD=" find in message -> sample : AT+CUSD=1,"*140*11#",15 (USSD request)
            USSD_RESPONSE,              //"+CUSD:" find in message -> sample : +CUSD: 2, "...",72 (1 sample , there is some model respose)
            SMS_SEND_COMMAND,           //"AT+CMGS=" find in message -> sample : AT+CMGS=20
            SMS_SEND_RESPONSE,          //"+CMGS:" find in message -> sample : +CMGS: 144
            SMS_SEND_PROMPT,            //">" find in message -> in SMS Send
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
            SMS_DELIVERY_REPORT,        //"+CDS:"  find in message, +CDS: 25(PDU Len) 069189...(PDU)
        }
        //End : Buffer Action
        //Buffer Action Status
        internal enum BUFFER_ACTION_STATUS
        {
            UNKNOWN,
            SUCCEEDED,
            FAIL,
            OK
        }
        //End : Buffer Action Status 
        //Buffer Update Source
        enum BUFFER_UPDATE_SOURCE
        {
            UNKNOWN = 0,
            WORKFLOW_START = 1,
            WORKFLOW = 2,
            TIMER = 3,
            DATA_UPDATE_ONLY = 4,
        }
        //End : Buffer Update Source
        //Buffer Function (call Buffer Update)
        internal enum BUFFER_UPDATE_TYPE
        {
            DEVICE_NUMBER_GET,
            DEVICE_NUMBER_SET,
            GSM_POWER_GET,
            GSM_POWER_SET,
            GSM_IS_CONNECT,
            GSM_IS_READY,
            IMEI_GET,
            SOFTWARE_VERSION_GET,
            HARDWARE_MODEL_GET,
            SIM_ID_GET,
            NATIONAL_MOBILE_ID_GET,
            SIM_STATUS_GET,
            SIGNAL_GET,
            SMS_CENTER_GET,
            SMS_CENTER_SET,
            NETWORK_NAME_GET,
            NETWORK_NAME_SET,
            GSM_DATE_TIME_GET,
            GSM_DATE_TIME_SET,
            SMS_TYPE_SET,
            CHAR_MODE_SET,
            SMS_SEND,
            USSD_SEND,
            USSD_RESPONSE,
            MODEM_STARTUP,
            MODEM_READY,
            MODEM_STARTUP_READY,
            INBOX_SMS_RECEIVED,
            INBOX_SMS_DELIVERY_REPORT,
            SMS_DELIVERY_REPORT,
            UNKOWN
        }
        //End : Buffer Function (call Buffer Update)
        //End: Buffer
        //End : Constants

        //Group 1 : Status
        /// <summary>
        /// Defines the matching strictness used when analyzing a buffer pattern.
        /// </summary>
        internal enum BUFFER_PATTERN_MODE
        {
            /// <summary>
            /// Requires all expected actions to appear consecutively without any
            /// intervening records.
            /// </summary>
            STRICT,
            /// <summary>
            /// Allows any number of NULL records between consecutive expected actions.
            /// Other actions are not allowed between pattern elements.
            /// </summary>
            NULL_ALLOWED,
            /// <summary>
            /// Allows any number of records, regardless of their action,
            /// between consecutive expected actions.
            /// </summary>
            FLEXIBLE
        }
        /// <summary>
        /// Represents the operational result or modem state inferred
        /// from the modem's response.
        /// </summary>
        internal enum BUFFER_OPERATION_RESULT
        {
            /// <summary>
            /// The operational result has not been determined.
            /// </summary>
            UNKNOWN,
            /// <summary>
            /// The requested operation completed successfully.
            /// </summary>
            SUCCESS,
            /// <summary>
            /// The requested operation failed.
            /// </summary>
            FAIL,
            /// <summary>
            /// The modem is identified as powered off.
            /// </summary>
            POWER_OFF,
            /// <summary>
            /// The modem reports an adapter disconnection or related condition.
            /// </summary>
            ADAPTOR_DC,
            /// <summary>
            /// The modem reports a USB disconnection or related condition.
            /// </summary>
            USB_DC
        }
        /// <summary>
        /// Defines a sequence of expected buffer records and its matching mode.
        /// </summary>
        internal class BufferPattern
        {
            /// <summary>
            /// Gets or sets the matching strictness between pattern items.
            /// </summary>
            public BUFFER_PATTERN_MODE Mode { get; set; }
            /// <summary>
            /// Gets or sets the ordered sequence of expected buffer items.
            /// </summary>
            public List<BufferPatternItem> Items { get; set; }
        }
        /// <summary>
        /// Represents one expected buffer record within a pattern.
        /// </summary>
        internal class BufferPatternItem
        {
            /// <summary>
            /// Gets or sets the expected buffer action.
            /// </summary>
            public BUFFER_ACTION? Action { get; set; }
            /// <summary>
            /// Gets or sets the expected buffer status.
            /// </summary>
            public BUFFER_ACTION_STATUS? Status { get; set; }
        }
        /// <summary>
        /// Represents the final result produced by buffer analysis.
        /// </summary>
        internal class BufferAnalyzeResult
        {
            /// <summary>
            /// Gets or sets the result of pattern recognition.
            /// </summary>
            public FUNC_RESULT Result { get; set; }
            /// <summary>
            /// Gets or sets the operational result or modem state
            /// inferred from the recognized response.
            /// </summary>
            public BUFFER_OPERATION_RESULT OperationResult { get; set; }
            /// <summary>
            /// Gets the collected output values extracted from matched buffer details.
            /// </summary>
            public List<string> BufferResults { get; private set; }
            /// <summary>
            /// Initializes a new instance of the BufferAnalyzeResult class.
            /// </summary>
            public BufferAnalyzeResult()
            {
                this.Result = FUNC_RESULT.UNKNOWN;
                this.OperationResult = BUFFER_OPERATION_RESULT.UNKNOWN;
                this.BufferResults = new List<string>();
            }
        }

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

            public GsmStatus GsmStatus = new GsmStatus();
        }
        private sealed class GsmStatus
        {
            public bool ModemStartup = false;
            public bool ModemReady = false;
            public int ModemATE = DeviceDefinitions.NUMBER_INVALID;
        }
        //End : class for Device information
        //class Device Runtime Configuration
        private sealed class DeviceRuntimeConfiguration
        {
            public bool BufferCheck = true;
            public int SignalMinAcceptableValue = 10;
            public bool EnableDoubleReadValidation = false;
            public bool TimeoutCheck = true;
            public int TimeoutMaxMs = (int)BUFFER_TIME_OUT_MS.TIMEOUT_MAX_MS;
            public bool PowerONDoubleCheck = true;
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
            public string HardwareModel = DeviceDefinitions.STRING_INVALID;
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
            public DeviceDefinitions.SIM_STATUS SIMStatus = DeviceDefinitions.SIM_STATUS.UNKHOWN;
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
            public string BufferMessage = DeviceDefinitions.STRING_INVALID;             //only for singel read or test
            public int BufferMessageLen = DeviceDefinitions.NUMBER_INVALID;             //only for singel read or test
            public DateTime BufferLastUpdate = DateTime.MinValue;
            public DateTime BufferLastCheck = DateTime.MinValue;
            public List<BufferRawData> BufferDataRaw = new List<BufferRawData>();       //RAW Message list, without any Analyze, ONLY REMOVE NULL string from this list
            public List<BufferDataGroup> BufferDataGroup = new List<BufferDataGroup>(); //this list is Analyze of BufferDataRaw list
            public bool NewInboxNotifiction = false;
            public bool ModemStartup = false;
            public bool ModemReady = false;
            public List<string> NewInboxSlot = new List<string>();
        }
        //End : class Device Runtime Buffer
        //Class Buffer Data -> Analyze RAW Message
        private sealed class BufferRawData
        {
            public string Message = DeviceDefinitions.STRING_INVALID;
            public int MessageLen = DeviceDefinitions.NUMBER_INVALID;
            public DateTime ReceivedTime = DateTime.MinValue;
        }
        //End : Class Buffer Data
        private sealed class BufferDataGroup
        {
            public BufferRawData BufferRawData = new BufferRawData();
            public List<BufferData> BufferData = new List<BufferData>();
        }
        /*
         * MessageAnalyzed = all string after pattern detection to end of detection
         * List Detail = all variable in MessageAnalyzed
         * sample :
         * AT+CSQ
         * +CSQ: 15,3
         * OK
         * After Analyze -> MessageAnalyzed = +CSQ: 15,3 / Detail[0] = "15" / Detail[1] = "3"
        */
        private sealed class BufferData
        {
            public BUFFER_ACTION Action = BUFFER_ACTION.UNKNOWN;
            public BUFFER_ACTION_STATUS Status = BUFFER_ACTION_STATUS.UNKNOWN;
            public string Message = DeviceDefinitions.STRING_INVALID;
            public List<string> Detail = new List<string>();
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

        private ErrorTracer _errorTracer;
        private DeviceLogger _logger;

        private static readonly Dictionary<BUFFER_UPDATE_TYPE, int> BufferCheckTimeoutMap = new Dictionary<BUFFER_UPDATE_TYPE, int>
        {
            { BUFFER_UPDATE_TYPE.DEVICE_NUMBER_GET, (int)BUFFER_TIME_OUT_MS.SHORT },
            { BUFFER_UPDATE_TYPE.GSM_POWER_GET, (int)BUFFER_TIME_OUT_MS.SHORT },
            { BUFFER_UPDATE_TYPE.GSM_POWER_SET, (int)BUFFER_TIME_OUT_MS.SHORT },
            { BUFFER_UPDATE_TYPE.MODEM_STARTUP, (int)BUFFER_TIME_OUT_MS.MODEM_STARTUP },
            { BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY, (int)BUFFER_TIME_OUT_MS.MODEM_STARTUP },
            { BUFFER_UPDATE_TYPE.MODEM_READY, (int)BUFFER_TIME_OUT_MS.MODEM_READY }
        };
        //End : Variable
    }
}
