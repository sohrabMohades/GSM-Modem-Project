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
                result = this.BufferLastIndexGetBaseCheck(ref bufferLastIndex);
            }
            this.XXXBaseSetValue(result, bufferLastIndex, DeviceDefinitions.NUMBER_INVALID, ref BufferLastIndex);
            return result;
        }
        private FUNC_RESULT BufferLastIndexGetBaseDll(ref int BufferLastIndex)
        {
            BufferLastIndex = (int)this._dllWrapper.BufferLastIndexGet();
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        private FUNC_RESULT BufferLastIndexGetBaseCheck(ref int BufferLastIndex)
        {
            if (BufferLastIndex > DeviceDefinitions.BUFFER_LAST_INDEX_MAX)
            {
                BufferLastIndex = DeviceDefinitions.BUFFER_LAST_INDEX_MAX;
                return FUNC_RESULT.SUCCESS;
            }
            else if (BufferLastIndex > 0)
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
        private FUNC_RESULT BufferUpdate(BUFFER_UPDATE_SOURCE BufferUpdateSource, string WorkFlowName)
        {
            if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.UNKNOWN)
                return FUNC_RESULT.MAJOR_ERROR;
            //Variable Initial
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            //End : Variable Initial

            //Time Set and Check
            if (BufferUpdateSource != BUFFER_UPDATE_SOURCE.WORKFLOW)
                this._runtimeBuffer.BufferLastUpdatePerior = this._runtimeBuffer.BufferLastUpdate;
            this._runtimeBuffer.BufferLastUpdate = DateTime.Now;
            result = this.BufferTimeoutCheck(BufferUpdateSource , WorkFlowName);
            if (result != FUNC_RESULT.SUCCESS)
                return result;
            //End : Time Set and Check

            //Layer 1
            result = this.BufferUpdateList();
            //End : Layer 1

            //Layer 2 -> if Analyze allow
            if (result == FUNC_RESULT.SUCCESS && BufferUpdateSource != BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY)
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
            if (result == FUNC_RESULT.SUCCESS && bufferLastIndex > DeviceDefinitions.BUFFER_LAST_INDEX_INVALID)
                result = FUNC_RESULT.SUCCESS;
            else
                result = FUNC_RESULT.FAIL;
            if (result == FUNC_RESULT.SUCCESS)
                result = this.BufferUpdateListCheck(result, bufferLastIndex);
            //End : get Buffer data and check data is available

            //ADD Buffer RAW List
            if (result == FUNC_RESULT.SUCCESS)
                result = this.BufferUpdateListSetData(result, bufferLastIndex);
            //End : ADD Buffer RAW List

            //Buffer Clear -> buffer must clear with any result
            if (result == FUNC_RESULT.SUCCESS)
                result = this.BufferClearExecute((int)OPERATION_DELAY_MS.BUFFER_CLEAR);
            else
                this.BufferClearExecute((int)OPERATION_DELAY_MS.BUFFER_CLEAR);
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
            DateTime currentTime = DateTime.Now;
            for (int i = 1; i <= BufferLastIndex; i++)
            {
                FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
                for (int j = 0; j < countTry && result != FUNC_RESULT.SUCCESS && result != FUNC_RESULT.MAJOR_ERROR; j++)
                    result = this.BufferMessageGetExecute((int)OPERATION_DELAY_MS.BUFFER_UPDATE_CONTINUE, i, ref bufferMessage, ref bufferMessageLen);
                if (result == FUNC_RESULT.MAJOR_ERROR)
                    return FUNC_RESULT.MAJOR_ERROR;
                else if (result == FUNC_RESULT.SUCCESS)
                    this._runtimeBuffer.BufferDataRaw.Add(new BufferRawData { Message = bufferMessage, MessageLen = bufferMessageLen, ReceivedTime = currentTime });
            }
            return FUNC_RESULT.SUCCESS;
        }
        //End : Buffer Update RAW List
        //Buffer Update Data List
        private FUNC_RESULT BufferUpdateDataList()
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            for (int i = this._runtimeBuffer.BufferDataRaw.Count -1 ; i >= 0; i--)
            {
                this._runtimeBuffer.BufferDataGroup.Add(StringAnalyzer.BufferAnalyzeRawMessage(this._runtimeBuffer.BufferDataRaw[i]));
                this._runtimeBuffer.BufferDataRaw.RemoveAt(i);
            }
            result = FUNC_RESULT.SUCCESS;
            return result;
        }
        //End : Buffer Update Data List
        //End : Buffer Update List
        //Buffer Update Analyze
        private FUNC_RESULT BufferUpdateAnalyze(BUFFER_UPDATE_SOURCE BufferUpdateSource, string WorkFlowName)
        {
            if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY)
                return FUNC_RESULT.SUCCESS;
            else if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.WORKFLOW || BufferUpdateSource == BUFFER_UPDATE_SOURCE.WORKFLOW_START)
                return this.BufferUpdateAnalyzeWorkFlow(WorkFlowName);
            else if (BufferUpdateSource == BUFFER_UPDATE_SOURCE.TIMER)
                return this.BufferUpdateAnalyzeTimer();
            else
                return FUNC_RESULT.MAJOR_ERROR;
        }
        private FUNC_RESULT BufferUpdateAnalyzeWorkFlow(string WorkFlowName)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            foreach (BufferData bufferData in this._runtimeBuffer.BufferDataGroup.SelectMany(group => group.BufferData).Where(data => data.ReceivedTime >= this._runtimeBuffer.BufferLastUpdatePerior))
            {
                switch (WorkFlowName)
                {
                    case nameof(this.DeviceSelectByIndexExecute):
                        {
                            if (bufferData.Status == BUFFER_ACTION_STATUS.SUCCEEDED)
                                return FUNC_RESULT.SUCCESS;
                            else
                                result = FUNC_RESULT.FAIL;
                            break;
                        }

                    default:
                        result = FUNC_RESULT.UNKNOWN;
                        break;
                }
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
        private FUNC_RESULT BufferTimeoutCheck(BUFFER_UPDATE_SOURCE BufferUpdateSource, string WorkFlowName)
        {
            if (BufferUpdateSource != BUFFER_UPDATE_SOURCE.WORKFLOW)
                return FUNC_RESULT.SUCCESS;
            
            int timeout = (int)(this._runtimeBuffer.BufferLastUpdate - this._runtimeBuffer.BufferLastUpdatePerior).TotalMilliseconds;
            if (timeout > this._runtimeConfig.TimeoutMaxMs)
                return FUNC_RESULT.MAJOR_ERROR;
            switch (WorkFlowName)
            {
                case nameof(this.DeviceSelectByIndexExecute):
                    if(timeout <= (int)BUFFER_TIME_OUT_MS.NORMAL_LONG)
                        return FUNC_RESULT.SUCCESS;
                    else
                        return FUNC_RESULT.MAJOR_ERROR;
                default:
                    return FUNC_RESULT.UNKNOWN;
            }
        }
        //End : Buffer TimeOut Check
        //End : Buffer Update

    }
}
