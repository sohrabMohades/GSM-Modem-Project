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
        private FUNC_RESULT GSMPowerGetWorkflow(ref bool Result, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            BOOL_TYPE result1 = BOOL_TYPE.UNKNOWN, result2 = BOOL_TYPE.UNKNOWN, outputResult = BOOL_TYPE.UNKNOWN;
            result = this.GSMPowerGetExecute((int)TRY_COUNT.POWER, (int)OPERATION_DELAY_MS.SHORT, ref funcResult1, ref funcResult2, ref result1, ref result2);
            result = this.XXXCheck(result, funcResult1, funcResult2, result1, result2);
            this.XXXSetValue(result, result1, BOOL_TYPE.UNKNOWN, ref outputResult, ref this._runtimeHardware.GsmPower, InternalValueSet);
            Result = this.XXXOutput(outputResult);
            return result;
        }
        private FUNC_RESULT GSMPowerGetExecute(int loopCount, int Delay, ref FUNC_RESULT FuncResult1, ref FUNC_RESULT FuncResult2, ref BOOL_TYPE Result1, ref BOOL_TYPE Result2)
        {
            // if Power is OFF never set FuncResult2 , Solution -> FuncResult2 = FUNC_RESULT.FAIL 
            FuncResult2 = FUNC_RESULT.SUCCESS;
            Result2 = BOOL_TYPE.FALSE;
            while (loopCount > 0 && (FuncResult1 != FUNC_RESULT.SUCCESS && FuncResult2 != FUNC_RESULT.SUCCESS) && (Result1 != BOOL_TYPE.TRUE && Result2 != BOOL_TYPE.TRUE))
            {
                Thread.Sleep(Delay);
                if (FuncResult1 != FUNC_RESULT.SUCCESS && Result1 != BOOL_TYPE.TRUE)
                {
                    FuncResult1 = this.GSMPowerGetBase(ref Result1);
                }
                else
                {
                    FuncResult2 = this.GSMPowerGetBase(ref Result2);
                }
                loopCount--;
                if (FuncResult1 == FUNC_RESULT.MAJOR_ERROR || FuncResult2 == FUNC_RESULT.MAJOR_ERROR)
                {
                    return FUNC_RESULT.MAJOR_ERROR;
                }
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT GSMPowerGetBase(ref BOOL_TYPE Result)
        {
            bool result = false;
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            funcResult = this.GSMPowerGetBaseDll(ref result);
            this.XXXBaseSetValue(funcResult, result, ref Result);
            return funcResult;
        }
        private FUNC_RESULT GSMPowerGetBaseDll(ref bool Result)
        {
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            Result = this._dllWrapper.GsmPowerGet();
            funcResult = this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
            return funcResult;
        }
        //End : GSM Power Get
        //GSM Power Set
        private FUNC_RESULT GSMPowerSetWorkflow(bool PowerSet, bool PowerCheck, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            BOOL_TYPE result1 = BOOL_TYPE.UNKNOWN, result2 = BOOL_TYPE.UNKNOWN, outputResult = BOOL_TYPE.UNKNOWN;
            result = this.GSMPowerSetExecute(PowerSet, (int)OPERATION_DELAY_MS.SHORT);
            if (result == FUNC_RESULT.SUCCESS && PowerCheck)
            {
                if (PowerSet)
                {
                    Thread.Sleep((int)OPERATION_DELAY_MS.AFTER_DEVICE_POWER_ON);
                }
                result = this.GSMPowerGetExecute((int)TRY_COUNT.POWER, (int)OPERATION_DELAY_MS.SHORT, ref funcResult1, ref funcResult2, ref result1, ref result2);
                result = this.XXXCheck(result, funcResult1, funcResult2, result1, result2);
                this.XXXSetValue(result, result1, BOOL_TYPE.UNKNOWN, ref outputResult, ref this._runtimeHardware.GsmPower, InternalValueSet);
            }
            return result;
        }
        private FUNC_RESULT GSMPowerSetExecute(bool Power, int Delay)
        {
            Thread.Sleep(Delay);
            return this.GSMPowerSetBase(Power);
        }
        private FUNC_RESULT GSMPowerSetBase(bool Power)
        {
            return this.GSMPowerSetBaseDll(Power);
        }
        private FUNC_RESULT GSMPowerSetBaseDll(bool Power)
        {
            this._dllWrapper.GsmPowerSet(Power);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : GSM Power Set
        //GSM Is Connect
        private FUNC_RESULT GSMIsConnectWorkflow(ref bool Result, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, funcResult1 = FUNC_RESULT.UNKNOWN;
            BOOL_TYPE result1 = BOOL_TYPE.UNKNOWN, outputResult = BOOL_TYPE.UNKNOWN;
            result = this.GSMIsConnectExecute((int)TRY_COUNT.IS_CONNECT, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref funcResult1, ref result1);
            this.XXXSetValue(result, result1, BOOL_TYPE.UNKNOWN, ref outputResult, ref this._runtimeHardware.GsmIsConnect, InternalValueSet);
            Result = this.XXXOutput(outputResult);
            return result;
        }
        private FUNC_RESULT GSMIsConnectExecute(int loopCount, int Delay, ref FUNC_RESULT FuncResult1, ref BOOL_TYPE Result1)
        {
            while (loopCount > 0 && (FuncResult1 != FUNC_RESULT.SUCCESS && Result1 != BOOL_TYPE.TRUE))
            {
                Thread.Sleep(Delay);
                FuncResult1 = this.GSMIsConnectBase(ref Result1);
                loopCount--;
                if (FuncResult1 == FUNC_RESULT.MAJOR_ERROR)
                {
                    return FUNC_RESULT.MAJOR_ERROR;
                }
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT GSMIsConnectBase(ref BOOL_TYPE Result)
        {
            bool result = false;
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            funcResult = this.GSMIsConnectBaseDll(ref result);
            this.XXXBaseSetValue(funcResult, result, ref Result);
            return funcResult;
        }
        private FUNC_RESULT GSMIsConnectBaseDll(ref bool Result)
        {
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            Result = this._dllWrapper.GsmIsConnect();
            funcResult = this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
            return funcResult;
        }
        //End : GSM Is Connect
        //GSM Is Ready
        private FUNC_RESULT GSMIsReadyWorkflow(ref bool Result, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            BOOL_TYPE result1 = BOOL_TYPE.UNKNOWN, result2 = BOOL_TYPE.UNKNOWN, outputResult = BOOL_TYPE.UNKNOWN;
            result = this.GSMIsReadyExecute((int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.SHORT, ref funcResult1, ref funcResult2, ref result1, ref result2);
            result = this.XXXCheck(result, funcResult1, funcResult2, result1, result2);
            this.XXXSetValue(result, result1, BOOL_TYPE.UNKNOWN, ref outputResult, ref this._runtimeHardware.GsmIsReady, InternalValueSet);
            Result = this.XXXOutput(outputResult);
            return result;
        }
        private FUNC_RESULT GSMIsReadyExecute(int loopCount, int Delay, ref FUNC_RESULT FuncResult1, ref FUNC_RESULT FuncResult2, ref BOOL_TYPE Result1, ref BOOL_TYPE Result2)
        {
            FuncResult2 = FUNC_RESULT.SUCCESS;
            Result2 = BOOL_TYPE.FALSE;
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = BOOL_TYPE.TRUE;
            }
            while (loopCount > 0 && (FuncResult1 != FUNC_RESULT.SUCCESS && FuncResult2 != FUNC_RESULT.SUCCESS) && (Result1 != BOOL_TYPE.TRUE && Result2 != BOOL_TYPE.TRUE))
            {
                Thread.Sleep(Delay);
                if (FuncResult1 != FUNC_RESULT.SUCCESS)
                {
                    FuncResult1 = this.GSMIsReadyBase(ref Result1);
                }
                else
                {
                    FuncResult2 = this.GSMIsReadyBase(ref Result2);
                }
                loopCount--;
                if (FuncResult1 == FUNC_RESULT.MAJOR_ERROR || FuncResult2 == FUNC_RESULT.MAJOR_ERROR)
                {
                    return FUNC_RESULT.MAJOR_ERROR;
                }
            }
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = Result1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT GSMIsReadyBase(ref BOOL_TYPE Result)
        {
            bool result = false;
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            funcResult = this.GSMIsReadyBaseDll(ref result);
            this.XXXBaseSetValue(funcResult, result, ref Result);
            return funcResult;
        }
        private FUNC_RESULT GSMIsReadyBaseDll(ref bool Result)
        {
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            Result = this._dllWrapper.GsmIsReady();
            funcResult = this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
            return funcResult;
        }
        //End : GSM Is Ready
        //End : Hardware Status

        //Hardware Property
        //IMEI Get
        private FUNC_RESULT IMEIGetWorkflow(ref string IMEI, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            string imei1 = DeviceDefinitions.STRING_INVALID, imei2 = DeviceDefinitions.STRING_INVALID;
            result = this.IMEIGetExecute((int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_LONG, ref result1, ref result2, ref imei1, ref imei2);
            result = this.XXXCheck(result, result1, result2, imei1, imei2);
            this.XXXSetValue(result, imei1, DeviceDefinitions.STRING_INVALID, ref IMEI, ref this._runtimeHardware.IMEI, InternalValueSet);
            return result;
        }
        private FUNC_RESULT IMEIGetExecute(int loopCount, int Delay, ref FUNC_RESULT Result1, ref FUNC_RESULT Result2, ref string IMEI1, ref string IMEI2)
        {
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = FUNC_RESULT.SUCCESS;
            }
            while (loopCount > 0 && (Result1 != FUNC_RESULT.SUCCESS || Result2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (Result1 != FUNC_RESULT.SUCCESS)
                {
                    Result1 = this.IMEIGetBase(ref IMEI1);
                    if (Result1 != FUNC_RESULT.SUCCESS)
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
                    Result2 = this.IMEIGetBase(ref IMEI2);
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
                IMEI2 = IMEI1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT IMEIGetBase(ref string IMEI)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            string imei = DeviceDefinitions.STRING_INVALID;
            result = this.IMEIGetBaseDll(ref imei);
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.IMEIGetBaseCheck(imei);
            }
            this.XXXBaseSetValue(result, imei, DeviceDefinitions.STRING_INVALID, ref IMEI);
            return result;
        }
        private FUNC_RESULT IMEIGetBaseDll(ref string IMEI)
        {
            if (this._dllWrapper.IMEI(out IMEI))
                return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR)
                return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.IMEI, "IMEI = " + IMEI);
            return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT IMEIGetBaseCheck(string IMEI)
        {
            if (IMEI.Length >= DeviceDefinitions.IMEI_LEN_MIN && IMEI.Length <= DeviceDefinitions.IMEI_LEN_MAX)
            {
                return FUNC_RESULT.SUCCESS;
            }
            this.ErrorSet(ERROR_CODE.IMEI, "IMEI = " + IMEI + " -> Invalid");
            return FUNC_RESULT.FAIL;
        }
        //End : IMEI Get
        //Software Version Get
        private FUNC_RESULT SoftwareVersionGetWorkflow(ref string SoftwareVersion, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            string softwareVer1 = DeviceDefinitions.STRING_INVALID, softwareVer2 = DeviceDefinitions.STRING_INVALID;
            result = this.SoftwareVersionGetExecute((int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.SHORT, ref result1, ref result2, ref softwareVer1, ref softwareVer2);
            result = this.XXXCheck(result, result1, result2, softwareVer1, softwareVer2);
            this.XXXSetValue(result, softwareVer1, DeviceDefinitions.STRING_INVALID, ref SoftwareVersion, ref this._runtimeHardware.SoftwareVersion, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SoftwareVersionGetExecute(int loopCount, int Delay, ref FUNC_RESULT Result1, ref FUNC_RESULT Result2, ref string SoftwareVer1, ref string SoftwareVer2)
        {
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = FUNC_RESULT.SUCCESS;
            }
            while (loopCount > 0 && (Result1 != FUNC_RESULT.SUCCESS || Result2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (Result1 != FUNC_RESULT.SUCCESS)
                {
                    Result1 = this.SoftwareVersionGetBase(ref SoftwareVer1);
                    if (Result1 != FUNC_RESULT.SUCCESS)
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
                    Result2 = this.SoftwareVersionGetBase(ref SoftwareVer2);
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
                SoftwareVer2 = SoftwareVer1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT SoftwareVersionGetBase(ref string SoftwareVersion)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            string softwareVer = DeviceDefinitions.STRING_INVALID;
            result = this.SoftwareVersionGetBaseDll(ref softwareVer);
            this.XXXBaseSetValue(result, softwareVer, DeviceDefinitions.STRING_INVALID, ref SoftwareVersion);
            return result;
        }
        private FUNC_RESULT SoftwareVersionGetBaseDll(ref string SoftwareVersion)
        {
            if (this._dllWrapper.SoftwareVersion(out SoftwareVersion))
                return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR)
                return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.SOFTWARE_VERSION, "Software Version = " + SoftwareVersion);
            return FUNC_RESULT.FAIL;
        }
        //End : Software Version Get
        //Device Information Get
        private FUNC_RESULT DeviceInformationGetWorkflow(ref string DeviceInformation, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            string deviceInfo1 = DeviceDefinitions.STRING_INVALID, deviceInfo2 = DeviceDefinitions.STRING_INVALID;
            result = this.DeviceInformationGetExecute((int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.SHORT, ref result1, ref result2, ref deviceInfo1, ref deviceInfo2);
            result = this.XXXCheck(result, result1, result2, deviceInfo1, deviceInfo2);
            this.XXXSetValue(result, deviceInfo1, DeviceDefinitions.STRING_INVALID, ref DeviceInformation, ref this._runtimeHardware.DeviceInformation, InternalValueSet);
            return result;
        }
        private FUNC_RESULT DeviceInformationGetExecute(int loopCount, int Delay, ref FUNC_RESULT Result1, ref FUNC_RESULT Result2, ref string DeviceInfo1, ref string DeviceInfo2)
        {
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = FUNC_RESULT.SUCCESS;
            }
            while (loopCount > 0 && (Result1 != FUNC_RESULT.SUCCESS || Result2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (Result1 != FUNC_RESULT.SUCCESS)
                {
                    Result1 = this.DeviceInformationGetBase(ref DeviceInfo1);
                    if (Result1 != FUNC_RESULT.SUCCESS)
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
                    Result2 = this.DeviceInformationGetBase(ref DeviceInfo2);
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
                DeviceInfo2 = DeviceInfo1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT DeviceInformationGetBase(ref string DeviceInformation)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            string deviceInfo = DeviceDefinitions.STRING_INVALID;
            result = this.DeviceInformationGetBaseDll(ref deviceInfo);
            this.XXXBaseSetValue(result, deviceInfo, DeviceDefinitions.STRING_INVALID, ref DeviceInformation);
            return result;
        }
        private FUNC_RESULT DeviceInformationGetBaseDll(ref string DeviceInformation)
        {
            if (this._dllWrapper.DeviceInfo(out DeviceInformation))
                return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR)
                return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.DEVICE_INFORMATION, "Device Information = " + DeviceInformation);
            return FUNC_RESULT.FAIL;
        }
        //End : Device Information Get
        //End : Hardware Property

        //SIM Property
        //SimID Get
        private FUNC_RESULT SimIDGetWorkflow(ref string SimID, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            string simID1 = DeviceDefinitions.STRING_INVALID, simID2 = DeviceDefinitions.STRING_INVALID;
            result = this.SimIDGetExecute((int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref simID1, ref simID2);
            result = this.XXXCheck(result, result1, result2, simID1, simID2);
            this.XXXSetValue(result, simID1, DeviceDefinitions.STRING_INVALID, ref SimID, ref this._runtimeSim.SIMID, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SimIDGetExecute(int loopCount, int Delay, ref FUNC_RESULT Result1, ref FUNC_RESULT Result2, ref string SimID1, ref string SimID2)
        {
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = FUNC_RESULT.SUCCESS;
            }
            while (loopCount > 0 && (Result1 != FUNC_RESULT.SUCCESS || Result2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (Result1 != FUNC_RESULT.SUCCESS)
                {
                    Result1 = this.SimIDGetBase(ref SimID1);
                    if (Result1 != FUNC_RESULT.SUCCESS)
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
                    Result2 = this.SimIDGetBase(ref SimID2);
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
                SimID2 = SimID1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT SimIDGetBase(ref string SimID)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            string simID = DeviceDefinitions.STRING_INVALID;
            result = this.SimIDGetBaseDll(ref simID);
            result = this.SimIDGetBaseCheck(simID);
            this.XXXBaseSetValue(result, simID, DeviceDefinitions.STRING_INVALID, ref SimID);
            return result;
        }
        private FUNC_RESULT SimIDGetBaseDll(ref string SimID)
        {
            if (this._dllWrapper.SimID(out SimID))
                return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR)
                return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.SIM_ID, "SIM ID = " + SimID);
            return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT SimIDGetBaseCheck(string SimID)
        {
            if (SimID.Length >= DeviceDefinitions.SIM_ID_LEN_MIN && SimID.Length <= DeviceDefinitions.SIM_ID_LEN_MAX)
            {
                return FUNC_RESULT.SUCCESS;
            }
            this.ErrorSet(ERROR_CODE.SIM_ID, "SIM ID = " + SimID + " , SIM ID Len = " + SimID.Length);
            return FUNC_RESULT.FAIL;
        }
        //End : SimID Get
        //National Mobile ID Get
        private FUNC_RESULT NationalMobileIDGetWorkflow(ref string NationalMobileID, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            string nMobileID1 = DeviceDefinitions.STRING_INVALID, nMobileID2 = DeviceDefinitions.STRING_INVALID;
            result = this.NationalMobileIDGetExecute((int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.SHORT, ref result1, ref result2, ref nMobileID1, ref nMobileID2);
            result = this.XXXCheck(result, result1, result2, nMobileID1, nMobileID2);
            this.XXXSetValue(result, nMobileID1, DeviceDefinitions.STRING_INVALID, ref NationalMobileID, ref this._runtimeSim.NationalMobileID, InternalValueSet);
            return result;
        }
        private FUNC_RESULT NationalMobileIDGetExecute(int loopCount, int Delay, ref FUNC_RESULT Result1, ref FUNC_RESULT Result2, ref string NationalMobileID1, ref string NationalMobileID2)
        {
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = FUNC_RESULT.SUCCESS;
            }
            while (loopCount > 0 && (Result1 != FUNC_RESULT.SUCCESS || Result2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (Result1 != FUNC_RESULT.SUCCESS)
                {
                    Result1 = this.NationalMobileIDGetBase(ref NationalMobileID1);
                    if (Result1 != FUNC_RESULT.SUCCESS)
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
                    Result2 = this.NationalMobileIDGetBase(ref NationalMobileID2);
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
                NationalMobileID2 = NationalMobileID1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT NationalMobileIDGetBase(ref string NationalMobileID)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            string nMobileID = DeviceDefinitions.STRING_INVALID;
            result = this.NationalMobileIDGetBaseDll(ref nMobileID);
            result = this.NationalMobileIDGetBaseCheck(nMobileID);
            this.XXXBaseSetValue(result, nMobileID, DeviceDefinitions.STRING_INVALID, ref NationalMobileID);
            return result;
        }
        private FUNC_RESULT NationalMobileIDGetBaseDll(ref string NationalMobileID)
        {
            if (this._dllWrapper.NationalMobileID(out NationalMobileID))
                return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR)
                return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.NATIONAL_MOBILE_ID, "National Mobile ID = " + NationalMobileID);
            return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT NationalMobileIDGetBaseCheck(string NationalMobileID)
        {
            if (NationalMobileID.Length >= DeviceDefinitions.NATIONAL_MOBILE_ID_LEN_MIN && NationalMobileID.Length <= DeviceDefinitions.NATIONAL_MOBILE_ID_LEN_MAX)
            {
                return FUNC_RESULT.SUCCESS;
            }
            this.ErrorSet(ERROR_CODE.NATIONAL_MOBILE_ID, "National Mobile ID = " + NationalMobileID + " , National Mobile ID Len = " + NationalMobileID.Length);
            return FUNC_RESULT.FAIL;
        }
        //End : National Mobile ID Get
        //Sim Status Get
        private FUNC_RESULT SimStatusGetWorkflow(ref DeviceDefinitions.SIM_STATUS SimStatus, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            DeviceDefinitions.SIM_STATUS simStatus1 = DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR, simStatus2 = DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR;
            result = this.SimStatusGetExecute((int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref simStatus1, ref simStatus2);
            result = this.XXXCheck(result, result1, result2, simStatus1, simStatus2);
            this.XXXSetValue(result, simStatus1, DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR, ref SimStatus, ref this._runtimeSim.SIMStatus, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SimStatusGetExecute(int loopCount, int Delay, ref FUNC_RESULT Result1, ref FUNC_RESULT Result2,
                                     ref DeviceDefinitions.SIM_STATUS SimStatus1, ref DeviceDefinitions.SIM_STATUS SimStatus2)
        {
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = FUNC_RESULT.SUCCESS;
            }
            while (loopCount > 0 && (Result1 != FUNC_RESULT.SUCCESS || Result2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (Result1 != FUNC_RESULT.SUCCESS)
                {
                    Result1 = this.SimStatusGetBase(ref SimStatus1);
                    if (Result1 != FUNC_RESULT.SUCCESS)
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
                    Result2 = this.SimStatusGetBase(ref SimStatus2);
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
                SimStatus2 = SimStatus1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT SimStatusGetBase(ref DeviceDefinitions.SIM_STATUS SimStatus)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            DeviceDefinitions.SIM_STATUS simStatus = DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR;
            result = this.SimStatusGetBaseDll(ref simStatus);
            result = this.SimStatusGetBaseCheck(simStatus);
            this.XXXBaseSetValue(result, simStatus, DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR, ref SimStatus);
            return result;
        }
        private FUNC_RESULT SimStatusGetBaseDll(ref DeviceDefinitions.SIM_STATUS SimStatus)
        {
            if (this._dllWrapper.SimStatus(ref SimStatus))
                return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR)
                return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.SIM_STATUS, "SIM Status = " + SimStatus.ToString());
            return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT SimStatusGetBaseCheck(DeviceDefinitions.SIM_STATUS SimStatus)
        {
            if (SimStatus == DeviceDefinitions.SIM_STATUS.READY)
            {
                return FUNC_RESULT.SUCCESS;
            }
            this.ErrorSet(ERROR_CODE.SIM_STATUS, "SIM Status = " + SimStatus.ToString());
            return FUNC_RESULT.FAIL;
        }
        //End : Sim Status Get
        //SIM Property

        //Network Property
        //Signal Get
        private FUNC_RESULT SignalGetWorkflow(ref int Signal, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN;
            int signal1 = DeviceDefinitions.NUMBER_INVALID;
            result = this.SignalGetExecute((int)TRY_COUNT.NETWORK_DATA, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref signal1);
            result = this.XXXCheck(result, result1, result1, signal1, signal1);
            this.XXXSetValue(result, signal1, DeviceDefinitions.NUMBER_INVALID, ref Signal, ref this._runtimeNetwork.ModemSignal, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SignalGetExecute(int loopCount, int Delay, ref FUNC_RESULT Result1, ref int Signal1)
        {
            while (loopCount > 0 && Result1 != FUNC_RESULT.SUCCESS)
            {
                Thread.Sleep(Delay);
                Result1 = this.SignalGetBase(ref Signal1);
                loopCount--;
                if (Result1 == FUNC_RESULT.MAJOR_ERROR)
                {
                    return FUNC_RESULT.MAJOR_ERROR;
                }
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT SignalGetBase(ref int Signal)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            int signal = DeviceDefinitions.NUMBER_INVALID;
            result = this.SignalGetBaseDll(ref signal);
            result = SignalGetBaseCheck(signal);
            this.XXXBaseSetValue(result, signal, DeviceDefinitions.NUMBER_INVALID, ref Signal);
            return result;
        }
        private FUNC_RESULT SignalGetBaseDll(ref int Signal)
        {
            if (this._dllWrapper.ModemSignal(ref Signal))
                return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR)
                return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.GSM_SIGNAL, "Signal = " + Signal.ToString());
            return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT SignalGetBaseCheck(int Signal)
        {
            if (Signal >= DeviceDefinitions.SIGNAL_MIN && Signal <= DeviceDefinitions.SIGNAL_MAX && Signal >= this._runtimeConfig.SignalMinAcceptableValue)
            {
                return FUNC_RESULT.SUCCESS;
            }
            this.ErrorSet(ERROR_CODE.GSM_SIGNAL, "Signal = " + Signal);
            return FUNC_RESULT.FAIL;
        }
        //End : Siganl Get
        //SMS Center Get
        private FUNC_RESULT SMSCenterGetWorkflow(ref string SMSCenter, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            string smsCenter1 = DeviceDefinitions.STRING_INVALID, smsCenter2 = DeviceDefinitions.STRING_INVALID;
            result = this.SMSCenterGetExecute((int)TRY_COUNT.NETWORK_DATA, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref smsCenter1, ref smsCenter2);
            result = this.XXXCheck(result, result1, result2, smsCenter1, smsCenter2);
            this.XXXSetValue(result, smsCenter1, DeviceDefinitions.STRING_INVALID, ref SMSCenter, ref this._runtimeNetwork.SMSCenter, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SMSCenterGetExecute(int loopCount, int Delay, ref FUNC_RESULT Result1, ref FUNC_RESULT Result2, ref string SmsCenter1, ref string SmsCenter2)
        {
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = FUNC_RESULT.SUCCESS;
            }
            while (loopCount > 0 && (Result1 != FUNC_RESULT.SUCCESS || Result2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (Result1 != FUNC_RESULT.SUCCESS)
                {
                    Result1 = this.SMSCenterGetBase(ref SmsCenter1);
                    if (Result1 != FUNC_RESULT.SUCCESS)
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
                    Result2 = this.SMSCenterGetBase(ref SmsCenter2);
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
                SmsCenter2 = SmsCenter1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT SMSCenterGetBase(ref string SMSCenter)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            string smsCenter = DeviceDefinitions.STRING_INVALID;
            result = this.SMSCenterGetBaseDll(ref smsCenter);
            this.XXXBaseSetValue(result, smsCenter, DeviceDefinitions.STRING_INVALID, ref SMSCenter);
            return result;
        }
        private FUNC_RESULT SMSCenterGetBaseDll(ref string SMSCenter)
        {
            if (this._dllWrapper.SmsCenterGet(out SMSCenter))
                return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR)
                return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.SMS_CENTER_GET, "SMS Center = " + SMSCenter);
            return FUNC_RESULT.FAIL;
        }
        //End : SMS Center Get
        //SMS Center Get
        private FUNC_RESULT SMSCenterSetWorkflow(string SMSCenter, bool SMSCenterCheck, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            string smsCenter1 = DeviceDefinitions.STRING_INVALID, smsCenter2 = DeviceDefinitions.STRING_INVALID, smsCenter = DeviceDefinitions.STRING_INVALID;
            result = this.SMSCenterSetExecute(SMSCenter, (int)OPERATION_DELAY_MS.NORMAL_SHORT);
            if (result == FUNC_RESULT.SUCCESS && SMSCenterCheck)
            {
                result = this.SMSCenterGetExecute((int)TRY_COUNT.NETWORK_DATA, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref smsCenter1, ref smsCenter2);
                result = this.XXXCheck(result, result1, result2, smsCenter1, smsCenter2);
            }
            if (SMSCenterCheck && (SMSCenter != smsCenter1))
            {
                result = FUNC_RESULT.MAJOR_ERROR;
            }
            this.XXXSetValue(result, smsCenter1, DeviceDefinitions.STRING_INVALID, ref smsCenter, ref this._runtimeNetwork.SMSCenter, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SMSCenterSetExecute(string SMSCenter, int Delay)
        {
            Thread.Sleep(Delay);
            return this.SMSCenterSetBase(SMSCenter);
        }
        private FUNC_RESULT SMSCenterSetBase(string SMSCenter)
        {
            return this.SMSCenterSetBaseDll(SMSCenter);
        }
        private FUNC_RESULT SMSCenterSetBaseDll(string SMSCenter)
        {
            this._dllWrapper.SmsCenterSet(SMSCenter);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : SMS Center Set
        //Network Name Get
        private FUNC_RESULT NetworkNameGetWorkflow(ref string NetworkName, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            string networkName1 = DeviceDefinitions.STRING_INVALID, networkName2 = DeviceDefinitions.STRING_INVALID;
            result = this.NetworkNameGetExecute((int)TRY_COUNT.NETWORK_DATA, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref networkName1, ref networkName2);
            result = this.XXXCheck(result, result1, result2, networkName1, networkName2);
            this.XXXSetValue(result, networkName1, DeviceDefinitions.STRING_INVALID, ref NetworkName, ref this._runtimeNetwork.NetworkName, InternalValueSet);
            return result;
        }
        private FUNC_RESULT NetworkNameGetExecute(int loopCount, int Delay, ref FUNC_RESULT Result1, ref FUNC_RESULT Result2, ref string NetworkName1, ref string NetworkName2)
        {
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = FUNC_RESULT.SUCCESS;
            }
            while (loopCount > 0 && (Result1 != FUNC_RESULT.SUCCESS || Result2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (Result1 != FUNC_RESULT.SUCCESS)
                {
                    Result1 = this.NetworkNameGetBase(ref NetworkName1);
                    if (Result1 != FUNC_RESULT.SUCCESS)
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
                    Result2 = this.NetworkNameGetBase(ref NetworkName2);
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
                NetworkName2 = NetworkName1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT NetworkNameGetBase(ref string NetworkName)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            string networkName = DeviceDefinitions.STRING_INVALID;
            result = this.NetworkNameGetBaseDll(ref NetworkName);
            this.XXXBaseSetValue(result, networkName, DeviceDefinitions.STRING_INVALID, ref NetworkName);
            return result;
        }
        private FUNC_RESULT NetworkNameGetBaseDll(ref string NetworkName)
        {
            if (this._dllWrapper.SimNetworkGet(out NetworkName))
                return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR)
                return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.NETWORK_NAME_GET, "Network Name = " + NetworkName);
            return FUNC_RESULT.FAIL;
        }
        //End : Network Name Get
        //Network Name Set
        private FUNC_RESULT NetworkNameSetWorkflow(bool NetworkCheck, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            string networkName1 = DeviceDefinitions.STRING_INVALID, networkName2 = DeviceDefinitions.STRING_INVALID, networkName = DeviceDefinitions.STRING_INVALID;
            result = this.NetworkNameSetExecute((int)OPERATION_DELAY_MS.NORMAL_SHORT);
            if (result == FUNC_RESULT.SUCCESS && NetworkCheck)
            {
                result = this.NetworkNameGetExecute((int)TRY_COUNT.NETWORK_DATA, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref networkName1, ref networkName2);
                result = this.XXXCheck(result, result1, result2, networkName1, networkName2);
                this.XXXSetValue(result, networkName1, DeviceDefinitions.STRING_INVALID, ref networkName, ref this._runtimeNetwork.NetworkName, InternalValueSet);
            }
            return result;
        }
        private FUNC_RESULT NetworkNameSetExecute(int Delay)
        {
            Thread.Sleep(Delay);
            return this.NetworkNameSetBase();
        }
        private FUNC_RESULT NetworkNameSetBase()
        {
            return this.NetworkNameSetBaseDll();
        }
        private FUNC_RESULT NetworkNameSetBaseDll()
        {
            this._dllWrapper.SIMNetworkSelect();
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : Network Name Get
        //Network Property

        //Property -> UnKnown level
        //GSMDateTime Get
        private FUNC_RESULT GSMDateTimeGetWorkflow(ref string GSMDateTime, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            string dateTime1 = DeviceDefinitions.STRING_INVALID, dateTime2 = DeviceDefinitions.STRING_INVALID;
            result = this.NetworkNameGetExecute((int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref dateTime1, ref dateTime2);
            //dateTime2 = dateTime1 + x , because we read new data after delay and time increase
            result = this.XXXCheck(result, result1, result2, dateTime1, dateTime1);
            this.XXXSetValue(result, dateTime1, DeviceDefinitions.STRING_INVALID, ref GSMDateTime, ref this._runtimeUngroup.GsmDateTime, InternalValueSet);
            return result;
        }
        private FUNC_RESULT GSMDateTimeGetExecute(int loopCount, int Delay, ref FUNC_RESULT Result1, ref FUNC_RESULT Result2, ref string DateTime1, ref string DateTime2)
        {
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = FUNC_RESULT.SUCCESS;
            }
            while (loopCount > 0 && (Result1 != FUNC_RESULT.SUCCESS || Result2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (Result1 != FUNC_RESULT.SUCCESS)
                {
                    Result1 = this.GSMDateTimeGetBase(ref DateTime1);
                    if (Result1 != FUNC_RESULT.SUCCESS)
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
                    Result2 = this.GSMDateTimeGetBase(ref DateTime2);
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
                DateTime2 = DateTime1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT GSMDateTimeGetBase(ref string GSMDateTime)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            string dateTime = DeviceDefinitions.STRING_INVALID;
            result = this.GSMDateTimeGetBaseDll(ref dateTime);
            this.XXXBaseSetValue(result, dateTime, DeviceDefinitions.STRING_INVALID, ref GSMDateTime);
            return result;
        }
        private FUNC_RESULT GSMDateTimeGetBaseDll(ref string GSMDateTime)
        {
            if (this._dllWrapper.GsmDateTimeGet(out GSMDateTime))
                return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR)
                return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.GSM_DATE_TIME_GET, "GSM Date Time = " + GSMDateTime);
            return FUNC_RESULT.FAIL;
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
        private FUNC_RESULT GSMDateTimeSetWorkflow(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second, bool CheckDateTime,
                                                   bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            string dateTime1 = DeviceDefinitions.STRING_INVALID, dateTime2 = DeviceDefinitions.STRING_INVALID, datetime = DeviceDefinitions.STRING_INVALID;
            result = this.GSMDateTimeSetExecute(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second, (int)OPERATION_DELAY_MS.NORMAL_SHORT);
            if (result == FUNC_RESULT.SUCCESS && CheckDateTime)
            {
                result = this.NetworkNameGetExecute((int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref dateTime1, ref dateTime2);
                //dateTime2 = dateTime1 + x , because we read new data after delay and time increase
                result = this.XXXCheck(result, result1, result2, dateTime1, dateTime1);
                this.XXXSetValue(result, dateTime1, DeviceDefinitions.STRING_INVALID, ref datetime, ref this._runtimeUngroup.GsmDateTime, InternalValueSet);
            }
            return result;
        }
        private FUNC_RESULT GSMDateTimeSetExecute(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second, int Delay)
        {
            Thread.Sleep(Delay);
            return this.GSMDateTimeSetBase(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second);
        }
        private FUNC_RESULT GSMDateTimeSetBase(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second)
        {
            FUNC_RESULT result = this.GSMDateTimeSetBaseInputCheck(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second);
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.GSMDateTimeSetBaseDll(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second);
            }
            return result;
        }
        private FUNC_RESULT GSMDateTimeSetBaseInputCheck(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second)
        {
            if (this.DateTimeCheck(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second))
                return FUNC_RESULT.SUCCESS;
            this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Year = " + GSM_Year + " , Month = " + GSM_Month + " , Day = " + GSM_Day + " , Hour = " + GSM_Hour + ", Min = " +
                          GSM_Minute + " , Sec = " + GSM_Second);
            return FUNC_RESULT.MAJOR_ERROR;
        }
        private FUNC_RESULT GSMDateTimeSetBaseDll(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second)
        {
            this._dllWrapper.GSMDateTimeSet(Convert.ToInt16(GSM_Year), Convert.ToInt16(GSM_Month), Convert.ToInt16(GSM_Day), Convert.ToInt16(GSM_Hour),
                                            Convert.ToInt16(GSM_Minute), Convert.ToInt16(GSM_Second));
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : GSMDateTime Set
        //GSM Status Get
        private FUNC_RESULT GSMStatusGetWorkflow(ref DeviceDefinitions.GSM_STATUS GsmStatus, ref int GsmStatusValue, ref string GsmStatusMessage, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN, result1 = FUNC_RESULT.UNKNOWN, result2 = FUNC_RESULT.UNKNOWN;
            DeviceDefinitions.GSM_STATUS gsmStatus1 = DeviceDefinitions.GSM_STATUS.ERROR, gsmStatus2 = DeviceDefinitions.GSM_STATUS.ERROR;
            int gsmStatusValue1 = DeviceDefinitions.NUMBER_INVALID, gsmStatusValue2 = DeviceDefinitions.NUMBER_INVALID;
            string gsmStatusMessage1 = DeviceDefinitions.STRING_INVALID, gsmStatusMessage2 = DeviceDefinitions.STRING_INVALID;
            result = this.GSMStatusGetExecute((int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref result1, ref result2, ref gsmStatus1, ref gsmStatus2,
                                              ref gsmStatusValue1, ref gsmStatusValue2, ref gsmStatusMessage1, ref gsmStatusMessage2);
            result = this.XXXCheck(result, result1, result2, gsmStatus1, gsmStatus2);
            this.XXXSetValue(result, gsmStatus1, DeviceDefinitions.GSM_STATUS.UNKHOWN, ref GsmStatus, ref this._runtimeHardware.GsmStatus, InternalValueSet);
            this.XXXSetValue(result, gsmStatusValue1, DeviceDefinitions.NUMBER_INVALID, ref GsmStatusValue, ref this._runtimeHardware.GsmStatusValue, InternalValueSet);
            this.XXXSetValue(result, gsmStatusMessage1, DeviceDefinitions.STRING_INVALID, ref GsmStatusMessage, ref this._runtimeHardware.GsmStatusMessage, InternalValueSet);
            return result;
        }
        private FUNC_RESULT GSMStatusGetExecute(int loopCount, int Delay, ref FUNC_RESULT Result1, ref FUNC_RESULT Result2, ref DeviceDefinitions.GSM_STATUS GsmStatus1,
                                                ref DeviceDefinitions.GSM_STATUS GsmStatus2, ref int GsmStatusValue1, ref int GsmStatusValue2,
                                                ref string GsmStatusMessage1, ref string GsmStatusMessage2)
        {
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            if (!this._runtimeConfig.EnableDoubleReadValidation)
            {
                Result2 = FUNC_RESULT.SUCCESS;
            }
            while (loopCount > 0 && (Result1 != FUNC_RESULT.SUCCESS || Result2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (Result1 != FUNC_RESULT.SUCCESS)
                {
                    Result1 = this.GSMStatusGetBase(ref GsmStatus1, ref GsmStatusValue1, ref GsmStatusMessage1);
                    if (Result1 != FUNC_RESULT.SUCCESS)
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
                    Result2 = this.GSMStatusGetBase(ref GsmStatus2, ref GsmStatusValue2, ref GsmStatusMessage2);
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
                GsmStatus2 = GsmStatus1;
                GsmStatusValue2 = GsmStatusValue1;
                GsmStatusMessage2 = GsmStatusMessage1;
            }
            return FUNC_RESULT.SUCCESS;
        }
        private FUNC_RESULT GSMStatusGetBase(ref DeviceDefinitions.GSM_STATUS GsmStatus, ref int GsmStatusValue, ref string GsmStatusMessage)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            int gsmStatusValue = (int)DeviceDefinitions.GSM_STATUS.ERROR;
            string gsmStatusMessage = DeviceDefinitions.STRING_INVALID;
            result = this.GSMStatusGetBaseDll(ref gsmStatusValue, ref gsmStatusMessage);
            if (result == FUNC_RESULT.SUCCESS)
            {
                result = this.GSMStatusGetBaseCheck(gsmStatusValue);
            }
            this.GSMStatusGetBaseSetValue(result, (int)DeviceDefinitions.GSM_STATUS.ERROR, DeviceDefinitions.STRING_INVALID,
                                              ref GsmStatus, ref GsmStatusValue, ref GsmStatusMessage);
            return result;
        }
        private FUNC_RESULT GSMStatusGetBaseDll(ref int GsmStatusValue, ref string GsmStatusMessage)
        {
            if (this._dllWrapper.GsmStatus(ref GsmStatusValue, out GsmStatusMessage))
                return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR)
                return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.GSM_STATUS, "GSM Status = " + GsmStatusValue.ToString() + " - GSM Message = " + GsmStatusMessage);
            return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT GSMStatusGetBaseCheck(int GsmStatusValue)
        {
            try
            {
                if (GsmStatusValue == (int)DeviceDefinitions.GSM_STATUS.READY || GsmStatusValue == (int)DeviceDefinitions.GSM_STATUS.STARTUP)
                {
                    return FUNC_RESULT.SUCCESS;
                }
                else
                {
                    this.ErrorSet(ERROR_CODE.GSM_STATUS, "Gsm Status Value = " + GsmStatusValue);
                    return FUNC_RESULT.FAIL;
                }
            }
            catch (Exception ex)
            {
                this.ErrorSet(ERROR_CODE.GSM_STATUS, "GSMStatusGetBaseCheck Error : " + ex.Message);
                return FUNC_RESULT.FAIL;
            }
        }
        private void GSMStatusGetBaseSetValue(FUNC_RESULT FuncResult, int GsmStatusValueNewValue, string GsmStatusMessageNewValue, ref DeviceDefinitions.GSM_STATUS GsmStatus,
                                              ref int GsmStatusValue, ref string GsmStatusMessage)
        {
            if (FuncResult != FUNC_RESULT.SUCCESS)
            {
                GsmStatus = DeviceDefinitions.GSM_STATUS.UNKHOWN;
                GsmStatusValue = DeviceDefinitions.NUMBER_INVALID;
                GsmStatusMessage = DeviceDefinitions.STRING_INVALID;
                return;
            }
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

        //Refresh
        private FUNC_RESULT DeviceRefreshComplexWorkFlow(bool PowerRestartAllow)
        {
            bool MajorErrorStop = true;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            REFRESH_RESULT refreshResult = REFRESH_RESULT.UNKNOWN_ERROR;
            refreshResult = this.DeviceRefreshComplexWorkFlowHelper(MajorErrorStop);
            switch (refreshResult)
            {
                case REFRESH_RESULT.POWER_OFF:
                    {
                        result = this.GSMPowerSetWorkflow(true, true, true);
                        if (result == FUNC_RESULT.SUCCESS)
                        {
                            refreshResult = this.DeviceRefreshComplexWorkFlowHelper(true);
                        }
                        break;
                    }

                case REFRESH_RESULT.RESTART_NEED:
                    {
                        if (PowerRestartAllow)
                        {
                            result = this.DeviceRestartComplexWorkFlow();
                            if (result == FUNC_RESULT.SUCCESS)
                            {
                                refreshResult = this.DeviceRefreshComplexWorkFlowHelper(true);
                            }
                        }
                        break;
                    }

                default:
                    break;
            }
            return this.DeviceRefreshComplexWorkFlowCheck(refreshResult);
        }
        private FUNC_RESULT DeviceRefreshComplexWorkFlowCheck(REFRESH_RESULT RefreshResult)
        {
            if (RefreshResult == REFRESH_RESULT.SUCCEED)
                return FUNC_RESULT.SUCCESS;
            if (RefreshResult == REFRESH_RESULT.DEVICE_NOT_SELECT || RefreshResult == REFRESH_RESULT.DEVICE_NUMBER_CHANGED)
                return FUNC_RESULT.MAJOR_ERROR;
            return FUNC_RESULT.FAIL;
        }
        private REFRESH_RESULT DeviceRefreshComplexWorkFlowHelper(bool ErrorStop = true , bool InternalValueSet = true)
        {
            bool tempBool = false;
            int tempInt = DeviceDefinitions.NUMBER_INVALID;
            string tempStr = DeviceDefinitions.STRING_INVALID;
            DeviceDefinitions.SIM_STATUS tempSimStatus = DeviceDefinitions.SIM_STATUS.UNKHOWN_ERROR;
            DeviceDefinitions.GSM_STATUS tempGsmStatus = DeviceDefinitions.GSM_STATUS.UNKHOWN;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            REFRESH_RESULT refreshResult = REFRESH_RESULT.SUCCEED;
            //Device Selection check
            result = this.DeviceNumberGetWorkflow(ref tempInt, false);
            if (result == FUNC_RESULT.MAJOR_ERROR)
                return REFRESH_RESULT.DEVICE_NOT_SELECT;
            if (result != FUNC_RESULT.SUCCESS)
            {
                result = this.DeviceSelectByDeviceNumberWorkflow(this._runtimeIdentity.DeviceNumber, true, false);
                if (result != FUNC_RESULT.SUCCESS)
                    return REFRESH_RESULT.DEVICE_NOT_SELECT;
            }
            else
                if (tempInt != this._runtimeIdentity.DeviceNumber)
                    return REFRESH_RESULT.DEVICE_NUMBER_CHANGED;
            //Device Selection
            //Reset internal value
            this.InternalValueReset();
            //End : Reset internal value

            //Hardware Check
            result = this.GSMPowerGetWorkflow(ref tempBool, InternalValueSet);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.POWER_OFF);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR)
                return refreshResult;

            result = this.GSMIsConnectWorkflow(ref tempBool, InternalValueSet);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.RESTART_NEED);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR)
                return refreshResult;

            result = this.GSMIsReadyWorkflow(ref tempBool, InternalValueSet);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.UNKNOWN_ERROR);
            //End : Hardware Check

            //Hardware Data Get
            result = this.IMEIGetWorkflow(ref tempStr, InternalValueSet);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.RESTART_NEED);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR)
                return refreshResult;

            result = this.SoftwareVersionGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.UNKNOWN_ERROR);

            result = this.DeviceInformationGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.UNKNOWN_ERROR);

            result = this.GSMStatusGetWorkflow(ref tempGsmStatus, ref tempInt, ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.UNKNOWN_ERROR);
            //End : Hardware Data Get

            //SIM Data
            result = this.SimIDGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.RESTART_NEED);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR)
                return refreshResult;

            result = this.SimStatusGetWorkflow(ref tempSimStatus, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.SIM_NOT_READY);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR)
                return refreshResult;

            result = this.NationalMobileIDGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.UNKNOWN_ERROR);
            //End : SIM Data

            //Network check
            result = this.SignalGetWorkflow(ref tempInt, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.RESTART_NEED);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR)
                return refreshResult;
            //End : Network check

            //Network Data Get
            result = this.NetworkNameGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.RESTART_NEED);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR)
                return refreshResult;

            result = this.SMSCenterGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.UNKNOWN_ERROR);
            
            result = this.GSMDateTimeGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.UNKNOWN_ERROR);
            //End : Network Data Get

            return refreshResult;
        }
        private REFRESH_RESULT DeviceRefreshComplexWorkFlowHelperCheck(FUNC_RESULT Result, REFRESH_RESULT RefreshResult)
        {
            if (Result != FUNC_RESULT.SUCCESS && RefreshResult == REFRESH_RESULT.SUCCEED)
                return REFRESH_RESULT.UNKNOWN_ERROR;
            return RefreshResult;
        }
        private REFRESH_RESULT DeviceRefreshComplexWorkFlowHelperCheck(FUNC_RESULT Result, REFRESH_RESULT RefreshResult, REFRESH_RESULT RefreshResultNew)
        {
            if (Result != FUNC_RESULT.SUCCESS)
                return RefreshResultNew;
            return RefreshResult;
        }

        //End : Refresh

        //Device Restart
        private FUNC_RESULT DeviceRestartComplexWorkFlow()
        {
            bool power = false, powerCheck = true, internalValueSet = true;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            //Power OFF
            power = false;
            result = this.GSMPowerSetWorkflow(power, powerCheck, internalValueSet);
            //End : Power OFF
            //Power ON
            if (result == FUNC_RESULT.SUCCESS)
            {
                power = true;
                result = this.GSMPowerSetWorkflow(power, powerCheck, internalValueSet);
            }
            //End : Power ON
            return result;
        }
        //Device Restart

        //Helper
        private void InternalValueReset()
        {
            this._runtimeHardware.IMEI = DeviceDefinitions.STRING_INVALID;
            this._runtimeHardware.GsmPower = BOOL_TYPE.UNKNOWN;
            this._runtimeHardware.GsmIsConnect = BOOL_TYPE.UNKNOWN;
            this._runtimeHardware.GsmIsReady = BOOL_TYPE.UNKNOWN;
            this._runtimeSim.SIMStatus = DeviceDefinitions.SIM_STATUS.UNKHOWN_ERROR;
            this._runtimeOperator.MobileNumber = DeviceDefinitions.STRING_INVALID;
            this._runtimeSim.SIMID = DeviceDefinitions.STRING_INVALID;
            this._runtimeNetwork.ModemSignal = DeviceDefinitions.NUMBER_INVALID;
            this._runtimeHardware.SoftwareVersion = DeviceDefinitions.STRING_INVALID;
            this._runtimeHardware.DeviceInformation = DeviceDefinitions.STRING_INVALID;
            this._runtimeUngroup.GsmDateTime = DeviceDefinitions.STRING_INVALID;
            this._runtimeNetwork.SMSCenter = DeviceDefinitions.STRING_INVALID;
            this._runtimeNetwork.NetworkName = DeviceDefinitions.STRING_INVALID;
            this._runtimeSim.NationalMobileID = DeviceDefinitions.STRING_INVALID;
            this._runtimeInboxProperty.TotalSpace = DeviceDefinitions.NUMBER_INVALID;
            this._runtimeInboxProperty.SMSNumber = DeviceDefinitions.NUMBER_INVALID;
        }
        private string BuildGsmDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            if (this.DateTimeCheck(year, month, day, hour, minute, second))
                return $"{year:D2}/{month:D2}/{day:D2} {hour:D2}:{minute:D2}:{second:D2}";
            return string.Empty;
        }
        private bool DateTimeCheck(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second)
        {
            if (GSM_Year >= DeviceDefinitions.DATETIME_YEAR_MIN && GSM_Year <= DeviceDefinitions.DATETIME_YEAR_MAX &&
                GSM_Month >= DeviceDefinitions.DATETIME_MON_MIN && GSM_Month <= DeviceDefinitions.DATETIME_MON_MAX &&
                GSM_Day >= DeviceDefinitions.DATETIME_DAY_MIN && GSM_Day <= DeviceDefinitions.DATETIME_DAY_MAX &&
                GSM_Hour >= DeviceDefinitions.DATETIME_HOUR_MIN && GSM_Hour <= DeviceDefinitions.DATETIME_HOUR_MAX &&
                GSM_Minute >= DeviceDefinitions.DATETIME_MINUTE_MIN && GSM_Minute <= DeviceDefinitions.DATETIME_MINUTE_MAX &&
                GSM_Second >= DeviceDefinitions.DATETIME_SECOND_MIN && GSM_Second <= DeviceDefinitions.DATETIME_SECOND_MAX)
                return true;
            return false;
        }
        //End : Helper

    }
}
