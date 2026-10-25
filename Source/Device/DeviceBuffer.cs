using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        //Buffer Clear
        private FUNC_RESULT BufferClearWorkflow(bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.BufferClearExecute(result, (int)OPERATION_DELAY_MS.BUFFER_UPDATE);
            this.XXXSetValue(DeviceDefinitions.NUMBER_INVALID, ref this._runtimeBuffer.BufferLastIndex, InternalValueSet);
            this.XXXSetValue(false, ref this._runtimeBuffer.BufferNewData, InternalValueSet);
            this.XXXSetValue(DeviceDefinitions.STRING_INVALID, ref this._runtimeBuffer.BufferMessage, InternalValueSet);
            this.XXXSetValue(DeviceDefinitions.NUMBER_INVALID, ref this._runtimeBuffer.BufferMessageLen, InternalValueSet);
            return result;
        }
        private FUNC_RESULT BufferClearExecute(FUNC_RESULT Result, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.BufferClearBase();
        }
        private FUNC_RESULT BufferClearBase()
        {
            return this.BufferClearBaseDll(FUNC_RESULT.SUCCESS);
        }
        private FUNC_RESULT BufferClearBaseDll(FUNC_RESULT Result)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            this._dllWrapper.ClearBuffer();
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : Buffer Clear

        //Buffer new Index read
        private FUNC_RESULT BufferLastIndexGetWorkflow(ref int BufferLastIndex, bool InternalValueSet)
        {
            bool tempBool = false;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.LastBufferIndexGetExecute(result, (int)OPERATION_DELAY_MS.BUFFER_UPDATE, ref BufferLastIndex);
            result = this.LastBufferIndexGetCheck(result, this._runtimeBuffer.BufferLastIndex, BufferLastIndex);
            this.XXXSetValue(result, BufferLastIndex, DeviceDefinitions.NUMBER_INVALID, ref BufferLastIndex, ref this._runtimeBuffer.BufferLastIndex, InternalValueSet);
            this.XXXSetValue(result, true, false, ref tempBool, ref this._runtimeBuffer.BufferNewData, InternalValueSet);
            return result;
        }
        private FUNC_RESULT LastBufferIndexGetExecute(FUNC_RESULT Result, int Delay, ref int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.BufferLastIndexGetBase(ref BufferLastIndex);
        }
        private FUNC_RESULT LastBufferIndexGetCheck(FUNC_RESULT Result, int BufferLastIndexOld, int BufferLastIndexNew)
        {
            if (Result == FUNC_RESULT.SUCCESS && BufferLastIndexOld != BufferLastIndexNew && BufferLastIndexNew != DeviceDefinitions.NUMBER_INVALID) return FUNC_RESULT.SUCCESS;
            else if (Result == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            else return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT BufferLastIndexGetBase(ref int BufferLastIndex)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            int bufferLastIndex = DeviceDefinitions.NUMBER_INVALID;
            result = this.BufferLastIndexGetBaseDll(result, ref bufferLastIndex);
            result = this.BufferLastIndexGetBaseCheck(result, ref bufferLastIndex);
            this.XXXBaseSetValue(result, bufferLastIndex, DeviceDefinitions.NUMBER_INVALID, ref BufferLastIndex);
            return result;
        }
        private FUNC_RESULT BufferLastIndexGetBaseDll(FUNC_RESULT Result, ref int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            BufferLastIndex = (int)this._dllWrapper.BufferLastIndexGet();
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        private FUNC_RESULT BufferLastIndexGetBaseCheck(FUNC_RESULT Result, ref int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (BufferLastIndex > DeviceDefinitions.BUFFER_LAST_INDEX_MAX) { BufferLastIndex = DeviceDefinitions.BUFFER_LAST_INDEX_MAX; return FUNC_RESULT.SUCCESS; }
            else if (BufferLastIndex > 0) return FUNC_RESULT.SUCCESS;
            else return FUNC_RESULT.FAIL;
        }
        //End : Buffer new Index read

        //Buffer new Index read
        private FUNC_RESULT BufferMessageGetWorkflow(int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.BufferMessageGetExecute(result, (int)OPERATION_DELAY_MS.BUFFER_UPDATE, BufferLastIndex, ref BufferMessage, ref BufferMessageLen);
            this.XXXSetValue(result, BufferMessage, DeviceDefinitions.STRING_INVALID, ref BufferMessage, ref this._runtimeBuffer.BufferMessage, InternalValueSet);
            this.XXXSetValue(result, BufferMessageLen, DeviceDefinitions.NUMBER_INVALID, ref BufferMessageLen, ref this._runtimeBuffer.BufferMessageLen, InternalValueSet);
            return result;
        }
        private FUNC_RESULT BufferMessageGetExecute(FUNC_RESULT Result, int Delay, int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.BufferMessageGetBase(BufferLastIndex, ref BufferMessage, ref BufferMessageLen);
        }
        private FUNC_RESULT BufferMessageGetBase(int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.BufferMessageGetBaseCheck(result, BufferLastIndex);
            result = this.BufferMessageGetBaseDll(result, BufferLastIndex, ref BufferMessage, ref BufferMessageLen);
            this.XXXBaseSetValue(result, BufferMessage, DeviceDefinitions.STRING_INVALID, ref BufferMessage);
            this.XXXBaseSetValue(result, BufferMessageLen, DeviceDefinitions.NUMBER_INVALID, ref BufferMessageLen);
            return result;
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
            this._dllWrapper.BufferRead(ref BufferLastIndex, out BufferMessage, ref BufferMessageLen);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : Buffer new Index read

        //Buffer Update
        //Buffer Update Main
        private FUNC_RESULT BufferUpdate(BUFFER_UPDATE_SOURCE BufferUpdateSource, ref BUFFER_UPDATE_TYPE BufferUpdateType)
        {
            if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.UNKNOWN) return FUNC_RESULT.MAJOR_ERROR;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            if (BufferUpdateSource != BUFFER_UPDATE_SOURCE.WORKFLOW) this.BufferUpdateAnalyzeRemoveExtra(result , null);
            result = this.BufferTimeoutCheck(result, BufferUpdateSource, BufferUpdateType);
            result = this.BufferUpdateList(result, BufferUpdateSource);
            result = this.BufferUpdateAnalyze(result, BufferUpdateSource, ref BufferUpdateType);
            return result;
        }
        //End : Buffer Update Main
        //Remove Null and Not Usable BufferGroup[...].BaferData[...]
        private void BufferUpdateAnalyzeRemoveExtra(FUNC_RESULT Result, BufferData targetBufferData)
        {
            if (Result != FUNC_RESULT.SUCCESS && targetBufferData == null) return;
            this.BufferUpdateAnalyzeRemoveAllNull();
            if (targetBufferData == null)
                foreach (BufferDataGroup group in this._runtimeBuffer.BufferDataGroup.AsEnumerable().Reverse())
                {
                    foreach (BufferData bufferData in group.BufferData.AsEnumerable().Reverse()) if (bufferData != null) { targetBufferData = bufferData; break; }
                    if (targetBufferData != null) break;
                }
            if (targetBufferData == null) return;
            bool IsPreservedAction(BUFFER_ACTION action) => action == BUFFER_ACTION.MODEM_STARTUP || action == BUFFER_ACTION.MODEM_READY || action == BUFFER_ACTION.SMS_RECEIVED ||
                action == BUFFER_ACTION.SMS_DELIVERY_REPORT;
            bool preserveTarget = IsPreservedAction(targetBufferData.Action);
            for (int groupIndex = 0; groupIndex < this._runtimeBuffer.BufferDataGroup.Count; groupIndex++)
            {
                BufferDataGroup group = this._runtimeBuffer.BufferDataGroup[groupIndex];int targetIndex = -1;
                for (int dataIndex = 0; dataIndex < group.BufferData.Count; dataIndex++)
                    if (ReferenceEquals(group.BufferData[dataIndex], targetBufferData)) { targetIndex = dataIndex; break; }
                if (targetIndex > -1)
                {
                    group.BufferData.RemoveAt(targetIndex);
                    for (int dataIndex = targetIndex - 1; dataIndex >= 0; dataIndex--)
                        if (!IsPreservedAction(group.BufferData[dataIndex].Action)) group.BufferData.RemoveAt(dataIndex);
                    break;
                }
                for (int dataIndex = group.BufferData.Count - 1; dataIndex >= 0; dataIndex--)
                    if (!IsPreservedAction(group.BufferData[dataIndex].Action)) group.BufferData.RemoveAt(dataIndex);
            }
        }
        private void BufferUpdateAnalyzeRemoveAllNull()
        {
            for (int groupIndex = this._runtimeBuffer.BufferDataGroup.Count - 1; groupIndex >= 0; groupIndex--)
                for (int dataIndex = this._runtimeBuffer.BufferDataGroup[groupIndex].BufferData.Count - 1; dataIndex >= 0; dataIndex--)
                    if (this._runtimeBuffer.BufferDataGroup[groupIndex].BufferData[dataIndex] == null) this._runtimeBuffer.BufferDataGroup[groupIndex].BufferData.RemoveAt(dataIndex);
            for (int groupIndex = this._runtimeBuffer.BufferDataGroup.Count - 1; groupIndex >= 0; groupIndex--)
                if (this._runtimeBuffer.BufferDataGroup[groupIndex].BufferData.Count == 0) this._runtimeBuffer.BufferDataGroup.RemoveAt(groupIndex);
        }
        //End : Remove Null and Not Usable BufferGroup[...].BaferData[...]
        //Buffer TimeOut Check
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
        //End : Buffer TimeOut Check
        //Buffer Update List
        private FUNC_RESULT BufferUpdateList(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.BufferUpdateRawList(result, BufferUpdateSource);
            result = this.BufferUpdateDataList(result);
            return BufferUpdateListResult(result , BufferUpdateSource);
        }
        //Buffer Update RAW List
        private FUNC_RESULT BufferUpdateRawList(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource)
        {
            if(Result != FUNC_RESULT.SUCCESS) return Result;
            int bufferLastIndex = DeviceDefinitions.NUMBER_INVALID;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.LastBufferIndexGetExecute(result, (int)OPERATION_DELAY_MS.BUFFER_UPDATE, ref bufferLastIndex);
            this.BufferUpdateTimeSet(result, BufferUpdateSource);
            result = this.BufferUpdateListCheck(result, bufferLastIndex);
            result = this.BufferUpdateListSetData(result, bufferLastIndex);
            if (result == FUNC_RESULT.SUCCESS) result = this.BufferClearExecute(result, (int)OPERATION_DELAY_MS.BUFFER_CLEAR);
            else this.BufferClearExecute(FUNC_RESULT.SUCCESS, (int)OPERATION_DELAY_MS.BUFFER_CLEAR);
            this.XXXSetValue(false, ref this._runtimeBuffer.BufferNewData, true);
            this.XXXSetValue(DeviceDefinitions.NUMBER_INVALID, ref this._runtimeBuffer.BufferLastIndex, true);
            return result;
        }
        private void BufferUpdateTimeSet(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource)
        {
            if (Result != FUNC_RESULT.SUCCESS) return;
            else this._runtimeBuffer.BufferLastUpdate = DateTime.Now;
            if (BufferUpdateSource != BUFFER_UPDATE_SOURCE.WORKFLOW) this._runtimeBuffer.BufferLastUpdatePerior = this._runtimeBuffer.BufferLastUpdate;
        }
        private FUNC_RESULT BufferUpdateListCheck(FUNC_RESULT Result, int BufferLastIndexNew)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (BufferLastIndexNew > DeviceDefinitions.BUFFER_LAST_INDEX_INVALID) return FUNC_RESULT.SUCCESS;
            else return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT BufferUpdateListSetData(FUNC_RESULT Result, int BufferLastIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            DateTime currentTime = DateTime.Now;
            FUNC_RESULT resultTemp = FUNC_RESULT.UNKNOWN, result = FUNC_RESULT.FAIL;
            for (int bufferIndex = 1; bufferIndex <= BufferLastIndex && resultTemp != FUNC_RESULT.MAJOR_ERROR; bufferIndex++)
            {
                int bufferMessageLen = DeviceDefinitions.NUMBER_INVALID;
                string bufferMessage = DeviceDefinitions.STRING_INVALID;
                resultTemp = FUNC_RESULT.UNKNOWN;
                for (int j = 0; j < (int)TRY_COUNT.BUFFER_COUNT_TRY && resultTemp != FUNC_RESULT.SUCCESS && resultTemp != FUNC_RESULT.MAJOR_ERROR; j++)
                    resultTemp = this.BufferMessageGetExecute(FUNC_RESULT.SUCCESS, (int)OPERATION_DELAY_MS.BUFFER_UPDATE_CONTINUE, bufferIndex, ref bufferMessage, ref bufferMessageLen);
                if (resultTemp == FUNC_RESULT.SUCCESS)
                { this._runtimeBuffer.BufferDataRaw.Add(new BufferRawData { Message = bufferMessage, MessageLen = bufferMessageLen, ReceivedTime = currentTime }); result = FUNC_RESULT.SUCCESS; }
            }
            if (resultTemp == FUNC_RESULT.MAJOR_ERROR) result = FUNC_RESULT.MAJOR_ERROR;
            return result;
        }
        //End : Buffer Update RAW List
        //BufferDataGroup Update
        private FUNC_RESULT BufferUpdateDataList(FUNC_RESULT Result)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            for (int i = 0 ; i < this._runtimeBuffer.BufferDataRaw.Count; i++)
            {
                this._runtimeBuffer.BufferDataGroup.Add(StringAnalyzer.BufferAnalyzeRawMessage(this._runtimeBuffer.BufferDataRaw[i]));
            }
            this._runtimeBuffer.BufferDataRaw.Clear();
            result = FUNC_RESULT.SUCCESS;
            return result;
        }
        //End : BufferDataGroup Update
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
        private FUNC_RESULT BufferUpdateAnalyze(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource, ref BUFFER_UPDATE_TYPE BufferUpdateType)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            BufferData successBufferData = new BufferData();
            if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY) result = FUNC_RESULT.SUCCESS;
            else if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.WORKFLOW || BufferUpdateSource == BUFFER_UPDATE_SOURCE.WORKFLOW_START)
            { result = this.BufferUpdateAnalyzeWorkFlow(ref BufferUpdateType, out successBufferData); this.BufferUpdateAnalyzeRemoveExtra(result , successBufferData); }
            else if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.TIMER) result = this.BufferUpdateAnalyzeTimer();
            else result = FUNC_RESULT.MAJOR_ERROR;
            this.BufferUpdateAnalyzeRemoveExtra(result , successBufferData);
            return result;
        }
        private FUNC_RESULT BufferUpdateAnalyzeWorkFlow(ref BUFFER_UPDATE_TYPE BufferUpdateType, out BufferData SuccessBufferData)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            SuccessBufferData = null;
            foreach (BufferData bufferData in this._runtimeBuffer.BufferDataGroup.SelectMany(group => group.BufferData).Where(data => data.ReceivedTime >= this._runtimeBuffer.BufferLastUpdatePerior))
            {
                switch (BufferUpdateType)
                {
                    case BUFFER_UPDATE_TYPE.DEVICE_NUMBER_GET:
                    case BUFFER_UPDATE_TYPE.GSM_POWER_GET:
                    case BUFFER_UPDATE_TYPE.GSM_POWER_SET:
                        {result = bufferData.Status == BUFFER_ACTION_STATUS.SUCCEEDED ? FUNC_RESULT.SUCCESS : FUNC_RESULT.FAIL; break; }

                    case BUFFER_UPDATE_TYPE.MODEM_STARTUP:
                        {
                            if (bufferData.Action == BUFFER_ACTION.MODEM_READY) { result = FUNC_RESULT.SUCCESS; BufferUpdateType = BUFFER_UPDATE_TYPE.MODEM_READY; }
                            else { result = bufferData.Action == BUFFER_ACTION.MODEM_STARTUP ? FUNC_RESULT.SUCCESS : FUNC_RESULT.FAIL; }
                            break;
                        }

                    case BUFFER_UPDATE_TYPE.MODEM_READY:
                        { result = bufferData.Action == BUFFER_ACTION.MODEM_READY ? FUNC_RESULT.SUCCESS : FUNC_RESULT.FAIL; break; }

                    case BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY:
                        if (bufferData.Action == BUFFER_ACTION.MODEM_STARTUP) { result = FUNC_RESULT.SUCCESS; BufferUpdateType = BUFFER_UPDATE_TYPE.MODEM_STARTUP; }
                        else if (bufferData.Action == BUFFER_ACTION.MODEM_READY) { result = FUNC_RESULT.SUCCESS; BufferUpdateType = BUFFER_UPDATE_TYPE.MODEM_READY; }
                        else result = FUNC_RESULT.FAIL;
                        break;

                    default:
                        result = FUNC_RESULT.UNKNOWN;break;
                }
                if (result == FUNC_RESULT.SUCCESS) { SuccessBufferData = bufferData; break; }
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

        private FUNC_RESULT XXXBufferCheck(FUNC_RESULT Result, ref BUFFER_UPDATE_SOURCE BufferUpdateSource, BUFFER_UPDATE_TYPE BufferUpdateType)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            BUFFER_UPDATE_TYPE bufferUpdateType = BufferUpdateType;
            do
            {
                result = this.BufferUpdate(BufferUpdateSource, ref bufferUpdateType);
                if (result == FUNC_RESULT.SUCCESS)
                    switch (bufferUpdateType)
                    {
                        case BUFFER_UPDATE_TYPE.MODEM_STARTUP_READY:
                        case BUFFER_UPDATE_TYPE.MODEM_READY:
                            { this._runtimeInfo.GsmStatus.ModemStartup = true; this._runtimeInfo.GsmStatus.ModemStartup = true; break; }

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
        //End : Buffer Update

    }
}
