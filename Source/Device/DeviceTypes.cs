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
        //Constants
        //Config Values
        private const int DELAY_SHORT_MS = 200;
        private const int DELAY_NORMAL1_MS = 400;
        private const int DELAY_NORMAL2_MS = 600;
        private const int DELAY_LONG_MS = 1000;
        private const int DELAY_AFTER_DEVICE_POWER_ON = 5000;
        private const int DELAY_AFTER_DEVICE_NUM_CHANGE_MS = 10000;

        private const int TRY_READ_IS_CONNECT = 2;
        private const int TRY_READ_FAST = 3;
        private const int TRY_READ_NORMAL = 5;
        private const int TRY_READ_POWER = 10;
        private const int TRY_READ_NETWORK_DATA = 30;

        private const int TRY_FAIL_TIME = 3;
        private const int TRY_FAIL_TIME_DEVICE_NUMBER_GET = 2;
        private const bool TIME_OUT_CHECK = true;
        private const int TIME_OUT_MAX_MS = 30000;
        //End : Config Values
        //Internal Device Status
        private enum INTERNAL_STATUS
        {
            INVALID = 0,            //DllWrapper creation error
            DEVICE_NOT_SELECT = 1,  //Device Select or USB connection error
            HARDWARE_NOT_READY = 2, //Hardware Error
            SIM_NOT_READY = 3,      //Sim Error
            NETWORK_NOT_READY = 4,  //Network Error
            READY = 5               //ready for any operations
        }
        //End : Internal Device Status
        //External Device Status
        public enum STATUS
        {
            DISCONNECTED = 0,       //DEVICE_STATUS = INVALID , Device cannot continue and must be destroyed and recreated (INTERNAL_STATUS = INVALID)
            NOT_READY = 1,          //DEVICE_STATUS = HARDWARE_NOT_READY , SIM_NOT_READY , NETWORK_NOT_READY
            READY = 2,              //DEVICE_STATUS = READY
            BUSY = 3                //_busyFlag = True , Doing any operation wait operation end then internal operation change or NO change DEVICE_STATUS
        }
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
            INBOX_TOTAL_SPACE = 31, //Inbox total space get error
            INBOX_FREE_SPACE = 32,  //Inbox free space get error
            SMS_INBOX_NUMBER = 33,  //Inbox SMS number get error
            INBOX_ALARM_SET = 34,   //Inbox alarm set error
            SEND_SMS_COMMON = 35,   //Send SMS common error
            SEND_AT_COMMON = 36,    //Send AT Command common error
            READ_INBOX_COMMON = 37, //Read Inbox common error
            DEVICE_SELECT_INDEX = 38,//Device Select By Index Error
            DEVICE_SELECT_NUMBER = 39,//Device Select By Device Number Error
            DEVICE_NUMBER = 40,     //Device Number out of range or error
            Execute_TIMEOUT = 41,   //Execute DLL function timeout
            BUSY = 42,              //Device is Busy wait untill current work end
            INTERNAL_STATUS = 43,   //current Status NOT in correct Internal Status
            GSM_STATUS = 44,        //GSM Status fail
            DEVICE_NUMBER_CHANGE_NOT_ALLOW = 45,//Device Number not allow change
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
        //End : Constants

        //Fields
        //Variable
        private INTERNAL_STATUS _internalStatus = INTERNAL_STATUS.INVALID;
        private int _busyFlag = 0;
        private ERROR_CODE _errorCode = ERROR_CODE.NO_ERROR;
        private string _errorMessage = "";
        private readonly string _dllFileName;
        private DllWrapper.ManshoorGsmDllWrapper _dllWrapper;
        private bool _enableDoubleReadValidation = false;
        private DateTime _lastRefreshTime;
        private bool _disposed;
        //End : Variable
        //identity
        private int _deviceCount = DeviceDefinitions.NUMBER_INVALID;
        private int _deviceIndex = DeviceDefinitions.NUMBER_INVALID;
        private int _deviceNumber = DeviceDefinitions.NUMBER_INVALID;
        private bool _deviceNumberChangeAllow = true;
        //End : identity
        //property
        private string _imei = DeviceDefinitions.STRING_INVALID;
        private bool _gsmPower = false;
        private bool _gsmIsConnect = false;
        private bool _gsmIsReady = false;
        private int _gsmStatusValue = (int)DeviceDefinitions.GSM_STATUS.ERROR;
        private DeviceDefinitions.GSM_STATUS _gsmStatus = DeviceDefinitions.GSM_STATUS.ERROR;
        private string _gsmStatusMessage = DeviceDefinitions.STRING_INVALID;
        private DeviceDefinitions.SIM_STATUS _simStatus = DeviceDefinitions.SIM_STATUS.UNKHOWN_ERROR;
        private string _simNumber = DeviceDefinitions.STRING_INVALID;
        private string _simID = DeviceDefinitions.STRING_INVALID;
        private int _modemSignal = DeviceDefinitions.NUMBER_INVALID;
        private string _softwareVersion = DeviceDefinitions.STRING_INVALID;
        private string _deviceInformation = DeviceDefinitions.STRING_INVALID;
        private string _gsmDateTime = DeviceDefinitions.STRING_INVALID;
        private string _smsCenter = DeviceDefinitions.STRING_INVALID;
        private string _networkName = DeviceDefinitions.STRING_INVALID;
        private string _natinalMobileID = DeviceDefinitions.STRING_INVALID;
        //End : property
        //Common Operations
        private DeviceDefinitions.SMS_TYPE _smsType = DeviceDefinitions.SMS_TYPE.Persian;
        private DeviceDefinitions.SMS_CHAR_MODE _smsCharMode = DeviceDefinitions.SMS_CHAR_MODE.GSM;
        //End : Common Operations
        //Send SMS data
        private bool _smsSendDeliveryReport = DeviceDefinitions.SMS_SEND_DELIVERY_REPORT_DEFAULT;
        private int _smsSendSendInterval = DeviceDefinitions.SMS_SEND_SEND_INTERVAL_DEFAULT;
        private int _smsSendRetryTimes = DeviceDefinitions.SMS_SEND_RETRY_TIMES_DEFAULT;
        private DeviceDefinitions.SMS_VALIDITY _smsSendSmsValidity = DeviceDefinitions.SMS_SEND_VALIDITY_DEFAULT;
        private int _smsSendSmsReference = DeviceDefinitions.SMS_SEND_REFERENCE_DEFAULT;
        private string _smsSendCountryCode = DeviceDefinitions.SMS_SEND_COUNTRY_CODE_DEFAULT;
        //Send SMS data
        //Send Operations
        private int _atCommandDelay = 5000;
        //End : Send Operations
        //Get Operations
        private int _inboxTotalSpace = DeviceDefinitions.NUMBER_INVALID;
        private int _inboxFreeSpace = DeviceDefinitions.NUMBER_INVALID;
        private int _inboxSMSNumber = DeviceDefinitions.NUMBER_INVALID;
        private bool _newInboxAlarm = false;
        //End : Get Operations
        //Check condition
        private int _signalMinAcceptableValue = 10;
        private DateTime _timeOutCounter;
        //End : Check condition
        //End : Fields
    }
}
