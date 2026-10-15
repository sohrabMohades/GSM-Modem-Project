using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GSM.Core.Source.Device;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        private static class StringAnalyzer
        {
            private const string SMS_SEND_RESULT_SAMPLE = " - Sample: R1:11--R2:12--R3:13--";
            private const string SMS_SEND_RESULT_START = " - Must Start with: R1:";
            private const string SMS_SEND_RESULT_GAP = " - R serry must without Gap -> Error  R1,R3,R4 (R2 lose)";
            private const string SMS_SEND_RESULT_SAME = " - R serry must without Same Rx -> Error  R1,R1,R2 (R1 same)";

            public enum ERROR_CODE
            {
                NO_ERROR = 0,
                SEND_SMS_RESULT = 1,
                SEND_SMS_RESULT_PARSE_ERROR = 2,
                USSD_BALANCE_PARSE_ERROR = 3,
                USSD_MOBILE_NUMBER_PARSE_ERROR = 4,
            }
            public class AnalyzerResult<T>
            {
                public bool Result { get; set; }
                public T Data { get; set; }
                public ERROR_CODE ErrorCode { get; set; }
                public string ErrorMessage { get; set; }
            }
            public class BalanceData
            {
                public string Value { get; set; }
                public string ProviderDomain { get; set; }
                public string UssdText { get; set; }
            }
            public class MobileData
            {
                public string Value { get; set; }
                public string UssdText { get; set; }
            }

            private static readonly Regex _cusdRegex = new Regex(@"\+CUSD:\s*(?<status>[012])\s*,\s*""(?<payload>[^""]*)""\s*,\s*72",
                RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _hexPayloadRegex = new Regex(@"\A[0-9A-Fa-f]+\z", RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _whitespaceRegex = new Regex(@"\s+", RegexOptions.CultureInvariant | RegexOptions.Compiled);

            private static readonly Regex _smsSendResultRegex = new Regex(@"R(?<index>\d+):(?<reference>\d+)--", RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _smsSendResultFullRegex = new Regex(@"^\s*(R\d+:\d+--)+\s*$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _balanceTextRegex = new Regex(@"(?<balance>[0-9]+)\s*ریال", RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _balanceQuestionMarkRegex = new Regex(@"(?<balance>[0-9]{1,})\?+", RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _providerDomainRegex = new Regex(@"(?<domain>SHOP\.MCI\.IR|MCI\.IR|IRANCELL\.IR|MTN\.IR)",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _mobileNumberRegex = new Regex(@"[01]\.(?<mobileNumber>09[0-9]{9})", RegexOptions.CultureInvariant | RegexOptions.Compiled);

            public static AnalyzerResult<List<string>> ParseSmsSendResult(string smsSendResult)
            {
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

                    var items = new List<(int R, string Reference)>();
                    foreach (Match match in matches)
                    {
                        int r = int.Parse(match.Groups["index"].Value);
                        string reference = match.Groups["reference"].Value;
                        items.Add((r, reference));
                    }

                    // شماره R تکراری
                    if (items.GroupBy(x => x.R).Any(g => g.Count() > 1))
                    {
                        return new AnalyzerResult<List<string>>
                        {
                            Result = false,
                            Data = null,
                            ErrorCode = ERROR_CODE.SEND_SMS_RESULT_PARSE_ERROR,
                            ErrorMessage = smsSendResult + SMS_SEND_RESULT_SAME
                        };
                    }

                    // مرتب سازی
                    items = items.OrderBy(x => x.R).ToList();
                    // باید از R1 شروع شود
                    if (items[0].R != 1)
                    {
                        return new AnalyzerResult<List<string>>
                        {
                            Result = false,
                            Data = null,
                            ErrorCode = ERROR_CODE.SEND_SMS_RESULT_PARSE_ERROR,
                            ErrorMessage = smsSendResult + SMS_SEND_RESULT_START
                        };
                    }

                    // بررسی نبود شکاف
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
                catch
                {
                    return new AnalyzerResult<List<string>>
                    {
                        Result = false,
                        Data = null,
                        ErrorCode = ERROR_CODE.SEND_SMS_RESULT_PARSE_ERROR,
                        ErrorMessage = smsSendResult
                    };
                }
            }

            public static AnalyzerResult<BalanceData> TryParseCusdBalance(string response)
            {
                if (string.IsNullOrWhiteSpace(response))
                {
                    return new AnalyzerResult<BalanceData>
                    {
                        Result = false,
                        Data = null,
                        ErrorCode = ERROR_CODE.NO_ERROR,
                        ErrorMessage = string.Empty
                    };
                }
                List<string> ussdTexts = ExtractCusdTexts(response);
                foreach (string ussdText in ussdTexts)
                {
                    Match balanceMatch = _balanceQuestionMarkRegex.Match(ussdText);
                    if (!balanceMatch.Success)
                    {
                        balanceMatch = _balanceTextRegex.Match(ussdText);
                        if (!balanceMatch.Success)
                        {
                            continue;
                        }
                    }
                    string balance = balanceMatch.Groups["balance"].Value.Trim();
                    string providerDomain = string.Empty;
                    Match domainMatch = _providerDomainRegex.Match(ussdText);
                    if (domainMatch.Success)
                    {
                        providerDomain = domainMatch.Groups["domain"].Value;
                    }
                    return new AnalyzerResult<BalanceData>
                    {
                        Result = true,
                        Data = new BalanceData { Value = balance, ProviderDomain = providerDomain, UssdText = ussdText },
                        ErrorCode = ERROR_CODE.NO_ERROR,
                        ErrorMessage = string.Empty
                    };
                }
                return new AnalyzerResult<BalanceData>
                {
                    Result = false,
                    Data = null,
                    ErrorCode = ERROR_CODE.USSD_BALANCE_PARSE_ERROR,
                    ErrorMessage = response
                };
            }

            public static AnalyzerResult<MobileData> TryParseCusdMobileNumber(string response)
            {
                if (string.IsNullOrWhiteSpace(response))
                {
                    return new AnalyzerResult<MobileData>
                    {
                        Result = false,
                        Data = null,
                        ErrorCode = ERROR_CODE.NO_ERROR,
                        ErrorMessage = string.Empty
                    };
                }
                List<string> ussdTexts = ExtractCusdTexts(response);
                foreach (string ussdText in ussdTexts)
                {
                    Match mobileMatch = _mobileNumberRegex.Match(ussdText);
                    if (!mobileMatch.Success)
                    {
                        continue;
                    }
                    return new AnalyzerResult<MobileData>
                    {
                        Result = true,
                        Data = new MobileData { Value = mobileMatch.Groups["mobileNumber"].Value, UssdText = ussdText },
                        ErrorCode = ERROR_CODE.NO_ERROR,
                        ErrorMessage = string.Empty
                    };
                }
                return new AnalyzerResult<MobileData>
                {
                    Result = false,
                    Data = null,
                    ErrorCode = ERROR_CODE.USSD_MOBILE_NUMBER_PARSE_ERROR,
                    ErrorMessage = string.Empty
                };
            }

            public static bool UssdResponseDetection(string response)
            {
                if (string.IsNullOrWhiteSpace(response))
                {
                    return false;
                }
                return _cusdRegex.IsMatch(response);
            }
            
            //Helper
            //USSD response finder and decoder
            private static List<string> ExtractCusdTexts(string response)
            {
                List<string> result = new List<string>();
                if (string.IsNullOrWhiteSpace(response))
                {
                    return result;
                }
                MatchCollection cusdMatches = _cusdRegex.Matches(response);
                foreach (Match cusdMatch in cusdMatches)
                {
                    string payload = cusdMatch.Groups["payload"].Value;
                    string decodedText;
                    if (TryDecodeCusdHexPayload(payload, out decodedText))
                    {
                        result.Add(decodedText);
                    }
                    else
                    {
                        result.Add(payload);
                    }
                }
                return result;
            }
            //End : USSD response finder and decoder
            //Hex Decoder
            private static bool TryDecodeCusdHexPayload(string payload, out string decodedText)
            {
                decodedText = null;
                if (string.IsNullOrWhiteSpace(payload))
                {
                    return false;
                }
                string hexPayload = _whitespaceRegex.Replace(payload, string.Empty);
                if (hexPayload.Length == 0)
                {
                    return false;
                }
                //Hex String contain 4 code for every character, such : "F0E1" or "123F089A"
                if (hexPayload.Length % 4 != 0)
                {
                    return false;
                }
                //try to decode the hex string to text
                if (!_hexPayloadRegex.IsMatch(hexPayload))
                {
                    return false;
                }
                StringBuilder decoded = new StringBuilder(hexPayload.Length / 4);
                for (int index = 0; index < hexPayload.Length; index += 4)
                {
                    if (!ushort.TryParse(hexPayload.Substring(index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort codeUnit))
                    {
                        return false;
                    }
                    decoded.Append((char)codeUnit);
                }
                decodedText = decoded.ToString();
                return true;
            }
            //Hex Decoder
            //End : Helper

            // ============================================================
            // Buffer Layer 1 - Pattern Definitions
            // ============================================================
            // MODEM:STARTUP -> Modem Startup
            private static readonly Regex _bufferModemStartupRegex = new Regex(@"MODEM:STARTUP", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            // +PBREADY -> Modem Ready
            private static readonly Regex _bufferModemReadyRegex = new Regex(@"\+PBREADY", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            // ATE1 / ATE0 -> Echo Mode Set
            private static readonly Regex _bufferEchoModeSetRegex = new Regex(@"ATE[01]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _bufferEchoModeResultRegex = new Regex(@"ATE[ \t]*(?<echoMode>[01])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            // AT (بدون پسوند) -> GSM Connection Check
            private static readonly Regex _bufferGsmConnectionCheckRegex = new Regex(@"AT[ \t]*\r?\n", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            // AT+CSQ -> Signal Get
            private static readonly Regex _bufferSignalGetRegex = new Regex(@"AT\+CSQ", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _bufferSignalResultRegex = new Regex(@"AT\+CSQ[ \t]*:[ \t]*(?<rssi>\d+)[ \t]*,[ \t]*(?<ber>\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant |
                                                                               RegexOptions.Compiled);
            // AT+CMGF= -> PDU Mode Set
            private static readonly Regex _bufferPduModeSetRegex = new Regex(@"AT\+CMGF[ \t]*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _bufferPduModeResultRegex = new Regex(@"AT\+CMGF[ \t]*=[ \t]*(?<mode>\d)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            // AT+CSCS= -> Char Mode Set
            private static readonly Regex _bufferCharModeSetRegex = new Regex(@"AT\+CSCS[ \t]*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _bufferCharModeResultRegex = new Regex(@"AT\+CSCS[ \t]*=[ \t]*""(?<charType>[^""]*)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant |
                                                                                 RegexOptions.Compiled);
            // AT+CUSD= -> USSD Request
            private static readonly Regex _bufferUssdRequestRegex = new Regex(@"AT\+CUSD[ \t]*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _bufferUssdRequestResultRegex = new Regex(@"AT\+CUSD[ \t]*=[ \t]*(?<mode>\d+)[ \t]*,[ \t]*""(?<ussdCode>[^""]*)""[ \t]*,[ \t]*(?<dcs>\d+)",
                                                                                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            // +CUSD: -> USSD Result
            private static readonly Regex _bufferUssdResponseRegex = new Regex(@"\+CUSD[ \t]*:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _bufferUssdResponseResultRegex = new Regex(@"\+CUSD[ \t]*:[ \t]*(?<status>\d+)[ \t]*,[ \t]*""(?<ussdText>[^""]*)""[ \t]*,[ \t]*(?<dcs>\d+)",
                                                                                     RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _bufferSmsReceivedRegex = new Regex(@"\+SMSFLAG[ \t]*:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _bufferSmsReceivedResultRegex = new Regex(@"\+SMSFLAG[ \t]*:[ \t]*""(?<storage>[^""]*)""[ \t]*,[ \t]*(?<index>\d+)",
                                                                                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

            //private static readonly Regex _bufferCmgrCommandRegex = new Regex(@"[ \t]*AT\+CMGR[ \t]*=[ \t]*(?<index>\d+)[ \t]*",
            //                        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            //private static readonly Regex _bufferCmgrResponseRegex = new Regex(@"[ \t]*\+CMGR[ \t]*:[ \t]*(?<header>[\r\n]*)",
            //                        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

            private static readonly Regex _bufferOkRegex = new Regex(@"(?:OK)",
                                    RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _bufferErrorRegex = new Regex(@"(?:ERROR)",
                                    RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            // پایان پاسخ CMGR می‌تواند OK یا ERROR باشد.
            private static readonly Regex _bufferCmgrResponseEndRegex = new Regex(@"(?:OK|ERROR)",
                                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            private static readonly Regex _bufferTerminalRegex = new Regex(@"(?:OK|ERROR)",
                                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            // ============================================================
            // Buffer Pattern Definition
            // ============================================================
            private sealed class BufferPattern
            {
                public BUFFER_ACTION Action;
                public Regex StartRegex;
                public Regex EndRegex;
                public BufferPattern(BUFFER_ACTION Action, Regex StartRegex, Regex EndRegex = null)
                {
                    this.Action = Action;
                    this.StartRegex = StartRegex;
                    this.EndRegex = EndRegex;
                }
            }
            // ترتیب الگوها در صورت پیدا شدن چند الگو در یک موقعیت
            // اولویت را مشخص می‌کند.
            private static readonly BufferPattern[] _bufferPatterns =
            {
                new BufferPattern(BUFFER_ACTION.MODEM_STARTUP, _bufferModemStartupRegex),
                new BufferPattern(BUFFER_ACTION.MODEM_READY, _bufferModemReadyRegex),
                new BufferPattern(BUFFER_ACTION.GSM_CONNECTION_ATE1, _bufferEchoModeSetRegex),
                new BufferPattern(BUFFER_ACTION.GSM_CONNECTION_AT, _bufferGsmConnectionCheckRegex),
                new BufferPattern(BUFFER_ACTION.GSM_SIGNAL, _bufferSignalGetRegex),
                new BufferPattern(BUFFER_ACTION.PDU_MODE_SET, _bufferPduModeSetRegex),
                new BufferPattern(BUFFER_ACTION.CHAR_MODE_SET, _bufferCharModeSetRegex),
                new BufferPattern(BUFFER_ACTION.USSD_REQUEST, _bufferUssdRequestRegex),
                new BufferPattern(BUFFER_ACTION.USSD_RESPONSE, _bufferUssdResponseRegex),
                new BufferPattern(BUFFER_ACTION.SMS_RECEIVED, _bufferSmsReceivedRegex),
                //new BufferPattern(BUFFER_ACTION.INBOX_READ_COMMAND, _bufferCmgrCommandRegex),
                //new BufferPattern(BUFFER_ACTION.INBOX_READ_RESPONSE, _bufferCmgrResponseRegex, _bufferCmgrResponseEndRegex),
            };
            // ============================================================
            // Buffer Layer 1 - Main Analyzer
            // ============================================================
            public static BufferDataGroup BufferAnalyzeRawMessage(string SourceMessage)
            {
                BufferDataGroup bufferDataGroup = new BufferDataGroup { SourceMessage = SourceMessage };
                if (string.IsNullOrEmpty(SourceMessage))
                    return bufferDataGroup;
                int position = 0;
                BufferPattern nextPattern;
                Match nextMatch;
                while (position < SourceMessage.Length)
                {
                    BufferPattern pattern;
                    Match startMatch;

                    bool found = BufferFindNextPattern(SourceMessage, position, out pattern, out startMatch);
                    // هیچ الگوی شناخته‌شده دیگری وجود ندارد.
                    // تمام باقی‌مانده به‌عنوان UNKNOWN حفظ می‌شود.
                    // بررسی نشانگر پایانی از موقعیت فعلی
                    Match terminalAtPosition = _bufferTerminalRegex.Match(SourceMessage, position);
                    // اگر نشانگر پایانی قبل از الگوی شناخته‌شده بعدی باشد،
                    // یا اصلاً الگوی شناخته‌شده‌ای وجود نداشته باشد،
                    // نشانگر پایانی را همین‌جا پردازش کن.
                    if (terminalAtPosition.Success && (!found || terminalAtPosition.Index < startMatch.Index))
                    {
                        int terminalEnd = terminalAtPosition.Index + terminalAtPosition.Length;
                        string terminalMessage = SourceMessage.Substring(position, terminalEnd - position);
                        BufferAddData(bufferDataGroup, BUFFER_ACTION.UNKNOWN, terminalMessage);
                        position = terminalEnd;
                        continue;
                    }
                    if (!found)
                    {
                        BufferAddData(bufferDataGroup, BUFFER_ACTION.UNKNOWN, SourceMessage.Substring(position));
                        break;
                    }
                    // متن قبل از الگوی بعدی، یک بخش مستقل است.
                    if (startMatch.Index > position)
                    {
                        string unknownMessage = SourceMessage.Substring(position, startMatch.Index - position);
                        BufferAddData(bufferDataGroup, BUFFER_ACTION.UNKNOWN, unknownMessage);
                    }
                    int segmentStart = startMatch.Index;
                    int segmentEnd = SourceMessage.Length;
                    // 1. پیدا کردن الگوی شناخته‌شده بعدی
                    bool nextFound = BufferFindNextPattern(SourceMessage, startMatch.Index + startMatch.Length, out nextPattern, out nextMatch);
                    if (nextFound)
                        segmentEnd = nextMatch.Index;
                    // 2. جست‌وجوی نشانگر پایانی OK یا ERROR
                    Match terminalMatch = _bufferTerminalRegex.Match(SourceMessage, startMatch.Index + startMatch.Length);
                    if (terminalMatch.Success)
                    {
                        int terminalEnd = terminalMatch.Index + terminalMatch.Length;
                        if (terminalEnd < segmentEnd)
                            segmentEnd = terminalEnd;
                    }
                    // 2. بررسی پایان اختصاصی الگو، در صورت وجود
                    if (pattern.EndRegex != null)
                    {
                        Match endMatch = pattern.EndRegex.Match(SourceMessage, startMatch.Index + startMatch.Length);
                        if (endMatch.Success)
                        {
                            int endPosition = endMatch.Index + endMatch.Length;
                            // پایان اختصاصی فقط وقتی معتبر است که
                            // قبل از الگوی شناخته‌شده بعدی قرار داشته باشد.
                            if (endPosition <= segmentEnd)
                                segmentEnd = endPosition;
                        }
                    }
                    // محافظت در برابر حلقه بی‌نهایت
                    if (segmentEnd <= segmentStart)
                    {
                        BufferAddData(bufferDataGroup, BUFFER_ACTION.UNKNOWN, SourceMessage.Substring(segmentStart, 1));
                        position = segmentStart + 1;
                        continue;
                    }
                    string message = SourceMessage.Substring(segmentStart, segmentEnd - segmentStart);
                    BufferAddData(bufferDataGroup, pattern.Action, message);
                    position = segmentEnd;
                }
                return bufferDataGroup;
            }
            // ============================================================
            // Find the next known pattern
            // ============================================================
            private static bool BufferFindNextPattern(string SourceMessage, int SearchFrom, out BufferPattern FoundPattern, out Match FoundMatch)
            {
                FoundPattern = null;
                FoundMatch = Match.Empty;
                int earliestIndex = int.MaxValue;
                foreach (BufferPattern pattern in _bufferPatterns)
                {
                    Match match = pattern.StartRegex.Match(SourceMessage, SearchFrom);
                    if (!match.Success)
                        continue;
                    if (match.Index < earliestIndex)
                    {
                        earliestIndex = match.Index;
                        FoundPattern = pattern;
                        FoundMatch = match;
                    }
                }
                return FoundPattern != null;
            }
            // ============================================================
            // Create and populate one BufferData record
            // ============================================================
            private static void BufferAddData(BufferDataGroup bufferDataGroup, BUFFER_ACTION Action, string Message)
            {
                if (string.IsNullOrWhiteSpace(Message))
                    return;
                Message = Message.Trim();
                BufferData data = new BufferData
                {
                    Action = Action,
                    Message = Message,
                    ReceivedTime = DateTime.Now
                };
                BufferExtractStatus(data);
                BufferExtractFields(data);
                bufferDataGroup.BufferData.Add(data);
            }
            // ============================================================
            // Extract explicitly observed status markers
            // ============================================================
            private static void BufferExtractStatus(BufferData data)
            {
                if (_bufferErrorRegex.IsMatch(data.Message))
                    data.Status = BUFFER_ACTION_STATUS.FAIL;
                else if (data.Action == BUFFER_ACTION.USSD_RESPONSE && _bufferUssdResponseResultRegex.IsMatch(data.Message))
                    data.Status = BUFFER_ACTION_STATUS.SUCCEEDED;
                else if (_bufferOkRegex.IsMatch(data.Message))
                    data.Status = BUFFER_ACTION_STATUS.SUCCEEDED;
                else
                    data.Status = BUFFER_ACTION_STATUS.UNKNOWN;
            }
            // ============================================================
            // Extract fields according to the recognized pattern
            // ============================================================
            private static void BufferExtractFields(BufferData data)
            {
                switch (data.Action)
                {
                    case BUFFER_ACTION.MODEM_STARTUP:
                        {
                            break;
                        }

                    case BUFFER_ACTION.MODEM_READY:
                        {
                            break;
                        }

                    case BUFFER_ACTION.GSM_CONNECTION_ATE1:
                        {
                            Match match = _bufferEchoModeResultRegex.Match(data.Message);
                            if (match.Success)
                                data.Detail.Add(match.Groups["echoMode"].Value);
                            break;
                        }

                    case BUFFER_ACTION.GSM_CONNECTION_AT:
                        {
                            break;
                        }

                    case BUFFER_ACTION.GSM_SIGNAL:
                        {
                            Match match = _bufferSignalResultRegex.Match(data.Message);
                            if (match.Success)
                            {
                                data.Detail.Add(match.Groups["rssi"].Value);
                                data.Detail.Add(match.Groups["ber"].Value);
                            }
                            break;
                        }

                    case BUFFER_ACTION.PDU_MODE_SET:
                        {
                            Match match = _bufferPduModeResultRegex.Match(data.Message);
                            if (match.Success)
                                data.Detail.Add(match.Groups["mode"].Value);
                            break;
                        }

                    case BUFFER_ACTION.CHAR_MODE_SET:
                        {
                            Match match = _bufferCharModeResultRegex.Match(data.Message);
                            if (match.Success)
                                data.Detail.Add(match.Groups["charType"].Value);
                            break;
                        }

                    case BUFFER_ACTION.USSD_REQUEST:
                        {
                            Match match = _bufferUssdRequestResultRegex.Match(data.Message);
                            if (match.Success)
                            {
                                data.Detail.Add(match.Groups["mode"].Value);
                                data.Detail.Add(match.Groups["ussdCode"].Value);
                                data.Detail.Add(match.Groups["dcs"].Value);
                            }
                            break;
                        }

                    case BUFFER_ACTION.USSD_RESPONSE:
                        {
                            Match match = _bufferUssdResponseResultRegex.Match(data.Message);
                            if (match.Success)
                            {
                                data.Detail.Add(match.Groups["status"].Value);
                                data.Detail.Add(match.Groups["ussdText"].Value);
                                data.Detail.Add(match.Groups["dcs"].Value);
                            }
                            break;
                        }

                    case BUFFER_ACTION.SMS_RECEIVED:
                        {
                            Match match = _bufferSmsReceivedResultRegex.Match(data.Message);
                            if (match.Success)
                            {
                                data.Detail.Add(match.Groups["storage"].Value);
                                data.Detail.Add(match.Groups["index"].Value);
                            }
                            break;
                        }

                    case BUFFER_ACTION.INBOX_READ_COMMAND:
                        {
                            /*
                            Match match = _bufferCmgrCommandRegex.Match(data.Message);
                            if (match.Success)
                                data.MessageAnalyzed = match.Groups["index"].Value;
                            */
                            break;
                        }

                    case BUFFER_ACTION.INBOX_READ_RESPONSE:
                        {
                            /*
                            Match headerMatch = _bufferCmgrResponseRegex.Match(data.Message);
                            if (!headerMatch.Success)
                                break;
                            // Header: 1,,158
                            data.Detail = headerMatch.Groups["header"].Value;
                            // استخراج محتوا از انتهای Header تا نشانگر پایانی.
                            int bodyStart = headerMatch.Index + headerMatch.Length;
                            Match endMatch = _bufferCmgrResponseEndRegex.Match(data.Message, bodyStart);
                            int bodyEnd = endMatch.Success ? endMatch.Index : data.Message.Length;
                            if (bodyEnd >= bodyStart)
                            {
                                string body = data.Message.Substring(bodyStart, bodyEnd - bodyStart);
                                body = body.Trim();
                                if (!string.IsNullOrEmpty(body))
                                    data.MessageAnalyzed = body;
                            }
                            */
                            break;
                        }

                    default:
                        {
                            // UNKNOWN یا الگوی فاقد استخراج اختصاصی:
                            // متن اصلی حفظ می‌شود و تحلیل اضافی انجام نمی‌دهیم.
                            break;
                        }
                }
            }
        }
    }
}
