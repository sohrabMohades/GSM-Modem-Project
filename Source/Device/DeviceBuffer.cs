using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            result = this.BufferClearExecute((int)OPERATION_DELAY_MS.NORMAL_SHORT);
            this.XXXSetValue(result, DeviceDefinitions.NUMBER_INVALID, DeviceDefinitions.NUMBER_INVALID, ref tempInt, ref this._runtimeBuffer.BufferLastIndex, InternalValueSet);
            this.XXXSetValue(result, false, false, ref tempBool, ref this._runtimeBuffer.BufferNewData, InternalValueSet);
            this.XXXSetValue(result, DeviceDefinitions.STRING_INVALID, DeviceDefinitions.STRING_INVALID, ref tempStr, ref this._runtimeBuffer.BufferMessage, InternalValueSet);
            this.XXXSetValue(result, DeviceDefinitions.NUMBER_INVALID, DeviceDefinitions.NUMBER_INVALID, ref tempInt, ref this._runtimeBuffer.BufferMessageLen, InternalValueSet);
            return result;
        }
        private FUNC_RESULT BufferClearExecute(int Delay)
        {
            //Thread.Sleep(Delay);
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
            result = this.LastBufferIndexGetExecute((int)OPERATION_DELAY_MS.SHORT, ref bufferLastIndex);
            result = this.LastBufferIndexGetCheck(result, this._runtimeBuffer.BufferLastIndex, bufferLastIndex);
            this.XXXSetValue(result, bufferLastIndex, DeviceDefinitions.NUMBER_INVALID, ref BufferLastIndex, ref this._runtimeBuffer.BufferLastIndex, InternalValueSet);
            this.XXXSetValue(result, true, false, ref tempBool, ref this._runtimeBuffer.BufferNewData, InternalValueSet);
            return result;
        }
        private FUNC_RESULT LastBufferIndexGetExecute(int Delay, ref int BufferLastIndex)
        {
            //Thread.Sleep(Delay);
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
            result = this.BufferMessageGetExecute((int)OPERATION_DELAY_MS.SHORT, BufferLastIndex, ref bufferMessage, ref bufferMessageLen);
            this.XXXSetValue(result, bufferMessage, DeviceDefinitions.STRING_INVALID, ref BufferMessage, ref this._runtimeBuffer.BufferMessage, InternalValueSet);
            this.XXXSetValue(result, bufferMessageLen, DeviceDefinitions.NUMBER_INVALID, ref BufferMessageLen, ref this._runtimeBuffer.BufferMessageLen, InternalValueSet);
            return result;
        }
        private FUNC_RESULT BufferMessageGetExecute(int Delay, int BufferLastIndex, ref string BufferMessage, ref int BufferMessageLen)
        {
            //Thread.Sleep(Delay);
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

        //Buffer Last Data Update
        private FUNC_RESULT BufferLastDataUpdate()
        {
            int bufferLastIndex = DeviceDefinitions.NUMBER_INVALID;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            result = this.LastBufferIndexGetExecute((int)OPERATION_DELAY_MS.SHORT, ref bufferLastIndex);
            result = this.LastDataUpdateCheck(result, bufferLastIndex);
            result = this.LastDataUpdateSetData(result, bufferLastIndex);
            result = this.BufferClearExecute((int)OPERATION_DELAY_MS.NORMAL_SHORT);
            return result;
        }
        private FUNC_RESULT LastDataUpdateCheck(FUNC_RESULT Result, int BufferLastIndex)
        {
            if (Result == FUNC_RESULT.SUCCESS && BufferLastIndex > DeviceDefinitions.NUMBER_INVALID)
                return FUNC_RESULT.SUCCESS;
            return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT LastDataUpdateSetData(FUNC_RESULT Result, int BufferLastIndex)
        {
            int bufferMessageLen = DeviceDefinitions.NUMBER_INVALID, count = 3;
            string bufferMessage = DeviceDefinitions.STRING_INVALID;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            if (Result != FUNC_RESULT.SUCCESS)
                return FUNC_RESULT.FAIL;
            for (int i = 1; i <= BufferLastIndex; i++)
            {
                for (int j = 0; j < count && result != FUNC_RESULT.SUCCESS; j++)
                    result = this.BufferMessageGetExecute((int)OPERATION_DELAY_MS.SHORT, BufferLastIndex, ref bufferMessage, ref bufferMessageLen);
                if (Result != FUNC_RESULT.SUCCESS)
                    return FUNC_RESULT.MAJOR_ERROR;
                this._runtimeBuffer.BufferDataRaw.Add(bufferMessage);
            }
            return FUNC_RESULT.SUCCESS;
        }
        //End : Buffer Last Data Update
    }
}
