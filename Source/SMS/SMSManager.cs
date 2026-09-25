using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSM.Core.Source.SMSManager
{
    public partial class SMSManager
    {
        //Add new Data to list
        public void AddData(List<(int ExternalIndex, string PhoneNumber, string SmsText)> smsList)
        {
            if (smsList == null || smsList.Count == 0)
            {
                return;
            }
            lock (this._smsLock)
            {
                foreach (var sms in smsList)
                {
                    this._smsList.Add(new SmsData()
                    {
                        InternalIndex = _nextInternalIndex++,
                        ExternalIndex = sms.ExternalIndex,
                        PhoneNumber = sms.PhoneNumber,
                        SmsText = sms.SmsText,
                        Status = SMS_STATUS.READY,
                        CreateDateTime = DateTime.Now,
                        RetryTime = 0
                    });
                }
            }
        }
        //End : Add new Data to list

        //Execute data for SMS Send
        public bool GetData(ref int internalIndex, ref string phoneNumber, ref string smsText)
        {
            lock (this._smsLock)
            {
                SmsData smsData = this._smsList.Where(x => x.Status == SMS_STATUS.READY || x.Status == SMS_STATUS.SEND_FAIL).OrderBy(x => x.InternalIndex).FirstOrDefault();
                if (smsData == null)
                {
                    return false;
                }
                smsData.Status = SMS_STATUS.SENDING;
                internalIndex = smsData.InternalIndex;
                phoneNumber = smsData.PhoneNumber;
                smsText = smsData.SmsText;
                return true;
            }
        }
        //End : Execute data for SMS Send

        //Update SMS Data status by SMS Send result
        public void UpdateData(int internalIndex, bool sendResult)
        {
            lock (this._smsLock)
            {
                SmsData smsData = this._smsList.FirstOrDefault(x => x.InternalIndex == internalIndex);
                if (smsData == null)
                {
                    return;
                }
                if (sendResult)
                {
                    smsData.Status = SMS_STATUS.SENDED;
                }
                else
                {
                    smsData.Status = SMS_STATUS.SEND_FAIL;
                    smsData.RetryTime++;
                }
            }
        }
        //End : Update SMS Data status by SMS Send result

        //index list of SMS Sended
        public List<int> GetSendedSms()
        {
            lock (this._smsLock)
            {
                List<int> result = _smsList.Where(x => x.Status == SMS_STATUS.SENDED).Select(x => x.ExternalIndex).ToList();
                this._smsList.RemoveAll(x => x.Status == SMS_STATUS.SENDED);
                return result;
            }
        }
        //End : index list of SMS Sended

        //SMS list count
        public int GetCount()
        {
            lock (_smsLock)
            {
                return this._smsList.Count;
            }
        }
        //End : SMS list count
    }
}
