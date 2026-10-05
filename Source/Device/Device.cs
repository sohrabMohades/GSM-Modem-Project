using GSM.Core.Source.Common;
using GSM.Core.Source.DllWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GSM.Core.Source.Common.DeviceDefinitions;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        /*
         *************************** MUST UPDATE ***************************
         
        ====================================================================
        Device Status & Error Management Rules
        ====================================================================

        Error Code
        --------------------------------------------------------------------
        Purpose:
            Report what happened.

        Description:
            Error Code only describes the last detected error.

        Examples:
            DLL_LOAD
            DLL_TIMEOUT
            DEVICE_NUMBER_GET
            IMEI
            SIM_STATUS

        Notes:
            - Error Code does NOT define device state.
            - Error Code does NOT define allowed operations.
            - Error Code is used for logging, debugging and error reporting.


        Internal Status
        --------------------------------------------------------------------
        Purpose:
            Define what the Device can do and cannot do.

        Description:
            Internal Status is the main decision flag inside Device.

        Used For:
            - Operation permission checks
            - Workflow decisions
            - State transitions
            - Recovery logic

        Examples:
            INVALID
            DEVICE_NOT_SELECT
            HARDWARE_NOT_READY
            SIM_NOT_READY
            NETWORK_NOT_READY
            READY

        Notes:
            - Some operations may change Internal Status.
            - Internal Status controls which operations are allowed.
            - Device methods must validate Internal Status before execution.


        External Status
        --------------------------------------------------------------------
        Purpose:
            Represent Device state to DeviceManager.

        Description:
            External Status is a simplified view of Internal Status.

        Used For:
            - DeviceManager decisions
            - Scheduling operations
            - Device availability checks

        Examples:
            DISCONNECTED
            NOT_READY
            READY
            BUSY

        Notes:
            - DeviceManager should use External Status only.
            - DeviceManager must not use Internal Status directly.
            - External Status may represent multiple Internal Status values.


        Status Ownership
        --------------------------------------------------------------------
        Device
            Owns:
                Error Code
                Internal Status
                External Status

        DeviceManager
            Uses:
                External Status

        DeviceManager
            Does NOT use:
                Internal Status


        Decision Hierarchy
        --------------------------------------------------------------------
        Error Code => What happened?

        Internal Status => What can Device do now?

        External Status => What DeviceManager can request from Device?

        ====================================================================
        New Rules :
            1. Device Number Change only 1 time and befor Refresh (Device Startup), in DeviceManager only in discovery, Unless Device Must KILL and construct again
            2. Busy check and set only in public method
        */

        //Event definition
        public delegate void DeviceOperationCompletedEventHandler(Device sender, DeviceDefinitions.DEVICE_OPERATION operation, bool result);
        public event DeviceOperationCompletedEventHandler DeviceOperationCompleted;
        private void OnOperationCompleted(DeviceDefinitions.DEVICE_OPERATION operation, bool result)
        {
            this.DeviceOperationCompleted?.Invoke(this, operation, result);
        }        
        //End : Event definition

        //Get
        //Variable
        public STATUS Status
        {
            get
            {
                if (this._busyFlag == 1)
                {
                    return STATUS.BUSY;
                }

                switch (this._internalStatus)
                {
                    case INTERNAL_STATUS.INVALID:
                        return STATUS.DISCONNECTED;

                    case INTERNAL_STATUS.DEVICE_NOT_SELECT:
                    case INTERNAL_STATUS.HARDWARE_NOT_READY:
                    case INTERNAL_STATUS.SIM_NOT_READY:
                    case INTERNAL_STATUS.NETWORK_NOT_READY:
                        return STATUS.NOT_READY;

                    case INTERNAL_STATUS.READY:
                        return STATUS.READY;

                    default:
                        return STATUS.DISCONNECTED;
                }
            }
        }
        public ERROR_CODE ErrorCode
        {
            get => this._errorCode;
        }
        public string ErrorMessage
        {
            get => this._errorMessage;
        }
        public string DllFileName
        {
            get => this._dllFileName;
        }
        public DateTime LastRefreshTime
        {
            get => this._lastRefreshTime;
        }
        //End : Variable
        //identity
        public int DeviceCount
        {
            get => this._deviceCount;
        }
        public int DeviceIndex
        {
            get => this._deviceIndex;
            set
            {
                if (this.DeviceIndexRangeCheck(value))
                { 
                    this._deviceIndex = value; 
                }
            }
        }
        public int DeviceNumber
        {
            get => this._deviceNumber;
        }
        //End : identity
        //property
        public bool GsmPower
        {
            get => this._gsmPower;
        }
        public bool GsmIsConnect
        {
            get => this._gsmIsConnect;
        }
        public bool GsmIsReady
        {
            get => this._gsmIsReady;
        }
        public DeviceDefinitions.GSM_STATUS GsmStatus
        {
            get => this._gsmStatus;
        }
        public int GsmStatusValue
        {
            get => this._gsmStatusValue;
        }
        public string GsmStatusMessage
        {
            get => this._gsmStatusMessage;
        }
        public DeviceDefinitions.SIM_STATUS SimStatus
        {
            get => this._simStatus;
        }
        public string IMEI
        {
            get => this._imei;
        }
        public string SimNumber
        {
            get => this._simNumber;
        }
        public string SimID
        {
            get => this._simID;
        }
        public string SoftwareModel
        {
            get => this._softwareVersion;
        }
        public string DeviceModel
        {
            get => this._deviceInformation;
        }
        public string SMSCenter
        {
            get => this._smsCenter;
        }
        public string NetworkName
        {
            get => this._networkName;
        }
        public string NationalMobileID
        {
            get => this._nationalMobileID;
        }
        public int Signal
        {
            get => this._modemSignal;
        }
        public string GSMDateTime
        {
            get => this._gsmDateTime;
        }
        //End : property
        //Common Operations
        public DeviceDefinitions.SMS_TYPE SMSType
        {
            get => this._smsType;
        }
        public DeviceDefinitions.SMS_CHAR_MODE SMSCharMode
        {
            get => this._smsCharMode;
        }
        //End : Common Operations
        //Send SMS data
        public bool SmsSendDeliveryReport
        {
            get => this._smsSendDeliveryReport;
            set => this._smsSendDeliveryReport = value;
        }
        public int SmsSendSendInterval
        {
            get => this._smsSendSendInterval;
            set
            {
                if (value >= DeviceDefinitions.SMS_SEND_SEND_INTERVAL_MIN && value <= DeviceDefinitions.SMS_SEND_SEND_INTERVAL_MAX)
                {
                    this._smsSendSendInterval = value;
                }
            }
        }
        public int SmsSendRetryTimes
        {
            get => this._smsSendRetryTimes;
            set
            {
                if (value >= DeviceDefinitions.SMS_SEND_RETRY_TIMES_MIN && value <= DeviceDefinitions.SMS_SEND_RETRY_TIMES_MAX)
                {
                    this._smsSendRetryTimes = value;
                }
            }
        }
        public DeviceDefinitions.SMS_VALIDITY SmsSendSmsValidity
        {
            get => this._smsSendSmsValidity;
            set => this._smsSendSmsValidity = value;
        }
        //End : Send SMS data
        //Send Operations
        public int ATCommandDelay
        {
            get => this._atCommandDelay;
        }
        //End : Send Operations
        //Get Operations
        public int InboxTotalSpace
        {
            get => this._inboxTotalSpace;
        }
        public int InboxFreeSpace
        {
            get => this._inboxFreeSpace;
        }
        public int InboxSMSNumber
        {
            get => this._inboxSMSNumber;
        }
        public bool NewInboxAlarm
        {
            get => this._newInboxAlarm;
        }
        //End : Get Operations
        //Check condition
        public int SignalMinValue
        {
            get => this._signalMinAcceptableValue;
            set
            {
                if (value >= DeviceDefinitions.SIGNAL_MIN && value <= DeviceDefinitions.SIGNAL_MAX)
                {
                    this._signalMinAcceptableValue = value;
                }
            }
        }
        //End : Check condition
        //End : Get

        //constructor
        public Device(string DllFileName)
        {
            this.ErrorReset();
            this._dllFileName = DllFileName;
            bool result = true;

            if (result)
            {
                result = this.DllLoad();
            }

            if (result)
            {
                result = this.DllLoadCheck();
            }

            this.ConstructorStatusSet(result);
        }
        //End : constructor

        //Deconstructor
        ~Device()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_dllWrapper != null)
                {
                    _dllWrapper.Dispose();
                    _dllWrapper = null;
                }
            }

            _disposed = true;
        }
        //End : Deconstructor

        //Refresh
        public bool DeviceRefresh()
        {
            return this.DeviceRefresh(true);
        }
        public bool DeviceRefresh(bool PowerRestartAllow)
        {
            bool result = false;
            try
            {
                if (this.TrySetBusy())
                {
                    result = false;
                    this.ErrorSet(ERROR_CODE.BUSY , nameof(this.DeviceRefresh));
                    return result;
                }
                result = this.DeviceRefreshWorkFlow(PowerRestartAllow);
                return result;
            }
            finally
            {
                this.DeviceBusyDeactive(nameof(this.DeviceRefresh) , result);
            }
        }
        private bool DeviceRefreshWorkFlow(bool PowerRestartAllow)
        {
            bool result = true;
            REFRESH_RESULT refreshResult = REFRESH_RESULT.UNKNOWN_ERROR;
            
            refreshResult = this.DeviceRefreshBase(true);
            switch (refreshResult)
            {
                case REFRESH_RESULT.POWER_OFF:
                    {
                        result = this.GSMPowerSetWorkflow(true, true, true);
                        if (result)
                        {
                            refreshResult = this.DeviceRefreshBase(true);
                        }
                        break;
                    }

                case REFRESH_RESULT.RESTART_NEED:
                    {
                        if (PowerRestartAllow)
                        {
                            result = this.DeviceRestart();
                            if (result)
                            {
                                refreshResult = this.DeviceRefreshBase(true);
                            }
                        }
                        break;
                    }

                default:
                    break;
            }

            result = this.DeviceRefreshWorkFlowCheck(refreshResult);
            return result;
        }
        private bool DeviceRefreshWorkFlowCheck(REFRESH_RESULT RefreshResult)
        {
            if (RefreshResult == REFRESH_RESULT.SUCCEED)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private REFRESH_RESULT DeviceRefreshBase(bool MajorErrorStop = true)
        {
            bool result = false, temp = false;
            int tempInt = DeviceDefinitions.NUMBER_INVALID;
            string tempStr = DeviceDefinitions.STRING_INVALID;
            DeviceDefinitions.SIM_STATUS tempSimStatus = DeviceDefinitions.SIM_STATUS.UNKHOWN_ERROR;
            DeviceDefinitions.GSM_STATUS tempGsmStatus = DeviceDefinitions.GSM_STATUS.UNKHOWN;
            REFRESH_RESULT refreshResult = REFRESH_RESULT.UNKNOWN_ERROR;
            this._lastRefreshTime = DateTime.Now;

            //Internal Status Check
            /*if (!this.InternalStatusCheck(INTERNAL_STATUS.DEVICE_NOT_SELECT, nameof(this.DeviceRefreshBase)))
            {
                return REFRESH_RESULT.INTERNAL_STATUS_FAIL;
            }*/
            //End : Internal Status Check

            //Device Selection check
            result = this.DeviceNumberGetWorkflow(ref tempInt, false);
            if (!result)
            {
                //Device Not select , try select by Device Number from internal Device Number
                result = this.DeviceSelectByDeviceNumberWorkflow(this._deviceNumber, true, false);
                if (!result)
                {
                    return REFRESH_RESULT.DEVICE_NOT_SELECT;
                }
            }
            else
            {
                //Device Number Changed -> Invalid Status
                if (tempInt != this._deviceNumber)
                {
                    return REFRESH_RESULT.DEVICE_NUMBER_CHANGED;
                }
            }
            //Device Selection

            //Reset internal value
            this.InternalValueReset();
            //End : Reset internal value

            //Hardware Check
            temp = this.GSMPowerGetWorkflow(true);
            result = temp;
            if (!temp)
            {
                refreshResult = REFRESH_RESULT.POWER_OFF;
                if (MajorErrorStop)
                {
                    return refreshResult;
                }
            }
            temp = this.GSMIsConnectWorkflow(true);
            result = result && temp;
            if (!temp)
            {
                refreshResult = REFRESH_RESULT.RESTART_NEED;
                if (MajorErrorStop)
                {
                    return refreshResult;
                }
            }
            temp = this.GSMIsReadyWorkflow(true);
            result = result && temp;
            //End : Hardware Check

            //Hardware Data Get
            temp = this.IMEIGetWorkflow(ref tempStr, true);
            result = result && temp;
            if (!temp)
            {
                refreshResult = REFRESH_RESULT.RESTART_NEED;
                if (MajorErrorStop)
                {
                    return refreshResult;
                }
            }
            temp = this.SoftwareVersionGetWorkflow(ref tempStr, true);
            result = result && temp;
            temp = this.DeviceInformationGetWorkflow(ref tempStr, true);
            result = result && temp;
            temp = this.GSMStatusGetWorkflow(ref tempGsmStatus, ref tempInt, ref tempStr, true);
            //End : Hardware Data Get

            //SIM Data
            temp = this.SimIDGetWorkflow(ref tempStr, true);
            result = result && temp;
            if (!temp)
            {
                refreshResult = REFRESH_RESULT.RESTART_NEED;
                if (MajorErrorStop)
                {
                    return refreshResult;
                }
            }
            temp = this.NationalMobileIDGetWorkflow(ref tempStr, true);
            result = result && temp;
            temp = this.SimStatusGetWorkflow(ref tempSimStatus, true);
            if (!temp)
            {
                refreshResult = REFRESH_RESULT.SIM_NOT_READY;
                if (MajorErrorStop)
                {
                    return refreshResult;
                }
            }
            //End : SIM Data

            //Network check
            temp = this.SignalGetWorkflow(ref tempInt, true);
            result = result && temp;
            if (!temp)
            {
                refreshResult = REFRESH_RESULT.RESTART_NEED;
                if (MajorErrorStop)
                {
                    return refreshResult;
                }
            }
            //End : Network check

            //Network Data Get
            temp = this.NetworkNameGetWorkflow(ref tempStr, true);
            result = result && temp;
            if (!temp)
            {
                refreshResult = REFRESH_RESULT.RESTART_NEED;
                if (MajorErrorStop)
                {
                    return refreshResult;
                }
            }
            temp = this.SMSCenterGetWorkflow(ref tempStr, true);
            result = result && temp;
            temp = this.GSMDateTimeGetWorkflow(ref tempStr, true);
            //End : Network Data Get

            if (result)
            {
                refreshResult = REFRESH_RESULT.SUCCEED;
            }
            return refreshResult;
        }
        //End : Refresh

        //Helper
        //Constructor
        private bool DllLoad()
        {
            try
            {
                this._dllWrapper = new DllWrapper.ManshoorGsmDllWrapper(this._dllFileName);
                return true;
            }
            catch (Exception ex)
            {
                DllWrapperErrorCheckSetValue($"Device Constructor Error : {ex.Message}");
                return false;
            }
        }
        private bool DllLoadCheck()
        {
            try
            {
                if (this._dllWrapper.ErrorCode == DllWrapper.ERROR_CODE.NO_ERROR)
                {
                    return true;
                }
                else
                {
                    this._errorCode = ERROR_CODE.DLL_WRAPPER;
                    this._errorMessage = "DllWrapper ErrorCode = " + this._dllWrapper.ErrorCode.ToString() + " - ErrorMessage = " + this._dllWrapper.ErrorMessage;
                    return false;
                }
            }
            catch (Exception ex)
            {
                DllWrapperErrorCheckSetValue($"Device Constructor Error : {ex.Message}");
                return false;
            }
        }
        private void ConstructorStatusSet(bool result)
        {
            if (result)
            {
                this._internalStatus = INTERNAL_STATUS.DEVICE_NOT_SELECT;
            }
            else
            {
                this._internalStatus = INTERNAL_STATUS.INVALID;
            }
        }
        //Constructor
        //Common Function
        private bool DeviceRestart()
        {
            bool result = true;
            //Power OFF
            if (result)
            {
                result = this.GSMPowerSetWorkflow(false, true, true);
            }
            //End : Power OFF
            //Power ON
            if (result)
            {
                result = this.GSMPowerSetWorkflow(true, true, true);
            }
            //End : Power ON
            return result;
        }
        private void InternalValueReset()
        {
            this._imei = DeviceDefinitions.STRING_INVALID;
            this._gsmPower = false;
            this._gsmIsConnect = false;
            this._gsmIsReady = false;
            this._simStatus = DeviceDefinitions.SIM_STATUS.UNKHOWN_ERROR;
            this._simNumber = DeviceDefinitions.STRING_INVALID;
            this._simID = DeviceDefinitions.STRING_INVALID;
            this._modemSignal = DeviceDefinitions.NUMBER_INVALID;
            this._softwareVersion = DeviceDefinitions.STRING_INVALID;
            this._deviceInformation = DeviceDefinitions.STRING_INVALID;
            this._gsmDateTime = DeviceDefinitions.STRING_INVALID;
            this._smsCenter = DeviceDefinitions.STRING_INVALID;
            this._networkName = DeviceDefinitions.STRING_INVALID;
            this._nationalMobileID = DeviceDefinitions.STRING_INVALID;
            this._inboxTotalSpace = DeviceDefinitions.NUMBER_INVALID;
            this._inboxFreeSpace = DeviceDefinitions.NUMBER_INVALID;
            this._inboxSMSNumber = DeviceDefinitions.NUMBER_INVALID;
        }
        //End : Common Function
        //State Manager
        //State Manager
        //End : Helper
    }
}
