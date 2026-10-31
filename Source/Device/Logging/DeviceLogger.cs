using Serilog;
using Serilog.Formatting.Compact;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSM.Core.Source.Device.Logging
{
    /// <summary>
    /// Central logger for Device operations
    /// Provides structured logging with Serilog
    /// </summary>
    public class DeviceLogger : IDisposable
    {
        private static readonly Dictionary<string, DeviceLogger> _instances = new Dictionary<string, DeviceLogger>();
        private readonly ILogger _logger;
        private readonly string _deviceId;
        private bool _disposed = false;
        private static readonly object _instancesLock = new object();

        private DeviceLogger(string deviceId)
        {
            _deviceId = deviceId;
            // Sanitize DeviceId for file name
            string safeDeviceId = new string(deviceId.Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_').ToArray());
            if (string.IsNullOrWhiteSpace(safeDeviceId)) safeDeviceId = "DEFAULT";
            // Log directory
            string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logsDirectory);
            // Log file paths
            string textLogPath = Path.Combine(logsDirectory, $"device-{safeDeviceId}-.txt");
            string jsonLogPath = Path.Combine(logsDirectory, $"device-{safeDeviceId}-structured-.json");
            // Serilog Configuration
            _logger = new LoggerConfiguration().MinimumLevel.Debug()
                // Text Log
                .WriteTo.File(path: textLogPath, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                              rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, rollOnFileSizeLimit: true, retainedFileCountLimit: 100)
                // Structured JSON Log
                .WriteTo.File(formatter: new CompactJsonFormatter(), path: jsonLogPath,
                              rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, rollOnFileSizeLimit: true, retainedFileCountLimit: 100)
                // Enrich Logs
                .Enrich.FromLogContext().Enrich.WithProperty("DeviceId", deviceId).CreateLogger();
            // Log initialization
            _logger.Information("Logger initialized for Device: {DeviceId}", deviceId);
        }
        /// <summary>
        /// Get or create logger instance for device
        /// </summary>
        public static DeviceLogger GetInstance(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) deviceId = "DEFAULT";
            lock (_instancesLock)
            {
                DeviceLogger logger;
                if (!_instances.TryGetValue(deviceId, out logger)) { logger = new DeviceLogger(deviceId); _instances.Add(deviceId, logger); }
                return logger;
            }
        }
        // ================== OPERATION LIFECYCLE ==================
        /// <summary>
        /// Log operation start
        /// </summary>
        public LogContext OperationStart(string operationName, object parameters = null)
        {
            if (string.IsNullOrWhiteSpace(operationName)) throw new ArgumentException("Operation name cannot be null or empty", nameof(operationName));
            string correlationId = Guid.NewGuid().ToString("N").ToUpperInvariant();
            var context = new LogContext { CorrelationId = correlationId, OperationName = operationName, StartTime = DateTime.UtcNow, Parameters = parameters };
            _logger.Information("OPERATION_START | Operation={Operation} | CorrelationId={CorrelationId} | Params={@Parameters}", operationName, correlationId, parameters ?? new { });
            return context;
        }
        /// <summary>
        /// Log operation successful completion
        /// </summary>
        public void OperationComplete(LogContext context, string status = "SUCCESS", object result = null)
        {
            if (context == null) return;
            if (string.IsNullOrWhiteSpace(status)) status = "UNKNOWN";
            var duration = DateTime.UtcNow - context.StartTime;
            _logger.Information("OPERATION_COMPLETE | Operation={Operation} | CorrelationId={CorrelationId} | Status={Status} | DurationMs={DurationMs} | Result={@Result}",
                                context.OperationName, context.CorrelationId, status, duration.TotalMilliseconds, result ?? new { });
        }
        /// <summary>
        /// Log operation failure
        /// </summary>
        public void OperationFailed(LogContext context, Exception ex)
        {
            if (context == null) return;
            var duration = DateTime.UtcNow - context.StartTime;
            _logger.Error(ex, "❌ OPERATION_FAILED | Operation={Operation} | CorrelationId={CorrelationId} | Duration={DurationMs}ms | Error={ErrorMessage}", context.OperationName,
                          context.CorrelationId, duration.TotalMilliseconds, ex?.Message ?? "Unknown");
        }
        // ================== ERROR LOGGING ==================
        /// <summary>
        /// Log error with code and message
        /// </summary>
        public void LogError(string errorCode, string errorMessage, string operation, Exception ex = null)
        {
            _logger.Error(ex, "🔴 ERROR | Code={ErrorCode} | Message={ErrorMessage} | Operation={Operation} | Time={Timestamp:yyyy-MM-dd HH:mm:ss.fff}", errorCode, errorMessage,
                          operation, DateTime.UtcNow);
        }
        /// <summary>
        /// Log warning message
        /// </summary>
        public void LogWarning(string message, string operation)
        {
            _logger.Warning("⚠️  WARNING | Message={Message} | Operation={Operation} | Time={Timestamp:yyyy-MM-dd HH:mm:ss.fff}", message, operation, DateTime.UtcNow);
        }
        // ================== DLL OPERATIONS ==================
        /// <summary>
        /// Log DLL method call
        /// </summary>
        public void LogDllCall(string methodName, Dictionary<string, object> parameters = null)
        {
            _logger.Debug("🔗 DLL_CALL | Method={Method} | Parameters={@Parameters}", methodName, parameters ?? new Dictionary<string, object>());
        }
        /// <summary>
        /// Log DLL method result
        /// </summary>
        public void LogDllResult(string methodName, object result, long durationMs)
        {
            _logger.Debug("🔗 DLL_RESULT | Method={Method} | Duration={DurationMs}ms | Result={@Result}", methodName, durationMs, result ?? new { });
        }
        // ================== STATE CHANGES ==================
        /// <summary>
        /// Log internal state change
        /// </summary>
        public void LogStateChange(string oldState, string newState, string reason)
        {
            _logger.Information("🔄 STATE_CHANGE | From={OldState} | To={NewState} | Reason={Reason} | Time={Timestamp:yyyy-MM-dd HH:mm:ss.fff}", oldState, newState, reason, DateTime.UtcNow);
        }
        /// <summary>
        /// Log flag change
        /// </summary>
        public void LogFlagChange(string flagName, object oldValue, object newValue)
        {
            _logger.Debug("🚩 FLAG_CHANGE | Flag={Flag} | From={OldValue} | To={NewValue}", flagName, oldValue, newValue);
        }
        public void Dispose()
        {
            if (_disposed) return;
            Log.CloseAndFlush();
            _disposed = true;
        }
    }
    public class LogContext
    {
        public string CorrelationId { get; set; }
        public string OperationName { get; set; }
        public DateTime StartTime { get; set; }
        public object Parameters { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public override string ToString() { return $"[{CorrelationId}] {OperationName}"; }
        public TimeSpan GetDuration() { return DateTime.UtcNow - StartTime; }
    }
    public class ErrorTracer
    {
        private static ErrorTracer _instance;
        private readonly Queue<ErrorRecord> _errorHistory = new Queue<ErrorRecord>();
        private readonly DeviceLogger _logger;
        private const int MAX_HISTORY = 1000;
        public class ErrorRecord
        {
            public DateTime Timestamp { get; set; }
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
            public string Operation { get; set; }
            public Exception Exception { get; set; }
            public int Layer { get; set; } // Controller=1, Workflow=2, Base=3, DLL=4
            public string LayerName => Layer == 1 ? "Controller" : Layer == 2 ? "Workflow" : Layer == 3 ? "Base" : Layer == 4 ? "DLL" : "Unknown";
        }
        private ErrorTracer(DeviceLogger logger) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }
        /// <summary>
        /// Get or create singleton instance
        /// </summary>
        public static ErrorTracer GetInstance(DeviceLogger logger) { return _instance ?? (_instance = new ErrorTracer(logger)); }
        /// <summary>
        /// Record an error occurrence
        /// </summary>
        public void RecordError(Device.ERROR_CODE code, string message, string operation, int layer = 1, Exception ex = null)
        {
            var record = new ErrorRecord { Timestamp = DateTime.UtcNow, ErrorCode = code.ToString(), ErrorMessage = message, Operation = operation, Layer = layer, Exception = ex };
            _errorHistory.Enqueue(record);
            // Maintain maximum size
            if (_errorHistory.Count > MAX_HISTORY) _errorHistory.Dequeue();
            // Log the error
            _logger?.LogError(code.ToString(), message, $"{operation} [{record.LayerName}]", ex);
        }
        /// <summary>
        /// Get error patterns grouped by code
        /// </summary>
        public List<(string Code, int Count, DateTime FirstOccurrence, DateTime LastOccurrence, string CommonMessage)> GetErrorPatterns()
        {
            return _errorHistory.GroupBy(e => e.ErrorCode).Select(g => (g.Key, g.Count(), g.Min(x => x.Timestamp), g.Max(x => x.Timestamp),
                                         g.OrderByDescending(x => x.Timestamp).First().ErrorMessage)).OrderByDescending(x => x.Item2).ToList();
        }
        /// <summary>
        /// Get recent errors
        /// </summary>
        public List<ErrorRecord> GetRecentErrors(int count = 20) { return _errorHistory.OrderByDescending(e => e.Timestamp).Take(count).ToList(); }
        /// <summary>
        /// Get errors by operation
        /// </summary>
        public List<ErrorRecord> GetErrorsByOperation(string operation) { return _errorHistory.Where(e => e.Operation == operation).OrderByDescending(e => e.Timestamp).ToList(); }
        /// <summary>
        /// Get errors by error code
        /// </summary>
        public List<ErrorRecord> GetErrorsByCode(string errorCode) { return _errorHistory.Where(e => e.ErrorCode == errorCode).OrderByDescending(e => e.Timestamp).ToList(); }
        /// <summary>
        /// Get critical errors (last 24 hours)
        /// </summary>
        public List<ErrorRecord> GetCriticalErrors()
        {
            var twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);
            return _errorHistory.Where(e => e.Timestamp >= twentyFourHoursAgo).OrderByDescending(e => e.Timestamp).ToList();
        }
        /// <summary>
        /// Get total error count
        /// </summary>
        public int GetTotalErrorCount() { return _errorHistory.Count; }
        /// <summary>
        /// Generate comprehensive error report
        /// </summary>
        public string GenerateErrorReport()
        {
            var patterns = GetErrorPatterns();
            var sb = new StringBuilder();
            sb.AppendLine("════════════════════════════════════════════════════════════");
            sb.AppendLine("                      ERROR REPORT                          ");
            sb.AppendLine("════════════════════════════════════════════════════════════");
            sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Total Errors: {_errorHistory.Count}");
            sb.AppendLine($"Unique Error Codes: {patterns.Count}");
            sb.AppendLine();
            sb.AppendLine("Top 10 Error Patterns:");
            int index = 1;
            foreach (var (code, count, firstOcc, lastOcc, lastMsg) in patterns.Take(10))
            {
                sb.AppendLine($"  {index}. [{code}] - Occurred {count} times");
                sb.AppendLine($"     ├─ First: {firstOcc:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"     ├─ Last:  {lastOcc:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"     └─ Message: {lastMsg}");
                sb.AppendLine();
                index++;
            }
            sb.AppendLine("Recent 5 Errors:");
            var recent = GetRecentErrors(5);
            for (int i = 0; i < recent.Count; i++)
            {
                var err = recent[i];
                sb.AppendLine($"  {i + 1}. [{err.ErrorCode}] at {err.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
                sb.AppendLine($"     ├─ Operation: {err.Operation}");
                sb.AppendLine($"     ├─ Layer: {err.LayerName}");
                sb.AppendLine($"     └─ Message: {err.ErrorMessage}");
                sb.AppendLine();
            }
            sb.AppendLine("════════════════════════════════════════════════════════════");
            return sb.ToString();
        }
        /// <summary>
        /// Clear error history
        /// </summary>
        public void ClearHistory() { _errorHistory.Clear(); }
        /// <summary>
        /// Export error history as CSV
        /// </summary>
        public string ExportAsCSV()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Timestamp,ErrorCode,Operation,Layer,Message");
            foreach (var error in _errorHistory.OrderBy(e => e.Timestamp))
                sb.AppendLine($"\"{error.Timestamp:yyyy-MM-dd HH:mm:ss.fff}\",\"{error.ErrorCode}\",\"{error.Operation}\",\"{error.LayerName}\",\"{error.ErrorMessage}\"");
            return sb.ToString();
        }
    }
    public static class LoggingInitializer
    {
        private static bool _initialized = false;
        /// <summary>
        /// Initialize logging system
        /// Call this once at application startup
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            try
            {
                // Create logs directory if it doesn't exist
                string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logsDirectory)) Directory.CreateDirectory(logsDirectory);
                _initialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize logging: {ex.Message}");
            }
        }
        /// <summary>
        /// Check if logging is initialized
        /// </summary>
        public static bool IsInitialized => _initialized;
    }
    public static class LoggingExtensions
    {
        /// <summary>
        /// Wraps a returning operation with automatic logging
        /// Usage: return device.LogOperation(() => operation(), "OperationName");
        /// </summary>
        public static T LogOperation<T>(this Device device, Func<T> operation, string operationName)
        {
            if (string.IsNullOrEmpty(operationName)) throw new ArgumentException("Operation name cannot be null or empty", nameof(operationName));
            var logger = DeviceLogger.GetInstance(device?.DllFileName ?? "Device");
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
        /// Wraps a void operation with automatic logging
        /// Usage: device.LogOperation(() => DoSomething(), "OperationName");
        /// </summary>
        public static void LogOperation(this Device device, Action operation, string operationName)
        {
            if (string.IsNullOrEmpty(operationName)) throw new ArgumentException("Operation name cannot be null or empty", nameof(operationName));
            var logger = DeviceLogger.GetInstance(device?.DllFileName ?? "Device");
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
        /// <summary>
        /// Get logger instance for the device
        /// </summary>
        public static DeviceLogger GetLogger(this Device device) { return DeviceLogger.GetInstance(device?.DllFileName ?? "Device"); }
        /// <summary>
        /// Get error tracer instance for the device
        /// </summary>
        public static ErrorTracer GetErrorTracer(this Device device) { var logger = GetLogger(device); return ErrorTracer.GetInstance(logger); }
    }

}
