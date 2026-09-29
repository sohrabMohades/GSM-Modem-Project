using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GSM.Core.Source.StringAnalyzer
{
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
    }
}
