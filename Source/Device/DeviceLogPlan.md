# پلن جامع سیستم لاگینگ و رصد عملکرد برای Device

**تاریخ تحلیل:** 1403/01
**نسخه:** 1.0
**وضعیت:** پیشنهاد اولیه

---

## 1️⃣ خلاصه موجود (Current State Analysis)

### ساختار معماری فعلی
```
Device Layer Architecture
├── Controller Layer (DeviceControllers.cs)
│   ├── Error Management
│   ├── Status Checks
│   └── Operation Entry Points
├── Complex Workflow Layer (Various .cs)
│   ├── Multi-step orchestration
│   └── Sequence coordination
├── Workflow & Execute Layers
│   ├── Retry logic
│   ├── Delay management
│   └── Confirmation patterns
├── Base Layer
│   ├── Atomic operations
│   ├── DLL access
│   └── Validation
└── DllWrapper Layer
	└── Direct DLL calls
```

### نقاط ضعف لاگینگ فعلی
1. ❌ **خالی بودن لاگ سیستماتیک**
2. ❌ **عدم رصد عملکرد (Performance)**
3. ❌ **لاگ غیر ساختاری**
4. ❌ **کمبود تحلیل خطا**
5. ❌ **عدم ردیابی حالت عملیات**
6. ❌ **خالی بودن Correlation IDs**
7. ❌ **عدم آنالیز تایمینگ**

---

## 2️⃣ پلن #1: سیستم لاگینگ ساختاری (Structured Logging)

### 🎯 هدف
ایجاد یک سیستم لاگینگ کارآمد، منظم و قابل پیگیری با استفاده از الگوهای مدرن.

### 🏗️ معماری پیشنهادی

```csharp
// DeviceLogger.cs - کلاس لاگینگ مرکزی
public interface IDeviceLogger
{
	// Operation Lifecycle
	void LogOperationStart(string operation, string correlationId, Dictionary<string, object> context);
	void LogOperationEnd(string operation, string correlationId, FUNC_RESULT result, long durationMs);

	// Error & Exception
	void LogError(ERROR_CODE code, string message, string operation, Exception ex = null);
	void LogWarning(string message, string operation);

	// Performance Metrics
	void LogPerformance(string operation, long durationMs, string details);

	// State Changes
	void LogStateChange(INTERNAL_STATUS oldState, INTERNAL_STATUS newState, string reason);
	void LogFlagChange(string flagName, object oldValue, object newValue);

	// DLL Operations
	void LogDllCall(string methodName, Dictionary<string, object> parameters);
	void LogDllResult(string methodName, object result, long durationMs);
}

public class DeviceLogger : IDeviceLogger
{
	private readonly ILogger _logger; // Serilog or similar
	private readonly string _deviceId;

	public DeviceLogger(string deviceId)
	{
		_deviceId = deviceId;
		_logger = new Logger($"Device-{deviceId}");
	}

	public void LogOperationStart(string operation, string correlationId, 
								  Dictionary<string, object> context)
	{
		var log = new
		{
			Timestamp = DateTime.UtcNow,
			DeviceId = _deviceId,
			CorrelationId = correlationId,
			Operation = operation,
			Stage = "START",
			Context = context
		};
		_logger.Information("Operation started: {@Log}", log);
	}

	// ... سایر متدها
}
```

### 📊 فرمت لاگ پیشنهادی

```json
{
  "timestamp": "2024-01-15T10:30:45.123Z",
  "correlationId": "OP-20240115-001234",
  "deviceId": "DEV-001",
  "operation": "DeviceCountGet",
  "stage": "START|EXECUTE|COMPLETE|ERROR",
  "duration_ms": 45,
  "result": "SUCCESS|FAIL|MAJOR_ERROR",
  "errorCode": "NO_ERROR|DLL_WRAPPER|TIMEOUT",
  "errorMessage": "Error description",
  "metrics": {
	"retry_count": 2,
	"buffer_size": 1024,
	"dll_call_duration_ms": 23
  },
  "context": {
	"deviceCount": 5,
	"deviceIndex": 2,
	"status": "BASIC_LEVEL"
  }
}
```

### ✅ مزایا
- ✅ پیگیری کامل عملیات
- ✅ تحلیل خطاهای مفصل
- ✅ رصد عملکرد دقیق
- ✅ روابط بین عملیات (Correlation)
- ✅ سهولت جستجو و فیلتر

### 🛠️ پیاده‌سازی
1. `DeviceLogger.cs` - کلاس لاگ مرکزی
2. `DeviceLogContext.cs` - Context برای عملیات
3. تعارض با متد‌های موجود و اضافه کردن لاگ

---

## 3️⃣ پلن #2: رصد عملکرد (Performance Monitoring)

### 🎯 هدف
اندازه‌گیری دقیق زمان اجرای هر عملیات و شناسایی گلوگاه‌های عملکرد.

### 🏗️ معماری پیشنهادی

```csharp
public class PerformanceTracker
{
	private Dictionary<string, OperationMetrics> _metrics = new();

	public class OperationMetrics
	{
		public string OperationName { get; set; }
		public long MinDuration { get; set; }
		public long MaxDuration { get; set; }
		public long AverageDuration { get; set; }
		public int ExecutionCount { get; set; }
		public int FailureCount { get; set; }
		public double SuccessRate { get; set; }
		public List<long> RecentDurations { get; set; } = new();
	}

	public void RecordOperation(string operation, long durationMs, bool success)
	{
		if (!_metrics.ContainsKey(operation))
			_metrics[operation] = new OperationMetrics { OperationName = operation };

		var metric = _metrics[operation];

		metric.ExecutionCount++;
		if (!success) metric.FailureCount++;
		metric.SuccessRate = (metric.ExecutionCount - metric.FailureCount) 
						   / (double)metric.ExecutionCount * 100;

		metric.MinDuration = Math.Min(metric.MinDuration, durationMs);
		metric.MaxDuration = Math.Max(metric.MaxDuration, durationMs);
		metric.RecentDurations.Add(durationMs);

		// Keep only last 100 durations
		if (metric.RecentDurations.Count > 100)
			metric.RecentDurations.RemoveAt(0);

		metric.AverageDuration = (long)metric.RecentDurations.Average();
	}

	public Dictionary<string, OperationMetrics> GetMetrics() => _metrics;

	public string GenerateReport()
	{
		var sb = new StringBuilder();
		foreach (var metric in _metrics.Values)
		{
			sb.AppendLine($"Operation: {metric.OperationName}");
			sb.AppendLine($"  Executions: {metric.ExecutionCount}");
			sb.AppendLine($"  Success Rate: {metric.SuccessRate:F2}%");
			sb.AppendLine($"  Min Duration: {metric.MinDuration}ms");
			sb.AppendLine($"  Max Duration: {metric.MaxDuration}ms");
			sb.AppendLine($"  Avg Duration: {metric.AverageDuration}ms");
			sb.AppendLine();
		}
		return sb.ToString();
	}
}
```

### 📊 معیارهای رصد‌پذیری

```
Operation: DeviceCountGet
├── Total Calls: 1,245
├── Success Rate: 98.5%
├── Failed: 19 times
├── Duration Stats:
│   ├── Min: 12ms
│   ├── Max: 245ms
│   ├── Average: 45ms
│   └── P95: 120ms
├── DLL Call Duration: 23ms (avg)
└── Retry Count: 0.15 (average retries)
```

### ✅ مزایا
- ✅ شناسایی گلوگاه‌های عملکرد
- ✅ تحلیل روند عملکرد
- ✅ پیش‌بینی مسائل عملکردی
- ✅ تصمیم‌گیری بهتر برای بهینه‌سازی

### 🛠️ پیاده‌سازی
1. `PerformanceTracker.cs` - رصد عملکرد
2. `Stopwatch` استفاده در `Workflow` layers
3. گزارش‌گیری دوره‌ای

---

## 4️⃣ پلن #3: سیستم ردیابی خطا مرتبط (Error Correlation & Tracing)

### 🎯 هدف
ایجاد یک سیستم جامع برای دنبال‌کردن، تحلیل و رفع مسائل.

### 🏗️ معماری پیشنهادی

```csharp
public class ErrorTracing
{
	public class ErrorContext
	{
		public string CorrelationId { get; set; }
		public string OperationName { get; set; }
		public ERROR_CODE ErrorCode { get; set; }
		public string ErrorMessage { get; set; }
		public int Layer { get; set; } // Controller=1, Workflow=2, Base=3, DLL=4
		public DateTime Timestamp { get; set; }
		public Stack<string> CallStack { get; set; }
		public Dictionary<string, object> DeviceState { get; set; }
		public Exception Exception { get; set; }
	}

	private Queue<ErrorContext> _errorHistory = new(maxSize: 1000);

	public void RecordError(ERROR_CODE code, string message, string operation, 
						   int layer, Exception ex = null)
	{
		var context = new ErrorContext
		{
			CorrelationId = Guid.NewGuid().ToString(),
			OperationName = operation,
			ErrorCode = code,
			ErrorMessage = message,
			Layer = layer,
			Timestamp = DateTime.UtcNow,
			CallStack = new Stack<string>(new[] 
			{ 
				"Controller.DeviceCountGet -> Workflow.DeviceCountGetWorkflow -> Execute -> Base"
			}),
			DeviceState = CaptureDeviceState(),
			Exception = ex
		};

		_errorHistory.Enqueue(context);
		LogErrorPattern(context);
	}

	// Pattern Analysis
	public class ErrorPattern
	{
		public ERROR_CODE Code { get; set; }
		public int Frequency { get; set; }
		public DateTime FirstOccurrence { get; set; }
		public DateTime LastOccurrence { get; set; }
		public double FrequencyPerHour { get; set; }
		public string CommonMessage { get; set; }
	}

	public List<ErrorPattern> AnalyzeErrors(TimeSpan period)
	{
		var patterns = new Dictionary<ERROR_CODE, ErrorPattern>();

		foreach (var error in _errorHistory.Where(e => DateTime.UtcNow - e.Timestamp < period))
		{
			if (!patterns.ContainsKey(error.ErrorCode))
				patterns[error.ErrorCode] = new ErrorPattern 
				{ 
					Code = error.ErrorCode,
					FirstOccurrence = error.Timestamp
				};

			var pattern = patterns[error.ErrorCode];
			pattern.Frequency++;
			pattern.LastOccurrence = error.Timestamp;
			pattern.CommonMessage = error.ErrorMessage;
		}

		return patterns.Values.ToList();
	}
}
```

### 📊 نمونه‌ای از دیتاهای ردیابی‌شده

```
Error Correlation Report
├── Correlation ID: OP-20240115-001234
├── Operation: DeviceCountGet
├── Timeline:
│   ├── [10:30:45.120] Controller: DeviceCountGet called
│   ├── [10:30:45.125] Workflow: DeviceCountGetWorkflow started
│   ├── [10:30:45.148] Execute: Loop attempt 1
│   ├── [10:30:45.167] Base: DLL call
│   ├── [10:30:45.245] Error: DLL_WRAPPER returned
│   └── [10:30:45.248] Result: MAJOR_ERROR
└── Device State at Error:
	├── InternalStatus: BASIC_LEVEL
	├── Busy: 0
	├── DeviceCount: 5
	├── ErrorCode: DLL_WRAPPER
	└── ErrorMessage: "DLL error details"
```

### ✅ مزایا
- ✅ ردیابی دقیق مسیر خطا
- ✅ شناسایی الگوهای تکراری خطا
- ✅ تحلیل ریشه‌ی خطا
- ✅ تصمیم‌گیری برای اصلاح

### 🛠️ پیاده‌سازی
1. `ErrorTracing.cs` - کلاس ردیابی خطا
2. `ErrorContext.cs` - Context خطا
3. یکپارچگی با `DeviceLogger`

---

## 5️⃣ پلن #4: رصد وضعیت و حالات (State Monitoring)

### 🎯 هدف
ردیابی تغییرات حالت بهترین و تشخیص حالات غیرعادی.

### 🏗️ معماری پیشنهادی

```csharp
public class StateMonitor
{
	public class StateSnapshot
	{
		public DateTime Timestamp { get; set; }
		public INTERNAL_STATUS InternalStatus { get; set; }
		public int Busy { get; set; }
		public ERROR_CODE LastErrorCode { get; set; }
		public int DeviceCount { get; set; }
		public int DeviceIndex { get; set; }
		public Dictionary<string, object> RuntimeFlags { get; set; }
		public Dictionary<string, int> RuntimeCounters { get; set; }
	}

	private List<StateSnapshot> _stateHistory = new();
	private StateSnapshot _lastState;

	public void CaptureState(Device device)
	{
		var snapshot = new StateSnapshot
		{
			Timestamp = DateTime.UtcNow,
			InternalStatus = device.InternalStatus,
			Busy = device.Busy,
			LastErrorCode = device.ErrorCode,
			DeviceCount = device.DeviceCount,
			DeviceIndex = device.DeviceIndex,
			RuntimeFlags = CaptureFlags(device),
			RuntimeCounters = CaptureCounters(device)
		};

		_stateHistory.Add(snapshot);
		DetectStateAnomalies(snapshot);
		_lastState = snapshot;
	}

	public void DetectStateAnomalies(StateSnapshot current)
	{
		if (_lastState == null) return;

		// Anomaly 1: Unexpected state transition
		if (IsInvalidStateTransition(current.InternalStatus, _lastState.InternalStatus))
		{
			LogAnomaly($"Invalid state transition: {_lastState.InternalStatus} -> {current.InternalStatus}");
		}

		// Anomaly 2: Stuck in error state
		if (current.LastErrorCode != ERROR_CODE.NO_ERROR && 
			_lastState.LastErrorCode == current.LastErrorCode &&
			(current.Timestamp - _lastState.Timestamp).TotalSeconds > 5)
		{
			LogAnomaly($"Stuck in error: {current.LastErrorCode}");
		}

		// Anomaly 3: Busy flag stuck
		if (current.Busy == 1 && _lastState.Busy == 1 &&
			(current.Timestamp - _lastState.Timestamp).TotalSeconds > 30)
		{
			LogAnomaly("Busy flag stuck for more than 30 seconds");
		}
	}

	public StateReport GenerateStateReport()
	{
		return new StateReport
		{
			TotalStateChanges = _stateHistory.Count,
			CurrentState = _lastState?.InternalStatus.ToString(),
			StateChangeFrequency = CalculateFrequency(),
			AnomalyCount = _anomalies.Count,
			AnomalyDetails = _anomalies.Take(10).ToList()
		};
	}
}
```

### 📊 نمونه‌ای از گزارش وضعیت

```
State Monitoring Report
├── Current Status: SINGLE_DEVICE
├── Busy Flag: 0 (Not busy)
├── Error Code: NO_ERROR
├── State Change History (Last 10):
│   ├── 10:25:30 INVALID -> BASIC_LEVEL
│   ├── 10:26:15 BASIC_LEVEL -> SINGLE_DEVICE
│   ├── 10:27:00 SINGLE_DEVICE -> WORKING_MODE
│   └── ... (7 more)
├── Anomalies Detected: 2
│   ├── ⚠️ Invalid transition attempt at 10:30:45
│   └── ⚠️ Busy flag stuck for 15 seconds at 10:35:20
└── State Stability: 99.2%
```

### ✅ مزایا
- ✅ تشخیص دقیق تغییرات حالت
- ✅ تحلیل انتقال‌های نامعتبر
- ✅ تشخیص Deadlocks و حالات stuck
- ✅ پیش‌گویی مسائل آتی

### 🛠️ پیاده‌سازی
1. `StateMonitor.cs` - کلاس رصد حالت
2. `StateSnapshot.cs` - عکس وضعیت
3. ادغام در `InternalStatusUpdate()`

---

## 6️⃣ پلن #5: رصد DLL و عملیات سخت‌افزاری

### 🎯 هدف
ردیابی تمام فراخوانی‌های DLL، پارامترها و نتایج آن‌ها.

### 🏗️ معماری پیشنهادی

```csharp
public class DllOperationTracker
{
	public class DllCallRecord
	{
		public string MethodName { get; set; }
		public Dictionary<string, object> Parameters { get; set; }
		public object Result { get; set; }
		public long DurationMs { get; set; }
		public DateTime Timestamp { get; set; }
		public bool Success { get; set; }
		public string ErrorInfo { get; set; }
	}

	private Queue<DllCallRecord> _callRecords = new(maxSize: 5000);

	public void RecordDllCall(string method, Dictionary<string, object> parameters,
							 object result, long duration, bool success, string error = null)
	{
		var record = new DllCallRecord
		{
			MethodName = method,
			Parameters = parameters,
			Result = result,
			DurationMs = duration,
			Timestamp = DateTime.UtcNow,
			Success = success,
			ErrorInfo = error
		};

		_callRecords.Enqueue(record);
	}

	public DllHealthReport GenerateHealthReport()
	{
		return new DllHealthReport
		{
			TotalCalls = _callRecords.Count,
			SuccessCalls = _callRecords.Count(c => c.Success),
			FailedCalls = _callRecords.Count(c => !c.Success),
			SuccessRate = (_callRecords.Count(c => c.Success) / (double)_callRecords.Count) * 100,
			AverageDuration = _callRecords.Average(c => c.DurationMs),
			MostCalledMethod = _callRecords
				.GroupBy(c => c.MethodName)
				.OrderByDescending(g => g.Count())
				.First().Key,
			SlowestMethod = _callRecords
				.GroupBy(c => c.MethodName)
				.OrderByDescending(g => g.Average(c => c.DurationMs))
				.First().Key,
			RecentErrors = _callRecords
				.Where(c => !c.Success)
				.OrderByDescending(c => c.Timestamp)
				.Take(20)
				.ToList()
		};
	}
}

public class DllHealthReport
{
	public int TotalCalls { get; set; }
	public int SuccessCalls { get; set; }
	public int FailedCalls { get; set; }
	public double SuccessRate { get; set; }
	public double AverageDuration { get; set; }
	public string MostCalledMethod { get; set; }
	public string SlowestMethod { get; set; }
	public List<DllOperationTracker.DllCallRecord> RecentErrors { get; set; }
}
```

### 📊 نمونه‌ای از گزارش DLL

```
DLL Operations Health Report
├── Total Calls: 12,450
├── Successful: 12,245 (98.35%)
├── Failed: 205 (1.65%)
├── Average Duration: 23ms
├── Most Called Methods:
│   ├── DeviceCount(): 2,100 calls (16.87%)
│   ├── GsmPowerGet(): 1,950 calls (15.66%)
│   └── BufferLastIndexGet(): 1,800 calls (14.46%)
├── Slowest Methods:
│   ├── SmsSend(): avg 245ms
│   ├── DeviceIdentity(): avg 156ms
│   └── GsmPowerSet(): avg 134ms
└── Recent Errors (Last 20):
	├── [10:35:22] GsmPowerGet() - DLL_WRAPPER error
	├── [10:34:18] DeviceCount() - Timeout
	└── ... (18 more)
```

### ✅ مزایا
- ✅ تشخیص مسائل DLL
- ✅ شناسایی عملیات آهسته
- ✅ تحلیل نمونه‌های ارتباطی
- ✅ بهینه‌سازی فراخوانی‌های DLL

### 🛠️ پیاده‌سازی
1. `DllOperationTracker.cs` - رصد عملیات DLL
2. Wrapper method های جدید برای logging
3. ادغام در `DllWrapperHasError()`

---

## 7️⃣ پلن #6: سیستم هشدار و اطلاع‌رسانی

### 🎯 هدف
ایجاد سیستم هشدار فوری برای موقعیت‌های بحرانی.

### 🏗️ معماری پیشنهادی

```csharp
public interface IAlertProvider
{
	void SendAlert(AlertLevel level, string title, string message, string details);
}

public enum AlertLevel { Info, Warning, Critical, Emergency }

public class AlertManager
{
	private readonly List<IAlertProvider> _providers = new();
	private readonly Queue<Alert> _alertQueue = new();

	public class Alert
	{
		public AlertLevel Level { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }
		public string Details { get; set; }
		public DateTime CreatedAt { get; set; }
		public bool Acknowledged { get; set; }
	}

	public void RaiseAlert(AlertLevel level, string title, string message, string details = "")
	{
		var alert = new Alert
		{
			Level = level,
			Title = title,
			Message = message,
			Details = details,
			CreatedAt = DateTime.UtcNow
		};

		_alertQueue.Enqueue(alert);

		foreach (var provider in _providers)
		{
			provider.SendAlert(level, title, message, details);
		}
	}

	// Predefined Alert Scenarios
	public void AlertDllFailure(ERROR_CODE code, string details)
	{
		RaiseAlert(AlertLevel.Critical, 
				  "DLL Operation Failed", 
				  $"DLL operation failed with error code: {code}",
				  details);
	}

	public void AlertHighErrorRate(string operation, double errorRate)
	{
		if (errorRate > 5.0) // مثلاً بیشتر از 5% خطا
		{
			RaiseAlert(AlertLevel.Warning,
					  $"High Error Rate Detected",
					  $"Operation '{operation}' has error rate of {errorRate:F2}%",
					  "Consider restarting device or investigating underlying issues");
		}
	}

	public void AlertPerformanceDegradation(string operation, long avgDuration, long threshold)
	{
		if (avgDuration > threshold)
		{
			RaiseAlert(AlertLevel.Warning,
					  "Performance Degradation",
					  $"'{operation}' is taking {avgDuration}ms (threshold: {threshold}ms)",
					  "Check system resources and DLL performance");
		}
	}

	public void AlertBusyTimeout(int seconds)
	{
		RaiseAlert(AlertLevel.Critical,
				  "Device Stuck",
				  $"Device busy flag stuck for {seconds} seconds",
				  "Immediate intervention required - consider device restart");
	}
}

// Implementation examples
public class ConsoleAlertProvider : IAlertProvider
{
	public void SendAlert(AlertLevel level, string title, string message, string details)
	{
		Console.ForegroundColor = level switch
		{
			AlertLevel.Info => ConsoleColor.Blue,
			AlertLevel.Warning => ConsoleColor.Yellow,
			AlertLevel.Critical => ConsoleColor.Red,
			AlertLevel.Emergency => ConsoleColor.Magenta,
			_ => ConsoleColor.White
		};

		Console.WriteLine($"[{level.ToString().ToUpper()}] {title}");
		Console.WriteLine($"  Message: {message}");
		if (!string.IsNullOrEmpty(details))
			Console.WriteLine($"  Details: {details}");
		Console.ResetColor();
	}
}

public class EmailAlertProvider : IAlertProvider
{
	public void SendAlert(AlertLevel level, string title, string message, string details)
	{
		// Send email implementation
		// Only for Critical and Emergency levels
	}
}

public class DatabaseAlertProvider : IAlertProvider
{
	public void SendAlert(AlertLevel level, string title, string message, string details)
	{
		// Store alert in database for historical analysis
	}
}
```

### 📊 نمونه‌ای از هشدارها

```
🔴 [EMERGENCY] Device Stuck
	Message: Device busy flag stuck for 45 seconds
	Details: Immediate intervention required - consider device restart
	CreatedAt: 2024-01-15T10:35:22Z
	Action Required: URGENT

🟠 [CRITICAL] DLL Operation Failed
	Message: DLL operation failed with error code: DLL_WRAPPER
	Details: GsmPowerGet() returned invalid result
	CreatedAt: 2024-01-15T10:34:18Z
	Status: Sent to Admin via Email

🟡 [WARNING] High Error Rate Detected
	Message: Operation 'DeviceCountGet' has error rate of 8.5%
	Details: Consider restarting device or investigating underlying issues
	CreatedAt: 2024-01-15T10:33:45Z
	Status: Logged

⚫ [INFO] Performance Degradation
	Message: 'SmsSend' is taking 340ms (threshold: 300ms)
	Details: Check system resources and DLL performance
	CreatedAt: 2024-01-15T10:30:12Z
	Status: Monitored
```

### ✅ مزایا
- ✅ کاهش زمان پاسخ به مسائل
- ✅ رصد فوری از وضعیت سیستم
- ✅ اطلاع‌رسانی مسلح‌شده
- ✅ تاریخچه و تحلیل هشدارها

### 🛠️ پیاده‌سازی
1. `AlertManager.cs` - مدیریت هشدارها
2. `IAlertProvider.cs` - Interface پروایدرهای هشدار
3. پیاده‌سازی‌های مختلف (Console, Email, Database)
4. ادغام با دیگر صفحات رصد

---

## 8️⃣ پلن #7: تجزیه و تحلیل تجمعی (Aggregated Analytics)

### 🎯 هدف
ایجاد یک داشبورد جامع برای تحلیل کل سیستم.

### 🏗️ معماری پیشنهادی

```csharp
public class DeviceAnalyticsEngine
{
	private readonly PerformanceTracker _performanceTracker;
	private readonly ErrorTracing _errorTracing;
	private readonly StateMonitor _stateMonitor;
	private readonly DllOperationTracker _dllTracker;

	public class AnalyticsSnapshot
	{
		public DateTime Timestamp { get; set; }

		// Health Metrics
		public double OverallHealthScore { get; set; } // 0-100
		public string HealthStatus { get; set; } // Healthy, Warning, Critical

		// Performance Summary
		public double AverageOperationDuration { get; set; }
		public double OperationSuccessRate { get; set; }

		// Error Summary
		public int ErrorCount24h { get; set; }
		public int CriticalErrorCount { get; set; }
		public List<ErrorPattern> TopErrors { get; set; }

		// State Summary
		public string CurrentInternalStatus { get; set; }
		public int StateChangeCount24h { get; set; }

		// DLL Summary
		public double DllSuccessRate { get; set; }
		public double AvgDllDuration { get; set; }

		// Recommendations
		public List<string> Recommendations { get; set; }
	}

	public AnalyticsSnapshot GenerateSnapshot()
	{
		var perfMetrics = _performanceTracker.GetMetrics();
		var errors = _errorTracing.AnalyzeErrors(TimeSpan.FromHours(24));
		var stateReport = _stateMonitor.GenerateStateReport();
		var dllReport = _dllTracker.GenerateHealthReport();

		var snapshot = new AnalyticsSnapshot
		{
			Timestamp = DateTime.UtcNow,
			OverallHealthScore = CalculateHealthScore(perfMetrics, errors, dllReport),
			HealthStatus = DetermineHealthStatus(perfMetrics, errors, dllReport),
			AverageOperationDuration = perfMetrics.Values.Average(m => m.AverageDuration),
			OperationSuccessRate = perfMetrics.Values.Average(m => m.SuccessRate),
			ErrorCount24h = errors.Sum(e => e.Frequency),
			CriticalErrorCount = errors.Count(e => e.Code == ERROR_CODE.DLL_WRAPPER || 
													e.Code == ERROR_CODE.EXECUTE_TIMEOUT),
			TopErrors = errors.OrderByDescending(e => e.Frequency).Take(5).ToList(),
			Recommendations = GenerateRecommendations(perfMetrics, errors, dllReport)
		};

		return snapshot;
	}

	private List<string> GenerateRecommendations(Dictionary<string, OperationMetrics> perf,
												 List<ErrorPattern> errors,
												 DllHealthReport dllReport)
	{
		var recommendations = new List<string>();

		// Performance recommendations
		var slowOps = perf.Where(p => p.Value.AverageDuration > 200).ToList();
		if (slowOps.Any())
			recommendations.Add($"⚠️ {slowOps.Count} operations are slow (>{200}ms avg). Consider optimization.");

		// Error recommendations
		var highErrorOps = perf.Where(p => p.Value.SuccessRate < 95).ToList();
		if (highErrorOps.Any())
			recommendations.Add($"🔴 {highErrorOps.Count} operations have low success rates. Investigate immediately.");

		// DLL recommendations
		if (dllReport.SuccessRate < 99)
			recommendations.Add($"⚠️ DLL success rate is {dllReport.SuccessRate:F2}%. Check DLL stability.");

		// Resource recommendations
		if (dllReport.AverageDuration > 100)
			recommendations.Add("⚠️ Average DLL call duration is high. Check system resources.");

		if (!recommendations.Any())
			recommendations.Add("✅ System is running optimally. No issues detected.");

		return recommendations;
	}
}
```

### 📊 نمونه‌ای از گزارش تجمعی

```
═════════════════════════════════════════════════════════════════
		  DEVICE ANALYTICS REPORT - 2024-01-15 10:45:00 UTC
═════════════════════════════════════════════════════════════════

📊 OVERALL HEALTH SCORE: 87/100 [WARNING]
   └─ Status: OPERATIONAL WITH CONCERNS

┌─────────────────────────────────────────────────────────────────
│ PERFORMANCE METRICS (Past 24 Hours)
├─────────────────────────────────────────────────────────────────
│ Average Operation Duration: 45ms
│ Operation Success Rate: 96.8%
│ Total Operations: 12,450
│ Failed Operations: 395
└─────────────────────────────────────────────────────────────────

┌─────────────────────────────────────────────────────────────────
│ ERROR ANALYSIS
├─────────────────────────────────────────────────────────────────
│ Total Errors (24h): 395
│ Critical Errors: 18
│ Top 5 Errors:
│   1. DLL_WRAPPER (87 occurrences) - 22.0%
│   2. TIMEOUT (65 occurrences) - 16.5%
│   3. DEVICE_COUNT (43 occurrences) - 10.9%
│   4. INVALID_INPUT (32 occurrences) - 8.1%
│   5. SIM_STATUS (28 occurrences) - 7.1%
└─────────────────────────────────────────────────────────────────

┌─────────────────────────────────────────────────────────────────
│ STATE TRANSITIONS
├─────────────────────────────────────────────────────────────────
│ Current Status: SINGLE_DEVICE
│ State Changes (24h): 23
│ Anomalies Detected: 2
│ State Stability: 98.5%
└─────────────────────────────────────────────────────────────────

┌─────────────────────────────────────────────────────────────────
│ DLL HEALTH
├─────────────────────────────────────────────────────────────────
│ DLL Call Success Rate: 98.35%
│ Total Calls: 12,450
│ Failed Calls: 205
│ Average Call Duration: 23ms
│ Slowest Method: SmsSend (245ms avg)
└─────────────────────────────────────────────────────────────────

┌─────────────────────────────────────────────────────────────────
│ RECOMMENDATIONS
├─────────────────────────────────────────────────────────────────
│ 1. 🔴 1 operations have low success rates. Investigate immediately.
│    └─ SmsSend: 92.3% success rate
│
│ 2. ⚠️ DLL success rate is 98.35%. Check DLL stability.
│    └─ Recent errors: GsmPowerGet, DeviceCount
│
│ 3. ⚠️ 2 operations are slow (>200ms avg). Consider optimization.
│    └─ SmsSend (245ms), GsmPowerSet (234ms)
│
│ 4. 💡 Schedule preventive device restart given current state.
│    └─ Uptime: 48 hours
│
│ 5. ✅ State transition mechanism is stable.
└─────────────────────────────────────────────────────────────────

═════════════════════════════════════════════════════════════════
		   Generated: 2024-01-15 10:45:00 UTC
		   Next Report: 2024-01-15 11:45:00 UTC (Scheduling)
═════════════════════════════════════════════════════════════════
```

### ✅ مزایا
- ✅ دیدی یکپارچه از کل سیستم
- ✅ تصمیم‌گیری بر اساس داده‌ها
- ✅ پیش‌گویی مسائل کاهش‌یافته
- ✅ بهینه‌سازی آسان‌تر

### 🛠️ پیاده‌سازی
1. `DeviceAnalyticsEngine.cs` - موتور تجزیه‌تحلیل
2. ادغام کامل با تمام سیستم‌های دیگر
3. گزارش‌گیری خودکار دوره‌ای

---

## 9️⃣ خلاصه پیاده‌سازی و ترتیب اولویت

### 📋 فایل‌های جدید مورد نیاز

```
Source/Device/Logging/
├── DeviceLogger.cs                    [پلن #1]
├── DeviceLogContext.cs                [پلن #1]
├── PerformanceTracker.cs              [پلن #2]
├── ErrorTracing.cs                    [پلن #3]
├── ErrorContext.cs                    [پلن #3]
├── StateMonitor.cs                    [پلن #4]
├── StateSnapshot.cs                   [پلن #4]
├── DllOperationTracker.cs             [پلن #5]
├── AlertManager.cs                    [پلن #6]
├── AlertProvider/
│   ├── IAlertProvider.cs              [پلن #6]
│   ├── ConsoleAlertProvider.cs        [پلن #6]
│   ├── EmailAlertProvider.cs          [پلن #6]
│   └── DatabaseAlertProvider.cs       [پلن #6]
└── DeviceAnalyticsEngine.cs           [پلن #7]
```

### 🎯 ترتیب اولویت پیاده‌سازی

```
Phase 1 (Critical - Weeks 1-2):
├── [✅ HIGH] DeviceLogger.cs          - Basic structured logging
├── [✅ HIGH] ErrorTracing.cs          - Error recording
├── [✅ HIGH] StateMonitor.cs          - State tracking
└── [✅ MEDIUM] PerformanceTracker.cs  - Basic metrics

Phase 2 (Important - Weeks 3-4):
├── [✅ MEDIUM] DllOperationTracker.cs - DLL monitoring
├── [✅ MEDIUM] AlertManager.cs        - Alert system
└── [✅ MEDIUM] Alert Providers        - Notification channels

Phase 3 (Enhancement - Weeks 5-6):
├── [✅ LOW] DeviceAnalyticsEngine.cs  - Dashboard & reporting
└── [✅ LOW] Advanced analytics        - ML-based predictions
```

### 📌 نکات حائز اهمیت

1. **Thread Safety**: تمام کلاس‌های logging thread-safe باشند
2. **Performance Impact**: بیش‌الحد logging عملکرد را نباید کاهش دهد
3. **Storage Management**: نسخ‌های قدیمی लاग‌ها و metrics حذف شود
4. **Configuration**: تنظیمات logging قابل‌تغیر در Runtime باشد
5. **Testing**: شامل Unit Tests و Integration Tests باشد

---

## 🔟 سوالات مهم برای بحث

### ❓ سوال 1: انتخاب Library لاگینگ
**گزینه‌ها:**
- [ ] Serilog (پیشنهادی - ساختاری، غنی، قابل‌توسعه)
- [ ] NLog (سبک‌تر، سریع‌تر)
- [ ] log4net (قدیمی‌تر اما پایدار)
- [ ] Custom Solution (کنترل کامل)

**سوال:** کدام library را ترجیح می‌دهید؟

---

### ❓ سوال 2: رویکرد ذخیره‌سازی Logs
**گزینه‌ها:**
- [ ] File-based (.txt, .json)
- [ ] Database (SQL Server, SQLite)
- [ ] Message Queue (RabbitMQ, Kafka)
- [ ] Cloud Service (Azure Application Insights, CloudWatch)
- [ ] Hybrid (ترکیبی)

**سوال:** بهترین رویکرد برای صورتحال شما کدام است؟

---

### ❓ سوال 3: بازه زمانی نگهداری Logs
**گزینه‌ها:**
- [ ] 7 روز
- [ ] 30 روز
- [ ] 90 روز
- [ ] سال
- [ ] نامحدود (با آرشیو)

**سوال:** برای چه مدت log داده‌ها را نگهداری می‌کنید؟

---

### ❓ سوال 4: سطح جزئیات Logging
**گزینه‌ها:**
- [ ] فقط خطاها و هشدارها (کم)
- [ ] + عملیات مهم (متوسط)
- [ ] + تمام عملیات (زیاد)
- [ ] + جزئیات Parameter ها (بسیار زیاد)

**سوال:** چه سطح جزئیاتی لازم است؟

---

### ❓ سوال 5: تعادل Performance و Logging
**گزینه‌ها:**
- [ ] بدون Logging
- [ ] Minimal Logging (فقط خطاها)
- [ ] Balanced (متوسط)
- [ ] Comprehensive (کامل)
- [ ] Configurable (تغییرپذیر)

**سوال:** ترجیح شما کدام است؟

---

## 1️⃣1️⃣ دستور العمل بعدی

### مرحله 1: تصمیم‌گیری
لطفاً در مورد سوالات فوق نظر خود را بیان کنید.

### مرحله 2: طراحی تفصیلی
بر اساس پاسخ‌های شما، طرح تفصیلی ایجاد خواهم کرد.

### مرحله 3: پیاده‌سازی
پیاده‌سازی مرحله‌ای بر اساس اولویت‌ها.

### مرحله 4: تست
تست جامع برای اطمینان از کارکرد صحیح.

### مرحله 5: اسناد
تهیه اسناد و راهنمای استفاده.

---

**📝 تاریخ آپدیت:** 1403/01
**👤 نویسنده:** تحلیل Automated
**✅ وضعیت:** منتظر تصمیم‌گیری از طرف کاربر

