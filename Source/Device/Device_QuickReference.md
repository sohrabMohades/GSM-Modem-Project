# Device کلاس - خلاصه سریع (Quick Reference)

## 🔍 خطاهای فوری که نیاز به تصحیح دارند

### 1️⃣ **Busy Flag Logic - CRITICAL** ❌
```csharp
// فایل: Source\Device\Device.cs
// خطوط: 197-199
if (this._busyFlag == 0)
{
	return STATUS.BUSY;  // ❌ WRONG - Logic is inverted!
}

// ✅ درست:
if (this._busyFlag == 1)
{
	return STATUS.BUSY;
}
```

### 2️⃣ **TimeOut Method - CRITICAL** ❌
```csharp
// فایل: Source\Device\Device.cs
// خطوط: 928-937
private bool TimeOutEnd()
{
	if ((DateTime.Now - this._timeOutCounter).Seconds >= TIME_OUT_MAX_MS)
	{
		return true;  // ❌ WRONG - Seconds vs Milliseconds!
	}
}

// ✅ درست:
private bool TimeOutEnd()
{
	if ((DateTime.Now - this._timeOutCounter).TotalMilliseconds >= TIME_OUT_MAX_MS)
	{
		return true;
	}
}
```

### 3️⃣ **Typo - natinal** 🟡
```csharp
// فایل: Source\Device\Device.cs
// خطوط: 321-323
public string NationalMobileID
{
	get => this._natinalMobileID;  // ❌ "natinal" should be "national"
}
```

### 4️⃣ **Typo - ATCommnad** 🟡
```csharp
// خط: 379
public int ATCommnadDelay  // ❌ "Commnad" should be "Command"
```

### 5️⃣ **Incomplete Method - UpdateInternalStatus** 🟡
```csharp
// خطوط: 777-783
private void UpdateInternalStatus(string funcName, bool result)
{
	if (this.ErrorCode == ERROR_CODE.DLL_WRAPPER)
	{
		this._internalStatus = INTERNAL_STATUS.INVALID;
		return;
	}
	// ❌ باقی logic پیاده‌نشده است - نمی‌تواند خالی بماند
}
```

---

## 📚 معماری کلاس Device

### 1. سه‌لایه Status System:
```
ErrorCode (ما‌ذا شد؟)
	⬇️
InternalStatus (Device چی می‌تونه انجام بده؟)
	⬇️
ExternalStatus (DeviceManager چی می‌تونه درخواست کند؟)
```

### 2. Internal Status Flow:
```
INVALID
	↓
DEVICE_NOT_SELECT
	↓
HARDWARE_NOT_READY
	↓
SIM_NOT_READY
	↓
NETWORK_NOT_READY
	↓
READY ✅
```

### 3. External Status Mapping:
```
DISCONNECTED: INVALID or DLL_WRAPPER error
NOT_READY: Any intermediate internal status
READY: Full operational capability
BUSY: Operation in progress (_busyFlag = 1)
```

---

## 🔑 مفاهیم کلیدی

| مفهوم | توضیح | مثال |
|------|------|------|
| **Busy Flag** | Thread-safe operation lock | Interlocked (معمولاً 0=free, 1=busy) |
| **Device Number** | Immutable ID after Refresh | 0-127 |
| **Device Index** | Mutable position in list | 0-127 |
| **Internal Status** | Device capability checker | Controls which operations allowed |
| **Status Property** | External interface for Manager | BUSY, DISCONNECTED, NOT_READY, READY |

---

## ✅ Workflow ترجیح‌داده شده

```csharp
// 1. ایجاد Device
var device = new Device("C:\\path\\to\\dll.dll");

// 2. بررسی کنید Status صحیح است
if (device.Status == STATUS.DISCONNECTED)
{
	Console.WriteLine($"Error: {device.ErrorMessage}");
	return;
}

// 3. Discovery: تعداد و انتخاب
device.DeviceCountGet();
device.DeviceSelect(0);  // First device

// 4. Refresh: تمام اطلاعات به دست آوید
if (!device.DeviceRefresh())
{
	Console.WriteLine($"Refresh failed: {device.ErrorMessage}");
	return;
}

// 5. الآن آماده استفاده
Console.WriteLine($"IMEI: {device.IMEI}");
Console.WriteLine($"Signal: {device.Signal}");
Console.WriteLine($"Operator: {device.NetworkName}");

// 6. SMS ارسال کنید
device.SmsSendRetryTimes = 3;
device.SmsSendDeliveryReport = true;
device.SmsSend("+989123456789", "پیام تستی");

// 7. پاک کنید
device.Dispose();
```

---

## ⚠️ قوانین مهم

### Device Number قانون:
```
❌ WRONG:
device.DeviceRefresh();            // First refresh
device.DeviceNumberChange(5);     // FAIL - Too late!

✅ CORRECT:
device.DeviceNumberChange(5);    // Before refresh
device.DeviceRefresh();
```

### Busy Flag قانون:
```
❌ WRONG:
// Don't check _busyFlag directly from outside
if (device._busyFlag == 0) { }     // Private!

✅ CORRECT:
// Check Status property instead
if (device.Status == STATUS.BUSY)  // Good!
{
	Console.WriteLine("Device is busy");
}
```

### Async/Await قانون:
```
❌ WRONG:
device.DeviceRefresh().Wait();    // BLOCKING - Bad!

✅ CORRECT:
// Implement async wrapper if needed
// DeviceRefresh returns immediately
// Subscribe to DeviceOperationCompleted event
```

---

## 🛠️ توصیه‌های بهبود

### فوری (Do Now):
- [ ] Busy flag logic را تصحیح کنید (1 درجه)
- [ ] TimeOut method را بازنویسی کنید (2 درجه)  
- [ ] Typo ها را اصلاح کنید (3 درجه)

### میان‌مدت (Do Soon):
- [ ] UpdateInternalStatus کامل شود
- [ ] XML comments تکمیل شود
- [ ] Unit tests اضافه شود

### بلند‌مدت (Do Later):
- [ ] Thread safety full audit
- [ ] Error recovery mechanisms
- [ ] Performance optimization

---

## 📖 مثال کامل

```csharp
using GSM.Core.Source.Device;
using GSM.Core.Source.Common;
using static GSM.Core.Source.Common.DeviceDefinitions;

class Program
{
	static void Main()
	{
		// 1. Initialize
		Console.WriteLine("Initializing GSM Device...");
		var device = new Device("C:\\ModemLibraries\\ManshoorGsm.dll");

		if (device.Status == STATUS.DISCONNECTED)
		{
			Console.WriteLine($"❌ Initialization failed: {device.ErrorMessage}");
			return;
		}

		// 2. Discovery
		Console.WriteLine("\n📱 Device Discovery...");
		if (device.DeviceCountGet(out int count))
		{
			Console.WriteLine($"✅ Found {count} modems");
		}

		// 3. Select and Refresh
		Console.WriteLine("\n🔧 Selecting and refreshing device...");
		device.DeviceSelect(0);
		device.DeviceNumberChange(0);

		if (!device.DeviceRefresh())
		{
			Console.WriteLine($"❌ Refresh failed: {device.ErrorMessage}");
			return;
		}

		// 4. Display Info
		Console.WriteLine("\n📋 Device Information:");
		Console.WriteLine($"   IMEI: {device.IMEI}");
		Console.WriteLine($"   Model: {device.DeviceModel}");
		Console.WriteLine($"   Software: {device.SoftwareModel}");
		Console.WriteLine($"   Network: {device.NetworkName}");
		Console.WriteLine($"   Signal: {device.Signal}%");
		Console.WriteLine($"   SIM Status: {device.SimStatus}");

		// 5. Send SMS
		Console.WriteLine("\n📤 Sending SMS...");
		device.SmsSendRetryTimes = 2;
		device.SmsSendDeliveryReport = true;

		if (device.SmsSend("+989123456789", "Hello from GSM!"))
		{
			Console.WriteLine("✅ SMS queued");
			// Wait for DeviceOperationCompleted event
		}
		else
		{
			Console.WriteLine($"❌ Send failed: {device.ErrorMessage}");
		}

		// 6. Cleanup
		Console.WriteLine("\n🧹 Cleaning up...");
		device.Dispose();
		Console.WriteLine("✅ Done");
	}
}
```

---

## 📞 Support References

- **فایل توثیق کامل**: `Device_Documentation.md`
- **XML Comments Reference**: `PublicMethodsDocumentation.cs`
- **Architecture Doc**: `Docs\Architecture\GSM_Architecture_v1.1_FA.docx`

---

**آخرین بروزرسانی**: 2024
**نسخه**: 1.0

