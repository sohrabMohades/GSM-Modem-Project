using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
// Adjust this namespace to match the actual Device class namespace.
using GsmDevice = GSM.Core.Source.Device.Device;

namespace GSM.Core.Tests.IntegrationTests
{
#if DEVICELOGGER_TEST

    [TestClass]
    public class DeviceLoggerBasicTests
    {
        private GsmDevice.DeviceLogger _logger;
        private GsmDevice.ErrorTracer _errorTracer;

        [TestInitialize]
        public void TestInitialize() { this._logger = new GsmDevice.DeviceLogger(); this._errorTracer = new GsmDevice.ErrorTracer(); }

        [TestCleanup]
        public void TestCleanup()
        {
            if (this._logger != null) { this._logger.Dispose(); this._logger = null; }
            this._errorTracer = null;
        }

        // =========================================================
        // FUNC LIFECYCLE
        // =========================================================
        [TestMethod]
        public void FuncStart_ShouldPushContext()
        {
            this._logger.FuncStart("TestFunction", GsmDevice.FUNC_TYPE.WORKFLOW, new { Input = "TestInput" });
            Assert.AreEqual(1, this.GetFuncStackCount());
        }

        [TestMethod]
        public void FuncEnd_ShouldPopContext()
        {
            this._logger.FuncStart("TestFunction", GsmDevice.FUNC_TYPE.WORKFLOW);
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.SUCCESS);
            Assert.AreEqual(0, this.GetFuncStackCount());
        }

        [TestMethod]
        public void NestedFunctions_ShouldMaintainStack()
        {
            // Start Root Function
            this._logger.FuncStart("RootFunction", GsmDevice.FUNC_TYPE.WORKFLOW);
            Assert.AreEqual(1, this.GetFuncStackCount());
            // Record an error inside Root Function
            this._logger.LogError(this._errorTracer, this.GetValidErrorCode(), "Root error");
            // Start Child Function
            this._logger.FuncStart("ChildFunction", GsmDevice.FUNC_TYPE.COMPLEX_WORKFLOW);
            Assert.AreEqual(2, this.GetFuncStackCount());
            // Record an error inside Child Function
            this._logger.LogError(this._errorTracer, this.GetValidErrorCode(), "Child error");
            // Start Nested Function
            this._logger.FuncStart("NestedFunction", GsmDevice.FUNC_TYPE.WORKFLOW_SUB);
            Assert.AreEqual(3, this.GetFuncStackCount());
            // End Nested Function
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.SUCCESS);
            Assert.AreEqual(2, this.GetFuncStackCount());
            // End Child Function
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.SUCCESS);
            Assert.AreEqual(1, this.GetFuncStackCount());
            // End Root Function
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.SUCCESS);
            Assert.AreEqual(0, this.GetFuncStackCount());
            // Verify ErrorTracer records
            List<GsmDevice.ErrorTracer.ErrorRecord> history = this._errorTracer.GetHistory();
            Assert.AreEqual(2, history.Count);
            // Root Function
            Assert.AreEqual("RootFunction", history[0].FunctionName);
            Assert.IsNull(history[0].ParentFuncId);
            // Child Function
            Assert.AreEqual("ChildFunction", history[1].FunctionName);
            Assert.AreEqual(history[0].FuncId, history[1].ParentFuncId);
            // Root and Child must have different FuncIds
            Assert.AreNotEqual(history[0].FuncId, history[1].FuncId);
        }
        /*
        public void NestedFunctions_ShouldMaintainStack()
        {
            this._logger.FuncStart("RootFunction", GsmDevice.FUNC_TYPE.WORKFLOW);
            Assert.AreEqual(1, this.GetFuncStackCount());

            this._logger.FuncStart("ChildFunction", GsmDevice.FUNC_TYPE.COMPLEX_WORKFLOW);
            Assert.AreEqual(2, this.GetFuncStackCount());

            this._logger.FuncStart("NestedFunction", GsmDevice.FUNC_TYPE.EXECUTE);
            Assert.AreEqual(3, this.GetFuncStackCount());

            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.SUCCESS);
            Assert.AreEqual(2, this.GetFuncStackCount());

            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.SUCCESS);
            Assert.AreEqual(1, this.GetFuncStackCount());

            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.SUCCESS);
            Assert.AreEqual(0, this.GetFuncStackCount());
        }
        */

        [TestMethod]
        public void FuncEnd_WithOutputParameters_ShouldPopContext()
        {
            object outputParameters = new { Result = "OK", Count = 10 };
            this._logger.FuncStart("TestFunction", GsmDevice.FUNC_TYPE.WORKFLOW, new { Input = "InputValue" });
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.SUCCESS, outputParameters);
            Assert.AreEqual(0, this.GetFuncStackCount());
        }

        // =========================================================
        // ERROR LOGGING
        // =========================================================
        [TestMethod]
        public void LogError_WithoutErrorTracer_ShouldNotThrow()
        {
            this._logger.FuncStart("ErrorTest", GsmDevice.FUNC_TYPE.WORKFLOW);
            this._logger.LogError(this.GetValidErrorCode(), "Test error message");
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.MAJOR_ERROR);
        }

        [TestMethod]
        public void LogError_WithErrorTracer_ShouldRecordError()
        {
            string functionName = "ErrorTest";
            GsmDevice.ERROR_CODE errorCode = this.GetValidErrorCode();
            string errorMessage = "Test error message";
            this._logger.FuncStart(functionName, GsmDevice.FUNC_TYPE.WORKFLOW);
            this._logger.LogError(this._errorTracer, errorCode, errorMessage);
            List<GsmDevice.ErrorTracer.ErrorRecord> history = this._errorTracer.GetHistory();
            Assert.AreEqual(1, history.Count);
            GsmDevice.ErrorTracer.ErrorRecord record = history[0];
            Assert.AreEqual(errorCode, record.ErrorCode);
            Assert.AreEqual(errorMessage, record.ErrorMessage);
            Assert.AreEqual(functionName, record.FunctionName);
            Assert.AreEqual(GsmDevice.FUNC_TYPE.WORKFLOW, record.FunctionType);
            Assert.IsFalse(string.IsNullOrWhiteSpace(record.FuncId));
            Assert.IsNull(record.ParentFuncId);
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.MAJOR_ERROR);
        }
        /*
        public void LogError_WithErrorTracer_ShouldRecordError()
        {
            this._logger.FuncStart("ErrorTest", GsmDevice.FUNC_TYPE.WORKFLOW);
            this._logger.LogError(this._errorTracer, this.GetValidErrorCode(), "Test error message");
            Assert.AreEqual(1, this._errorTracer.Count);
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.MAJOR_ERROR);
            //Assert.AreEqual(2, this.GetFuncStackCount());
        }
        */

        [TestMethod]
        public void LogError_WithNoErrorCode_ShouldNotRecordError()
        {
            this._logger.FuncStart("ErrorTest", GsmDevice.FUNC_TYPE.WORKFLOW);
            this._logger.LogError(this._errorTracer, GsmDevice.ERROR_CODE.NO_ERROR, "This should not be recorded");
            Assert.AreEqual(0, this._errorTracer.Count);
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.SUCCESS);
            //Assert.AreEqual(2, this.GetFuncStackCount());
        }

        [TestMethod]
        public void LogError_WithException_ShouldRecordException()
        {
            Exception exception = new InvalidOperationException("Test exception");
            this._logger.FuncStart("ExceptionTest", GsmDevice.FUNC_TYPE.WORKFLOW);
            this._logger.LogError(this._errorTracer, this.GetValidErrorCode(), "Exception test", exception);
            var history = this._errorTracer.GetHistory();
            Assert.AreEqual(1, history.Count);
            Assert.AreSame(exception, history[0].Exception);
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.MAJOR_ERROR);
        }

        // =========================================================
        // WARNING LOGGING
        // =========================================================
        [TestMethod]
        public void LogWarning_WithErrorCode_ShouldNotRecordInErrorTracer()
        {
            this._logger.FuncStart("WarningTest", GsmDevice.FUNC_TYPE.WORKFLOW);
            this._logger.LogWarning(this.GetValidErrorCode(), "Test warning");
            Assert.AreEqual(0, this._errorTracer.Count);
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.SUCCESS);
        }

        [TestMethod]
        public void LogWarning_WithNoErrorCode_ShouldNotThrow()
        {
            this._logger.FuncStart("WarningTest", GsmDevice.FUNC_TYPE.WORKFLOW);
            this._logger.LogWarning(GsmDevice.ERROR_CODE.NO_ERROR, "This warning should not be logged");
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.SUCCESS);
        }

        [TestMethod]
        public void LogWarning_WithMessage_ShouldNotThrow()
        {
            this._logger.FuncStart("WarningTest", GsmDevice.FUNC_TYPE.WORKFLOW);
            this._logger.LogWarning("General warning message");
            this._logger.FuncEnd(GsmDevice.FUNC_RESULT.SUCCESS);
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private int GetFuncStackCount()
        {
            FieldInfo field = typeof(GsmDevice.DeviceLogger).GetField("_funcContextStack", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "_funcContextStack field was not found.");
            object stack = field.GetValue(this._logger);
            Assert.IsNotNull(stack);
            return ((ICollection)stack).Count;
        }

        private GsmDevice.ERROR_CODE GetValidErrorCode()
        {
            foreach (GsmDevice.ERROR_CODE code in Enum.GetValues(typeof(GsmDevice.ERROR_CODE)))
                if (code != GsmDevice.ERROR_CODE.NO_ERROR) return code;
            throw new InvalidOperationException("No valid ERROR_CODE value was found.");
        }
    }

    [TestClass]
    public class ErrorTracerTests
    {
        private GsmDevice.ErrorTracer _errorTracer;
        [TestInitialize]
        public void TestInitialize() => this._errorTracer = new GsmDevice.ErrorTracer();

        [TestMethod]
        public void RecordError_ShouldIncreaseCount()
        {
            this.RecordTestError(1);
            Assert.AreEqual(1, this._errorTracer.Count);
        }

        [TestMethod]
        public void GetHistory_ShouldReturnSnapshot()
        {
            this.RecordTestError(1);
            List<GsmDevice.ErrorTracer.ErrorRecord> snapshot = this._errorTracer.GetHistory();
            Assert.AreEqual(1, snapshot.Count);
            this.RecordTestError(2);
            // Existing snapshot must not change.
            Assert.AreEqual(1, snapshot.Count);
            // New history must contain both records.
            Assert.AreEqual(2, this._errorTracer.GetHistory().Count);
        }

        [TestMethod]
        public void GetRecentErrors_ShouldReturnRequestedCount()
        {
            for (int i = 0; i < 10; i++) this.RecordTestError(i);
            Assert.AreEqual(5, this._errorTracer.GetRecentErrors(5).Count);
        }

        [TestMethod]
        public void GetErrorsByCode_ShouldReturnMatchingRecords()
        {
            //correction, 2 deference error record
            GsmDevice.ERROR_CODE errorCode1 = this.GetValidErrorCode(1);
            this.RecordTestError(1, errorCode1);
            GsmDevice.ERROR_CODE errorCode2 = this.GetValidErrorCode(2);
            this.RecordTestError(2, errorCode2);
            Assert.AreNotEqual(errorCode1, errorCode2);
            List<GsmDevice.ErrorTracer.ErrorRecord> result1 = this._errorTracer.GetErrorsByCode(errorCode1);
            List<GsmDevice.ErrorTracer.ErrorRecord> result2 = this._errorTracer.GetErrorsByCode(errorCode2);
            Assert.AreEqual(1, result1.Count);
            Assert.AreEqual(1, result2.Count);
        }

        [TestMethod]
        public void GetErrorsByFunctionName_ShouldReturnMatchingRecords()
        {
            GsmDevice.ERROR_CODE errorCode = this.GetValidErrorCode(1);
            this._errorTracer.RecordError(errorCode, "Test message", "FUNC-001", null, "SendSms", GsmDevice.FUNC_TYPE.WORKFLOW, null);
            errorCode = this.GetValidErrorCode(2);
            this._errorTracer.RecordError(errorCode, "Test message", "FUNC-002", null, "ReadSms", GsmDevice.FUNC_TYPE.WORKFLOW, null);
            Assert.AreEqual(1, this._errorTracer.GetErrorsByFunctionName("SendSms").Count);
        }

        [TestMethod]
        public void GetErrorsLast24Hours_ShouldReturnRecentRecords()
        {
            this.RecordTestError(1);
            Assert.AreEqual(1, this._errorTracer.GetErrorsLast24Hours().Count);
        }

        [TestMethod]
        public void RecordMoreThanMaxHistory_ShouldRemoveOldestRecords()
        {
            GsmDevice.ERROR_CODE errorCode = this.GetValidErrorCode(1);
            for (int i = 1; i <= 1005; i++)
                this._errorTracer.RecordError(errorCode, $"Test error message {i}", Guid.NewGuid().ToString("N"), null, "TestFunction", GsmDevice.FUNC_TYPE.WORKFLOW, null);
            List<GsmDevice.ErrorTracer.ErrorRecord> history = this._errorTracer.GetHistory();
            Assert.AreEqual(1000, history.Count);
            // The first five records must have been removed.
            Assert.AreEqual("Test error message 6", history[0].ErrorMessage);
            // The latest record must still exist.
            Assert.AreEqual("Test error message 1005", history[999].ErrorMessage);
        }
        /*
        public void RecordMoreThanMaxHistory_ShouldRemoveOldestRecords()
        {
            for (int i = 0; i < 1005; i++) this.RecordTestError(1);
            Assert.AreEqual(1000, this._errorTracer.Count);
            Assert.AreEqual(1000, this._errorTracer.GetHistory().Count);
        }
        */

        [TestMethod]
        public void Clear_ShouldRemoveAllRecords()
        {
            this.RecordTestError(1);
            this.RecordTestError(2);
            this._errorTracer.Clear();
            Assert.AreEqual(0, this._errorTracer.Count);
            Assert.AreEqual(0, this._errorTracer.GetHistory().Count);
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private void RecordTestError(int ErrorCountReject, GsmDevice.ERROR_CODE errorCode = GsmDevice.ERROR_CODE.NO_ERROR)
        {
            if (errorCode == GsmDevice.ERROR_CODE.NO_ERROR) errorCode = this.GetValidErrorCode(ErrorCountReject);
            this._errorTracer.RecordError(errorCode, $"Test error message {ErrorCountReject}", Guid.NewGuid().ToString("N"), null, "TestFunction", GsmDevice.FUNC_TYPE.WORKFLOW, null);
        }

        private GsmDevice.ERROR_CODE GetValidErrorCode(int ErrorCountReject)
        {
            foreach (GsmDevice.ERROR_CODE code in Enum.GetValues(typeof(GsmDevice.ERROR_CODE)))
                if (code != GsmDevice.ERROR_CODE.NO_ERROR) if (--ErrorCountReject <= 0) return code;
            throw new InvalidOperationException("No valid ERROR_CODE value was found.");
        }
    }

    [TestClass]
    public class ReportExportTests
    {
        private GsmDevice.DeviceLogger _logger;
        private GsmDevice.ErrorTracer _errorTracer;

        [TestInitialize]
        public void TestInitialize()
        {
            this._logger = new GsmDevice.DeviceLogger();
            this._errorTracer = new GsmDevice.ErrorTracer();
            this.RecordTestErrors(5);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (this._logger != null) { this._logger.Dispose(); this._logger = null; }
            this._errorTracer = null;
        }

        [TestMethod]
        public void GenerateErrorSummaryReport_ShouldReturnReport()
        {
            List<GsmDevice.ErrorTracer.ErrorRecord> snapshot = this._errorTracer.GetHistory();
            string report = this._logger.GenerateErrorSummaryReport(snapshot);
            Assert.IsFalse(string.IsNullOrWhiteSpace(report));
        }

        [TestMethod]
        public void GenerateFullErrorReport_ShouldReturnAllRecords()
        {
            List<GsmDevice.ErrorTracer.ErrorRecord> snapshot = this._errorTracer.GetHistory();
            string report = this._logger.GenerateFullErrorReport(snapshot);
            Assert.IsFalse(string.IsNullOrWhiteSpace(report));
        }

        [TestMethod]
        public void GenerateFullErrorReport_WithMaxCount_ShouldLimitRecords()
        {
            List<GsmDevice.ErrorTracer.ErrorRecord> snapshot = this._errorTracer.GetHistory();
            string report = this._logger.GenerateFullErrorReport(snapshot, true, 2);
            Assert.IsFalse(string.IsNullOrWhiteSpace(report));
            int includedRecords = 0;
            for (int i = 0; i < 5; i++)
                if (report.Contains($"Test error {i}")) includedRecords++;
            Assert.AreEqual(2, includedRecords);
        }
        /*
        public void GenerateFullErrorReport_WithMaxCount_ShouldLimitRecords()
        {
            List<GsmDevice.ErrorTracer.ErrorRecord> snapshot = this._errorTracer.GetHistory();
            string report = this._logger.GenerateFullErrorReport(snapshot, true, 2);
            Assert.IsFalse(string.IsNullOrWhiteSpace(report));
        }
        */

        [TestMethod]
        public void ExportAsCSV_ShouldReturnCsvContent()
        {
            List<GsmDevice.ErrorTracer.ErrorRecord> snapshot = this._errorTracer.GetHistory();
            string csv = this._logger.ExportAsCSV(snapshot);
            Assert.IsFalse(string.IsNullOrWhiteSpace(csv));
            Assert.IsTrue(csv.Contains("ErrorCode"));
        }

        [TestMethod]
        public void ExportAsCSV_WithMaxCount_ShouldReturnCsvContent()
        {
            List<GsmDevice.ErrorTracer.ErrorRecord> snapshot = this._errorTracer.GetHistory();
            string csv = this._logger.ExportAsCSV(snapshot, true, 2);
            Assert.IsFalse(string.IsNullOrWhiteSpace(csv));
            Assert.IsTrue(csv.Contains("ErrorCode"));
            int includedRecords = 0;
            for (int i = 0; i < 5; i++)
                if (csv.Contains($"Test error {i}")) includedRecords++;
            Assert.AreEqual(2, includedRecords);
        }
        /*
        public void ExportAsCSV_WithMaxCount_ShouldReturnCsvContent()
        {
            List<GsmDevice.ErrorTracer.ErrorRecord> snapshot = this._errorTracer.GetHistory();
            string csv = this._logger.ExportAsCSV(snapshot, true, 2);
            Assert.IsFalse(string.IsNullOrWhiteSpace(csv));
        }
        */

        // =========================================================
        // HELPERS
        // =========================================================
        private void RecordTestErrors(int count)
        {
            GsmDevice.ERROR_CODE errorCode;
            for (int i = 0; i < count; i++)
            {
                errorCode = this.GetValidErrorCode(i + 1);
                this._errorTracer.RecordError(errorCode, $"Test error {i}", Guid.NewGuid().ToString("N"), null, "TestFunction", GsmDevice.FUNC_TYPE.WORKFLOW, null);
            }
        }
        private GsmDevice.ERROR_CODE GetValidErrorCode(int ErrorCountReject)
        {
            foreach (GsmDevice.ERROR_CODE code in Enum.GetValues(typeof(GsmDevice.ERROR_CODE)))
                if (code != GsmDevice.ERROR_CODE.NO_ERROR) if (--ErrorCountReject <= 0) return code;
            throw new InvalidOperationException("No valid ERROR_CODE value was found.");
        }
    }

    [TestClass]
    public class DisposeTests
    {
        [TestMethod]
        public void Dispose_WithEmptyStack_ShouldComplete()
        {
            GsmDevice.DeviceLogger logger = new GsmDevice.DeviceLogger();
            logger.Dispose();
        }

        [TestMethod]
        public void Dispose_WithActiveFunc_ShouldNotCompleteFuncAutomatically()
        {
            GsmDevice.DeviceLogger logger = new GsmDevice.DeviceLogger();
            logger.FuncStart("ActiveFunction", GsmDevice.FUNC_TYPE.WORKFLOW);
            logger.Dispose();
            // The active Func must not be automatically completed or popped.
            Assert.AreEqual(1, this.GetFuncStackCount(logger));
        }

        private int GetFuncStackCount(GsmDevice.DeviceLogger logger)
        {
            System.Reflection.FieldInfo field = typeof(GsmDevice.DeviceLogger).GetField("_funcContextStack", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            System.Collections.ICollection stack = (System.Collections.ICollection)field.GetValue(logger);
            return stack.Count;
        }
    }

    internal class TestLogEventSink : ILogEventSink
    {
        private readonly List<LogEvent> _events = new List<LogEvent>();
        public IReadOnlyList<LogEvent> Events {get => this._events.AsReadOnly(); }
        public void Emit(LogEvent logEvent) => this._events.Add(logEvent);
    }
#endif   
}
