# معماری Device

## ۱. مقدمه و نمای کلی

کلاس `Device` یک تجزیهٔ شده (partial class) است که با چند فایل دیگر تشکیل یک سیستم مدیریت دستگاه GSM را می‌دهد. این کلاس مسئول ارتباط با مودم‌های GSM، مدیریت حالت دستگاه، نظارت بر buffer‌های دریافتی، و انجام عملیات هایی مانند ارسال SMS و درخواست USSD است.

## ۲. معماری لایه‌ای

معماری کلاس `Device` بر اساس نمودار لایه‌ای زیر ساخته شده است:

```
┌─────────────────────────────────┐
│    Controller Layer             │
│  - DeviceCountGet               │
│  - DeviceIdentity               │
│  - DeviceRefresh                │
│  - SmsSend                       │
└─────────────┬───────────────────┘
			  │
	  ┌───────┴──────────┐
	  │                  │
	  ▼                  ▼
ComplexWorkflow      Workflow
	  │                  │
	  │         ┌────────┤
	  │         │        │
	  └────────►│        │
	 ┌──────────┘        │
	 │              ┌────┴────┐
	 ▼              ▼         ▼
 Workflow        Execute  Workflow
	 │              │
	 └──────┬───────┘
			▼
		  Base
	 ┌──────┴──────┐
	 ▼             ▼
  DllWrapper   DataValidation
	 │
	 ▼
  Modem(DLL)
```

### ۲.۱ لایه‌های معماری

| لایه | مسئولیت‌ها | شامل |
|---|---|---|
| **Controller** | - بازدهی تمام اپریشن‌های درخواستی<br>- کنترل Busy Flag<br>- بررسی Internal Status<br>- بروزرسانی Flags و Counters<br>- ایجاد Event هایی | DeviceControllers.cs |
| **ComplexWorkflow** | - تنسیق چندین Workflow<br>- مدیریت شاخه‌های شرطی<br>- هماهنگی عملیات چند‌مرحله‌ای | DeviceCommonOperations.cs<br>DeviceBuffer.cs |
| **Workflow** | - مدیریت تأخیر‌ها (delay)<br>- منطق retry<br>- تأیید چندگانه (multi-read)<br>- نظارت timeout | DeviceCommonOperations.cs<br>DeviceProperties.cs<br>DeviceIdentity.cs |
| **Execute** | - کنترل ترتیب عملیات<br>- اعمال تأخیر بین دستورات | تمام *Workflow متد‌ها |
| **Base** | - عملیات اتمی (atomic)<br>- اعتبارسنجی ورودی<br>- اعتبارسنجی خروجی<br>- فراخوانی DLL | متد‌های پایانی *Base |
| **DllWrapper** | - دسترسی مستقیم DLL<br>- بدون منطق دستگاه | DllWrapper.cs |

## ۳. وضعیت‌های داخلی (Internal Status)

کلاس `Device` شامل یک ماشین‌ حالت است که کنترل می‌کند کدام عملیات‌ها در هر لحظه قابل انجام هستند:

```csharp
enum INTERNAL_STATUS
{
	INVALID = 0,           // دستگاه نمی‌تواند کار کند
	BASIC_LEVEL = 1,       // فقط عملیات شناسایی و ابتدایی
	SINGLE_DEVICE = 2,     // انتخاب شده، عملیات عمومی
	WORKING_MODE = 3,      // اتصال SIM و شبکه برقرار
	FULL_MODE = 4          // تمام عملیات ممکن
}
```

**رابطه عملیات‌ها با وضعیت‌ها:**

| عملیات | حالت مورد نیاز | نتیجه فعال‌سازی |
|---|---|---|
| `DeviceCountGet` | ≥ BASIC_LEVEL | تعداد دستگاه‌ها |
| `DeviceIdentity` | = BASIC_LEVEL | شناسایی دستگاه |
| `DeviceNumberChange` | = BASIC_LEVEL | تغییر شماره |
| `DeviceRefresh` | ≥ SINGLE_DEVICE | بروزرسانی اطلاعات |
| `SmsSend` | ≥ SINGLE_DEVICE | ارسال پیام |

## ۴. قوانین مدیریت حالت (State Management)

### ۴.۱ Busy Flag

```csharp
Busy Flag
├── بررسی تنها در Controller
├── تنظیم تنها در Controller
└── پاک‌سازی تنها در Controller
```

**کاربرد:** جلوگیری از بروز عملیات‌های concurrent. به صورت atomic با `Interlocked` کنترل می‌شود.

### ۴.۲ Error Code و Error Message

```csharp
// تنهایی نتیجهٔ آخرین خطا را نشان دهد
ERROR_CODE ErrorCode { get; }
string ErrorMessage { get; }
```

**نکته:** Error Code حالت دستگاه را تعریف نمی‌کند؛ فقط برای logging و debugging است.

### ۴.۳ Flags و Counters

```csharp
class DeviceRuntimeFlags  // DeviceTypes.cs
{
	NeedRefresh              // نیاز به بروزرسانی
	NeedSimChange            // نیاز به تغییر SIM
	LowCredit                // اعتبار کم
	AutoRefreshEnabled       // بروزرسانی خودکار
	AutoSmsSendEnabled       // ارسال خودکار
	AutoInboxEnabled         // صندوق دریافتی خودکار
}

class DeviceRuntimeInfo  // DeviceTypes.cs
{
	RefreshFailConsecutive   // خرابی متوالی refresh
	RefreshFailTotal         // کل خرابی refresh
	TimeoutConsecutive       // تایم‌اوت متوالی
	LastRefreshTime          // آخرین زمان refresh
	// ...
}
```

## ۵. روابط بین لایه‌ها

### ۵.۱ قوانین فراخوانی

```
Controller ──┐
			 ├──► ComplexWorkflow
			 │         │
			 │         └──► Workflow ──┐
			 │                         │
			 └────────► Workflow ──────┤
									   │
									   └──► Execute
											   │
											   └──► Base
													 │
													 └──► DllWrapper
```

**قوانین سختگیرانه:**

| فراخوانی | مجاز | ممنوع |
|---|---|---|
| Controller | Workflow, ComplexWorkflow | Base, Execute, Controller |
| ComplexWorkflow | Workflow | Controller, ComplexWorkflow, Execute, Base |
| Workflow | Execute, Workflow (محدود) | Controller, ComplexWorkflow |
| Execute | Base | Controller, ComplexWorkflow, Workflow, Execute |
| Base | DllWrapper | هر چیز دیگری |

### ۵.۲ مسئولیت‌های هر لایه

**Controller:**
- تنها لایهٔ مجاز برای تغییر Internal Status
- تنها لایهٔ مجاز برای تغییر Busy Flag
- تنها لایهٔ مجاز برای reset Error
- تنها لایهٔ مجاز برای ایجاد Event

**ComplexWorkflow:**
- مدیریت چندین Workflow متوالی
- کنترل شاخه‌سازی (branching)
- منطق fallback

**Workflow:**
- retry logic
- delay management
- multi-read validation
- timeout monitoring
- **نمی‌تواند:** Status تغییر دهد، Busy Flag عوض کند، Event ایجاد کند

**Base:**
- عملیات اتمی (atomic)
- اعتبارسنجی ورودی
- اعتبارسنجی خروجی
- تنها قسمتی که DLL را فراخوانی می‌کند

## ۶. الگوی Workflow

هر Workflow با این الگو پیروی می‌کند:

```
Workflow
	│
	├─► InputValidation
	│
	├─► Execute (Execute متد)
	│       │
	│       ├─► Delay
	│       │
	│       └─► Base (Base متد)
	│
	├─► Check (اعتبارسنجی خروجی)
	│
	└─► CommitValues (تنظیم خروجی‌ها و مقادیر داخلی)
```

**مثال: CharModeSetWorkflow**

```csharp
private FUNC_RESULT CharModeSetWorkflow(SMS_CHAR_MODE CharMode, bool InternalValueSet)
{
	FUNC_RESULT result = FUNC_RESULT.SUCCESS;

	// Execute
	result = this.CharModeSetExecute(result, CharMode, (int)OPERATION_DELAY_MS.NORMAL_SHORT);

	// CommitValues
	this.XXXSetValue(result, CharMode, SMS_CHAR_MODE.UNKHOWN, 
					 ref CharMode, ref this._runtimeWorkingConfig.SmsCharMode, InternalValueSet);

	return result;
}
```

## ۷. الگوی Base

هر Base متد با این الگو پیروی می‌کند:

```
Base
	├─► InputCheck
	├─► DllCall
	├─► DllErrorCheck
	├─► OutputValidation
	└─► SetOutputValues
```

**مثال: CharModeSetBase**

```csharp
private FUNC_RESULT CharModeSetBase(SMS_CHAR_MODE CharMode)
{
	FUNC_RESULT result = FUNC_RESULT.SUCCESS;

	// InputCheck
	result = this.CharModeSetBaseInputCheck(result, CharMode);

	// DllCall + DllErrorCheck
	result = this.CharModeSetBaseDll(result, CharMode);

	return result;
}
```

## ۸. مدیریت Buffer

Buffer داخلی برای ذخیره پیام‌های دریافتی از مودم استفاده می‌شود:

```csharp
class DeviceRuntimeBuffer  // DeviceTypes.cs
{
	List<BufferRawData> BufferDataRaw;      // پیام‌های خام بدون تحلیل
	List<BufferDataGroup> BufferDataGroup;  // پیام‌های تحلیل‌شده
	int BufferLastIndex;                     // آخرین شاخص
	bool BufferNewData;                      // آیا داده جدید وجود دارد؟
	// ...
}
```

**عملیات Buffer:**
1. `BufferClearWorkflow`: پاک‌سازی Buffer
2. `BufferLastIndexGetWorkflow`: دریافت شاخص جدید
3. `BufferMessageGetWorkflow`: دریافت پیام جدید
4. `BufferUpdate`: تجزیهٔ و تحلیل پیام‌ها

## ۹. خروجی و کدهای نتیجه

```csharp
enum FUNC_RESULT
{
	UNKNOWN = 0,
	SUCCESS = 1,
	FAIL = 2,
	MAJOR_ERROR = 3
}

enum ERROR_CODE
{
	NO_ERROR = 0,
	DLL_WRAPPER = 1,
	BUSY = 2,
	INTERNAL_STATUS = 3,
	INVALID_INPUT = 4,
	DEVICE_COUNT = 5,
	// ... بیش‌تر
}
```

## ۱۰. روند اجرای یک Controller

```
Public Controller Method (e.g., DeviceCountGet)
	│
	├─► TrySetBusy() ─────► بازگرد false اگر قبلاً Busy
	│
	├─► InternalStatusCheck() ─────► بازگرد false اگر وضعیت نامناسب
	│
	├─► ErrorReset() ─────► پاک‌سازی خطای قبلی
	│
	├─► Workflow Call (e.g., DeviceCountGetWorkflow)
	│       │
	│       └─ Execute/Base/DllWrapper
	│
	├─► InternalStatusUpdate() ─────► تحدیث وضعیت بر اساس نتیجه
	│
	├─► Flag/Counter Updates
	│
	└─► Finally: DeviceBusyDeactive() ─────► آزادکردن Busy Flag
		└─► Return FuncResult(funcResult)
```

## ۱۱. خصوصیات و Properties

کلاس `Device` خصوصیات‌ عمومی کشف می‌دهد:

**شناسایی دستگاه:**
- `DeviceCount`, `DeviceIndex`, `DeviceNumber`

**وضعیت سخت‌افزار:**
- `GsmPower`, `GsmIsConnect`, `GsmIsReady`

**اطلاعات دستگاه:**
- `IMEI`, `SoftwareModel`, `HardwareModel`, `SimStatus`

**اطلاعات شبکه:**
- `NetworkName`, `Signal`, `SMSCenter`

**اطلاعات اپراتور:**
- `MobileNumber`, `NationalMobileID`

**پیکربندی SMS:**
- `SMSType`, `SMSCharMode`, `SmsSendDeliveryReport`, `SmsSendRetryTimes`

**Buffer:**
- `BufferNewMessage`, `BufferLastIndex`, `BufferMessage`

## ۱۲. خطاها و بازیابی خرابی‌ها

**منابع خطا:**

1. **DLL Errors**: از DllWrapper
2. **Validation Errors**: در Base و Workflow
3. **Workflow Errors**: ناموفقی retry یا timeout
4. **Status Errors**: وضعیت نامناسب

**مدیریت خطا:**
- تمام خطاها در `ErrorCode` و `ErrorMessage` ثبت می‌شوند.
- هر فراخوانی Controller جدید قبل از شروع کار `ErrorReset()` صدا می‌کند.
- Workflow‌ها تأخیر و retry ارائه می‌دهند.

## ۱۳. ساختار فایل‌ها

```
Source/Device/
├── Device.cs                    # شامل: قوانین و constans
├── DeviceRuntime.cs             # شامل: ErrorReset, BusyFlag, StatusCheck
├── DeviceControllers.cs         # شامل: متد‌های public (Controller)
├── DeviceCommonOperations.cs    # شامل: ComplexWorkflow + Workflow
├── DeviceProperties.cs          # شامل: Workflow برای خصوصیات
├── DeviceIdentity.cs            # شامل: Workflow برای شناسایی
├── DeviceSendOperations.cs      # شامل: Workflow برای ارسال
├── DeviceBuffer.cs              # شامل: Workflow برای Buffer
└── DeviceTypes.cs               # شامل: تمام داده‌های داخلی
```

## ۱۴. خلاصهٔ معماری

- **Layered Architecture**: کنترل رفتار و جریان که توسط لایه‌های مختلف جداگانه شده است.
- **State Machine**: وضعیت‌های مختلف برای کنترل عملیات‌های مجاز.
- **Error-Centric Design**: خطاها داخل کدها مدیریت می‌شوند، نه Exception.
- **Atomic Operations**: Base لایه عملیات اتمی انجام می‌دهد.
- **Retry & Validation**: Workflow لایه retry و اعتبارسنجی چندگانه انجام می‌دهد.
- **Buffer Management**: پیام‌های دریافتی در Buffer ذخیره و تحلیل می‌شوند.
