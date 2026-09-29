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
        /*
        ====================================================================
        Device Method Architecture
        ====================================================================

        (private)
        Workflow Layer
            ↓
        Execute
            ↓
        Check (compare result and output is equal -> NOT check value renge , such 0 <= X <= 127 or IndexCheck ...)
            ↓
        Commit Values (Set Internal and Output Values)

        (Private) , (Atomic Layer)
        Base Method 
            ↓
        ErrorReset
            ↓
        Internal Status Check -> (maybe move to WorkFlow !!!)
            ↓
        Input Check
            ↓
        DLL Call , DLL Error Check
            ↓
        Output Check
            ↓
        Set Output Values
            ↓
        Return result


        Rules
        --------------------------------------------------------------------
        private WorkFlow Method:
            - May contain Retry
            - May contain Delay
            - May contain Confirmation
            - May contain Multi-Step Operations
            - May call one or more Base Methods

        Private Base Method:
            - Single Atomic Operation
            - No Retry
            - No Delay
            - No Confirmation
            - No Workflow Logic
            - Responsible for Dll Access

        ====================================================================
        */

        //Device Count
        public bool DeviceCountGet()
        {
            int deviceCount = DeviceDefinitions.NUMBER_INVALID;
            return this.DeviceCountGet(ref deviceCount, true);
        }
        public bool DeviceCountGet(ref int DeviceCount)
        {
            return this.DeviceCountGet(ref DeviceCount, true);
        }
        public bool DeviceCountGet(ref int DeviceCount, bool InternalValueSet)
        {
            bool result = false;
            if (this.TrySetBusy())
            {
                result = false;
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceCountGet));
                return result;
            }
            try
            {
                result = this.DeviceCountGetWorkflow(ref DeviceCount, InternalValueSet);
                return result;
            }
            finally
            {
                this.DeviceBusyDeactive(nameof(this.DeviceCountGet) , result);
            }
        }
        private bool DeviceCountGetWorkflow(ref int DeviceCount, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            int deviceCount1 = DeviceDefinitions.NUMBER_INVALID, deviceCount2 = DeviceDefinitions.NUMBER_INVALID;
            if (result)
            {
                this.DeviceCountGetExecute(TRY_READ_FAST, DELAY_SHORT_MS, ref result1, ref result2, ref deviceCount1, ref deviceCount2);
            }
            if (result)
            {
                result = this.DeviceCountGetCheck(result1, result2, deviceCount1, deviceCount2);
            }
            if (result)
            {
                this.DeviceCountGetSetValue(deviceCount1, ref DeviceCount, InternalValueSet);
            }
            else
            {
                this.DeviceCountGetSetValue(DeviceDefinitions.NUMBER_INVALID, ref DeviceCount, InternalValueSet);
            }
            return result;
        }
        private void DeviceCountGetExecute(int loopCount,int Delay, ref bool Result1, ref bool Result2, ref int DeviceCount1, ref int DeviceCount2)
        {
            //How Many continues fail accepted
            int resultCount = TRY_FAIL_TIME;
            //1 result get or 2 result get and check 1 and 2
            if (!this._enableDoubleReadValidation)
            {
                Result2 = true;
            }
            //try time
            while (loopCount > 0 && (!Result1 || !Result2))
            {
                //delay
                Thread.Sleep(Delay);
                //try for first correct result
                if (!Result1)
                {
                    Result1 = this.DeviceCountGetBase(ref DeviceCount1);
                    //major error check
                    if (this.BaseErrorCheck())
                    {
                        return;
                    }
                    //continues fail check
                    if (!Result1)
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
                    //major error check
                    if (this.BaseErrorCheck())
                    {
                        return;
                    }
                }
                loopCount--;
            }
            //1 result get or 2 result get and check 1 and 2
            if (!this._enableDoubleReadValidation)
            {
                DeviceCount2 = DeviceCount1;
            }
        }
        private bool DeviceCountGetCheck(bool Result1, bool Result2, int DeviceCount1, int DeviceCount2)
        {
            return (Result1 && Result2 && DeviceCount1 == DeviceCount2);
        }
        private void DeviceCountGetSetValue(int DeviceCountNewValue, ref int DeviceCount, bool InternalValueSet)
        {
            DeviceCount = DeviceCountNewValue;
            if (InternalValueSet)
            {
                this._deviceCount = DeviceCountNewValue;
            }
        }
        private bool DeviceCountGetBase(ref int DeviceCount)
        {
            bool result = true;
            int deviceCount = 0;
            this.ErrorReset();
            if (result)
            {
                result = this.DeviceCountGetBaseDll(ref deviceCount);
            }
            if (result)
            {
                result = this.DeviceCountGetBaseCheck(deviceCount);
            }
            if (result)
            {
                this.DeviceCountGetBaseSetValue(deviceCount, ref DeviceCount);
            }
            else
            {
                this.DeviceCountGetBaseSetValue(DeviceDefinitions.NUMBER_INVALID, ref DeviceCount);
            }
            return result;
        }
        private bool DeviceCountGetBaseDll(ref int DeviceCount)
        {
            try
            {
                DeviceCount = this._dllWrapper.DeviceCount();
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"DeviceCountGetDll Error : {ex.Message}");
                return false;
            }
        }
        private bool DeviceCountGetBaseCheck(int DeviceCount)
        {
            try
            {
                if (DeviceCount >= DeviceDefinitions.DEVICE_COUNT_MIN && DeviceCount <= DeviceDefinitions.DEVICE_COUNT_MAX)
                {
                    return true;
                }
                else
                {
                    return this.ErrorSet(ERROR_CODE.DEVICE_COUNT, "Device Count = " + DeviceCount.ToString());
                }
            }
            catch (Exception ex)
            {
                return this.ErrorSet(ERROR_CODE.DEVICE_COUNT, "DeviceCountGetCheck Error : " + ex.Message);
            }
        }
        private void DeviceCountGetBaseSetValue(int DeviceCountNewValue, ref int DeviceCount)
        {
            DeviceCount = DeviceCountNewValue;
        }
        //End : Device Count

        //Device Select By Index
        private bool DeviceSelectByIndexWorkflow(int DeviceIndex, bool CheckDeviceNumber, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            int deviceNumber1 = DeviceDefinitions.NUMBER_INVALID, deviceNumber2 = DeviceDefinitions.NUMBER_INVALID;
            if (result)
            {
                result = this.DeviceSelectByIndexExecute(DeviceIndex, DELAY_SHORT_MS);
            }
            if (result && CheckDeviceNumber)
            {
                this.DeviceNumberGetExecute(TRY_READ_NORMAL, DELAY_NORMAL1_MS, ref result1, ref result2, ref deviceNumber1, ref deviceNumber2);
                result = this.DeviceSelectByIndexCheck(result1, result2, deviceNumber1, deviceNumber2);
            }
            if (result)
            {
                this.DeviceSelectByIndexSetValue(DeviceIndex, deviceNumber1, InternalValueSet, CheckDeviceNumber);
            }
            else
            {
                this.DeviceSelectByIndexSetValue(DeviceDefinitions.NUMBER_INVALID, DeviceDefinitions.NUMBER_INVALID, InternalValueSet, CheckDeviceNumber);
            }
            return result;
        }
        private bool DeviceSelectByIndexExecute(int DeviceIndex, int Delay)
        {
            Thread.Sleep(Delay);
            return this.DeviceSelectByIndexBase(DeviceIndex);
        }
        private bool DeviceSelectByIndexCheck(bool Result1, bool Result2, int DeviceNumber1, int DeviceNumber2)
        {
            return (Result1 && Result2 && DeviceNumber1 == DeviceNumber2);
        }
        private void DeviceSelectByIndexSetValue(int DeviceIndexNewValue, int DeviceNumberNewValue, bool InternalValueSet , bool CheckDeviceNumber)
        {
            if (InternalValueSet)
            {
                this._deviceIndex = DeviceIndexNewValue;
                if (CheckDeviceNumber)
                {
                    this._deviceNumber = DeviceNumberNewValue;
                }
            }
        }
        private bool DeviceSelectByIndexBase(int DeviceIndex)
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.DeviceSelectByIndexBaseInputCheck(DeviceIndex);
            }
            if (result)
            {
                result = this.DeviceSelectByIndexBaseDll(DeviceIndex);
            }
            return result;
        }
        private bool DeviceSelectByIndexBaseInputCheck(int DeviceIndex)
        {
            bool result = this.DeviceIndexRangeCheck(DeviceIndex);
            if (!result)
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Index = " + DeviceIndex.ToString() + " -> out of range");
            }
            return result;
        }
        private bool DeviceSelectByIndexBaseDll(int DeviceIndex)
        {
            try
            {
                this._dllWrapper.SelectByDeviceIndex(Convert.ToInt16(DeviceIndex));
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"DeviceSelectByIndexDll Error : {ex.Message}");
                return false;
            }
        }
        /*
         * 1. Importan Note : Index limited from 0 to 127 in Manshor software test
        */
        //End : Device Select By Index
        //DeviceManager -> Discovery Scope
        //Device Identity
        public bool DeviceIdentity(int DeviceIndex, ref int DeviceNumber, ref string IMEI)
        {
            bool result = false;
            if (this.TrySetBusy())
            {
                result = false;
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceIdentity));
                return result;
            }
            try
            {
                result = this.DeviceIdentityWorkFlow(DeviceIndex, ref DeviceNumber, ref IMEI);
                return result;
            }
            finally
            {
                this.DeviceBusyDeactive(nameof(this.DeviceIdentity), result);
            }
        }
        private bool DeviceIdentityWorkFlow(int DeviceIndex, ref int DeviceNumber, ref string IMEI)
        {
            bool result = true;
            string imei = string.Empty;
            if (result)
            {
                result = this.DeviceSelectByIndexWorkflow(DeviceIndex, true, true);
                //if result => this._deviceNumber fill by correct value
            }
            if (result)
            {
                result = this.GSMIsConnectWorkflow(false);
                if (result)
                {
                    result = this.IMEIGetWorkflow(ref imei, true);
                    //if result => this._imei fill by correct value
                }
                else
                {
                    result = this.DeviceRestart();
                    if (result)
                    {
                        result = this.GSMIsConnectWorkflow(false);
                        if (result)
                        {
                            result = this.IMEIGetWorkflow(ref imei, true);
                            //if result => this._imei fill by correct value
                        }
                    }
                }
            }
            this.DeviceIdentitySetValue(this.DeviceNumber, this.IMEI, ref DeviceNumber, ref IMEI);
            return result;
        }
        private void DeviceIdentitySetValue(int DeviceNumberNewValue, string IMEINewValue, ref int DeviceNumber, ref string IMEI)
        {
            DeviceNumber = DeviceNumberNewValue;
            IMEI = IMEINewValue;
        }
        //End : Device Identity
        //End : DeviceManager -> Discovery Scope

        //DeviceManager -> Device wroking scope
        //Device Number Change
        public bool DeviceNumberChange(int DeviceNumberNew)
        {
            bool result = false;
            if (this.TrySetBusy())
            {
                result = false;
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceNumberChange));
                return result;
            }
            if (!this.DeviceNumberChangeAllow(nameof(DeviceNumberChange)))
            {
                result = false;
                return result;
            }
            try
            {
                result = this.DeviceNumberChangeWorkFlow(DeviceIndex, DeviceNumberNew);
                if (result)
                {
                    this.DeviceNumberChanged();
                }
                return result;
            }
            finally
            {
                this.DeviceBusyDeactive(nameof(this.DeviceNumberChange), result);
            }
        }
        private bool DeviceNumberChangeWorkFlow(int DeviceIndex, int DeviceNumber)
        {
            bool result = true;
            if (result)
            {
                result = this.DeviceSelectByIndexWorkflow(DeviceIndex, true, true);
                //if result => this._deviceNumber fill by correct value
            }
            if (result)
            {
                if (this.DeviceNumber == DeviceNumber)
                {
                    return true;
                }
                result = this.DeviceNumberSetWorkflow(DeviceNumber, true, true);
            }
            return result;
        }
        //Device Number Change
        //Device Index Unselect
        public bool DeviceUnselect()
        {
            bool result = false;
            if (this.TrySetBusy())
            {
                result = false;
                this.ErrorSet(ERROR_CODE.BUSY, nameof(this.DeviceUnselect));
                return result;
            }
            try
            {
                result = this.DeviceUnselectWorkFlow();
                return result;
            }
            finally
            {
                this.DeviceBusyDeactive(nameof(this.DeviceUnselect), result);
            }
        }
        private bool DeviceUnselectWorkFlow()
        {
            bool result = true;
            if (result)
            {
                result = this.DeviceSelectByIndexWorkflow(DeviceDefinitions.DEVICE_INDEX_MAX, false, false);
                //if result = false => this._deviceNumber is invalid and DLL unselect this index
            }
            return !result;
        }
        //End : Device Index Unselect

        //Device Select By Device Number
        private bool DeviceSelectByDeviceNumberWorkflow(int DeviceNumber, bool CheckDeviceNumber, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            int oldDeviceNumber = this._deviceNumber;
            int deviceNumber1 = DeviceDefinitions.NUMBER_INVALID, deviceNumber2 = DeviceDefinitions.NUMBER_INVALID;
            if (result)
            {
                result = this.DeviceSelectByDeviceNumberExecute(DeviceNumber, DELAY_SHORT_MS);
            }
            if (result && CheckDeviceNumber)
            {
                this.DeviceNumberGetExecute(TRY_READ_NORMAL, DELAY_NORMAL1_MS, ref result1, ref result2, ref deviceNumber1, ref deviceNumber2);
                result = this.DeviceSelectByDeviceNumberCheck(result1, result2, deviceNumber1, deviceNumber2);
            }
            if (result)
            {
                this.DeviceSelectByDeviceNumberSetValue(DeviceNumber, InternalValueSet, (DeviceNumber != oldDeviceNumber));
            }
            else
            {
                this.DeviceSelectByDeviceNumberSetValue(DeviceDefinitions.NUMBER_INVALID, InternalValueSet, false);
            }
            return result;
        }
        private bool DeviceSelectByDeviceNumberExecute(int DeviceNumber, int Delay)
        {
            Thread.Sleep(Delay);
            return this.DeviceSelectByDeviceNumberBase(DeviceNumber);
        }
        private bool DeviceSelectByDeviceNumberCheck(bool Result1, bool Result2, int DeviceNumber1, int DeviceNumber2)
        {
            return (Result1 && Result2 && DeviceNumber1 == DeviceNumber2);
        }
        private void DeviceSelectByDeviceNumberSetValue(int deviceNumberNewValue, bool InternalValueSet , bool DeviceNumberChange)
        {
            if (InternalValueSet)
            {
                this._deviceNumber = deviceNumberNewValue;
                if (DeviceNumberChange)
                {
                    this._deviceIndex = DeviceDefinitions.NUMBER_INVALID;
                }
            }
        }
        private bool DeviceSelectByDeviceNumberBase(int DeviceNumber)
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.DeviceSelectByDeviceNumberBaseInputCheck(DeviceNumber);
            }
            if (result)
            {
                result = this.DeviceSelectByDeviceNumberBaseDll(DeviceNumber);
            }
            return result;
        }
        private bool DeviceSelectByDeviceNumberBaseInputCheck(int DeviceNumber)
        {
            bool result = this.DeviceNumberRangeCheck(DeviceNumber);
            if (!result)
            {
                this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Number = " + DeviceNumber.ToString());
            }
            return result;
        }
        private bool DeviceSelectByDeviceNumberBaseDll(int DeviceNumber)
        {
            try
            {
                this._dllWrapper.SelectByDeviceNumber(Convert.ToInt16(DeviceNumber));
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"DeviceSelectByDeviceNumberDll Error : {ex.Message}");
                return false;
            }
        }
        /*
         * 1. Important Note : Device Number from 0 to 127 , after 127 it is changed but can not select by device number
        */
        //End : Device Select By Device Number

        //Device Number Get
        private bool DeviceNumberGetWorkflow(ref int DeviceNumber, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            int deviceNumber1 = DeviceDefinitions.NUMBER_INVALID, deviceNumber2 = DeviceDefinitions.NUMBER_INVALID;
            if (result)
            {
                this.DeviceNumberGetExecute(TRY_READ_NORMAL, DELAY_NORMAL1_MS, ref result1, ref result2, ref deviceNumber1, ref deviceNumber2);
            }
            if (result)
            {
                result = this.DeviceNumberGetCheck(result1, result2, deviceNumber1, deviceNumber2);
            }
            if (result)
            {
                this.DeviceNumberGetSetValue(deviceNumber1, ref DeviceNumber, InternalValueSet);
            }
            else
            {
                this.DeviceNumberGetSetValue(DeviceDefinitions.NUMBER_INVALID, ref DeviceNumber, InternalValueSet);
            }
            return result;
        }
        private void DeviceNumberGetExecute(int loopCount, int Delay, ref bool result1, ref bool result2, ref int deviceNumber1, ref int deviceNumber2)
        {
            //How Many continues fail accepted
            int resultCount = TRY_FAIL_TIME_DEVICE_NUMBER_GET;
            //1 result get or 2 result get and check 1 and 2
            if (!this._enableDoubleReadValidation)
            {
                result2 = true;
            }
            //try time
            while (loopCount > 0 && (!result1 || !result2))
            {
                //delay
                Thread.Sleep(Delay);
                //try for first correct result
                if (!result1)
                {
                    result1 = this.DeviceNumberGetBase(ref deviceNumber1);
                    //major error check
                    if (this.BaseErrorCheck())
                    {
                        return;
                    }
                    //continues fail check
                    if (!result1)
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
                    result2 = this.DeviceNumberGetBase(ref deviceNumber2);
                    //major error check
                    if (this.BaseErrorCheck())
                    {
                        return;
                    }
                }
                loopCount--;
            }
            if (!this._enableDoubleReadValidation)
            {
                deviceNumber2 = deviceNumber1;
            }
        }
        private bool DeviceNumberGetCheck(bool Result1, bool Result2, int DeviceNumber1, int DeviceNumber2)
        {
            return (Result1 && Result2 && DeviceNumber1 == DeviceNumber2);
        }
        private void DeviceNumberGetSetValue(int deviceNumberNewValue, ref int DeviceNumber, bool InternalValueSet)
        {
            DeviceNumber = deviceNumberNewValue;
            if (InternalValueSet)
            {
                this._deviceNumber = deviceNumberNewValue;
            }
        }
        private bool DeviceNumberGetBase(ref int DeviceNumber)
        {
            bool result = true;
            int deviceNumber = DeviceDefinitions.NUMBER_INVALID;
            this.ErrorReset();
            if (result)
            {
                result = this.DeviceNumberGetBaseDll(ref deviceNumber);
            }
            if (result)
            {
                result = this.DeviceNumberGetBaseCheck(deviceNumber);
            }
            if (result)
            {
                DeviceNumberGetBaseSetValue(deviceNumber, ref DeviceNumber);
            }
            else
            {
                DeviceNumberGetBaseSetValue(DeviceDefinitions.NUMBER_INVALID, ref DeviceNumber);
            }
            return result;
        }
        private bool DeviceNumberGetBaseDll(ref int DeviceNumber)
        {
            try
            {
                if (this._dllWrapper.DeviceNumberGet(ref DeviceNumber))
                {
                    return this.DllWrapperErrorCheck();
                }
                else
                {
                    return this.ErrorSet(ERROR_CODE.DEVICE_NUMBER_GET, "Device Number = " + DeviceNumber.ToString());
                }
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"DeviceNumberGetDll Error : {ex.Message}");
                return false;
            }
        }
        private bool DeviceNumberGetBaseCheck(int DeviceNumber)
        {
            try
            {
                if (this.DeviceNumberRangeCheck(DeviceNumber))
                {
                    return true;
                }
                else
                {
                    return this.ErrorSet(ERROR_CODE.DEVICE_NUMBER_GET, "Device Number = " + DeviceNumber.ToString());
                }
            }
            catch (Exception ex)
            {
                return this.ErrorSet(ERROR_CODE.DEVICE_NUMBER_GET, "DeviceNumberGetCheck Error : " + ex.Message);
            }
        }
        private void DeviceNumberGetBaseSetValue(int DeviceNumberNewValue, ref int DeviceNumber)
        {
            DeviceNumber = DeviceNumberNewValue;
        }
        //End : Device Number Get

        //Device Number Set
        private bool DeviceNumberSetWorkflow(int DeviceNumber, bool CheckDeviceNumber, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            int deviceNumber1 = DeviceDefinitions.NUMBER_INVALID, deviceNumber2 = DeviceDefinitions.NUMBER_INVALID;
            if (result)
            {
                result = this.DeviceNumberSetExecute(DeviceNumber, DELAY_SHORT_MS);
            }
            if (result && CheckDeviceNumber)
            {
                result = this.DeviceSelectByIndexExecute(this.DeviceIndex, DELAY_AFTER_DEVICE_NUM_CHANGE_MS);
            }
            if (result && CheckDeviceNumber)
            {
                this.DeviceNumberGetExecute(TRY_READ_NORMAL, DELAY_NORMAL1_MS, ref result1, ref result2, ref deviceNumber1, ref deviceNumber2);
                result = this.DeviceNumberSetCheck(result1, result2, deviceNumber1, deviceNumber2);
            }
            if (result)
            {
                this.DeviceNumberSetSetValue(DeviceNumber, InternalValueSet);
            }
            else
            {
                this.DeviceNumberSetSetValue(DeviceDefinitions.NUMBER_INVALID, InternalValueSet);
            }
            return result;
        }
        private bool DeviceNumberSetExecute(int DeviceNumber, int Delay)
        {
            Thread.Sleep(Delay);
            return this.DeviceNumberSetBase(DeviceNumber);
        }
        private bool DeviceNumberSetCheck(bool Result1, bool Result2, int DeviceNumber1, int DeviceNumber2)
        {
            return (Result1 && Result2 && DeviceNumber1 == DeviceNumber2);
        }
        private void DeviceNumberSetSetValue(int deviceNumberNewValue, bool InternalValueSet)
        {
            if (InternalValueSet)
            {
                this._deviceNumber = deviceNumberNewValue;
            }
        }
        private bool DeviceNumberSetBase(int DeviceNumber)
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.DeviceNumberSetBaseInputCheck(DeviceNumber);
            }
            if (result)
            {
                result = this.DeviceNumberSetBaseDll(DeviceNumber);
            }
            return result;
        }
        private bool DeviceNumberSetBaseInputCheck(int DeviceNumber)
        {
            try
            {
                if (this.DeviceNumberRangeCheck(DeviceNumber))
                {
                    return true;
                }
                else
                {
                    return this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Device Number = " + DeviceNumber.ToString());
                }
            }
            catch (Exception ex)
            {
                return this.ErrorSet(ERROR_CODE.INVALID_INPUT, "DeviceNumberSetCheck Error : " + ex.Message);
            }
        }
        private bool DeviceNumberSetBaseDll(int DeviceNumber)
        {
            try
            {
                this._dllWrapper.DeviceNumberSet(DeviceNumber);
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"DeviceNumberSetDll Error : {ex.Message}");
                return false;
            }
        }
        //End : Device Number Set

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
