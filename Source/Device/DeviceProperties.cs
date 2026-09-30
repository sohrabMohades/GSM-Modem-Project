using GSM.Core.Source.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GSM.Core.Source.Common.DeviceDefinitions;

namespace GSM.Core.Source.Device
{
    public partial class Device : IDisposable
    {
        //Hardware Status
        //GSM Power Get
        private bool GSMPowerGetWorkflow(bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;

            if (result)
            {
                this.GSMPowerGetExecute(TRY_READ_POWER, DELAY_SHORT_MS, ref result1, ref result2);
            }
            if (result)
            {
                result = this.GSMPowerGetCheck(result1, result2);
            }
            this.GSMPowerGetSetValue(result, InternalValueSet);
            return result;
        }
        private void GSMPowerGetExecute(int loopCount, int Delay, ref bool result1, ref bool result2)
        {
            //Default Device must Power ON unless after some try opposite is true
            bool firstExecute = true;
            //try time
            while (loopCount > 0 && (!result1 || !result2))
            {
                //delay
                Thread.Sleep(Delay);
                //try find first Power ON
                if (firstExecute)
                {
                    result1 = this.GSMPowerGetBase();
                    //major error check
                    if (this.BaseErrorCheck())
                    {
                        return;
                    }
                    //first Power ON detect
                    if (result1)
                    {
                        firstExecute = false;
                    }
                }
                //try find second Power ON
                else
                {
                    result2 = this.GSMPowerGetBase();
                    //major error check
                    if (this.BaseErrorCheck())
                    {
                        return;
                    }
                    //second Power ON detect
                }
                loopCount--;
            }
        }
        private bool GSMPowerGetCheck(bool Result1, bool Result2)
        {
            return (Result1 && Result2);
        }
        private void GSMPowerGetSetValue(bool GSMPowerNewValue, bool InternalValueSet)
        {
            if (InternalValueSet)
            {
                this._gsmPower = GSMPowerNewValue;
            }
        }
        private bool GSMPowerGetBase()
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.GSMPowerGetBaseDll();
            }
            return result;
        }
        private bool GSMPowerGetBaseDll()
        {
            try
            {
                bool result = this._dllWrapper.GsmPowerGet();
                return (this.DllWrapperErrorCheck() && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"GSMPowerGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        //End : GSM Power Get
        //GSM Power Set
        private bool GSMPowerSetWorkflow(bool PowerSet, bool PowerCheck, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false, getPower = false;
            if (result)
            {
                result = this.GSMPowerSetExecute(DELAY_SHORT_MS, PowerSet);
            }
            if (result && PowerCheck)
            {
                if (PowerSet)
                {
                    Thread.Sleep(DELAY_AFTER_DEVICE_POWER_ON);
                    this.GSMPowerGetExecute(TRY_READ_POWER, DELAY_SHORT_MS, ref result1, ref result2);
                }
                else
                {
                    this.GSMPowerGetExecute(TRY_READ_POWER, DELAY_SHORT_MS, ref result1, ref result2);
                }
                getPower = this.GSMPowerGetCheck(result1, result2);
            }
            if (result)
            {
                result = this.GSMPowerSetCheck(PowerSet, getPower);
            }
            this.GSMPowerSetSetValue(result, PowerSet, InternalValueSet);
            return result;
        }
        private bool GSMPowerSetExecute(int Delay, bool Power)
        {
            Thread.Sleep(Delay);
            return this.GSMPowerSetBase(Power);
        }
        private bool GSMPowerSetCheck(bool SetPower, bool GetPower)
        {
            if (SetPower == GetPower)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void GSMPowerSetSetValue(bool result, bool PowerNewValue, bool InternalValueSet)
        {
            if (InternalValueSet)
            {
                this._gsmPower = PowerNewValue;
            }
        }
        private bool GSMPowerSetBase(bool Power)
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.GSMPowerBaseDll(Power);
            }
            return result;
        }
        private bool GSMPowerBaseDll(bool Power)
        {
            try
            {
                this._dllWrapper.GsmPowerSet(Power);
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"GSMPowerBaseDll Error : {ex.Message}");
                return false;
            }
        }
        //End : GSM Power Set
        //GSM Is Connect
        private bool GSMIsConnectWorkflow(bool InternalValueSet)
        {
            bool result = true, result1 = false;
            if (result)
            {
                this.GSMIsConnectExecute(TRY_READ_IS_CONNECT, DELAY_NORMAL1_MS, ref result1);
            }
            if (result)
            {
                result = this.GSMIsConnectCheck(result1);
            }
            this.GSMIsConnectSetValue(result, InternalValueSet);
            return result;
        }
        private void GSMIsConnectExecute(int loopCount, int Delay, ref bool result)
        {
            //if device is connect want return false has long delay
            //try time
            while (loopCount > 0 && !result)
            {
                //delay
                Thread.Sleep(Delay);
                result = this.GSMIsConnectBase();
                //major error check
                if (this.BaseErrorCheck())
                {
                    return;
                }
                loopCount--;
            }
        }
        private bool GSMIsConnectCheck(bool Result)
        {
            return Result;
        }
        private void GSMIsConnectSetValue(bool GSMIsConnectNewValue, bool InternalValueSet)
        {
            if (InternalValueSet)
            {
                this._gsmIsConnect = GSMIsConnectNewValue;
            }
        }
        private bool GSMIsConnectBase()
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.GSMIsConnectBaseDll();
            }
            return result;
        }
        private bool GSMIsConnectBaseDll()
        {
            try
            {
                bool result = this._dllWrapper.GsmIsConnect();
                return (this.DllWrapperErrorCheck() && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"GSMIsConnectBaseDll Error : {ex.Message}");
                return false;
            }
        }
        //End : GSM Is Connect
        //GSM Is Ready
        private bool GSMIsReadyWorkflow(bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            if (result)
            {
                this.GSMIsReadyExecute(TRY_READ_FAST, DELAY_SHORT_MS, ref result1, ref result2);
            }
            if (result)
            {
                result = this.GSMIsReadyCheck(result1, result2);
            }
            this.GSMIsReadySetValue(result, InternalValueSet);
            return result;
        }
        private void GSMIsReadyExecute(int loopCount, int Delay, ref bool result1, ref bool result2)
        {
            if (!this._enableDoubleReadValidation)
            {
                result2 = true;
            }
            while (loopCount > 0 && (!result1 || !result2))
            {
                Thread.Sleep(Delay);
                if (!result1)
                {
                    result1 = this.GSMIsReadyBase();
                    //major error check
                    if (this.BaseErrorCheck())
                    {
                        return;
                    }
                }
                else
                {
                    result2 = this.GSMIsReadyBase();
                    //major error check
                    if (this.BaseErrorCheck())
                    {
                        return;
                    }
                }
                loopCount--;
            }
        }
        private bool GSMIsReadyCheck(bool Result1, bool Result2)
        {
            return (Result1 && Result2);
        }
        private void GSMIsReadySetValue(bool GSMIsReadyNewValue, bool InternalValueSet)
        {
            if (InternalValueSet)
            {
                this._gsmIsReady = GSMIsReadyNewValue;
            }
        }
        private bool GSMIsReadyBase()
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.GSMIsReadyBaseDll();
            }
            return result;
        }
        private bool GSMIsReadyBaseDll()
        {
            try
            {
                bool result = this._dllWrapper.GsmIsReady();
                return (this.DllWrapperErrorCheck() && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"GSMIsReadyBaseDll Error : {ex.Message}");
                return false;
            }
        }
        //End : GSM Is Ready
        //End : Hardware Status

        //Hardware Property
        //IMEI Get
        private bool IMEIGetWorkflow(ref string IMEI, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            string imei1 = DeviceDefinitions.STRING_INVALID, imei2 = DeviceDefinitions.STRING_INVALID;
            if (result)
            {
                this.IMEIGetExecute(TRY_READ_NORMAL, DELAY_NORMAL2_MS, ref result1, ref result2, ref imei1, ref imei2);
            }
            if (result)
            {
                result = this.IMEIGetCheck(result1, result2, imei1, imei2);
            }
            if (result)
            {
                this.IMEIGetSetValue(imei1, ref IMEI, InternalValueSet);
            }
            else
            {
                this.IMEIGetSetValue(DeviceDefinitions.STRING_INVALID, ref IMEI, InternalValueSet);
            }
            return result;
        }
        private void IMEIGetExecute(int loopCount, int Delay, ref bool result1, ref bool result2, ref string imei1, ref string imei2)
        {
            //How Many continues fail accepted
            int resultCount = TRY_FAIL_TIME;
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
                    result1 = this.IMEIGetBase(ref imei1);
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
                else
                {
                    result2 = this.IMEIGetBase(ref imei2);
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
                imei2 = imei1;
            }
        }
        private bool IMEIGetCheck(bool Result1, bool Result2, string IMEI1, string IMEI2)
        {
            return (Result1 && Result2 && IMEI1 == IMEI2);
        }
        private void IMEIGetSetValue(string IMEINewValue, ref string IMEI, bool InternalValueSet)
        {
            IMEI = IMEINewValue;
            if (InternalValueSet)
            {
                this._imei = IMEINewValue;
            }
        }
        private bool IMEIGetBase(ref string IMEI)
        {
            bool result = true;
            string imei = DeviceDefinitions.STRING_INVALID;
            this.ErrorReset();
            if (result)
            {
                result = this.IMEIGetBaseDll(ref imei);
            }
            if (result)
            {
                result = this.IMEIGetBaseCheck(imei);
            }
            if (result)
            {
                this.IMEIGetBaseSetValue(imei, ref IMEI);
            }
            else
            {
                this.IMEIGetBaseSetValue(DeviceDefinitions.STRING_INVALID, ref IMEI);
            }
            return result;
        }
        private bool IMEIGetBaseDll(ref string IMEI)
        {
            try
            {
                bool result = this._dllWrapper.IMEI(out IMEI);
                bool result1 = this.DllWrapperErrorCheck();
                if (!result1 && !result)
                {
                    return this.ErrorSet(ERROR_CODE.IMEI , "IMEI = " + IMEI);
                }
                return (result1 && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"IMEIGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool IMEIGetBaseCheck(string IMEI)
        {
            try
            {
                if (IMEI.Length >= DeviceDefinitions.IMEI_LEN_MIN && IMEI.Length <= DeviceDefinitions.IMEI_LEN_MAX)
                {
                    return true;
                }
                else
                {
                    return this.ErrorSet(ERROR_CODE.IMEI, "IMEI = " + IMEI + " , IMEI len = " + IMEI.Length);
                }
            }
            catch (Exception ex)
            {
                return this.ErrorSet(ERROR_CODE.IMEI, "IMEIGetBaseCheck Error : " + ex.Message);
            }
        }
        private void IMEIGetBaseSetValue(string IMEINewValue, ref string IMEI)
        {
            IMEI = IMEINewValue;
        }
        //End : IMEI Get
        //Software Version Get
        private bool SoftwareVersionGetWorkflow(ref string SoftwareVersion, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            string softwareVer1 = DeviceDefinitions.STRING_INVALID, softwareVer2 = DeviceDefinitions.STRING_INVALID;
            if (result)
            {
                this.SoftwareVersionGetExecute(TRY_READ_FAST, DELAY_SHORT_MS, ref result1, ref result2, ref softwareVer1, ref softwareVer2);
            }
            if (result)
            {
                result = this.SoftwareVersionGetCheck(result1, result2, softwareVer1, softwareVer2);
            }
            if (result)
            {
                this.SoftwareVersionGetSetValue(softwareVer1, ref SoftwareVersion, InternalValueSet);
            }
            else
            {
                this.SoftwareVersionGetSetValue(DeviceDefinitions.STRING_INVALID, ref SoftwareVersion, InternalValueSet);
            }
            return result;
        }
        private void SoftwareVersionGetExecute(int loopCount, int Delay, ref bool result1, ref bool result2, ref string SoftwareVer1, ref string SoftwareVer2)
        {
            //How Many continues fail accepted
            int resultCount = TRY_FAIL_TIME;
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
                    result1 = this.SoftwareVersionGetBase(ref SoftwareVer1);
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
                    result2 = this.SoftwareVersionGetBase(ref SoftwareVer2);
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
                SoftwareVer2 = SoftwareVer1;
            }
        }
        private bool SoftwareVersionGetCheck(bool Result1, bool Result2, string SoftwareVer1, string SoftwareVer2)
        {
            return (Result1 && Result2 && SoftwareVer1 == SoftwareVer2);
        }
        private void SoftwareVersionGetSetValue(string SoftwareVersionNewValue, ref string SoftwareVersion, bool InternalValueSet)
        {
            SoftwareVersion = SoftwareVersionNewValue;
            if (InternalValueSet)
            {
                this._softwareVersion = SoftwareVersionNewValue;
            }
        }
        private bool SoftwareVersionGetBase(ref string SoftwareVersion)
        {
            bool result = true;
            string softwareVer = DeviceDefinitions.STRING_INVALID;
            this.ErrorReset();
            if (result)
            {
                result = this.SoftwareVersionGetBaseDll(ref softwareVer);
            }
            if (result)
            {
                result = this.SoftwareVersionGetBaseCheck(softwareVer);
            }
            if (result)
            {
                this.SoftwareVersionGetBaseSetValue(softwareVer, ref SoftwareVersion);
            }
            else
            {
                this.SoftwareVersionGetBaseSetValue(DeviceDefinitions.STRING_INVALID, ref SoftwareVersion);
            }
            return result;
        }
        private bool SoftwareVersionGetBaseDll(ref string SoftwareVersion)
        {
            try
            {
                bool result = this._dllWrapper.SoftwareVersion(out SoftwareVersion);
                bool result1 = this.DllWrapperErrorCheck();
                if (!result1 && !result)
                {
                    return this.ErrorSet(ERROR_CODE.SOFTWARE_VERSION, "Software Version = " + SoftwareVersion);
                }
                return (result1 && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"SoftwareVersionGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool SoftwareVersionGetBaseCheck(string SoftwareVersion)
        {
            return true;
        }
        private void SoftwareVersionGetBaseSetValue(string SoftwareVersionNewValue, ref string SoftwareVersion)
        {
            SoftwareVersion = SoftwareVersionNewValue;
        }
        //End : Software Version Get
        //Device Information Get
        private bool DeviceInformationGetWorkflow(ref string DeviceInformation, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            string deviceInfo1 = DeviceDefinitions.STRING_INVALID, deviceInfo2 = DeviceDefinitions.STRING_INVALID;
            if (result)
            {
                this.DeviceInformationGetExecute(TRY_READ_FAST, DELAY_SHORT_MS, ref result1, ref result2, ref deviceInfo1, ref deviceInfo2);
            }
            if (result)
            {
                result = this.DeviceInformationGetCheck(result1, result2, deviceInfo1, deviceInfo2);
            }
            if (result)
            {
                this.DeviceInformationGetSetValue(deviceInfo1, ref DeviceInformation, InternalValueSet);
            }
            else
            {
                this.DeviceInformationGetSetValue(DeviceDefinitions.STRING_INVALID, ref DeviceInformation, InternalValueSet);
            }
            return result;
        }
        private void DeviceInformationGetExecute(int loopCount, int Delay, ref bool result1, ref bool result2, ref string DeviceInfo1, ref string DeviceInfo2)
        {
            //How Many continues fail accepted
            int resultCount = TRY_FAIL_TIME;
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
                    result1 = this.DeviceInformationGetBase(ref DeviceInfo1);
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
                    result2 = this.DeviceInformationGetBase(ref DeviceInfo2);
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
                DeviceInfo2 = DeviceInfo1;
            }
        }
        private bool DeviceInformationGetCheck(bool Result1, bool Result2, string DeviceInfo1, string DeviceInfo2)
        {
            return (Result1 && Result2 && DeviceInfo1 == DeviceInfo2);
        }
        private void DeviceInformationGetSetValue(string DeviceInformationNewValue, ref string DeviceInformation, bool InternalValueSet)
        {
            DeviceInformation = DeviceInformationNewValue;
            if (InternalValueSet)
            {
                this._deviceInformation = DeviceInformationNewValue;
            }
        }
        private bool DeviceInformationGetBase(ref string DeviceInformation)
        {
            bool result = true;
            string deviceInfo = DeviceDefinitions.STRING_INVALID;
            this.ErrorReset();
            if (result)
            {
                result = this.DeviceInformationGetBaseDll(ref deviceInfo);
            }
            if (result)
            {
                result = this.DeviceInformationGetBaseCheck(deviceInfo);
            }
            if (result)
            {
                this.DeviceInformationGetBaseSetValue(deviceInfo, ref DeviceInformation);
            }
            else
            {
                this.DeviceInformationGetBaseSetValue(DeviceDefinitions.STRING_INVALID, ref DeviceInformation);
            }
            return result;
        }
        private bool DeviceInformationGetBaseDll(ref string DeviceInformation)
        {
            try
            {
                bool result = this._dllWrapper.DeviceInfo(out DeviceInformation);
                bool result1 = this.DllWrapperErrorCheck();
                if (!result1 && !result)
                {
                    return this.ErrorSet(ERROR_CODE.DEVICE_INFORMATION, "Device Information = " + DeviceInformation);
                }
                return (result1 && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"DeviceInformationGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool DeviceInformationGetBaseCheck(string DeviceInformation)
        {
            return true;
        }
        private void DeviceInformationGetBaseSetValue(string DeviceInformationNewValue, ref string DeviceInformation)
        {
            DeviceInformation = DeviceInformationNewValue;
        }
        //End : Device Information Get
        //End : Hardware Property

        //SIM Property
        //SimID Get
        private bool SimIDGetWorkflow(ref string SimID, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            string simID1 = DeviceDefinitions.STRING_INVALID, simID2 = DeviceDefinitions.STRING_INVALID;
            if (result)
            {
                this.SimIDGetExecute(TRY_READ_NORMAL, DELAY_NORMAL1_MS, ref result1, ref result2, ref simID1, ref simID2);
            }
            if (result)
            {
                result = this.SimIDGetCheck(result1, result2, simID1, simID2);
            }
            if (result)
            {
                this.SimIDGetSetValue(simID1, ref SimID, InternalValueSet);
            }
            else
            {
                this.SimIDGetSetValue(DeviceDefinitions.STRING_INVALID, ref SimID, InternalValueSet);
            }
            return result;
        }
        private void SimIDGetExecute(int loopCount, int Delay, ref bool result1, ref bool result2, ref string simID1, ref string simID2)
        {
            //How Many continues fail accepted
            int resultCount = TRY_FAIL_TIME;
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
                    result1 = this.SimIDGetBase(ref simID1);
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
                    result2 = this.SimIDGetBase(ref simID2);
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
                simID2 = simID1;
            }
        }
        private bool SimIDGetCheck(bool Result1, bool Result2, string SimID1, string SimID2)
        {
            return (Result1 && Result2 && SimID1 == SimID2);
        }
        private void SimIDGetSetValue(string SimIDNewValue, ref string SimID, bool InternalValueSet)
        {
            SimID = SimIDNewValue;
            if (InternalValueSet)
            {
                this._simID = SimIDNewValue;
            }
        }
        private bool SimIDGetBase(ref string SimID)
        {
            bool result = true;
            string simID = DeviceDefinitions.STRING_INVALID;
            this.ErrorReset();
            if (result)
            {
                result = this.SimIDGetBaseDll(ref simID);
            }
            if (result)
            {
                result = this.SimIDGetBaseCheck(simID);
            }
            if (result)
            {
                this.SimIDGetBaseSetValue(simID, ref SimID);
            }
            else
            {
                this.SimIDGetBaseSetValue(DeviceDefinitions.STRING_INVALID, ref SimID);
            }
            return result;
        }
        private bool SimIDGetBaseDll(ref string SimID)
        {
            try
            {
                bool result = this._dllWrapper.SimID(out SimID);
                bool result1 = this.DllWrapperErrorCheck();
                if (!result1 && !result)
                {
                    return this.ErrorSet(ERROR_CODE.SIM_ID, "SIM ID = " + SimID);
                }
                return (result1 && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"SimIDGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool SimIDGetBaseCheck(string SimID)
        {
            try
            {
                if (SimID.Length >= DeviceDefinitions.SIM_ID_LEN_MIN && SimID.Length <= DeviceDefinitions.SIM_ID_LEN_MAX)
                {
                    return true;
                }
                else
                {
                    return this.ErrorSet(ERROR_CODE.SIM_ID, "SIM ID = " + SimID + " , SIM ID Len = " + SimID.Length);
                }
            }
            catch (Exception ex)
            {
                return this.ErrorSet(ERROR_CODE.SIM_ID, "SimIDGetBaseCheck Error : " + ex.Message);
            }
        }
        private void SimIDGetBaseSetValue(string SimIDNewValue, ref string SimID)
        {
            SimID = SimIDNewValue;
        }
        //End : SimID Get
        //National Mobile ID Get
        private bool NationalMobileIDGetWorkflow(ref string NationalMobileID, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            string nMobileID1 = DeviceDefinitions.STRING_INVALID, nMobileID2 = DeviceDefinitions.STRING_INVALID;
            if (result)
            {
                this.NationalMobileIDGetExecute(TRY_READ_FAST, DELAY_SHORT_MS, ref result1, ref result2, ref nMobileID1, ref nMobileID2);
            }
            if (result)
            {
                result = this.NationalMobileIDGetCheck(result1, result2, nMobileID1, nMobileID2);
            }
            if (result)
            {
                this.NationalMobileIDGetSetValue(nMobileID1, ref NationalMobileID, InternalValueSet);
            }
            else
            {
                this.NationalMobileIDGetSetValue(DeviceDefinitions.STRING_INVALID, ref NationalMobileID, InternalValueSet);
            }
            return result;
        }
        private void NationalMobileIDGetExecute(int loopCount, int Delay, ref bool Result1, ref bool Result2, ref string NationalMobileID1, ref string NationalMobileID2)
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
                    Result1 = this.NationalMobileIDGetBase(ref NationalMobileID1);
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
                    Result2 = this.NationalMobileIDGetBase(ref NationalMobileID2);
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
                NationalMobileID2 = NationalMobileID1;
            }
        }
        private bool NationalMobileIDGetCheck(bool Result1, bool Result2, string NationalMobileID1, string NationalMobileID2)
        {
            return (Result1 && Result2 && NationalMobileID1 == NationalMobileID2);
        }
        private void NationalMobileIDGetSetValue(string NationalMobileIDNewValue, ref string NationalMobileID, bool InternalValueSet)
        {
            NationalMobileID = NationalMobileIDNewValue;
            if (InternalValueSet)
            {
                this._nationalMobileID = NationalMobileIDNewValue;
            }
        }
        private bool NationalMobileIDGetBase(ref string NationalMobileID)
        {
            bool result = true;
            string nMobileID = DeviceDefinitions.STRING_INVALID;
            this.ErrorReset();
            if (result)
            {
                result = this.NationalMobileIDGetBaseDll(ref nMobileID);
            }
            if (result)
            {
                result = this.NationalMobileIDGetBaseCheck(nMobileID);
            }
            if (result)
            {
                this.NationalMobileIDGetBaseSetValue(nMobileID, ref NationalMobileID);
            }
            else
            {
                this.NationalMobileIDGetBaseSetValue(DeviceDefinitions.STRING_INVALID, ref NationalMobileID);
            }
            return result;
        }
        private bool NationalMobileIDGetBaseDll(ref string NationalMobileID)
        {
            try
            {
                bool result = this._dllWrapper.NationalMobileID(out NationalMobileID);
                bool result1 = this.DllWrapperErrorCheck();
                if (!result1 && !result)
                {
                    return this.ErrorSet(ERROR_CODE.NATIONAL_MOBILE_ID, "National Mobile ID = " + NationalMobileID);
                }
                return (result1 && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"NationalMobileIDGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool NationalMobileIDGetBaseCheck(string NationalMobileID)
        {
            try
            {
                if (NationalMobileID.Length >= DeviceDefinitions.NATIONAL_MOBILE_ID_LEN_MIN && NationalMobileID.Length <= DeviceDefinitions.NATIONAL_MOBILE_ID_LEN_MAX)
                {
                    return true;
                }
                else
                {
                    return this.ErrorSet(ERROR_CODE.NATIONAL_MOBILE_ID, "National Mobile ID = " + NationalMobileID + " , National Mobile ID Len = " + NationalMobileID.Length);
                }
            }
            catch (Exception ex)
            {
                return this.ErrorSet(ERROR_CODE.NATIONAL_MOBILE_ID, "NationalMobileIDGetBaseCheck Error : " + ex.Message);
            }
        }
        private void NationalMobileIDGetBaseSetValue(string NationalMobileIDNewValue, ref string NationalMobileID)
        {
            NationalMobileID = NationalMobileIDNewValue;
        }
        //End : National Mobile ID Get
        //SimID Get
        private bool SimStatusGetWorkflow(ref DeviceDefinitions.SIM_STATUS SimStatus, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            DeviceDefinitions.SIM_STATUS simStatus1 = DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR, simStatus2 = DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR;
            if (result)
            {
                this.SimStatusGetExecute(TRY_READ_FAST, DELAY_NORMAL1_MS, ref result1, ref result2, ref simStatus1, ref simStatus2);
            }
            if (result)
            {
                result = this.SimStatusGetCheck(result1, result2, simStatus1, simStatus2);
            }
            if (result)
            {
                this.SimStatusGetSetValue(simStatus1, ref SimStatus, InternalValueSet);
            }
            else
            {
                this.SimStatusGetSetValue(DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR, ref SimStatus, InternalValueSet);
            }
            return result;
        }
        private void SimStatusGetExecute(int loopCount, int Delay, ref bool Result1, ref bool Result2,
                                     ref DeviceDefinitions.SIM_STATUS SimStatus1, ref DeviceDefinitions.SIM_STATUS SimStatus2)
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
                    Result1 = this.SimStatusGetBase(ref SimStatus1);
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
                    Result2 = this.SimStatusGetBase(ref SimStatus2);
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
                SimStatus2 = SimStatus1;
            }
        }
        private bool SimStatusGetCheck(bool Result1, bool Result2, DeviceDefinitions.SIM_STATUS SimStatus1, DeviceDefinitions.SIM_STATUS SimStatus2)
        {
            return (Result1 && Result2 && SimStatus1 == SimStatus2);
        }
        private void SimStatusGetSetValue(DeviceDefinitions.SIM_STATUS SimStatusNewValue, ref DeviceDefinitions.SIM_STATUS SimStatus, bool InternalValueSet)
        {
            SimStatus = SimStatusNewValue;
            if (InternalValueSet)
            {
                this._simStatus = SimStatusNewValue;
            }
        }
        private bool SimStatusGetBase(ref DeviceDefinitions.SIM_STATUS SimStatus)
        {
            bool result = true;
            DeviceDefinitions.SIM_STATUS simStatus = DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR;
            this.ErrorReset();
            if (result)
            {
                result = this.SimStatusGetBaseDll(ref simStatus);
            }
            if (result)
            {
                result = this.SimStatusGetBaseCheck(simStatus);
            }
            if (result)
            {
                this.SimStatusGetBaseSetValue(simStatus, ref SimStatus);
            }
            else
            {
                this.SimStatusGetBaseSetValue(DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR, ref SimStatus);
            }
            return result;
        }
        private bool SimStatusGetBaseDll(ref DeviceDefinitions.SIM_STATUS SimStatus)
        {
            try
            {
                bool result = this._dllWrapper.SimStatus(ref SimStatus);
                bool result1 = this.DllWrapperErrorCheck();
                if (!result1 && !result)
                {
                    return this.ErrorSet(ERROR_CODE.SIM_STATUS, "SIM Status = " + SimStatus.ToString());
                }
                return (result1 && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"SimStatusGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool SimStatusGetBaseCheck(DeviceDefinitions.SIM_STATUS SimStatus)
        {
            try
            {
                if (SimStatus == DeviceDefinitions.SIM_STATUS.READY)
                {
                    return true;
                }
                else
                {
                    return this.ErrorSet(ERROR_CODE.SIM_STATUS, "SIM Status = " + SimStatus.ToString());
                }
            }
            catch (Exception ex)
            {
                return this.ErrorSet(ERROR_CODE.SIM_ID, "SimStatusGetBaseCheck Error : " + ex.Message);
            }
        }
        private void SimStatusGetBaseSetValue(DeviceDefinitions.SIM_STATUS SimStatusNewValue, ref DeviceDefinitions.SIM_STATUS SimStatus)
        {
            SimStatus = SimStatusNewValue;
        }
        //End : SimID Get
        //SIM Property

        //Network Property
        //Signal Get
        private bool SignalGetWorkflow(ref int Signal, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            int signal1 = DeviceDefinitions.NUMBER_INVALID, signal2 = DeviceDefinitions.NUMBER_INVALID;
            if (result)
            {
                this.SignalGetExecute(TRY_READ_NETWORK_DATA, DELAY_NORMAL1_MS, ref result1, ref result2, ref signal1, ref signal2);
            }
            if (result)
            {
                result = this.SignalGetCheck(result1, result2, signal1, signal2);
            }
            if (result)
            {
                this.SignalGetSetValue(signal1, ref Signal, InternalValueSet);
            }
            else
            {
                this.SignalGetSetValue(DeviceDefinitions.NUMBER_INVALID, ref Signal, InternalValueSet);
            }
            return result;
        }
        private void SignalGetExecute(int loopCount, int Delay, ref bool Result1, ref bool Result2, ref int Signal1, ref int Signal2)
        {
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
                    this.TimeOutStart();
                    Result1 = this.SignalGetBase(ref Signal1);
                    //timeout and major error check
                    if (this.TimeOutEnd() || this.BaseErrorCheck())
                    {
                        return;
                    }
                    Result1 = this.SignalGetExecuteMinValueCheck(Result1 , Signal1);
                }
                //try for first correct result
                else
                {
                    this.TimeOutStart();
                    Result2 = this.SignalGetBase(ref Signal2);
                    //timeout and major error check
                    if (this.TimeOutEnd() || this.BaseErrorCheck())
                    {
                        return;
                    }
                    Result2 = this.SignalGetExecuteMinValueCheck(Result2, Signal2);
                }
                loopCount--;
            }
            if (!this._enableDoubleReadValidation)
            {
                Signal2 = Signal1;
            }
        }
        private bool SignalGetExecuteMinValueCheck(bool Result, int Signal)
        {
            return (Result && Signal >= this.SignalMinValue);
        }
        private bool SignalGetCheck(bool Result1, bool Result2, int Signal1, int Signal2)
        {
            return (Result1 && Result2 && Signal1 >= this.SignalMinValue && Signal2 >= this.SignalMinValue);
        }
        private void SignalGetSetValue(int SignalNewValue, ref int Signal, bool InternalValueSet)
        {
            Signal = SignalNewValue;
            if (InternalValueSet)
            {
                this._modemSignal = SignalNewValue;
            }
        }
        private bool SignalGetBase(ref int Signal)
        {
            bool result = true;
            int signal = DeviceDefinitions.NUMBER_INVALID;
            this.ErrorReset();
            if (result)
            {
                result = this.SignalGetBaseDll(ref signal);
            }
            if (result)
            {
                result = this.SignalGetBaseCheck(signal);
            }
            if (result)
            {
                this.SignalGetBaseSetValue(signal, ref Signal);
            }
            else
            {
                this.SignalGetBaseSetValue(DeviceDefinitions.NUMBER_INVALID, ref Signal);
            }
            return result;
        }
        private bool SignalGetBaseDll(ref int Signal)
        {
            try
            {
                bool result = this._dllWrapper.ModemSignal(ref Signal);
                bool result1 = this.DllWrapperErrorCheck();
                if (!result && !result1)
                {
                    return this.ErrorSet(ERROR_CODE.GSM_SIGNAL, "Signal = " + Signal.ToString());
                }
                return (result && result1);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"SignalGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool SignalGetBaseCheck(int Signal)
        {
            try
            {
                if (Signal >= DeviceDefinitions.SIGNAL_MIN && Signal <= DeviceDefinitions.SIGNAL_MAX)
                {
                    return true;
                }
                else
                {
                    return this.ErrorSet(ERROR_CODE.GSM_SIGNAL, "Signal = " + Signal);
                }
            }
            catch (Exception ex)
            {
                return this.ErrorSet(ERROR_CODE.GSM_SIGNAL, "SignalGetBaseCheck Error : " + ex.Message);
            }
        }
        private void SignalGetBaseSetValue(int SignalNewValue, ref int Signal)
        {
            Signal = SignalNewValue;
        }
        //End : Siganl Get
        //SMS Center Get
        private bool SMSCenterGetWorkflow(ref string SMSCenter, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            string smsCenter1 = DeviceDefinitions.STRING_INVALID, smsCenter2 = DeviceDefinitions.STRING_INVALID;
            if (result)
            {
                this.SMSCenterGetExecute(TRY_READ_NETWORK_DATA, DELAY_NORMAL1_MS, ref result1, ref result2, ref smsCenter1, ref smsCenter2);
            }
            if (result)
            {
                result = this.SMSCenterGetCheck(result1, result2, smsCenter1, smsCenter2);
            }
            if (result)
            {
                this.SMSCenterGetSetValue(smsCenter1, ref SMSCenter, InternalValueSet);
            }
            else
            {
                this.SMSCenterGetSetValue(DeviceDefinitions.STRING_INVALID, ref SMSCenter, InternalValueSet);
            }
            return result;
        }
        private void SMSCenterGetExecute(int loopCount, int Delay, ref bool Result1, ref bool Result2, ref string SmsCenter1, ref string SmsCenter2)
        {
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
                    Result1 = this.SMSCenterGetBase(ref SmsCenter1);
                    //major error check
                    if (this.BaseErrorCheck())
                    {
                        return;
                    }
                }
                else
                {
                    Result2 = this.SMSCenterGetBase(ref SmsCenter2);
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
                SmsCenter2 = SmsCenter1;
            }
        }
        private bool SMSCenterGetCheck(bool Result1, bool Result2, string SmsCenter1, string SmsCenter2)
        {
            return (Result1 && Result2 && SmsCenter1 == SmsCenter2);
        }
        private void SMSCenterGetSetValue(string SmsCenterNewValue, ref string SMSCenter, bool InternalValueSet)
        {
            SMSCenter = SmsCenterNewValue;
            if (InternalValueSet)
            {
                this._smsCenter = SmsCenterNewValue;
            }
        }
        private bool SMSCenterGetBase(ref string SMSCenter)
        {
            bool result = true;
            string smsCenter = DeviceDefinitions.STRING_INVALID;
            this.ErrorReset();
            if (result)
            {
                result = this.SMSCenterGetBaseDll(ref smsCenter);
            }
            if (result)
            {
                result = this.SMSCenterGetBaseCheck(smsCenter);
            }
            if (result)
            {
                this.SMSCenterGetBaseSetValue(smsCenter, ref SMSCenter);
            }
            else
            {
                this.SMSCenterGetBaseSetValue(DeviceDefinitions.STRING_INVALID, ref SMSCenter);
            }
            return result;
        }
        private bool SMSCenterGetBaseDll(ref string SMSCenter)
        {
            try
            {
                bool result = this._dllWrapper.SmsCenterGet(out SMSCenter);
                bool result1 = this.DllWrapperErrorCheck();
                if (!result1 && !result)
                {
                    return this.ErrorSet(ERROR_CODE.SMS_CENTER_GET, "SMS Center = " + SMSCenter);
                }
                return (result1 && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"SMSCenterGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool SMSCenterGetBaseCheck(string SMSCenter)
        {
            return true;
        }
        private void SMSCenterGetBaseSetValue(string SMSCenterNewValue, ref string SMSCenter)
        {
            SMSCenter = SMSCenterNewValue;
        }
        //End : SMS Center Get
        //SMS Center Get
        private bool SMSCenterSetWorkflow(string SMSCenter, bool SMSCenterCheck, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            string smsCenter1 = DeviceDefinitions.STRING_INVALID, smsCenter2 = DeviceDefinitions.STRING_INVALID;
            if (result)
            {
                result = this.SMSCenterSetExecute(SMSCenter, DELAY_NORMAL1_MS);
            }
            if (result && SMSCenterCheck)
            {
                this.SMSCenterGetExecute(TRY_READ_NETWORK_DATA, DELAY_NORMAL1_MS, ref result1, ref result2, ref smsCenter1, ref smsCenter2);
                result = this.SMSCenterSetCheck(result1, result2, smsCenter1, smsCenter2, SMSCenter);
            }
            if (SMSCenterCheck)
            {
                if (result)
                {
                    this.SMSCenterSetSetValue(smsCenter1, InternalValueSet);
                }
                else
                {
                    this.SMSCenterSetSetValue(DeviceDefinitions.STRING_INVALID, InternalValueSet);
                }
            }
            return result;
        }
        private bool SMSCenterSetExecute(string SMSCenter, int Delay)
        {
            Thread.Sleep(Delay);
            return this.SMSCenterSetBase(SMSCenter);
        }
        private bool SMSCenterSetCheck(bool Result1, bool Result2, string SmsCenter1, string SmsCenter2, string SMSCenter)
        {
            return (Result1 && Result2 && SmsCenter1 == SMSCenter && SmsCenter2 == SMSCenter);
        }
        private void SMSCenterSetSetValue(string SmsCenterNewValue, bool InternalValueSet)
        {
            if (InternalValueSet)
            {
                this._smsCenter = SmsCenterNewValue;
            }
        }
        private bool SMSCenterSetBase(string SMSCenter)
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.SMSCenterSetBaseDll(SMSCenter);
            }
            return result;
        }
        private bool SMSCenterSetBaseDll(string SMSCenter)
        {
            try
            {
                this._dllWrapper.SmsCenterSet(SMSCenter);
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"SMSCenterSetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        //End : SMS Center Set
        //Network Name Get
        private bool NetworkNameGetWorkflow(ref string NetworkName, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            string netName1 = DeviceDefinitions.STRING_INVALID, netName2 = DeviceDefinitions.STRING_INVALID;
            if (result)
            {
                this.NetworkNameGetExecute(TRY_READ_NETWORK_DATA, DELAY_NORMAL1_MS, ref result1, ref result2, ref netName1, ref netName2);
            }
            if (result)
            {
                result = this.NetworkNameGetCheck(result1, result2, netName1, netName2);
            }
            if (result)
            {
                this.NetworkNameGetSetValue(netName1, ref NetworkName, InternalValueSet);
            }
            else
            {
                this.NetworkNameGetSetValue(DeviceDefinitions.STRING_INVALID, ref NetworkName, InternalValueSet);
            }
            return result;
        }
        private void NetworkNameGetExecute(int loopCount, int Delay, ref bool Result1, ref bool Result2, ref string NetName1, ref string NetName2)
        {
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
                if (!Result1)
                {
                    Result1 = this.NetworkNameGetBase(ref NetName1);
                    //major error check
                    if (this.BaseErrorCheck())
                    {
                        return;
                    }
                }
                else
                {
                    Result2 = this.NetworkNameGetBase(ref NetName2);
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
                NetName2 = NetName1;
            }
        }
        private bool NetworkNameGetCheck(bool Result1, bool Result2, string NetName1, string NetName2)
        {
            return (Result1 && Result2 && NetName1 == NetName2);
        }
        private void NetworkNameGetSetValue(string NetworkNameNewValue, ref string NetworkName, bool InternalValueSet)
        {
            NetworkName = NetworkNameNewValue;
            if (InternalValueSet)
            {
                this._networkName = NetworkNameNewValue;
            }
        }
        private bool NetworkNameGetBase(ref string NetworkName)
        {
            bool result = true;
            string netName = DeviceDefinitions.STRING_INVALID;
            this.ErrorReset();
            if (result)
            {
                result = this.NetworkNameGetBaseDll(ref netName);
            }
            if (result)
            {
                result = this.NetworkNameGetBaseCheck(netName);
            }
            if (result)
            {
                this.NetworkNameGetBaseSetValue(netName, ref NetworkName);
            }
            else
            {
                this.NetworkNameGetBaseSetValue(DeviceDefinitions.STRING_INVALID, ref NetworkName);
            }
            return result;
        }
        private bool NetworkNameGetBaseDll(ref string NetworkName)
        {
            try
            {
                bool result = this._dllWrapper.SimNetworkGet(out NetworkName);
                bool result1 = this.DllWrapperErrorCheck();
                if (!result1 && !result)
                {
                    return this.ErrorSet(ERROR_CODE.NETWORK_NAME_GET, "Network Name = " + NetworkName);
                }
                return (result1 && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"NetworkNameGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool NetworkNameGetBaseCheck(string NetworkName)
        {
            return true;
        }
        private void NetworkNameGetBaseSetValue(string NetworkNameNewValue, ref string NetworkName)
        {
            NetworkName = NetworkNameNewValue;
        }
        //End : Network Name Get
        //Network Name Set
        private bool NetworkNameSetWorkflow(bool NetworkCheck, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            string netName1 = DeviceDefinitions.STRING_INVALID, netName2 = DeviceDefinitions.STRING_INVALID;
            if (result)
            {
                result = this.NetworkNameSetExecute(DELAY_NORMAL1_MS);
            }
            if (result && NetworkCheck)
            {
                this.NetworkNameGetExecute(TRY_READ_NETWORK_DATA, DELAY_NORMAL1_MS, ref result1, ref result2, ref netName1, ref netName2);
                result = this.NetworkNameGetCheck(result1, result2, netName1, netName2);
            }
            if (NetworkCheck)
            {
                if (result)
                {
                    this.NetworkNameSetSetValue(netName1, InternalValueSet);
                }
                else
                {
                    this.NetworkNameSetSetValue(DeviceDefinitions.STRING_INVALID, InternalValueSet);
                }
            }
            return result;
        }
        private bool NetworkNameSetExecute(int Delay)
        {
            Thread.Sleep(Delay);
            return this.NetworkNameSetBase();
        }
        private void NetworkNameSetSetValue(string NetworkNameNewValue, bool InternalValueSet)
        {
            if (InternalValueSet)
            {
                this._networkName = NetworkNameNewValue;
            }
        }
        private bool NetworkNameSetBase()
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.NetworkNameSetBaseDll();
            }
            return result;
        }
        private bool NetworkNameSetBaseDll()
        {
            try
            {
                this._dllWrapper.SIMNetworkSelect();
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"NetworkNameSetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        //End : Network Name Get
        //Network Property

        //Property -> UnKnown level
        //GSMDateTime Get
        private bool GSMDateTimeGetWorkflow(ref string GSMDateTime, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            string dateTime1 = DeviceDefinitions.STRING_INVALID, dateTime2 = DeviceDefinitions.STRING_INVALID;
            if (result)
            {
                this.GSMDateTimeGetExecute(TRY_READ_FAST, DELAY_SHORT_MS, ref result1, ref result2, ref dateTime1, ref dateTime2);
            }
            if (result)
            {
                result = this.GSMDateTimeGetCheck(result1, result2, dateTime1, dateTime2);
            }
            if (result)
            {
                this.GSMDateTimeGetSetValue(dateTime1, ref GSMDateTime, InternalValueSet);
            }
            else
            {
                this.GSMDateTimeGetSetValue(DeviceDefinitions.STRING_INVALID, ref GSMDateTime, InternalValueSet);
            }
            return result;
        }
        private void GSMDateTimeGetExecute(int loopCount, int Delay, ref bool Result1, ref bool Result2, ref string DateTime1, ref string DateTime2)
        {
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
                    Result1 = this.GSMDateTimeGetBase(ref DateTime1);
                    //major error check
                    if (this.BaseErrorCheck())
                    {
                        return;
                    }
                }
                //try for second correct result
                else
                {
                    Result2 = this.GSMDateTimeGetBase(ref DateTime2);
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
                DateTime2 = DateTime1;
            }
        }
        private bool GSMDateTimeGetCheck(bool Result1, bool Result2, string DateTime1, string DateTime2)
        {
            return (Result1 && Result2 && DateTime1 == DateTime2);
        }
        private void GSMDateTimeGetSetValue(string GSMDateTimeNewValue, ref string GSMDateTime, bool InternalValueSet)
        {
            GSMDateTime = GSMDateTimeNewValue;
            if (InternalValueSet)
            {
                this._gsmDateTime = GSMDateTimeNewValue;
            }
        }
        private bool GSMDateTimeGetBase(ref string GSMDateTime)
        {
            bool result = true;
            string dateTime = DeviceDefinitions.STRING_INVALID;
            this.ErrorReset();
            if (result)
            {
                result = this.GSMDateTimeGetBaseDll(ref dateTime);
            }
            if (result)
            {
                result = this.GSMDateTimeGetBaseCheck(dateTime);
            }
            if (result)
            {
                this.GSMDateTimeGetBaseSetValue(dateTime, ref GSMDateTime);
            }
            else
            {
                this.GSMDateTimeGetBaseSetValue(DeviceDefinitions.STRING_INVALID, ref GSMDateTime);
            }
            return result;
        }
        private bool GSMDateTimeGetBaseDll(ref string GSMDateTime)
        {
            try
            {
                bool result = this._dllWrapper.GsmDateTimeGet(out GSMDateTime);
                bool result1 = this.DllWrapperErrorCheck();
                if (!result1 && !result)
                {
                    return this.ErrorSet(ERROR_CODE.GSM_DATE_TIME_GET, "GSM Date Time = " + GSMDateTime);
                }
                return (result1 && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"GSMDateTimeGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool GSMDateTimeGetBaseCheck(string GSMDateTime)
        {
            return true;
        }
        private void GSMDateTimeGetBaseSetValue(string GSMDateTimeNewValue, ref string GSMDateTime)
        {
            GSMDateTime = GSMDateTimeNewValue;
        }
        //End : GSMDateTime Get
        //GSMDateTime Get
        /*
        public bool GSMDateTimeSet()
        {
            //set current date time in persian calender
            return this.GSMDateTimeSet(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second, true, true);
        }
        */
        private bool GSMDateTimeSetWorkflow(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second,
                                            bool CheckDateTime, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            string dateTime1 = DeviceDefinitions.STRING_INVALID, dateTime2 = DeviceDefinitions.STRING_INVALID, datetimeInput = DeviceDefinitions.STRING_INVALID;
            if (result)
            {
                result = this.GSMDateTimeSetExecute(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second, DELAY_NORMAL1_MS);
            }
            if (result && CheckDateTime)
            {
                this.GSMDateTimeGetExecute(TRY_READ_FAST, DELAY_SHORT_MS, ref result1, ref result2, ref dateTime1, ref dateTime2);
            }
            if (result && CheckDateTime)
            {
                datetimeInput = this.BuildGsmDateTime(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second);
                result = this.GSMDateTimeSetCheck(result1, result2, dateTime1, dateTime2, datetimeInput);
            }
            if (result)
            {
                this.GSMDateTimeSetSetValue(datetimeInput, InternalValueSet);
            }
            return result;
        }
        private bool GSMDateTimeSetExecute(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second , int Delay)
        {
            Thread.Sleep(Delay);
            return this.GSMDateTimeSetBase(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second);
        }
        private bool GSMDateTimeSetCheck(bool Result1, bool Result2, string DateTime1, string DateTime2 , string DatetimeInput)
        {
            return (Result1 && Result2);
        }
        private void GSMDateTimeSetSetValue(string GSMDateTimeNewValue, bool InternalValueSet)
        {
            if (InternalValueSet)
            {
                this._gsmDateTime = GSMDateTimeNewValue;
            }
        }
        private bool GSMDateTimeSetBase(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second)
        {
            bool result = true;
            this.ErrorReset();
            if (result)
            {
                result = this.GSMDateTimeSetBaseInputCheck(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second);
            }
            if (result)
            {
                result = this.GSMDateTimeSetBaseDll(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second);
            }
            return result;
        }
        private bool GSMDateTimeSetBaseInputCheck(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second)
        {
            try
            {
                if (GSM_Year >= DeviceDefinitions.DATETIME_YEAR_MIN && GSM_Year <= DeviceDefinitions.DATETIME_YEAR_MAX &&
                    GSM_Month >= DeviceDefinitions.DATETIME_MON_MIN && GSM_Month <= DeviceDefinitions.DATETIME_MON_MAX &&
                    GSM_Day >= DeviceDefinitions.DATETIME_DAY_MIN && GSM_Day <= DeviceDefinitions.DATETIME_DAY_MAX &&
                    GSM_Hour >= DeviceDefinitions.DATETIME_HOUR_MIN && GSM_Hour <= DeviceDefinitions.DATETIME_HOUR_MAX &&
                    GSM_Minute >= DeviceDefinitions.DATETIME_MINUTE_MIN && GSM_Minute <= DeviceDefinitions.DATETIME_MINUTE_MAX &&
                    GSM_Second >= DeviceDefinitions.DATETIME_SECOND_MIN && GSM_Second <= DeviceDefinitions.DATETIME_SECOND_MAX)
                {
                    return true;
                }
                else
                {
                    return this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Year = " + GSM_Year + " , Month = " + GSM_Month + " , Day = " + GSM_Day +
                                         " , Hour = " + GSM_Hour + ", Min = " + GSM_Minute + " , Sec = " + GSM_Second);
                }
            }
            catch(Exception ex)
            {
                return this.ErrorSet(ERROR_CODE.GSM_DATE_TIME_SET, "GSMDateTimeSetBaseInputCheck Error : " + ex.Message);
            }
        }
        private bool GSMDateTimeSetBaseDll(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second)
        {
            try
            {
                this._dllWrapper.GSMDateTimeSet(Convert.ToInt16(GSM_Year), Convert.ToInt16(GSM_Month), Convert.ToInt16(GSM_Day), Convert.ToInt16(GSM_Hour),
                                                Convert.ToInt16(GSM_Minute), Convert.ToInt16(GSM_Second));
                return this.DllWrapperErrorCheck();
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"GSMDateTimeGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        //End : GSMDateTime Set
        //GSM Status Get
        private bool GSMStatusGetWorkflow(ref DeviceDefinitions.GSM_STATUS GsmStatus, ref int GsmStatusValue, ref string GsmStatusMessage, bool InternalValueSet)
        {
            bool result = true, result1 = false, result2 = false;
            DeviceDefinitions.GSM_STATUS GsmStatus1 = DeviceDefinitions.GSM_STATUS.ERROR, GsmStatus2 = DeviceDefinitions.GSM_STATUS.ERROR;
            int GsmStatusValue1 = DeviceDefinitions.NUMBER_INVALID, GsmStatusValue2 = DeviceDefinitions.NUMBER_INVALID;
            string GsmStatusMessage1 = DeviceDefinitions.STRING_INVALID, GsmStatusMessage2 = DeviceDefinitions.STRING_INVALID;
            if (result)
            {
                this.GSMStatusGetExecute(TRY_READ_NORMAL, DELAY_NORMAL1_MS, ref result1, ref result2, ref GsmStatus1, ref GsmStatus2, ref GsmStatusValue1,
                                        ref GsmStatusValue2, ref GsmStatusMessage1, ref GsmStatusMessage2);
            }
            if (result)
            {
                result = this.GSMStatusGetCheck(result1, result2, GsmStatus1, GsmStatus2, GsmStatusValue1, GsmStatusValue2, GsmStatusMessage1, GsmStatusMessage2);
            }
            if (result)
            {
                this.GSMStatusGetSetValue(GsmStatus1, GsmStatusValue1, GsmStatusMessage1, ref GsmStatus, ref GsmStatusValue, ref GsmStatusMessage, InternalValueSet);
            }
            else
            {
                this.GSMStatusGetSetValue(DeviceDefinitions.GSM_STATUS.ERROR, (int)DeviceDefinitions.GSM_STATUS.ERROR, DeviceDefinitions.STRING_INVALID,
                                          ref GsmStatus, ref GsmStatusValue, ref GsmStatusMessage, InternalValueSet);
            }
            return result;
        }
        private void GSMStatusGetExecute(int loopCount, int Delay, ref bool Result1, ref bool Result2, ref DeviceDefinitions.GSM_STATUS GsmStatus1,
                                        ref DeviceDefinitions.GSM_STATUS GsmStatus2, ref int GsmStatusValue1, ref int GsmStatusValue2,
                                        ref string GsmStatusMessage1, ref string GsmStatusMessage2)
        {
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
                    Result1 = this.GSMStatusGetBase(ref GsmStatus1, ref GsmStatusValue1, ref GsmStatusMessage1);
                    //major error check
                    if (this.BaseErrorCheck())
                    {
                        return;
                    }
                }
                else
                {
                    Result2 = this.GSMStatusGetBase(ref GsmStatus2, ref GsmStatusValue2, ref GsmStatusMessage2);
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
                GsmStatus2 = GsmStatus1;
                GsmStatusValue2 = GsmStatusValue1;
                GsmStatusMessage2 = GsmStatusMessage1;
            }
        }
        private bool GSMStatusGetCheck(bool Result1, bool Result2, DeviceDefinitions.GSM_STATUS GsmStatus1, DeviceDefinitions.GSM_STATUS GsmStatus2,
                                       int GsmStatusValue1, int GsmStatusValue2, string GsmStatusMessage1, string GsmStatusMessage2)
        {
            return (Result1 && Result2 && GsmStatus1 == GsmStatus2 && GsmStatusValue1 == GsmStatusValue2);
        }
        private void GSMStatusGetSetValue(DeviceDefinitions.GSM_STATUS GsmStatusNewValue, int GsmStatusValueNewValue, string GsmStatusMessageNewValue,
                                     ref DeviceDefinitions.GSM_STATUS GsmStatus, ref int GsmStatusValue, ref string GsmStatusMessage, bool InternalValueSet)
        {
            GsmStatus = GsmStatusNewValue;
            GsmStatusValue = GsmStatusValueNewValue;
            GsmStatusMessage = GsmStatusMessageNewValue;
            if (InternalValueSet)
            {
                this._gsmStatus = GsmStatusNewValue;
                this._gsmStatusValue = GsmStatusValueNewValue;
                this._gsmStatusMessage = GsmStatusMessageNewValue;
            }
        }
        private bool GSMStatusGetBase(ref DeviceDefinitions.GSM_STATUS GsmStatus, ref int GsmStatusValue, ref string GsmStatusMessage)
        {
            bool result = true;
            int gsmStatusValue = (int)DeviceDefinitions.GSM_STATUS.ERROR;
            string gsmStatusMessage = DeviceDefinitions.STRING_INVALID;
            this.ErrorReset();
            if (result)
            {
                result = this.GSMStatusGetBaseDll(ref gsmStatusValue, ref gsmStatusMessage);
            }
            if (result)
            {
                result = this.GSMStatusGetBaseCheck(gsmStatusValue);
            }
            if (result)
            {
                this.GSMStatusGetBaseSetValue(gsmStatusValue, gsmStatusMessage, ref GsmStatus, ref GsmStatusValue, ref GsmStatusMessage);
            }
            else
            {
                this.GSMStatusGetBaseSetValue((int)DeviceDefinitions.GSM_STATUS.ERROR, DeviceDefinitions.STRING_INVALID,
                                              ref GsmStatus, ref GsmStatusValue, ref GsmStatusMessage);
            }
            return result;
        }
        private bool GSMStatusGetBaseDll(ref int GsmStatusValue, ref string GsmStatusMessage)
        {
            try
            {
                bool result = this._dllWrapper.GsmStatus(ref GsmStatusValue, out GsmStatusMessage);
                bool result1 = this.DllWrapperErrorCheck();
                if (!result1 && !result)
                {
                    return this.ErrorSet(ERROR_CODE.GSM_STATUS, "GSM Status = " + GsmStatusValue.ToString() + " - GSM Message = " + GsmStatusMessage);
                }
                return (result1 && result);
            }
            catch (Exception ex)
            {
                this.DllWrapperErrorCheckSetValue($"GSMStatusGetBaseDll Error : {ex.Message}");
                return false;
            }
        }
        private bool GSMStatusGetBaseCheck(int GsmStatusValue)
        {
            try
            {
                if (GsmStatusValue == (int)DeviceDefinitions.GSM_STATUS.READY || GsmStatusValue == (int)DeviceDefinitions.GSM_STATUS.STARTUP)
                {
                    return true;
                }
                else
                {
                    return this.ErrorSet(ERROR_CODE.GSM_STATUS, "Gsm Status Value = " + GsmStatusValue);
                }
            }
            catch (Exception ex)
            {
                return this.ErrorSet(ERROR_CODE.GSM_STATUS, "GSMStatusGetBaseCheck Error : " + ex.Message);
            }
        }
        private void GSMStatusGetBaseSetValue(int GsmStatusValueNewValue, string GsmStatusMessageNewValue, ref DeviceDefinitions.GSM_STATUS GsmStatus,
                                              ref int GsmStatusValue, ref string GsmStatusMessage)
        {
            GsmStatusValue = GsmStatusValueNewValue;
            GsmStatusMessage = GsmStatusMessageNewValue;
            switch (GsmStatusValueNewValue)
            {
                case (int)DeviceDefinitions.GSM_STATUS.READY:
                    GsmStatus = DeviceDefinitions.GSM_STATUS.READY;
                    break;

                case (int)DeviceDefinitions.GSM_STATUS.STARTUP:
                    GsmStatus = DeviceDefinitions.GSM_STATUS.STARTUP;
                    break;

                case (int)DeviceDefinitions.GSM_STATUS.ERROR:
                    GsmStatus = DeviceDefinitions.GSM_STATUS.ERROR;
                    break;

                default:
                    GsmStatus = DeviceDefinitions.GSM_STATUS.UNKHOWN;
                    break;
            }
        }
        //End : GSM Status Get
        //End : Property -> UnKnown level

        //Helper
        private string BuildGsmDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            return $"{year:D2}/{month:D2}/{day:D2} {hour:D2}:{minute:D2}:{second:D2}";
        }
        //End : Helper

    }
}
