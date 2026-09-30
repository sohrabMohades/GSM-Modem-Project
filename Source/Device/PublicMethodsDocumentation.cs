// ========================================================================
// XML DOCUMENTATION COMMENTS FOR DEVICE PUBLIC METHODS
// This file contains comprehensive XML documentation for all public methods
// so they display properly in IntelliSense tooltips.
// 
// Use this as reference to update DeviceIdentity.cs, DeviceGetOperations.cs, 
// DeviceSendOperations.cs, and other partial Device files.
// ========================================================================

/*

// ==================== DEVICE IDENTITY METHODS ====================

/// <summary>
/// Retrieves the total number of GSM modems currently connected to the system.
/// 
/// سیستم میں موجود کل GSM مودمز کی تعداد حاصل کرتا ہے۔
/// </summary>
/// <returns>
/// true: Count retrieval successful; DeviceCount property updated
/// false: Failed to communicate with DLL or device is busy
/// </returns>
/// <remarks>
/// ✅ Best Practice:
/// <code>
/// if (device.DeviceCountGet(out int count))
/// {
///     Console.WriteLine($"Found {count} modems");
/// }
/// else
/// {
///     Console.WriteLine($"Error: {device.ErrorMessage}");
/// }
/// </code>
/// 
/// Note: This must be called before any device selection/discovery.
/// </remarks>
public bool DeviceCountGet()

/// <summary>
/// Retrieves the device count and stores it in the provided output parameter.
/// 
/// فارسی: تعداد device را دریافت کرده و در پارامتر خروجی ذخیره می‌کند.
/// </summary>
/// <param name="DeviceCount">
/// Output: The number of available modems (0-255).
/// Set to -1 if retrieval failed.
/// </param>
/// <returns>true if successful; false if failed</returns>
public bool DeviceCountGet(ref int DeviceCount)

/// <summary>
/// Retrieves device count with control over internal value update.
/// 
/// فارسی: تعداد device را کنترل‌شده دریافت می‌کند.
/// </summary>
/// <param name="DeviceCount">Output parameter for the device count</param>
/// <param name="InternalValueSet">
/// true: Update internal DeviceCount property
/// false: Only return value, don't update property
/// </param>
/// <returns>true if successful; false if failed</returns>
public bool DeviceCountGet(ref int DeviceCount, bool InternalValueSet)

// ==================== DEVICE SELECTION METHODS ====================

/// <summary>
/// Retrieves or verifies the identity (index and number) of this device instance.
/// 
/// فارسی: هویت (شاخص و شماره) این device را دریافت یا تایید می‌کند.
/// </summary>
/// <returns>true if device is properly identified; false if not selected</returns>
/// <remarks>
/// ✅ Usage:
/// <code>
/// if (device.DeviceIdentity())
/// {
///     Console.WriteLine($"Device {device.DeviceNumber} at index {device.DeviceIndex}");
/// }
/// else
/// {
///     Console.WriteLine("Device not selected");
/// }
/// </code>
/// 
/// ⚠️ Important: Device number becomes immutable after first call.
/// </remarks>
public bool DeviceIdentity()

/// <summary>
/// Selects a specific device by its index number from available modems.
/// 
/// فارسی: یک device خاص را با شاخص آن انتخاب می‌کند.
/// </summary>
/// <param name="DeviceIndex">Index of device to select (0-127)</param>
/// <returns>true if device selected; false if invalid index or error</returns>
public bool DeviceSelect(int DeviceIndex)

/// <summary>
/// Attempts to select a device by its device number.
/// 
/// فارسی: یک device را با شماره آن انتخاب می‌کند.
/// </summary>
/// <param name="DeviceNumber">Unique device number to select</param>
/// <returns>true if selected; false if not found or error</returns>
/// <remarks>
/// This is typically used after device discovery to re-select the same device.
/// </remarks>
public bool DeviceSelectByDeviceNumber(int DeviceNumber)

/// <summary>
/// Changes the device number during the initialization/discovery phase.
/// 
/// فارسی: شماره device را در مرحله اولیه تغییر می‌دهد.
/// </summary>
/// <param name="DeviceNumber">New device number (0-127)</param>
/// <returns>true if changed; false if already locked or invalid</returns>
/// <remarks>
/// ⚠️ CRITICAL RULE:
/// - Can ONLY be called BEFORE the first Refresh() operation
/// - After Refresh: device number becomes IMMUTABLE
/// - Attempting to change after Refresh returns false with DEVICE_NUMBER_CHANGE_NOT_ALLOW error
/// - Device numbering strategy defined in DeviceManager discovery phase
/// 
/// ❌ WRONG:
/// <code>
/// device.DeviceRefresh();           // First refresh
/// device.DeviceNumberChange(5);    // ❌ Will fail!
/// </code>
/// 
/// ✅ CORRECT:
/// <code>
/// device.DeviceNumberChange(5);    // ✅ OK - before refresh
/// device.DeviceRefresh();           // ✅ OK - first refresh locks number
/// </code>
/// </remarks>
public bool DeviceNumberChange(int DeviceNumber)

/// <summary>
/// Deselects the current device and resets its identity information.
/// 
/// فارسی: device فعلی را deselect کرده و اطلاعات هویت آن را reset می‌کند.
/// </summary>
/// <returns>true if successfully unselected; false if failed</returns>
/// <remarks>
/// After unselect:
/// - DeviceNumber = -1
/// - DeviceIndex = -1
/// - InternalStatus = DEVICE_NOT_SELECT
/// - All properties reset to invalid values
/// </remarks>
public bool DeviceUnselect()

// ==================== SMS OPERATIONS ====================

/// <summary>
/// Sends an SMS message using the currently configured device settings.
/// 
/// فارسی: یک پیام SMS را با تنظیمات فعلی ارسال می‌کند.
/// </summary>
/// <param name="PhoneNumber">
/// Destination phone number.
/// Format: +<country_code><number> or <number>
/// Example: "+989123456789" or "09123456789"
/// </param>
/// <param name="SmsText">
/// Message text content.
/// Maximum length depends on SMS_CHAR_MODE:
/// - GSM: 160 characters per SMS
/// - UCS2 (Persian): 70 characters per SMS
/// </param>
/// <returns>true if SMS queued for sending; false if failed or device not ready</returns>
/// <remarks>
/// ✅ Prerequisites:
/// - Device.Status must be READY
/// - SimStatus must be READY
/// - Signal strength acceptable
/// - Device not already BUSY
/// 
/// ⚙️ Configuration (adjust before calling):
/// <code>
/// device.SmsSendDeliveryReport = true;      // Request delivery report
/// device.SmsSendSendInterval = 1000;        // 1 second between retries
/// device.SmsSendRetryTimes = 3;             // Try up to 3 times
/// device.SmsSendSmsValidity = SMS_VALIDITY._1DAY;  // 1 day validity
/// 
/// // Send SMS
/// if (device.SmsSend("+989123456789", "سلام"))
/// {
///     Console.WriteLine("SMS sent");
/// }
/// else
/// {
///     Console.WriteLine($"Error: {device.ErrorMessage}");
/// }
/// </code>
/// 
/// ⚠️ Notes:
/// - Actual sending is asynchronous
/// - DeviceOperationCompleted event raised on completion
/// - SMS center configured in SMSCenter property
/// - Character encoding per SMSCharMode setting
/// </remarks>
public bool SmsSend(string PhoneNumber, string SmsText)

// ==================== DEVICE STATUS METHODS ====================

/// <summary>
/// Gets the GSM modem's current power state.
/// 
/// فارسی: وضعیت قدرت مودم GSM را دریافت می‌کند.
/// </summary>
/// <returns>true if power check successful; false otherwise</returns>
/// <remarks>
/// After success, check GsmPower property for actual power state.
/// </remarks>
public bool GsmPowerGet()

/// <summary>
/// Enables or disables power to the GSM modem.
/// 
/// فارسی: قدرت مودم را فعال یا غیر‌فعال می‌کند.
/// </summary>
/// <param name="PowerEnable">true to turn ON; false to turn OFF</param>
/// <returns>true if successful; false if failed</returns>
public bool GsmPowerSet(bool PowerEnable)

/// <summary>
/// Checks if the GSM modem is connected to hardware.
/// 
/// فارسی: بررسی می‌کند که آیا مودم به سخت‌افزار متصل است.
/// </summary>
/// <returns>true if check succeeded; false otherwise</returns>
/// <remarks>
/// Check GsmIsConnect property for connection status.
/// </remarks>
public bool GsmIsConnectGet()

/// <summary>
/// Checks if the GSM modem is ready for operations.
/// 
/// فارسی: بررسی می‌کند که آیا مودم برای عملیات آماده است.
/// </summary>
/// <returns>true if ready; false otherwise</returns>
public bool GsmIsReadyGet()

/// <summary>
/// Retrieves the current GSM status and related information.
/// 
/// فارسی: وضعیت GSM فعلی و اطلاعات مرتبط را دریافت می‌کند.
/// </summary>
/// <param name="GsmStatus">Output: GSM status enum value</param>
/// <param name="StatusValue">Output: Numeric status code</param>
/// <param name="StatusMessage">Output: Descriptive status message</param>
/// <returns>true if retrieved successfully; false otherwise</returns>
public bool GsmStatusGet(ref GSM_STATUS GsmStatus, ref int StatusValue, ref string StatusMessage)

// ==================== DEVICE INFO METHODS ====================

/// <summary>
/// Retrieves the IMEI (International Mobile Equipment Identity) of the modem.
/// 
/// فارسی: شناسه بین‌المللی تجهیزات موبایل (IMEI) دریافت می‌کند.
/// </summary>
/// <param name="Imei">Output: 15-16 digit IMEI string</param>
/// <returns>true if successful; false otherwise</returns>
/// <remarks>
/// IMEI is a unique identifier for the hardware modem.
/// Format: 15-16 digits (e.g., "865807042010913")
/// </remarks>
public bool IMEIGet(ref string Imei)

/// <summary>
/// Retrieves the firmware version of the GSM modem.
/// 
/// فارسی: نسخه نرم‌افزار مودم GSM را دریافت می‌کند.
/// </summary>
/// <param name="Version">Output: Version string</param>
/// <returns>true if successful; false otherwise</returns>
public bool SoftwareVersionGet(ref string Version)

/// <summary>
/// Retrieves detailed information about the modem hardware model.
/// 
/// فارسی: اطلاعات مدل سخت‌افزار مودم را دریافت می‌کند.
/// </summary>
/// <param name="DeviceInfo">Output: Device model information string</param>
/// <returns>true if successful; false otherwise</returns>
public bool DeviceInformationGet(ref string DeviceInfo)

// ==================== SIM INFORMATION METHODS ====================

/// <summary>
/// Retrieves the SIM card Identity (ICCID).
/// 
/// فارسی: شناسه کارت SIM (ICCID) را دریافت می‌کند.
/// </summary>
/// <param name="SimId">Output: 18-22 digit ICCID string</param>
/// <returns>true if successful; false otherwise</returns>
/// <remarks>
/// ICCID uniquely identifies the SIM card.
/// Format: 18-22 digits prefixed with country/issuer codes
/// </remarks>
public bool SimIDGet(ref string SimId)

/// <summary>
/// Retrieves the current status of the SIM card.
/// 
/// فارسی: وضعیت فعلی کارت SIM را دریافت می‌کند.
/// </summary>
/// <param name="SimStatus">Output: SIM_STATUS enum value</param>
/// <returns>true if successful; false otherwise</returns>
/// <remarks>
/// Possible values:
/// - READY: SIM functional, ready for operations
/// - PIN_PUK_ERROR: PIN/PUK locked or incorrect
/// - IDENTITY_ERROR: SIM identity not recognized
/// - UNKNOWN_ERROR: Cannot determine status
/// </remarks>
public bool SimStatusGet(ref SIM_STATUS SimStatus)

/// <summary>
/// Retrieves the National Mobile Identity associated with the SIM.
/// 
/// فارسی: شناسه ملی موبایل مرتبط با SIM را دریافت می‌کند.
/// </summary>
/// <param name="NationalId">Output: National ID string (14-15 digits)</param>
/// <returns>true if successful; false otherwise</returns>
public bool NationalMobileIDGet(ref string NationalId)

/// <summary>
/// Retrieves the SMS Service Center (SMSC) number from the SIM.
/// 
/// فارسی: شماره مرکز سرویس پیام (SMSC) را دریافت می‌کند.
/// </summary>
/// <param name="SmscNumber">Output: SMSC phone number</param>
/// <returns>true if successful; false otherwise</returns>
public bool SMSCenterGet(ref string SmscNumber)

// ==================== NETWORK INFORMATION METHODS ====================

/// <summary>
/// Retrieves the current cellular signal strength.
/// 
/// فارسی: قدرت سیگنال فعلی را دریافت می‌کند.
/// </summary>
/// <param name="Signal">Output: Signal strength 0-100 (percentage)</param>
/// <returns>true if successful; false otherwise</returns>
/// <remarks>
/// Signal strength 0-100 represents percentage of maximum possible signal.
/// 0: No signal
/// 50: Average signal
/// 100: Excellent signal
/// 
/// Check SignalMinValue property for minimum acceptable threshold.
/// </remarks>
public bool SignalGet(ref int Signal)

/// <summary>
/// Retrieves the name/identifier of the current mobile network operator.
/// 
/// فارسی: نام اپراتور شبکه موبایل فعلی را دریافت می‌کند.
/// </summary>
/// <param name="OperatorName">Output: Operator name string</param>
/// <returns>true if successful; false otherwise</returns>
/// <remarks>
/// Examples: "MCI", "Hamrah-e Avval", "RighTel" (in Iran)
/// </remarks>
public bool NetworkNameGet(ref string OperatorName)

/// <summary>
/// Retrieves the date and time synchronized from the mobile network.
/// 
/// فارسی: تاریخ و زمان هماهنگ‌شده از شبکه را دریافت می‌کند.
/// </summary>
/// <param name="DateTime">Output: DateTime string in device format</param>
/// <returns>true if successful; false otherwise</returns>
/// <remarks>
/// Format depends on device manufacturer.
/// Typically: "24/07/15,12:30:45+04:30" (YY/MM/DD,HH:MM:SS+TZ)
/// </remarks>
public bool GSMDateTimeGet(ref string DateTime)

// ==================== INBOX / STORAGE METHODS ====================

/// <summary>
/// Retrieves SMS inbox storage information.
/// 
/// فارسی: اطلاعات ذخیره‌سازی صندوق ورودی پیام را دریافت می‌کند.
/// </summary>
/// <param name="TotalSpace">Output: Total SMS storage capacity</param>
/// <param name="FreeSpace">Output: Available free SMS slots</param>
/// <param name="SmsCount">Output: Current number of SMS messages stored</param>
/// <returns>true if successful; false otherwise</returns>
/// <remarks>
/// These values help monitor SIM card SMS storage.
/// Example: TotalSpace=255, FreeSpace=200, SmsCount=55
/// </remarks>
public bool InboxGet(ref int TotalSpace, ref int FreeSpace, ref int SmsCount)

*/

// ========================================================================
// END OF REFERENCE DOCUMENTATION
// Copy sections above and adapt them into the appropriate Device*.cs files
// ========================================================================
