using GSM.Core.Source.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        //Buffer Clear
        /// <summary>
        /// Clears the modem buffer and resets the corresponding runtime buffer state.
        /// </summary>
        /// <param name="ApplyOperationDelay">
        /// Specifies whether to apply the buffer update delay before clearing.
        /// </param>
        /// <param name="InternalValueSet">
        /// Specifies whether the internal runtime values should be updated.
        /// </param>
        /// <returns>
        /// SUCCESS if the operation succeeds; otherwise, the corresponding failure result.
        /// </returns>
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
        /// <summary>
        /// Executes the buffer clear operation after applying the specified delay.
        /// </summary>
        /// <param name="Result">The result of the previous operation.</param>
        /// <param name="Delay">The delay in milliseconds before execution.</param>
        /// <returns>The result of the buffer clear operation.</returns>
        private FUNC_RESULT BufferClearExecute(FUNC_RESULT Result, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferClearExecute), this.FuncType(nameof(this.BufferClearExecute)));
                Thread.Sleep(Delay);
                return result = this.BufferClearBase();
            }
            finally { this._logger.FuncEnd(result); }
        }
        /// <summary>
        /// Performs the base-level modem buffer clear operation.
        /// </summary>
        /// <returns>The result of the buffer clear operation.</returns>
        private FUNC_RESULT BufferClearBase()
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferClearBase), this.FuncType(nameof(this.BufferClearBase)));
                return result = this.BufferClearBaseDll(result);
            }
            finally { this._logger.FuncEnd(result); }
        }
        /// <summary>
        /// Calls the DLL function to clear the modem buffer.
        /// </summary>
        /// <param name="Result">The result of the previous operation.</param>
        /// <returns>The DLL operation result.</returns>
        private FUNC_RESULT BufferClearBaseDll(FUNC_RESULT Result)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferClearBaseDll), this.FuncType(nameof(this.BufferClearBaseDll)));
                this._dllWrapper.ClearBuffer();
                return result = this.DllWrapperHasError();
            }
            finally { this._logger.FuncEnd(result); }
        }
        //End : Buffer Clear

        //Buffer new Index read
        /// <summary>
        /// Reads the latest modem buffer index and updates the runtime buffer state.
        /// </summary>
        /// <param name="LastIndex">Receives the latest buffer index.</param>
        /// <param name="LastIndexNotEqual">
        /// Specifies whether the new index must differ from the previously stored index.
        /// </param>
        /// <param name="ApplyOperationDelay">
        /// Specifies whether to apply the buffer update delay.
        /// </param>
        /// <param name="InternalValueSet">
        /// Specifies whether internal runtime values should be updated.
        /// </param>
        /// <returns>The result of the buffer index retrieval operation.</returns>
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
        /// <summary>
        /// Executes the buffer index retrieval operation after applying the specified delay.
        /// </summary>
        /// <param name="Result">The result of the previous operation.</param>
        /// <param name="Delay">The delay in milliseconds before execution.</param>
        /// <param name="BufferLastIndex">Receives the latest buffer index.</param>
        /// <returns>The result of the retrieval operation.</returns>
        private FUNC_RESULT LastBufferIndexGetExecute(FUNC_RESULT Result, int Delay, ref int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.LastBufferIndexGetExecute), this.FuncType(nameof(this.LastBufferIndexGetExecute)));
                Thread.Sleep(Delay);
                return result = this.BufferLastIndexGetBase(ref BufferLastIndex);
            }
            finally { this._logger.FuncEnd(result, BufferLastIndex); }
        }
        /// <summary>
        /// Validates the retrieved buffer index against the previous index and the requested conditions.
        /// </summary>
        /// <param name="Result">The result of the retrieval operation.</param>
        /// <param name="BufferLastIndexOld">The previously stored buffer index.</param>
        /// <param name="BufferLastIndexNew">The newly retrieved buffer index.</param>
        /// <param name="LastIndexNotEqual">Specifies whether the new index must differ from the old index.</param>
        /// <returns>SUCCESS if the new index is valid and meets the requested conditions; otherwise, FAIL or MAJOR_ERROR.</returns>
        private FUNC_RESULT LastBufferIndexGetCheck(FUNC_RESULT Result, int BufferLastIndexOld, int BufferLastIndexNew, bool LastIndexNotEqual)
        {
            if (Result == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            else if (Result == FUNC_RESULT.SUCCESS && (!LastIndexNotEqual || BufferLastIndexOld != BufferLastIndexNew) && BufferLastIndexNew != DeviceDefinitions.NUMBER_INVALID) return FUNC_RESULT.SUCCESS;
            else return FUNC_RESULT.FAIL;
        }
        /// <summary>
        /// Retrieves and validates the modem buffer index at the base level.
        /// </summary>
        /// <param name="BufferLastIndex">Receives the validated buffer index.</param>
        /// <returns>The result of the retrieval and validation operation.</returns>
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
        /// <summary>
        /// Retrieves the latest buffer index through the modem DLL.
        /// </summary>
        /// <param name="Result">The result of the previous operation.</param>
        /// <param name="BufferLastIndex">Receives the index returned by the DLL.</param>
        /// <returns>The DLL operation result.</returns>
        private FUNC_RESULT BufferLastIndexGetBaseDll(FUNC_RESULT Result, ref int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferLastIndexGetBaseDll), this.FuncType(nameof(this.BufferLastIndexGetBaseDll)));
                BufferLastIndex = (int)this._dllWrapper.BufferLastIndexGet();
                return result = this.DllWrapperHasError();
            }
            finally { this._logger.FuncEnd(result, BufferLastIndex); }
        }
        /// <summary>
        /// Validates and normalizes the buffer index returned by the DLL.
        /// </summary>
        /// <param name="Result">The result of the DLL operation.</param>
        /// <param name="BufferLastIndex">The buffer index to validate and normalize.</param>
        /// <returns>SUCCESS if the index is valid; otherwise, the corresponding failure result.</returns>
        private FUNC_RESULT BufferLastIndexGetBaseCheck(FUNC_RESULT Result, ref int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (BufferLastIndex > DeviceDefinitions.BUFFER_LAST_INDEX_MAX) BufferLastIndex = DeviceDefinitions.BUFFER_LAST_INDEX_MAX;
            if (BufferLastIndex > DeviceDefinitions.NUMBER_INVALID) return FUNC_RESULT.SUCCESS;
            else { this.ErrorSet(ERROR_CODE.BUFFER_LAST_INDEX_GET, "BufferLastIndex = " + BufferLastIndex + " -> Invalid"); return FUNC_RESULT.FAIL; }
        }
        //End : Buffer new Index read

        //Buffer new Index read
        /// <summary>
        /// Validates and normalizes the buffer index returned by the DLL.
        /// </summary>
        /// <param name="Result">The result of the DLL operation.</param>
        /// <param name="BufferLastIndex">The buffer index to validate and normalize.</param>
        /// <returns>SUCCESS if the index is valid; otherwise, the corresponding failure result.</returns>
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
        /// <summary>
        /// Executes the buffer message retrieval operation after applying the specified delay.
        /// </summary>
        /// <param name="Result">The result of the previous operation.</param>
        /// <param name="Delay">The delay in milliseconds before execution.</param>
        /// <param name="BufferLastIndex">The buffer index to read.</param>
        /// <param name="BufferMessage">Receives the retrieved message.</param>
        /// <param name="BufferMessageLen">Receives the retrieved message length.</param>
        /// <returns>The result of the retrieval operation.</returns>
        private FUNC_RESULT BufferMessageGetExecute(FUNC_RESULT Result, int Delay, int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferMessageGetExecute), this.FuncType(nameof(this.BufferMessageGetExecute)), BufferLastIndex);
                Thread.Sleep(Delay);
                return result = this.BufferMessageGetBase(BufferLastIndex, ref BufferMessage, ref BufferMessageLen);
            }
            finally { this._logger.FuncEnd(result); }
        }
        /// <summary>
        /// Performs the base-level buffer message retrieval and validation.
        /// </summary>
        /// <param name="BufferLastIndex">The buffer index to read.</param>
        /// <param name="BufferMessage">Receives the retrieved message.</param>
        /// <param name="BufferMessageLen">Receives the retrieved message length.</param>
        /// <returns>The result of the retrieval operation.</returns>
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
        /// <summary>
        /// Validates the specified buffer index before reading the message.
        /// </summary>
        /// <param name="Result">The result of the previous operation.</param>
        /// <param name="BufferLastIndex">The buffer index to validate.</param>
        /// <returns>SUCCESS if the index is valid; otherwise, the corresponding failure result.</returns>
        private FUNC_RESULT BufferMessageGetBaseCheck(FUNC_RESULT Result, int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            else if (BufferLastIndex < DeviceDefinitions.BUFFER_LAST_INDEX_MIN || BufferLastIndex > DeviceDefinitions.BUFFER_LAST_INDEX_MAX)
            { this.ErrorSet(ERROR_CODE.INVALID_INPUT, "BufferLastIndex = " + BufferLastIndex + " -> Invalid"); return FUNC_RESULT.MAJOR_ERROR; }
            else return FUNC_RESULT.SUCCESS;
        }
        /// <summary>
        /// Reads a message from the modem buffer through the DLL.
        /// </summary>
        /// <param name="Result">The result of the previous operation.</param>
        /// <param name="BufferLastIndex">The buffer index to read.</param>
        /// <param name="BufferMessage">Receives the retrieved message.</param>
        /// <param name="BufferMessageLen">Receives the retrieved message length.</param>
        /// <returns>The DLL operation result.</returns>
        private FUNC_RESULT BufferMessageGetBaseDll(FUNC_RESULT Result, int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferMessageGetBaseDll), this.FuncType(nameof(this.BufferMessageGetBaseDll)), BufferLastIndex);
                this._dllWrapper.BufferRead(ref BufferLastIndex, out BufferMessage, ref BufferMessageLen);
                return result = this.DllWrapperHasError();
            }
            finally { this._logger.FuncEnd(result); }
        }
        //End : Buffer new Index read

        //Buffer Update
        /// <summary>
        /// Updates the modem buffer by retrieving raw messages and analyzing them.
        /// </summary>
        /// <param name="ApplyOperationDelay">Specifies whether to apply operation delays.</param>
        /// <param name="ClearBufferExtra">Specifies whether unnecessary buffer data should be removed.</param>
        /// <returns>The result of the buffer update operation.</returns>
        private FUNC_RESULT BufferUpdate(bool ApplyOperationDelay, bool ClearBufferExtra)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferUpdate), this.FuncType(nameof(this.BufferUpdate)));
                this.BufferDataRemoveUnnecessary(result, ClearBufferExtra);
                result = this.BufferUpdateRawList(result, ApplyOperationDelay);
                return result = this.BufferUpdateDataList(result);
            }
            finally { this._logger.FuncEnd(result); }
        }
        /// <summary>
        /// Removes buffer records that are not included in the preserved action list.
        /// </summary>
        /// <param name="Result">The result of the preceding operation.</param>
        /// <param name="ClearBufferExtra">Specifies whether extra buffer records should be removed.</param>
        private void BufferDataRemoveUnnecessary(FUNC_RESULT Result, bool ClearBufferExtra)
        {
            if (Result != FUNC_RESULT.SUCCESS || !ClearBufferExtra) return;
            List<BufferData> removedBufferData = new List<BufferData>();
            int removedNullCount = 0;
            int removedNullGroupCount = 0;
            int removedNullBufferDataListCount = 0;
            bool IsPreservedAction(BUFFER_ACTION action)
            {
                switch (action)
                {
                    case BUFFER_ACTION.MODEM_STARTUP: case BUFFER_ACTION.MODEM_READY: case BUFFER_ACTION.SMS_RECEIVED: case BUFFER_ACTION.SMS_DELIVERY_REPORT: return true;
                    default: return false;
                }
            }
            BufferDataGroup group;
            BufferData bufferData;
            try
            {
                this._logger.FuncStart(nameof(this.BufferDataRemoveUnnecessary), this.FuncType(nameof(this.BufferDataRemoveUnnecessary)));
                if (this._runtimeBuffer.BufferDataGroup == null) { this._logger.LogWarning("BufferDataGroup is NULL."); return; }
                for (int groupIndex = this._runtimeBuffer.BufferDataGroup.Count - 1; groupIndex >= 0; groupIndex--)
                {
                    if ((group = this._runtimeBuffer.BufferDataGroup[groupIndex]) == null) { removedNullGroupCount++; this._runtimeBuffer.BufferDataGroup.RemoveAt(groupIndex); continue; }
                    if (group.BufferData == null) { removedNullBufferDataListCount++; this._runtimeBuffer.BufferDataGroup.RemoveAt(groupIndex); continue; }
                    for (int dataIndex = group.BufferData.Count - 1; dataIndex >= 0; dataIndex--)
                    {
                        bufferData = group.BufferData[dataIndex];
                        if (bufferData == null) { removedNullCount++; group.BufferData.RemoveAt(dataIndex); }
                        else if (!IsPreservedAction(bufferData.Action)) { removedBufferData.Add(bufferData); group.BufferData.RemoveAt(dataIndex); }
                    }
                    if (group.BufferData.Count == 0) this._runtimeBuffer.BufferDataGroup.RemoveAt(groupIndex);
                }
            }
            finally { this._logger.FuncEnd(FUNC_RESULT.UNKNOWN, new { RemovedBufferData = removedBufferData, RemovedNullCount = removedNullCount, RemovedNullGroupCount = removedNullGroupCount, RemovedNullBufferDataListCount = removedNullBufferDataListCount }); }
        }
        /// <summary>
        /// Retrieves raw modem buffer messages and clears the modem buffer afterward.
        /// </summary>
        /// <param name="Result">The result of the preceding operation.</param>
        /// <param name="ApplyOperationDelay">Specifies whether to apply the buffer update delay.</param>
        /// <returns>The result of the raw buffer retrieval and clearing operation.</returns>
        private FUNC_RESULT BufferUpdateRawList(FUNC_RESULT Result, bool ApplyOperationDelay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            bool lastIndexNotEqual = false, internalValueSet = true, bufferCleareApplyDelay = false;
            int bufferLastIndex = DeviceDefinitions.NUMBER_INVALID;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS, resultClear = FUNC_RESULT.UNKNOWN;
            try
            {
                this._logger.FuncStart(nameof(this.BufferUpdateRawList), this.FuncType(nameof(this.BufferUpdateRawList)));
                result = this.BufferLastIndexGetWorkflow(ref bufferLastIndex, lastIndexNotEqual, ApplyOperationDelay, internalValueSet);
                result = this.BufferUpdateRawListFill(result, bufferLastIndex);
                resultClear = this.BufferClearWorkflow(bufferCleareApplyDelay, internalValueSet);
                return result = result == FUNC_RESULT.SUCCESS ? resultClear : result; 
            }
            finally { this._logger.FuncEnd(result); }
        }
        /// <summary>
        /// Reads buffer messages from the first index through the specified last index.
        /// </summary>
        /// <param name="Result">The result of the preceding operation.</param>
        /// <param name="BufferLastIndex">The last buffer index to read.</param>
        /// <returns>SUCCESS if at least one message is retrieved; otherwise, the corresponding failure result.</returns>
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
        /// <summary>
        /// Analyzes the retrieved raw buffer messages and adds the resulting records to the runtime buffer.
        /// </summary>
        /// <param name="Result">The result of the preceding operation.</param>
        /// <returns>SUCCESS if at least one raw message is analyzed successfully; otherwise, FAIL.</returns>
        private FUNC_RESULT BufferUpdateDataList(FUNC_RESULT Result)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            try
            {
                this._logger.FuncStart(nameof(this.BufferUpdateDataList), this.FuncType(nameof(this.BufferUpdateDataList)));
                result = FUNC_RESULT.FAIL;
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
        private List<BufferPattern> GetBufferAnalyzePatterns(BUFFER_UPDATE_TYPE BufferUpdateType)
        {
            if (BufferUpdateType == BUFFER_UPDATE_TYPE.MODEM_READY || BufferUpdateType == BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY) return new List<BufferPattern>
            {
                new BufferPattern 
                // Pattern 1: The modem directly reports that it is ready.
                { Mode = BUFFER_PATTERN_MODE.STRICT, Items = new List<BufferPatternItem> { new BufferPatternItem { Action = BUFFER_ACTION.MODEM_READY, Status = null } }  },
                // Pattern 2: The modem reports startup first and readiness afterward.
                new BufferPattern
                { Mode = BUFFER_PATTERN_MODE.STRICT, Items = new List<BufferPatternItem> { new BufferPatternItem { Action = BUFFER_ACTION.MODEM_STARTUP, Status = null } } }
            };
            return null;
        }
        /// <summary>
        /// Analyzes the buffer against the specified list of patterns.
        /// </summary>
        /// <param name="Patterns">The ordered list of patterns to be analyzed.</param>
        /// <param name="AnalyzeStartTime">The earliest timestamp eligible for analysis.</param>
        /// <returns>
        /// SUCCESS if any pattern is matched;
        /// FAIL if none of the patterns match;
        /// MAJOR_ERROR if an invalid pattern or a major error is encountered.
        /// </returns>
        private FUNC_RESULT BufferAnalyzeMatch(FUNC_RESULT Result, BUFFER_UPDATE_TYPE BufferUpdateType, DateTime AnalyzeStartTime)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            try
            {
                this._logger.FuncStart(nameof(this.BufferAnalyzeMatch), this.FuncType(nameof(this.BufferAnalyzeMatch)));
                List<BufferPattern> patterns = this.GetBufferAnalyzePatterns(BufferUpdateType);
                if ((result = BufferAnalyzeMatchCheck(patterns)) != FUNC_RESULT.SUCCESS) return result;
                foreach (BufferPattern pattern in patterns)
                {
                    result = this.BufferAnalyzeMatchPattern(pattern, AnalyzeStartTime);
                    if (result == FUNC_RESULT.MAJOR_ERROR || result == FUNC_RESULT.SUCCESS) return result;
                }
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        /// <summary>
        /// Validates the list of patterns before starting the analysis.
        /// </summary>
        /// <param name="Patterns">The list of patterns to be analyzed.</param>
        /// <returns>
        /// SUCCESS if the pattern list is valid;
        /// MAJOR_ERROR if the pattern list is null or empty.
        /// </returns>
        private FUNC_RESULT BufferAnalyzeMatchCheck(List<BufferPattern> Patterns)
        {
            if (Patterns == null || Patterns.Count == 0)
            { this.ErrorSet(ERROR_CODE.BUFFER_ANALYZE_PATTERN, Patterns == null ? "Pattern = NULL" : "Patterns.Count = 0"); return FUNC_RESULT.MAJOR_ERROR; }
            return FUNC_RESULT.SUCCESS;
        }
        /// <summary>
        /// Searches the runtime buffer for the specified pattern.
        /// </summary>
        /// <param name="Pattern">The pattern to be matched.</param>
        /// <param name="AnalyzeStartTime">The earliest timestamp eligible for analysis.</param>
        /// <returns>
        /// SUCCESS if the complete pattern is found;
        /// FAIL if no complete match is found;
        /// MAJOR_ERROR if the pattern definition or an internal operation is invalid.
        /// </returns>
        private FUNC_RESULT BufferAnalyzeMatchPattern(BufferPattern Pattern, DateTime AnalyzeStartTime)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            FUNC_RESULT matchResult;
            int patternIndex = 0;
            try
            {
                this._logger.FuncStart(nameof(this.BufferAnalyzeMatchPattern), this.FuncType(nameof(this.BufferAnalyzeMatchPattern)));
                if ((result = this.BufferAnalyzeMatchPatternCheck(Pattern)) != FUNC_RESULT.SUCCESS) return result;
                // Analyze all buffer records as one continuous sequence.
                foreach (BufferData bufferData in this._runtimeBuffer.BufferDataGroup.Where(group => group?.BufferData != null).SelectMany(group => group.BufferData).Where
                                                                                                            (data => data != null && data.ReceivedTime >= AnalyzeStartTime))
                {
                    matchResult = this.BufferAnalyzeMatchPatternItem(bufferData, Pattern.Items[patternIndex]);
                    if (matchResult == FUNC_RESULT.SUCCESS)
                    {
                        patternIndex++;
                        if (patternIndex == Pattern.Items.Count) return result = FUNC_RESULT.SUCCESS;
                        continue;
                    }
                    switch (Pattern.Mode)
                    {
                        case BUFFER_PATTERN_MODE.STRICT: break;

                        case BUFFER_PATTERN_MODE.NULL_ALLOWED: if (bufferData.Action == BUFFER_ACTION.NULL) continue; break;

                        case BUFFER_PATTERN_MODE.FLEXIBLE:
                            matchResult = this.BufferAnalyzeMatchPatternItem(bufferData, Pattern.Items[0]);
                            if (matchResult == FUNC_RESULT.SUCCESS)
                            { patternIndex = 1; if (patternIndex == Pattern.Items.Count) return result = FUNC_RESULT.SUCCESS; }
                            continue;
                    }
                    patternIndex = 0;
                    matchResult = this.BufferAnalyzeMatchPatternItem(bufferData, Pattern.Items[0]);
                    if (matchResult == FUNC_RESULT.SUCCESS)
                    { patternIndex = 1; if (patternIndex == Pattern.Items.Count) return result = FUNC_RESULT.SUCCESS; }
                }
                if (result == FUNC_RESULT.UNKNOWN) result = FUNC_RESULT.FAIL;
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        /// <summary>
        /// Validates the pattern definition and checks buffer availability.
        /// </summary>
        /// <param name="Pattern">The pattern definition to be validated.</param>
        /// <returns>
        /// SUCCESS if the pattern is valid and buffer data is available;
        /// FAIL if no buffer data is available;
        /// MAJOR_ERROR if the pattern definition is invalid.
        /// </returns>
        private FUNC_RESULT BufferAnalyzeMatchPatternCheck(BufferPattern Pattern)
        {
            string errorMessage = string.Empty;
            // Validate the pattern definition.
            if (Pattern == null || Pattern.Items == null || Pattern.Items.Count == 0 || !Enum.IsDefined(typeof(BUFFER_PATTERN_MODE), Pattern.Mode))
            {
                errorMessage = Pattern == null ? "Pattern = NULL" : (Pattern.Items == null ? "Pattern.Items = NULL" : (Pattern.Items.Count == 0 ? "Pattern.Items.Count = 0" : ""));
                errorMessage += Pattern != null && !Enum.IsDefined(typeof(BUFFER_PATTERN_MODE), Pattern.Mode) ? $"Pattern.Mode = {Pattern.Mode} -> Invalid" : "";
                this.ErrorSet(ERROR_CODE.BUFFER_ANALYZE_PATTERN, errorMessage);
                return FUNC_RESULT.MAJOR_ERROR;
            }
            // Validate all pattern items before starting the analysis.
            for (int index = 0; index < Pattern.Items.Count; index++)
            {
                BufferPatternItem patternItem = Pattern.Items[index];
                if (patternItem == null || (patternItem.Action == null && patternItem.Status == null))
                {
                    errorMessage = patternItem == null ? $"Pattern.Items[{index}] = NULL" : $"Pattern.Items[{index}] -> Action = NULL && Status = NULL";
                    this.ErrorSet(ERROR_CODE.BUFFER_ANALYZE_PATTERN, errorMessage);
                    return FUNC_RESULT.MAJOR_ERROR;
                }
            }
            // No buffer data is available for analysis.
            if (this._runtimeBuffer.BufferDataGroup == null) return FUNC_RESULT.FAIL;
            return FUNC_RESULT.SUCCESS;
        }
        /// <summary>
        /// Checks whether a buffer record matches the specified pattern item.
        /// </summary>
        /// <param name="BufferData">The buffer record to be checked.</param>
        /// <param name="PatternItem">The expected pattern item.</param>
        /// <returns>
        /// SUCCESS if the record matches the pattern item;
        /// FAIL if the record does not match;
        /// MAJOR_ERROR if the input or pattern definition is invalid.
        /// </returns>
        private FUNC_RESULT BufferAnalyzeMatchPatternItem(BufferData BufferData, BufferPatternItem PatternItem)
        {
            if (PatternItem.Action.HasValue && BufferData.Action != PatternItem.Action.Value) return FUNC_RESULT.FAIL;
            if (PatternItem.Status.HasValue && BufferData.Status != PatternItem.Status.Value) return FUNC_RESULT.FAIL;
            return FUNC_RESULT.SUCCESS;
        }
        //End : Buffer Analyze

        //Buffer Check
        /// <summary>
        /// Repeatedly updates the modem buffer until the requested operation completes or its timeout expires.
        /// </summary>
        /// <param name="Result">The result of the preceding operation.</param>
        /// <param name="BufferUpdateType">The type of operation whose buffer response is being checked.</param>
        /// <returns>The final result of the buffer check operation.</returns>
        private FUNC_RESULT BufferCheck(FUNC_RESULT Result, BUFFER_UPDATE_TYPE BufferUpdateType)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            bool applyOperationDelay = true, clearBufferExtra = false;
            DateTime timeStart = DateTime.Now;
            Stopwatch timeoutWatch = Stopwatch.StartNew();
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, resultTimeout = FUNC_RESULT.UNKNOWN;
            try
            {
                this._logger.FuncStart(nameof(this.BufferCheck), this.FuncType(nameof(this.BufferCheck)));
                do
                {
                    result = this.BufferUpdate(applyOperationDelay, clearBufferExtra);
                    resultTimeout = this.BufferCheckTimeout(result, BufferUpdateType, timeoutWatch.ElapsedMilliseconds);
                }
                while (this.BufferCheckWhileCondition(ref result, resultTimeout));
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        /// <summary>
        /// Determines whether the buffer check should continue based on the elapsed time and operation type.
        /// </summary>
        /// <param name="Result">The result of the latest buffer update operation.</param>
        /// <param name="BufferUpdateType">The type of operation being checked.</param>
        /// <param name="TimeoutMs">The elapsed time in milliseconds since the check started.</param>
        /// <returns>
        /// SUCCESS if the operation-specific timeout has not expired;
        /// FAIL if a timeout has expired;
        /// UNKNOWN if no further timeout decision is required or no rule is defined.
        /// </returns>
        private FUNC_RESULT BufferCheckTimeout(FUNC_RESULT Result, BUFFER_UPDATE_TYPE BufferUpdateType, long TimeoutMs)
        {
            if (Result == FUNC_RESULT.SUCCESS || Result == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.UNKNOWN;
            if (TimeoutMs > this._runtimeConfig.TimeoutMaxMs)
            { this._logger.LogWarning($"Timeout MAX = {this._runtimeConfig.TimeoutMaxMs} , Timeout = {TimeoutMs} , BufferType -> {BufferUpdateType}"); return FUNC_RESULT.FAIL; }
            int timeoutLimit;
            if (!BufferCheckTimeoutMap.TryGetValue(BufferUpdateType, out timeoutLimit)) return FUNC_RESULT.UNKNOWN;
            if (TimeoutMs <= timeoutLimit) return FUNC_RESULT.SUCCESS;
            else { this._logger.LogWarning($"Timeout Limit = {timeoutLimit} , Timeout = {TimeoutMs} , BufferType -> {BufferUpdateType}"); return FUNC_RESULT.FAIL; }
        }
        /// <summary>
        /// Determines whether the buffer check loop should continue.
        /// </summary>
        /// <param name="Result">The result of the latest buffer update operation.</param>
        /// <param name="ResultTimeout">The result of the timeout evaluation.</param>
        /// <returns>True if the buffer check loop should continue; otherwise, false.</returns>
        private bool BufferCheckWhileCondition(ref FUNC_RESULT Result, FUNC_RESULT ResultTimeout)
        {
            if (Result == FUNC_RESULT.SUCCESS || Result == FUNC_RESULT.MAJOR_ERROR) return false;
            if (ResultTimeout == FUNC_RESULT.FAIL) { Result = FUNC_RESULT.FAIL; return false; }
            return true;
        }
        //End :Buffer Check

    }
}
