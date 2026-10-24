using GSM.Core.Source.Common;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        //Device Count
        private FUNC_RESULT DeviceCountGetWorkflow(ref int DeviceCount, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.DeviceCountGetExecute(result, (int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.SHORT, ref DeviceCount);
            this.XXXSetValue(result, DeviceCount, DeviceDefinitions.NUMBER_INVALID, ref DeviceCount, ref this._runtimeIdentity.DeviceCount, InternalValueSet);
            return result;
        }
        private FUNC_RESULT DeviceCountGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref int DeviceCount)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR, deviceCount = DeviceDefinitions.NUMBER_INVALID;
            while (loopCount-- > 0 && (result1 != FUNC_RESULT.SUCCESS || result2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (result1 != FUNC_RESULT.SUCCESS)
                {
                    result1 = this.DeviceCountGetBase(ref DeviceCount); if (result1 != FUNC_RESULT.SUCCESS && --resultCount <= 0) break;
                }
                else if (this._runtimeConfig.EnableDoubleReadValidation) result2 = this.DeviceCountGetBase(ref deviceCount);
                else break;
                if (result1 == FUNC_RESULT.MAJOR_ERROR || result2 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, result1, result2, ref DeviceCount, deviceCount, DeviceDefinitions.NUMBER_INVALID);
        }
        private FUNC_RESULT DeviceCountGetBase(ref int DeviceCount)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.DeviceCountGetBaseDll(result, ref DeviceCount);
            result = this.DeviceCountGetBaseCheck(result, DeviceCount);
            this.XXXBaseSetValue(result , DeviceCount, DeviceDefinitions.NUMBER_INVALID, ref DeviceCount);
            return result;
        }
        private FUNC_RESULT DeviceCountGetBaseDll(FUNC_RESULT Result, ref int DeviceCount)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            DeviceCount = this._dllWrapper.DeviceCount();
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        private FUNC_RESULT DeviceCountGetBaseCheck(FUNC_RESULT Result, int DeviceCount)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            try
            {
                if (DeviceCount >= DeviceDefinitions.DEVICE_COUNT_MIN && DeviceCount <= DeviceDefinitions.DEVICE_COUNT_MAX) return FUNC_RESULT.SUCCESS;
                else
                {
                    this.ErrorSet(ERROR_CODE.DEVICE_COUNT, "Device Count = " + DeviceCount.ToString()); return FUNC_RESULT.FAIL;
                }
            }
            catch (Exception ex)
            {
                this.ErrorSet(ERROR_CODE.DEVICE_COUNT, "DeviceCountGetCheck Error : " + ex.Message); return FUNC_RESULT.FAIL;
            }
        }
        //End : Device Count

        //Device Select By Index
        private FUNC_RESULT DeviceSelectByIndexWorkflow(int DeviceIndex, bool CheckDeviceNumber, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.DeviceSelectByIndexExecute(result, DeviceIndex, (int)OPERATION_DELAY_MS.SHORT);
            if (CheckDeviceNumber)
            {
                int deviceNumber = DeviceDefinitions.NUMBER_INVALID;
                result = this.DeviceNumberGetExecute(result, (int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref deviceNumber);
                this.XXXSetValue(result, deviceNumber, DeviceDefinitions.NUMBER_INVALID, ref deviceNumber, ref this._runtimeIdentity.DeviceNumber, InternalValueSet);
            }
            this.XXXSetValue(result, DeviceIndex, DeviceDefinitions.NUMBER_INVALID, ref DeviceIndex, ref this._runtimeIdentity.DeviceIndex, InternalValueSet);
            return result;
        }
        private FUNC_RESULT DeviceSelectByIndexExecute(FUNC_RESULT Result, int DeviceIndex, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.DeviceSelectByIndexBase(DeviceIndex);
        }
        private FUNC_RESULT DeviceSelectByIndexBase(int DeviceIndex)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.DeviceSelectByIndexBaseInputCheck(result, DeviceIndex);
            result = this.DeviceSelectByIndexBaseDll(result, DeviceIndex);
            return result;
        }
        private FUNC_RESULT DeviceSelectByIndexBaseInputCheck(FUNC_RESULT Result, int DeviceIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (!this.DeviceIndexRangeCheck(DeviceIndex))
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Index = " + DeviceIndex.ToString() + " -> out of range"); return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT DeviceSelectByIndexBaseDll(FUNC_RESULT Result, int DeviceIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            this._dllWrapper.SelectByDeviceIndex(Convert.ToInt16(DeviceIndex));
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        /*
         * 1. Importan Note : Index limited from 0 to 127 in Manshor software test
        */
        //End : Device Select By Index
        //Device Select By Device Number
        private FUNC_RESULT DeviceSelectByDeviceNumberWorkflow(int DeviceNumber, bool CheckDeviceNumber, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            int oldDeviceNumber = this._runtimeIdentity.DeviceNumber;
            result = this.DeviceSelectByDeviceNumberExecute(result, DeviceNumber, (int)OPERATION_DELAY_MS.SHORT);
            if (CheckDeviceNumber) result = this.DeviceNumberGetExecute(result, (int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref DeviceNumber);
            if (result == FUNC_RESULT.SUCCESS && CheckDeviceNumber && DeviceNumber != oldDeviceNumber) result = FUNC_RESULT.MAJOR_ERROR;
            this.XXXSetValue(result, DeviceNumber, DeviceDefinitions.NUMBER_INVALID, ref DeviceNumber, ref this._runtimeIdentity.DeviceNumber, InternalValueSet);
            return result;
        }
        private FUNC_RESULT DeviceSelectByDeviceNumberExecute(FUNC_RESULT Result, int DeviceNumber, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.DeviceSelectByDeviceNumberBase(DeviceNumber);
        }
        private FUNC_RESULT DeviceSelectByDeviceNumberBase(int DeviceNumber)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.DeviceSelectByDeviceNumberBaseInputCheck(result, DeviceNumber);
            result = this.DeviceSelectByDeviceNumberBaseDll(result, DeviceNumber);
            return result;
        }
        private FUNC_RESULT DeviceSelectByDeviceNumberBaseInputCheck(FUNC_RESULT Result, int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (!this.DeviceNumberRangeCheck(DeviceNumber))
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Number = " + DeviceNumber.ToString() + " -> out of range"); return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT DeviceSelectByDeviceNumberBaseDll(FUNC_RESULT Result, int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS)
                return Result;
            this._dllWrapper.SelectByDeviceNumber(Convert.ToInt16(DeviceNumber));
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        /*
         * 1. Important Note : Device Number from 0 to 127 , after 127 it is changed but can not select by device number
        */
        //End : Device Select By Device Number

        //Device Number Get
        private FUNC_RESULT DeviceNumberGetWorkflow(ref int DeviceNumber, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.DeviceNumberGetExecute(result, !this._runtimeConfig.BufferCheck ? (int)TRY_COUNT.NORMAL : (int)TRY_COUNT.BUFFER_CHECK, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref DeviceNumber);
            this.XXXSetValue(result, DeviceNumber, DeviceDefinitions.NUMBER_INVALID, ref DeviceNumber, ref this._runtimeIdentity.DeviceNumber, InternalValueSet);
            return result;
        }
        private FUNC_RESULT DeviceNumberGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (!this._runtimeConfig.BufferCheck) return this.DeviceNumberGetExecuteNormal(loopCount, Delay, ref DeviceNumber); 
            else return this.DeviceNumberGetExecuteBufferCheck(loopCount, Delay, ref DeviceNumber);
        }
        private FUNC_RESULT DeviceNumberGetExecuteNormal(int loopCount, int Delay, ref int DeviceNumber)
        {
            FUNC_RESULT result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            int deviceNumber = DeviceDefinitions.NUMBER_INVALID;
            while (loopCount-- > 0 && (result1 != FUNC_RESULT.SUCCESS || result2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (result1 != FUNC_RESULT.SUCCESS) result1 = this.DeviceNumberGetBase(ref DeviceNumber);
                else if (this._runtimeConfig.EnableDoubleReadValidation) result2 = this.DeviceNumberGetBase(ref deviceNumber);
                else break;
                if (result1 == FUNC_RESULT.MAJOR_ERROR || result2 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, result1, result2, ref DeviceNumber, deviceNumber, DeviceDefinitions.NUMBER_INVALID);
        }
        private FUNC_RESULT DeviceNumberGetExecuteBufferCheck(int loopCount, int Delay, ref int DeviceNumber)
        {
            BUFFER_UPDATE_SOURCE bufferUpdateSource = BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            while (loopCount-- > 0 && result != FUNC_RESULT.SUCCESS && result != FUNC_RESULT.MAJOR_ERROR)
            {
                Thread.Sleep(Delay);
                if (bufferUpdateSource == BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY) this.XXXBufferCheck(ref bufferUpdateSource, nameof(this.DeviceNumberGetExecuteBufferCheck));
                result = this.DeviceNumberGetBase(ref DeviceNumber);
                if (result == FUNC_RESULT.SUCCESS) result = this.XXXBufferCheck(ref bufferUpdateSource, nameof(this.DeviceNumberGetExecuteBufferCheck));
            }
            return result;
        }
        private FUNC_RESULT DeviceNumberGetBase(ref int DeviceNumber)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.DeviceNumberGetBaseDll(result, ref DeviceNumber);
            result = this.DeviceNumberGetBaseCheck(result, DeviceNumber);
            this.XXXBaseSetValue(result, DeviceNumber, DeviceDefinitions.NUMBER_INVALID, ref DeviceNumber);
            return result;
        }
        private FUNC_RESULT DeviceNumberGetBaseDll(FUNC_RESULT Result, ref int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this._dllWrapper.DeviceNumberGet(ref DeviceNumber)) return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.DEVICE_NUMBER_GET, "Device Number = " + DeviceNumber.ToString());
            return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT DeviceNumberGetBaseCheck(FUNC_RESULT Result, int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (!this.DeviceNumberRangeCheck(DeviceNumber))
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Number = " + DeviceNumber.ToString() + " -> out of range"); return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        //End : Device Number Get

        //Device Number Set
        private FUNC_RESULT DeviceNumberSetWorkflow(int DeviceNumber, bool DeviceNumberCheck, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.DeviceNumberSetExecute(result, DeviceNumber, (int)OPERATION_DELAY_MS.SHORT);
            if (DeviceNumberCheck)
            {
                int deviceNumber = DeviceDefinitions.NUMBER_INVALID;
                result = this.DeviceSelectByIndexExecute(result, this._runtimeIdentity.DeviceIndex, (int)OPERATION_DELAY_MS.AFTER_DEVICE_NUM_CHANGE);
                result = this.DeviceNumberGetExecute(result, (int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref deviceNumber);
                if (result == FUNC_RESULT.SUCCESS && DeviceNumber != deviceNumber) result = FUNC_RESULT.MAJOR_ERROR;
            }
            this.XXXSetValue(result, DeviceNumber, DeviceDefinitions.NUMBER_INVALID, ref DeviceNumber, ref this._runtimeIdentity.DeviceNumber, InternalValueSet);
            return result;
        }
        private FUNC_RESULT DeviceNumberSetExecute(FUNC_RESULT Result, int DeviceNumber, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.DeviceNumberSetBase(DeviceNumber);
        }
        private FUNC_RESULT DeviceNumberSetBase(int DeviceNumber)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.DeviceNumberSetBaseInputCheck(result, DeviceNumber);
            result = this.DeviceNumberSetBaseDll(result, DeviceNumber);
            return result;
        }
        private FUNC_RESULT DeviceNumberSetBaseInputCheck(FUNC_RESULT Result, int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (!this.DeviceNumberRangeCheck(DeviceNumber))
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Number = " + DeviceNumber.ToString()); return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT DeviceNumberSetBaseDll(FUNC_RESULT Result, int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            this._dllWrapper.DeviceNumberSet(DeviceNumber);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : Device Number Set

        //DeviceManager -> Discovery Scope
        //Device Identity
        private FUNC_RESULT DeviceIdentityComplexWorkflow(int DeviceIndex, ref int DeviceNumber, ref string IMEI)
        {
            bool internalValueSet = true, checkDeviceNumber = true, gsmIsConnect = false;
            int tempInt = 0;
            string tempStr = string.Empty;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            result = this.DeviceSelectByIndexWorkflow(DeviceIndex, checkDeviceNumber, internalValueSet);
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.GSMIsConnectWorkflow(ref gsmIsConnect, internalValueSet);
                if (gsmIsConnect)
                    result = this.IMEIGetWorkflow(ref IMEI, internalValueSet);
                else if (result == FUNC_RESULT.FAIL)
                    {
                        result = this.DeviceRestartComplexWorkFlow();
                        if (result == FUNC_RESULT.SUCCESS)
                        {
                            result = this.GSMIsConnectWorkflow(ref gsmIsConnect, internalValueSet);
                            if (result == FUNC_RESULT.SUCCESS) result = this.IMEIGetWorkflow(ref IMEI, internalValueSet);
                        }
                    }
            }
            this.XXXSetValue(result, this._runtimeIdentity.DeviceNumber, DeviceDefinitions.NUMBER_INVALID, ref DeviceNumber, ref tempInt, internalValueSet);
            this.XXXSetValue(result, this._runtimeHardware.IMEI, DeviceDefinitions.STRING_INVALID, ref IMEI, ref tempStr, internalValueSet);
            return result;
        }
        //End : Device Identity
        //End : DeviceManager -> Discovery Scope

        //DeviceManager -> Device wroking scope

        //Device Index Unselect
        private FUNC_RESULT DeviceUnselectComplexWorkflow()
        {
            bool internalValueSet = false, checkDeviceNumber = false;
            return this.DeviceSelectByIndexWorkflow(DeviceDefinitions.DEVICE_INDEX_MAX, checkDeviceNumber, internalValueSet);
        }
        //End : Device Index Unselect


        //Device Index
        private bool DeviceIndexRangeCheck(int DeviceIndex)
        {
            if (DeviceIndex >= DeviceDefinitions.DEVICE_INDEX_MIN && DeviceIndex <= DeviceDefinitions.DEVICE_INDEX_MAX) return true;
            else return false;
        }
        //End : Device Index
        //Device Number
        private bool DeviceNumberRangeCheck(int DeviceNumber)
        {
            if (DeviceNumber >= DeviceDefinitions.DEVICE_NUMBER_MIN && DeviceNumber <= DeviceDefinitions.DEVICE_NUMBER_MAX) return true;
            else return false;
        }
        //End : Device Number

    }
}
