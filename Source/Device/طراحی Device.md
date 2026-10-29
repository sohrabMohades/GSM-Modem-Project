# طراحی Device - تفاصیل سیستم

## ۱. تفصیلات ساختار داده‌های داخلی

### ۱.۱ DeviceRuntimeState

```csharp
private sealed class DeviceRuntimeState
{
	public int Busy = 0;                              // Atomic: 0/1
	public ERROR_CODE ErrorCode = ERROR_CODE.NO_ERROR;
	public string ErrorMessage = string.Empty;
	public INTERNAL_STATUS InternalStatus = INTERNAL_STATUS.INVALID;
}
```

**جزئیات:**
- `Busy`: تنها از طریق `Interlocked.CompareExchange` تغییر می‌یابد (lock-free synchronization).
- `ErrorCode`: مقدار آخرین خطا در هر لحظه.
- `ErrorMessage`: توضیح متنی خطا برای logging.
- `InternalStatus`: حالت کنونی دستگاه؛ تنها توسط `InternalStatusUpdate()` تغییر می‌یابد.

### ۱.۲ DeviceRuntimeFlags

```csharp
private sealed class DeviceRuntimeFlags
{
	public bool NeedRefresh = false;           // بروزرسانی نیاز است
	public bool NeedSimChange = false;         // تغییر SIM نیاز است
	public bool LowCredit = false;             // اعتبار کم
	public bool AutoRefreshEnabled = true;     // فعال: خود‌کار refresh
	public bool AutoSmsSendEnabled = true;     // فعال: خود‌کار ارسال SMS
	public bool AutoInboxEnabled = true;       // فعال: خود‌کار فحص صندوق
}
```

**کاربرد:**
- تنها توسط Controller متد‌ها بروزرسانی می‌شوند.
- Workflow‌ها به آن‌ها دسترسی خواندنی دارند.
- DeviceManager بر اساس این Flag‌ها تصمیم می‌گیرد.

### ۱.۳ DeviceRuntimeInfo

```csharp
private sealed class DeviceRuntimeInfo
{
	// Counters
	public int RefreshFailConsecutive = 0;     // تعداد متوالی ناموفق
	public int RefreshFailTotal = 0;
	public int SmsSendFailConsecutive = 0;
	public int SmsSendFailTotal = 0;
	public int SmsSendSuccessTotal = 0;
	public int TimeoutConsecutive = 0;
	public int TimeoutTotal = 0;

	// Timestamps
	public DateTime LastRefreshTime = DateTime.MinValue;
	public DateTime LastSmsSendTime = DateTime.MinValue;
	public DateTime LastInboxReadTime = DateTime.MinValue;
	public DateTime CreateDateTime = DateTime.MinValue;
	public DateTime LastRecoveryTime = DateTime.MinValue;

	// Operation Tracking
	public DEVICE_OPERATION LastOperation = DEVICE_OPERATION.UNKNOWN;
	public FUNC_RESULT LastOperationResult = FUNC_RESULT.UNKNOWN;
	public DateTime LastOperationTime = DateTime.MinValue;

	// Timeout Management
	public DateTime TimeOutCounter = DateTime.MinValue;

	// GSM Status Snapshot
	public GsmStatus GsmStatus = new GsmStatus();
}

private sealed class GsmStatus
{
	public bool ModemStartup = false;          // MODEM:STARTUP دریافت شده
	public bool ModemReady = false;            // +PBREADY دریافت شده
	public int ModemATE = NUMBER_INVALID;      // ATE0 یا ATE1
}
```

**نکات:**
- شمارش‌گرهای متوالی برای شناخت الگوی خرابی استفاده می‌شوند.
- `TimeOutCounter` برای محاسبهٔ timeout استفاده می‌شود.
- `GsmStatus` وضعیت مودم را track می‌کند.

### ۱.۴ DeviceRuntimeConfiguration

```csharp
private sealed class DeviceRuntimeConfiguration
{
	public bool BufferCheck = true;            // استفاده از Buffer برای اعتبارسنجی
	public int SignalMinAcceptableValue = 10;  // حداقل سیگنال قابل قبول
	public bool EnableDoubleReadValidation = false;  // خواندن دوبار برای اعتبارسنجی
	public bool TimeoutCheck = true;           // بررسی timeout
	public int TimeoutMaxMs = (int)BUFFER_TIME_OUT_MS.TIMEOUT_MAX_MS;  // حد بیشین timeout
	public bool PowerONDoubleCheck = true;     // دوبار بررسی پاور ON
}
```

### ۱.۵ DeviceRuntimeProperties - سازمان‌دهی property‌ها

```
DeviceRuntimeIdentity
├── DeviceCount          // تعداد کل دستگاه‌ها
├── DeviceIndex          // شاخص دستگاه انتخاب‌شده
└── DeviceNumber         // شماره دستگاه

DeviceRuntimeHardware
├── IMEI                 // شناسهٔ دستگاه
├── SoftwareVersion      // نسخهٔ نرم‌افزار
├── HardwareModel        // مدل سخت‌افزار
├── GsmPower             // BOOL_TYPE: پاور ON/OFF
├── GsmIsConnect         // BOOL_TYPE: اتصال برقرار
├── GsmIsReady           // BOOL_TYPE: آماده برای ارتباط
└── (GSM Status - unused)

DeviceRuntimeSIM
├── SIMStatus            // وضعیت کارت SIM
├── SIMID                // شناسهٔ SIM
└── NationalMobileID     // شماره‌ملی (Mvno)

DeviceRuntimeNetwork
├── ModemSignal          // سطح سیگنال (-1 تا 31)
├── NetworkName          // نام شبکه
└── SMSCenter            # شماره مرکز SMS

DeviceRuntimeOperator
├── MobileNumber         // شماره موبایل
└── RemainBalance        // موجودی

DeviceRuntimeUnGrouped
└── GsmDateTime          // تاریخ و زمان GSM

DeviceRuntimeWorkingConfiguration
├── AtCommandDelay       // تأخیر دستور AT
├── SmsType              // نوع SMS (PDU یا Text)
└── SmsCharMode          // حالت کاراکتر (GSM, UCS2, ...)

DeviceRuntimeSmsSendConfiguration
├── DeliveryReport       // درخواست delivery report
├── SendInterval         // فاصلهٔ ارسال بین پیام‌ها
├── RetryTimes           // تعداد تلاش‌های مجدد
├── SmsValidity          // زمان اعتبار
├── SmsReference         // شمارهٔ reference
└── CountryCode          # کد کشور

DeviceRuntimeInbox
├── TotalSpace           // فضای کل
└── SMSNumber            // تعداد پیام‌های موجود

DeviceRuntimeBuffer
├── BufferAutoUpdate     // بروزرسانی خودکار buffer
├── BufferNewData        // آیا داده جدید وجود دارد
├── BufferLastIndex      // آخرین شاخص
├── BufferDataRaw        // لیست پیام‌های خام
├── BufferDataGroup      // لیست پیام‌های تحلیل‌شده
└── (مختلف timestamps)
```

## ۲. تفاصیل جریان کنترل (Controller)

### ۲.۱ Controller متد ساختار کلی

```csharp
public bool ControllerMethod(/* parameters */)
{
	FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;

	// Phase 1: Busy Check
	if (!this.TrySetBusy())
	{
		this.ErrorSet(ERROR_CODE.BUSY, nameof(this.ControllerMethod));
		return false;
	}

	try
	{
		// Phase 2: Status Check
		if (!this.InternalStatusCheck(INTERNAL_STATUS.REQUIRED_LEVEL, nameof(this.ControllerMethod)))
		{
			this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.ControllerMethod));
			return false;
		}

		// Phase 3: Clear Previous Error
		this.ErrorReset();

		// Phase 4: Call Workflow/ComplexWorkflow
		funcResult = this.WorkflowMethod(/* params */);

		// Phase 5: Update Flags & Counters
		// ... flag updates ...

		// Phase 6: Update Runtime Info
		// ... info updates ...

		// Phase 7: Update Internal Status
		this.InternalStatusUpdate();

		// Phase 8: Return Result
		return this.FuncResult(funcResult);
	}
	finally
	{
		// Phase 9: Always Clear Busy Flag (even on exception)
		this.DeviceBusyDeactive();

		// Phase 10: Optional Event Trigger
		// this.OnOperationCompleted(DEVICE_OPERATION, funcResult);
	}
}
```

### ۲.۲ StatusCheck Implementation

```csharp
private bool InternalStatusCheck(INTERNAL_STATUS MinStatusNeed, string FuncName)
{
	// DeviceNumberChange نیاز به تطابق دقیق دارد
	if (FuncName == nameof(this.DeviceNumberChange))
	{
		if ((int)this._state.InternalStatus == (int)MinStatusNeed) 
			return true;
		else 
			return false;
	}

	// دیگر متد‌ها: بررسی حداقل سطح مورد نیاز
	if ((int)this._state.InternalStatus >= (int)MinStatusNeed) 
		return true;
	else 
		return false;
}
```

## ۳. تفاصیل جریان Workflow

### ۳.۱ Workflow-Execute-Base Pattern

```
Workflow Pattern Example: GSMPowerGetWorkflow
│
├─► Phase 1: Determine Execution Mode
│       └─ BufferCheck enabled? → Choose GSMPowerGetExecuteBufferCheck
│                             → Choose GSMPowerGetExecuteNormal
│
├─► Phase 2: Call Execute (with optional double-read)
│       │
│       ├─ Loop: While loopCount > 0 AND condition not met
│       │   └─ Thread.Sleep(Delay)
│       │   └─ Call Base
│       │   └─ Check for MAJOR_ERROR (exit immediately)
│       │
│       └─ Return XXXExecuteReturn (compare results if double-read)
│
├─► Phase 3: Check & Commit Values
│       └─ XXXSetValue: update if success, else restore default
│
└─► Phase 4: Return Result
```

### ۳.۲ Execute Layer - DelayLoop Management

```csharp
private FUNC_RESULT GSMPowerGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref BOOL_TYPE ResultBool)
{
	if (Result != FUNC_RESULT.SUCCESS) return Result;

	// انتخاب نسخهٔ مناسب بر اساس BufferCheck
	if (!this._runtimeConfig.BufferCheck) 
		return this.GSMPowerGetExecuteNormal(loopCount, Delay, ref ResultBool);
	else 
		return this.GSMPowerGetExecuteBufferCheck(loopCount, Delay, ref ResultBool);
}

// نسخهٔ ساده: تکرار تا موفقیت یا شکست
private FUNC_RESULT GSMPowerGetExecuteNormal(int loopCount, int Delay, ref BOOL_TYPE ResultBool)
{
	FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
	BOOL_TYPE resultBool = BOOL_TYPE.FALSE;
	ResultBool = BOOL_TYPE.UNKNOWN;

	while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || 
		   funcResult2 != FUNC_RESULT.SUCCESS || ResultBool != BOOL_TYPE.TRUE))
	{
		Thread.Sleep(Delay);

		// اولین خواندن
		if (funcResult1 != FUNC_RESULT.SUCCESS || ResultBool != BOOL_TYPE.TRUE) 
			funcResult1 = this.GSMPowerGetBase(ref ResultBool);

		// دومین خواندن (برای اعتبارسنجی دوبار)
		else if (this._runtimeConfig.PowerONDoubleCheck) 
			funcResult2 = this.GSMPowerGetBase(ref resultBool);
		else 
			break;

		// اگر MAJOR_ERROR: برگشت فوری
		if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult2 == FUNC_RESULT.MAJOR_ERROR) 
			return FUNC_RESULT.MAJOR_ERROR;
	}

	// بررسی اگر دوبار validation فعال است
	return this.XXXExecuteReturn(this._runtimeConfig.PowerONDoubleCheck, 
								 funcResult1, funcResult2, 
								 ref ResultBool, resultBool, BOOL_TYPE.FALSE);
}
```

### ۳.۳ Execute با Buffer Check

```csharp
private FUNC_RESULT GSMPowerGetExecuteBufferCheck(int loopCount, int Delay, ref BOOL_TYPE ResultBool)
{
	BUFFER_UPDATE_SOURCE bufferUpdateSource = BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY;
	FUNC_RESULT result = FUNC_RESULT.UNKNOWN;

	while (loopCount-- > 0 && result != FUNC_RESULT.SUCCESS && result != FUNC_RESULT.MAJOR_ERROR)
	{
		Thread.Sleep(Delay);

		// اولین‌بار: فقط Buffer را بروزرسانی کنید
		if (bufferUpdateSource == BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY) 
			this.XXXBufferCheck(FUNC_RESULT.SUCCESS, ref bufferUpdateSource, BUFFER_UPDATE_TYPE.GSM_POWER_GET);

		// Base فراخوانی کنید
		result = this.GSMPowerGetBase(ref ResultBool);

		// نتیجهٔ Base را بخش Buffer validate کنید
		result = this.XXXBufferCheck(result, ref bufferUpdateSource, BUFFER_UPDATE_TYPE.GSM_POWER_GET);
	}

	return result;
}
```

### ۳.۴ Base Layer - Atomicity

```csharp
private FUNC_RESULT GSMPowerGetBase(ref BOOL_TYPE Result)
{
	bool result = false;
	FUNC_RESULT funcResult = FUNC_RESULT.SUCCESS;

	// Step 1: DLL Call
	funcResult = this.GSMPowerGetBaseDll(funcResult, ref result);

	// Step 2: Output Validation
	// (no additional validation for GsmPowerGet)

	// Step 3: Set Output Values
	this.XXXBaseSetValue(funcResult, result, ref Result);

	return funcResult;
}

private FUNC_RESULT GSMPowerGetBaseDll(FUNC_RESULT FuncResult, ref bool Result)
{
	if (FuncResult != FUNC_RESULT.SUCCESS) return FuncResult;

	// DLL Call
	Result = this._dllWrapper.GsmPowerGet();

	// DLL Error Check
	return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
}
```

## ۴. مدیریت خطا - تفاصیل

### ۴.۱ Error Reset & Set Pattern

```csharp
// بروز رسانی خطا
private bool ErrorSet(ERROR_CODE ErrorCode, string ErrorMessage)
{
	this._state.ErrorCode = ErrorCode;
	this._state.ErrorMessage = ErrorMessage;
	return false;  // بیشتر اوقات با false برمی‌گردد
}

// پاک‌سازی خطا
private void ErrorReset()
{
	this._state.ErrorCode = ERROR_CODE.NO_ERROR;
	this._state.ErrorMessage = string.Empty;
}

// بررسی خطای DLL
private FUNC_RESULT DllWrapperHasError(FUNC_RESULT funcResult)
{
	try
	{
		if (this._dllWrapper.ErrorCode == DllWrapper.ERROR_CODE.NO_ERROR) 
			return funcResult;
		else
		{
			this.DllWrapperErrorCheckSetValue(this._dllWrapper.ErrorMessage);
			return FUNC_RESULT.MAJOR_ERROR;
		}
	}
	catch (Exception ex)
	{
		this.DllWrapperErrorCheckSetValue(ex.Message);
		return FUNC_RESULT.MAJOR_ERROR;
	}
}

private void DllWrapperErrorCheckSetValue(string error)
{
	this._state.ErrorCode = ERROR_CODE.DLL_WRAPPER;
	this._state.ErrorMessage = error;
}
```

## ۵. مدیریت Busy Flag - Synchronization

### ۵.۱ Lock-Free Pattern

```csharp
// تنی‌تر Busy Flag را تنظیم کند: Controller
private bool TrySetBusy()
{
	// Atomic Compare-And-Swap: اگر 0 است، به 1 تغییر دهید
	// اگر موفق باشد (قبلاً 0 بود)، true برگردانید
	return Interlocked.CompareExchange(ref this._state.Busy, 1, 0) == 0;
}

// تنها برای پاک‌سازی
private void DeviceBusyDeactive()
{
	// Atomic Set to 0
	Interlocked.Exchange(ref this._state.Busy, 0);
}

// Get: بدون synchronization (concurrent read OK)
public bool IsBusy => this._state.Busy == 1;
```

## ۶. Timeout Management

### ۶.۱ Timeout Pattern

```csharp
private void TimeOutStart()
{
	this._runtimeInfo.TimeOutCounter = DateTime.Now;
}

private bool TimeOutEnd()
{
	if ((DateTime.Now - this._runtimeInfo.TimeOutCounter).TotalMilliseconds >= 
		this._runtimeConfig.TimeoutMaxMs) 
		return true;
	else 
		return false;
}
```

### ۶.۲ Buffer Timeout Check

```csharp
private FUNC_RESULT BufferTimeoutCheck(FUNC_RESULT Result, 
										BUFFER_UPDATE_SOURCE BufferUpdateSource, 
										BUFFER_UPDATE_TYPE BufferUpdateType)
{
	if (Result != FUNC_RESULT.SUCCESS) return Result;

	// فقط Workflow source timeout را بررسی کند
	if (BufferUpdateSource != BUFFER_UPDATE_SOURCE.WORKFLOW) 
		return FUNC_RESULT.SUCCESS;

	// محاسبهٔ timeout
	int timeout = (int)(DateTime.Now - this._runtimeBuffer.BufferLastUpdatePerior).TotalMilliseconds;

	if (timeout > this._runtimeConfig.TimeoutMaxMs) 
		return FUNC_RESULT.MAJOR_ERROR;

	// بررسی بر اساس نوع عملیات
	switch (BufferUpdateType)
	{
		case BUFFER_UPDATE_TYPE.DEVICE_NUMBER_GET:
		case BUFFER_UPDATE_TYPE.GSM_POWER_GET:
		case BUFFER_UPDATE_TYPE.GSM_POWER_SET:
			if (timeout <= (int)BUFFER_TIME_OUT_MS.SHORT) 
				return FUNC_RESULT.SUCCESS;
			else 
				return FUNC_RESULT.MAJOR_ERROR;

		case BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY:
			// بررسی تدریجی: ابتدا منتظر STARTUP، سپس READY
			if (!this._runtimeInfo.GsmStatus.ModemStartup)
				if (timeout <= (int)BUFFER_TIME_OUT_MS.MODEM_STARTUP) 
					return FUNC_RESULT.SUCCESS;
				else 
					return FUNC_RESULT.MAJOR_ERROR;
			else if (!this._runtimeInfo.GsmStatus.ModemReady)
				if (timeout <= (int)BUFFER_TIME_OUT_MS.MODEM_READY) 
					return FUNC_RESULT.SUCCESS;
				else 
					return FUNC_RESULT.MAJOR_ERROR;
			else 
				return FUNC_RESULT.MAJOR_ERROR;

		default:
			return FUNC_RESULT.SUCCESS;
	}
}
```

## ۷. تفاصیل Buffer Operations

### ۷.۱ BufferClear Pattern

```csharp
private FUNC_RESULT BufferClearWorkflow(bool InternalValueSet)
{
	FUNC_RESULT result = FUNC_RESULT.SUCCESS;

	// Execute with delay
	result = this.BufferClearExecute(result, (int)OPERATION_DELAY_MS.BUFFER_UPDATE);

	// Commit Values: پاک‌سازی تمام فیلدهای Buffer
	this.XXXSetValue(DeviceDefinitions.NUMBER_INVALID, 
					 ref this._runtimeBuffer.BufferLastIndex, InternalValueSet);
	this.XXXSetValue(false, 
					 ref this._runtimeBuffer.BufferNewData, InternalValueSet);
	this.XXXSetValue(DeviceDefinitions.STRING_INVALID, 
					 ref this._runtimeBuffer.BufferMessage, InternalValueSet);
	this.XXXSetValue(DeviceDefinitions.NUMBER_INVALID, 
					 ref this._runtimeBuffer.BufferMessageLen, InternalValueSet);

	return result;
}

private FUNC_RESULT BufferClearExecute(FUNC_RESULT Result, int Delay)
{
	if (Result != FUNC_RESULT.SUCCESS) return Result;
	Thread.Sleep(Delay);
	return this.BufferClearBase();
}

private FUNC_RESULT BufferClearBase()
{
	return this.BufferClearBaseDll(FUNC_RESULT.SUCCESS);
}

private FUNC_RESULT BufferClearBaseDll(FUNC_RESULT Result)
{
	if (Result != FUNC_RESULT.SUCCESS) return Result;
	this._dllWrapper.ClearBuffer();
	return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
}
```

### ۷.۲ BufferUpdate Composition

```csharp
private FUNC_RESULT BufferUpdate(BUFFER_UPDATE_SOURCE BufferUpdateSource, 
								  ref BUFFER_UPDATE_TYPE BufferUpdateType)
{
	if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.UNKNOWN) 
		return FUNC_RESULT.MAJOR_ERROR;

	FUNC_RESULT result = FUNC_RESULT.SUCCESS;

	// Phase 1: Remove old data (if not from WORKFLOW)
	if (BufferUpdateSource != BUFFER_UPDATE_SOURCE.WORKFLOW) 
		this.BufferUpdateAnalyzeRemoveExtra(result, null);

	// Phase 2: Timeout Check
	result = this.BufferTimeoutCheck(result, BufferUpdateSource, BufferUpdateType);

	// Phase 3: Read new messages
	result = this.BufferUpdateList(result, BufferUpdateSource);

	// Phase 4: Parse messages
	result = this.BufferUpdateAnalyze(result, BufferUpdateSource, ref BufferUpdateType);

	return result;
}
```

## ۸. Pattern‌های کمکی - XXX Functions

### ۸.۱ Value Setting Helpers

```csharp
// تنظیم مقدار در صورت موفقیت، در غیر صورت مقدار پیش‌فرض
private void XXXBaseSetValue<T>(FUNC_RESULT result, T successValue, 
								 T failValue, ref T target)
{
	target = result == FUNC_RESULT.SUCCESS ? successValue : failValue;
}

// تنظیم مقدار در دو مکان (output و internal)
private void XXXSetValue<T>(FUNC_RESULT result, T successValue, T failValue, 
							 ref T outputValue, ref T internalValue, bool internalValueSet)
{
	outputValue = result == FUNC_RESULT.SUCCESS ? successValue : failValue;
	if (internalValueSet) 
		internalValue = outputValue;
}

// برای BOOL_TYPE خاص
private void XXXBaseSetValue(FUNC_RESULT resultFunc, bool inputResult, ref BOOL_TYPE outResult)
{
	outResult = resultFunc == FUNC_RESULT.SUCCESS ? 
				(inputResult ? BOOL_TYPE.TRUE : BOOL_TYPE.FALSE) : BOOL_TYPE.UNKNOWN;
}

// تبدیل BOOL_TYPE به bool
private bool XXXOutput(BOOL_TYPE value)
{
	return value == BOOL_TYPE.TRUE;
}

// تبدیل FUNC_RESULT + bool به BOOL_TYPE
private BOOL_TYPE BoolType(FUNC_RESULT FuncResult, bool ResultBool)
{
	if (FuncResult == FUNC_RESULT.SUCCESS && ResultBool) 
		return BOOL_TYPE.TRUE;
	else 
		return BOOL_TYPE.FALSE;
}
```

### ۸.۲ Double-Read Validation

```csharp
private FUNC_RESULT XXXExecuteReturn<T>(bool DoubleCheck, 
										FUNC_RESULT Result1, FUNC_RESULT Result2, 
										ref T InputValue1, T InputValue2, T InvalidValue)
{
	// MAJOR_ERROR بلافاصله برگشت
	if (Result1 == FUNC_RESULT.MAJOR_ERROR || Result2 == FUNC_RESULT.MAJOR_ERROR) 
		return FUNC_RESULT.MAJOR_ERROR;

	// اگر double-check فعال است
	if (DoubleCheck)
	{
		// هر دو موفق و مقادیر برابر؟
		if (Result1 == FUNC_RESULT.SUCCESS && Result2 == FUNC_RESULT.SUCCESS && 
			EqualityComparer<T>.Default.Equals(InputValue1, InputValue2)) 
			return FUNC_RESULT.SUCCESS;
		else 
		{ 
			InputValue1 = InvalidValue; 
			return FUNC_RESULT.FAIL; 
		}
	}

	// یا فقط اولی را برگردان
	else 
		return Result1;
}
```

## ۹. معاملات ویژه

### ۹.۱ SMS Send Workflow

```csharp
public bool SmsSend(string PhoneNumber, string SmsText, ref List<string> SMSReference)
{
	// ... (Controller Boilerplate)

	try
	{
		// Phase 1: Prepare SMS Data
		DeviceDefinitions.SmsSendData smsSendData = null;
		funcResult = this.SmsSendDataInitial(ref smsSendData, PhoneNumber, SmsText);

		// Phase 2: Set SMS Configuration (CharMode, Type)
		if (funcResult == FUNC_RESULT.SUCCESS) 
			funcResult = this.SendConfigurationSetComplexWorkflow(SMS_CHAR_MODE.UCS2, SMS_TYPE.PERSIAN);

		// Phase 3: Send SMS
		if (funcResult == FUNC_RESULT.SUCCESS) 
			funcResult = this.SMSSendWorkflow(smsSendData, ref SMSReference);

		// ... (Controller Boilerplate)
	}
	finally { /* ... */ }
}

private FUNC_RESULT SMSSendBase(SmsSendData smsSendData, ref List<string> SMSReference)
{
	FUNC_RESULT result = FUNC_RESULT.SUCCESS;
	string smsSendResultRaw = string.Empty;
	SMSReference = new List<string>();

	// Phase 1: DLL Call
	result = this.SMSSendBaseDll(result, smsSendData, ref smsSendResultRaw);

	// Phase 2: Parsing Result (using StringAnalyzer)
	result = this.SMSSendBaseCheck(result, smsSendResultRaw, ref SMSReference);

	return result;
}

private FUNC_RESULT SMSSendBaseCheck(FUNC_RESULT Result, string SmsSendResultRaw, 
									  ref List<string> SMSReference)
{
	if (Result != FUNC_RESULT.SUCCESS) return Result;

	// تحلیل نتیجهٔ ارسال
	var result = StringAnalyzer.ParseSmsSendResult(SmsSendResultRaw);

	if (result.Result)
	{
		// موفقیت: تنظیم references
		SMSReference = result.Data;
		return FUNC_RESULT.SUCCESS;
	}
	else if (result.ErrorCode != StringAnalyzer.ERROR_CODE.NO_ERROR)
	{
		// خطای تحلیل
		this.ErrorSet(ERROR_CODE.SEND_SMS_RESULT, result.ErrorMessage);
		return FUNC_RESULT.MAJOR_ERROR;
	}
	else
	{
		// شکست معمول (خالی بودن)
		return FUNC_RESULT.FAIL;
	}
}
```

### ۹.۲ Device Identity ComplexWorkflow

```csharp
private FUNC_RESULT DeviceIdentityComplexWorkflow(int DeviceIndex, 
												   ref int DeviceNumber, 
												   ref string IMEI)
{
	FUNC_RESULT result = FUNC_RESULT.SUCCESS;

	// Phase 1: Select Device By Index
	result = this.DeviceSelectByIndexWorkflow(DeviceIndex, true, true);
	if (result != FUNC_RESULT.SUCCESS) return result;

	// Phase 2: Get IMEI
	result = this.ImeiGetWorkflow(ref IMEI, true);
	if (result != FUNC_RESULT.SUCCESS) return result;

	// Phase 3: Output DeviceNumber from Runtime
	DeviceNumber = this._runtimeIdentity.DeviceNumber;

	return FUNC_RESULT.SUCCESS;
}
```

## ۱۰. Refresh Operation - Multi-Step Example

```csharp
private FUNC_RESULT DeviceRefreshComplexWorkFlow(bool PowerRestartAllow)
{
	FUNC_RESULT result = FUNC_RESULT.SUCCESS;

	// بررسی GsmPower
	bool powerResult = false;
	result = this.GSMPowerGetWorkflow(ref powerResult, true);
	if (result != FUNC_RESULT.SUCCESS) return result;

	// اگر OFF است و PowerRestart مجاز: روشن کنید
	if (!powerResult && PowerRestartAllow)
	{
		result = this.GSMPowerSetWorkflow(true, true, true);
		if (result != FUNC_RESULT.SUCCESS) return result;
	}

	// بررسی اتصال
	bool connectResult = false;
	result = this.GSMIsConnectWorkflow(ref connectResult, true);
	if (result != FUNC_RESULT.SUCCESS) return result;

	// اگر متصل: اطلاعات را خوانید
	if (connectResult)
	{
		// Read IMEI, SimID, Signal, etc.
		string imei = string.Empty;
		result = this.ImeiGetWorkflow(ref imei, true);
		if (result != FUNC_RESULT.SUCCESS) return result;

		// ... دیگر موارد ...
	}

	return FUNC_RESULT.SUCCESS;
}
```

## ۱۱. خلاصهٔ طراحی ریز

- **Layered with Clear Responsibilities**: راحت برای testing و maintenance.
- **Error-Centric, Not Exception-Centric**: تمام خطاها کنترل می‌شوند، exception نیست.
- **Atomic Base Operations**: هر Base فقط یک کار انجام می‌دهد.
- **Workflow for Orchestration**: تکرار و تأخیر در Workflow سطح بندی می‌شود.
- **State Machine for Control**: وضعیت دستگاه تصمیم‌گیری را رهبری می‌کند.
- **Lock-Free Synchronization**: Busy Flag از Interlocked استفاده می‌کند.
- **XXX Helpers for Consistency**: مقادیر و نتایج بر اساس الگوهای یکسان تنظیم می‌شوند.
