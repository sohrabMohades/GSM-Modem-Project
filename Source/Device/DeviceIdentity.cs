using GSM.Core.Source.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            try
            {
                this._logger.FuncStart(nameof(this.DeviceCountGetWorkflow), this.FuncType(nameof(this.DeviceCountGetWorkflow)));
                result = this.DeviceCountGetExecute(result, (int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.SHORT, ref DeviceCount);
                this.XXXSetValue(result, DeviceCount, DeviceDefinitions.NUMBER_INVALID, ref DeviceCount, ref this._runtimeIdentity.DeviceCount, InternalValueSet);
                return result;
            }
            finally { this._logger.FuncEnd(result, DeviceCount); }
        }
        private FUNC_RESULT DeviceCountGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref int DeviceCount)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR, deviceCount = DeviceDefinitions.NUMBER_INVALID;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceCountGetExecute), this.FuncType(nameof(this.DeviceCountGetExecute)));
                while (loopCount-- > 0 && (result1 != FUNC_RESULT.SUCCESS || result2 != FUNC_RESULT.SUCCESS))
                {
                    Thread.Sleep(Delay);
                    if (result1 != FUNC_RESULT.SUCCESS)
                    {
                        result1 = this.DeviceCountGetBase(ref DeviceCount); if (result1 != FUNC_RESULT.SUCCESS && --resultCount <= 0) break;
                    }
                    else if (this._runtimeConfig.EnableDoubleReadValidation) result2 = this.DeviceCountGetBase(ref deviceCount);
                    else break;
                    if (result1 == FUNC_RESULT.MAJOR_ERROR || result2 == FUNC_RESULT.MAJOR_ERROR) return result = FUNC_RESULT.MAJOR_ERROR;
                }
                return result = this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, result1, result2, ref DeviceCount, deviceCount, DeviceDefinitions.NUMBER_INVALID);
            }
            finally { this._logger.FuncEnd(result, deviceCount); }
        }
        private FUNC_RESULT DeviceCountGetBase(ref int DeviceCount)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceCountGetBase), this.FuncType(nameof(this.DeviceCountGetBase)));
                result = this.DeviceCountGetBaseDll(result, ref DeviceCount);
                result = this.DeviceCountGetBaseCheck(result, DeviceCount);
                this.XXXBaseSetValue(result, DeviceCount, DeviceDefinitions.NUMBER_INVALID, ref DeviceCount);
                return result;
            }
            finally { this._logger.FuncEnd(result, DeviceCount); }
        }
        private FUNC_RESULT DeviceCountGetBaseDll(FUNC_RESULT Result, ref int DeviceCount)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceCountGetBase), this.FuncType(nameof(this.DeviceCountGetBase)));
                DeviceCount = this._dllWrapper.DeviceCount();
                return result = this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
            }
            finally { this._logger.FuncEnd(result, DeviceCount); }
        }
        private FUNC_RESULT DeviceCountGetBaseCheck(FUNC_RESULT Result, int DeviceCount)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            try
            {
                if (DeviceCount >= DeviceDefinitions.DEVICE_COUNT_MIN && DeviceCount <= DeviceDefinitions.DEVICE_COUNT_MAX) return FUNC_RESULT.SUCCESS;
                else { this.ErrorSet(ERROR_CODE.DEVICE_COUNT, "Device Count = " + DeviceCount.ToString()); return FUNC_RESULT.FAIL; }
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
            try
            {
                this._logger.FuncStart(nameof(this.DeviceSelectByIndexWorkflow), this.FuncType(nameof(this.DeviceSelectByIndexWorkflow)));
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
            finally { this._logger.FuncEnd(result, new { DeviceIndex  , CheckDeviceNumber}); }
        }
        private FUNC_RESULT DeviceSelectByIndexExecute(FUNC_RESULT Result, int DeviceIndex, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceSelectByIndexExecute), this.FuncType(nameof(this.DeviceSelectByIndexExecute)));
                Thread.Sleep(Delay);
                return result = this.DeviceSelectByIndexBase(DeviceIndex);
            }
            finally { this._logger.FuncEnd(result, DeviceIndex); }
        }
        private FUNC_RESULT DeviceSelectByIndexBase(int DeviceIndex)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceSelectByIndexBase), this.FuncType(nameof(this.DeviceSelectByIndexBase)));
                result = this.DeviceSelectByIndexBaseInputCheck(result, DeviceIndex);
                return result = this.DeviceSelectByIndexBaseDll(result, DeviceIndex);
            }
            finally { this._logger.FuncEnd(result, DeviceIndex); }
        }
        private FUNC_RESULT DeviceSelectByIndexBaseInputCheck(FUNC_RESULT Result, int DeviceIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (!this.DeviceIndexRangeCheck(DeviceIndex)) { this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Index = " + DeviceIndex.ToString() + " -> out of range"); return FUNC_RESULT.MAJOR_ERROR; }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT DeviceSelectByIndexBaseDll(FUNC_RESULT Result, int DeviceIndex)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceSelectByIndexBase), this.FuncType(nameof(this.DeviceSelectByIndexBase)));
                this._dllWrapper.SelectByDeviceIndex(Convert.ToInt16(DeviceIndex));
                return result = this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
            }
            finally { this._logger.FuncEnd(result, DeviceIndex); }
        }
        /*
         * 1. Important Note : Index limited from 0 to 127 in Manshour software test
        */
        //End : Device Select By Index
        //Device Select By Device Number
        private FUNC_RESULT DeviceSelectByDeviceNumberWorkflow(int DeviceNumber, bool CheckDeviceNumber, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            int oldDeviceNumber = this._runtimeIdentity.DeviceNumber;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceSelectByDeviceNumberWorkflow), this.FuncType(nameof(this.DeviceSelectByDeviceNumberWorkflow)));
                result = this.DeviceSelectByDeviceNumberExecute(result, DeviceNumber, (int)OPERATION_DELAY_MS.SHORT);
                if (CheckDeviceNumber) result = this.DeviceNumberGetExecute(result, (int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref DeviceNumber);
                if (result == FUNC_RESULT.SUCCESS && CheckDeviceNumber && DeviceNumber != oldDeviceNumber) result = FUNC_RESULT.MAJOR_ERROR;
                this.XXXSetValue(result, DeviceNumber, DeviceDefinitions.NUMBER_INVALID, ref DeviceNumber, ref this._runtimeIdentity.DeviceNumber, InternalValueSet);
                return result;
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber, oldDeviceNumber, CheckDeviceNumber }); }
        }
        private FUNC_RESULT DeviceSelectByDeviceNumberExecute(FUNC_RESULT Result, int DeviceNumber, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceSelectByDeviceNumberExecute), this.FuncType(nameof(this.DeviceSelectByDeviceNumberExecute)));
                Thread.Sleep(Delay);
                return result = this.DeviceSelectByDeviceNumberBase(DeviceNumber);
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber }); }
        }
        private FUNC_RESULT DeviceSelectByDeviceNumberBase(int DeviceNumber)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceSelectByDeviceNumberBase), this.FuncType(nameof(this.DeviceSelectByDeviceNumberBase)));
                result = this.DeviceSelectByDeviceNumberBaseInputCheck(result, DeviceNumber);
                return result = this.DeviceSelectByDeviceNumberBaseDll(result, DeviceNumber);
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber }); }
        }
        private FUNC_RESULT DeviceSelectByDeviceNumberBaseInputCheck(FUNC_RESULT Result, int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (!this.DeviceNumberRangeCheck(DeviceNumber)) { this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Number = " + DeviceNumber.ToString() + " -> out of range"); return FUNC_RESULT.MAJOR_ERROR; }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT DeviceSelectByDeviceNumberBaseDll(FUNC_RESULT Result, int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceSelectByDeviceNumberBaseDll), this.FuncType(nameof(this.DeviceSelectByDeviceNumberBaseDll)));
                this._dllWrapper.SelectByDeviceNumber(Convert.ToInt16(DeviceNumber));
            return result = this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber }); }
        }
        /*
         * 1. Important Note : Device Number from 0 to 127 , after 127 it is changed but can not select by device number -> Device Number allow 0 to 96
        */
        //End : Device Select By Device Number

        //Device Number Get
        private FUNC_RESULT DeviceNumberGetWorkflow(ref int DeviceNumber, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceNumberGetWorkflow), this.FuncType(nameof(this.DeviceNumberGetWorkflow)));
                result = this.DeviceNumberGetExecute(result, !this._runtimeConfig.BufferCheck ? (int)TRY_COUNT.NORMAL : (int)TRY_COUNT.BUFFER_CHECK, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref DeviceNumber);
                this.XXXSetValue(result, DeviceNumber, DeviceDefinitions.NUMBER_INVALID, ref DeviceNumber, ref this._runtimeIdentity.DeviceNumber, InternalValueSet);
                return result;
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber }); }
        }
        private FUNC_RESULT DeviceNumberGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceNumberGetExecute), this.FuncType(nameof(this.DeviceNumberGetExecute)));
                if (!this._runtimeConfig.BufferCheck) return result = this.DeviceNumberGetExecuteNormal(loopCount, Delay, ref DeviceNumber);
                else return result = this.DeviceNumberGetExecuteBufferCheck(loopCount, Delay, ref DeviceNumber);
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber, this._runtimeConfig.BufferCheck }); }
        }
        private FUNC_RESULT DeviceNumberGetExecuteNormal(int loopCount, int Delay, ref int DeviceNumber)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            int deviceNumber = DeviceDefinitions.NUMBER_INVALID;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceNumberGetExecuteNormal), this.FuncType(nameof(this.DeviceNumberGetExecuteNormal)));
                while (loopCount-- > 0 && (result1 != FUNC_RESULT.SUCCESS || result2 != FUNC_RESULT.SUCCESS))
                {
                    Thread.Sleep(Delay);
                    if (result1 != FUNC_RESULT.SUCCESS) result1 = this.DeviceNumberGetBase(ref DeviceNumber);
                    else if (this._runtimeConfig.EnableDoubleReadValidation) result2 = this.DeviceNumberGetBase(ref deviceNumber);
                    else break;
                    if (result1 == FUNC_RESULT.MAJOR_ERROR || result2 == FUNC_RESULT.MAJOR_ERROR) return result = FUNC_RESULT.MAJOR_ERROR;
                }
                return result = this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, result1, result2, ref DeviceNumber, deviceNumber, DeviceDefinitions.NUMBER_INVALID);
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber }); }
        }
        private FUNC_RESULT DeviceNumberGetExecuteBufferCheck(int loopCount, int Delay, ref int DeviceNumber)
        {
            BUFFER_UPDATE_SOURCE bufferUpdateSource = BUFFER_UPDATE_SOURCE.DATA_UPDATE_ONLY;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceNumberGetExecuteBufferCheck), this.FuncType(nameof(this.DeviceNumberGetExecuteBufferCheck)));
                while (loopCount-- > 0 && result != FUNC_RESULT.SUCCESS && result != FUNC_RESULT.MAJOR_ERROR)
                {
                    Thread.Sleep(Delay);
                    result = this.DeviceNumberGetBase(ref DeviceNumber);
                }
                return result;
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber }); }
        }
        private FUNC_RESULT DeviceNumberGetBase(ref int DeviceNumber)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceNumberGetBase), this.FuncType(nameof(this.DeviceNumberGetBase)));
                result = this.DeviceNumberGetBaseDll(result, ref DeviceNumber);
                result = this.DeviceNumberGetBaseCheck(result, DeviceNumber);
                this.XXXBaseSetValue(result, DeviceNumber, DeviceDefinitions.NUMBER_INVALID, ref DeviceNumber);
                return result;
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber }); }
        }
        private FUNC_RESULT DeviceNumberGetBaseDll(FUNC_RESULT Result, ref int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceNumberGetBaseDll), this.FuncType(nameof(this.DeviceNumberGetBaseDll)));
                if (this._dllWrapper.DeviceNumberGet(ref DeviceNumber)) return result = FUNC_RESULT.SUCCESS;
                if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return result = FUNC_RESULT.MAJOR_ERROR;
                this.ErrorSet(ERROR_CODE.DEVICE_NUMBER_GET, "Device Number = " + DeviceNumber.ToString());
                return result = FUNC_RESULT.FAIL;
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber }); }
        }
        private FUNC_RESULT DeviceNumberGetBaseCheck(FUNC_RESULT Result, int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (!this.DeviceNumberRangeCheck(DeviceNumber)) { this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Number = " + DeviceNumber.ToString() + " -> out of range"); return FUNC_RESULT.MAJOR_ERROR; }
            return FUNC_RESULT.SUCCESS;
        }
        //End : Device Number Get

        //Device Number Set
        private FUNC_RESULT DeviceNumberSetWorkflow(int DeviceNumber, bool DeviceNumberCheck, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceNumberSetWorkflow), this.FuncType(nameof(this.DeviceNumberSetWorkflow)));
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
            finally { this._logger.FuncEnd(result, new { DeviceNumber, DeviceNumberCheck }); }
        }
        private FUNC_RESULT DeviceNumberSetExecute(FUNC_RESULT Result, int DeviceNumber, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceNumberSetExecute), this.FuncType(nameof(this.DeviceNumberSetExecute)));
                Thread.Sleep(Delay);
                return result = this.DeviceNumberSetBase(DeviceNumber);
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber }); }
        }
        private FUNC_RESULT DeviceNumberSetBase(int DeviceNumber)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceNumberSetBase), this.FuncType(nameof(this.DeviceNumberSetBase)));
                result = this.DeviceNumberSetBaseInputCheck(result, DeviceNumber);
                return result = this.DeviceNumberSetBaseDll(result, DeviceNumber);
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber }); }
        }
        private FUNC_RESULT DeviceNumberSetBaseInputCheck(FUNC_RESULT Result, int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (!this.DeviceNumberRangeCheck(DeviceNumber)) { this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Number = " + DeviceNumber.ToString()); return FUNC_RESULT.MAJOR_ERROR; }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT DeviceNumberSetBaseDll(FUNC_RESULT Result, int DeviceNumber)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceNumberSetBaseDll), this.FuncType(nameof(this.DeviceNumberSetBaseDll)));
                this._dllWrapper.DeviceNumberSet(DeviceNumber);
                return result = this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
            }
            finally { this._logger.FuncEnd(result, new { DeviceNumber }); }
        }
        //End : Device Number Set

        //DeviceManager -> Discovery Scope
        //Device Identity
        private FUNC_RESULT DeviceIdentityComplexWorkflow(int DeviceIndex, ref int DeviceNumber, ref string IMEI)
        {
            bool internalValueSet = true, checkDeviceNumber = true, gsmIsConnect = false, power = true, powerCheck = true;
            int tempInt = 0;
            string tempStr = string.Empty;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceIdentityComplexWorkflow), this.FuncType(nameof(this.DeviceIdentityComplexWorkflow)));
                result = this.DeviceSelectByIndexWorkflow(DeviceIndex, checkDeviceNumber, internalValueSet);
                if (result == FUNC_RESULT.SUCCESS)
                {
                    result = this.GSMIsConnectWorkflow(ref gsmIsConnect, internalValueSet);
                    if (gsmIsConnect)
                        result = this.IMEIGetWorkflow(ref IMEI, internalValueSet);
                    else if (result == FUNC_RESULT.FAIL)
                    {
                        result = this.GSMPowerSetWorkflow(power, powerCheck, internalValueSet);
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
            finally { this._logger.FuncEnd(result, new { DeviceIndex, DeviceNumber, IMEI, checkDeviceNumber, power, powerCheck }); }
        }
        //End : Device Identity
        //End : DeviceManager -> Discovery Scope

        //DeviceManager -> Device wroking scope

        //Device Index Unselect
        private FUNC_RESULT DeviceUnselectComplexWorkflow()
        {
            bool internalValueSet = false, checkDeviceNumber = false;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            try
            {
                this._logger.FuncStart(nameof(this.DeviceUnselectComplexWorkflow), this.FuncType(nameof(this.DeviceUnselectComplexWorkflow)));
                return result = this.DeviceSelectByIndexWorkflow(DeviceDefinitions.DEVICE_INDEX_MAX, checkDeviceNumber, internalValueSet);
            }
            finally { this._logger.FuncEnd(result, new { this._runtimeIdentity.DeviceIndex }); }
        }
        //End : Device Index Unselect


        //Device Index
        private bool DeviceIndexRangeCheck(int DeviceIndex)
        {
            if (DeviceIndex >= DeviceDefinitions.DEVICE_INDEX_MIN && DeviceIndex <= DeviceDefinitions.DEVICE_INDEX_MAX) return true;
            return false;
        }
        //End : Device Index
        //Device Number
        private bool DeviceNumberRangeCheck(int DeviceNumber)
        {
            if (DeviceNumber >= DeviceDefinitions.DEVICE_NUMBER_MIN && DeviceNumber <= DeviceDefinitions.DEVICE_NUMBER_MAX) return true;
            return false;
        }
        //End : Device Number

    }
}
