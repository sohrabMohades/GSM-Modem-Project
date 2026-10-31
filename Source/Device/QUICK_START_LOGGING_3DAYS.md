# 🚀 پلن سریع لاگینگ - 3 روز
## Quick Start Logging Implementation Plan

**تاریخ شروع:** امروز
**مدت زمان:** 3 روز
**هدف:** Structured Logging + Error Tracing
**محدودیت:** حداقل تغییر در کدهای موجود

---

## 📋 خلاصه تصمیم‌های نهایی شما

| موضوع | انتخاب |
|-------|--------|
| **Library لاگینگ** | ✅ Serilog |
| **ذخیره‌سازی** | ✅ File-based (JSON) |
| **مدت نگهداری** | ✅ نامحدود |
| **سطح جزئیات** | ✅ Comprehensive + Configurable |
| **Performance** | ✅ Minimal changes - فقط شروع/پایان/return |
| **Scope اولیه** | ✅ Structured Logging + Error Tracing فقط |
| **Timeline** | ✅ 3 روز |

---

## 🎯 معماری پیشنهادی - "Minimalist Approach"

```
موجود کد:
	Controller -> Workflow -> Execute -> Base -> DLL

نیاز:
	❌ تغییر تمام متدها

راه‌حل:
	✅ Decorator Pattern + Static Logger
	✅ فقط "Entry" و "Exit" points لاگ شوند
	✅ Exception handling خودکار
	✅ Zero code changes در توابع!
```

---

## 📦 ساختار فایل‌های جدید

```
Source/Device/Logging/
├── 1. DeviceLogger.cs           [Main Logger - Entry Point]
├── 2. LogContext.cs             [Context for tracking]
├── 3. ErrorTracer.cs            [Error Recording]
├── 4. LoggingDecorator.cs       [Decorator for minimal changes]
└── appsettings.json             [Configuration]

Source/Device/
├── Device.cs                     [فقط 2-3 خط اضافه در شروع]
└── [سایر فایل‌ها - بدون تغییر]
```

---

## 🗓️ تقسیم‌بندی 3 روزه

### **روز 1: Infrastructure (4-6 ساعت)**
```
✅ Serilog Setup
✅ DeviceLogger.cs
✅ LogContext.cs
✅ appsettings.json
✅ Initialization in Device.cs
```

### **روز 2: Error Tracking (4-6 ساعت)**
```
✅ ErrorTracer.cs
✅ ErrorContext.cs
✅ Integration with ErrorSet()
✅ Error Log File Creation
```

### **روز 3: Testing & Configuration (4-6 ساعت)**
```
✅ تست تمام عملیات
✅ Verify Log Files
✅ Configuration Tuning
✅ Documentation
```

---

## 📐 ارکیتکچر دقیق

### **الگوی Decorator - Zero Changes**

```csharp
// مثال استفاده - بدون تغییر تابع اصلی!
public bool DeviceCountGet()
{
	// == شروع لاگ (خط اول) ==
	var logger = DeviceLogger.GetInstance("DEV-001");
	var opContext = logger.OperationStart("DeviceCountGet", new { });

	try 
	{
		// اصلی کد - بدون تغییر
		int deviceCount = DeviceDefinitions.NUMBER_INVALID;
		bool result = this.DeviceCountGet(ref deviceCount, true);

		// == پایان لاگ (خط آخر) ==
		logger.OperationComplete(opContext, "SUCCESS", new { Result = result });
		return result;
	}
	catch (Exception ex)
	{
		logger.OperationFailed(opContext, ex);
		throw;
	}
}

// اختیاری: استفاده از Action Wrapper برای تمام متدها
// device.LogOperation(() => device.DeviceCountGet(), "DeviceCountGet");
```

### **الگو: Extension Methods (Alternative)**

```csharp
// زیاد بهتر - هیچ تغییری در کد اصلی!
public static class LoggingExtensions
{
	public static T LogOperation<T>(this Device device, 
									Func<T> operation, 
									string operationName)
	{
		var logger = DeviceLogger.GetInstance(device.DeviceId);
		var context = logger.OperationStart(operationName, new { });

		try
		{
			var result = operation.Invoke();
			logger.OperationComplete(context, "SUCCESS");
			return result;
		}
		catch (Exception ex)
		{
			logger.OperationFailed(context, ex);
			throw;
		}
	}
}

// استفاده:
device.LogOperation(() => device.DeviceCountGet(), "DeviceCountGet");
```

---

## 💻 کد پیاده‌سازی

### **1️⃣ DeviceLogger.cs** (Serilog wrapper)

```csharp
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace GSM.Core.Source.Device.Logging
{
	public class DeviceLogger : IDisposable
	{
		private static readonly Dictionary<string, DeviceLogger> _instances = new();
		private readonly ILogger _logger;
		private readonly string _deviceId;

		private DeviceLogger(string deviceId)
		{
			_deviceId = deviceId;

			// Serilog Configuration
			_logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.File(
					path: $"logs/device-{deviceId}-.txt",
					outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
					rollingInterval: RollingInterval.Day,
					fileSizeLimitBytes: 10485760, // 10MB
					retainedFileCountLimit: null) // نامحدود
				.WriteTo.File(
					formatter: new CompactJsonFormatter(),
					path: $"logs/device-{deviceId}-structured-.json",
					rollingInterval: RollingInterval.Day,
					fileSizeLimitBytes: 10485760,
					retainedFileCountLimit: null)
				.Enrich.FromLogContext()
				.Enrich.WithProperty("DeviceId", deviceId)
				.CreateLogger();
		}

		public static DeviceLogger GetInstance(string deviceId)
		{
			if (!_instances.ContainsKey(deviceId))
			{
				_instances[deviceId] = new DeviceLogger(deviceId);
			}
			return _instances[deviceId];
		}

		// ===== Operation Lifecycle =====
		public LogContext OperationStart(string operationName, object parameters = null)
		{
			var correlationId = Guid.NewGuid().ToString("N").Substring(0, 12);
			var context = new LogContext
			{
				CorrelationId = correlationId,
				OperationName = operationName,
				StartTime = DateTime.UtcNow,
				Parameters = parameters
			};

			_logger.Information(
				"⏹️ Operation START | Operation={Operation} | CorrelationId={CorrelationId} | Params={@Params}",
				operationName, correlationId, parameters ?? new { });

			return context;
		}

		public void OperationComplete(LogContext context, string status, object result = null)
		{
			var duration = DateTime.UtcNow - context.StartTime;

			_logger.Information(
				"✅ Operation COMPLETE | Operation={Operation} | CorrelationId={CorrelationId} | Status={Status} | Duration={DurationMs}ms | Result={@Result}",
				context.OperationName, context.CorrelationId, status, duration.TotalMilliseconds, result ?? new { });
		}

		public void OperationFailed(LogContext context, Exception ex)
		{
			var duration = DateTime.UtcNow - context.StartTime;

			_logger.Error(ex,
				"❌ Operation FAILED | Operation={Operation} | CorrelationId={CorrelationId} | Duration={DurationMs}ms | Error={ErrorMessage}",
				context.OperationName, context.CorrelationId, duration.TotalMilliseconds, ex.Message);
		}

		// ===== Error Logging =====
		public void LogError(string errorCode, string errorMessage, string operation, Exception ex = null)
		{
			_logger.Error(ex,
				"🔴 ERROR | Code={ErrorCode} | Message={ErrorMessage} | Operation={Operation}",
				errorCode, errorMessage, operation);
		}

		public void LogWarning(string message, string operation)
		{
			_logger.Warning(
				"⚠️ WARNING | Message={Message} | Operation={Operation}",
				message, operation);
		}

		// ===== DLL Logging =====
		public void LogDllCall(string methodName, Dictionary<string, object> parameters)
		{
			_logger.Debug(
				"🔗 DLL CALL | Method={Method} | Params={@Params}",
				methodName, parameters ?? new { });
		}

		public void LogDllResult(string methodName, object result, long durationMs)
		{
			_logger.Debug(
				"🔗 DLL RESULT | Method={Method} | Duration={DurationMs}ms | Result={@Result}",
				methodName, durationMs, result ?? new { });
		}

		// ===== State Changes =====
		public void LogStateChange(string oldState, string newState, string reason)
		{
			_logger.Information(
				"🔄 STATE CHANGE | From={OldState} | To={NewState} | Reason={Reason}",
				oldState, newState, reason);
		}

		public void Dispose()
		{
			Log.CloseAndFlush();
		}
	}

	// JSON Formatter for compact logs
	public class CompactJsonFormatter : ITextFormatter
	{
		public void Format(LogEvent logEvent, TextWriter output)
		{
			var json = new
			{
				Timestamp = logEvent.Timestamp.DateTime,
				Level = logEvent.Level.ToString(),
				Message = logEvent.RenderMessage(),
				Properties = logEvent.Properties,
				Exception = logEvent.Exception?.ToString()
			};

			output.WriteLine(System.Text.Json.JsonSerializer.Serialize(json));
		}
	}
}
```

### **2️⃣ LogContext.cs**

```csharp
using System;
using System.Collections.Generic;

namespace GSM.Core.Source.Device.Logging
{
	public class LogContext
	{
		public string CorrelationId { get; set; }
		public string OperationName { get; set; }
		public DateTime StartTime { get; set; }
		public object Parameters { get; set; }
		public Dictionary<string, object> Metadata { get; set; } = new();

		public override string ToString()
		{
			return $"[{CorrelationId}] {OperationName}";
		}
	}
}
```

### **3️⃣ ErrorTracer.cs**

```csharp
using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSM.Core.Source.Device.Logging
{
	public class ErrorTracer
	{
		private static ErrorTracer _instance;
		private readonly Queue<ErrorRecord> _errorHistory = new();
		private readonly DeviceLogger _logger;
		private const int MAX_HISTORY = 1000;

		private ErrorTracer(DeviceLogger logger)
		{
			_logger = logger;
		}

		public static ErrorTracer GetInstance(DeviceLogger logger)
		{
			return _instance ?? (_instance = new ErrorTracer(logger));
		}

		public class ErrorRecord
		{
			public DateTime Timestamp { get; set; }
			public string ErrorCode { get; set; }
			public string ErrorMessage { get; set; }
			public string Operation { get; set; }
			public Exception Exception { get; set; }
			public int Layer { get; set; } // Controller=1, Workflow=2, Base=3, DLL=4
		}

		public void RecordError(ERROR_CODE code, string message, string operation, int layer = 1, Exception ex = null)
		{
			var record = new ErrorRecord
			{
				Timestamp = DateTime.UtcNow,
				ErrorCode = code.ToString(),
				ErrorMessage = message,
				Operation = operation,
				Layer = layer,
				Exception = ex
			};

			_errorHistory.Enqueue(record);

			// محدود کردن اندازه تاریخچه
			if (_errorHistory.Count > MAX_HISTORY)
				_errorHistory.Dequeue();

			// لاگ کردن
			_logger.LogError(code.ToString(), message, operation, ex);
		}

		public List<(string Code, int Count, string LastMessage)> GetErrorPatterns()
		{
			return _errorHistory
				.GroupBy(e => e.ErrorCode)
				.Select(g => (g.Key, g.Count(), g.OrderByDescending(x => x.Timestamp).First().ErrorMessage))
				.OrderByDescending(x => x.Item2)
				.ToList();
		}

		public List<ErrorRecord> GetRecentErrors(int count = 20)
		{
			return _errorHistory
				.OrderByDescending(e => e.Timestamp)
				.Take(count)
				.ToList();
		}

		public string GenerateErrorReport()
		{
			var patterns = GetErrorPatterns();
			var sb = new System.Text.StringBuilder();

			sb.AppendLine("════════════════════════════════════════════");
			sb.AppendLine("              ERROR REPORT                   ");
			sb.AppendLine("════════════════════════════════════════════");
			sb.AppendLine($"Total Errors: {_errorHistory.Count}");
			sb.AppendLine();

			sb.AppendLine("Top Errors:");
			foreach (var (code, count, lastMsg) in patterns.Take(10))
			{
				sb.AppendLine($"  ├─ {code}: {count} times");
				sb.AppendLine($"  │  └─ Last: {lastMsg}");
			}

			sb.AppendLine();
			sb.AppendLine("════════════════════════════════════════════");

			return sb.ToString();
		}
	}
}
```

### **4️⃣ LoggingExtensions.cs** (Optional - بهترین حل!)

```csharp
using System;

namespace GSM.Core.Source.Device.Logging
{
	public static class LoggingExtensions
	{
		/// <summary>
		/// Wraps any operation with automatic logging (zero code changes required!)
		/// </summary>
		public static T LogOperation<T>(this Device device, 
									   Func<T> operation, 
									   string operationName)
		{
			var logger = DeviceLogger.GetInstance("Device");
			var context = logger.OperationStart(operationName);

			try
			{
				var result = operation.Invoke();
				logger.OperationComplete(context, "SUCCESS", new { Result = result });
				return result;
			}
			catch (Exception ex)
			{
				logger.OperationFailed(context, ex);
				throw;
			}
		}

		/// <summary>
		/// Wraps void operations with automatic logging
		/// </summary>
		public static void LogOperation(this Device device,
									   Action operation,
									   string operationName)
		{
			var logger = DeviceLogger.GetInstance("Device");
			var context = logger.OperationStart(operationName);

			try
			{
				operation.Invoke();
				logger.OperationComplete(context, "SUCCESS");
			}
			catch (Exception ex)
			{
				logger.OperationFailed(context, ex);
				throw;
			}
		}
	}
}
```

### **5️⃣ appsettings.json** (Configuration)

```json
{
  "Logging": {
	"LogLevel": {
	  "Default": "Debug",
	  "GSM.Core": "Debug"
	}
  },
  "Serilog": {
	"MinimumLevel": "Debug",
	"WriteTo": [
	  {
		"Name": "File",
		"Args": {
		  "path": "logs/device-.txt",
		  "rollingInterval": "Day",
		  "fileSizeLimitBytes": 10485760,
		  "retainedFileCountLimit": null,
		  "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
		}
	  },
	  {
		"Name": "File",
		"Args": {
		  "path": "logs/device-structured-.json",
		  "rollingInterval": "Day",
		  "fileSizeLimitBytes": 10485760,
		  "retainedFileCountLimit": null,
		  "formatter": "CompactJsonFormatter"
		}
	  }
	],
	"Enrich": [
	  "FromLogContext",
	  "WithProperty"
	]
  }
}
```

### **6️⃣ تغییر فقط در Device.cs** (3 خط!)

```csharp
// در Constructor
public Device(string DllFileName)
{
	this.ErrorReset();
	this._dllFileName = DllFileName;

	// ✅ یک خط اضافه: Initialize Logger
	this._logger = DeviceLogger.GetInstance(DllFileName);
	this._errorTracer = ErrorTracer.GetInstance(this._logger);

	bool result = true;
	if (result) { result = this.DllLoad(); }
	if (result) { result = this.DllLoadCheck(); }

	this.ConstructorStatusSet(result);
}

// یک خط در ErrorSet() موجود:
private bool ErrorSet(ERROR_CODE ErrorCode, string ErrorMessage)
{
	this._state.ErrorCode = ErrorCode;
	this._state.ErrorMessage = ErrorMessage;

	// ✅ یک خط اضافه: Log the error
	this._errorTracer?.RecordError(ErrorCode, ErrorMessage, "Unknown");

	return false;
}

// یک خط دیگر اختیاری در DllWrapperHasError():
private FUNC_RESULT DllWrapperHasError(FUNC_RESULT funcResult)
{
	try
	{
		if (this._dllWrapper.ErrorCode == DllWrapper.ERROR_CODE.NO_ERROR)
			return funcResult;
		else
		{
			this.DllWrapperErrorCheckSetValue(this._dllWrapper.ErrorMessage);
			// ✅ لاگ DLL خطا
			this._logger?.LogDllResult("DllWrapper", "ERROR", 0);
			return FUNC_RESULT.MAJOR_ERROR;
		}
	}
	catch (Exception ex)
	{
		this.DllWrapperErrorCheckSetValue(ex.Message);
		this._logger?.LogError("DLL_ERROR", ex.Message, "DllWrapperHasError");
		return FUNC_RESULT.MAJOR_ERROR;
	}
}
```

---

## 🗂️ موارد نیاز برای اضافه کردن به Device.cs

```csharp
// در بالای کلاس
private DeviceLogger _logger;
private ErrorTracer _errorTracer;

// در Properties
public string DeviceId => this._dllFileName;
```

---

## 🚀 مراحل پیاده‌سازی (3 روز)

### **روز 1 - صبح:**
```
1. NuGet Package install: Serilog
2. ایجاد Logging folder
3. ایجاد DeviceLogger.cs
4. ایجاد LogContext.cs
5. اضافه کردن properties به Device.cs
6. Initialize logger در constructor
```

### **روز 1 - بعدازظهر:**
```
7. ایجاد appsettings.json
8. تست Logger در یک Controller
9. تعمیر Log Path Issues
10. Verify file creation
```

### **روز 2 - صبح:**
```
11. ایجاد ErrorTracer.cs
12. اضافه ErrorTracer initialization
13. اضافه error logging به ErrorSet()
14. اضافه logging به DllWrapperHasError()
15. تست خطاهای مختلف
```

### **روز 2 - بعدازظهر:**
```
16. ایجاد LoggingExtensions.cs
17. تست Extension Methods
18. اضافه logging به 3-4 Controller methods
19. Verify correlation IDs
```

### **روز 3 - صبح:**
```
20. تست تمام scenarios
21. بررسی folderness logs
22. Performance testing
23. Memory leak check
```

### **روز 3 - بعدازظهر:**
```
24. Documentation نمونه‌ها
25. Configuration fine-tuning
26. Final testing
27. Ready for production!
```

---

## 📊 نتیجهٔ متوقع

### Log File مثال:

**Text Log:**
```
2024-01-15 10:30:45.123 +00:00 [INF] ⏹️ Operation START | Operation=DeviceCountGet | CorrelationId=a1b2c3d4e5f6 | Params={}
2024-01-15 10:30:45.167 +00:00 [DBG] 🔗 DLL CALL | Method=DeviceCount | Params={}
2024-01-15 10:30:45.245 +00:00 [DBG] 🔗 DLL RESULT | Method=DeviceCount | Duration=78ms | Result=5
2024-01-15 10:30:45.248 +00:00 [INF] ✅ Operation COMPLETE | Operation=DeviceCountGet | CorrelationId=a1b2c3d4e5f6 | Status=SUCCESS | Duration=125ms | Result={Result: true}
```

**JSON Log:**
```json
{
  "Timestamp": "2024-01-15T10:30:45.123Z",
  "Level": "Information",
  "Message": "⏹️ Operation START | Operation=DeviceCountGet | CorrelationId=a1b2c3d4e5f6",
  "Properties": {
	"Operation": "DeviceCountGet",
	"CorrelationId": "a1b2c3d4e5f6",
	"DeviceId": "Device"
  }
}
```

---

## ✅ مزایای این رویکرد

| مورد | مزیت |
|------|------|
| **کد تغییرات** | ✅ فقط 3-5 خط! |
| **Minimal Impact** | ✅ بدون تاثیر بر Logic |
| **Performance** | ✅ Async + Buffered |
| **Configuration** | ✅ اختیاری و تغییرپذیر |
| **Error Tracking** | ✅ خودکار و مکمل |
| **Future Proof** | ✅ آماده برای توسعه |

---

## ❓ سوالات تصمیم‌گیری نهایی

### سوال 1: جای Logs؟
```
الف) /logs/ (relative to exe)
ب) C:/Temp/GSM/logs/
ج) Custom path via config?
```
**پیشنهاد:** الف یا ج

---

### سوال 2: Retention Policy؟
```
الف) کل‌نی archive (monthly)
ب) بدون حذف (نامحدود)
ج) Auto-cleanup هر 1 سال
```
**پیشنهاد:** ب (شما گفتید نامحدود)

---

### سوال 3: Logging به کجاهای دیگر؟
```
الف) فقط Device
ب) Device + DllWrapper
ج) تمام کلاسها
```
**پیشنهاد:** الف (فقط Device برای حالا)

---

### سوال 4: Alert/Email هنگام خطا مهم؟
```
الف) خیر، فقط log file
ب) Console alert
ج) Email alert
```
**پیشنهاد:** الف یا ب

---

## 🎬 شروع فوری

اگر می‌خواهید **فوری شروع کنید**:

```powershell
# مراحل:
# 1. Install Serilog
dotnet add package Serilog
dotnet add package Serilog.Sinks.File

# 2. Create Logging folder
mkdir Source/Device/Logging

# 3. Copy my code files above

# 4. Add to Device.cs (3 خط)

# 5. Test!
```

---

## 📞 لازم؟

کدام سوال تصمیم‌گیری؟ و می‌خواهید:
1. ✅ کل کد را الان ایجاد کنم؟
2. ✅ قدم به قدم عملی‌کردن؟
3. ✅ فقط 1 فایل برای شروع؟

