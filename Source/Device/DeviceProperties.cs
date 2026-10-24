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
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            BOOL_TYPE resultBool = BOOL_TYPE.UNKNOWN;
            result = this.GSMPowerGetExecute(result, (int)TRY_COUNT.POWER, (int)OPERATION_DELAY_MS.SHORT, ref resultBool);
            this.XXXSetValue(result, resultBool, BOOL_TYPE.FALSE, ref resultBool, ref this._runtimeHardware.GsmPower, InternalValueSet);
            Result = this.XXXOutput(resultBool);
            return result;
        }
        private FUNC_RESULT GSMPowerGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref BOOL_TYPE ResultBool)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            BOOL_TYPE resultBool = BOOL_TYPE.FALSE;
            ResultBool = BOOL_TYPE.UNKNOWN;
            while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || funcResult2 != FUNC_RESULT.SUCCESS || ResultBool != BOOL_TYPE.TRUE || resultBool != BOOL_TYPE.TRUE))
            {
                Thread.Sleep(Delay);
                if (funcResult1 != FUNC_RESULT.SUCCESS || ResultBool != BOOL_TYPE.TRUE) funcResult1 = this.GSMPowerGetBase(ref ResultBool);
                else if (this._runtimeConfig.PowerONDoubleCheck) funcResult2 = this.GSMPowerGetBase(ref resultBool);
                else break;
                if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult2 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return this.XXXExecuteReturn(this._runtimeConfig.PowerONDoubleCheck, funcResult1, funcResult2, ref ResultBool, resultBool, BOOL_TYPE.FALSE);
        }
        private FUNC_RESULT GSMPowerGetBase(ref BOOL_TYPE Result)
        {
            bool result = false;
            FUNC_RESULT funcResult = FUNC_RESULT.SUCCESS;
            funcResult = this.GSMPowerGetBaseDll(funcResult, ref result);
            this.XXXBaseSetValue(funcResult, result, ref Result);
            return funcResult;
        }
        private FUNC_RESULT GSMPowerGetBaseDll(FUNC_RESULT FuncResult, ref bool Result)
        {
            if (FuncResult != FUNC_RESULT.SUCCESS) return FuncResult;
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            Result = this._dllWrapper.GsmPowerGet();
            funcResult = this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
            return funcResult;
        }
        //End : GSM Power Get
        //GSM Power Set
        private FUNC_RESULT GSMPowerSetWorkflow(bool PowerSet, bool PowerCheck, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            BOOL_TYPE resultBool = BOOL_TYPE.UNKNOWN;
            result = this.GSMPowerSetExecute(result, PowerSet, (int)OPERATION_DELAY_MS.SHORT);
            resultBool = this.BoolType(result, PowerSet);
            if (PowerCheck)
            {
                if (PowerSet) Thread.Sleep((int)OPERATION_DELAY_MS.AFTER_DEVICE_POWER_ON);
                result = this.GSMPowerGetExecute(result, (int)TRY_COUNT.POWER, (int)OPERATION_DELAY_MS.SHORT, ref resultBool);
            }
            this.XXXSetValue(result, resultBool, BOOL_TYPE.FALSE, ref resultBool, ref this._runtimeHardware.GsmPower, InternalValueSet);
            return result;
        }
        private FUNC_RESULT GSMPowerSetExecute(FUNC_RESULT Result, bool Power, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.GSMPowerSetBase(Power);
        }
        private FUNC_RESULT GSMPowerSetBase(bool Power)
        {
            return this.GSMPowerSetBaseDll(FUNC_RESULT.SUCCESS, Power);
        }
        private FUNC_RESULT GSMPowerSetBaseDll(FUNC_RESULT Result, bool Power)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            this._dllWrapper.GsmPowerSet(Power);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : GSM Power Set
        //GSM Is Connect
        private FUNC_RESULT GSMIsConnectWorkflow(ref bool Result, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            BOOL_TYPE resultBool = BOOL_TYPE.UNKNOWN;
            result = this.GSMIsConnectExecute(result, (int)TRY_COUNT.IS_CONNECT, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref resultBool);
            this.XXXSetValue(result, resultBool, BOOL_TYPE.UNKNOWN, ref resultBool, ref this._runtimeHardware.GsmIsConnect, InternalValueSet);
            Result = this.XXXOutput(resultBool);
            return result;
        }
        private FUNC_RESULT GSMIsConnectExecute(FUNC_RESULT Result, int loopCount, int Delay, ref BOOL_TYPE ResultBool)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            ResultBool = BOOL_TYPE.UNKNOWN;
            while (loopCount-- > 0 && (result != FUNC_RESULT.SUCCESS || ResultBool != BOOL_TYPE.TRUE))
            {
                Thread.Sleep(Delay);
                result = this.GSMIsConnectBase(ref ResultBool);
                if (result == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return result;
        }
        private FUNC_RESULT GSMIsConnectBase(ref BOOL_TYPE Result)
        {
            bool result = false;
            FUNC_RESULT funcResult = FUNC_RESULT.SUCCESS;
            funcResult = this.GSMIsConnectBaseDll(funcResult, ref result);
            this.XXXBaseSetValue(funcResult, result, ref Result);
            return funcResult;
        }
        private FUNC_RESULT GSMIsConnectBaseDll(FUNC_RESULT Result, ref bool ResultBool)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            ResultBool = this._dllWrapper.GsmIsConnect();
            funcResult = this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
            return funcResult;
        }
        //End : GSM Is Connect
        //GSM Is Ready
        private FUNC_RESULT GSMIsReadyWorkflow(ref bool Result, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            BOOL_TYPE resultBool = BOOL_TYPE.UNKNOWN;
            result = this.GSMIsReadyExecute(result, (int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.SHORT, ref resultBool);
            this.XXXSetValue(result, resultBool, BOOL_TYPE.UNKNOWN, ref resultBool, ref this._runtimeHardware.GsmIsReady, InternalValueSet);
            Result = this.XXXOutput(resultBool);
            return result;
        }
        private FUNC_RESULT GSMIsReadyExecute(FUNC_RESULT Result, int loopCount, int Delay, ref BOOL_TYPE ResultBool)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            BOOL_TYPE resultBool = BOOL_TYPE.FALSE;
            while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || funcResult2 != FUNC_RESULT.SUCCESS || ResultBool != BOOL_TYPE.TRUE || resultBool != BOOL_TYPE.TRUE))
            {
                Thread.Sleep(Delay);
                if (funcResult1 != FUNC_RESULT.SUCCESS || ResultBool != BOOL_TYPE.TRUE) funcResult1 = this.GSMIsReadyBase(ref ResultBool);
                else if (this._runtimeConfig.PowerONDoubleCheck) funcResult2 = this.GSMIsReadyBase(ref resultBool);
                else break;
                if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult2 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref ResultBool, resultBool, BOOL_TYPE.FALSE);
        }
        private FUNC_RESULT GSMIsReadyBase(ref BOOL_TYPE Result)
        {
            bool result = false;
            FUNC_RESULT funcResult = FUNC_RESULT.SUCCESS;
            funcResult = this.GSMIsReadyBaseDll(funcResult, ref result);
            this.XXXBaseSetValue(funcResult, result, ref Result);
            return funcResult;
        }
        private FUNC_RESULT GSMIsReadyBaseDll(FUNC_RESULT FuncResult, ref bool Result)
        {
            if (FuncResult != FUNC_RESULT.SUCCESS) return FuncResult;
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
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.IMEIGetExecute(result, (int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_LONG, ref IMEI);
            this.XXXSetValue(result, IMEI, DeviceDefinitions.STRING_INVALID, ref IMEI, ref this._runtimeHardware.IMEI, InternalValueSet);
            return result;
        }
        private FUNC_RESULT IMEIGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref string IMEI)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            string imei = DeviceDefinitions.STRING_INVALID;
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || funcResult2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (funcResult1 != FUNC_RESULT.SUCCESS)
                {
                    funcResult1 = this.IMEIGetBase(ref IMEI); if (funcResult1 != FUNC_RESULT.SUCCESS && --resultCount <= 0) break;
                }
                else if (this._runtimeConfig.EnableDoubleReadValidation) funcResult2 = this.IMEIGetBase(ref imei);
                else break;
                if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult1 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref IMEI, imei, DeviceDefinitions.STRING_INVALID);
        }
        private FUNC_RESULT IMEIGetBase(ref string IMEI)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.IMEIGetBaseDll(result, ref IMEI);
            result = this.IMEIGetBaseCheck(result, IMEI);
            this.XXXBaseSetValue(result, IMEI, DeviceDefinitions.STRING_INVALID, ref IMEI);
            return result;
        }
        private FUNC_RESULT IMEIGetBaseDll(FUNC_RESULT Result, ref string IMEI)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this._dllWrapper.IMEI(out IMEI)) return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.IMEI, "IMEI = " + IMEI); return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT IMEIGetBaseCheck(FUNC_RESULT Result, string IMEI)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (IMEI.Length < DeviceDefinitions.IMEI_LEN_MIN || IMEI.Length > DeviceDefinitions.IMEI_LEN_MAX)
            {
                this.ErrorSet(ERROR_CODE.IMEI, "IMEI = " + IMEI + " -> Invalid"); return FUNC_RESULT.FAIL;
            }
            return FUNC_RESULT.SUCCESS;
        }
        //End : IMEI Get
        //Software Version Get
        private FUNC_RESULT SoftwareVersionGetWorkflow(ref string SoftwareVersion, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SoftwareVersionGetExecute(result, (int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.SHORT, ref SoftwareVersion);
            this.XXXSetValue(result, SoftwareVersion, DeviceDefinitions.STRING_INVALID, ref SoftwareVersion, ref this._runtimeHardware.SoftwareVersion, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SoftwareVersionGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref string SoftwareVersion)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            string softwareVersion = DeviceDefinitions.STRING_INVALID;
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || funcResult2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (funcResult1 != FUNC_RESULT.SUCCESS)
                {
                    funcResult1 = this.SoftwareVersionGetBase(ref SoftwareVersion); if (funcResult1 != FUNC_RESULT.SUCCESS && --resultCount <= 0) break;
                }
                else if (this._runtimeConfig.EnableDoubleReadValidation) funcResult2 = this.SoftwareVersionGetBase(ref softwareVersion);
                else break;
                if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult1 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref SoftwareVersion, softwareVersion, DeviceDefinitions.STRING_INVALID);
        }
        private FUNC_RESULT SoftwareVersionGetBase(ref string SoftwareVersion)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SoftwareVersionGetBaseDll(result, ref SoftwareVersion);
            this.XXXBaseSetValue(result, SoftwareVersion, DeviceDefinitions.STRING_INVALID, ref SoftwareVersion);
            return result;
        }
        private FUNC_RESULT SoftwareVersionGetBaseDll(FUNC_RESULT Result, ref string SoftwareVersion)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this._dllWrapper.SoftwareVersion(out SoftwareVersion)) return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.SOFTWARE_VERSION, "Software Version = " + SoftwareVersion); return FUNC_RESULT.FAIL;
        }
        //End : Software Version Get
        //Device Information Get
        private FUNC_RESULT DeviceInformationGetWorkflow(ref string DeviceInformation, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.DeviceInformationGetExecute(result, (int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.SHORT, ref DeviceInformation);
            this.XXXSetValue(result, DeviceInformation, DeviceDefinitions.STRING_INVALID, ref DeviceInformation, ref this._runtimeHardware.DeviceInformation, InternalValueSet);
            return result;
        }
        private FUNC_RESULT DeviceInformationGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref string DeviceInformation)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            string deviceInformation = DeviceDefinitions.STRING_INVALID;
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || funcResult2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (funcResult1 != FUNC_RESULT.SUCCESS)
                {
                    funcResult1 = this.DeviceInformationGetBase(ref DeviceInformation); if (funcResult1 != FUNC_RESULT.SUCCESS && --resultCount <= 0) break;
                }
                else if (this._runtimeConfig.EnableDoubleReadValidation) funcResult2 = this.DeviceInformationGetBase(ref deviceInformation);
                else break;
                if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult1 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref DeviceInformation, deviceInformation, DeviceDefinitions.STRING_INVALID);
        }
        private FUNC_RESULT DeviceInformationGetBase(ref string DeviceInformation)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            result = this.DeviceInformationGetBaseDll(result, ref DeviceInformation);
            this.XXXBaseSetValue(result, DeviceInformation, DeviceDefinitions.STRING_INVALID, ref DeviceInformation);
            return result;
        }
        private FUNC_RESULT DeviceInformationGetBaseDll(FUNC_RESULT Result, ref string DeviceInformation)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this._dllWrapper.DeviceInfo(out DeviceInformation)) return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.DEVICE_INFORMATION, "Device Information = " + DeviceInformation); return FUNC_RESULT.FAIL;
        }
        //End : Device Information Get
        //End : Hardware Property

        //SIM Property
        //SimID Get
        private FUNC_RESULT SimIDGetWorkflow(ref string SimID, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SimIDGetExecute(result, (int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref SimID);
            this.XXXSetValue(result, SimID, DeviceDefinitions.STRING_INVALID, ref SimID, ref this._runtimeSim.SIMID, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SimIDGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref string SimID)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            string simID = DeviceDefinitions.STRING_INVALID;
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || funcResult2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (funcResult1 != FUNC_RESULT.SUCCESS)
                {
                    funcResult1 = this.SimIDGetBase(ref SimID); if (funcResult1 != FUNC_RESULT.SUCCESS && --resultCount <= 0) break;
                }
                else if (this._runtimeConfig.EnableDoubleReadValidation) funcResult2 = this.SimIDGetBase(ref simID);
                else break;
                if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult1 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref SimID, simID, DeviceDefinitions.STRING_INVALID);
        }
        private FUNC_RESULT SimIDGetBase(ref string SimID)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SimIDGetBaseDll(result, ref SimID);
            result = this.SimIDGetBaseCheck(result, SimID);
            this.XXXBaseSetValue(result, SimID, DeviceDefinitions.STRING_INVALID, ref SimID);
            return result;
        }
        private FUNC_RESULT SimIDGetBaseDll(FUNC_RESULT Result, ref string SimID)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this._dllWrapper.SimID(out SimID)) return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.SIM_ID, "SIM ID = " + SimID); return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT SimIDGetBaseCheck(FUNC_RESULT Result, string SimID)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (SimID.Length < DeviceDefinitions.SIM_ID_LEN_MIN || SimID.Length > DeviceDefinitions.SIM_ID_LEN_MAX)
            {
                this.ErrorSet(ERROR_CODE.SIM_ID, "SIM ID = " + SimID + " , SIM ID Len = " + SimID.Length); return FUNC_RESULT.FAIL;
            }
            return FUNC_RESULT.SUCCESS;
        }
        //End : SimID Get
        //National Mobile ID Get
        private FUNC_RESULT NationalMobileIDGetWorkflow(ref string NationalMobileID, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.NationalMobileIDGetExecute(result, (int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.SHORT, ref NationalMobileID);
            this.XXXSetValue(result, NationalMobileID, DeviceDefinitions.STRING_INVALID, ref NationalMobileID, ref this._runtimeSim.NationalMobileID, InternalValueSet);
            return result;
        }
        private FUNC_RESULT NationalMobileIDGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref string NationalMobileID)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            string nationalMobileID = DeviceDefinitions.STRING_INVALID;
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || funcResult2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (funcResult1 != FUNC_RESULT.SUCCESS)
                {
                    funcResult1 = this.NationalMobileIDGetBase(ref NationalMobileID); if (funcResult1 != FUNC_RESULT.SUCCESS && --resultCount <= 0) break;
                }
                else if (this._runtimeConfig.EnableDoubleReadValidation) funcResult2 = this.NationalMobileIDGetBase(ref nationalMobileID);
                else break;
                if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult1 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref NationalMobileID, nationalMobileID, DeviceDefinitions.STRING_INVALID);
        }
        private FUNC_RESULT NationalMobileIDGetBase(ref string NationalMobileID)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.NationalMobileIDGetBaseDll(result, ref NationalMobileID);
            result = this.NationalMobileIDGetBaseCheck(result, NationalMobileID);
            this.XXXBaseSetValue(result, NationalMobileID, DeviceDefinitions.STRING_INVALID, ref NationalMobileID);
            return result;
        }
        private FUNC_RESULT NationalMobileIDGetBaseDll(FUNC_RESULT Result, ref string NationalMobileID)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this._dllWrapper.NationalMobileID(out NationalMobileID)) return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.NATIONAL_MOBILE_ID, "National Mobile ID = " + NationalMobileID); return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT NationalMobileIDGetBaseCheck(FUNC_RESULT Result, string NationalMobileID)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (NationalMobileID.Length < DeviceDefinitions.NATIONAL_MOBILE_ID_LEN_MIN || NationalMobileID.Length > DeviceDefinitions.NATIONAL_MOBILE_ID_LEN_MAX)
            {
                this.ErrorSet(ERROR_CODE.NATIONAL_MOBILE_ID, "National Mobile ID = " + NationalMobileID + " , National Mobile ID Len = " + NationalMobileID.Length); return FUNC_RESULT.FAIL;
            }
            return FUNC_RESULT.SUCCESS;
        }
        //End : National Mobile ID Get
        //Sim Status Get
        private FUNC_RESULT SimStatusGetWorkflow(ref DeviceDefinitions.SIM_STATUS SimStatus, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SimStatusGetExecute(result, (int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref SimStatus);
            this.XXXSetValue(result, SimStatus, DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR, ref SimStatus, ref this._runtimeSim.SIMStatus, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SimStatusGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref DeviceDefinitions.SIM_STATUS SimStatus)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            DeviceDefinitions.SIM_STATUS simStatus = DeviceDefinitions.SIM_STATUS.UNKHOWN;
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || funcResult2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (funcResult1 != FUNC_RESULT.SUCCESS)
                {
                    funcResult1 = this.SimStatusGetBase(ref SimStatus); if (funcResult1 != FUNC_RESULT.SUCCESS && --resultCount <= 0) break;
                }
                else if (this._runtimeConfig.EnableDoubleReadValidation) funcResult2 = this.SimStatusGetBase(ref simStatus);
                else break;
                if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult1 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref SimStatus, simStatus, DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR);
        }
        private FUNC_RESULT SimStatusGetBase(ref DeviceDefinitions.SIM_STATUS SimStatus)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SimStatusGetBaseDll(result, ref SimStatus);
            result = this.SimStatusGetBaseCheck(result, SimStatus);
            this.XXXBaseSetValue(result, SimStatus, DeviceDefinitions.SIM_STATUS.IDENTITY_ERROR, ref SimStatus);
            return result;
        }
        private FUNC_RESULT SimStatusGetBaseDll(FUNC_RESULT Result, ref DeviceDefinitions.SIM_STATUS SimStatus)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this._dllWrapper.SimStatus(ref SimStatus)) return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.SIM_STATUS, "SIM Status = " + SimStatus.ToString()); return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT SimStatusGetBaseCheck(FUNC_RESULT Result, DeviceDefinitions.SIM_STATUS SimStatus)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (SimStatus != DeviceDefinitions.SIM_STATUS.READY)
            {
                this.ErrorSet(ERROR_CODE.SIM_STATUS, "SIM Status = " + SimStatus.ToString()); return FUNC_RESULT.FAIL;
            }
            return FUNC_RESULT.SUCCESS;
        }
        //End : Sim Status Get
        //SIM Property

        //Network Property
        //Signal Get
        private FUNC_RESULT SignalGetWorkflow(ref int Signal, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SignalGetExecute(result, (int)TRY_COUNT.NETWORK_DATA, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref Signal);
            this.XXXSetValue(result, Signal, DeviceDefinitions.NUMBER_INVALID, ref Signal, ref this._runtimeNetwork.ModemSignal, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SignalGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref int Signal)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult = FUNC_RESULT.UNKNOWN;
            while (loopCount-- > 0 && funcResult != FUNC_RESULT.SUCCESS)
            {
                Thread.Sleep(Delay);
                funcResult = this.SignalGetBase(ref Signal);
                if (funcResult == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return funcResult;
        }
        private FUNC_RESULT SignalGetBase(ref int Signal)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SignalGetBaseDll(result, ref Signal);
            result = SignalGetBaseCheck(result, Signal);
            this.XXXBaseSetValue(result, Signal, DeviceDefinitions.NUMBER_INVALID, ref Signal);
            return result;
        }
        private FUNC_RESULT SignalGetBaseDll(FUNC_RESULT Result, ref int Signal)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this._dllWrapper.ModemSignal(ref Signal)) return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.GSM_SIGNAL, "Signal = " + Signal.ToString()); return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT SignalGetBaseCheck(FUNC_RESULT Result, int Signal)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (Signal < DeviceDefinitions.SIGNAL_MIN || Signal > DeviceDefinitions.SIGNAL_MAX || Signal < this._runtimeConfig.SignalMinAcceptableValue)
            {
                this.ErrorSet(ERROR_CODE.GSM_SIGNAL, "Signal = " + Signal); return FUNC_RESULT.FAIL;
            }
            return FUNC_RESULT.SUCCESS;
        }
        //End : Siganl Get
        //SMS Center Get
        private FUNC_RESULT SMSCenterGetWorkflow(ref string SMSCenter, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SMSCenterGetExecute(result, (int)TRY_COUNT.NETWORK_DATA, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref SMSCenter);
            this.XXXSetValue(result, SMSCenter, DeviceDefinitions.STRING_INVALID, ref SMSCenter, ref this._runtimeNetwork.SMSCenter, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SMSCenterGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref string SmsCenter)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            string smsCenter = DeviceDefinitions.STRING_INVALID;
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || funcResult2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (funcResult1 != FUNC_RESULT.SUCCESS)
                {
                    funcResult1 = this.SMSCenterGetBase(ref SmsCenter); if (funcResult1 != FUNC_RESULT.SUCCESS && --resultCount <= 0) break;
                }
                else if (this._runtimeConfig.EnableDoubleReadValidation) funcResult2 = this.SMSCenterGetBase(ref smsCenter);
                else break;
                if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult1 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref SmsCenter, smsCenter, DeviceDefinitions.STRING_INVALID);
        }
        private FUNC_RESULT SMSCenterGetBase(ref string SMSCenter)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SMSCenterGetBaseDll(result, ref SMSCenter);
            this.XXXBaseSetValue(result, SMSCenter, DeviceDefinitions.STRING_INVALID, ref SMSCenter);
            return result;
        }
        private FUNC_RESULT SMSCenterGetBaseDll(FUNC_RESULT Result, ref string SMSCenter)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this._dllWrapper.SmsCenterGet(out SMSCenter)) return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.SMS_CENTER_GET, "SMS Center = " + SMSCenter); return FUNC_RESULT.FAIL;
        }
        //End : SMS Center Get
        //SMS Center Get
        private FUNC_RESULT SMSCenterSetWorkflow(string SMSCenter, bool SMSCenterCheck, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.SMSCenterSetExecute(result, SMSCenter, (int)OPERATION_DELAY_MS.NORMAL_SHORT);
            if (SMSCenterCheck)
            {
                string smsCenter = DeviceDefinitions.STRING_INVALID;
                result = this.SMSCenterGetExecute(result, (int)TRY_COUNT.NETWORK_DATA, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref smsCenter);
                if (SMSCenter != smsCenter)  result = FUNC_RESULT.MAJOR_ERROR;
            }
            this.XXXSetValue(result, SMSCenter, DeviceDefinitions.STRING_INVALID, ref SMSCenter, ref this._runtimeNetwork.SMSCenter, InternalValueSet);
            return result;
        }
        private FUNC_RESULT SMSCenterSetExecute(FUNC_RESULT Result, string SMSCenter, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.SMSCenterSetBase(SMSCenter);
        }
        private FUNC_RESULT SMSCenterSetBase(string SMSCenter)
        {
            return this.SMSCenterSetBaseDll(FUNC_RESULT.SUCCESS, SMSCenter);
        }
        private FUNC_RESULT SMSCenterSetBaseDll(FUNC_RESULT Result, string SMSCenter)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            this._dllWrapper.SmsCenterSet(SMSCenter);
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : SMS Center Set
        //Network Name Get
        private FUNC_RESULT NetworkNameGetWorkflow(ref string NetworkName, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.NetworkNameGetExecute(result, (int)TRY_COUNT.NETWORK_DATA, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref NetworkName);
            this.XXXSetValue(result, NetworkName, DeviceDefinitions.STRING_INVALID, ref NetworkName, ref this._runtimeNetwork.NetworkName, InternalValueSet);
            return result;
        }
        private FUNC_RESULT NetworkNameGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref string NetworkName)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            string networkName = DeviceDefinitions.STRING_INVALID;
            while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || funcResult2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (funcResult1 != FUNC_RESULT.SUCCESS) funcResult1 = this.NetworkNameGetBase(ref NetworkName);
                else if (this._runtimeConfig.EnableDoubleReadValidation) funcResult2 = this.NetworkNameGetBase(ref networkName);
                else break;
                if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult1 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref NetworkName, networkName, DeviceDefinitions.STRING_INVALID);
        }
        private FUNC_RESULT NetworkNameGetBase(ref string NetworkName)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.NetworkNameGetBaseDll(result, ref NetworkName);
            this.XXXBaseSetValue(result, NetworkName, DeviceDefinitions.STRING_INVALID, ref NetworkName);
            return result;
        }
        private FUNC_RESULT NetworkNameGetBaseDll(FUNC_RESULT Result, ref string NetworkName)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this._dllWrapper.SimNetworkGet(out NetworkName)) return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.NETWORK_NAME_GET, "Network Name = " + NetworkName); return FUNC_RESULT.FAIL;
        }
        //End : Network Name Get
        //Network Name Set
        private FUNC_RESULT NetworkNameSetWorkflow(bool NetworkCheck, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            string networkName = DeviceDefinitions.STRING_INVALID;
            result = this.NetworkNameSetExecute(result, (int)OPERATION_DELAY_MS.NORMAL_SHORT);
            if (NetworkCheck)
            {
                result = this.NetworkNameGetExecute(result, (int)TRY_COUNT.NETWORK_DATA, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref networkName);
                this.XXXSetValue(result, networkName, DeviceDefinitions.STRING_INVALID, ref networkName, ref this._runtimeNetwork.NetworkName, InternalValueSet);
            }
            return result;
        }
        private FUNC_RESULT NetworkNameSetExecute(FUNC_RESULT Result, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.NetworkNameSetBase();
        }
        private FUNC_RESULT NetworkNameSetBase()
        {
            return this.NetworkNameSetBaseDll(FUNC_RESULT.SUCCESS);
        }
        private FUNC_RESULT NetworkNameSetBaseDll(FUNC_RESULT Result)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            this._dllWrapper.SIMNetworkSelect();
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : Network Name Get
        //Network Property

        //Property -> UnKnown level
        //GSMDateTime Get
        private FUNC_RESULT GSMDateTimeGetWorkflow(ref string GSMDateTime, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.GSMDateTimeGetExecute(result, (int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref GSMDateTime);
            this.XXXSetValue(result, GSMDateTime, DeviceDefinitions.STRING_INVALID, ref GSMDateTime, ref this._runtimeUngroup.GsmDateTime, InternalValueSet);
            return result;
        }
        private FUNC_RESULT GSMDateTimeGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref string DateTime)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            string dateTime = DeviceDefinitions.STRING_INVALID;
            int resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || funcResult2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (funcResult1 != FUNC_RESULT.SUCCESS)
                {
                    funcResult1 = this.GSMDateTimeGetBase(ref DateTime); if (funcResult1 != FUNC_RESULT.SUCCESS && --resultCount <= 0) break;
                }
                else if (this._runtimeConfig.EnableDoubleReadValidation) funcResult2 = this.GSMDateTimeGetBase(ref dateTime);
                else break;
                if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult1 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            //dateTime2 = dateTime1 + x , because we read new data after delay and time increase
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref DateTime, DateTime, DeviceDefinitions.STRING_INVALID);
        }
        private FUNC_RESULT GSMDateTimeGetBase(ref string GSMDateTime)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.GSMDateTimeGetBaseDll(result, ref GSMDateTime);
            this.XXXBaseSetValue(result, GSMDateTime, DeviceDefinitions.STRING_INVALID, ref GSMDateTime);
            return result;
        }
        private FUNC_RESULT GSMDateTimeGetBaseDll(FUNC_RESULT Result, ref string GSMDateTime)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this._dllWrapper.GsmDateTimeGet(out GSMDateTime)) return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.GSM_DATE_TIME_GET, "GSM Date Time = " + GSMDateTime); return FUNC_RESULT.FAIL;
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
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.GSMDateTimeSetExecute(result, GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second, (int)OPERATION_DELAY_MS.NORMAL_SHORT);
            if (CheckDateTime)
            {
                string dateTime = DeviceDefinitions.STRING_INVALID;
                result = this.GSMDateTimeGetExecute(result, (int)TRY_COUNT.FAST, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref dateTime);
                this.XXXSetValue(result, dateTime, DeviceDefinitions.STRING_INVALID, ref dateTime, ref this._runtimeUngroup.GsmDateTime, InternalValueSet);
            }
            return result;
        }
        private FUNC_RESULT GSMDateTimeSetExecute(FUNC_RESULT Result, int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second, int Delay)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            Thread.Sleep(Delay);
            return this.GSMDateTimeSetBase(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second);
        }
        private FUNC_RESULT GSMDateTimeSetBase(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.GSMDateTimeSetBaseInputCheck(result, GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second);
            result = this.GSMDateTimeSetBaseDll(result, GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second);
            return result;
        }
        private FUNC_RESULT GSMDateTimeSetBaseInputCheck(FUNC_RESULT Result, int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this.DateTimeCheck(GSM_Year, GSM_Month, GSM_Day, GSM_Hour, GSM_Minute, GSM_Second)) return FUNC_RESULT.SUCCESS;
            this.ErrorSet(ERROR_CODE.INVALID_INPUT, "Year = " + GSM_Year + " , Month = " + GSM_Month + " , Day = " + GSM_Day + " , Hour = " + GSM_Hour + ", Min = " +
                          GSM_Minute + " , Sec = " + GSM_Second); return FUNC_RESULT.MAJOR_ERROR;
        }
        private FUNC_RESULT GSMDateTimeSetBaseDll(FUNC_RESULT Result, int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            this._dllWrapper.GSMDateTimeSet(Convert.ToInt16(GSM_Year), Convert.ToInt16(GSM_Month), Convert.ToInt16(GSM_Day), Convert.ToInt16(GSM_Hour),
                                            Convert.ToInt16(GSM_Minute), Convert.ToInt16(GSM_Second));
            return this.DllWrapperHasError(FUNC_RESULT.SUCCESS);
        }
        //End : GSMDateTime Set
        //GSM Status Get
        private FUNC_RESULT GSMStatusGetWorkflow(ref DeviceDefinitions.GSM_STATUS GsmStatus, ref int GsmStatusValue, ref string GsmStatusMessage, bool InternalValueSet)
        {
            FUNC_RESULT result = FUNC_RESULT.SUCCESS;
            result = this.GSMStatusGetExecute(result, (int)TRY_COUNT.NORMAL, (int)OPERATION_DELAY_MS.NORMAL_SHORT, ref GsmStatus, ref GsmStatusValue, ref GsmStatusMessage);
            this.XXXSetValue(result, GsmStatus, DeviceDefinitions.GSM_STATUS.UNKHOWN, ref GsmStatus, ref this._runtimeHardware.GsmStatus, InternalValueSet);
            this.XXXSetValue(result, GsmStatusValue, DeviceDefinitions.NUMBER_INVALID, ref GsmStatusValue, ref this._runtimeHardware.GsmStatusValue, InternalValueSet);
            this.XXXSetValue(result, GsmStatusMessage, DeviceDefinitions.STRING_INVALID, ref GsmStatusMessage, ref this._runtimeHardware.GsmStatusMessage, InternalValueSet);
            return result;
        }
        private FUNC_RESULT GSMStatusGetExecute(FUNC_RESULT Result, int loopCount, int Delay, ref DeviceDefinitions.GSM_STATUS GsmStatus, ref int GsmStatusValue, ref string GsmStatusMessage)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            FUNC_RESULT funcResult1 = FUNC_RESULT.UNKNOWN, funcResult2 = FUNC_RESULT.UNKNOWN;
            DeviceDefinitions.GSM_STATUS gsmStatus = DeviceDefinitions.GSM_STATUS.UNKHOWN;
            int gsmStatusValue = DeviceDefinitions.NUMBER_INVALID, resultCount = (int)FAIL_LIMIT.NUM_ERROR;
            ;
            string gsmStatusMessage = DeviceDefinitions.STRING_INVALID;
            while (loopCount-- > 0 && (funcResult1 != FUNC_RESULT.SUCCESS || funcResult2 != FUNC_RESULT.SUCCESS))
            {
                Thread.Sleep(Delay);
                if (funcResult1 != FUNC_RESULT.SUCCESS) funcResult1 = this.GSMStatusGetBase(ref GsmStatus, ref GsmStatusValue, ref GsmStatusMessage);
                    if (funcResult1 != FUNC_RESULT.SUCCESS && --resultCount <= 0) break;
                else if (this._runtimeConfig.EnableDoubleReadValidation) funcResult2 = this.GSMStatusGetBase(ref gsmStatus, ref gsmStatusValue, ref gsmStatusMessage);
                else break;
                if (funcResult1 == FUNC_RESULT.MAJOR_ERROR || funcResult1 == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            }
            this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref GsmStatusValue, gsmStatusValue, DeviceDefinitions.NUMBER_INVALID);
            this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref GsmStatusMessage, gsmStatusMessage, DeviceDefinitions.STRING_INVALID);
            return this.XXXExecuteReturn(this._runtimeConfig.EnableDoubleReadValidation, funcResult1, funcResult2, ref GsmStatus, gsmStatus, DeviceDefinitions.GSM_STATUS.ERROR);
        }
        private FUNC_RESULT GSMStatusGetBase(ref DeviceDefinitions.GSM_STATUS GsmStatus, ref int GsmStatusValue, ref string GsmStatusMessage)
        {
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            int gsmStatusValue = (int)DeviceDefinitions.GSM_STATUS.ERROR;
            string gsmStatusMessage = DeviceDefinitions.STRING_INVALID;
            result = this.GSMStatusGetBaseDll(result, ref gsmStatusValue, ref gsmStatusMessage);
            result = this.GSMStatusGetBaseCheck(result, gsmStatusValue);
            this.GSMStatusGetBaseSetValue(result, gsmStatusValue, gsmStatusMessage,
                                              ref GsmStatus, ref GsmStatusValue, ref GsmStatusMessage);
            return result;
        }
        private FUNC_RESULT GSMStatusGetBaseDll(FUNC_RESULT Result, ref int GsmStatusValue, ref string GsmStatusMessage)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            if (this._dllWrapper.GsmStatus(ref GsmStatusValue, out GsmStatusMessage)) return FUNC_RESULT.SUCCESS;
            if (this.DllWrapperHasError(FUNC_RESULT.SUCCESS) == FUNC_RESULT.MAJOR_ERROR) return FUNC_RESULT.MAJOR_ERROR;
            this.ErrorSet(ERROR_CODE.GSM_STATUS, "GSM Status = " + GsmStatusValue.ToString() + " - GSM Message = " + GsmStatusMessage); return FUNC_RESULT.FAIL;
        }
        private FUNC_RESULT GSMStatusGetBaseCheck(FUNC_RESULT Result, int GsmStatusValue)
        {
            if (Result != FUNC_RESULT.SUCCESS) return Result;
            try
            {
                if (GsmStatusValue == (int)DeviceDefinitions.GSM_STATUS.READY || GsmStatusValue == (int)DeviceDefinitions.GSM_STATUS.STARTUP) return FUNC_RESULT.SUCCESS;
                else { this.ErrorSet(ERROR_CODE.GSM_STATUS, "Gsm Status Value = " + GsmStatusValue); return FUNC_RESULT.FAIL; }
            }
            catch (Exception ex)
            {
                this.ErrorSet(ERROR_CODE.GSM_STATUS, "GSMStatusGetBaseCheck Error : " + ex.Message); return FUNC_RESULT.FAIL;
            }
        }
        private void GSMStatusGetBaseSetValue(FUNC_RESULT FuncResult, int GsmStatusValueNewValue, string GsmStatusMessageNewValue, ref DeviceDefinitions.GSM_STATUS GsmStatus,
                                              ref int GsmStatusValue, ref string GsmStatusMessage)
        {
            GsmStatusValue = GsmStatusValueNewValue;
            GsmStatusMessage = GsmStatusMessageNewValue;
            GsmStatus = (FuncResult != FUNC_RESULT.SUCCESS) ? (DeviceDefinitions.GSM_STATUS.ERROR) : ((Enum.IsDefined(typeof(DeviceDefinitions.GSM_STATUS), GsmStatusValueNewValue)) ?
                ((DeviceDefinitions.GSM_STATUS)GsmStatusValueNewValue) : (DeviceDefinitions.GSM_STATUS.ERROR));
        }
        //End : GSM Status Get
        //End : Property -> UnKnown level

        //Refresh
        private FUNC_RESULT DeviceRefreshComplexWorkFlow(bool PowerOnAllow)
        {
            bool MajorErrorStop = true;
            REFRESH_RESULT refreshResult = REFRESH_RESULT.UNKNOWN_ERROR;
            refreshResult = this.DeviceRefreshComplexWorkFlowHelper(MajorErrorStop);
            if (refreshResult == REFRESH_RESULT.POWER_OFF_AND_RESTART && PowerOnAllow) 
                if (this.DeviceRestartComplexWorkFlow() == FUNC_RESULT.SUCCESS) refreshResult = this.DeviceRefreshComplexWorkFlowHelper(true);
            return this.DeviceRefreshComplexWorkFlowCheck(refreshResult);
        }
        private FUNC_RESULT DeviceRefreshComplexWorkFlowCheck(REFRESH_RESULT RefreshResult)
        {
            if (RefreshResult == REFRESH_RESULT.SUCCEED) return FUNC_RESULT.SUCCESS;
            else if (RefreshResult == REFRESH_RESULT.POWER_OFF_AND_RESTART || RefreshResult == REFRESH_RESULT.SIM_NOT_READY) return FUNC_RESULT.FAIL;
            else return FUNC_RESULT.MAJOR_ERROR;
        }
        private REFRESH_RESULT DeviceRefreshComplexWorkFlowHelper(bool ErrorStop = true , bool InternalValueSet = true)
        {
            bool tempBool = false;
            int tempInt = DeviceDefinitions.NUMBER_INVALID;
            string tempStr = DeviceDefinitions.STRING_INVALID;
            DeviceDefinitions.SIM_STATUS tempSimStatus = DeviceDefinitions.SIM_STATUS.UNKHOWN;
            DeviceDefinitions.GSM_STATUS tempGsmStatus = DeviceDefinitions.GSM_STATUS.UNKHOWN;
            FUNC_RESULT result = FUNC_RESULT.UNKNOWN;
            REFRESH_RESULT refreshResult = REFRESH_RESULT.SUCCEED;
            //Device Selection check
            result = this.DeviceNumberGetWorkflow(ref tempInt, false);
            if (result == FUNC_RESULT.MAJOR_ERROR) return REFRESH_RESULT.DEVICE_NOT_SELECT;
            if (result != FUNC_RESULT.SUCCESS)
            {
                result = this.DeviceSelectByDeviceNumberWorkflow(this._runtimeIdentity.DeviceNumber, true, false);
                if (result != FUNC_RESULT.SUCCESS) return REFRESH_RESULT.DEVICE_NOT_SELECT;
            }
            else if (tempInt != this._runtimeIdentity.DeviceNumber) return REFRESH_RESULT.DEVICE_NUMBER_CHANGED;
            //End : Device Selection check
            
            //Reset internal value
            this.InternalValueReset();
            //End : Reset internal value
            
            //Hardware Check
            //Power ON Check
            result = this.GSMPowerGetWorkflow(ref tempBool, InternalValueSet);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.POWER_OFF_AND_RESTART);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR) return refreshResult;
            //End : Power ON Check
            //GSM Is Connect
            result = this.GSMIsConnectWorkflow(ref tempBool, InternalValueSet);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.POWER_OFF_AND_RESTART);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR) return refreshResult;
            //End : GSM Is Connect
            //GSM Is Ready
            result = this.GSMIsReadyWorkflow(ref tempBool, InternalValueSet);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult);
            //End : GSM Is Ready
            //End : Hardware Check

            //Hardware Data Get
            //IMEI Get
            result = this.IMEIGetWorkflow(ref tempStr, InternalValueSet);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.POWER_OFF_AND_RESTART);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR) return refreshResult;
            //End : IMEI Get
            //Software Version Get
            result = this.SoftwareVersionGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult);
            //End : Software Version Get
            //Device Information Get
            result = this.DeviceInformationGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult);
            //End : Device Information Get
            //GSM Status Get -> GSMStatus result NOT Trustable, only update
            result = this.GSMStatusGetWorkflow(ref tempGsmStatus, ref tempInt, ref tempStr, true);
            //End : GSM Status Get
            //End : Hardware Data Get

            //SIM Data
            //SIM ID Get
            result = this.SimIDGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.POWER_OFF_AND_RESTART);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR) return refreshResult;
            //End : SIM ID Get
            //SIM Status Get
            result = this.SimStatusGetWorkflow(ref tempSimStatus, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.SIM_NOT_READY);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR) return refreshResult;
            //End : SIM Status Get
            //National Mobile ID Get
            result = this.NationalMobileIDGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult);
            //End : National Mobile ID Get
            //End : SIM Data

            //Network Data
            //Signal Get
            result = this.SignalGetWorkflow(ref tempInt, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.POWER_OFF_AND_RESTART);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR) return refreshResult;
            //End : Signal Get
            //Network Name Get
            result = this.NetworkNameGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult, REFRESH_RESULT.POWER_OFF_AND_RESTART);
            if ((result == FUNC_RESULT.FAIL && ErrorStop) || result == FUNC_RESULT.MAJOR_ERROR) return refreshResult;
            //End : Network Name Get
            //SMS Center Get
            result = this.SMSCenterGetWorkflow(ref tempStr, true);
            refreshResult = this.DeviceRefreshComplexWorkFlowHelperCheck(result, refreshResult);
            //GSM DateTime Get -> GSMDateTime NOT important so NOT allow change Refresh Result, only update
            result = this.GSMDateTimeGetWorkflow(ref tempStr, true);
            //End : GSM DateTime Get
            //End : Network Data

            return refreshResult;
        }
        private REFRESH_RESULT DeviceRefreshComplexWorkFlowHelperCheck(FUNC_RESULT Result, REFRESH_RESULT RefreshResult)
        {
            //Note : bigger RefreshResult number say Problem is Major from less number, So NEVER Bigger number replace with Smaller number
            if (Result == FUNC_RESULT.SUCCESS || (int)RefreshResult > (int)REFRESH_RESULT.MINOR_ERROR) return RefreshResult;
            else return REFRESH_RESULT.MINOR_ERROR;
        }
        private REFRESH_RESULT DeviceRefreshComplexWorkFlowHelperCheck(FUNC_RESULT Result, REFRESH_RESULT RefreshResult, REFRESH_RESULT RefreshResultNew)
        {
            //Note : bigger RefreshResult number say Problem is Major from less number, So NEVER Bigger number replace with Smaller number
            if (Result == FUNC_RESULT.SUCCESS || (int)RefreshResult > (int)RefreshResultNew) return RefreshResult;
            else return RefreshResultNew;
        }

        //End : Refresh

        //Device Restart
        private FUNC_RESULT DeviceRestartComplexWorkFlow()
        {
            bool power = true, powerCheck = true, internalValueSet = true;
            return this.GSMPowerSetWorkflow(power, powerCheck, internalValueSet);
        }
        //Device Restart

        //Helper
        private void InternalValueReset()
        {
            this._runtimeHardware.IMEI = DeviceDefinitions.STRING_INVALID;
            this._runtimeHardware.GsmPower = BOOL_TYPE.UNKNOWN;
            this._runtimeHardware.GsmIsConnect = BOOL_TYPE.UNKNOWN;
            this._runtimeHardware.GsmIsReady = BOOL_TYPE.UNKNOWN;
            this._runtimeSim.SIMStatus = DeviceDefinitions.SIM_STATUS.UNKHOWN;
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
            if (this.DateTimeCheck(year, month, day, hour, minute, second)) return $"{year:D2}/{month:D2}/{day:D2} {hour:D2}:{minute:D2}:{second:D2}";
            else return string.Empty;
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
            else return false;
        }
        //End : Helper

    }
}
