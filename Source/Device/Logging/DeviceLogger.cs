using Serilog;
using Serilog.Formatting.Compact;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSM.Core.Source.Device
{
    public partial class Device
    {
        /// <summary>
        /// Central logger for Device operations
        /// Provides structured logging with Serilog
        /// </summary>
        private class DeviceLogger : IDisposable
        {
            private readonly string _logId;
            /// <summary>
            /// Provides logging methods for the current device.
            /// </summary>
            private readonly ILogger _logger;
            /// <summary>
            /// Indicates whether this logger has been disposed.
            /// </summary>
            private bool _disposed = false;
            /// <summary>
            /// Stores active function logging contexts in nested execution order.
            /// </summary>
            private readonly Stack<FuncLogContext> _funcContextStack;

            /// <summary>
            /// Represents the logging context of a single function execution.
            /// Each context is uniquely identified and linked to its parent function.
            /// </summary>
            private sealed class FuncLogContext
            {
                /// <summary>
                /// Gets the unique identifier of this function execution.
                /// </summary>
                public string FuncId { get; private set; }
                /// <summary>
                /// Gets the identifier of the parent function.
                /// Null if this function is a root function.
                /// </summary>
                public string ParentFuncId { get; private set; }
                /// <summary>
                /// Gets the name of the function.
                /// </summary>
                public string FunctionName { get; private set; }
                /// <summary>
                /// Gets the type of the function.
                /// </summary>
                public Device.FUNC_TYPE FunctionType { get; private set; }
                /// <summary>
                /// Gets the result of the function execution.
                /// The initial value is UNKNOWN until the function ends.
                /// </summary>
                public Device.FUNC_RESULT FuncResult { get; private set; }
                /// <summary>
                /// Gets the UTC time when the function started.
                /// </summary>
                public DateTime StartTime { get; private set; }
                /// <summary>
                /// Gets the UTC time when the function ended.
                /// Null while the function is still active.
                /// </summary>
                public DateTime? EndTime { get; private set; }
                /// <summary>
                /// Gets the duration of the function execution.
                /// Null while the function is still active.
                /// </summary>
                public TimeSpan? TimeDuration { get; private set; }
                /// <summary>
                /// Gets the parameters associated with the function.
                /// </summary>
                public object Parameters { get; private set; }
                /// <summary>
                /// Initializes a new function logging context.
                /// </summary>
                /// <param name="parentFuncId">
                /// The identifier of the parent function, or null for a root function.
                /// </param>
                /// <param name="functionName">The name of the function.</param>
                /// <param name="functionType">The type of the function.</param>
                /// <param name="parameters">The parameters associated with the function.</param>
                public FuncLogContext(string parentFuncId, string functionName, Device.FUNC_TYPE functionType, object parameters = null)
                {
                    this.FuncId = Guid.NewGuid().ToString("N");
                    this.ParentFuncId = parentFuncId;
                    this.FunctionName = functionName;
                    this.FunctionType = functionType;
                    this.FuncResult = Device.FUNC_RESULT.UNKNOWN;
                    this.StartTime = DateTime.UtcNow;
                    this.Parameters = parameters;
                }
                /// <summary>
                /// Completes the function execution by assigning its result,
                /// recording its end time, and calculating its duration.
                /// </summary>
                /// <param name="funcResult">The final result of the function execution.</param>
                public void Complete(Device.FUNC_RESULT funcResult)
                {
                    DateTime endTime = DateTime.UtcNow;
                    this.FuncResult = funcResult;
                    this.EndTime = endTime;
                    this.TimeDuration = endTime - this.StartTime;
                }
            }

            // ================== Constructor & Decunstructor ==================
            /// <summary>
            /// Initializes a new DeviceLogger with a unique LogId.
            /// Creates the Serilog logger and its associated ErrorTracer.
            /// </summary>
            internal DeviceLogger()
            {
                // Initialize function context stack
                this._funcContextStack = new Stack<FuncLogContext>();
                // Generate unique LogId
                this._logId = Guid.NewGuid().ToString("N");
                // Sanitize LogId for file name
                string safeLogId = new string(this._logId.Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_').ToArray());
                if (string.IsNullOrWhiteSpace(safeLogId)) safeLogId = "DEFAULT";
                // Initialize log directory
                string logsDirectory = InitializeLogging();
                // Define log file paths
                string textLogPath = Path.Combine(logsDirectory, $"device-{safeLogId}-.txt");
                string jsonLogPath = Path.Combine(logsDirectory, $"device-{safeLogId}-structured-.json");
                // Configure Serilog
                this._logger = new LoggerConfiguration().MinimumLevel.Debug()
                    // Text Log
                    .WriteTo.File(path: textLogPath, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                                  rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, rollOnFileSizeLimit: true, retainedFileCountLimit: 100)
                    // Structured JSON Log
                    .WriteTo.File(formatter: new CompactJsonFormatter(), path: jsonLogPath,
                                  rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10485760, rollOnFileSizeLimit: true, retainedFileCountLimit: 100)
                    // Enrich Logs
                    .Enrich.FromLogContext().Enrich.WithProperty("LogId", this._logId).CreateLogger();
                // Log initialization
                this._logger.Information("Logger initialized for Device: {LogId}", this._logId);
            }
            /// <summary>
            /// Initializes the logging directory.
            /// </summary>
            /// <returns>The full path of the logging directory.</returns>
            private static string InitializeLogging()
            {
                string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(logsDirectory);
                return logsDirectory;
            }
            /// <summary>
            /// Releases the resources used by this logger.
            /// </summary>
            public void Dispose()
            {
                if (this._disposed) return;
                this._disposed = true;
                if (!this.FuncContextIsEmpty()) this._logger.Warning("Logger disposed while {FuncCount} function context(s) were still active.", this._funcContextStack.Count);
                IDisposable disposableLogger = this._logger as IDisposable;
                if (disposableLogger != null) disposableLogger.Dispose();
            }

            // ================== Error Report ==================
            /// <summary>
            /// Generates a textual report of the recorded errors.
            /// </summary>
            /// <returns>A formatted error report.</returns>
            public string GenerateErrorSummaryReport(List<ErrorTracer.ErrorRecord> snapshot)
            {
                //List<ErrorTracer.ErrorRecord> snapshot = this._errorTracer.GetHistory();
                var patterns = snapshot.GroupBy(e => e.ErrorCode).Select(g => new
                {
                    Code = g.Key,
                    Count = g.Count(),
                    FirstOccurrence = g.Min(x => x.Timestamp),
                    LastOccurrence = g.Max(x => x.Timestamp),
                    LastMessage = g.OrderByDescending(x => x.Timestamp).First().ErrorMessage
                }).OrderByDescending(x => x.Count).ToList();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("════════════════════════════════════════════════════════════");
                sb.AppendLine("                      ERROR REPORT                          ");
                sb.AppendLine("════════════════════════════════════════════════════════════");
                sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}");
                sb.AppendLine($"Total Errors: {snapshot.Count}");
                sb.AppendLine($"Unique Error Codes: {patterns.Count}");
                sb.AppendLine();
                sb.AppendLine("Top 10 Error Patterns:");
                int index = 1;
                foreach (var pattern in patterns.Take(10))
                {
                    sb.AppendLine($"  {index}. [{pattern.Code}] - Occurred {pattern.Count} times");
                    sb.AppendLine($"     ├─ First: {pattern.FirstOccurrence:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine($"     ├─ Last:  {pattern.LastOccurrence:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine($"     └─ Message: {pattern.LastMessage}");
                    sb.AppendLine();
                    index++;
                }
                sb.AppendLine("Recent 5 Errors:");
                List<ErrorTracer.ErrorRecord> recent = snapshot.OrderByDescending(e => e.Timestamp).Take(5).ToList();
                for (int i = 0; i < recent.Count; i++)
                {
                    ErrorTracer.ErrorRecord err = recent[i];
                    sb.AppendLine($"  {i + 1}. [{err.ErrorCode}] at {err.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
                    sb.AppendLine($"     ├─ Function: {err.FunctionName}");
                    sb.AppendLine($"     ├─ Type: {err.FunctionType}");
                    sb.AppendLine($"     └─ Message: {err.ErrorMessage}");
                    sb.AppendLine();
                }
                sb.AppendLine("════════════════════════════════════════════════════════════");
                return sb.ToString();
            }
            /// <summary>
            /// Generates a detailed text report containing a specified number
            /// of error records, ordered by timestamp.
            /// </summary>
            /// <param name="snapshot">
            /// A snapshot of the error history retrieved from ErrorTracer.
            /// </param>
            /// <param name="descending">
            /// Determines the order of error records.
            /// True: Newest to oldest.
            /// False: Oldest to newest.
            /// </param>
            /// <param name="maxCount">
            /// Maximum number of error records to include in the report.
            /// Zero means all available error records.
            /// </param>
            /// <returns>
            /// A formatted text report containing the selected error records.
            /// </returns>
            public string GenerateFullErrorReport(List<ErrorTracer.ErrorRecord> snapshot, bool descending = true, int maxCount = 0)
            {
                if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
                if (maxCount < 0) throw new ArgumentOutOfRangeException(nameof(maxCount));
                StringBuilder sb = new StringBuilder();
                // Determine the ordering of error records
                IEnumerable<ErrorTracer.ErrorRecord> orderedErrors = descending ? snapshot.OrderByDescending(error => error.Timestamp) : snapshot.OrderBy(error => error.Timestamp);
                // Apply the maximum record count when specified
                if (maxCount > 0) orderedErrors = orderedErrors.Take(maxCount);
                // Materialize the selected records
                List<ErrorTracer.ErrorRecord> reportErrors = orderedErrors.ToList();
                // Report header
                sb.AppendLine("============================================================");
                sb.AppendLine("                    FULL ERROR REPORT");
                sb.AppendLine("============================================================");
                sb.AppendLine($"Generated At : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
                sb.AppendLine($"Total Errors : {reportErrors.Count}");
                sb.AppendLine($"Order        : {(descending ? "Newest to Oldest" : "Oldest to Newest")}");
                sb.AppendLine($"Requested    : {(maxCount == 0 ? "All" : maxCount.ToString())}");
                sb.AppendLine("============================================================");
                sb.AppendLine();
                // Handle empty error history
                if (reportErrors.Count == 0) { sb.AppendLine("No errors have been recorded."); return sb.ToString(); }
                // Write each error record
                for (int i = 0; i < reportErrors.Count; i++)
                {
                    ErrorTracer.ErrorRecord error = reportErrors[i];
                    sb.AppendLine($"[{i + 1}] ERROR RECORD");
                    sb.AppendLine("------------------------------------------------------------");
                    sb.AppendLine($"Timestamp      : {error.Timestamp:yyyy-MM-dd HH:mm:ss.fff} UTC");
                    sb.AppendLine($"Error Code     : {error.ErrorCode}");
                    sb.AppendLine($"Function Name  : {error.FunctionName}");
                    sb.AppendLine($"Function Type  : {error.FunctionType}");
                    sb.AppendLine($"Func ID        : {error.FuncId}");
                    sb.AppendLine($"Parent Func ID : {error.ParentFuncId ?? "N/A"}");
                    sb.AppendLine($"Error Message  : {error.ErrorMessage}");
                    // Include exception details when available
                    if (error.Exception != null) { sb.AppendLine("Exception      :"); sb.AppendLine(error.Exception.ToString()); }
                    sb.AppendLine("============================================================");
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            /// <summary>
            /// Exports the recorded error history as CSV.
            /// </summary>
            /// <returns>A CSV-formatted string containing the error records.</returns>
            public string ExportAsCSV(List<ErrorTracer.ErrorRecord> snapshot)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Timestamp,ErrorCode,FuncId,ParentFuncId,FunctionName,FunctionType,Message");
                foreach (ErrorTracer.ErrorRecord error in snapshot.OrderBy(e => e.Timestamp))
                    sb.AppendLine($"\"{error.Timestamp:yyyy-MM-dd HH:mm:ss.fff}\",\"{EscapeCsv(error.ErrorCode.ToString())}\",\"{EscapeCsv(error.FuncId)}\",\"{EscapeCsv(error.ParentFuncId)}\"," +
                                  $"\"{EscapeCsv(error.FunctionName)}\",\"{EscapeCsv(error.FunctionType.ToString())}\",\"{EscapeCsv(error.ErrorMessage)}\"");
                return sb.ToString();
            }
            /// <summary>
            /// Exports a specified number of error records from the snapshot
            /// to CSV format, ordered by timestamp.
            /// </summary>
            /// <param name="snapshot">
            /// A snapshot of the error history retrieved from ErrorTracer.
            /// </param>
            /// <param name="descending">
            /// Determines the order of error records.
            /// True: Newest to oldest.
            /// False: Oldest to newest.
            /// </param>
            /// <param name="maxCount">
            /// Maximum number of error records to export.
            /// Zero means all available error records.
            /// </param>
            /// <returns>
            /// A CSV-formatted string containing the selected error records.
            /// </returns>
            public string ExportAsCSV(List<ErrorTracer.ErrorRecord> snapshot, bool descending, int maxCount = 0)
            {
                if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
                if (maxCount < 0) throw new ArgumentOutOfRangeException(nameof(maxCount));
                StringBuilder sb = new StringBuilder();
                // Determine the ordering of error records
                IEnumerable<ErrorTracer.ErrorRecord> orderedErrors = descending ? snapshot.OrderByDescending(error => error.Timestamp) : snapshot.OrderBy(error => error.Timestamp);
                // Apply the maximum record count when specified
                if (maxCount > 0) orderedErrors = orderedErrors.Take(maxCount);
                // CSV header
                sb.AppendLine("Timestamp,ErrorCode,FuncId,ParentFuncId,FunctionName,FunctionType,Message,Exception");
                // Write error records
                foreach (ErrorTracer.ErrorRecord error in orderedErrors)
                    sb.AppendLine($"\"{EscapeCsv(error.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"))}\",\"{EscapeCsv(error.ErrorCode.ToString())}\",\"{EscapeCsv(error.FuncId)}\"," +
                                  $"\"{EscapeCsv(error.ParentFuncId)}\",\"{EscapeCsv(error.FunctionName)}\",\"{EscapeCsv(error.FunctionType.ToString())}\"," +
                                  $"\"{EscapeCsv(error.ErrorMessage)}\",\"{EscapeCsv(error.Exception == null ? string.Empty : error.Exception.ToString())}\"");
                return sb.ToString();
            }
            /// <summary>
            /// Escapes a value for CSV formatting.
            /// </summary>
            private static string EscapeCsv(string value) => (value ?? string.Empty).Replace("\"", "\"\"");

            // ================== Helper Functions ==================
            //Stack Helper Methods
            /// <summary>
            /// Determines whether the function context stack is empty.
            /// </summary>
            /// <returns>
            /// True if the stack contains no active function contexts; otherwise, false.
            /// </returns>
            private bool FuncContextIsEmpty() => this._funcContextStack.Count == 0;
            /// <summary>
            /// Gets the currently active function context without removing it from the stack.
            /// </summary>
            /// <returns>
            /// The top function context, or null if the stack is empty.
            /// </returns>
            private FuncLogContext FuncContextPeek() => this.FuncContextIsEmpty() ? null : this._funcContextStack.Peek();
            /// <summary>
            /// Pushes a function context onto the stack.
            /// </summary>
            /// <param name="context">The function context to add.</param>
            private void FuncContextPush(FuncLogContext context)
            {
                if (context == null) throw new ArgumentNullException(nameof(context));
                this._funcContextStack.Push(context);
            }
            /// <summary>
            /// Removes and returns the top function context from the stack.
            /// Returns null if the stack is empty.
            /// </summary>
            private FuncLogContext FuncContextPop() => this.FuncContextIsEmpty() ? null : this._funcContextStack.Pop();
            //End : Stack Helper Methods
        }

        /// <summary>
        /// Manages the in-memory history of error records.
        /// </summary>
        private class ErrorTracer
        {
            /// <summary>
            /// Stores recent error records in memory.
            /// </summary>
            private readonly Queue<ErrorRecord> _errorHistory = new Queue<ErrorRecord>();
            /// <summary>
            /// Synchronizes access to the error history.
            /// </summary>
            private readonly object _historyLock = new object();
            /// <summary>
            /// Defines the maximum number of error records kept in memory.
            /// </summary>
            private const int MAX_HISTORY = 1000;
            /// <summary>
            /// Represents an error record stored in the error history.
            /// </summary>
            public class ErrorRecord
            {
                /// <summary>
                /// Gets or sets the timestamp when the error occurred.
                /// </summary>
                public DateTime Timestamp { get; set; }
                /// <summary>
                /// Gets or sets the error code.
                /// </summary>
                public Device.ERROR_CODE ErrorCode { get; set; }
                /// <summary>
                /// Gets or sets the error message.
                /// </summary>
                public string ErrorMessage { get; set; }
                /// <summary>
                /// Gets or sets the identifier of the function where the error occurred.
                /// </summary>
                public string FuncId { get; set; }
                /// <summary>
                /// Gets or sets the identifier of the parent function.
                /// </summary>
                public string ParentFuncId { get; set; }
                /// <summary>
                /// Gets or sets the name of the function where the error occurred.
                /// </summary>
                public string FunctionName { get; set; }
                /// <summary>
                /// Gets or sets the type of the function where the error occurred.
                /// </summary>
                public Device.FUNC_TYPE FunctionType { get; set; }
                /// <summary>
                /// Gets or sets the exception associated with the error.
                /// </summary>
                public Exception Exception { get; set; }
            }
            /// <summary>
            /// Returns a snapshot of the current error history.
            /// </summary>
            public List<ErrorRecord> GetHistory() { lock (this._historyLock) { return this._errorHistory.ToList(); } }
            /// <summary>
            /// Records an error in the in-memory history.
            /// Removes the oldest record when the maximum capacity is reached.
            /// </summary>
            public void RecordError(Device.ERROR_CODE errorCode, string errorMessage, string funcId, string parentFuncId, string functionName, Device.FUNC_TYPE functionType, Exception exception = null)
            {
                ErrorRecord record = new ErrorRecord
                {
                    Timestamp = DateTime.UtcNow,
                    ErrorCode = errorCode,
                    ErrorMessage = errorMessage,
                    Exception = exception,
                    FuncId = funcId,
                    ParentFuncId = parentFuncId,
                    FunctionName = functionName,
                    FunctionType = functionType
                };
                lock (this._historyLock)
                {
                    while (this._errorHistory.Count >= MAX_HISTORY) this._errorHistory.Dequeue();
                    this._errorHistory.Enqueue(record);
                }
            }
            /// <summary>
            /// Returns error patterns grouped by error code.
            /// Results are ordered by occurrence count in descending order.
            /// </summary>
            public List<(Device.ERROR_CODE Code, int Count, DateTime FirstOccurrence, DateTime LastOccurrence, string LastMessage)> GetErrorPatterns()
            {
                List<ErrorRecord> snapshot;
                lock (this._historyLock) snapshot = this._errorHistory.ToList();
                return snapshot.GroupBy(e => e.ErrorCode).Select(g => (Code: g.Key, Count: g.Count(), FirstOccurrence: g.Min(x => x.Timestamp), LastOccurrence: g.Max(x => x.Timestamp),
                                                                 LastMessage: g.OrderByDescending(x => x.Timestamp).First().ErrorMessage)).OrderByDescending(x => x.Count).ToList();
            }
            /// <summary>
            /// Returns the most recent error records.
            /// </summary>
            /// <param name="count">Maximum number of records to return.</param>
            public List<ErrorRecord> GetRecentErrors(int count = 20)
            {
                List<ErrorRecord> snapshot;
                lock (this._historyLock) snapshot = this._errorHistory.ToList();
                return snapshot.OrderByDescending(e => e.Timestamp).Take(count).ToList();
            }
            /// <summary>
            /// Returns error records associated with a specific function name.
            /// </summary>
            public List<ErrorRecord> GetErrorsByFunctionName(string functionName)
            {
                List<ErrorRecord> snapshot;
                lock (this._historyLock) snapshot = this._errorHistory.ToList();

                return snapshot.Where(e => e.FunctionName == functionName).OrderByDescending(e => e.Timestamp).ToList();
            }
            /// <summary>
            /// Returns error records associated with a specific error code.
            /// </summary>
            public List<ErrorRecord> GetErrorsByCode(Device.ERROR_CODE errorCode)
            {
                List<ErrorRecord> snapshot;
                lock (this._historyLock) snapshot = this._errorHistory.ToList();
                return snapshot.Where(e => e.ErrorCode == errorCode).OrderByDescending(e => e.Timestamp).ToList();
            }
            /// <summary>
            /// Returns error records from the last 24 hours.
            /// </summary>
            public List<ErrorRecord> GetErrorsLast24Hours()
            {
                DateTime twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);
                List<ErrorRecord> snapshot;
                lock (this._historyLock) snapshot = this._errorHistory.ToList();
                return snapshot.Where(e => e.Timestamp >= twentyFourHoursAgo).OrderByDescending(e => e.Timestamp).ToList();
            }
            /// <summary>
            /// Returns the number of error records currently stored.
            /// </summary>
            public int Count { get { lock (this._historyLock) return this._errorHistory.Count; } }
            /// <summary>
            /// Clears all error records from the history.
            /// </summary>
            public void Clear() { lock (this._historyLock) this._errorHistory.Clear(); }
        }
    }
}
