# 📐 معماری سیستم Logging

## سیستم Logging ساختار کلی

```
┌─────────────────────────────────────────────────────────────────┐
│                          Application                            │
└────────────────────┬────────────────────────────────────────────┘
					 │
					 ▼
	  ┌──────────────────────────────┐
	  │   Device Class              │
	  │  ┌────────────────────────┐  │
	  │  │ public Methods:        │  │
	  │  │ - DeviceCountGet()     │  │
	  │  │ - DeviceIdentity()     │  │
	  │  │ - Sms Send, etc        │  │
	  │  └────────────────────────┘  │
	  │            │                  │
	  │    ┌───────┴────────┐        │
	  │    │                │        │
	  │    ▼                ▼        │
	  │  Error Set    DllWrapper    │  (موجود)
	  │                             │
	  └─────────────────────────────┘
					 │
	   ┌─────────────┼─────────────┐
	   │             │             │
	   ▼             ▼             ▼
	Constructor  Methods        Error
	----------   -------        -----
	│ Logger     │ Logging     │ ErrorTracer
	│ Init       │ Start/End   │ Log Error
	▼             ▼             ▼


════════════════════════════════════════════════════════════════════

NEW LOGGING LAYER (Separate)

════════════════════════════════════════════════════════════════════

┌───────────────────────────────────────────────────────────────────┐
│                     Logging Infrastructure                        │
├───────────────────────────────────────────────────────────────────┤
│                                                                   │
│  LoggingInitializer.Initialize()  (Call once at startup)         │
│         │                                                         │
│         ▼                                                         │
│  ┌──────────────────────────────────────────────┐               │
│  │      DeviceLogger (Serilog wrapper)         │               │
│  ├──────────────────────────────────────────────┤               │
│  │ Public Methods:                              │               │
│  │  - OperationStart()                          │               │
│  │  - OperationComplete()                       │               │
│  │  - OperationFailed()                         │               │
│  │  - LogError()                                │               │
│  │  - LogDllCall()                              │               │
│  │  - LogDllResult()                            │               │
│  │  - LogStateChange()                          │               │
│  └──────────────────────────────────────────────┘               │
│         │                                                         │
│         ├─────────────────┬─────────────────┐                    │
│         │                 │                 │                    │
│         ▼                 ▼                 ▼                    │
│   ┌──────────┐      ┌──────────┐    ┌──────────────┐            │
│   │ Serilog  │      │LogContext│    │ ErrorTracer  │            │
│   ├──────────┤      ├──────────┤    ├──────────────┤            │
│   │  Text    │      │ Corr ID  │    │ Error List   │            │
│   │   Log    │      │ Op Name  │    │ Patterns     │            │
│   │   File   │      │ Duration │    │ Reports      │            │
│   └──────────┘      └──────────┘    └──────────────┘            │
│         │                                     │                   │
│         ▼                                     ▼                   │
│   ┌──────────┐                        ┌──────────────┐            │
│   │  JSON    │                        │  CSV Export  │            │
│   │  Struct. │                        │  Analysis    │            │
│   │   Log    │                        │  Report      │            │
│   │  File    │                        │  Generation  │            │
│   └──────────┘                        └──────────────┘            │
│         │                                     │                   │
│         └─────────────────┬───────────────────┘                  │
│                           │                                       │
└───────────────────────────┼───────────────────────────────────────┘
							│
							▼
					  ┌──────────────┐
					  │ /logs/ Folder│
					  ├──────────────┤
					  │ device-.txt  │
					  │ device-.json │
					  │ (Unlimited)  │
					  └──────────────┘


════════════════════════════════════════════════════════════════════

FLOW - عملیات یک Call

════════════════════════════════════════════════════════════════════

Application Call:
  │
  ▼
device.DeviceCountGet()
  │
  ├─► Logger.OperationStart("DeviceCountGet")
  │     ├─► Generate CorrelationId: A1B2C3D4
  │     ├─► Record StartTime
  │     └─► Write to log:
  │         "⏹️ Operation START | CorrelationId=A1B2C3D4"
  │
  ├─► Internal Logic
  │     ├─► Call _dllWrapper.DeviceCount()
  │     │     └─► Logger.LogDllResult()
  │     │
  │     └─► Check Result & Validation
  │
  └─► Logger.OperationComplete() OR Logger.OperationFailed()
	  └─► Calculate Duration
	  └─► Write to log:
		  "✅ Operation COMPLETE | Duration=125ms"
		  OR
		  "❌ Operation FAILED | Error=..."


════════════════════════════════════════════════════════════════════

FILE STRUCTURE

════════════════════════════════════════════════════════════════════

Project Root/
├── appsettings.json                  [Configuration for Serilog]
├── Source/
│   └── Device/
│       ├── Device.cs                 [+ 4 lines for logging init]
│       ├── DeviceRuntime.cs          [+ 2 lines for error logging]
│       ├── LOGGING_IMPLEMENTATION_SUMMARY.md
│       ├── QUICK_START_LOGGING_3DAYS.md
│       ├── IMPLEMENTATION_GUIDE.md
│       ├── DeviceLogPlan.md
│       └── Logging/                  [NEW FOLDER]
│           ├── DeviceLogger.cs       [Main logger]
│           ├── LogContext.cs         [Context tracking]
│           ├── ErrorTracer.cs        [Error tracking]
│           ├── LoggingExtensions.cs  [Helper methods]
│           ├── LoggingInitializer.cs [Startup init]
│           └── USAGE_EXAMPLES.cs     [Usage examples]
│
└── logs/                             [AUTO-CREATED]
	├── device-device.dll-.txt        [Text logs]
	├── device-device.dll-.json       [JSON logs]
	├── device-device.dll-20240115.txt [Rolled daily]
	└── device-device.dll-20240115.json
```


════════════════════════════════════════════════════════════════════

INTEGRATION POINTS

════════════════════════════════════════════════════════════════════

In Device.cs Constructor:
  │
  ├─► LoggingInitializer.Initialize()         ← Setup logs folder
  │
  └─► _logger = DeviceLogger.GetInstance()    ← Create logger
	  └─► _errorTracer = ErrorTracer.GetInstance()  ← Create tracer


In DeviceRuntime.cs ErrorSet():
  │
  └─► _errorTracer.RecordError()              ← Log all errors


In DeviceRuntime.cs DllWrapperHasError():
  │
  ├─► _logger.LogDllResult()                  ← Log DLL results
  │
  └─► _logger.LogError()                      ← Log DLL errors


════════════════════════════════════════════════════════════════════

DATA FLOW - دادهٔ جریان

════════════════════════════════════════════════════════════════════

Device Operation
	  │
	  ▼
  LogContext ← DeviceLogger.OperationStart()
	  │
	  ├─► CorrelationId (unique)
	  ├─► OperationName  
	  ├─► StartTime
	  ├─► Parameters
	  └─► Metadata
			 │
			 ▼
		 Serilog.ILogger
			 │
			 ├─► Text File Writer
			 │     └─► /logs/device-.txt
			 │
			 └─► JSON File Writer  
				   └─► /logs/device-.json


════════════════════════════════════════════════════════════════════

CORRELATION ID مثال

════════════════════════════════════════════════════════════════════

Call 1:
  ⏹️ Operation START | CorrelationId=A1B2C3D4
	  └─► All related logs share this ID
	  └─► Can trace entire operation
  ✅ Operation COMPLETE | CorrelationId=A1B2C3D4

Call 2:
  ⏹️ Operation START | CorrelationId=E5F6G7H8
	  └─► Different ID for different call
  ❌ Operation FAILED | CorrelationId=E5F6G7H8


════════════════════════════════════════════════════════════════════

ERROR TRACKING FLOW

════════════════════════════════════════════════════════════════════

Operation Occurs
	  │
	  ▼
ErrorSet(ERROR_CODE.DLL_WRAPPER, "Error message")
	  │
	  ▼
_errorTracer.RecordError()
	  │
	  ├─► Add to _errorHistory queue
	  │
	  ├─► Maintain size (max 1000)
	  │
	  └─► Log to Logger
		   │
		   ▼
	   Log Files
		   │
		   ├─► error_report.txt
		   └─► error_report.json

Later:
	  │
	  ▼
errorTracer.GenerateErrorReport()
	  │
	  ├─► Get patterns
	  │   ├─► Count occurrences
	  │   ├─► First/Last timestamp
	  │   └─► Common message
	  │
	  └─► Print to Console or File


════════════════════════════════════════════════════════════════════

ZERO CODE CHANGE APPROACH (Alternative)

════════════════════════════════════════════════════════════════════

Instead of modifying DeviceCountGet():

┌─────────────────────────────────────────────┐
│  Original Code                              │
│  public bool DeviceCountGet() { ... }       │
└─────────────────────────────────────────────┘
			   │
			   ▼ (بدون تغییر کد)
┌─────────────────────────────────────────────┐
│  LoggingExtensions - Wrapper                │
│  device.LogOperation(                       │
│    () => device.DeviceCountGet(),           │
│    "DeviceCountGet"                         │
│  )                                          │
└─────────────────────────────────────────────┘
			   │
			   ├─► Logger.OperationStart()
			   │
			   ├─► Invoke actual method
			   │
			   └─► Logger.OperationComplete/Failed()
				   └─► Automatic logging!

═══════════════════════════════════════════════════════════════════
```

---

## 🔄 Lifecycle Example

```
Time │ Action                      │ Log Output
────┼────────────────────────────┼──────────────────────────────
10ms│ device.DeviceCountGet()    │
	│                            │
20ms│ START logging              │ ⏹️ Operation START | CorrId=A1B2
	│                            │    CorrelationId=A1B2C3D4E5F6
	│                            │
30ms│ Call _dllWrapper.Count()   │ 🔗 DLL CALL | Method=DeviceCount
	│                            │
80ms│ DLL returns 5              │ 🔗 DLL RESULT | Duration=50ms
	│                            │                Result=5
	│                            │
90ms│ Validate result            │ (no log)
	│                            │
100ms│ Return true               │
	│                            │
105ms│ COMPLETE logging          │ ✅ Operation COMPLETE | CorrId=A1B2
	│                            │    Duration=105ms | Result=true
────┴────────────────────────────┴──────────────────────────────

Total Duration: 105ms - All logged with single CorrelationId!
```

---

## 📊 Log File Example

### Text Format

```
2024-01-15 10:30:45.123 +00:00 [INF] 🟢 Logger initialized for Device: Device.dll
2024-01-15 10:30:45.125 +00:00 [INF] ⏹️ Operation START | Operation=DeviceCountGet | CorrelationId=A1B2C3D4 | Params={}
2024-01-15 10:30:45.178 +00:00 [DBG] 🔗 DLL_CALL | Method=DeviceCount | Parameters={}
2024-01-15 10:30:45.230 +00:00 [DBG] 🔗 DLL_RESULT | Method=DeviceCount | Duration=52ms | Result=5
2024-01-15 10:30:45.235 +00:00 [INF] ✅ Operation COMPLETE | Operation=DeviceCountGet | CorrelationId=A1B2C3D4 | Status=SUCCESS | Duration=110ms | Result={Result: true}
```

### JSON Format

```json
{"Timestamp":"2024-01-15 10:30:45.123","Level":"Information","Message":"🟢 Logger initialized for Device: Device.dll","Properties":{"DeviceId":"Device.dll"}}
{"Timestamp":"2024-01-15 10:30:45.125","Level":"Information","Message":"⏹️ Operation START | Operation=DeviceCountGet | CorrelationId=A1B2C3D4","Properties":{"Operation":"DeviceCountGet","CorrelationId":"A1B2C3D4","DeviceId":"Device.dll"}}
{"Timestamp":"2024-01-15 10:30:45.230","Level":"Debug","Message":"🔗 DLL_RESULT | Method=DeviceCount | Duration=52ms | Result=5","Properties":{"Method":"DeviceCount","Duration":52,"DeviceId":"Device.dll"}}
```

