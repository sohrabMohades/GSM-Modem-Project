// مثال نحوهٔ استفاده از Logging System در Device

using GSM.Core.Source.Device;
using GSM.Core.Source.Device.Logging;
using System;

namespace GSM.Core.Examples
{
    public class DeviceLoggingExample
    {
        public static void Main()
        {
            // مرحله 1: Initialize logging (یک بار در شروع برنامه)
            LoggingInitializer.Initialize();

            try
            {
                // مرحله 2: Create device (logger خودکار initialize می‌شود)
                var device = new Device("Device.dll");

                // ✅ حالا تمام عملیات logged می‌شوند!

                // مرحله 3: استفاده از Device
                Console.WriteLine("Getting device count...");
                int deviceCount = 0;
                bool result = device.DeviceCountGet(ref deviceCount);

                if (result)
                {
                    Console.WriteLine($"Device count: {deviceCount}");
                    Console.WriteLine("✅ Check logs/device-Device.dll-.txt for detailed logs");
                }

                // مرحله 4: Get error tracer for reporting
                var errorTracer = device.GetErrorTracer();
                var errorReport = errorTracer.GenerateErrorReport();
                Console.WriteLine("\n" + errorReport);

                // مرحله 5: Get logger for manual operations (اختیاری)
                var logger = device.GetLogger();
                var context = logger.OperationStart("CustomOperation", new { CustomData = "test" });

                System.Threading.Thread.Sleep(100);

                logger.OperationComplete(context, "SUCCESS", new { Duration = 100 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// مثال 2: همزمان استفاده از Exception Handling
        /// </summary>
        public static void ExampleWithErrorHandling()
        {
            LoggingInitializer.Initialize();

            var device = new Device("Device.dll");
            var errorTracer = device.GetErrorTracer();

            try
            {
                // انجام عملیات
                bool result = device.DeviceCountGet();

                if (!result)
                {
                    // Device error occured
                    var recentErrors = errorTracer.GetRecentErrors(5);
                    Console.WriteLine($"Latest errors: {recentErrors.Count}");

                    foreach (var error in recentErrors)
                    {
                        Console.WriteLine($"  - {error.ErrorCode}: {error.ErrorMessage}");
                    }
                }
            }
            finally
            {
                // تمام logs خودکار ذخیره می‌شوند
                device?.Dispose();
            }
        }

        /// <summary>
        /// مثال 3: استفاده از Extension Methods
        /// </summary>
        public static void ExampleWithExtensionMethods()
        {
            LoggingInitializer.Initialize();

            var device = new Device("Device.dll");

            // استفاده از LogOperation extension (بدون تغییر کد اصلی!)
            try
            {
                // مثال 1: operation با return value
                bool result = device.LogOperation(() =>
                {
                    int count = 0;
                    return device.DeviceCountGet(ref count);
                }, "GetDeviceCountOperation");

                // مثال 2: operation بدون return
                device.LogOperation(() =>
                {
                    // انجام کار
                    System.Threading.Thread.Sleep(100);
                }, "SomeOperation");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation failed: {ex.Message}");
                // All logs are automatically recorded
            }
        }

        /// <summary>
        /// مثال 4: صرف بررسی Error Patterns
        /// </summary>
        public static void ExampleErrorPatternAnalysis()
        {
            LoggingInitializer.Initialize();

            var device = new Device("Device.dll");
            var errorTracer = device.GetErrorTracer();

            // انجام چند عملیات مختلف
            for (int i = 0; i < 5; i++)
            {
                device.DeviceCountGet();
            }

            // بررسی patterns
            var patterns = errorTracer.GetErrorPatterns();
            Console.WriteLine("Error Patterns Analysis:");
            foreach (var pattern in patterns)
            {
                Console.WriteLine($"  {pattern.Code}: {pattern.Count} times");
                Console.WriteLine($"    First: {pattern.FirstOccurrence:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"    Last: {pattern.LastOccurrence:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"    Message: {pattern.CommonMessage}");
            }

            // Export to CSV
            string csv = errorTracer.ExportAsCSV();
            System.IO.File.WriteAllText("error_report.csv", csv);
            Console.WriteLine("✅ Error report exported to error_report.csv");
        }
    }
}

/*
 * EXPECTED LOG OUTPUT:
 * 
 * logs/device-Device.dll-.txt:
 * ─────────────────────────────────────────
 * 2024-01-15 10:30:45.123 +00:00 [INF] 🟢 Logger initialized for Device: Device.dll
 * 2024-01-15 10:30:45.125 +00:00 [INF] ⏹️ Operation START | Operation=Device.Constructor | CorrelationId=A1B2C3D4 | Params={DllFileName: "Device.dll"}
 * 2024-01-15 10:30:45.250 +00:00 [INF] ✅ Operation COMPLETE | Operation=Device.Constructor | CorrelationId=A1B2C3D4 | Status=SUCCESS | Duration=125ms | Result={Result: true}
 * 2024-01-15 10:30:46.100 +00:00 [INF] ⏹️ Operation START | Operation=DeviceCountGet | CorrelationId=F1E2D3C4
 * 2024-01-15 10:30:46.234 +00:00 [INF] ✅ Operation COMPLETE | Operation=DeviceCountGet | CorrelationId=F1E2D3C4 | Status=SUCCESS | Duration=134ms | Result={Result: true}
 * 
 * logs/device-Device.dll-structured-.json:
 * ─────────────────────────────────────────
 * {"Timestamp":"2024-01-15 10:30:45.123","Level":"Information","Message":"🟢 Logger initialized for Device: Device.dll","Properties":{"DeviceId":"Device.dll"}}
 * {"Timestamp":"2024-01-15 10:30:45.125","Level":"Information","Message":"⏹️ Operation START | Operation=Device.Constructor | CorrelationId=A1B2C3D4","Properties":{"Operation":"Device.Constructor","CorrelationId":"A1B2C3D4"}}
 */
