# 📝 نمونه پیاده‌سازی Logging در Device

## گام 1: صحت اضافه کردن Namespace

فایل: `Source/Device/Device.cs`

```csharp
// در بالای فایل، بعد از using های دیگر:

using GSM.Core.Source.Common;
using GSM.Core.Source.DllWrapper;
using GSM.Core.Source.Device.Logging;  // ✅ اضافه شد
using System;
using System.Collections.Generic;
...
```

✅ **انجام شد!**

---

## گام 2: اضافه کردن Private Members

فایل: `Source/Device/Device.cs`

**جایی که `_dllWrapper` و سایر private members تعریف شده‌اند، اضافه کنید:**

```csharp
// بعد از سایر private members:

private DeviceLogger _logger;
private ErrorTracer _errorTracer;
```

---

## گام 3: Initialize Logger در Constructor

فایل: `Source/Device/Device.cs`

```csharp
public Device(string DllFileName)
{
	this.ErrorReset();
	this._dllFileName = DllFileName;

	// ✅ اضافه شد - Initialize Logger
	try
	{
		LoggingInitializer.Initialize();  // یک بار در شروع
		this._logger = DeviceLogger.GetInstance(DllFileName);
		this._errorTracer = ErrorTracer.GetInstance(this._logger);
		this._logger.OperationStart("Device.Constructor", new { DllFileName });
	}
	catch (Exception ex)
	{
		System.Diagnostics.Debug.WriteLine($"Logger initialization failed: {ex}");
	}

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

	// ✅ اضافه شد - Log completion
	if (this._logger != null)
	{
		this._logger.OperationComplete(
			new LogContext { OperationName = "Device.Constructor", StartTime = DateTime.UtcNow },
			"SUCCESS",
			new { Result = result });
	}
}
```

---

## گام 4: اضافه کردن Logging به ErrorSet()

فایل: `Source/Device/DeviceRuntime.cs`

```csharp
private bool ErrorSet(ERROR_CODE ErrorCode, string ErrorMessage)
{
	this._state.ErrorCode = ErrorCode;
	this._state.ErrorMessage = ErrorMessage;

	// ✅ اضافه شد - Error Tracing
	if (this._errorTracer != null)
	{
		this._errorTracer.RecordError(ErrorCode, ErrorMessage, "ErrorSet", 1);
	}

	return false;
}
```

---

## گام 5: اضافه کردن Logging به DllWrapperHasError()

فایل: `Source/Device/DeviceRuntime.cs`

```csharp
private FUNC_RESULT DllWrapperHasError(FUNC_RESULT funcResult)
{
	try
	{
		if (this._dllWrapper.ErrorCode == DllWrapper.ERROR_CODE.NO_ERROR)
			return funcResult;
		else
		{
			this.DllWrapperErrorCheckSetValue(this._dllWrapper.ErrorMessage);

			// ✅ اضافه شد - Log DLL Error
			if (this._logger != null)
			{
				this._logger.LogDllResult("DllWrapper", "ERROR", 0);
			}

			return FUNC_RESULT.MAJOR_ERROR;
		}
	}
	catch (Exception ex)
	{
		this.DllWrapperErrorCheckSetValue(ex.Message);

		// ✅ اضافه شد - Log Exception
		if (this._logger != null)
		{
			this._logger.LogError("DLL_ERROR", ex.Message, "DllWrapperHasError", ex);
		}

		return FUNC_RESULT.MAJOR_ERROR;
	}
}
```

---

## گام 6: استفاده Extension Methods (اختیاری - بهترین روش!)

### نمونه 1: DeviceCountGet()

**قبل:**
```csharp
public bool DeviceCountGet()
{
	int deviceCount = DeviceDefinitions.NUMBER_INVALID;
	return this.DeviceCountGet(ref deviceCount, true);
}
```

**بعد (Option A - Manual Logging):**
```csharp
public bool DeviceCountGet()
{
	var logger = this._logger ?? DeviceLogger.GetInstance(this._dllFileName);
	var context = logger.OperationStart("DeviceCountGet");

	try
	{
		int deviceCount = DeviceDefinitions.NUMBER_INVALID;
		bool result = this.DeviceCountGet(ref deviceCount, true);

		logger.OperationComplete(context, "SUCCESS", new { DeviceCount = deviceCount });
		return result;
	}
	catch (Exception ex)
	{
		logger.OperationFailed(context, ex);
		throw;
	}
}
```

**بعد (Option B - Using Extension):**
```csharp
public bool DeviceCountGet()
{
	return this.LogOperation(() =>
	{
		int deviceCount = DeviceDefinitions.NUMBER_INVALID;
		return this.DeviceCountGet(ref deviceCount, true);
	}, "DeviceCountGet");
}
```

---

### نمونه 2: DeviceIdentity()

**قبل:**
```csharp
public bool DeviceIdentity(int DeviceIndex, ref int DeviceNumber, ref string IMEI)
{
	FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
	if (!this.TrySetBusy())
	{
		this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceIdentity)); return false;
	}
	if (!this.InternalStatusCheck(INTERNAL_STATUS.BASIC_LEVEL, nameof(this.DeviceIdentity)))
	{
		this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.DeviceIdentity)); return false;
	}
	try
	{
		this.ErrorReset();
		funcResult = this.DeviceIdentityComplexWorkflow(DeviceIndex, ref DeviceNumber, ref IMEI);
		this.InternalStatusUpdate();
		return this.FuncResult(funcResult);
	}
	finally
	{
		this.DeviceBusyDeactive();
	}
}
```

**بعد (با Logging):**
```csharp
public bool DeviceIdentity(int DeviceIndex, ref int DeviceNumber, ref string IMEI)
{
	var logger = this._logger ?? DeviceLogger.GetInstance(this._dllFileName);
	var context = logger.OperationStart("DeviceIdentity", new { DeviceIndex });

	FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
	if (!this.TrySetBusy())
	{
		logger.LogWarning("Device is busy", "DeviceIdentity");
		this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceIdentity)); 
		return false;
	}
	if (!this.InternalStatusCheck(INTERNAL_STATUS.BASIC_LEVEL, nameof(this.DeviceIdentity)))
	{
		logger.LogWarning("Invalid internal status", "DeviceIdentity");
		this.ErrorSet(ERROR_CODE.INTERNAL_STATUS, nameof(this.DeviceIdentity)); 
		return false;
	}
	try
	{
		this.ErrorReset();
		funcResult = this.DeviceIdentityComplexWorkflow(DeviceIndex, ref DeviceNumber, ref IMEI);
		this.InternalStatusUpdate();

		bool result = this.FuncResult(funcResult);
		logger.OperationComplete(context, funcResult.ToString(), new { DeviceNumber, IMEI });
		return result;
	}
	catch (Exception ex)
	{
		logger.OperationFailed(context, ex);
		throw;
	}
	finally
	{
		this.DeviceBusyDeactive();
	}
}
```

---

## 🎯 Quick Reference - تمام تغییرات مورد نیاز

### فایلی که باید ایجاد شود:
- ✅ `Source/Device/Logging/DeviceLogger.cs`
- ✅ `Source/Device/Logging/LogContext.cs`
- ✅ `Source/Device/Logging/ErrorTracer.cs`
- ✅ `Source/Device/Logging/LoggingExtensions.cs`
- ✅ `Source/Device/Logging/LoggingInitializer.cs`

### فایلی که باید اصلاح شود (Source/Device/Device.cs):
```diff
+ using GSM.Core.Source.Device.Logging;

+ private DeviceLogger _logger;
+ private ErrorTracer _errorTracer;

  public Device(string DllFileName)
  {
	  this.ErrorReset();
	  this._dllFileName = DllFileName;

+     LoggingInitializer.Initialize();
+     this._logger = DeviceLogger.GetInstance(DllFileName);
+     this._errorTracer = ErrorTracer.GetInstance(this._logger);

	  // existing code...
  }
```

### فایلی که باید اصلاح شود (Source/Device/DeviceRuntime.cs):
```diff
  private bool ErrorSet(ERROR_CODE ErrorCode, string ErrorMessage)
  {
	  this._state.ErrorCode = ErrorCode;
	  this._state.ErrorMessage = ErrorMessage;
+     this._errorTracer?.RecordError(ErrorCode, ErrorMessage, "ErrorSet", 1);
	  return false;
  }

  private FUNC_RESULT DllWrapperHasError(FUNC_RESULT funcResult)
  {
	  try
	  {
		  if (this._dllWrapper.ErrorCode == DllWrapper.ERROR_CODE.NO_ERROR)
			  return funcResult;
		  else
		  {
			  this.DllWrapperErrorCheckSetValue(this._dllWrapper.ErrorMessage);
+             this._logger?.LogDllResult("DllWrapper", "ERROR", 0);
			  return FUNC_RESULT.MAJOR_ERROR;
		  }
	  }
	  catch (Exception ex)
	  {
		  this.DllWrapperErrorCheckSetValue(ex.Message);
+         this._logger?.LogError("DLL_ERROR", ex.Message, "DllWrapperHasError", ex);
		  return FUNC_RESULT.MAJOR_ERROR;
	  }
  }
```

---

## 📊 نتیجهٔ Logs

**بعد از اجرا:**

```
logs/device-Device-.txt
├── 2024-01-15 10:30:45.123 +00:00 [INF] 🟢 Logger initialized for Device: Device
├── 2024-01-15 10:30:45.125 +00:00 [INF] ⏹️ Operation START | Operation=Device.Constructor | CorrelationId=A1B2C3D4E5F6 | Params={DllFileName: "Device.dll"}
├── 2024-01-15 10:30:45.250 +00:00 [INF] ✅ Operation COMPLETE | Operation=Device.Constructor | CorrelationId=A1B2C3D4E5F6 | Status=SUCCESS | Duration=125ms | Result={Result: true}
├── 2024-01-15 10:30:46.100 +00:00 [INF] ⏹️ Operation START | Operation=DeviceCountGet | CorrelationId=F1E2D3C4B5A6
├── 2024-01-15 10:30:46.234 +00:00 [INF] ✅ Operation COMPLETE | Operation=DeviceCountGet | CorrelationId=F1E2D3C4B5A6 | Status=SUCCESS | Duration=134ms | Result={Result: true}
└── ...
```

---

## ✅ Checklist پیاده‌سازی

- [ ] Serilog NuGet installed
- [ ] Logging folder ایجاد شد
- [ ] 5 فایل Logging ایجاد شد
- [ ] Device.cs: using اضاف شد
- [ ] Device.cs: private members اضاف شد
- [ ] Device.cs: constructor اصلاح شد
- [ ] DeviceRuntime.cs: ErrorSet اصلاح شد
- [ ] DeviceRuntime.cs: DllWrapperHasError اصلاح شد
- [ ] یک یا دو Controller method اصلاح شد
- [ ] Build successful
- [ ] Log files creating و writing
- [ ] JSON logs readable

---

## 🚀 تست فوری

```csharp
// در Program.cs یا Main:
var device = new Device("device.dll");

// LoggingInitializer خودکار initialize شود
// اما می‌تواند دستی هم صدا زده شود:
LoggingInitializer.Initialize();

// اکنون:
// ✅ logs/ folder ایجاد شد
// ✅ device-device.dll-.txt و device-device.dll-structured-.json ایجاد شد
// ✅ تمام عملیات logged می‌شوند
```

