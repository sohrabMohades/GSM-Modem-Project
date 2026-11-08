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
        private class DeviceLoggerOld : IDisposable
        {
            private readonly string _logId;
            /// <summary>
            /// Provides logging methods for the current device.
            /// </summary>
            //private readonly DeviceLogger _logger;
            private readonly ILogger _logger;
            /// <summary>
            /// Indicates whether this logger has been disposed.
            /// </summary>
            private bool _disposed = false;
            /// <summary>
            /// Provides error tracking for the current device.
            /// </summary>
            private readonly ErrorTracer _errorTracer;

            /*
            /// <summary>
            /// Stores information about a device operation for logging and tracking.
            /// </summary>
            internal class LogContext
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
                private bool _isCompleted;
                private readonly object _completionLock = new object();
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
                /// <summary>
                /// Attempts to mark the operation as completed.
                /// Returns false if the operation has already been completed.
                /// This prevents duplicate completion or failure logging.
                /// </summary>
                internal bool TryComplete()
                {
                    lock (this._completionLock)
                    {
                        if (this._isCompleted) return false;
                        this._isCompleted = true;
                        return true;
                    }
                }
            }
            */


            /// <summary>
            /// Represents the logging context of the main Device operation.
            /// Manages operation information and the nested function execution contexts.
            /// </summary>
            public sealed class LogContext
            {
                /// <summary>
                /// Gets the unique identifier of the main operation.
                /// </summary>
                public string CorrelationId { get; private set; }
                /// <summary>
                /// Gets the name of the main operation.
                /// </summary>
                public string OperationName { get; private set; }
                /// <summary>
                /// Gets the UTC time when the operation started.
                /// </summary>
                public DateTime StartTime { get; private set; }
                /// <summary>
                /// Gets the UTC time when the operation ended.
                /// </summary>
                public DateTime? EndTime { get; private set; }
                /// <summary>
                /// Gets the parameters associated with the main operation.
                /// </summary>
                public object Parameters { get; private set; }
                /// <summary>
                /// Stores the active function contexts in nested execution order.
                /// </summary>
                private readonly Stack<FuncLogContext> _funcContexts;
                /// <summary>
                /// Initializes a new logging context for the specified operation.
                /// </summary>
                /// <param name="operationName">The name of the main operation.</param>
                /// <param name="parameters">The parameters associated with the operation.</param>
                public LogContext(string operationName, object parameters = null)
                {
                    this.CorrelationId = Guid.NewGuid().ToString("N");
                    this.OperationName = operationName;
                    this.StartTime = DateTime.UtcNow;
                    this.Parameters = parameters;
                    this._funcContexts = new Stack<FuncLogContext>();
                }
                /// <summary>
                /// Marks the operation as completed by recording its end time.
                /// </summary>
                public void Complete() => this.EndTime = DateTime.UtcNow;
                /// <summary>
                /// Gets the operation duration.
                /// If the operation has ended, calculates the duration using its end time;
                /// otherwise, calculates the duration up to the current time.
                /// </summary>
                /// <returns>The duration of the operation.</returns>
                public TimeSpan GetDuration() { DateTime endTime = this.EndTime ?? DateTime.UtcNow; return endTime - this.StartTime; }
                /// <summary>
                /// Creates and activates a new function logging context.
                /// The current active function, if any, becomes the parent function.
                /// </summary>
                /// <param name="functionName">The name of the function.</param>
                /// <param name="functionType">The type of the function.</param>
                /// <param name="parameters">The parameters associated with the function.</param>
                /// <returns>The newly created function logging context.</returns>
                public FuncLogContext StartFunc(string functionName, string functionType, object parameters = null)
                {
                    string parentFuncId = null;
                    if (this._funcContexts.Count > 0) parentFuncId = this._funcContexts.Peek().FuncId;
                    FuncLogContext context = new FuncLogContext(this.CorrelationId, functionName, functionType, parentFuncId, parameters);
                    this._funcContexts.Push(context);
                    return context;
                }
                /// <summary>
                /// Gets the currently active function logging context.
                /// </summary>
                /// <returns>The active function context, or null if no function is active.</returns>
                public FuncLogContext GetActiveFuncContext() { if (this._funcContexts.Count == 0) return null; return this._funcContexts.Peek(); }
                /// <summary>
                /// Completes the active function if its identifier matches the specified FuncId.
                /// Only the function at the top of the stack can be completed.
                /// </summary>
                /// <param name="funcId">The identifier of the function to complete.</param>
                /// <param name="funcResult">The final result of the function.</param>
                /// <param name="completedContext">Receives the completed function context.</param>
                /// <returns>True if the function was successfully completed; otherwise, false.</returns>
                public bool TryCompleteFunc(string funcId, FUNC_RESULT funcResult, out FuncLogContext completedContext)
                {
                    completedContext = null;
                    if (this._funcContexts.Count == 0) return false;
                    FuncLogContext activeContext = this._funcContexts.Peek();
                    if (activeContext.FuncId != funcId) return false;
                    if (!activeContext.TryComplete(funcResult)) return false;
                    completedContext = this._funcContexts.Pop();
                    return true;
                }
                /// <summary>
                /// Gets whether one or more function contexts are still active.
                /// </summary>
                public bool HasActiveFuncs { get => this._funcContexts.Count > 0; }
                /// <summary>
                /// Returns a string containing the operation identifier and name.
                /// </summary>
                /// <returns>The formatted operation context.</returns>
                public override string ToString() => $"[{this.CorrelationId}] {this.OperationName}";
            }

            /// <summary>
            /// Represents the logging context of a single function execution
            /// within a main Device operation.
            /// </summary>
            public sealed class FuncLogContext
            {
                /// <summary>
                /// Gets the identifier of the parent operation.
                /// </summary>
                public string CorrelationId { get; private set; }
                /// <summary>
                /// Gets the unique identifier of this function execution.
                /// </summary>
                public string FuncId { get; private set; }
                /// <summary>
                /// Gets the identifier of the parent function execution.
                /// Null if this function is directly under the main operation.
                /// </summary>
                public string ParentFuncId { get; private set; }
                /// <summary>
                /// Gets the name of the function.
                /// </summary>
                public string FunctionName { get; private set; }
                /// <summary>
                /// Gets the type of the function.
                /// Examples include Workflow, ComplexWorkflow, and Execute.
                /// </summary>
                public string FunctionType { get; private set; }
                /// <summary>
                /// Gets the result of the function execution.
                /// The initial value is UNKNOWN until a result is assigned.
                /// </summary>
                public FUNC_RESULT FuncResult { get; private set; }
                /// <summary>
                /// Gets the UTC time when the function started.
                /// </summary>
                public DateTime StartTime { get; private set; }
                /// <summary>
                /// Gets the UTC time when the function ended.
                /// </summary>
                public DateTime? EndTime { get; private set; }
                /// <summary>
                /// Gets the parameters associated with the function.
                /// </summary>
                public object Parameters { get; private set; }
                /// <summary>
                /// Gets whether the function execution has been completed.
                /// </summary>
                public bool IsCompleted { get; private set; }
                /// <summary>
                /// Initializes a new function logging context.
                /// </summary>
                /// <param name="correlationId">The identifier of the parent operation.</param>
                /// <param name="functionName">The name of the function.</param>
                /// <param name="functionType">The type of the function.</param>
                /// <param name="parentFuncId">The identifier of the parent function, if any.</param>
                /// <param name="parameters">The parameters associated with the function.</param>
                public FuncLogContext(string correlationId, string functionName, string functionType, string parentFuncId, object parameters = null)
                {
                    this.CorrelationId = correlationId;
                    this.FuncId = Guid.NewGuid().ToString("N");
                    this.ParentFuncId = parentFuncId;
                    this.FunctionName = functionName;
                    this.FunctionType = functionType;
                    this.FuncResult = FUNC_RESULT.UNKNOWN;
                    this.StartTime = DateTime.UtcNow;
                    this.Parameters = parameters;
                    this.IsCompleted = false;
                }
                /// <summary>
                /// Completes the function execution by assigning its result
                /// and recording its end time.
                /// </summary>
                /// <param name="funcResult">The final result of the function execution.</param>
                /// <returns>True if the function was completed; otherwise, false.</returns>
                public bool TryComplete(FUNC_RESULT funcResult)
                {
                    if (this.IsCompleted) return false;
                    this.FuncResult = funcResult;
                    this.EndTime = DateTime.UtcNow;
                    this.IsCompleted = true;
                    return true;
                }
                /// <summary>
                /// Gets the function execution duration.
                /// If the function has ended, calculates the duration using its end time;
                /// otherwise, calculates the duration up to the current time.
                /// </summary>
                /// <returns>The duration of the function execution.</returns>
                public TimeSpan GetDuration() { DateTime endTime = this.EndTime ?? DateTime.UtcNow; return endTime - this.StartTime; }
            }


            /// <summary>
            /// Gets the error tracer associated with this device logger.
            /// </summary>
            public ErrorTracer Errors { get => this._errorTracer; }
            
            /// <summary>
            /// Initializes a new DeviceLogger with a unique LogId.
            /// Creates the Serilog logger and its associated ErrorTracer.
            /// </summary>
            internal DeviceLoggerOld()
            {
                this._logId = Guid.NewGuid().ToString("N");
                // Sanitize DeviceId for file name
                string safeLogId = new string(this._logId.Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_').ToArray());
                if (string.IsNullOrWhiteSpace(safeLogId)) safeLogId = "DEFAULT";
                // Log directory
                string logsDirectory = InitializeLogging();
                // Log file paths
                string textLogPath = Path.Combine(logsDirectory, $"device-{safeLogId}-.txt");
                string jsonLogPath = Path.Combine(logsDirectory, $"device-{safeLogId}-structured-.json");
                // Serilog Configuration
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
                //this._errorTracer = new ErrorTracer(this);
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
                IDisposable disposableLogger = this._logger as IDisposable;
                if (disposableLogger != null) disposableLogger.Dispose();
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
                var context = new LogContext(operationName, parameters);
                this._logger.Information("OPERATION_START | Operation={Operation} | CorrelationId={CorrelationId} | Params={@Parameters}", operationName, correlationId, parameters ?? new { });
                return context;
            }
            /// <summary>
            /// Ends the specified operation by logging its final status.
            /// Calls OperationComplete or OperationFailed based on the operation status.
            /// </summary>
            /// <param name="context">The context of the operation being completed.</param>
            /// <param name="status">The final status of the operation.</param>
            /// <param name="exception">The exception associated with the operation failure, if any.</param>
            private void OperationEnd(LogContext context, FUNC_RESULT FuncResult, Exception exception = null)
            {
                if (FuncResult != FUNC_RESULT.SUCCESS || exception != null) this.OperationFailed(context, exception);
                else this.OperationComplete(context);
            }
            /// <summary>
            /// Logs the successful or normal completion of an operation.
            /// </summary>
            /// <param name="context">The context of the operation.</param>
            /// <param name="operationResult">The final result of the operation.</param>
            /// <param name="result">Optional operation result data.</param>
            public void OperationComplete(LogContext context, string operationResult = "SUCCESS", object result = null)
            {
                // Prevent null context and duplicate operation completion.
                if (context == null) return;// || !context.TryComplete()) return;
                if (string.IsNullOrWhiteSpace(operationResult)) operationResult = "UNKNOWN";
                var duration = context.GetDuration();
                this._logger.Information("OPERATION_COMPLETE | Operation={Operation} | CorrelationId={CorrelationId} | operationResult={operationResult} | DurationMs={DurationMs} | " +
                                         " Result={@Result}", context.OperationName, context.CorrelationId, operationResult, duration.TotalMilliseconds, result ?? new { });
            }
            /// <summary>
            /// Logs an operation failure with its result and error details.
            /// </summary>
            /// <param name="context">The context of the failed operation.</param>
            /// <param name="ex">The exception associated with the failure.</param>
            /// <param name="operationResult">The failure operation result.</param>
            /// <param name="errorCode">The error code, if available.</param>
            /// <param name="errorMessage">The error description.</param>
            public void OperationFailed(LogContext context, Exception ex = null, string operationResult = "FAIL", string errorCode = null, string errorMessage = null, object result = null)
            {
                // Prevent null context and duplicate operation completion.
                if (context == null) return;// || !context.TryComplete()) return;
                if (string.IsNullOrWhiteSpace(operationResult)) operationResult = "FAIL";
                if (string.IsNullOrWhiteSpace(errorMessage)) errorMessage = ex != null ? ex.Message : operationResult;
                var duration = context.GetDuration();
                this._logger.Error(ex, "OPERATION_FAILED | Operation={Operation} | CorrelationId={CorrelationId} | operationResult={operationResult} | ErrorCode={ErrorCode} | " +
                                   "DurationMs={DurationMs} | ErrorMessage={ErrorMessage} | Result={@Result}", context.OperationName, context.CorrelationId, operationResult, errorCode,
                                   duration.TotalMilliseconds, errorMessage, result ?? new { });
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
                if (string.IsNullOrWhiteSpace(errorCode)) errorCode = "UNKNOWN";
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
            /// Executes an operation and logs its start, completion, or failure.
            /// </summary>
            /// <typeparam name="T">The type of the operation result.</typeparam>
            /// <param name="operation">The operation to execute.</param>
            /// <param name="operationName">The name of the operation.</param>
            /// <returns>The result returned by the operation.</returns>
            public T LogOperation<T>(Func<T> operation, string operationName)
            {
                if (operation == null) throw new ArgumentNullException(nameof(operation));
                var context = this.OperationStart(operationName);
                try
                {
                    var result = operation.Invoke();
                    this.OperationComplete(context, "SUCCESS", new { Result = result });
                    return result;
                }
                catch (Exception ex)
                {
                    this.OperationFailed(context, ex);
                    throw;
                }
            }
            /// <summary>
            /// Executes an operation without a return value and logs its
            /// start, completion, or failure.
            /// </summary>
            /// <param name="operation">The operation to execute.</param>
            /// <param name="operationName">The name of the operation.</param>
            public void LogOperation(Action operation, string operationName)
            {
                if (operation == null) throw new ArgumentNullException(nameof(operation));
                var context = this.OperationStart(operationName);
                try
                {
                    operation.Invoke();
                    this.OperationComplete(context, "SUCCESS");
                }
                catch (Exception ex)
                {
                    this.OperationFailed(context, ex);
                    throw;
                }
            }
        }
    }
}
