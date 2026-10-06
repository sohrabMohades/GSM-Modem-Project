using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
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
            INTERNAL_STATUS_FAIL = 1,
            DEVICE_NOT_SELECT = 2,
            DEVICE_NUMBER_CHANGED = 3,
            POWER_OFF = 4,
            RESTART_NEED = 5,
            SIM_NOT_READY = 6,
            SUCCEED = 7
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
            DEVICE_NUM_ERROR = 2
        }
        //End : Try Fail read limitation
        //bool type
        private enum BOOL_TYPE
        {
            UNKNOWN = -1,
            FALSE = 0,
            TRUE = 1
        }
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

            public DateTime _timeOutCounter = DateTime.MinValue;
        }
        //End : class for Device information
        //class Device Runtime Configuration
        private sealed class DeviceRuntimeConfiguration
        {
            public int SignalMinAcceptableValue = 10;
            public bool EnableDoubleReadValidation = false;
            public bool TimeoutCheck = true;
            public int TimeoutMaxMs = 30000;
        }
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
        //End : Variable
    }
}
