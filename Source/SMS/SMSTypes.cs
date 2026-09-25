using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSM.Core.Source.SMSManager
{
    public partial class SMSManager
    {
        //Constant
        //Enum SMS Status
        private enum SMS_STATUS
        {
            UNKNOWN = 0,
            READY = 1,
            SENDING = 2,
            SENDED = 3,
            SEND_FAIL = 4
        }
        //End : Enum SMS Status
        //End : Constant

        //SMS Data Structure
        private class SmsData
        {
            public int InternalIndex { get; set; }
            public int ExternalIndex { get; set; }
            public string PhoneNumber { get; set; }
            public string SmsText { get; set; }
            public SMS_STATUS Status { get; set; }
            public DateTime CreateDateTime { get; set; }
            public int RetryTime { get; set; }
        }
        //End : SMS Data Structure

        //Variable
        private readonly object _smsLock = new object();
        private readonly List<SmsData> _smsList = new List<SmsData>();
        private int _nextInternalIndex = 1;
        //End : Variable
    }
}
