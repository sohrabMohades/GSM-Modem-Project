using GSM.Core.Source.Common;
using System;
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
            result = this.BufferMessageGetBaseDll(result, BufferLastIndex, ref BufferMessage, ref BufferMessageLen);
            this.XXXBaseSetValue(result, BufferMessage, DeviceDefinitions.STRING_INVALID, ref BufferMessage);
            this.XXXBaseSetValue(result, BufferMessageLen, DeviceDefinitions.NUMBER_INVALID, ref BufferMessageLen);
            return result;
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
        private FUNC_RESULT BufferUpdate(BUFFER_UPDATE_SOURCE BufferUpdateSource, string WorkFlowName)
        {
            if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.UNKNOWN) return FUNC_RESULT.MAJOR_ERROR;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.BufferTimeoutCheck(result, BufferUpdateSource, WorkFlowName);
            result = this.BufferUpdateList(result, BufferUpdateSource);
            result = this.BufferUpdateAnalyze(result, ref BufferUpdateSource, WorkFlowName);
            return result;
        }
        //End : Buffer Update Main
        //Buffer Update List
        private FUNC_RESULT BufferUpdateList(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.BufferUpdateRawList(result, BufferUpdateSource);
            result = this.BufferUpdateDataList(result);
            return result;
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
        //Buffer Update Data List
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
        //End : Buffer Update Data List
        //End : Buffer Update List
        //Buffer Update Analyze
        private FUNC_RESULT BufferUpdateAnalyze(FUNC_RESULT Result, ref BUFFER_UPDATE_SOURCE BufferUpdateSource, string WorkFlowName)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            else if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY) return FUNC_RESULT.SUCCESS;
            else if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.WORKFLOW || BufferUpdateSource == BUFFER_UPDATE_SOURCE.WORKFLOW_START) return this.BufferUpdateAnalyzeWorkFlow(WorkFlowName);
            else if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.TIMER) return this.BufferUpdateAnalyzeTimer();
            else return FUNC_RESULT.MAJOR_ERROR;
        }
        private FUNC_RESULT BufferUpdateAnalyzeWorkFlow(string WorkFlowName)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            foreach (BufferData bufferData in this._runtimeBuffer.BufferDataGroup.SelectMany(group => group.BufferData).Where(data => data.ReceivedTime >= this._runtimeBuffer.BufferLastUpdatePerior))
            {
                switch (WorkFlowName)
                {
                    case nameof(this.DeviceNumberGetExecuteBufferCheck):
                        {
                            if (bufferData.Status == BUFFER_ACTION_STATUS.SUCCEEDED) result = FUNC_RESULT.SUCCESS;
                            else result = FUNC_RESULT.FAIL;
                            break;
                        }

                    case nameof(this.GSMPowerGetExecuteBufferCheck):
                        {
                            if (bufferData.Status == BUFFER_ACTION_STATUS.SUCCEEDED) result = FUNC_RESULT.SUCCESS;
                            else result = FUNC_RESULT.FAIL;
                            break;
                        }

                    case nameof(this.GSMPowerSetExecuteBufferCheck):
                        {
                            if (bufferData.Status == BUFFER_ACTION_STATUS.SUCCEEDED) result = FUNC_RESULT.SUCCESS;
                            else result = FUNC_RESULT.FAIL;
                            break;
                        }

                    case nameof(this.BufferModemStartupReadyCheck):
                        {
                            if (!this._runtimeInfo.GsmStatus.ModemStartup)
                                if (bufferData.Action == BUFFER_ACTION.MODEM_STARTUP) result = FUNC_RESULT.SUCCESS;
                                else result = FUNC_RESULT.FAIL;
                            else if (!this._runtimeInfo.GsmStatus.ModemReady)
                                if (bufferData.Action == BUFFER_ACTION.MODEM_READY) result = FUNC_RESULT.SUCCESS;
                                else result = FUNC_RESULT.FAIL;
                            else result = FUNC_RESULT.UNKNOWN;
                            break;
                        }

                    default:
                        result = FUNC_RESULT.UNKNOWN;
                        break;
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
        //Buffer TimeOut Check
        private void BufferUpdateTimeSet(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource)
        {
            if (Result != FUNC_RESULT.SUCCESS) return;
            else this._runtimeBuffer.BufferLastUpdate = DateTime.Now;
            if (BufferUpdateSource != BUFFER_UPDATE_SOURCE.WORKFLOW) this._runtimeBuffer.BufferLastUpdatePerior = this._runtimeBuffer.BufferLastUpdate;
        }
        private FUNC_RESULT BufferTimeoutCheck(FUNC_RESULT Result, BUFFER_UPDATE_SOURCE BufferUpdateSource, string WorkFlowName)
        {
            if (BufferUpdateSource != BUFFER_UPDATE_SOURCE.WORKFLOW) return FUNC_RESULT.SUCCESS;
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            int timeout = (int)(DateTime.Now - this._runtimeBuffer.BufferLastUpdatePerior).TotalMilliseconds;
            if (timeout > this._runtimeConfig.TimeoutMaxMs) return FUNC_RESULT.MAJOR_ERROR;
            switch (WorkFlowName)
            {
                case nameof(this.DeviceNumberGetExecuteBufferCheck):
                    if(timeout <= (int)BUFFER_TIME_OUT_MS.NORMAL_SHORT) return FUNC_RESULT.SUCCESS;
                    else return FUNC_RESULT.MAJOR_ERROR;

                case nameof(this.GSMPowerGetExecuteBufferCheck):
                    if (timeout <= (int)BUFFER_TIME_OUT_MS.SHORT) return FUNC_RESULT.SUCCESS;
                    else return FUNC_RESULT.MAJOR_ERROR;

                case nameof(this.GSMPowerSetExecuteBufferCheck):
                    if (timeout <= (int)BUFFER_TIME_OUT_MS.SHORT) return FUNC_RESULT.SUCCESS;
                    else return FUNC_RESULT.MAJOR_ERROR;

                case nameof(this.BufferModemStartupReadyCheck):
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
        //End : Buffer Update

    }
}
