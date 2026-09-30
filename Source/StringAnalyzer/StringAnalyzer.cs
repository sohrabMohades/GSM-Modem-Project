using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GSM.Core.Source.StringAnalyzer
{
    public enum ACTION
    {
        None = 0,
        Ok,
        EchoModeConfigured,
        SoftwareVersion,
        ModemStartup,
        ModemReady,
        CharacterSetConfigured,
        SmsFormatConfigured,
        UssdResultReceived,
        ClockReadResult,
        ClockSetAccepted,
        SignalStrengthResult,
        SmsSendPrompt,
        SmsSendSucceeded,
        SmsSendFailed,
        NewSmsReceived,
        UssdCommandAccepted,
        SmsReadDeliveryReport,
        SmsReadMessage,
        SmsReadMultipartPart,
        SmsDeleteOneAccepted,
        SmsDeleteReadAccepted,
        SmsDeleteReadAndSentAccepted,
        SmsDeleteReadSentAndUnsentAccepted,
        SmsDeleteAllAccepted,
        SmsStorageStatus,
        ConnectionCheckSucceeded,
        AtError,
        CmeError,
        CmsError,
        NoCarrier,
        Busy,
        NoAnswer,
        NoDialTone,
        Connect,
        SimReady,
        SimPinRequired,
        SimPin2Required,
        SimPukRequired,
        SimPuk2Required,
        SimNotInserted,
        SimStatusUnknown,
        NetworkRegisteredHome,
        NetworkRegisteredRoaming,
        NetworkNotRegistered,
        NetworkSearching,
        NetworkRegistrationDenied,
        NetworkRegistrationUnknown,
        SmsNotification
    }

    public static class StringAnalyzer
    {
        public enum ERROR_CODE
        {
            NO_ERROR = 0,
            SEND_SMS_RESULT = 1
        }
        public static bool ParseSmsSendResult(string smsSendResult, ref List<int> smsReference, ref ERROR_CODE ErrorCode, ref string ErrorMessage)
        {
            smsReference = null;
            // Rule 2: null input
            if (smsSendResult == null || smsSendResult.Length <= 0)
            {
                ErrorCode = ERROR_CODE.NO_ERROR;
                ErrorMessage = string.Empty;
                return false;
            }
            try
            {
                smsReference = new List<int>();
                // Pattern: R{1-10}:{1-2 digits}--
                var regex = new System.Text.RegularExpressions.Regex(@"R(\d+):(\d{1,2})--");
                var matches = regex.Matches(smsSendResult);
                // Rule 4: No matches found
                if (matches.Count == 0)
                {
                    ErrorCode = ERROR_CODE.SEND_SMS_RESULT;
                    ErrorMessage = smsSendResult;
                    return false;
                }
                // Extract and validate each reference
                foreach (Match match in matches)
                {
                    int rNumber = int.Parse(match.Groups[1].Value);
                    int reference = int.Parse(match.Groups[2].Value);
                    // Rule 4: Validate R number is in range 1-10
                    if (rNumber < 1 || rNumber > 10)
                    {
                        ErrorCode = ERROR_CODE.SEND_SMS_RESULT;
                        ErrorMessage = smsSendResult;
                        return false;
                    }
                    smsReference.Add(reference);
                }
                // Rule 3: Valid format
                ErrorCode = ERROR_CODE.NO_ERROR;
                ErrorMessage = string.Empty;
                return true;
            }
            catch
            {
                // Rule 4: Parse error
                ErrorCode = ERROR_CODE.SEND_SMS_RESULT;
                ErrorMessage = smsSendResult;
                return false;
            }
        }

    public sealed class ModemBufferParser
    {
        private readonly object _syncRoot = new object();
        private readonly StringBuilder _pendingText = new StringBuilder();
        private string _pendingCommand = string.Empty;
        private bool _waitingForCmgrPdu;
        private bool _smsSendInProgress;
        private bool _smsSendResultSeen;

        public bool AppendAndParse(string newData, out List<ACTION> actions)
        {
            actions = new List<ACTION>();
            if (string.IsNullOrEmpty(newData))
            {
                return false;
            }

            lock (_syncRoot)
            {
                _pendingText.Append(newData);
                var text = _pendingText.ToString();
                var segmentStart = 0;
                var insideQuotes = false;
                var index = 0;

                while (index < text.Length)
                {
                    if (text[index] == '"')
                    {
                        insideQuotes = !insideQuotes;
                    }
                    else if (!insideQuotes && (text[index] == '\r' || text[index] == '\n'))
                    {
                        ProcessSegment(text.Substring(segmentStart, index - segmentStart), actions);
                        index++;
                        while (index < text.Length && (text[index] == '\r' || text[index] == '\n'))
                        {
                            index++;
                        }
                        segmentStart = index;
                        continue;
                    }

                    index++;
                }

                if (segmentStart > 0)
                {
                    _pendingText.Remove(0, segmentStart);
                }

                var trailingSegment = _pendingText.ToString();
                if (IsCompleteWithoutLineEnding(trailingSegment))
                {
                    ProcessSegment(trailingSegment, actions);
                    _pendingText.Clear();
                }

                if (_pendingText.Length > 65536)
                {
                    _pendingText.Clear();
                    _pendingCommand = string.Empty;
                    _waitingForCmgrPdu = false;
                    _smsSendInProgress = false;
                    _smsSendResultSeen = false;
                }
            }

            return actions.Count > 0;
        }

        public void Reset()
        {
            lock (_syncRoot)
            {
                _pendingText.Clear();
                _pendingCommand = string.Empty;
                _waitingForCmgrPdu = false;
                _smsSendInProgress = false;
                _smsSendResultSeen = false;
            }
        }

        private void ProcessSegment(string segment, List<ACTION> actions)
        {
            var cleanSegment = Regex.Replace(segment, @"[\x00-\x08\x0B-\x1F\x7F]", string.Empty);
            var line = Regex.Replace(cleanSegment, @"\s+", " ").Trim();
            if (line.Length == 0)
            {
                return;
            }

            if (_waitingForCmgrPdu)
            {
                var pdu = Regex.Replace(line, @"\s+", string.Empty);
                if (Regex.IsMatch(pdu, @"\A[0-9A-Fa-f]+\z") && pdu.Length % 2 == 0 && TryClassifyPdu(pdu, out var pduAction))
                {
                    actions.Add(pduAction);
                    _waitingForCmgrPdu = false;
                    return;
                }

                if (!line.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    _waitingForCmgrPdu = false;
                }
            }

            if (Regex.IsMatch(line, @"\AAT(?:\+.*|[A-Z].*|)\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                _pendingCommand = line;
                if (Regex.IsMatch(line, @"\AAT\+CMGS\s*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    _smsSendInProgress = true;
                    _smsSendResultSeen = false;
                }
                return;
            }

            if (line.Equals("OK", StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(ACTION.Ok);
                CompletePendingCommand(actions);
                return;
            }

            if (line.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(ACTION.AtError);
                if (_smsSendInProgress)
                {
                    actions.Add(ACTION.SmsSendFailed);
                }
                ClearPendingCommand();
                return;
            }

            if (Regex.IsMatch(line, @"\A\+CME\s+ERROR\s*:?.*\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.CmeError);
                if (_smsSendInProgress)
                {
                    actions.Add(ACTION.SmsSendFailed);
                }
                ClearPendingCommand();
                return;
            }

            if (Regex.IsMatch(line, @"\A\+CMS\s+ERROR\s*:?.*\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.CmsError);
                if (_smsSendInProgress)
                {
                    actions.Add(ACTION.SmsSendFailed);
                }
                ClearPendingCommand();
                return;
            }

            if (line.Equals(">", StringComparison.Ordinal))
            {
                if (_smsSendInProgress)
                {
                    actions.Add(ACTION.SmsSendPrompt);
                }
                return;
            }

            if (line.Equals("MODEM:STARTUP", StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(ACTION.ModemStartup);
                return;
            }

            if (line.Equals("+PBREADY", StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(ACTION.ModemReady);
                return;
            }

            if (Regex.IsMatch(line, @"\AMS_SMS\d{4}\.\d{2}\.\d{2}\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.SoftwareVersion);
                return;
            }

            if (Regex.IsMatch(line, @"\A\+CUSD:\s*\d+\s*,\s*"".*""\s*,\s*\d+\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.UssdResultReceived);
                return;
            }

            if (Regex.IsMatch(line, @"\A\+CCLK:\s*""[^""]+""\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.ClockReadResult);
                return;
            }

            if (Regex.IsMatch(line, @"\A\+CSQ:\s*\d+\s*,\s*\d+\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.SignalStrengthResult);
                return;
            }

            if (Regex.IsMatch(line, @"\A\+CMGS:\s*\d+\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.SmsSendSucceeded);
                _smsSendResultSeen = true;
                return;
            }
            if (Regex.IsMatch(line, @"\A\+SMSFLAG:\s*\x22[^\x22]+\x22\s*,\s*\d+\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.NewSmsReceived);
                actions.Add(ACTION.SmsNotification);
                return;
            }

            if (Regex.IsMatch(line, @"\A\+CPMS:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.SmsStorageStatus);
                return;
            }

            if (Regex.IsMatch(line, @"\A\+CMGR:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                _waitingForCmgrPdu = true;
                return;
            }

            if (Regex.IsMatch(line, @"\A\+CPIN:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                ClassifySimStatus(line, actions);
                return;
            }

            if (Regex.IsMatch(line, @"\A\+(?:C|CG|CE)REG:", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                ClassifyNetworkStatus(line, actions);
                return;
            }

            if (line.Equals("NO CARRIER", StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(ACTION.NoCarrier);
                return;
            }

            if (line.Equals("BUSY", StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(ACTION.Busy);
                return;
            }

            if (line.Equals("NO ANSWER", StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(ACTION.NoAnswer);
                return;
            }

            if (Regex.IsMatch(line, @"\ANO\s+DIAL\s*TONE\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.NoDialTone);
                return;
            }

            if (Regex.IsMatch(line, @"\ACONNECT(?:\s+\d+)?\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.Connect);
            }
        }

        private static bool IsCompleteWithoutLineEnding(string segment)
        {
            var line = Regex.Replace(Regex.Replace(segment, @"[\x00-\x08\x0B-\x1F\x7F]", string.Empty), @"\s+", " ").Trim();
            if (line.Length == 0)
            {
                return false;
            }

            if (line.Equals("OK", StringComparison.OrdinalIgnoreCase) ||
                line.Equals("ERROR", StringComparison.OrdinalIgnoreCase) ||
                line.Equals("BUSY", StringComparison.OrdinalIgnoreCase) ||
                line.Equals("CONNECT", StringComparison.OrdinalIgnoreCase) ||
                line.Equals("NO CARRIER", StringComparison.OrdinalIgnoreCase) ||
                line.Equals("NO ANSWER", StringComparison.OrdinalIgnoreCase) ||
                line.Equals(">", StringComparison.Ordinal) ||
                line.Equals("MODEM:STARTUP", StringComparison.OrdinalIgnoreCase) ||
                line.Equals("+PBREADY", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return Regex.IsMatch(line, @"\AMS_SMS\d{4}\.\d{2}\.\d{2}\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ||
                   Regex.IsMatch(line, @"\A\+CUSD:\s*\d+\s*,\s*"".*""\s*,\s*\d+\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ||
                   Regex.IsMatch(line, @"\A\+CCLK:\s*""[^""]+""\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ||
                   Regex.IsMatch(line, @"\A\+CSQ:\s*\d+\s*,\s*\d+\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ||
                   Regex.IsMatch(line, @"\A\+CMGS:\s*\d+\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ||
                   Regex.IsMatch(line, @"\A\+SMSFLAG:\s*\S+\s*,\s*\d+\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ||
                   Regex.IsMatch(line, @"\A\+CPIN:\s*.+\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ||
                   Regex.IsMatch(line, @"\A\+(?:C|CG|CE)REG:\s*\d+(?:\s*,\s*\d+)?(?:\s*,.*)?\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private void CompletePendingCommand(List<ACTION> actions)
        {
            if (string.IsNullOrEmpty(_pendingCommand))
            {
                return;
            }

            if (Regex.IsMatch(_pendingCommand, @"\AAT\+CMGS\s*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                if (_smsSendResultSeen)
                {
                    ClearPendingCommand();
                }
                return;
            }

            if (_pendingCommand.Equals("AT", StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(ACTION.ConnectionCheckSucceeded);
            }
            else if (_pendingCommand.Equals("ATE1", StringComparison.OrdinalIgnoreCase))
            {
                actions.Add(ACTION.EchoModeConfigured);
            }
            else if (Regex.IsMatch(_pendingCommand, @"\AAT\+CSCS\s*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.CharacterSetConfigured);
            }
            else if (Regex.IsMatch(_pendingCommand, @"\AAT\+CMGF\s*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.SmsFormatConfigured);
            }
            else if (Regex.IsMatch(_pendingCommand, @"\AAT\+CCLK\s*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.ClockSetAccepted);
            }
            else if (Regex.IsMatch(_pendingCommand, @"\AAT\+CUSD\s*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                actions.Add(ACTION.UssdCommandAccepted);
            }
            else if (Regex.IsMatch(_pendingCommand, @"\AAT\+CMGD\s*=", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                ClassifyDeleteCommand(_pendingCommand, actions);
            }

            _pendingCommand = string.Empty;
        }

        private static void ClassifyDeleteCommand(string command, List<ACTION> actions)
        {
            var match = Regex.Match(command, @"\AAT\+CMGD\s*=\s*\d+\s*(?:,\s*(?<mode>\d+))?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!match.Success || !match.Groups["mode"].Success || match.Groups["mode"].Value == "0")
            {
                actions.Add(ACTION.SmsDeleteOneAccepted);
                return;
            }

            switch (match.Groups["mode"].Value)
            {
                case "1": actions.Add(ACTION.SmsDeleteReadAccepted); break;
                case "2": actions.Add(ACTION.SmsDeleteReadAndSentAccepted); break;
                case "3": actions.Add(ACTION.SmsDeleteReadSentAndUnsentAccepted); break;
                case "4": actions.Add(ACTION.SmsDeleteAllAccepted); break;
            }
        }

        private void ClassifySimStatus(string line, List<ACTION> actions)
        {
            var match = Regex.Match(line, @"\A\+CPIN:\s*(?<status>.+)\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!match.Success)
            {
                return;
            }

            switch (match.Groups["status"].Value.Trim().Trim('"').ToUpperInvariant())
            {
                case "READY": actions.Add(ACTION.SimReady); break;
                case "SIM PIN": actions.Add(ACTION.SimPinRequired); break;
                case "SIM PIN2": actions.Add(ACTION.SimPin2Required); break;
                case "SIM PUK": actions.Add(ACTION.SimPukRequired); break;
                case "SIM PUK2": actions.Add(ACTION.SimPuk2Required); break;
                case "NOT INSERTED": actions.Add(ACTION.SimNotInserted); break;
                default: actions.Add(ACTION.SimStatusUnknown); break;
            }
        }

        private static void ClassifyNetworkStatus(string line, List<ACTION> actions)
        {
            var match = Regex.Match(line, @"\A\+(?:C|CG|CE)REG:\s*(?:\d+\s*,\s*)?(?<status>\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            int status;
            if (!match.Success || !int.TryParse(match.Groups["status"].Value, out status))
            {
                return;
            }

            switch (status)
            {
                case 0: actions.Add(ACTION.NetworkNotRegistered); break;
                case 1:
                case 6:
                case 9: actions.Add(ACTION.NetworkRegisteredHome); break;
                case 2: actions.Add(ACTION.NetworkSearching); break;
                case 3: actions.Add(ACTION.NetworkRegistrationDenied); break;
                case 5:
                case 7:
                case 10: actions.Add(ACTION.NetworkRegisteredRoaming); break;
                default: actions.Add(ACTION.NetworkRegistrationUnknown); break;
            }
        }

        private static bool TryClassifyPdu(string pdu, out ACTION action)
        {
            action = ACTION.SmsReadMessage;
            int serviceCenterLength;
            if (pdu.Length < 4 || !int.TryParse(pdu.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out serviceCenterLength))
            {
                return false;
            }

            var tpduOffset = (serviceCenterLength + 1) * 2;
            int firstTpduOctet;
            if (pdu.Length < tpduOffset + 2 || !int.TryParse(pdu.Substring(tpduOffset, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out firstTpduOctet))
            {
                return false;
            }

            if ((firstTpduOctet & 0x03) == 0x02)
            {
                action = ACTION.SmsReadDeliveryReport;
            }
            else if ((firstTpduOctet & 0x40) != 0)
            {
                action = ACTION.SmsReadMultipartPart;
            }

            return true;
        }

        private void ClearPendingCommand()
        {
            _pendingCommand = string.Empty;
            _smsSendInProgress = false;
            _smsSendResultSeen = false;
        }
    }
        public static bool ParseSmsSendResult(string smsSendResult, ref List<string> smsReference, ref ERROR_CODE ErrorCode, ref string ErrorMessage)
        {
            smsReference = null;
            // Rule 2: null input
            if (smsSendResult == null || smsSendResult.Length <= 0)
            {
                ErrorCode = ERROR_CODE.NO_ERROR;
                ErrorMessage = string.Empty;
                return false;
            }
            try
            {
                smsReference = new List<string>();
                // Pattern: R{1-10}:{1-2 digits}--
                var regex = new System.Text.RegularExpressions.Regex(@"R(\d+):(\d{1,2})--");
                var matches = regex.Matches(smsSendResult);
                // Rule 4: No matches found
                if (matches.Count == 0)
                {
                    ErrorCode = ERROR_CODE.SEND_SMS_RESULT;
                    ErrorMessage = smsSendResult;
                    return false;
                }
                // Extract and validate each reference
                foreach (Match match in matches)
                {
                    int rNumber = int.Parse(match.Groups[1].Value);
                    int reference = int.Parse(match.Groups[2].Value);
                    // Rule 4: Validate R number is in range 1-10
                    if (rNumber < 1 || rNumber > 10)
                    {
                        ErrorCode = ERROR_CODE.SEND_SMS_RESULT;
                        ErrorMessage = smsSendResult;
                        return false;
                    }
                    smsReference.Add(reference.ToString());
                }
                // Rule 3: Valid format
                ErrorCode = ERROR_CODE.NO_ERROR;
                ErrorMessage = string.Empty;
                return true;
            }
            catch
            {
                // Rule 4: Parse error
                ErrorCode = ERROR_CODE.SEND_SMS_RESULT;
                ErrorMessage = smsSendResult;
                return false;
            }
        }

        public static bool TryParseCusdBalance(string response, out string balance, out string providerDomain)
        {
            balance = string.Empty;
            providerDomain = string.Empty;

            if (string.IsNullOrWhiteSpace(response))
            {
                return false;
            }

            var cusdMatches = Regex.Matches(response, @"\+CUSD:\s*0\s*,\s*""(?<payload>[^""]*)""", RegexOptions.CultureInvariant);
            foreach (Match cusdMatch in cusdMatches)
            {
                var payload = cusdMatch.Groups["payload"].Value;
                var balanceMatch = Regex.Match(payload, @"(?<balance>[0-9]{3,})\?+", RegexOptions.CultureInvariant);
                if (!balanceMatch.Success)
                {
                    continue;
                }

                balance = balanceMatch.Groups["balance"].Value;
                var domainMatch = Regex.Match(payload, @"(?<domain>SHOP\.MCI\.IR)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (domainMatch.Success)
                {
                    providerDomain = domainMatch.Groups["domain"].Value;
                }

                return true;
            }

            return false;
        }

        public static bool TryParseCusdHexBalance(string response, out string balance, out string translatedText)
        {
            balance = string.Empty;
            translatedText = string.Empty;

            if (string.IsNullOrWhiteSpace(response))
            {
                return false;
            }

            var cusdMatches = Regex.Matches(response, @"\+CUSD:\s*2\s*,\s*""(?<payload>[^""]*)""", RegexOptions.CultureInvariant);
            foreach (Match cusdMatch in cusdMatches)
            {
                var hexPayload = Regex.Replace(cusdMatch.Groups["payload"].Value, @"\s+", string.Empty);
                if (hexPayload.Length == 0 || hexPayload.Length % 4 != 0 || !Regex.IsMatch(hexPayload, @"\A[0-9A-Fa-f]+\z", RegexOptions.CultureInvariant))
                {
                    continue;
                }

                var decoded = new StringBuilder(hexPayload.Length / 4);
                for (var index = 0; index < hexPayload.Length; index += 4)
                {
                    var codeUnit = int.Parse(hexPayload.Substring(index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    decoded.Append((char)codeUnit);
                }

                var text = decoded.ToString();
                var balanceMatch = Regex.Match(text, @"(?<balance>[0-9]+)\s*ریال", RegexOptions.CultureInvariant);
                if (!balanceMatch.Success)
                {
                    continue;
                }

                balance = balanceMatch.Groups["balance"].Value;
                translatedText = text;
                return true;
            }

            return false;
        }
    }
}
