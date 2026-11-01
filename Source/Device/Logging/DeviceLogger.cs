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
        /// <summary>
        /// Stores all created DeviceLogger instances.
        /// </summary>
        private static readonly Dictionary<string, DeviceLogger> _instances = new Dictionary<string, DeviceLogger>();
        /// <summary>
        /// Provides logging methods for the current device.
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// Gets the unique identifier of the device.
        /// </summary>
        private readonly string _deviceId;
        /// <summary>
        /// Indicates whether this logger has been disposed.
        /// </summary>
        private bool _disposed = false;
        /// <summary>
        /// Synchronizes access to the logger instances collection.
        /// </summary>
        private static readonly object _instancesLock = new object();
        /// <summary>
        /// Provides error tracking for the current device.
        /// </summary>
        private readonly ErrorTracer _errorTracer;

        /// <summary>
        /// Stores information about a device operation for logging and tracking.
        /// </summary>
        public class LogContext
        {
            /// <summary>
            /// Gets or sets the unique identifier of the operation.
            /// </summary>
            public string CorrelationId { get; set; }
            /// <summary>
            /// Gets or sets the name of the operation.
            /// </summary>
            public string OperationName { get; set; }
            /// <summary>
            /// Gets or sets the operation start time in UTC.
            /// </summary>
            public DateTime StartTime { get; set; }
            /// <summary>
            /// Gets or sets the parameters provided when the operation started.
            /// </summary>
            public object Parameters { get; set; }
            /// <summary>
            /// Gets or sets additional information about the operation.
            /// </summary>
            public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
            /// <summary>
            /// Returns a short text representation of the operation context.
            /// </summary>
            /// <returns>The correlation ID and operation name.</returns>
            public override string ToString() { return $"[{CorrelationId}] {OperationName}"; }
            /// <summary>
            /// Gets the elapsed time since the operation started.
            /// </summary>
            /// <returns>The elapsed time as a TimeSpan value.</returns>

            public TimeSpan GetDuration() { return DateTime.UtcNow - StartTime; }
        }

        /// <summary>
        /// Gets the error tracer associated with this device logger.
        /// </summary>
        public ErrorTracer Errors
        {
            get { return _errorTracer; }
        }

        /// <summary>
        /// Initializes a new DeviceLogger for the specified device.
        /// </summary>
        /// <param name="deviceId">The unique identifier of the device.</param>
        private DeviceLogger(string deviceId)
        {
            this._deviceId = deviceId;
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
            this._logger = new LoggerConfiguration().MinimumLevel.Debug()
                // Text Log
                .WriteTo.File(path: textLogPath, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                              rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, rollOnFileSizeLimit: true, retainedFileCountLimit: 100)
                // Structured JSON Log
                .WriteTo.File(formatter: new CompactJsonFormatter(), path: jsonLogPath,
                              rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, rollOnFileSizeLimit: true, retainedFileCountLimit: 100)
                // Enrich Logs
                .Enrich.FromLogContext().Enrich.WithProperty("DeviceId", deviceId).CreateLogger();
            // Log initialization
            this._logger.Information("Logger initialized for Device: {DeviceId}", deviceId);
            _errorTracer = new ErrorTracer(this);
        }
        /// <summary>
        /// Gets the existing DeviceLogger or creates a new instance.
        /// </summary>
        /// <param name="deviceId">The unique identifier of the device.</param>
        /// <returns>The DeviceLogger instance for the specified device.</returns>
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
        /// Logs the start of an operation and creates its context.
        /// </summary>
        /// <param name="operationName">The name of the operation.</param>
        /// <param name="parameters">Optional operation parameters.</param>
        /// <returns>The context of the started operation.</returns>
        public LogContext OperationStart(string operationName, object parameters = null)
        {
            if (string.IsNullOrWhiteSpace(operationName)) throw new ArgumentException("Operation name cannot be null or empty", nameof(operationName));
            string correlationId = Guid.NewGuid().ToString("N").ToUpperInvariant();
            var context = new LogContext { CorrelationId = correlationId, OperationName = operationName, StartTime = DateTime.UtcNow, Parameters = parameters };
            this._logger.Information("OPERATION_START | Operation={Operation} | CorrelationId={CorrelationId} | Params={@Parameters}", operationName, correlationId, parameters ?? new { });
            return context;
        }
        /// <summary>
        /// Logs the successful or normal completion of an operation.
        /// </summary>
        /// <param name="context">The context of the operation.</param>
        /// <param name="status">The final status of the operation.</param>
        /// <param name="result">Optional operation result data.</param>
        public void OperationComplete(LogContext context, string status = "SUCCESS", object result = null)
        {
            if (context == null) return;
            if (string.IsNullOrWhiteSpace(status)) status = "UNKNOWN";
            var duration = DateTime.UtcNow - context.StartTime;
            this._logger.Information("OPERATION_COMPLETE | Operation={Operation} | CorrelationId={CorrelationId} | Status={Status} | DurationMs={DurationMs} | Result={@Result}",
                                     context.OperationName, context.CorrelationId, status, duration.TotalMilliseconds, result ?? new { });
        }
        /// <summary>
        /// Logs an operation failure with its status and error details.
        /// </summary>
        /// <param name="context">The context of the failed operation.</param>
        /// <param name="ex">The exception associated with the failure.</param>
        /// <param name="status">The failure status.</param>
        /// <param name="errorCode">The error code, if available.</param>
        /// <param name="errorMessage">The error description.</param>
        public void OperationFailed( LogContext context, Exception ex = null, string status = "FAIL", string errorCode = null, string errorMessage = null)
        {
            if (context == null) return;
            if (string.IsNullOrWhiteSpace(status)) status = "FAIL";
            if (string.IsNullOrWhiteSpace(errorMessage)) errorMessage = ex != null ? ex.Message : status;
            var duration = DateTime.UtcNow - context.StartTime;
            this._logger.Error(ex, "OPERATION_FAILED | Operation={Operation} | CorrelationId={CorrelationId} | Status={Status} | ErrorCode={ErrorCode} | DurationMs={DurationMs} | " +
                               "ErrorMessage={ErrorMessage}", context.OperationName, context.CorrelationId, status, errorCode, duration.TotalMilliseconds, errorMessage);
        }
        // ================== ERROR LOGGING ==================
        /// <summary>
        /// Logs an error message with its code and operation name.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage">The error description.</param>
        /// <param name="operation">The name of the related operation.</param>
        /// <param name="ex">The related exception, if available.</param>
        public void LogError(string errorCode, string errorMessage, string operation, Exception ex = null)
        {
            if (string.IsNullOrWhiteSpace(errorCode))  errorCode = "UNKNOWN";
            if (string.IsNullOrWhiteSpace(operation)) operation = "UNKNOWN";
            if (string.IsNullOrWhiteSpace(errorMessage)) errorMessage = ex != null ? ex.Message : "No error message provided.";
            this._logger.Error(ex, "ERROR | ErrorCode={ErrorCode} | ErrorMessage={ErrorMessage} | Operation={Operation}", errorCode, errorMessage, operation);
        }
        /// <summary>
        /// Logs a warning message for the specified operation.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="operation">The name of the related operation.</param>
        public void LogWarning(string message, string operation)
        {
            if (string.IsNullOrWhiteSpace(message)) message = "No warning message provided.";
            if (string.IsNullOrWhiteSpace(operation)) operation = "UNKNOWN";
            this._logger.Warning("WARNING | Message={Message} | Operation={Operation}", message, operation);
        }
        // ================== DLL OPERATIONS ==================
        /// <summary>
        /// Logs a DLL function call with its method name and parameters.
        /// </summary>
        /// <param name="methodName">The name of the DLL function.</param>
        /// <param name="parameters">The function parameters, if available.</param>
        public void LogDllCall(string methodName, Dictionary<string, object> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(methodName)) methodName = "UNKNOWN";
            this._logger.Debug("DLL_CALL | Method={Method} | Parameters={@Parameters}", methodName, parameters ?? new Dictionary<string, object>());
        }
        /// <summary>
        /// Logs the result and execution time of a DLL function.
        /// </summary>
        /// <param name="methodName">The name of the DLL function.</param>
        /// <param name="result">The function return value.</param>
        /// <param name="durationMs">The execution time in milliseconds.</param>
        public void LogDllResult(string methodName, object result, long durationMs)
        {
            if (string.IsNullOrWhiteSpace(methodName)) methodName = "UNKNOWN";
            this._logger.Debug("DLL_RESULT | Method={Method} | Result={Result} | DurationMs={DurationMs}", methodName, result, durationMs);
        }
        // ================== STATE CHANGES ==================
        /// <summary>
        /// Logs a device state change with its reason.
        /// </summary>
        /// <param name="oldState">The previous device state.</param>
        /// <param name="newState">The new device state.</param>
        /// <param name="reason">The reason for the state change.</param>
        public void LogStateChange(string oldState, string newState, string reason)
        {
            if (string.IsNullOrWhiteSpace(oldState)) oldState = "UNKNOWN";
            if (string.IsNullOrWhiteSpace(newState)) newState = "UNKNOWN";
            if (string.IsNullOrWhiteSpace(reason)) reason = "Not specified";
            this._logger.Information("STATE_CHANGE | OldState={OldState} | NewState={NewState} | Reason={Reason}", oldState, newState, reason);
        }
        /// <summary>
        /// Logs a change in a device flag or control value.
        /// </summary>
        /// <param name="flagName">The name of the flag or variable.</param>
        /// <param name="oldValue">The previous value.</param>
        /// <param name="newValue">The new value.</param>
        public void LogFlagChange(string flagName, object oldValue, object newValue)
        {
            if (string.IsNullOrWhiteSpace(flagName)) flagName = "UNKNOWN";
            this._logger.Debug("FLAG_CHANGE | FlagName={FlagName} | OldValue={OldValue} | NewValue={NewValue}", flagName, oldValue, newValue);
        }
        /// <summary>
        /// Releases the resources used by this logger.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed) return;
            (this._logger as IDisposable)?.Dispose();
            this._disposed = true;
        }

        public class ErrorTracer
        {
            /// <summary>
            /// Stores recent error records in memory.
            /// </summary>
            private readonly Queue<ErrorRecord> _errorHistory = new Queue<ErrorRecord>();
            /// <summary>
            /// Provides logging services for error records.
            /// </summary>
            private readonly DeviceLogger _logger;
            /// <summary>
            /// Synchronizes access to the error history.
            /// </summary>
            private readonly object _historyLock = new object();
            /// <summary>
            /// Defines the maximum number of error records kept in memory.
            /// </summary>
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
            /// <summary>
            /// Initializes a new ErrorTracer for the specified device logger.
            /// </summary>
            /// <param name="logger">The logger associated with the device.</param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when the logger is null.
            /// </exception>
            internal ErrorTracer(DeviceLogger logger)
            {
                if (logger == null) throw new ArgumentNullException(nameof(logger));
                this._logger = logger;
            }
            /// <summary>
            /// Records an error in the device error history and log file.
            /// </summary>
            /// <param name="code">The error code.</param>
            /// <param name="message">The error description.</param>
            /// <param name="operation">The operation where the error occurred.</param>
            /// <param name="layer">The device layer where the error occurred.</param>
            /// <param name="ex">The related exception, if available.</param>
            public void RecordError(Device.ERROR_CODE code, string message, string operation, int layer = 1, Exception ex = null)
            {
                var record = new ErrorRecord { Timestamp = DateTime.UtcNow, ErrorCode = code.ToString(), ErrorMessage = message, Operation = operation, Layer = layer, Exception = ex };
                // Store the error and maintain the history limit.
                lock (_historyLock)
                {
                    this._errorHistory.Enqueue(record);
                    if (this._errorHistory.Count > MAX_HISTORY) this._errorHistory.Dequeue();
                }
                // Write the error to the log file.
                this._logger.LogError(record.ErrorCode, record.ErrorMessage, $"{record.Operation} [{record.LayerName}]", record.Exception);
            }
            /// <summary>
            /// Gets error patterns grouped by error code.
            /// </summary>
            /// <returns>
            /// A list of error patterns ordered by occurrence count.
            /// </returns>
            public List<(string Code, int Count, DateTime FirstOccurrence, DateTime LastOccurrence, string LastMessage)> GetErrorPatterns()
            {
                List<ErrorRecord> snapshot;
                lock (this._historyLock) snapshot = this._errorHistory.ToList();
                return snapshot.GroupBy(e => e.ErrorCode).Select(g => (Code: g.Key, Count: g.Count(), FirstOccurrence: g.Min(x => x.Timestamp), LastOccurrence: g.Max(x => x.Timestamp),
                        LastMessage: g.OrderByDescending(x => x.Timestamp).First().ErrorMessage)).OrderByDescending(x => x.Count).ToList();
            }
            /// <summary>
            /// Gets the most recent errors.
            /// </summary>
            /// <param name="count">The maximum number of errors to return.</param>
            /// <returns>A list of recent errors, ordered from newest to oldest.</returns>
            public List<ErrorRecord> GetRecentErrors(int count = 20)
            {
                List<ErrorRecord> snapshot;
                lock (this._historyLock) snapshot = this._errorHistory.ToList();
                return snapshot.OrderByDescending(e => e.Timestamp).Take(count).ToList();
            }
            /// <summary>
            /// Gets errors associated with a specific operation.
            /// </summary>
            /// <param name="operation">The operation name to search for.</param>
            /// <returns>A list of matching errors, ordered from newest to oldest.</returns>
            public List<ErrorRecord> GetErrorsByOperation(string operation)
            {
                List<ErrorRecord> snapshot;
                lock (this._historyLock) snapshot = this._errorHistory.ToList();
                return snapshot.Where(e => e.Operation == operation).OrderByDescending(e => e.Timestamp).ToList();
            }
            /// <summary>
            /// Gets errors associated with a specific error code.
            /// </summary>
            /// <param name="errorCode">The error code to search for.</param>
            /// <returns>A list of matching errors, ordered from newest to oldest.</returns>
            public List<ErrorRecord> GetErrorsByCode(string errorCode)
            {
                List<ErrorRecord> snapshot;
                lock (this._historyLock) snapshot = this._errorHistory.ToList();
                return snapshot.Where(e => e.ErrorCode == errorCode).OrderByDescending(e => e.Timestamp).ToList();
            }
            /// <summary>
            /// Gets errors recorded during the last 24 hours.
            /// </summary>
            /// <returns>A list of errors, ordered from newest to oldest.</returns>
            public List<ErrorRecord> GetErrorsLast24Hours()
            {
                DateTime twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);
                List<ErrorRecord> snapshot;
                lock (this._historyLock) snapshot = this._errorHistory.ToList();
                return snapshot.Where(e => e.Timestamp >= twentyFourHoursAgo).OrderByDescending(e => e.Timestamp).ToList();
            }
            /// <summary>
            /// Gets critical errors based on the defined critical error criteria.
            /// This method will be implemented after the critical error rules are defined.
            /// </summary>
            /// <returns>A list of critical errors.</returns>
            public List<ErrorRecord> GetCriticalErrors()
            {
                // TODO: Implement after defining the critical error criteria.
                return new List<ErrorRecord>();
            }
            /// <summary>
            /// Gets the total number of errors currently stored in history.
            /// </summary>
            /// <returns>The number of stored errors.</returns>
            public int GetTotalErrorCount() { lock (this._historyLock) return this._errorHistory.Count; }
           
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
    }
    /// <summary>
    /// Tracks error history and provides error analysis utilities.
    /// </summary>
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
