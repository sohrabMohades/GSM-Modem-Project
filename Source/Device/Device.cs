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
        ====================================================================
        State Management Rules
        ====================================================================

        Busy Flag:
        --------------------------------------------------------------------
            - Check only in Controller
            - Set only in Controller
            - Clear only in Controller
        --------------------------------------------------------------------

        Internal Status
        --------------------------------------------------------------------
        Purpose:
            Define what the Device can do and cannot do.

        Description:
            Internal Status is the main decision flag inside Device.

        Used For:
            - Operation permission checks
            - State transitions
            - Recovery logic

        Examples:
            INVALID         -> Device cannot operate, must be destroyed and recreated
            BASIC_LEVEL     -> Device is not associated with a valid selected modem. Only discovery and identity operations are allowed.
                               Device Number Change is only allowed in this status to prevent Device Number conflicts.
            SINGLE_DEVICE   -> Device is associated with a modem (only Select By Device Number) and can perform refresh and device-level operations.
            WORKING_MODE    -> Device can communicate with SIM and Network. SMS sending is not allowed.
            FULL_MODE       -> All operations are allowed, including Main Operation -> SMS Send

        Notes:
            - Only Status Update is allowed to change Internal Status. Status Update evaluates current device information, Error Code, Flags, Counters and
              Runtime Information, then determines the next Internal Status.
            - Controller Layer must never change Internal Status directly.
            - Controller Layer methods must validate Internal Status before execution.
            - Controller Layer may only call Status Update and use the resulting Internal Status for operation permission checks.            
        --------------------------------------------------------------------

        Flags:
        --------------------------------------------------------------------
            - Update only in Controller
        --------------------------------------------------------------------

        Counters:
        --------------------------------------------------------------------
            - Update only in Controller
        --------------------------------------------------------------------

        Runtime Information:
        --------------------------------------------------------------------
            - Update only in Controller
        --------------------------------------------------------------------

        Last Operation:
        --------------------------------------------------------------------
            - Update only in Controller
        --------------------------------------------------------------------

        Events:
        --------------------------------------------------------------------
            - Raise only in Controller
        --------------------------------------------------------------------

        ====================================================================
        Error Management Rules
        ====================================================================

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
            - Errors may occur in any layer and set Error Code and Error Message.
            - Controller Layer is responsible for evaluating the current Error Code, Internal Status and Runtime Information, then deciding the next action.
        DLL Error:
            - Generated in DllWrapper/Base
            - Processed in Controller

        Validation Error:
            - Generated in Base

        Workflow Error:
            - Generated in Workflow

        Status Decision:
            - Generated in Controller

        ====================================================================
        Decision Hierarchy
        ====================================================================
        Error Code => What happened?

        Internal Status => What can Device do now? What DeviceManager can request from Device?

        Flags / Runtime Information => Additional Device Conditions

        Controller => What action should be taken next?

        ====================================================================
        */

        /*
        ====================================================================
        Device Method Architecture
        ====================================================================

        Controller Layer
        ------------------------------------------------
        Responsibilities:
        - Busy Check
        - Status Check
        - Status Update
        - Runtime Update
        - Flag Update
        - Counter Update
        - Last Operation Update
        - Event Trigger
        - Error reset

        Rules:
        - Entry point of Device operations
        - Only layer allowed to change Runtime State
        - Only layer allowed to change Internal Status
        - Only layer allowed to update Flags and Counters
        - Only layer allowed to raise Events
        - Only layer allowed to Reset Error
        - Must call Workflow methods
        - Must NOT call Base methods directly

        Acceptable Operations:
        - Controller -> Workflow (only 1 Workflow)
        - Controller -> ComplexWorkflow (only 1 ComplexWorkflow)

        NOT Acceptable Operations:
        - Controller -> Execute
        - Controller -> Base
        - Controller -> Controller
        ------------------------------------------------

        ComplexWorkflow Layer
        ------------------------------------------------
        Responsibilities:
        - Call multiple Workflow methods
        - Sequence multiple operations
        - Branch based on Workflow result
        - Coordinate fallback paths
        - Coordinate multi-step device operations

        Acceptable Operations:
        - ComplexWorkflow -> Workflow, Workflow, Workflow, ...

        NOT Acceptable Operations:
        - ComplexWorkflow -> Controller
        - ComplexWorkflow -> ComplexWorkflow
        - ComplexWorkflow -> Execute
        - ComplexWorkflow -> Base
        ------------------------------------------------

        Workflow and Execute Layer
        ------------------------------------------------
        Responsibilities:
        - Retry
        - Delay
        - Confirmation
        - Multi-Step Operations
        - Execute Sequence Control

        Rules:
        - May call one or more Base methods
        - May call one or more Workflow methods
        - May contain loops
        - May contain retry logic
        - May contain confirmation logic
        - May contain recovery logic

        Must NOT:
        - Change Internal Status
        - Change Runtime State
        - Change Busy Flag
        - Raise Events

        Acceptable Operations:
        - Workflow -> Execute, Execute, Execute, ...

        NOT Acceptable Operations:
        - Workflow -> Controller
        - Workflow -> ComplexWorkflow
        - Workflow -> Workflow
        - Workflow -> Base

        Acceptable Operations:
        - Execute -> Base, Base, Base, ...

        NOT Acceptable Operations:
        - Execute -> Controller
        - Execute -> ComplexWorkflow
        - Execute -> Workflow
        - Execute -> Execute
        ------------------------------------------------

        Base Layer
        ------------------------------------------------
        Responsibilities:
        - Single Atomic Operation
        - DLL Access
        - Input Validation
        - Output Validation
        - Output Value Assignment

        Rules:
        - One hardware operation only
        - confirmation Dll Value or Values
        - No retry, No delay, No recovery
        - No workflow logic

        Must NOT:
        - Base Layer validates DLL returned values.
        - Base Layer does NOT perform multi-read confirmation.
        - Multi-read confirmation belongs to Workflow Layer.
        - Change Internal Status
        - Change Runtime State
        - Change Busy Flag
        - Raise Events
        ------------------------------------------------

        DllWrapper
        ------------------------------------------------
        Responsibilities:
        - Direct DLL Access Only

        Rules:
        - No Device Logic
        - No Retry Logic
        - No Recovery Logic
        - No Status Logic
        ------------------------------------------------

        ====================================================================
        Layer Relation
        ====================================================================

        ------------------------------------------------
        Controller
            |----------------
            ↓               |
        ComplexWorkflow     |
            ↓               ↓
        Workflow         Workflow
            ↓               ↓
          Execute        Execute
            ↓               ↓
          Base            Base
            ↓               ↓
        DllWrapper     DllWrapper
        ------------------------------------------------

        ------------------------------------------------
        Controller
            ├─ Workflow
            ├─ Workflow
            └─ ComplexWorkflow
                  ├─ Workflow
                  ├─ Workflow
                  └─ Workflow
        ------------------------------------------------

        ====================================================================
        Execution Flow
        ====================================================================

        Controller
            ↓
        Workflow
            ↓
        Controller Update
            ↓
        Event

        ====================================================================
        Workflow Pattern
        ====================================================================

        Workflow
            ↓
        Execute
                ↓
                Base
                ↓
                Base
                ↓
                Base
            ↓
        Check
            ↓
        Commit Values
            ↓
        Return Result

        Notes:

        Check:
            - Compare multiple reads
            - Compare confirmation values
            - Verify operation consistency

        Commit Values:
            - Update output parameters
            - Update internal cached values

        ====================================================================
        Base Pattern
        ====================================================================

        Base Method
            ↓
        Input Check
            ↓
        DLL Call
            ↓
        DLL Error Check
            ↓
        Output Check
            ↓
        Set Output Values
            ↓
        Return Result
        */

        //Get
        //Variable
        /*
        public STATUS Status
        {
            get
            {
                if (this._state.Busy == 1)
                {
                    return STATUS.BUSY;
                }

                switch (this._state.InternalStatus)
                {
                    case INTERNAL_STATUS.INVALID:
                        return STATUS.DISCONNECTED;

                    case INTERNAL_STATUS.BASIC_LEVEL:
                    case INTERNAL_STATUS.SINGLE_DEVICE:
                    case INTERNAL_STATUS.WORKING_MODE:
                    case INTERNAL_STATUS.FULL_MODE:
                        return STATUS.READY;

                    default:
                        return STATUS.NOT_READY;
                }
            }
        }
        */
        public ERROR_CODE ErrorCode
        {
            get => this._state.ErrorCode;
        }
        public string ErrorMessage
        {
            get => this._state.ErrorMessage;
        }
        public string DllFileName
        {
            get => this._dllFileName;
        }
        //End : Variable
        //identity
        public int DeviceCount
        {
            get => this._runtimeIdentity.DeviceCount;
        }
        public int DeviceIndex
        {
            get => this._runtimeIdentity.DeviceIndex;
            set
            {
                if (this.DeviceIndexRangeCheck(value))
                {
                    this._runtimeIdentity.DeviceIndex = value;
                }
            }
        }
        public int DeviceNumber
        {
            get => this._runtimeIdentity.DeviceNumber;
        }
        //End : identity
        //property
        public bool GsmPower
        {
            get
            {
                if (this._runtimeHardware.GsmPower == BOOL_TYPE.TRUE)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool GsmIsConnect
        {
            get
            {
                if (this._runtimeHardware.GsmIsConnect == BOOL_TYPE.TRUE)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool GsmIsReady
        {
            get
            {
                if (this._runtimeHardware.GsmIsReady == BOOL_TYPE.TRUE)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public DeviceDefinitions.GSM_STATUS GsmStatus
        {
            get => this._runtimeHardware.GsmStatus;
        }
        public int GsmStatusValue
        {
            get => this._runtimeHardware.GsmStatusValue;
        }
        public string GsmStatusMessage
        {
            get => this._runtimeHardware.GsmStatusMessage;
        }
        public DeviceDefinitions.SIM_STATUS SimStatus
        {
            get => this._runtimeSim.SIMStatus;
        }
        public string IMEI
        {
            get => this._runtimeHardware.IMEI;
        }
        public string MobileNumber
        {
            get => this._runtimeOperator.MobileNumber;
        }
        public string SimID
        {
            get => this._runtimeSim.SIMID;
        }
        public string SoftwareModel
        {
            get => this._runtimeHardware.SoftwareVersion;
        }
        public string DeviceModel
        {
            get => this._runtimeHardware.DeviceInformation;
        }
        public string SMSCenter
        {
            get => this._runtimeNetwork.SMSCenter;
        }
        public string NetworkName
        {
            get => this._runtimeNetwork.NetworkName;
        }
        public string NationalMobileID
        {
            get => this._runtimeSim.NationalMobileID;
        }
        public int Signal
        {
            get => this._runtimeNetwork.ModemSignal;
        }
        public string GSMDateTime
        {
            get => this._runtimeUngroup.GsmDateTime;
        }
        //End : property
        //Common Operations
        public DeviceDefinitions.SMS_TYPE SMSType
        {
            get => this._runtimeWorkingConfig.SmsType;
        }
        public DeviceDefinitions.SMS_CHAR_MODE SMSCharMode
        {
            get => this._runtimeWorkingConfig.SmsCharMode;
        }
        //End : Common Operations
        //Send SMS data
        public bool SmsSendDeliveryReport
        {
            get => this._runtimeSmsSendConfig.DeliveryReport;
            set => this._runtimeSmsSendConfig.DeliveryReport = value;
        }
        public int SmsSendSendInterval
        {
            get => this._runtimeSmsSendConfig.SendInterval;
            set
            {
                if (value >= DeviceDefinitions.SMS_SEND_SEND_INTERVAL_MIN && value <= DeviceDefinitions.SMS_SEND_SEND_INTERVAL_MAX)
                {
                    this._runtimeSmsSendConfig.SendInterval = value;
                }
            }
        }
        public int SmsSendRetryTimes
        {
            get => this._runtimeSmsSendConfig.RetryTimes;
            set
            {
                if (value >= DeviceDefinitions.SMS_SEND_RETRY_TIMES_MIN && value <= DeviceDefinitions.SMS_SEND_RETRY_TIMES_MAX)
                {
                    this._runtimeSmsSendConfig.RetryTimes = value;
                }
            }
        }
        public DeviceDefinitions.SMS_VALIDITY SmsSendSmsValidity
        {
            get => this._runtimeSmsSendConfig.SmsValidity;
            set => this._runtimeSmsSendConfig.SmsValidity = value;
        }
        //End : Send SMS data
        //Send Operations
        public int ATCommandDelay
        {
            get => this._runtimeWorkingConfig.AtCommandDelay;
        }
        //End : Send Operations
        //Get Operations
        public int InboxTotalSpace
        {
            get => this._runtimeInboxProperty.TotalSpace;
        }
        public int InboxFreeSpace
        {
            get => (this._runtimeInboxProperty.TotalSpace - this._runtimeInboxProperty.SMSNumber);
        }
        public int InboxSMSNumber
        {
            get => this._runtimeInboxProperty.SMSNumber;
        }
        public bool NewInboxAlarm
        {
            get => this._runtimeInboxConfig.NewInboxAlarm;
        }
        //End : Get Operations
        //Check condition
        public int SignalMinValue
        {
            get => this._runtimeConfig.SignalMinAcceptableValue;
            set
            {
                if (value >= DeviceDefinitions.SIGNAL_MIN && value <= DeviceDefinitions.SIGNAL_MAX)
                {
                    this._runtimeConfig.SignalMinAcceptableValue = value;
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

        //Helper
        //Constructor Helper
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
                    DllWrapperErrorCheckSetValue("DllWrapper ErrorCode = " + this._dllWrapper.ErrorCode.ToString() + " - ErrorMessage = " + this._dllWrapper.ErrorMessage);
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
                this._state.InternalStatus = INTERNAL_STATUS.BASIC_LEVEL;
            }
            else
            {
                this._state.InternalStatus = INTERNAL_STATUS.INVALID;
            }
        }
        //Constructor Helper
        //Device Helper
        private void XXXBaseSetValue<T>(FUNC_RESULT result, T successValue, T failValue, ref T target)
        {
            target = result == FUNC_RESULT.SUCCESS ? successValue : failValue;
        }
        private void XXXBaseSetValue<T>(T newValue, ref T target)
        {
            target = newValue;
        }
        private void XXXSetValue<T>(FUNC_RESULT result, T successValue, T failValue, ref T outputValue, ref T internalValue, bool internalValueSet)
        {
            T value = result == FUNC_RESULT.SUCCESS ? successValue  : failValue;
            outputValue = value;
            if (internalValueSet)
            {
                internalValue = value;
            }
        }
        private void XXXSetValue<T>(T newValue, ref T outputValue, ref T internalValue, bool internalValueSet)
        {
            outputValue = newValue;
            if (internalValueSet)
            {
                internalValue = newValue;
            }
        }
        private FUNC_RESULT XXXCheck<T>(FUNC_RESULT Result, FUNC_RESULT Result1, FUNC_RESULT Result2, T Value1, T Value2)
        {
            if (Result == FUNC_RESULT.MAJOR_ERROR)
            {
                return FUNC_RESULT.MAJOR_ERROR;
            }
            if (Result1 == FUNC_RESULT.SUCCESS && Result2 == FUNC_RESULT.SUCCESS && EqualityComparer<T>.Default.Equals(Value1, Value2))
            {
                return FUNC_RESULT.SUCCESS;
            }
            return FUNC_RESULT.FAIL;
        }
        //End : Device Helper
        //End : Helper
    }
}
