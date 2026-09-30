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
    /// <summary>
    /// **Device کلاس - مدیریت یک مودم GSM واحد**
    /// 
    /// **English Description:**
    /// The Device class represents and manages a single GSM modem instance with comprehensive 
    /// control over its operations, state management, and communication capabilities.
    /// 
    /// **Description (فارسی):**
    /// کلاس Device نمایندگی و مدیریت یک نمونه تک مودم GSM است که دارای کنترل جامع بر 
    /// روی عملیات‌های آن، مدیریت وضعیت و قابلیت‌های ارتباطی است.
    /// 
    /// **Key Responsibilities (مسئولیت‌های اصلی):**
    /// 1. **State Management**: Tracks internal and external device states (Device Status, Error Code)
    /// 2. **Hardware Control**: Manages power, connection, and readiness states
    /// 3. **Device Identity**: Handles device selection, indexing, and numbering
    /// 4. **SIM Management**: Monitors SIM status, identity, and mobile network information
    /// 5. **Network Monitoring**: Tracks signal strength, network availability, and connectivity
    /// 6. **Operation Execution**: Coordinates device operations with busy flag management
    /// 7. **Error Handling**: Centralizes error tracking and reporting
    /// 
    /// **Architecture Notes:**
    /// - Uses a three-level status system: ErrorCode, InternalStatus, ExternalStatus
    /// - Implements thread-safe busy flag using Interlocked operations
    /// - Separates device operations into private WorkFlow and Base method layers
    /// - Maintains atomic operations to prevent concurrent state modifications
    /// - Uses event-based completion notifications for public operations
    /// 
    /// **Status Hierarchy:**
    /// InternalStatus (device capability) → ExternalStatus (manager interface) → ErrorCode (diagnostics)
    /// 
    /// **Important Rules:**
    /// • Device Number is immutable after first Refresh operation
    /// • Busy flag prevents concurrent operations on a single device
    /// • Each operation must properly set and clear the busy flag
    /// • Internal Status controls which operations are permitted
    /// • External Status is the only interface visible to DeviceManager
    /// </summary>
    public partial class Device : IDisposable
    {
        /*
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
        /// <summary>
        /// Delegate for device operation completion notifications.
        /// </summary>
        /// <param name="sender">The Device instance that completed the operation</param>
        /// <param name="operation">The type of operation that was completed</param>
        /// <param name="result">True if operation succeeded, False if it failed</param>
        public delegate void DeviceOperationCompletedEventHandler(Device sender, DeviceDefinitions.DEVICE_OPERATION operation, bool result);

        /// <summary>
        /// Event raised when a device operation completes (both success and failure).
        /// </summary>
        public event DeviceOperationCompletedEventHandler DeviceOperationCompleted;
        private void OnOperationCompleted(DeviceDefinitions.DEVICE_OPERATION operation, bool result)
        {
            this.DeviceOperationCompleted?.Invoke(this, operation, result);
        }        
        //End : Event definition

        //Get
        //Variable
        /// <summary>
        /// Gets the current external status of the device visible to DeviceManager.
        /// This status represents a simplified view of the internal state.
        /// </summary>
        /// <value>
        /// BUSY: Device is currently executing an operation
        /// DISCONNECTED: Device is not connected or in an invalid state (ErrorCode = DLL_WRAPPER or InternalStatus = INVALID)
        /// NOT_READY: Device is connected but not ready (missing hardware, SIM, or network)
        /// READY: Device is fully operational and ready for operations
        /// </value>
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
        /// <summary>
        /// Gets the error code representing the last occurred error or operation result.
        /// This is used for logging, debugging, and error reporting.
        /// </summary>
        /// <value>The last error code; NO_ERROR if no error has occurred</value>
        public ERROR_CODE ErrorCode
        {
            get => this._errorCode;
        }
        /// <summary>
        /// Gets the descriptive error message associated with the last error code.
        /// پیام توصیفی خطای آخر
        /// </summary>
        /// <value>Error message string; empty string if no error occurred</value>
        public string ErrorMessage
        {
            get => this._errorMessage;
        }
        /// <summary>
        /// Gets the filename of the DLL wrapper currently being used by this device instance.
        /// نام فایل DLL wrapper مورد استفاده
        /// </summary>
        /// <value>The DLL filename path</value>
        public string DllFileName
        {
            get => this._dllFileName;
        }
        /// <summary>
        /// Gets the timestamp of the last successful device refresh operation.
        /// زمان آخرین عملیات refresh موفق
        /// </summary>
        /// <value>DateTime of the last refresh operation; default time if never refreshed</value>
        public DateTime LastRefreshTime
        {
            get => this._lastRefreshTime;
        }
        //End : Variable
        //identity
        /// <summary>
        /// Gets the total number of available GSM modems in the current system.
        /// تعداد کل مودم‌های GSM دستیاب در سیستم
        /// </summary>
        /// <value>Number of modems available (0-255)</value>
        public int DeviceCount
        {
            get => this._deviceCount;
        }
        /// <summary>
        /// Gets or sets the index of this device among all available devices.
        /// Index must be within valid range (0-127).
        /// شاخص این device در میان تمام devices
        /// </summary>
        /// <value>The device index (0-127). Value is only set if range check passes.</value>
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
        /// <summary>
        /// Gets the unique device number assigned to this modem.
        /// Device number is IMMUTABLE after the first Refresh() operation.
        /// شماره منحصر بفرد device - تغییرناپذیر بعد از Refresh
        /// </summary>
        /// <value>The device number (0-127); -1 if not assigned</value>
        public int DeviceNumber
        {
            get => this._deviceNumber;
        }
        //End : identity
        //property
        /// <summary>
        /// Gets the power state of the GSM modem.
        /// وضعیت قدرت (power) مودم GSM
        /// </summary>
        /// <value>true if power is ON; false if OFF</value>
        public bool GsmPower
        {
            get => this._gsmPower;
        }
        /// <summary>
        /// Gets the connection state of the GSM modem to the hardware.
        /// وضعیت اتصال مودم GSM به سخت‌افزار
        /// </summary>
        /// <value>true if connected; false otherwise</value>
        public bool GsmIsConnect
        {
            get => this._gsmIsConnect;
        }
        /// <summary>
        /// Gets the readiness state of the GSM modem for operations.
        /// وضعیت آمادگی مودم GSM برای عملیات
        /// </summary>
        /// <value>true if ready; false otherwise</value>
        public bool GsmIsReady
        {
            get => this._gsmIsReady;
        }
        /// <summary>
        /// Gets the current GSM modem status.
        /// وضعیت فعلی مودم GSM
        /// </summary>
        /// <value>GSM_STATUS enum: ERROR, READY, STARTUP, or UNKNOWN</value>
        public DeviceDefinitions.GSM_STATUS GsmStatus
        {
            get => this._gsmStatus;
        }
        /// <summary>
        /// Gets the numeric value representation of the GSM status.
        /// مقدار عددی نمایندگی وضعیت GSM
        /// </summary>
        /// <value>Integer status code</value>
        public int GsmStatusValue
        {
            get => this._gsmStatusValue;
        }
        /// <summary>
        /// Gets the descriptive message for the current GSM status.
        /// پیام توصیفی وضعیت GSM
        /// </summary>
        /// <value>Status description string</value>
        public string GsmStatusMessage
        {
            get => this._gsmStatusMessage;
        }
        /// <summary>
        /// Gets the current SIM card status.
        /// وضعیت کارت SIM
        /// </summary>
        /// <value>SIM_STATUS enum: IDENTITY_ERROR, PIN_PUK_ERROR, READY, or UNKNOWN_ERROR</value>
        public DeviceDefinitions.SIM_STATUS SimStatus
        {
            get => this._simStatus;
        }
        /// <summary>
        /// Gets the International Mobile Equipment Identity (IMEI) of the modem.
        /// شناسه بین‌المللی تجهیزات موبایل (IMEI)
        /// </summary>
        /// <value>15-16 digit IMEI string; empty if not available</value>
        public string IMEI
        {
            get => this._imei;
        }
        /// <summary>
        /// Gets the SIM card phone number (MSI SDN).
        /// شماره تلفن کارت SIM
        /// </summary>
        /// <value>Phone number string; empty if not available</value>
        public string SimNumber
        {
            get => this._simNumber;
        }
        /// <summary>
        /// Gets the SIM card Identity (ICCID).
        /// شناسه کارت SIM
        /// </summary>
        /// <value>18-22 digit ICCID string; empty if not available</value>
        public string SimID
        {
            get => this._simID;
        }
        /// <summary>
        /// Gets the firmware/software version of the GSM modem.
        /// نسخه نرم‌افزار مودم GSM
        /// </summary>
        /// <value>Version string</value>
        public string SoftwareModel
        {
            get => this._softwareVersion;
        }
        /// <summary>
        /// Gets the hardware model information of the GSM modem.
        /// اطلاعات مدل سخت‌افزار مودم GSM
        /// </summary>
        /// <value>Device model information string</value>
        public string DeviceModel
        {
            get => this._deviceInformation;
        }
        /// <summary>
        /// Gets the SMS Service Center (SMSC) number configured on the SIM.
        /// شماره مرکز سرویس پیام کوتاه (SMSC)
        /// </summary>
        /// <value>SMSC number string</value>
        public string SMSCenter
        {
            get => this._smsCenter;
        }
        /// <summary>
        /// Gets the name of the current mobile network operator.
        /// نام اپراتور شبکه موبایل فعلی
        /// </summary>
        /// <value>Network operator name string</value>
        public string NetworkName
        {
            get => this._networkName;
        }
        /// <summary>
        /// Gets the National Mobile Identity related to the SIM/device.
        /// شناسه ملی موبایل
        /// </summary>
        /// <value>14-15 digit national ID string; empty if not available</value>
        public string NationalMobileID
        {
            get => this._nationalMobileID;
        }
        /// <summary>
        /// Gets the current signal strength from the network.
        /// قدرت سیگنال فعلی از شبکه
        /// </summary>
        /// <value>Signal strength 0-100 (percentage); -1 if not available</value>
        public int Signal
        {
            get => this._modemSignal;
        }
        /// <summary>
        /// Gets the date and time synchronized from the GSM network.
        /// تاریخ و زمان هماهنگ‌شده از شبکه GSM
        /// </summary>
        /// <value>DateTime string in device format</value>
        public string GSMDateTime
        {
            get => this._gsmDateTime;
        }
        //End : property
        //Common Operations
        /// <summary>
        /// Gets the SMS character encoding type currently configured.
        /// نوع کدگذاری کاراکتر پیام
        /// </summary>
        /// <value>SMS_TYPE: ENGLISH or PERSIAN</value>
        public DeviceDefinitions.SMS_TYPE SMSType
        {
            get => this._smsType;
        }
        /// <summary>
        /// Gets the SMS character mode/encoding currently configured.
        /// حالت کدگذاری کاراکتر پیام
        /// </summary>
        /// <value>SMS_CHAR_MODE: GSM, HEX, IRA, PCCP437, 8859_1, or UCS2</value>
        public DeviceDefinitions.SMS_CHAR_MODE SMSCharMode
        {
            get => this._smsCharMode;
        }
        //End : Common Operations
        //Send SMS data
        /// <summary>
        /// Gets or sets whether SMS delivery reports are requested for sent messages.
        /// آیا گزارش تحویل پیام درخواست می‌شود
        /// </summary>
        /// <value>true to request delivery report; false otherwise (default: true)</value>
        public bool SmsSendDeliveryReport
        {
            get => this._smsSendDeliveryReport;
            set => this._smsSendDeliveryReport = value;
        }
        /// <summary>
        /// Gets or sets the delay interval between SMS send retry attempts (in milliseconds).
        /// فاصله زمانی بین تلاش‌های مجدد ارسال پیام
        /// </summary>
        /// <value>Interval in milliseconds (100-10000; default: 1000)</value>
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
        /// <summary>
        /// Gets or sets the number of retry attempts for failed SMS sends.
        /// تعداد تلاش‌های مجدد برای پیام‌های ناموفق
        /// </summary>
        /// <value>Retry count (1-10; default: 1)</value>
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
        /// <summary>
        /// Gets or sets the validity period for SMS messages (how long they're stored on SMSC).
        /// مدت زمان اعتبار پیام (مدت زمان نگهداری در SMSC)
        /// </summary>
        /// <value>SMS_VALIDITY enum: 1HOUR, 3HOUR, 6HOUR, 12HOUR, 1DAY, 1WEEK, or MAX</value>
        public DeviceDefinitions.SMS_VALIDITY SmsSendSmsValidity
        {
            get => this._smsSendSmsValidity;
            set => this._smsSendSmsValidity = value;
        }
        //End : Send SMS data
        //Send Operations
        /// <summary>
        /// Gets the delay configured between AT commands sent to the modem (in milliseconds).
        /// تأخیر تنظیم‌شده بین دستورات AT ارسال‌شده به مودم
        /// </summary>
        /// <value>Command delay in milliseconds</value>
        public int ATCommandDelay
        {
            get => this._atCommandDelay;
        }
        //End : Send Operations
        //Get Operations
        /// <summary>
        /// Gets the total storage space capacity for SMS inbox on the SIM card.
        /// کل ظرفیت ذخیره‌سازی برای صندوق ورودی پیام‌ها
        /// </summary>
        /// <value>Total SMS storage slots; -1 if not available</value>
        public int InboxTotalSpace
        {
            get => this._inboxTotalSpace;
        }
        /// <summary>
        /// Gets the remaining free space in the SMS inbox.
        /// فضای آزاد باقی‌مانده در صندوق ورودی
        /// </summary>
        /// <value>Free SMS storage slots; -1 if not available</value>
        public int InboxFreeSpace
        {
            get => this._inboxFreeSpace;
        }
        /// <summary>
        /// Gets the current number of SMS messages stored in the inbox.
        /// تعداد پیام‌هایی که در صندوق ورودی ذخیره شده
        /// </summary>
        /// <value>SMS message count; -1 if not available</value>
        public int InboxSMSNumber
        {
            get => this._inboxSMSNumber;
        }
        /// <summary>
        /// Gets whether a new SMS inbox alarm/notification is pending.
        /// آیا یک هشدار پیام جدید منتظر است
        /// </summary>
        /// <value>true if new messages notification pending; false otherwise</value>
        public bool NewInboxAlarm
        {
            get => this._newInboxAlarm;
        }
        //End : Get Operations
        //Check condition
        /// <summary>
        /// Gets or sets the minimum acceptable signal strength threshold (0-100).
        /// حداقل قدرت سیگنال قابل قبول
        /// </summary>
        /// <value>Signal threshold 0-100 (default: system dependent)</value>
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
        /// <summary>
        /// Initializes a new Device instance and loads the GSM DLL wrapper.
        /// 
        /// یک نمونه جدید Device را مقدار‌دهی اولیه کرده و DLL wrapper GSM را بارگذاری می‌کند.
        /// </summary>
        /// <param name="DllFileName">
        /// Full path to the GSM modem DLL wrapper file.
        /// مسیر کامل فایل DLL wrapper مودم GSM
        /// Example: "C:\\Program Files\\ManshoorGsm\\Manshoor.dll"
        /// </param>
        /// <remarks>
        /// Initialization flow:
        /// 1. Reset error tracking
        /// 2. Load DLL wrapper from specified path
        /// 3. Verify DLL loaded successfully
        /// 4. Set InternalStatus based on result:
        ///    - Success: DEVICE_NOT_SELECT (ready for discovery)
        ///    - Failure: INVALID (DLL error occurred)
        ///
        /// ⚠️ اگر DLL لود نشود:
        /// - ErrorCode will be set to DLL_WRAPPER
        /// - Status will be DISCONNECTED
        /// - Cannot proceed with any operations
        /// </remarks>
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
        /// <summary>
        /// Finalizer - ensures resources are cleaned up.
        /// تضمین می‌کند که منابع تمیز می‌شوند
        /// </summary>
        ~Device()
        {
            Dispose(false);
        }
        /// <summary>
        /// Releases all resources and disconnects from the DLL wrapper.
        /// تمام منابع را آزاد می‌کند و اتصال به DLL wrapper را قطع می‌کند.
        /// </summary>
        /// <remarks>
        /// ✅ Best Practice: Use 'using' statement:
        /// <code>
        /// using (var device = new Device(dllPath))
        /// {
        ///     device.DeviceRefresh();
        ///     device.SmsSend(number, text);
        /// } // Dispose() called automatically
        /// </code>
        /// 
        /// Or call manually:
        /// <code>
        /// var device = new Device(dllPath);
        /// try { /* use device */ }
        /// finally { device.Dispose(); }
        /// </code>
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Protected dispose pattern implementation.
        /// الگوی dispose محافظت‌شده را پیاده‌سازی می‌کند.
        /// </summary>
        /// <param name="disposing">true if called from Dispose(); false if from finalizer</param>
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
        /// <summary>
        /// Refreshes all device information by querying hardware status.
        /// Calls DeviceRefresh(true) - allows power restart if needed.
        /// 
        /// تمام اطلاعات device را refresh می‌کند با اجازه restart در صورت نیاز.
        /// </summary>
        /// <returns>true if refresh succeeded; false otherwise</returns>
        /// <remarks>
        /// This is a convenience overload. For more control, use:
        /// <code>
        /// device.DeviceRefresh(powerRestartAllow: false);
        /// </code>
        /// </remarks>
        public bool DeviceRefresh()
        {
            return this.DeviceRefresh(true);
        }
        /// <summary>
        /// Refreshes all device information with optional power restart capability.
        /// 
        /// تمام اطلاعات device را refresh می‌کند با امکان کنترل power restart.
        /// </summary>
        /// <param name="PowerRestartAllow">
        /// true: Allow device power restart if needed for recovery
        /// false: Skip power restart, only perform status queries
        /// Default: true
        /// </param>
        /// <returns>true if successful; false if failed or device is already BUSY</returns>
        /// <remarks>
        /// ⚠️ مهم / IMPORTANT:
        /// - Sets device Status to BUSY during execution
        /// - Raises DeviceOperationCompleted event upon completion
        /// - If Status == BUSY: returns false with ErrorCode.BUSY
        /// - Updates all status properties (IMEI, Signal, SimStatus, etc.)
        /// - May take several seconds to complete
        /// 
        /// The refresh workflow:
        /// 1. Verify device is still selected
        /// 2. Check hardware (Power, Connection, Readiness)
        /// 3. Get device info (IMEI, Software Version, Model)
        /// 4. Check SIM (ID, Status, Mobile ID)
        /// 5. Check network (Signal, Operator, SMS Center)
        /// 6. Update InternalStatus based on results
        /// 7. Return overall success/failure
        /// </remarks>
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
            this.UpdateInternalStatus(refreshResult);
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
        private void UpdateInternalStatus(string funcName, bool result)
        {
            if (this.ErrorCode == ERROR_CODE.DLL_WRAPPER)
            {
                this._internalStatus = INTERNAL_STATUS.INVALID;
                return;
            }
        }
        private void UpdateInternalStatus(REFRESH_RESULT refreshResult)
        {
            if (this._dllWrapper.ErrorCode != DllWrapper.ERROR_CODE.NO_ERROR)
            {
                this._internalStatus = INTERNAL_STATUS.INVALID;
                return;
            }
            if (refreshResult == REFRESH_RESULT.DEVICE_NUMBER_CHANGED)
            {
                this._internalStatus = INTERNAL_STATUS.INVALID;
                return;
            }
            this.UpdateInternalStatusCheckVariable();
        }
        private void UpdateInternalStatusCheckVariable()
        {
            if(this._deviceNumber == DeviceDefinitions.DEVICE_NUMBER_INVALID)
            {
                this._internalStatus = INTERNAL_STATUS.DEVICE_NOT_SELECT;
                return;
            }
            if (this._imei == DeviceDefinitions.STRING_INVALID)
            {
                this._internalStatus = INTERNAL_STATUS.HARDWARE_NOT_READY;
                return;
            }
            if(this._simID == DeviceDefinitions.STRING_INVALID || this._simStatus != DeviceDefinitions.SIM_STATUS.READY)
            {
                this._internalStatus = INTERNAL_STATUS.SIM_NOT_READY;
                return;
            }
            if (this._modemSignal == DeviceDefinitions.NUMBER_INVALID || this._networkName == DeviceDefinitions.STRING_INVALID)
            {
                this._internalStatus = INTERNAL_STATUS.NETWORK_NOT_READY;
                return;
            }
            this._internalStatus = INTERNAL_STATUS.READY;
        }
        private bool InternalStatusCheck(INTERNAL_STATUS RequiredStatus, string FunctionName)
        {
            if ((int)this._internalStatus >= (int)RequiredStatus)
            {
                return true;
            }
            return this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, FunctionName);
        }
        //State Manager
        //Error Manager
        private void ErrorReset()
        {
            this._errorCode = ERROR_CODE.NO_ERROR;
            this._errorMessage = string.Empty;
        }
        private bool DllWrapperErrorCheck()
        {
            try
            {
                if (this._dllWrapper.ErrorCode == DllWrapper.ERROR_CODE.NO_ERROR)
                {
                    return true;
                }
                else
                {
                    this.DllWrapperErrorCheckSetValue(this._dllWrapper.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue(ex.Message);
                return false;
            }
        }
        private void DllWrapperErrorCheckSetValue(string error)
        {
            this._errorCode = ERROR_CODE.DLL_WRAPPER;
            this._errorMessage = error;
        }
        private bool ErrorSet(ERROR_CODE ErrorCode, string ErrorMessage)
        {
            this._errorCode = ErrorCode;
            this._errorMessage = ErrorMessage;
            return false;
        }
        private bool BaseErrorCheck()
        {
            if (this._errorCode == ERROR_CODE.DLL_WRAPPER || this._errorCode == ERROR_CODE.INTERNAL_STATUS || this._errorCode == ERROR_CODE.INVALID_INPUT)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //Error Manager
        //Busy Flag manager
        private bool TrySetBusy()
        {
            return Interlocked.CompareExchange(ref this._busyFlag, 1, 0) == 1;
        }
        private void DeviceBusyDeactive(string FuncName,bool FuncResult)
        {
            DeviceDefinitions.DEVICE_OPERATION operation = DeviceDefinitions.DEVICE_OPERATION.UNKNOWN;
            switch (FuncName)
            {
                case nameof(this.DeviceCountGet):
                    operation = DeviceDefinitions.DEVICE_OPERATION.DEVICE_COUNT;
                    break;

                case nameof(this.DeviceIdentity):
                    operation = DeviceDefinitions.DEVICE_OPERATION.DEVICE_IDENTITY;
                    break;

                case nameof(this.DeviceNumberChange):
                    operation = DeviceDefinitions.DEVICE_OPERATION.DEVICE_NUMBER_CHANGE;
                    break;

                case nameof(this.DeviceUnselect):
                    operation = DeviceDefinitions.DEVICE_OPERATION.DEVICE_UNSELECT;
                    break;

                case nameof(this.DeviceRefresh):
                    operation = DeviceDefinitions.DEVICE_OPERATION.DEVICE_REFRESH;
                    break;

                case nameof(this.SmsSend):
                    operation = DeviceDefinitions.DEVICE_OPERATION.SMS_SEND;
                    break;

                default:
                    operation = DeviceDefinitions.DEVICE_OPERATION.UNKNOWN;
                    break;
            }
            Interlocked.Exchange(ref this._busyFlag, 0);
            this.OnOperationCompleted(operation , FuncResult);
        }
        //Busy Flag manager
        //Timeout
        private void TimeOutStart()
        {
            this._timeOutCounter = DateTime.Now;
        }
        private bool TimeOutEnd()
        {
            if ((DateTime.Now - this._timeOutCounter).TotalMilliseconds >= TIME_OUT_MAX_MS)
            {
                return true;
            }
            else
            { 
                return false; 
            }
        }
        //End : Timeout
        //Device Number Change Allow
        private bool DeviceNumberChangeAllow(string FuncName)
        {
            if (!this._deviceNumberChangeAllow)
            {
                return this.ErrorSet(ERROR_CODE.DEVICE_NUMBER_CHANGE_NOT_ALLOW , FuncName);
            }
            return true;
        }
        private void DeviceNumberChanged()
        {
            this._deviceNumberChangeAllow = false;
        }
        //End : Device Number Change Allow
        //End : Helper
    }
}
