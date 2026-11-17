using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        //Buffer Clear
        private FUNC_RESULT BufferClearWorkflow(bool ApplyOperationDelay, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferClearWorkflow), this.FuncType(nameof(this.BufferClearWorkflow)));
                result = this.BufferClearExecute(result, ApplyOperationDelay ? (int)OPERATION_DELAY_MS.BUFFER_UPDATE : (int)OPERATION_DELAY_MS.NO_DELAY);
                this.XXXSetValue(DeviceDefinitions.NUMBER_INVALID, ref this._runtimeBuffer.BufferLastIndex, InternalValueSet);
                this.XXXSetValue(false, ref this._runtimeBuffer.BufferNewData, InternalValueSet);
                this.XXXSetValue(DeviceDefinitions.STRING_INVALID, ref this._runtimeBuffer.BufferMessage, InternalValueSet);
                this.XXXSetValue(DeviceDefinitions.NUMBER_INVALID, ref this._runtimeBuffer.BufferMessageLen, InternalValueSet);
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        private FUNC_RESULT BufferClearExecute(FUNC_RESULT Result, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferClearExecute), this.FuncType(nameof(this.BufferClearExecute)));
                Thread.Sleep(Delay);
                result = this.BufferClearBase();
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        private FUNC_RESULT BufferClearBase()
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferClearBase), this.FuncType(nameof(this.BufferClearBase)));
                result = this.BufferClearBaseDll(result);
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        private FUNC_RESULT BufferClearBaseDll(FUNC_RESULT Result)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferClearBaseDll), this.FuncType(nameof(this.BufferClearBaseDll)));
                this._dllWrapper.ClearBuffer();
                result = this.DllWrapperHasError(result);
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        //End : Buffer Clear

        //Buffer new Index read
        private FUNC_RESULT BufferLastIndexGetWorkflow(ref int LastIndex, bool LastIndexNotEqual, bool ApplyOperationDelay, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferLastIndexGetWorkflow), this.FuncType(nameof(this.BufferLastIndexGetWorkflow)), this._runtimeBuffer.BufferLastIndex);
                bool tempBool = false;
                DateTime tempDateTime = DateTime.MinValue;
                result = this.LastBufferIndexGetExecute(result, ApplyOperationDelay ? (int)OPERATION_DELAY_MS.BUFFER_UPDATE : (int)OPERATION_DELAY_MS.NO_DELAY, ref LastIndex);
                result = this.LastBufferIndexGetCheck(result, this._runtimeBuffer.BufferLastIndex, LastIndex, LastIndexNotEqual);
                this.XXXSetValue(result, LastIndex, DeviceDefinitions.NUMBER_INVALID, ref LastIndex, ref this._runtimeBuffer.BufferLastIndex, InternalValueSet);
                this.XXXSetValue(result, DateTime.Now, this._runtimeBuffer.BufferLastCheck, ref tempDateTime, ref this._runtimeBuffer.BufferLastCheck, InternalValueSet);
                this.XXXSetValue(result, true, false, ref tempBool, ref this._runtimeBuffer.BufferNewData, InternalValueSet);
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        private FUNC_RESULT LastBufferIndexGetExecute(FUNC_RESULT Result, int Delay, ref int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.LastBufferIndexGetExecute), this.FuncType(nameof(this.LastBufferIndexGetExecute)));
                Thread.Sleep(Delay);
                result = this.BufferLastIndexGetBase(ref BufferLastIndex);
                return result;
            }
            finally { this._logger.FuncEnd(result, BufferLastIndex); }
        }
        private FUNC_RESULT LastBufferIndexGetCheck(FUNC_RESULT Result, int BufferLastIndexOld, int BufferLastIndexNew, bool LastIndexNotEqual)
        {
            if (Result == FUNC_RESULT.SUCCESS && (!LastIndexNotEqual || BufferLastIndexOld != BufferLastIndexNew) && BufferLastIndexNew != DeviceDefinitions.NUMBER_INVALID) return FUNC_RESULT.SUCCESS;
            else if (Result == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            else return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT BufferLastIndexGetBase(ref int BufferLastIndex)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            int bufferLastIndex = DeviceDefinitions.NUMBER_INVALID;
            try
            {
                this._logger.FuncStart(nameof(this.BufferLastIndexGetBase), this.FuncType(nameof(this.BufferLastIndexGetBase)));
                result = this.BufferLastIndexGetBaseDll(result, ref bufferLastIndex);
                result = this.BufferLastIndexGetBaseCheck(result, ref bufferLastIndex);
                this.XXXBaseSetValue(result, bufferLastIndex, DeviceDefinitions.NUMBER_INVALID, ref BufferLastIndex);
                return result;
            }
            finally { this._logger.FuncEnd(result, bufferLastIndex); }
        }
        private FUNC_RESULT BufferLastIndexGetBaseDll(FUNC_RESULT Result, ref int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferLastIndexGetBaseDll), this.FuncType(nameof(this.BufferLastIndexGetBaseDll)));
                BufferLastIndex = (int)this._dllWrapper.BufferLastIndexGet();
                result = this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
                return result;
            }
            finally { this._logger.FuncEnd(result, BufferLastIndex); }
        }
        private FUNC_RESULT BufferLastIndexGetBaseCheck(FUNC_RESULT Result, ref int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (BufferLastIndex > DeviceDefinitions.BUFFER_LAST_INDEX_MAX) BufferLastIndex = DeviceDefinitions.BUFFER_LAST_INDEX_MAX; 
            if (BufferLastIndex > DeviceDefinitions.NUMBER_INVALID) return FUNC_RESULT.SUCCESS;
            else return FUNC_RESULT.FAIL;
        }
        //End : Buffer new Index read

        //Buffer new Index read
        private FUNC_RESULT BufferMessageGetWorkflow(int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen, bool ApplyOperationDelay, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferMessageGetWorkflow), this.FuncType(nameof(this.BufferMessageGetWorkflow)), BufferLastIndex);
                DateTime tempDateTime = DateTime.MinValue;
                result = this.BufferMessageGetExecute(result, ApplyOperationDelay ? (int)OPERATION_DELAY_MS.BUFFER_UPDATE : (int)OPERATION_DELAY_MS.NO_DELAY, BufferLastIndex, ref BufferMessage, ref BufferMessageLen);
                this.XXXSetValue(result, BufferMessage, DeviceDefinitions.STRING_INVALID, ref BufferMessage, ref this._runtimeBuffer.BufferMessage, InternalValueSet);
                this.XXXSetValue(result, BufferMessageLen, DeviceDefinitions.NUMBER_INVALID, ref BufferMessageLen, ref this._runtimeBuffer.BufferMessageLen, InternalValueSet);
                this.XXXSetValue(result, DateTime.Now, this._runtimeBuffer.BufferLastUpdate , ref tempDateTime, ref this._runtimeBuffer.BufferLastUpdate, InternalValueSet);
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        private FUNC_RESULT BufferMessageGetExecute(FUNC_RESULT Result, int Delay, int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferMessageGetExecute), this.FuncType(nameof(this.BufferMessageGetExecute)), BufferLastIndex);
                Thread.Sleep(Delay);
                result = this.BufferMessageGetBase(BufferLastIndex, ref BufferMessage, ref BufferMessageLen);
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        private FUNC_RESULT BufferMessageGetBase(int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferMessageGetBase), this.FuncType(nameof(this.BufferMessageGetBase)), BufferLastIndex);
                result = this.BufferMessageGetBaseCheck(result, BufferLastIndex);
                result = this.BufferMessageGetBaseDll(result, BufferLastIndex, ref BufferMessage, ref BufferMessageLen);
                this.XXXBaseSetValue(result, BufferMessage, DeviceDefinitions.STRING_INVALID, ref BufferMessage);
                this.XXXBaseSetValue(result, BufferMessageLen, DeviceDefinitions.NUMBER_INVALID, ref BufferMessageLen);
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        private FUNC_RESULT BufferMessageGetBaseCheck(FUNC_RESULT Result, int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            else if (BufferLastIndex < DeviceDefinitions.BUFFER_LAST_INDEX_MIN || BufferLastIndex > DeviceDefinitions.BUFFER_LAST_INDEX_MAX) return FUNC_RESULT.MAJOR_ERROR;
            else return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT BufferMessageGetBaseDll(FUNC_RESULT Result, int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferMessageGetBaseDll), this.FuncType(nameof(this.BufferMessageGetBaseDll)), BufferLastIndex);
                this._dllWrapper.BufferRead(ref BufferLastIndex, out BufferMessage, ref BufferMessageLen);
                result = this.DllWrapperHasError(result);
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        //End : Buffer new Index read

        //Buffer Update
        private FUNC_RESULT BufferUpdate(bool ApplyOperationDelay, bool ClearBufferExtra)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferUpdate), this.FuncType(nameof(this.BufferUpdate)));
                this.BufferDataRemoveExtra(result, ClearBufferExtra);
                result = this.BufferUpdateRawList(result, ApplyOperationDelay);
                result = this.BufferUpdateDataList(result);
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        private void BufferDataRemoveExtra(FUNC_RESULT Result, bool ClearBufferExtra)
        {
            if (Result != FUNC_RESULT.SUCCESS) return;
            if (!ClearBufferExtra) return;
            BufferDataGroup group;
            BufferData bufferData;
            List<BufferData> removedBufferData = new List<BufferData>();
            int removedNullCount = 0;
            try
            {
                this._logger.FuncStart(nameof(this.BufferDataRemoveExtra), this.FuncType(nameof(this.BufferDataRemoveExtra)));
                bool IsPreservedAction(BUFFER_ACTION action)
                {
                    switch (action)
                    {
                        case BUFFER_ACTION.MODEM_STARTUP: case BUFFER_ACTION.MODEM_READY: case BUFFER_ACTION.SMS_RECEIVED: case BUFFER_ACTION.SMS_DELIVERY_REPORT: return true;
                        default: return false;
                    }
                }
                for (int groupIndex = this._runtimeBuffer.BufferDataGroup.Count - 1; groupIndex >= 0; groupIndex--)
                {
                    group = this._runtimeBuffer.BufferDataGroup[groupIndex];
                    for (int dataIndex = group.BufferData.Count - 1; dataIndex >= 0; dataIndex--)
                    {
                        bufferData = group.BufferData[dataIndex];
                        if (bufferData == null) { removedNullCount++; group.BufferData.RemoveAt(dataIndex); }
                        else if (!IsPreservedAction(bufferData.Action)) { removedBufferData.Add(bufferData); group.BufferData.RemoveAt(dataIndex); }
                    }
                    if (group.BufferData.Count == 0) this._runtimeBuffer.BufferDataGroup.RemoveAt(groupIndex);
                }
            }
            finally { this._logger.FuncEnd(FUNC_RESULT.UNKNOWN, new { RemovedBufferData = removedBufferData, RemovedNullCount = removedNullCount }); }
        }
        private FUNC_RESULT BufferUpdateRawList(FUNC_RESULT Result, bool ApplyOperationDelay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            bool lastIndexNotEqual = false, internalValueSet = true, bufferCleareApplyDelay = false;
            int bufferLastIndex = DeviceDefinitions.NUMBER_INVALID;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferUpdateRawList), this.FuncType(nameof(this.BufferUpdateRawList)));
                result = this.BufferLastIndexGetWorkflow(ref bufferLastIndex, lastIndexNotEqual, ApplyOperationDelay, internalValueSet);
                result = this.BufferUpdateRawListFill(result, bufferLastIndex);
                FUNC_RESULT resultClear = this.BufferClearWorkflow(bufferCleareApplyDelay, internalValueSet);
                result = result == FUNC_RESULT.SUCCESS ? resultClear : result; 
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        private FUNC_RESULT BufferUpdateRawListFill(FUNC_RESULT Result, int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT resultTemp = FUNC_RESULT.UNKNOWN, result = FUNC_RESULT.FAIL;
            bool internalValueSet = true, applyDelay = false;
            int bufferMessageLen = DeviceDefinitions.NUMBER_INVALID, bufferIndex;
            string bufferMessage = DeviceDefinitions.STRING_INVALID;
            try
            {
                this._logger.FuncStart(nameof(this.BufferUpdateRawListFill), this.FuncType(nameof(this.BufferUpdateRawListFill)));
                for (bufferIndex = 1; bufferIndex <= BufferLastIndex && resultTemp != FUNC_RESULT.MAJOR_ERROR; bufferIndex++)
                {
                    resultTemp = FUNC_RESULT.UNKNOWN;
                    for (int j = 0; j < (int)TRY_COUNT.BUFFER_COUNT_TRY && resultTemp != FUNC_RESULT.SUCCESS && resultTemp != FUNC_RESULT.MAJOR_ERROR; j++)
                        resultTemp = this.BufferMessageGetWorkflow(bufferIndex, ref bufferMessage, ref bufferMessageLen, applyDelay, internalValueSet);
                    if (resultTemp == FUNC_RESULT.SUCCESS)
                    {
                        this._runtimeBuffer.BufferDataRaw.Add(new BufferRawData { Message = bufferMessage, MessageLen = bufferMessageLen, ReceivedTime = this._runtimeBuffer.BufferLastUpdate });
                        result = FUNC_RESULT.SUCCESS;
                    }
                    else
                        this._logger.LogWarning($"BufferMessageGetWorkflow failed for index {bufferIndex} : bufferMessage = {bufferMessage} , bufferMessageLen = {bufferMessageLen}, result = {resultTemp}");
                    if (resultTemp == FUNC_RESULT.MAJOR_ERROR) result = FUNC_RESULT.MAJOR_ERROR;
                }
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        private FUNC_RESULT BufferUpdateDataList(FUNC_RESULT Result)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.FAIL;
            try
            {
                this._logger.FuncStart(nameof(this.BufferUpdateDataList), this.FuncType(nameof(this.BufferUpdateDataList)));
                for (int i = 0; i < this._runtimeBuffer.BufferDataRaw.Count; i++)
                {
                    try { this._runtimeBuffer.BufferDataGroup.Add(StringAnalyzer.BufferAnalyzeRawMessage(this._runtimeBuffer.BufferDataRaw[i])); result = FUNC_RESULT.SUCCESS; }
                    catch { this._logger.LogWarning(this._runtimeBuffer.BufferDataRaw[i]); }
                }
                this._runtimeBuffer.BufferDataRaw.Clear();
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        //End : Buffer Update

        //Buffer Analyze
        /// <summary>
        /// Gets the valid response patterns used to identify a modem
        /// that has completed its startup and is ready for operation.
        /// </summary>
        /// <returns>
        /// A list of accepted response patterns, ordered by matching priority.
        /// </returns>
        private List<BufferPattern> GetModemStartupReadyPatterns()
        {
            return new List<BufferPattern>
            {
                // Pattern 1: The modem directly reports that it is ready.
                new BufferPattern { Mode = BUFFER_PATTERN_MODE.STRICT, Actions = new List<BUFFER_ACTION> { BUFFER_ACTION.MODEM_READY }  },
                // Pattern 2: The modem reports startup first and readiness afterward.
                new BufferPattern
                { Mode = BUFFER_PATTERN_MODE.STRICT, Actions = new List<BUFFER_ACTION> { BUFFER_ACTION.MODEM_STARTUP, BUFFER_ACTION.MODEM_READY } }
            };
        }
        /// <summary>
        /// Checks whether the specified buffer pattern exists in the runtime buffer,
        /// considering only records received at or after the analysis start time.
        /// </summary>
        /// <param name="Pattern">The buffer pattern to be matched.</param>
        /// <param name="AnalyzeStartTime">The minimum received time of eligible buffer records.</param>
        /// <returns>
        /// SUCCESS if the complete pattern is found; otherwise, FAIL.
        /// </returns>
        private FUNC_RESULT BufferAnalyzeMatchPattern(BufferPattern Pattern, DateTime AnalyzeStartTime)
        {
            if (this._runtimeBuffer.BufferDataGroup == null || Pattern == null || Pattern.Actions == null || Pattern.Actions.Count == 0) return FUNC_RESULT.MAJOR_ERROR;
            FUNC_RESULT result = FUNC_RESULT.FAIL;
            try
            {
                this._logger.FuncStart(nameof(this.BufferAnalyzeMatchPattern), this.FuncType(nameof(this.BufferAnalyzeMatchPattern)));
                int patternIndex = 0;
                foreach (BufferDataGroup group in this._runtimeBuffer.BufferDataGroup)
                {
                    if (group == null || group.BufferData == null) continue;
                    foreach (BufferData bufferData in group.BufferData)
                    {
                        if (bufferData == null || bufferData.ReceivedTime < AnalyzeStartTime) continue;
                        // The first expected action must match before pattern matching begins.
                        if (patternIndex == 0)
                        {
                            if (bufferData.Action == Pattern.Actions[0])
                            {
                                patternIndex++;
                                if (patternIndex == Pattern.Actions.Count)
                                {
                                    result = FUNC_RESULT.SUCCESS;
                                    return result;
                                }
                            }

                            continue;
                        }

                        // Apply the matching mode between consecutive expected actions.
                        switch (Pattern.Mode)
                        {
                            case BUFFER_PATTERN_MODE.STRICT:

                                if (bufferData.Action == Pattern.Actions[patternIndex])
                                {
                                    patternIndex++;
                                }
                                else
                                {
                                    patternIndex = 0;
                                }

                                break;

                            case BUFFER_PATTERN_MODE.NULL_ALLOWED:

                                if (bufferData.Action == Pattern.Actions[patternIndex])
                                {
                                    patternIndex++;
                                }
                                else if (bufferData.Action != BUFFER_ACTION.NULL)
                                {
                                    patternIndex = 0;
                                }

                                break;

                            case BUFFER_PATTERN_MODE.FLEXIBLE:

                                if (bufferData.Action == Pattern.Actions[patternIndex])
                                    patternIndex++;

                                break;

                            default:
                                return FUNC_RESULT.FAIL;
                        }

                        if (patternIndex == Pattern.Actions.Count)
                        {
                            result = FUNC_RESULT.SUCCESS;
                            return result;
                        }
                    }
                }

                return result;
            }
            finally
            {
                this._logger.FuncEnd(result);
            }
        }
        //End : Buffer Analyze

        //////////////////////////////////////////////////////////// Next Step
        //Buffer Update Analyze
        private FUNC_RESULT BufferUpdateAnalyze(BUFFER_UPDATE_TYPE BufferUpdateType, DateTime AnylizeStartTime)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            result = this.BufferUpdateAnalyzeWorkFlow(BufferUpdateType, AnylizeStartTime);
            return result;
        }
        private FUNC_RESULT BufferUpdateAnalyzeWorkFlow(BUFFER_UPDATE_TYPE BufferUpdateType, DateTime AnylizeStartTime)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            foreach (BufferData bufferData in this._runtimeBuffer.BufferDataGroup.SelectMany(group => group.BufferData).Where(data => 
                                                                                data != null && data.ReceivedTime >= AnylizeStartTime))
            {
                switch (BufferUpdateType)
                {
                    case BUFFER_UPDATE_TYPE.DEVICE_NUMBER_GET:
                    case BUFFER_UPDATE_TYPE.GSM_POWER_GET:
                    case BUFFER_UPDATE_TYPE.GSM_POWER_SET:
                        {result = bufferData.Status == BUFFER_ACTION_STATUS.SUCCEEDED ? FUNC_RESULT.SUCCESS : FUNC_RESULT.FAIL; break; }

                    case BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY:
                    case BUFFER_UPDATE_TYPE.MODEM_STARTUP:
                        { result = bufferData.Action == BUFFER_ACTION.MODEM_STARTUP ? FUNC_RESULT.SUCCESS : FUNC_RESULT.FAIL; break; }

                    case BUFFER_UPDATE_TYPE.MODEM_READY:
                        { result = bufferData.Action == BUFFER_ACTION.MODEM_READY ? FUNC_RESULT.SUCCESS : FUNC_RESULT.FAIL; break; }

                    default:
                        result = FUNC_RESULT.UNKNOWN;break;
                }
                if (result == FUNC_RESULT.SUCCESS) break;
            }
            return result;
        }
        //End : Buffer Update Analyze

        //Buffer Check
        private FUNC_RESULT BufferCheck(FUNC_RESULT Result, BUFFER_UPDATE_TYPE BufferUpdateType)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            bool applyOperationDelay = true, clearBufferExtra = false;
            DateTime timeStart = DateTime.Now;
            Stopwatch timeoutWatch = Stopwatch.StartNew();
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, resultTimeout = FUNC_RESULT.UNKNOWN;
            do
            {
                result = this.BufferUpdate(applyOperationDelay, clearBufferExtra);
                //result = this.BufferCheckRules(result, BufferUpdateType, ref bufferUpdateTypeInternal);
                resultTimeout = this.BufferCheckTimeout(result, BufferUpdateType, timeoutWatch.ElapsedMilliseconds);
            }
            while (this.BufferCheckWhileCondition(ref result, resultTimeout));
            return result;
        }
        private FUNC_RESULT BufferCheckTimeout(FUNC_RESULT Result, BUFFER_UPDATE_TYPE BufferUpdateType, long TimeoutMs)
        {
            if (Result == FUNC_RESULT.SUCCESS || Result == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.UNKNOWN;
            if (TimeoutMs > this._runtimeConfig.TimeoutMaxMs) return FUNC_RESULT.FAIL;
            switch (BufferUpdateType)
            {
                case BUFFER_UPDATE_TYPE.DEVICE_NUMBER_GET:
                case BUFFER_UPDATE_TYPE.GSM_POWER_GET:
                case BUFFER_UPDATE_TYPE.GSM_POWER_SET:
                    if (TimeoutMs <= (int)BUFFER_TIME_OUT_MS.SHORT) return FUNC_RESULT.SUCCESS;
                    else return FUNC_RESULT.FAIL;

                case BUFFER_UPDATE_TYPE.MODEM_STARTUP:
                case BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY:
                    if (TimeoutMs <= (int)BUFFER_TIME_OUT_MS.MODEM_STARTUP) return FUNC_RESULT.SUCCESS;
                    else return FUNC_RESULT.FAIL;

                case BUFFER_UPDATE_TYPE.MODEM_READY:
                    if (TimeoutMs <= (int)BUFFER_TIME_OUT_MS.MODEM_READY) return FUNC_RESULT.SUCCESS;
                    else return FUNC_RESULT.FAIL;

                default:
                    return FUNC_RESULT.UNKNOWN;
            }
        }
        private bool BufferCheckWhileCondition(ref FUNC_RESULT Result, FUNC_RESULT ResultTimeout)
        {
            if (Result == FUNC_RESULT.SUCCESS || Result == FUNC_RESULT.MAJOR_ERROR) return false;
            if (ResultTimeout == FUNC_RESULT.FAIL) { Result = FUNC_RESULT.FAIL; return false; }
            return true;
        }

        //////////////////////////////////////////////////////////// Next Step
        private FUNC_RESULT BufferCheckRules(FUNC_RESULT Result, BUFFER_UPDATE_TYPE BufferUpdateTypeExternal, ref BUFFER_UPDATE_TYPE BufferUpdateTypeInternal)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            switch (BufferUpdateTypeInternal)
            {
                case BUFFER_UPDATE_TYPE.MODEM_READY:
                    {
                        this._runtimeInfo.GsmStatus.ModemStartup = true;
                        this._runtimeInfo.GsmStatus.ModemReady = true;
                        result = FUNC_RESULT.SUCCESS;
                        break;
                    }

                case BUFFER_UPDATE_TYPE.MODEM_STARTUP:
                    {
                        if (BufferUpdateTypeExternal == BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY || BufferUpdateTypeExternal == BUFFER_UPDATE_TYPE.MODEM_READY)
                        {
                            this._runtimeInfo.GsmStatus.ModemStartup = true;
                            BufferUpdateTypeInternal = BUFFER_UPDATE_TYPE.MODEM_READY;
                            result = FUNC_RESULT.FAIL;
                        }
                        else if (BufferUpdateTypeExternal == BUFFER_UPDATE_TYPE.MODEM_STARTUP)
                        {
                            this._runtimeInfo.GsmStatus.ModemStartup = true;
                            result = FUNC_RESULT.SUCCESS;
                        }
                        else result = Result;
                        break;
                    }

                default:
                    result = Result;
                    break;
            }
            return result;
        }
        //End :Buffer Check

    }
}
