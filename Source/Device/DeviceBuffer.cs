using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        /// //////////////////Test
        public List<string> BufferList
        {
            get => this._runtimeBuffer.BufferDataRaw;
        }
        /// //////////////////Test

        //Buffer Clear
        private FUNC_RESULT BufferClearWorkflow(bool InternalValueSet)
        {
            bool tempBool = false;
            int tempInt = DeviceDefinitions.NUMBER_INVALID;
            string tempStr = DeviceDefinitions.STRING_INVALID;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            result = this.BufferClearExecute((int)OPERATION_DELAY_MS.BUFFER_UPDATE);
            this.XXXSetValue(result, DeviceDefinitions.NUMBER_INVALID, DeviceDefinitions.NUMBER_INVALID, ref tempInt, ref this._runtimeBuffer.BufferLastIndex, InternalValueSet);
            this.XXXSetValue(result, false, false, ref tempBool, ref this._runtimeBuffer.BufferNewData, InternalValueSet);
            this.XXXSetValue(result, DeviceDefinitions.STRING_INVALID, DeviceDefinitions.STRING_INVALID, ref tempStr, ref this._runtimeBuffer.BufferMessage, InternalValueSet);
            this.XXXSetValue(result, DeviceDefinitions.NUMBER_INVALID, DeviceDefinitions.NUMBER_INVALID, ref tempInt, ref this._runtimeBuffer.BufferMessageLen, InternalValueSet);
            return result;
        }
        private FUNC_RESULT BufferClearExecute(int Delay)
        {
            Thread.Sleep(Delay);
            return this.BufferClearBase();
        }
        private FUNC_RESULT BufferClearBase()
        {
            return this.BufferClearBaseDll();
        }
        private FUNC_RESULT BufferClearBaseDll()
        {
            this._dllWrapper.ClearBuffer();
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : Buffer Clear

        //Buffer new Index read
        private FUNC_RESULT BufferLastIndexGetWorkflow(ref int BufferLastIndex, bool InternalValueSet)
        {
            bool tempBool = false;
            int bufferLastIndex = DeviceDefinitions.NUMBER_INVALID;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            result = this.LastBufferIndexGetExecute((int)OPERATION_DELAY_MS.BUFFER_UPDATE, ref bufferLastIndex);
            result = this.LastBufferIndexGetCheck(result, this._runtimeBuffer.BufferLastIndex, bufferLastIndex);
            this.XXXSetValue(result, bufferLastIndex, DeviceDefinitions.NUMBER_INVALID, ref BufferLastIndex, ref this._runtimeBuffer.BufferLastIndex, InternalValueSet);
            this.XXXSetValue(result, true, false, ref tempBool, ref this._runtimeBuffer.BufferNewData, InternalValueSet);
            return result;
        }
        private FUNC_RESULT LastBufferIndexGetExecute(int Delay, ref int BufferLastIndex)
        {
            Thread.Sleep(Delay);
            return this.BufferLastIndexGetBase(ref BufferLastIndex);
        }
        private FUNC_RESULT LastBufferIndexGetCheck(FUNC_RESULT Result, int BufferLastIndexOld, int BufferLastIndexNew)
        {
            if (Result == FUNC_RESULT.SUCCESS && BufferLastIndexOld != BufferLastIndexNew && BufferLastIndexNew != DeviceDefinitions.NUMBER_INVALID)
                return FUNC_RESULT.SUCCESS;
            return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT BufferLastIndexGetBase(ref int BufferLastIndex)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            int bufferLastIndex = DeviceDefinitions.NUMBER_INVALID;
            result = this.BufferLastIndexGetBaseDll(ref bufferLastIndex);
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.BufferLastIndexGetBaseCheck(bufferLastIndex);
            }
            this.XXXBaseSetValue(result, bufferLastIndex, DeviceDefinitions.NUMBER_INVALID, ref BufferLastIndex);
            return result;
        }
        private FUNC_RESULT BufferLastIndexGetBaseDll(ref int BufferLastIndex)
        {
            BufferLastIndex = (int)this._dllWrapper.BufferLastIndexGet();
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        private FUNC_RESULT BufferLastIndexGetBaseCheck(int BufferLastIndex)
        {
            if (BufferLastIndex > 0)
                return FUNC_RESULT.SUCCESS;
            else
                return FUNC_RESULT.FAIL;
        }
        //End : Buffer new Index read

        //Buffer new Index read
        private FUNC_RESULT BufferMessageGetWorkflow(int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen, bool InternalValueSet)
        {
            int bufferMessageLen = DeviceDefinitions.NUMBER_INVALID;
            string bufferMessage = DeviceDefinitions.STRING_INVALID;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            result = this.BufferMessageGetExecute((int)OPERATION_DELAY_MS.BUFFER_UPDATE, BufferLastIndex, ref bufferMessage, ref bufferMessageLen);
            this.XXXSetValue(result, bufferMessage, DeviceDefinitions.STRING_INVALID, ref BufferMessage, ref this._runtimeBuffer.BufferMessage, InternalValueSet);
            this.XXXSetValue(result, bufferMessageLen, DeviceDefinitions.NUMBER_INVALID, ref BufferMessageLen, ref this._runtimeBuffer.BufferMessageLen, InternalValueSet);
            return result;
        }
        private FUNC_RESULT BufferMessageGetExecute(int Delay, int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen)
        {
            Thread.Sleep(Delay);
            return this.BufferMessageGetBase(BufferLastIndex, ref BufferMessage, ref BufferMessageLen);
        }
        private FUNC_RESULT BufferMessageGetBase(int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen)
        {
            int bufferMessageLen = DeviceDefinitions.NUMBER_INVALID;
            string bufferMessage = DeviceDefinitions.STRING_INVALID;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            result = this.BufferMessageGetBaseDll(BufferLastIndex, ref bufferMessage, ref bufferMessageLen);
            this.XXXBaseSetValue(result, bufferMessage, DeviceDefinitions.STRING_INVALID, ref BufferMessage);
            this.XXXBaseSetValue(result, bufferMessageLen, DeviceDefinitions.NUMBER_INVALID, ref BufferMessageLen);
            return result;
        }
        private FUNC_RESULT BufferMessageGetBaseDll(int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen)
        {
            this._dllWrapper.BufferRead(ref BufferLastIndex, out BufferMessage, ref BufferMessageLen);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : Buffer new Index read

        //Buffer Update
        //Buffer Update Main
        private FUNC_RESULT BufferUpdate(BUFFER_UPDATE_SOURCE BufferUpdateSource, string WorkFlowName, bool Analyze)
        {
            //Variable Initial
            DateTime priorBufferLastUpdate = DateTime.MinValue;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            if (Analyze)
                priorBufferLastUpdate = this._runtimeBuffer.BufferLastUpdate;
            this._runtimeBuffer.BufferLastUpdate = DateTime.Now;
            //End : Variable Initial

            //Layer 1
            result = this.BufferUpdateList();
            //End : Layer 1

            //Layer 2 -> if Analyze allow
            if (result == FUNC_RESULT.SUCCESS && Analyze)
            {
                result = this.BufferUpdateAnalyze(BufferUpdateSource, WorkFlowName);
            }
            //End : Layer 2 -> if Analyze allow

            return result;
        }
        //End : Buffer Update Main
        //Buffer Update List
        private FUNC_RESULT BufferUpdateList()
        {
            //Variable Initial
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            //End : Variable Initial

            //Buffer Update Raw List -> Read ALL Buffer, ADD to BufferDataRaw(List), Clear Buffer. if any error happen Clear Buffer happen and Buffer Remove
            result = this.BufferUpdateRawList();
            //End : Buffer Update Raw List

            //End : Buffer Update Data List
            if (result == FUNC_RESULT.SUCCESS)
                result = this.BufferUpdateDataList();
            //Buffer Update Data List

            return result;
        }
        //Buffer Update RAW List
        private FUNC_RESULT BufferUpdateRawList()
        {
            //Variable Initial
            bool tempBool = false;
            int bufferLastIndex = DeviceDefinitions.NUMBER_INVALID, tempInt = DeviceDefinitions.NUMBER_INVALID;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            //End : Variable Initial

            //get Buffer data and check data is available
            result = this.LastBufferIndexGetExecute((int)OPERATION_DELAY_MS.BUFFER_UPDATE, ref bufferLastIndex);
            result = this.BufferUpdateListCheck(result, bufferLastIndex);
            //End : get Buffer data and check data is available

            //ADD Buffer RAW List
            result = this.BufferUpdateListSetData(result, bufferLastIndex);
            //End : ADD Buffer RAW List

            //Buffer Clear
            result = this.BufferClearExecute((int)OPERATION_DELAY_MS.BUFFER_UPDATE);
            this.XXXSetValue(result, false, false, ref tempBool, ref this._runtimeBuffer.BufferNewData, true);
            this.XXXSetValue(result, DeviceDefinitions.NUMBER_INVALID, DeviceDefinitions.NUMBER_INVALID, ref tempInt, ref this._runtimeBuffer.BufferLastIndex, true);
            //End : Buffer Clear

            return result;
        }
        private FUNC_RESULT BufferUpdateListCheck(FUNC_RESULT Result, int BufferLastIndexNew)
        {
            if (Result == FUNC_RESULT.SUCCESS && BufferLastIndexNew > DeviceDefinitions.NUMBER_INVALID)
                return FUNC_RESULT.SUCCESS;
            return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT BufferUpdateListSetData(FUNC_RESULT Result, int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS)
                return FUNC_RESULT.FAIL;
            int countTry = 3;
            int bufferMessageLen = DeviceDefinitions.NUMBER_INVALID;
            string bufferMessage = DeviceDefinitions.STRING_INVALID;
            for (int i = 1; i <= BufferLastIndex; i++)
            {
                FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
                for (int j = 0; j < countTry && result != FUNC_RESULT.SUCCESS; j++)
                    result = this.BufferMessageGetExecute((int)OPERATION_DELAY_MS.BUFFER_UPDATE, i, ref bufferMessage, ref bufferMessageLen);
                if (result != FUNC_RESULT.SUCCESS)
                    return FUNC_RESULT.MAJOR_ERROR;
                this._runtimeBuffer.BufferDataRaw.Add(bufferMessage);
            }
            return FUNC_RESULT.SUCCESS;
        }
        //End : Buffer Update RAW List
        //Buffer Update Data List
        private FUNC_RESULT BufferUpdateDataList()
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            ///////////////Temp
            result = FUNC_RESULT.SUCCESS;
            ///////////////End : Temp
            return result;
        }
        //End : Buffer Update Data List
        //End : Buffer Update List
        //Buffer Update Analyze
        private FUNC_RESULT BufferUpdateAnalyze(BUFFER_UPDATE_SOURCE BufferUpdateSource, string WorkFlowName)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            ///////temp
            result = FUNC_RESULT.SUCCESS;
            ///////End : temp
            return result;
        }
        //End : Buffer Update Analyze
        //End : Buffer Update

        // ============================================================
        // Buffer Layer 1 - Pattern Definitions
        // ============================================================
        private static readonly Regex _bufferCmgrCommandRegex = new Regex(@"^[ \t]*AT\+CMGR[ \t]*=[ \t]*(?<index>\d+)[ \t]*",
                                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex _bufferCmgrResponseRegex = new Regex(@"^[ \t]*\+CMGR[ \t]*:[ \t]*(?<header>[^\r\n]*)",
                                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex _bufferOkRegex = new Regex(@"^[ \t]*OK[ \t]*$",
                                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex _bufferErrorRegex = new Regex(@"^[ \t]*ERROR[ \t]*$",
                                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        // پایان پاسخ CMGR می‌تواند OK یا ERROR باشد.
        private static readonly Regex _bufferCmgrResponseEndRegex = new Regex(@"^[ \t]*(?:OK|ERROR)[ \t]*$",
                                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        // ============================================================
        // Buffer Pattern Definition
        // ============================================================
        private sealed class BufferPattern
        {
            public BUFFER_ACTION Action;
            public Regex StartRegex;
            public Regex EndRegex;
            public BufferPattern(
                BUFFER_ACTION Action,
                Regex StartRegex,
                Regex EndRegex = null)
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
            new BufferPattern(BUFFER_ACTION.INBOX_READ_COMMAND, _bufferCmgrCommandRegex),
            new BufferPattern(BUFFER_ACTION.INBOX_READ_RESPONSE, _bufferCmgrResponseRegex, _bufferCmgrResponseEndRegex),
            new BufferPattern(BUFFER_ACTION.OK, _bufferOkRegex),
            new BufferPattern(BUFFER_ACTION.ERROR, _bufferErrorRegex)
        };
        // ============================================================
        // Buffer Layer 1 - Main Analyzer
        // ============================================================

        private BufferDataGroup BufferAnalyzeRawMessage(string SourceMessage)
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
                if (!found)
                {
                    BufferAddData(bufferDataGroup, SourceMessage.Substring(position), BUFFER_ACTION.UNKNOWN);
                    break;
                }

                // متن قبل از الگوی بعدی، یک بخش مستقل است.
                if (startMatch.Index > position)
                {
                    string unknownMessage = SourceMessage.Substring(position, startMatch.Index - position);
                    BufferAddData(bufferDataGroup, unknownMessage, BUFFER_ACTION.UNKNOWN);
                }

                int segmentStart = startMatch.Index;
                int segmentEnd;

                if (pattern.EndRegex != null)
                {
                    // این الگو پایان مشخص دارد؛ مانند پاسخ CMGR
                    // که تا OK یا ERROR ادامه پیدا می‌کند.
                    Match endMatch = pattern.EndRegex.Match(SourceMessage, startMatch.Index + startMatch.Length);
                    if (endMatch.Success)
                        // نشانگر پایانی نیز جزئی از همین رکورد است.
                        segmentEnd = endMatch.Index + endMatch.Length;
                    else
                    {
                        // اگر پایان اختصاصی پیدا نشد، بخش بعدی
                        // با الگوی شناخته‌شده بعدی مرزبندی می‌شود.
                        bool nextFound = BufferFindNextPattern(SourceMessage, startMatch.Index + startMatch.Length, out nextPattern, out nextMatch);
                        segmentEnd = nextFound ? nextMatch.Index : SourceMessage.Length;
                    }
                }
                else
                {
                    // الگوی بدون پایان اختصاصی تا شروع الگوی
                    // شناخته‌شده بعدی ادامه پیدا می‌کند.
                    bool nextFound = BufferFindNextPattern(SourceMessage, startMatch.Index + startMatch.Length, out nextPattern, out nextMatch);
                    segmentEnd = nextFound ? nextMatch.Index : SourceMessage.Length;
                }

                // محافظت در برابر حلقه بی‌نهایت
                if (segmentEnd <= segmentStart)
                {
                    BufferAddData(bufferDataGroup, SourceMessage.Substring(segmentStart, 1), BUFFER_ACTION.UNKNOWN);
                    position = segmentStart + 1;
                    continue;
                }

                string message = SourceMessage.Substring(segmentStart, segmentEnd - segmentStart);
                BufferAddData(bufferDataGroup, message, pattern.Action);
                position = segmentEnd;
            }

            return bufferDataGroup;
        }
        // ============================================================
        // Find the next known pattern
        // ============================================================
        private bool BufferFindNextPattern(string SourceMessage, int SearchFrom, out BufferPattern FoundPattern, out Match FoundMatch)
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
        private void BufferAddData(BufferDataGroup bufferDataGroup, string Message, BUFFER_ACTION Action)
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
        private void BufferExtractStatus(BufferData data)
        {
            if (_bufferOkRegex.IsMatch(data.Message))
                data.Status = BUFFER_ACTION_STATUS.SUCCEEDED;
            else if (_bufferErrorRegex.IsMatch(data.Message))
                data.Status = BUFFER_ACTION_STATUS.FAIL;
            else
                data.Status = BUFFER_ACTION_STATUS.UNKNOWN;
        }
        // ============================================================
        // Extract fields according to the recognized pattern
        // ============================================================
        private void BufferExtractFields(BufferData data)
        {
            switch (data.Action)
            {
                case BUFFER_ACTION.INBOX_READ_COMMAND:
                    {
                        Match match = _bufferCmgrCommandRegex.Match(data.Message);
                        if (match.Success)
                            data.MessageAnalyzed = match.Groups["index"].Value;
                        break;
                    }

                case BUFFER_ACTION.INBOX_READ_RESPONSE:
                    {
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
