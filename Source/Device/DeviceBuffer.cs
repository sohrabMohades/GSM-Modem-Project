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
            else if (BufferLastIndex <= DeviceDefinitions.BUFFER_LAST_INDEX_INVALID || BufferLastIndex > DeviceDefinitions.BUFFER_LAST_INDEX_MAX) return FUNC_RESULT.MAJOR_ERROR;
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
        //Buffer Update Main
        private FUNC_RESULT BufferUpdate(BUFFER_UPDATE_SOURCE BufferUpdateSource, BUFFER_UPDATE_TYPE BufferUpdateType, bool ApplyOperationDelay, ref BUFFER_UPDATE_TYPE BufferUpdateTypeOut)
        {
            if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.UNKNOWN) return FUNC_RESULT.MAJOR_ERROR;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferUpdate), this.FuncType(nameof(this.BufferUpdate)), new { BufferUpdateSource, BufferUpdateType });
                this.BufferDataRemoveExtra(result, BufferUpdateSource);
                result = this.BufferUpdateList(result, BufferUpdateSource, ApplyOperationDelay);
                result = this.BufferUpdateAnalyze(result, BufferUpdateSource, BufferUpdateType, ref BufferUpdateTypeOut);
                return result;
            }
            finally { this._logger.FuncEnd(result, BufferUpdateType); }
        }
        //End : Buffer Update Main

        /// <summary>
        /// Removes null and non-preserved BufferData records from all groups.
        /// Preserved records remain unchanged. Empty groups are also removed.
        /// BufferRawData is not accessed or modified.
        /// Call after a Workflow is completed or during TIMER and DATA_UPDATE_ONLY.
        /// </summary>
        /// <param name="Result">The previous operation result. Cleanup runs only when SUCCESS.</param>
        private void BufferDataRemoveExtra(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource = BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY)
        {
            if (Result != FUNC_RESULT.SUCCESS) return;
            if (BufferUpdateSource != BUFFER_UPDATE_SOURCE.TIMER && BufferUpdateSource != BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY) return;
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
        //Buffer Update List
        private FUNC_RESULT BufferUpdateList(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource, bool ApplyOperationDelay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.BufferUpdateList), this.FuncType(nameof(this.BufferUpdateList)));
                result = this.BufferUpdateRawList(result, BufferUpdateSource, ApplyOperationDelay);
                result = this.BufferUpdateDataList(result);
                return BufferUpdateListResult(result, BufferUpdateSource);
            }
            finally { this._logger.FuncEnd(result); }
        }
        //Buffer Update RAW List
        private FUNC_RESULT BufferUpdateRawList(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource, bool ApplyOperationDelay)
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
                if (result == FUNC_RESULT.SUCCESS) result = this.BufferClearWorkflow(bufferCleareApplyDelay, internalValueSet);
                else this.BufferClearWorkflow(bufferCleareApplyDelay, internalValueSet);
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
                }
                if (resultTemp == FUNC_RESULT.MAJOR_ERROR) result = FUNC_RESULT.MAJOR_ERROR;
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        //End : Buffer Update RAW List
        //BufferDataGroup Update
        private FUNC_RESULT BufferUpdateDataList(FUNC_RESULT Result)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            try
            {
                this._logger.FuncStart(nameof(this.BufferUpdateDataList), this.FuncType(nameof(this.BufferUpdateDataList)));
                for (int i = 0; i < this._runtimeBuffer.BufferDataRaw.Count; i++)
                {
                    try { this._runtimeBuffer.BufferDataGroup.Add(StringAnalyzer.BufferAnalyzeRawMessage(this._runtimeBuffer.BufferDataRaw[i])); }
                    catch { this._logger.LogWarning(this._runtimeBuffer.BufferDataRaw[i]); }
                }
                this._runtimeBuffer.BufferDataRaw.Clear();
                result = FUNC_RESULT.SUCCESS;
                return result;
            }
            finally { this._logger.FuncEnd(result); }
        }
        //End : BufferDataGroup Update
        //////////////////////////////////////////////////////////// Next Step
        private FUNC_RESULT BufferUpdateListResult(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource)
        {
            switch (BufferUpdateSource)
            {
                case BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY:
                case BUFFER_UPDATE_SOURCE.WORKFLOW_START:
                case BUFFER_UPDATE_SOURCE.WORKFLOW:
                    return Result;

                case BUFFER_UPDATE_SOURCE.TIMER:
                    int bufferDataCount = 0;
                    return this.BufferDataCount(ref bufferDataCount);

                case BUFFER_UPDATE_SOURCE.UNKNOWN:
                default:
                    return FUNC_RESULT.MAJOR_ERROR;
                    
            }
        }
        private FUNC_RESULT BufferDataCount(ref int BufferDataCount)
        {
            BufferDataCount = 0;
            foreach (BufferDataGroup bufferDataGroup in this._runtimeBuffer.BufferDataGroup)
                BufferDataCount += bufferDataGroup.BufferData.Count;
            if (BufferDataCount > 0) return FUNC_RESULT.SUCCESS;
            else return FUNC_RESULT.FAIL;
        }
        //End : Buffer Update List
        //Buffer Update Analyze
        private FUNC_RESULT BufferUpdateAnalyze(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource, BUFFER_UPDATE_TYPE BufferUpdateType, ref BUFFER_UPDATE_TYPE BufferUpdateTypeOut)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY) 
                result = FUNC_RESULT.SUCCESS;
            else if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.WORKFLOW || BufferUpdateSource == BUFFER_UPDATE_SOURCE.WORKFLOW_START)
                result = this.BufferUpdateAnalyzeWorkFlow(BufferUpdateType, ref BufferUpdateTypeOut);
            else if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.TIMER) 
                result = this.BufferUpdateAnalyzeTimer();
            else 
                result = FUNC_RESULT.MAJOR_ERROR;
            return result;
        }
        private FUNC_RESULT BufferUpdateAnalyzeWorkFlow(BUFFER_UPDATE_TYPE BufferUpdateType, ref BUFFER_UPDATE_TYPE BufferUpdateTypeOut)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            BufferUpdateTypeOut = BUFFER_UPDATE_TYPE.UNKOWN;
            foreach (BufferData bufferData in this._runtimeBuffer.BufferDataGroup.SelectMany(group => group.BufferData).Where(data => 
                                                                                data != null && data.ReceivedTime >= this._runtimeBuffer.BufferLastUpdatePerior))
            {
                switch (BufferUpdateType)
                {
                    case BUFFER_UPDATE_TYPE.DEVICE_NUMBER_GET:
                    case BUFFER_UPDATE_TYPE.GSM_POWER_GET:
                    case BUFFER_UPDATE_TYPE.GSM_POWER_SET:
                        {result = bufferData.Status == BUFFER_ACTION_STATUS.SUCCEEDED ? FUNC_RESULT.SUCCESS : FUNC_RESULT.FAIL; break; }

                    case BUFFER_UPDATE_TYPE.MODEM_STARTUP:
                        {
                            if (bufferData.Action == BUFFER_ACTION.MODEM_READY) { result = FUNC_RESULT.SUCCESS; BufferUpdateTypeOut = BUFFER_UPDATE_TYPE.MODEM_READY; }
                            else { result = bufferData.Action == BUFFER_ACTION.MODEM_STARTUP ? FUNC_RESULT.SUCCESS : FUNC_RESULT.FAIL; }
                            break;
                        }

                    case BUFFER_UPDATE_TYPE.MODEM_READY:
                        { result = bufferData.Action == BUFFER_ACTION.MODEM_READY ? FUNC_RESULT.SUCCESS : FUNC_RESULT.FAIL; break; }

                    case BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY:
                        if (bufferData.Action == BUFFER_ACTION.MODEM_STARTUP) { result = FUNC_RESULT.SUCCESS; BufferUpdateTypeOut = BUFFER_UPDATE_TYPE.MODEM_STARTUP; }
                        else if (bufferData.Action == BUFFER_ACTION.MODEM_READY) { result = FUNC_RESULT.SUCCESS; BufferUpdateTypeOut = BUFFER_UPDATE_TYPE.MODEM_READY; }
                        else result = FUNC_RESULT.FAIL;
                        break;

                    default:
                        result = FUNC_RESULT.UNKNOWN;break;
                }
                if (result == FUNC_RESULT.SUCCESS) break;
            }
            return result;
        }
        private FUNC_RESULT BufferUpdateAnalyzeTimer()
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            //MUST COMPLETE
            result = FUNC_RESULT.SUCCESS;
            //END : MUST COMPLETE
            return result;
        }
        //End : Buffer Update Analyze

        //Buffer Check
        private FUNC_RESULT BufferCheck(FUNC_RESULT Result, BUFFER_UPDATE_TYPE BufferUpdateType)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            bool applyOperationDelay = true;
            DateTime timeStart = DateTime.Now;
            Stopwatch timeoutWatch = Stopwatch.StartNew();
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, resultTimeout = FUNC_RESULT.UNKNOWN;
            BUFFER_UPDATE_SOURCE bufferUpdateSource = BUFFER_UPDATE_SOURCE.WORKFLOW_START;
            BUFFER_UPDATE_TYPE bufferUpdateTypeInternal = BufferUpdateType;
            do
            {
                result = this.BufferUpdate(bufferUpdateSource, BufferUpdateType, applyOperationDelay, ref bufferUpdateTypeInternal);
                this.BufferCheckUpdateSource(ref bufferUpdateSource);
                result = this.BufferCheckRules(result, BufferUpdateType, ref bufferUpdateTypeInternal);
                resultTimeout = this.BufferCheckTimeout(result, bufferUpdateSource, bufferUpdateTypeInternal);
            }
            while (this.BufferCheckWhileCondition(ref result, resultTimeout));
            return result;
        }
        private void BufferCheckUpdateSource(ref BUFFER_UPDATE_SOURCE BufferUpdateSource)
        {
            if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.WORKFLOW_START) BufferUpdateSource = BUFFER_UPDATE_SOURCE.WORKFLOW;
        }
        private FUNC_RESULT BufferCheckRules(FUNC_RESULT Result, BUFFER_UPDATE_TYPE BufferUpdateTypeExternal, ref BUFFER_UPDATE_TYPE BufferUpdateTypeInternal)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            switch (BufferUpdateTypeInternal)
            {
                case BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY: case BUFFER_UPDATE_TYPE.MODEM_READY:
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
        private FUNC_RESULT BufferCheckTimeout(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource, BUFFER_UPDATE_TYPE BufferUpdateType)
        {
            if (Result == FUNC_RESULT.SUCCESS || Result == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.UNKNOWN;//return Result;
            int timeout = (int)(DateTime.Now - this._runtimeBuffer.BufferLastUpdatePerior).TotalMilliseconds;
            if (timeout > this._runtimeConfig.TimeoutMaxMs) return FUNC_RESULT.FAIL;
            switch (BufferUpdateType)
            {
                case BUFFER_UPDATE_TYPE.DEVICE_NUMBER_GET:
                case BUFFER_UPDATE_TYPE.GSM_POWER_GET:
                case BUFFER_UPDATE_TYPE.GSM_POWER_SET:
                    if (timeout <= (int)BUFFER_TIME_OUT_MS.SHORT) return FUNC_RESULT.SUCCESS;
                    else return FUNC_RESULT.FAIL;

                case BUFFER_UPDATE_TYPE.MODEM_READY:
                case BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY:
                    if (!this._runtimeInfo.GsmStatus.ModemStartup)
                        if (timeout <= (int)BUFFER_TIME_OUT_MS.MODEM_STARTUP) return FUNC_RESULT.SUCCESS;
                        else return FUNC_RESULT.FAIL;
                    else if (!this._runtimeInfo.GsmStatus.ModemReady)
                        if (timeout <= (int)BUFFER_TIME_OUT_MS.MODEM_READY) return FUNC_RESULT.SUCCESS;
                        else return FUNC_RESULT.FAIL;
                    else return FUNC_RESULT.UNKNOWN;

                case BUFFER_UPDATE_TYPE.MODEM_STARTUP:
                    if (!this._runtimeInfo.GsmStatus.ModemStartup)
                        if (timeout <= (int)BUFFER_TIME_OUT_MS.MODEM_STARTUP) return FUNC_RESULT.SUCCESS;
                        else return FUNC_RESULT.FAIL;
                    else return FUNC_RESULT.UNKNOWN;

                default:
                    return FUNC_RESULT.UNKNOWN;
            }
        }
        private bool BufferCheckWhileCondition(ref FUNC_RESULT Result, FUNC_RESULT ResultTimeout)
        {
            if (Result == FUNC_RESULT.SUCCESS) return false;
            if (Result == FUNC_RESULT.MAJOR_ERROR) return false;
            if (ResultTimeout == FUNC_RESULT.FAIL) { Result = FUNC_RESULT.FAIL; return false; }
            return true;
        }
        //End :Buffer Check

        /*
        private FUNC_RESULT BufferTimeoutCheck(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource, BUFFER_UPDATE_TYPE BufferUpdateType)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (BufferUpdateSource != BUFFER_UPDATE_SOURCE.WORKFLOW) return FUNC_RESULT.SUCCESS;
            int timeout = (int)(DateTime.Now - this._runtimeBuffer.BufferLastUpdatePerior).TotalMilliseconds;
            if (timeout > this._runtimeConfig.TimeoutMaxMs) return FUNC_RESULT.MAJOR_ERROR;
            switch (BufferUpdateType)
            {
                case BUFFER_UPDATE_TYPE.DEVICE_NUMBER_GET:
                case BUFFER_UPDATE_TYPE.GSM_POWER_GET:
                case BUFFER_UPDATE_TYPE.GSM_POWER_SET:
                    if (timeout <= (int)BUFFER_TIME_OUT_MS.SHORT) return FUNC_RESULT.SUCCESS;
                    else return FUNC_RESULT.MAJOR_ERROR;

                case BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY:
                    if (!this._runtimeInfo.GsmStatus.ModemStartup)
                        if (timeout <= (int)BUFFER_TIME_OUT_MS.MODEM_STARTUP) return FUNC_RESULT.SUCCESS;
                        else return FUNC_RESULT.MAJOR_ERROR;
                    else if (!this._runtimeInfo.GsmStatus.ModemReady)
                        if (timeout <= (int)BUFFER_TIME_OUT_MS.MODEM_READY) return FUNC_RESULT.SUCCESS;
                        else return FUNC_RESULT.MAJOR_ERROR;
                    else return FUNC_RESULT.MAJOR_ERROR;

                default:
                    return FUNC_RESULT.UNKNOWN;
            }
        }
        */
        //End : Buffer TimeOut Check

        /*
        private FUNC_RESULT XXXBufferCheck(FUNC_RESULT Result, ref BUFFER_UPDATE_SOURCE BufferUpdateSource, BUFFER_UPDATE_TYPE BufferUpdateType)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            bool applyOperationDelay = true;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            BUFFER_UPDATE_TYPE bufferUpdateType = BufferUpdateType;
            do
            {
                result = this.BufferUpdate(BufferUpdateSource, ref bufferUpdateType, applyOperationDelay);
                if (result == FUNC_RESULT.SUCCESS)
                    switch (bufferUpdateType)
                    {
                        case BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY:
                        case BUFFER_UPDATE_TYPE.MODEM_READY:
                            { this._runtimeInfo.GsmStatus.ModemStartup = true; this._runtimeInfo.GsmStatus.ModemReady = true; break; }

                        case BUFFER_UPDATE_TYPE.MODEM_STARTUP:
                            {
                                this._runtimeInfo.GsmStatus.ModemStartup = true;
                                if (BufferUpdateType == BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY || BufferUpdateType == BUFFER_UPDATE_TYPE.MODEM_READY)
                                { result = FUNC_RESULT.FAIL; bufferUpdateType = BUFFER_UPDATE_TYPE.MODEM_READY; }
                                break;
                            }

                        default:
                            break;
                    }

                if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.WORKFLOW_START) BufferUpdateSource = BUFFER_UPDATE_SOURCE.WORKFLOW;
            }
            while (result != FUNC_RESULT.SUCCESS && result != FUNC_RESULT.MAJOR_ERROR && (BufferUpdateSource == BUFFER_UPDATE_SOURCE.WORKFLOW || BufferUpdateSource == BUFFER_UPDATE_SOURCE.WORKFLOW_START));
            if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY) BufferUpdateSource = BUFFER_UPDATE_SOURCE.WORKFLOW_START;
            return result;
        }
        */
        //End : Buffer Update

    }
}
