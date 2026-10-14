using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        //Device Count
        private FUNC_RESULT DeviceCountGetWorkflow(ref int DeviceCount, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            int deviceCount1 = DeviceDefinitions.NUMBER_INVALID, deviceCount2 = DeviceDefinitions.NUMBER_INVALID;
            result = this.DeviceCountGetExecute((int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.SHORT, ref result1, ref result2, ref deviceCount1, ref deviceCount2);
            result = this.XXXCheck(result, result1, result2, deviceCount1, deviceCount2);
            this.XXXSetValue(result, deviceCount1, DeviceDefinitions.NUMBER_INVALID, ref DeviceCount, ref this._runtimeIdentity.DeviceCount, InternalValueSet);
            return result;
        }
        private FUNC_RESULT DeviceCountGetExecute(int loopCount,int Delay, ref FUNC_RESULT Result1, ref FUNC_RESULT Result2, ref int DeviceCount1, ref int DeviceCount2)
        {
            //How Many continues fail accepted
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            //1 result get or 2 result get and check 1 and 2
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = FUNC_RESULT.SUCCESS;
            }
            //try time
            while (loopCount > 0 && (Result1 != FUNC_RESULT.SUCCESS || Result2 != FUNC_RESULT.SUCCESS))
            {
                //delay
                Thread.Sleep(Delay);
                //try for first correct result
                if (Result1 != FUNC_RESULT.SUCCESS)
                {
                    Result1 = this.DeviceCountGetBase(ref DeviceCount1);
                    //continues fail check
                    if (Result1 != FUNC_RESULT.SUCCESS)
                    {
                        resultCount--;
                        if (resultCount <= 0)
                        {
                            break;
                        }
                    }
                }
                //try for second correct result
                else
                {
                    Result2 = this.DeviceCountGetBase(ref DeviceCount2);
                }
                loopCount--;
                if (Result1 == FUNC_RESULT.MAJOR_ERROR || Result2 == FUNC_RESULT.MAJOR_ERROR)
                {
                    return FUNC_RESULT.MAJOR_ERROR;
                }
            }
            //1 result get or 2 result get and check 1 and 2
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                DeviceCount2 = DeviceCount1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT DeviceCountGetBase(ref int DeviceCount)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            int deviceCount = 0;
            result = this.DeviceCountGetBaseDll(ref deviceCount);
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.DeviceCountGetBaseCheck(deviceCount);
            }
            this.XXXBaseSetValue(result , deviceCount, DeviceDefinitions.NUMBER_INVALID, ref DeviceCount);
            return result;
        }
        private FUNC_RESULT DeviceCountGetBaseDll(ref int DeviceCount)
        {
            DeviceCount = this._dllWrapper.DeviceCount();
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        private FUNC_RESULT DeviceCountGetBaseCheck(int DeviceCount)
        {
            try
            {
                if (DeviceCount >= DeviceDefinitions.DEVICE_COUNT_MIN && DeviceCount <= DeviceDefinitions.DEVICE_COUNT_MAX)
                {
                    return FUNC_RESULT.SUCCESS;
                }
                else
                {
                    this.ErrorSet(ERROR_CODE.DEVICE_COUNT, "Device Count = " + DeviceCount.ToString());
                    return FUNC_RESULT.FAIL;
                }
            }
            catch (Exception ex)
            {
                this.ErrorSet(ERROR_CODE.DEVICE_COUNT, "DeviceCountGetCheck Error : " + ex.Message);
                return FUNC_RESULT.FAIL;
            }
        }
        //End : Device Count

        //Device Select By Index
        private FUNC_RESULT DeviceSelectByIndexWorkflow(int DeviceIndex, bool CheckDeviceNumber, bool InternalValueSet)
        {
            int deviceIndex = DeviceDefinitions.NUMBER_INVALID, deviceNumber = DeviceDefinitions.NUMBER_INVALID;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            int deviceNumber1 = DeviceDefinitions.NUMBER_INVALID, deviceNumber2 = DeviceDefinitions.NUMBER_INVALID;
            result = this.DeviceSelectByIndexExecute(DeviceIndex, (int)OPERATION_DELAY_MS.SHORT);
            if (result == FUNC_RESULT.SUCCESS && CheckDeviceNumber)
            {
                result = this.DeviceNumberGetExecute((int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref deviceNumber1, ref deviceNumber2);
                result = this.XXXCheck(result, result1, result2, deviceNumber1, deviceNumber2);
            }
            this.XXXSetValue(result, DeviceIndex, DeviceDefinitions.NUMBER_INVALID, ref deviceIndex, ref this._runtimeIdentity.DeviceIndex, InternalValueSet);
            if (CheckDeviceNumber)
                this.XXXSetValue(result, deviceNumber1, DeviceDefinitions.NUMBER_INVALID, ref deviceNumber, ref this._runtimeIdentity.DeviceNumber, InternalValueSet);
            return result;
        }
        private FUNC_RESULT DeviceSelectByIndexExecute(int DeviceIndex, int Delay)
        {
            Thread.Sleep(Delay);
            return this.DeviceSelectByIndexBase(DeviceIndex);
        }
        private FUNC_RESULT DeviceSelectByIndexBase(int DeviceIndex)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            result = this.DeviceSelectByIndexBaseInputCheck(DeviceIndex);
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.DeviceSelectByIndexBaseDll(DeviceIndex);
            }
            return result;
        }
        private FUNC_RESULT DeviceSelectByIndexBaseInputCheck(int DeviceIndex)
        {
            if (!this.DeviceIndexRangeCheck(DeviceIndex))
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Index = " + DeviceIndex.ToString() + " -> out of range");
                return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT DeviceSelectByIndexBaseDll(int DeviceIndex)
        {
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
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            int oldDeviceNumber = this._runtimeIdentity.DeviceNumber, deviceNumber = DeviceDefinitions.NUMBER_INVALID;
            int deviceNumber1 = DeviceDefinitions.NUMBER_INVALID, deviceNumber2 = DeviceDefinitions.NUMBER_INVALID;
            result = this.DeviceSelectByDeviceNumberExecute(DeviceNumber, (int)OPERATION_DELAY_MS.SHORT);
            if (result == FUNC_RESULT.SUCCESS && CheckDeviceNumber)
            {
                this.DeviceNumberGetExecute((int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref deviceNumber1, ref deviceNumber2);
                result = this.XXXCheck(result, result1, result2, deviceNumber1, deviceNumber2);
            }
            if (result == FUNC_RESULT.SUCCESS && CheckDeviceNumber && DeviceNumber != oldDeviceNumber)
                result = FUNC_RESULT.MAJOR_ERROR;
            this.XXXSetValue(result, deviceNumber1, DeviceDefinitions.NUMBER_INVALID, ref deviceNumber, ref this._runtimeIdentity.DeviceNumber, InternalValueSet);
            return result;
        }
        private FUNC_RESULT DeviceSelectByDeviceNumberExecute(int DeviceNumber, int Delay)
        {
            Thread.Sleep(Delay);
            return this.DeviceSelectByDeviceNumberBase(DeviceNumber);
        }
        private FUNC_RESULT DeviceSelectByDeviceNumberBase(int DeviceNumber)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            result = this.DeviceSelectByDeviceNumberBaseInputCheck(DeviceNumber);
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.DeviceSelectByDeviceNumberBaseDll(DeviceNumber);
            }
            return result;
        }
        private FUNC_RESULT DeviceSelectByDeviceNumberBaseInputCheck(int DeviceNumber)
        {
            if (!this.DeviceNumberRangeCheck(DeviceNumber))
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Number = " + DeviceNumber.ToString() + " -> out of range");
                return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT DeviceSelectByDeviceNumberBaseDll(int DeviceNumber)
        {
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
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            int deviceNumber1 = DeviceDefinitions.NUMBER_INVALID, deviceNumber2 = DeviceDefinitions.NUMBER_INVALID;
            result = this.DeviceNumberGetExecute((int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref deviceNumber1, ref deviceNumber2);
            result = this.XXXCheck(result, result1, result2, deviceNumber1, deviceNumber2);
            this.XXXSetValue(result, deviceNumber1, DeviceDefinitions.NUMBER_INVALID, ref DeviceNumber, ref this._runtimeIdentity.DeviceNumber, InternalValueSet);
            return result;
        }
        private FUNC_RESULT DeviceNumberGetExecute(int loopCount, int Delay, ref FUNC_RESULT Result1, ref FUNC_RESULT Result2, ref int DeviceNumber1, ref int DeviceNumber2)
        {
            //How Many continues fail accepted
            int resultCount = (int)FAIL_LIMIT.DEVICE_NUM_ERROR;
            //1 result get or 2 result get and check 1 and 2
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = FUNC_RESULT.SUCCESS;
            }
            //try time
            while (loopCount > 0 && (Result1 != FUNC_RESULT.SUCCESS || Result2 != FUNC_RESULT.SUCCESS))
            {
                //delay
                Thread.Sleep(Delay);
                //try for first correct result
                if (Result1 != FUNC_RESULT.SUCCESS)
                {
                    Result1 = this.DeviceNumberGetBase(ref DeviceNumber1);
                    if (Result1 != FUNC_RESULT.SUCCESS)
                    {
                        resultCount--;
                        if (resultCount <= 0)
                        {
                            break;
                        }
                    }
                }
                //try for second correct result
                else
                {
                    Result2 = this.DeviceNumberGetBase(ref DeviceNumber2);
                }
                loopCount--;
                if (Result1 == FUNC_RESULT.MAJOR_ERROR || Result2 == FUNC_RESULT.MAJOR_ERROR)
                {
                    return FUNC_RESULT.MAJOR_ERROR;
                }
            }
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                DeviceNumber2 = DeviceNumber1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT DeviceNumberGetBase(ref int DeviceNumber)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            int deviceNumber = DeviceDefinitions.NUMBER_INVALID;
            result = this.DeviceNumberGetBaseDll(ref deviceNumber);
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.DeviceNumberGetBaseCheck(deviceNumber);
            }
            this.XXXBaseSetValue(result, deviceNumber, DeviceDefinitions.NUMBER_INVALID, ref DeviceNumber);
            return result;
        }
        private FUNC_RESULT DeviceNumberGetBaseDll(ref int DeviceNumber)
        {
            if (this._dllWrapper.DeviceNumberGet(ref DeviceNumber))
                return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR)
                return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.DEVICE_NUMBER_GET, "Device Number = " + DeviceNumber.ToString());
            return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT DeviceNumberGetBaseCheck(int DeviceNumber)
        {
            if (!this.DeviceNumberRangeCheck(DeviceNumber))
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Number = " + DeviceNumber.ToString() + " -> out of range");
                return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        //End : Device Number Get

        //Device Number Set
        private FUNC_RESULT DeviceNumberSetWorkflow(int DeviceNumber, bool DeviceNumberCheck, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            int deviceNumber = DeviceDefinitions.NUMBER_INVALID;
            int deviceNumber1 = DeviceDefinitions.NUMBER_INVALID, deviceNumber2 = DeviceDefinitions.NUMBER_INVALID;
            result = this.DeviceNumberSetExecute(DeviceNumber, (int)OPERATION_DELAY_MS.SHORT);
            if (result == FUNC_RESULT.SUCCESS && DeviceNumberCheck)
            {
                result = this.DeviceSelectByIndexExecute(this._runtimeIdentity.DeviceIndex, (int)OPERATION_DELAY_MS.AFTER_DEVICE_NUM_CHANGE);
            }
            if (result == FUNC_RESULT.SUCCESS && DeviceNumberCheck)
            {
                this.DeviceNumberGetExecute((int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref deviceNumber1, ref deviceNumber2);
                result = this.XXXCheck(result, result1, result2, deviceNumber1, deviceNumber2);
            }
            if (DeviceNumberCheck && (DeviceNumber != deviceNumber1))
            {
                result = FUNC_RESULT.MAJOR_ERROR;
            }
            this.XXXSetValue(result, deviceNumber1, DeviceDefinitions.NUMBER_INVALID, ref deviceNumber, ref this._runtimeIdentity.DeviceNumber, InternalValueSet);
            return result;
        }
        private FUNC_RESULT DeviceNumberSetExecute(int DeviceNumber, int Delay)
        {
            Thread.Sleep(Delay);
            return this.DeviceNumberSetBase(DeviceNumber);
        }
        private FUNC_RESULT DeviceNumberSetBase(int DeviceNumber)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            result = this.DeviceNumberSetBaseInputCheck(DeviceNumber);
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.DeviceNumberSetBaseDll(DeviceNumber);
            }
            return result;
        }
        private FUNC_RESULT DeviceNumberSetBaseInputCheck(int DeviceNumber)
        {
            if (!this.DeviceNumberRangeCheck(DeviceNumber))
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Number = " + DeviceNumber.ToString());
                return FUNC_RESULT.MAJOR_ERROR;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT DeviceNumberSetBaseDll(int DeviceNumber)
        {
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
            string imei = DeviceDefinitions.STRING_INVALID;
            result = this.DeviceSelectByIndexWorkflow(DeviceIndex, checkDeviceNumber, internalValueSet);
            //test
            result = this.DeviceRestartComplexWorkFlow();
            return result;
            //test
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.GSMIsConnectWorkflow(ref gsmIsConnect, internalValueSet);
                if (result == FUNC_RESULT.MAJOR_ERROR)
                    return FUNC_RESULT.MAJOR_ERROR;
                if (gsmIsConnect)
                    result = this.IMEIGetWorkflow(ref imei, internalValueSet);
                else
                    if (result == FUNC_RESULT.FAIL)
                    {
                        result = this.DeviceRestartComplexWorkFlow();
                        if (result == FUNC_RESULT.SUCCESS)
                        {
                            result = this.GSMIsConnectWorkflow(ref gsmIsConnect, internalValueSet);
                            if (result == FUNC_RESULT.SUCCESS)
                                result = this.IMEIGetWorkflow(ref imei, internalValueSet);
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
            if (DeviceIndex >= DeviceDefinitions.DEVICE_INDEX_MIN && DeviceIndex <= DeviceDefinitions.DEVICE_INDEX_MAX)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //End : Device Index
        //Device Number
        private bool DeviceNumberRangeCheck(int DeviceNumber)
        {
            if (DeviceNumber >= DeviceDefinitions.DEVICE_NUMBER_MIN && DeviceNumber <= DeviceDefinitions.DEVICE_NUMBER_MAX)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //End : Device Number

    }
}
