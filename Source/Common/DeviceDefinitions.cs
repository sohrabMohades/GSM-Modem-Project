using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSM.Core.Source.Common
{
    public static class DeviceDefinitions
    {
        //Enum
        //GSM Status Type
        public enum GSM_STATUS
        {
            ERROR = -1,     //GSM detection Error , result = -1
            READY = 0,      //GSM start and detect SIM card and sim parameters , result = 0
            STARTUP = 2,    //GSM in starting process or some parameters such SIM card NOT detect , result = 2
            UNKHOWN = 1000  //GSM other response
        }
        //End : GSM Status Type
        //SIM Status Type
        public enum SIM_STATUS
        {
            UNKHOWN = 0,
            IDENTITY_ERROR = 1,
            PIN_PUK_ERROR = 2,
            READY = 3,
        }
        //End : SIM Status Type
        //SMS Type
        public enum SMS_TYPE
        {
            UNKHOWN = 0,
            ENGLISH = 1,
            PERSIAN = 2
        }
        //End : SMS Type
        //Char Mode
        public enum SMS_CHAR_MODE
        {
            UNKHOWN = 0,
            GSM = 1,
            HEX = 2,
            IRA = 3,
            PCCP437 = 4,
            _8859_1 = 5,
            UCS2 = 6
        }
        //End : Char Mode
        //SMS Delete Type
        public enum SMS_DELETE_TYPE
        {
            SMS_INDEX = 0,
            SMS_READED = 1,
            SMS_READED_SENDED = 2,
            SMS_READED_SENDED_UNSENDED = 3,
            ALL_SMS = 4
        }
        //End : SMS Delete Type
        //Device Operation
        public enum DEVICE_OPERATION
        {
            UNKNOWN = 0,
            DEVICE_COUNT = 1,
            DEVICE_IDENTITY = 2,
            DEVICE_NUMBER_CHANGE = 3,
            DEVICE_UNSELECT = 4,
            DEVICE_REFRESH = 5,
            SMS_SEND = 6,
            DEVICE_STOP =7
        }
        //End : Device Operation
        //End : Enum

        //Constants
        //Check Value
        public const int DEVICE_COUNT_MIN = 0;
        public const int DEVICE_COUNT_MAX = 255;
        public const int DEVICE_INDEX_MIN = 0;
        public const int DEVICE_INDEX_MAX = 127;
        public const int DEVICE_NUMBER_MIN = 0;
        public const int DEVICE_NUMBER_MAX = 127;
        public const int IMEI_LEN_MIN = 14;
        public const int IMEI_LEN_MAX = 16;
        public const int SIM_ID_LEN_MIN = 18;
        public const int SIM_ID_LEN_MAX = 22;
        public const int NATIONAL_MOBILE_ID_LEN_MIN = 14;
        public const int NATIONAL_MOBILE_ID_LEN_MAX = 15;
        public const int SIGNAL_MIN = 0;
        public const int SIGNAL_MAX = 100;
        public const int AT_COMMAND_DELAY_MIN = 0;
        public const int AT_COMMAND_DELAY_MAX = 60000;
        public const int DATETIME_YEAR_MIN = 0;
        public const int DATETIME_YEAR_MAX = 99;
        public const int DATETIME_MON_MIN = 1;
        public const int DATETIME_MON_MAX = 12;
        public const int DATETIME_DAY_MIN = 1;
        public const int DATETIME_DAY_MAX = 31;
        public const int DATETIME_HOUR_MIN = 0;
        public const int DATETIME_HOUR_MAX = 23;
        public const int DATETIME_MINUTE_MIN = 0;
        public const int DATETIME_MINUTE_MAX = 59;
        public const int DATETIME_SECOND_MIN = 0;
        public const int DATETIME_SECOND_MAX = 59;
        public const int BUFFER_LAST_INDEX_MIN = 1;
        public const int BUFFER_LAST_INDEX_MAX = 20;
        //End : Check Value
        //Initial Values
        public const int NUMBER_INVALID = -1;
        public const string STRING_INVALID = "";
        public const int DEVICE_COUNT_INVALID = -1;
        public const int DEVICE_INDEX_INVALID = -1;
        public const int DEVICE_NUMBER_INVALID = -1;
        public const int AT_COMMAND_DELAY_INVALID = 0;
        public const int AT_COMMAND_DELAY_DEFAULT = 5000;
        //End : Initial Values
        //Constants

        //Send SMS Data group
        public enum SMS_VALIDITY
        {
            _1HOUR = 1,
            _3HOUR = 2,
            _6HOUR = 3,
            _12HOUR = 4,
            _1DAY = 5,
            _1WEEK = 6,
            MAX = 7
        }
        public const bool SMS_SEND_DELIVERY_REPORT_DEFAULT = true;
        public const int SMS_SEND_SEND_INTERVAL_DEFAULT = 1000;
        public const int SMS_SEND_RETRY_TIMES_DEFAULT = 1;
        public const DeviceDefinitions.SMS_VALIDITY SMS_SEND_VALIDITY_DEFAULT = DeviceDefinitions.SMS_VALIDITY.MAX;
        public const int SMS_SEND_REFERENCE_DEFAULT = 0;
        public const string SMS_SEND_COUNTRY_CODE_DEFAULT = "+98";

        public const int SMS_SEND_SEND_INTERVAL_MAX = 10000;
        public const int SMS_SEND_SEND_INTERVAL_MIN = 100;
        public const int SMS_SEND_RETRY_TIMES_MAX = 10;
        public const int SMS_SEND_RETRY_TIMES_MIN = 1;
        //End : Send SMS Data group
        //Send SMS Data group
        public class SmsSendData
        {
            public string PhoneNumber;
            public string SmsText;
            public bool DeliveryReport;
            public int SendInterval;
            public int RetryTimes;
            public DeviceDefinitions.SMS_VALIDITY SmsValidity;
            public int SmsReference;
            public string SmsCenter;
        }
        //End : Send SMS Data group
    }
}
