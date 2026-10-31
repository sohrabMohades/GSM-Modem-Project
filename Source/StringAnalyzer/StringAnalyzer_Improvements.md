# راهکارهای عملی و کد بهبود شده برای StringAnalyzer

## فهرست محتویات
1. [کد بهبود شده برای ParseSmsSendResult](#1-بهبود-parsesmssendresult)
2. [کد بهبود شده برای Regex Patterns](#2-بهبود-regex-patterns)
3. [کد بهبود شده برای Buffer Parser](#3-بهبود-buffer-parser)
4. [سایر بهبودی‌ها](#4-سایر-بهبودیها)

---

## 1. بهبود ParseSmsSendResult

### مشکل اول: IndexOutOfRangeException + Exception Handling

```csharp
/// <summary>
/// تجزیه نتیجه ارسال SMS با بررسی‌های بهتر
/// </summary>
public static AnalyzerResult<List<string>> ParseSmsSendResult(string smsSendResult)
{
	// Guard Clause برای null یا خالی
	if (string.IsNullOrWhiteSpace(smsSendResult))
	{
		return new AnalyzerResult<List<string>>
		{
			Result = false,
			Data = null,
			ErrorCode = ERROR_CODE.NO_ERROR,
			ErrorMessage = string.Empty
		};
	}

	try
	{
		MatchCollection matches = _smsSendResultRegex.Matches(smsSendResult);
		if (matches.Count == 0 || !_smsSendResultFullRegex.IsMatch(smsSendResult))
		{
			return new AnalyzerResult<List<string>>
			{
				Result = false,
				Data = null,
				ErrorCode = ERROR_CODE.SEND_SMS_RESULT_PARSE_ERROR,
				ErrorMessage = smsSendResult + SMS_SEND_RESULT_SAMPLE
			};
		}

		var items = new List<(int R, string Reference)>(matches.Count);

		// اضافه کردن items به لیست
		foreach (Match match in matches)
		{
			if (!int.TryParse(match.Groups["index"].Value, out int r))
			{
				return new AnalyzerResult<List<string>>
				{
					Result = false,
					Data = null,
					ErrorCode = ERROR_CODE.SEND_SMS_RESULT_PARSE_ERROR,
					ErrorMessage = smsSendResult + " - Invalid R number format"
				};
			}

			string reference = match.Groups["reference"].Value;
			items.Add((r, reference));
		}

		// بررسی تکرار شماره R
		using (var hashSet = new HashSet<int>())
		{
			foreach (var item in items)
			{
				if (!hashSet.Add(item.R))  // بهتر از GroupBy
				{
					return new AnalyzerResult<List<string>>
					{
						Result = false,
						Data = null,
						ErrorCode = ERROR_CODE.SEND_SMS_RESULT_PARSE_ERROR,
						ErrorMessage = smsSendResult + SMS_SEND_RESULT_SAME
					};
				}
			}
		}

		// مرتب‌سازی
		items.Sort((a, b) => a.R.CompareTo(b.R));

		// بررسی شروع از R1 (معمول‌تر از items.Count > 0 بررسی)
		if (items.Count == 0 || items[0].R != 1)
		{
			return new AnalyzerResult<List<string>>
			{
				Result = false,
				Data = null,
				ErrorCode = ERROR_CODE.SEND_SMS_RESULT_PARSE_ERROR,
				ErrorMessage = smsSendResult + SMS_SEND_RESULT_START
			};
		}

		// بررسی شکاف‌ها
		for (int i = 0; i < items.Count; i++)
		{
			int expectedR = i + 1;
			if (items[i].R != expectedR)
			{
				return new AnalyzerResult<List<string>>
				{
					Result = false,
					Data = null,
					ErrorCode = ERROR_CODE.SEND_SMS_RESULT_PARSE_ERROR,
					ErrorMessage = smsSendResult + SMS_SEND_RESULT_GAP
				};
			}
		}

		return new AnalyzerResult<List<string>>
		{
			Result = true,
			Data = items.Select(x => x.Reference).ToList(),
			ErrorCode = ERROR_CODE.NO_ERROR,
			ErrorMessage = string.Empty
		};
	}
	catch (ArgumentException aex)
	{
		System.Diagnostics.Debug.WriteLine(
			$"ArgumentException in ParseSmsSendResult: {aex.Message}\n{aex.StackTrace}");

		return new AnalyzerResult<List<string>>
		{
			Result = false,
			Data = null,
			ErrorCode = ERROR_CODE.SEND_SMS_RESULT_PARSE_ERROR,
			ErrorMessage = "Argument error during parsing: " + aex.Message
		};
	}
	catch (Exception ex)
	{
		System.Diagnostics.Debug.WriteLine(
			$"Exception in ParseSmsSendResult: {ex.Message}\n{ex.StackTrace}");

		return new AnalyzerResult<List<string>>
		{
			Result = false,
			Data = null,
			ErrorCode = ERROR_CODE.SEND_SMS_RESULT_PARSE_ERROR,
			ErrorMessage = smsSendResult
		};
	}
}
```

---

## 2. بهبود Regex Patterns

### مشکل: Potential ReDoS vulnerability

```csharp
// ❌ قدیمی - خطرناک برای رشته‌های بزرگ
private static readonly Regex _bufferInboxReadResponseResultRegex = 
	new Regex(@"\+CMGR[ \t]*:[ \t]*(?<messageStatus>\d+)[ \t]*,[ \t]*(?<field2>[^,\r\n]*)[ \t]*,[ \t]*(?<length>\d+)[ \t]*\r?\n(?<textMessage>[\s\S]*)(?=OK|ERROR|$)",
		RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

// ✅ جدید - محدود شده و کارآمد‌تر
private static readonly Regex _bufferInboxReadResponseResultRegex = 
	new Regex(@"\+CMGR[ \t]*:[ \t]*(?<messageStatus>\d+)[ \t]*,[ \t]*(?<field2>[^,\r\n]*)[ \t]*,[ \t]*(?<length>\d+)[ \t]*\r?\n(?<textMessage>[\s\S]{0,50000}?)(?=OK|ERROR|$)",
		RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

// ❌ قدیمی - بسیار پیچیده
private static readonly Regex _bufferSmsDeliveryReportResultRegex = 
	new Regex(@"\+CDS[ \t]*:[ \t]*(?<length>\d+)[ \t]*\r?\n(?<pdu>[\s\S]*)(?=OK|ERROR|$)",
		RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

// ✅ جدید - محدود شده (PDU معمولاً کمتر از 300 کاراکتر)
private static readonly Regex _bufferSmsDeliveryReportResultRegex = 
	new Regex(@"\+CDS[ \t]*:[ \t]*(?<length>\d+)[ \t]*\r?\n(?<pdu>[\s\S]{0,300})(?=OK|ERROR|$)",
		RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
```

### کلاس Regex Validator برای تست:

```csharp
/// <summary>
/// کلاس کمکی برای اعتبارسنجی Regex‌ها
/// </summary>
public sealed class RegexPatternValidator
{
	/// <summary>
	/// تست Regex برای ReDoS vulnerabilities
	/// </summary>
	public static bool TestRegexPerformance(Regex regex, string input, int timeoutMs = 1000)
	{
		try
		{
			using (var cts = new CancellationTokenSource(timeoutMs))
			{
				var task = Task.Run(() => regex.IsMatch(input));
				return task.Wait(timeoutMs);
			}
		}
		catch (OperationCanceledException)
		{
			Debug.WriteLine($"Regex performance issue detected - likely ReDoS vulnerability");
			return false;
		}
	}

	/// <summary>
	/// تولید test input برای ReDoS detection
	/// </summary>
	public static string GenerateReDoSTestInput(string pattern, int length = 5000)
	{
		// حروف تکراری برای تست ReDoS
		return new string('a', length);
	}
}
```

---

## 3. بهبود Buffer Parser

### مشکل اول: استفاده از out parameters

```csharp
// ❌ قدیمی - out parameters
private static bool BufferFindNextPattern(string SourceMessage, int SearchFrom, 
	out BufferPattern FoundPattern, out Match FoundMatch)

// ✅ جدید - Tuple بازگشت
private static (bool Found, BufferPattern Pattern, Match Match) 
	BufferFindNextPatternImproved(string sourceMessage, int searchFrom)
{
	if (string.IsNullOrEmpty(sourceMessage) || searchFrom < 0 || searchFrom > sourceMessage.Length)
		return (false, null, Match.Empty);

	int earliestIndex = int.MaxValue;
	BufferPattern? foundPattern = null;
	Match foundMatch = Match.Empty;

	foreach (BufferPattern pattern in _bufferPatterns)
	{
		Match match = pattern.StartRegex.Match(sourceMessage, searchFrom);
		if (!match.Success)
			continue;

		if (match.Index < earliestIndex)
		{
			earliestIndex = match.Index;
			foundPattern = pattern;
			foundMatch = match;
		}
	}

	return (foundPattern != null, foundPattern, foundMatch);
}

// استفاده:
var (found, pattern, match) = BufferFindNextPatternImproved(message, position);
if (found)
{
	// استفاده از pattern و match
}
```

### مشکل دوم: Null Reference Risk در BufferExtractFields

```csharp
// ❌ خطرناک
private static void BufferExtractFields(BufferData data)
{
	if (data.Status == BUFFER_ACTION_STATUS.OK_MODEM_STATUS)
	{
		Match match = _bufferOkModemStatusRegex.Match(data.Message);
		if (match.Success) 
		{ 
			int modemStatus = (int)match.Groups["modemStatus"].Value[0]; 
			data.Detail.Add(modemStatus.ToString()); // اگر data.Detail null است!
		}
	}
	// ...
}

// ✅ ایمن
private static void BufferExtractFields(BufferData data)
{
	// Guard clause
	if (data == null || data.Detail == null)
		return;

	if (data.Status == BUFFER_ACTION_STATUS.OK_MODEM_STATUS)
	{
		Match match = _bufferOkModemStatusRegex.Match(data.Message);
		if (match.Success && match.Groups["modemStatus"].Success) 
		{ 
			if (match.Groups["modemStatus"].Value.Length > 0 &&
				int.TryParse(match.Groups["modemStatus"].Value.AsSpan(0, 1), 
					NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int modemStatus))
			{
				data.Detail.Add(modemStatus.ToString());
			}
		}
	}
}
```

---

## 4. سایر بهبودی‌ها

### 4.1 Logging Integration

```csharp
/// <summary>
/// StringAnalyzer بهبود شده با Logging
/// </summary>
private static class StringAnalyzerWithLogging
{
	private static ILogger? _logger;

	public static void Initialize(ILogger logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	private static void LogError(string method, string message, Exception? ex = null)
	{
		_logger?.LogError($"[StringAnalyzer.{method}] {message}", ex);
		System.Diagnostics.Debug.WriteLine($"ERROR [{method}]: {message}");
	}

	private static void LogWarning(string method, string message)
	{
		_logger?.LogWarning($"[StringAnalyzer.{method}] {message}");
		System.Diagnostics.Debug.WriteLine($"WARNING [{method}]: {message}");
	}

	private static void LogDebug(string method, string message)
	{
		_logger?.LogDebug($"[StringAnalyzer.{method}] {message}");
		System.Diagnostics.Debug.WriteLine($"DEBUG [{method}]: {message}");
	}
}
```

### 4.2 TryParse بهتر برای محدودیت‌های عددی

```csharp
// ❌ خطرناک
int r = int.Parse(match.Groups["index"].Value);

// ✅ ایمن
if (!int.TryParse(match.Groups["index"].Value, out int r))
{
	LogWarning("ParseSmsSendResult", "Invalid R number format");
	return ErrorResult();
}

// ✅ با محدودیت
if (!int.TryParse(match.Groups["index"].Value, out int r) || r < 1 || r > 255)
{
	LogWarning("ParseSmsSendResult", $"R number out of valid range: {r}");
	return ErrorResult();
}
```

### 4.3 Performance: Caching Regex Results

```csharp
/// <summary>
/// Cache برای نتایج Regex
/// </summary>
private static class RegexResultCache
{
	private static readonly Dictionary<string, Match> _cache = 
		new(StringComparer.Ordinal);

	private static readonly object _lockObject = new object();

	public static bool TryGetCachedMatch(string input, string cacheKey, out Match match)
	{
		lock (_lockObject)
		{
			return _cache.TryGetValue($"{cacheKey}:{input.GetHashCode()}", out match);
		}
	}

	public static void CacheMatch(string input, string cacheKey, Match match)
	{
		lock (_lockObject)
		{
			if (_cache.Count > 1000) // محدودیت cache
			{
				_cache.Clear();
			}
			_cache[$"{cacheKey}:{input.GetHashCode()}"] = match;
		}
	}
}
```

### 4.4 Null-Coalescing و Defensive Programming

```csharp
// ❌ قدیمی
public static AnalyzerResult<BalanceData> TryParseCusdBalance(string response)
{
	if (string.IsNullOrWhiteSpace(response))
	{
		return new AnalyzerResult<BalanceData> { /* ... */ };
	}
	List<string> ussdTexts = ExtractCusdTexts(response);
	// ...
}

// ✅ بهتر - defensive
public static AnalyzerResult<BalanceData> TryParseCusdBalance(string response)
{
	if (string.IsNullOrWhiteSpace(response))
	{
		return EmptyBalanceResult();
	}

	try
	{
		var ussdTexts = ExtractCusdTexts(response) ?? new List<string>();

		if (ussdTexts.Count == 0)
		{
			LogDebug("TryParseCusdBalance", "No USSD texts extracted");
			return EmptyBalanceResult();
		}

		foreach (string ussdText in ussdTexts)
		{
			if (string.IsNullOrWhiteSpace(ussdText))
				continue;

			var result = ParseBalance(ussdText);
			if (result?.Result == true)
				return result;
		}

		return EmptyBalanceResult();
	}
	catch (Exception ex)
	{
		LogError("TryParseCusdBalance", $"Error parsing USSD balance: {response}", ex);
		return ErrorBalanceResult(response);
	}
}

private static AnalyzerResult<BalanceData> EmptyBalanceResult() =>
	new() { Result = false, Data = null, ErrorCode = ERROR_CODE.NO_ERROR };

private static AnalyzerResult<BalanceData> ErrorBalanceResult(string response) =>
	new() { Result = false, Data = null, 
			ErrorCode = ERROR_CODE.USSD_BALANCE_PARSE_ERROR, 
			ErrorMessage = response };
```

---

## خلاصه بهبودی‌ها

| مشکل | راهکار | میزان تأثیر |
|------|--------|-----------|
| Exception Handling ناقص | Try-Catch بهتر + Logging | **بسیار زیاد** |
| ReDoS Vulnerability | محدودیت طول Regex | **بحرانی** |
| out parameters | Tuple بازگشت | **متوسط** |
| Null Reference | Guard Clauses | **زیاد** |
| عملکرد (GroupBy) | HashSet | **کم** |
| Logging | ILogger Integration | **زیاد** |

---

## نکات مهم برای پیاده‌سازی

1. ✅ تمام بهبودی‌ها **معکوس‌پذیر** هستند
2. ✅ می‌توان **به صورت تدریجی** پیاده‌کرد
3. ✅ متوافق با .NET Framework 4.8.1 و .NET 10
4. ⚠️ Tuple syntax نیاز به C# 7.0+ دارد
5. ⚠️ ILogger نیاز به ILogging package دارد

