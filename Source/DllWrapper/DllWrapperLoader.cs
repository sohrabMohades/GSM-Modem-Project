using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GSM.Core.Source.DllWrapper
{
    public partial class ManshoorGsmDllWrapper : IDisposable
    {
        //Windows API function definition
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string DllFileName);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr Handle);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr Handle, string ProcName);
        //End : Windows API function definition

        //Constructor
        public ManshoorGsmDllWrapper(string DllName)
        {
            try
            {
                if (this.LoadDll(DllName))
                {
                    this._status = STATUS.READY;
                }
                else
                {
                    this._status = STATUS.INVALID;
                    Dispose(false);
                }
            }
            catch (Exception)
            {
                this._status = STATUS.INVALID;
                Dispose(false);
            }
        }
        public ManshoorGsmDllWrapper()
        {
            try
            {
                if (this.LoadDll(_dllName))
                {
                    this._status = STATUS.READY;
                }
                else
                {
                    this._status = STATUS.INVALID;
                    Dispose(false);
                }
            }
            catch (Exception)
            {
                this._status = STATUS.INVALID;
                Dispose(false);
            }
        }
        //End : Constructor

        //End : DeConstructor
        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception)
            {
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (this._handle != IntPtr.Zero)
                {
                    FreeLibrary(this._handle);
                    this._handle = IntPtr.Zero;
                }
            }
            catch (Exception)
            {
            }
        }
        ~ManshoorGsmDllWrapper()
        {
            Dispose(false);
        }
        //End : DeConstructor


        //Get function method
        private T GetFunc<T>(string name) where T : Delegate
        {
            IntPtr ptr = GetProcAddress(this._handle, name);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            return Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }
        private T ExecuteDllFunction<T>(Func<T> func, string functionName)
        {
            ErrorReset();
            try
            {
                Task<T> task = Task.Run(func);
                if (!task.Wait(this._dllTimeoutMs))
                {
                    this._errorCode = ERROR_CODE.DLL_FUNC_TIMEOUT;
                    this._errorMessage = functionName + " timeout";
                    this._status = STATUS.INVALID;
                    return default(T);
                }
                return task.Result;
            }
            catch (Exception ex)
            {
                _errorCode = ERROR_CODE.DLL_FUNC_CALL_ERROR;
                this._errorMessage = functionName + " - " + ex.Message;
                this._status = STATUS.INVALID;
                return default(T);
            }
        }
        private class DllCallResult<T>
        {
            public bool Result { get; set; }
            public T Value { get; set; }
        }
        //End : Get function method

        //load Dll
        private bool LoadDll(string DllName)
        {
            bool result = true;
            this._errorCode = ERROR_CODE.NO_ERROR;
            try
            {
                //Load Dll
                this._dllName = DllName;
                if (this._handle != IntPtr.Zero)
                {
                    FreeLibrary(this._handle);
                }
                this._handle = LoadLibrary(DllName);
                if (this._handle == IntPtr.Zero)
                {
                    //DLL load error
                    result = false;
                    this._errorCode = ERROR_CODE.DLL_LOAD_FAIL;
                    this._errorMessage = DllName;
                    //End : DLL load error
                }
                //End : Load Dll
            }

            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_LOAD_FAIL;
                this._errorMessage = ex.Message;
                result = false;
            }

            try
            {
                //Get functions

                //Common Main function
                {
                    if (result)
                    {
                        this._errorMessage = "SMS_Device_Count";
                        this._smsDeviceCount = GetFunc<SMS_Device_Count_Delegete>("SMS_Device_Count");
                        if (this._smsDeviceCount == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                }
                //End : Common Main function

                //Device connection
                {
                    if (result)
                    {
                        this._errorMessage = "SMS_Select_Device";
                        this._smsSelectDevice = GetFunc<SMS_Select_Device_Delegete>("SMS_Select_Device");
                        if (this._smsSelectDevice == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "SMS_SelectDeicve_ByIndex";
                        this._smsSelectDeviceByIndex = GetFunc<SMS_SelectDevice_ByIndex_Delegete>("SMS_SelectDeicve_ByIndex");
                        if (this._smsSelectDeviceByIndex == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                }
                //End : Device connection

                //Device Hardware setting
                {
                    if (result)
                    {
                        this._errorMessage = "GSM_Set_Device_NO";
                        this._gsmSetDeviceNO = GetFunc<GSM_Set_Device_NO_Delegete>("GSM_Set_Device_NO");
                        if (this._gsmSetDeviceNO == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "GSM_Get_Device_NO";
                        this._gsmGetDeviceNO = GetFunc<GSM_Get_Device_NO_Delegete>("GSM_Get_Device_NO");
                        if (this._gsmGetDeviceNO == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "R_IMEI";
                        this._rIMEI = GetFunc<R_IMEI_Delegete>("R_IMEI");
                        if (this._rIMEI == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "GSM_ON";
                        this._gsmON = GetFunc<GSM_ON_Delegete>("GSM_ON");
                        if (this._gsmON == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "GSM_OFF";
                        this._gsmOFF = GetFunc<GSM_OFF_Delegete>("GSM_OFF");
                        if (this._gsmOFF == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "R_GSM_POW";
                        this._rgsmPow = GetFunc<R_GSM_Pow_Delegete>("R_GSM_POW");
                        if (this._rgsmPow == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "GSM_Ready";
                        this._gsmReady = GetFunc<GSM_Ready_Delegete>("GSM_Ready");
                        if (this._gsmReady == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "GSM_Test_Connect";
                        this._gsmTestConnect = GetFunc<GSM_Test_Connect_Delegete>("GSM_Test_Connect");
                        if (this._gsmTestConnect == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "SMS_Dev_INFO";
                        this._smsDevInfo = GetFunc<SMS_Dev_INFO_Delegete>("SMS_Dev_INFO");
                        if (this._smsDevInfo == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "SMS_Soft_Ver";
                        this._smsSoftVer = GetFunc<SMS_Soft_Ver_Delegete>("SMS_Soft_Ver");
                        if (this._smsSoftVer == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "Get_GSM_Message";
                        this._getGSMMessage = GetFunc<Get_GSM_Message_Delegete>("Get_GSM_Message");
                        if (this._getGSMMessage == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "R_GSM_Date_Time";
                        this._rgsmDateTime = GetFunc<R_GSM_Date_Time_Delegete>("R_GSM_Date_Time");
                        if (this._rgsmDateTime == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "Set_GSM_Date_Time";
                        this._setGSMDateTime = GetFunc<Set_GSM_Date_Time_Delegete>("Set_GSM_Date_Time");
                        if (this._setGSMDateTime == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                }
                //End : Device Hardware setting

                //Device Network setting
                {
                    if (result)
                    {
                        this._errorMessage = "R_Natttional_Mobile_ID";
                        this._rNatttionalMobileID = GetFunc<R_Natttional_Mobile_ID_Delegete>("R_Natttional_Mobile_ID");
                        if (this._rNatttionalMobileID == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "R_SIM_ID";
                        this._rSimID = GetFunc<R_SIM_ID_Delegete>("R_SIM_ID");
                        if (this._rSimID == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "Selected_Network";
                        this._selectedNetwork = GetFunc<Selected_Network_Delegete>("Selected_Network");
                        if (this._selectedNetwork == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "R_SIM_PIN";
                        this._rsimPin = GetFunc<R_SIM_PIN_Delegete>("R_SIM_PIN");
                        if (this._rsimPin == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "GSM_Signal";
                        this._gsmSignal = GetFunc<GSM_Signal_Delegete>("GSM_Signal");
                        if (this._gsmSignal == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "Select_SIM_Network";
                        this._selectSIMNetwork = GetFunc<Select_SIM_Network_Delegete>("Select_SIM_Network");
                        if (this._selectSIMNetwork == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "Insert_PIN";
                        this._insertPIN = GetFunc<Insert_PIN_Delegete>("Insert_PIN");
                        if (this._insertPIN == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "R_SMS_Center";
                        this._rsmsCenter = GetFunc<R_SMS_Center_Delegete>("R_SMS_Center");
                        if (this._rsmsCenter == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "Change_SMS_Center";
                        this._changesmsCenter = GetFunc<Change_SMS_Center_Delegete>("Change_SMS_Center");
                        if (this._changesmsCenter == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "SMS_PDU_Mode";
                        this._smsPDUMode = GetFunc<SMS_PDU_Mode_Delegete>("SMS_PDU_Mode");
                        if (this._smsPDUMode == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "SMS_Char_Mode";
                        this._smsCharMode = GetFunc<SMS_Char_Mode_Delegete>("SMS_Char_Mode");
                        if (this._smsCharMode == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                }
                //End : Device Network setting

                //SMS & AT Command setting
                if (result)
                {
                    this._errorMessage = "Res_AT_Delay";
                    this._resATDelay = GetFunc<Res_AT_Delay_Delegete>("Res_AT_Delay");
                    if (this._resATDelay == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                //End : SMS & AT Command setting

                //SMS Send
                {
                    if (result)
                    {
                        this._errorMessage = "S_SMS3";
                        this._sSMS3 = GetFunc<S_SMS3_Delegete>("S_SMS3");
                        if (this._sSMS3 == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    //Send_Text_SMS_Delegete -> NOT WORK
                    if (result)
                    {
                        this._errorMessage = "Send_Text_SMS";
                        this._sendTextSMS = GetFunc<Send_Text_SMS_Delegete>("Send_Text_SMS");
                        if (this._sendTextSMS == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "R_SMS_Ref";
                        this._rSMSRef = GetFunc<R_SMS_Ref_Delegete>("R_SMS_Ref");
                        if (this._rSMSRef == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "Send_SMS_Error_Code";
                        this._sendSMSErrorCode = GetFunc<Send_SMS_Error_Code_Delegete>("Send_SMS_Error_Code");
                        if (this._sendSMSErrorCode == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                }
                //End : SMS Send

                //AT Command
                if (result)
                {
                    this._errorMessage = "GSM_Send_AT";
                    this._gsmSendAT = GetFunc<GSM_Send_AT_Delegete>("GSM_Send_AT");
                    if (this._gsmSendAT == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                //End : AT Command

                //Buffer Manager
                {
                    if (result)
                    {
                        this._errorMessage = "Last_Buf_Index";
                        this._lastBufIndex = GetFunc<Last_Buf_Index_Delegete>("Last_Buf_Index");
                        if (this._lastBufIndex == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "Clear_Buf";
                        this._clearBuf = GetFunc<Clear_Buf_Delegete>("Clear_Buf");
                        if (this._clearBuf == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "GSM_MSG";
                        this._gsmMSG = GetFunc<GSM_MSG_Delegete>("GSM_MSG");
                        if (this._gsmMSG == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                }
                //End : Buffer Manager

                //SMS Inbox
                //New SMS Inbox Event , ACtive/DeActive , IndexOfSMS/SMS Number
                {
                    if (result)
                    {
                        this._errorMessage = "Active_New_SMS_Event";
                        this._activeNewSMSEvent = GetFunc<Active_New_SMS_Event_Delegete>("Active_New_SMS_Event");
                        if (this._activeNewSMSEvent == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "Recieved_New_SMS";
                        this._recievedNewSMS = GetFunc<Recieved_New_SMS_Delegete>("Recieved_New_SMS");
                        if (this._recievedNewSMS == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                }
                //End : New SMS Inbox Event , ACtive/DeActive , IndexOfSMS/SMS Number
                //Inbox Storage
                {
                    if (result)
                    {
                        this._errorMessage = "Get_SMS_NO";
                        this._getSMSNO = GetFunc<Get_SMS_NO_Delegete>("Get_SMS_NO");
                        if (this._getSMSNO == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                    if (result)
                    {
                        this._errorMessage = "Get_SMS_Space";
                        this._getSMSSpace = GetFunc<Get_SMS_Space_Delegete>("Get_SMS_Space");
                        if (this._getSMSSpace == null)
                        {
                            this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                            result = false;
                        }
                    }
                }
                //End : Inbox Storage
                //SMS Inbox Delete
                if (result)
                {
                    this._errorMessage = "SMS_Delete";
                    this._smsDelete = GetFunc<SMS_Delete_Delegete>("SMS_Delete");
                    if (this._smsDelete == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                //End : SMS Inbox Delete
                //SMS Inbox raed
                if (result)
                {
                    this._errorMessage = "R_SMS4";
                    this._rSMS4 = GetFunc<R_SMS4_Delegete>("R_SMS4");
                    if (this._rSMS4 == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                //End : SMS Inbox raed
                //End : SMS Inbox

                /*
                //Group SMS
                if (result)
                {
                    this._errorMessage = "Group_SMS_Setting";
                    this._groupSMSSetting = GetFunc<Group_SMS_Setting_Delegete>("Group_SMS_Setting");
                    if (this._groupSMSSetting == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "Add_Group_SMS_Buffer";
                    this._addGroupSMSBuffer = GetFunc<Add_Group_SMS_Buffer_Delegete>("Add_Group_SMS_Buffer");
                    if (this._addGroupSMSBuffer == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "Get_SMS_Buf_Status";
                    this._getSMSBufStatus = GetFunc<Get_SMS_Buf_Status_Delegete>("Get_SMS_Buf_Status");
                    if (this._getSMSBufStatus == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }

                if (result)
                {
                    this._errorMessage = "SMS_Buf_Pause";
                    this._smsBufPause = GetFunc<SMS_Buf_Pause_Delegete>("SMS_Buf_Pause");
                    if (this._smsBufPause == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "Get_SMS_Buf_Sended";
                    this._getSMSBufSended = GetFunc<Get_SMS_Buf_Sended_Delegete>("Get_SMS_Buf_Sended");
                    if (this._getSMSBufSended == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "Get_SMS_Buf_Indx";
                    this._getSMSBufIndx = GetFunc<Get_SMS_Buf_Indx_Delegete>("Get_SMS_Buf_Indx");
                    if (this._getSMSBufIndx == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "SMS_Buf_Start";
                    this._smsBufStart = GetFunc<SMS_Buf_Start_Delegete>("SMS_Buf_Start");
                    if (this._smsBufStart == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "SMS_Buf_Clear";
                    this._smsBufClear = GetFunc<SMS_Buf_Clear_Delegete>("SMS_Buf_Clear");
                    if (this._smsBufClear == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                //End : Group SMS
                */

                /*
                //Offline SMS -> Not Work
                if (result)
                {
                    this._errorMessage = "Total_Oflline_SMS_NO";
                    this._totalOfllineSMSNO = GetFunc<Total_Oflline_SMS_NO_Delegete>("Total_Oflline_SMS_NO");
                    if (this._totalOfllineSMSNO == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "Total_Oflline_SMS_NO";
                    this._showOfllineSMS = GetFunc<Show_Oflline_SMS_Delegete>("Total_Oflline_SMS_NO");
                    if (this._showOfllineSMS == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "Clear_Oflline_SMS_NO";
                    this._clearOfllineSMSNO = GetFunc<Clear_Oflline_SMS_NO_Delegete>("Clear_Oflline_SMS_NO");
                    if (this._clearOfllineSMSNO == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "G_Offline_SMS_Active";
                    this._gOfflineSMSActive = GetFunc<G_Offline_SMS_Active_Delegete>("G_Offline_SMS_Active");
                    if (this._gOfflineSMSActive == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "Offline_SMS_Active";
                    this._offlineSMSActive = GetFunc<Offline_SMS_Active_Delegete>("Offline_SMS_Active");
                    if (this._offlineSMSActive == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                //End : Offline SMS -> Not Work
                */

                /*
                //Call Functions
                if (result)
                {
                    this._errorMessage = "Disconnect_Voice_Call";
                    this._disconnectVoiceCall = GetFunc<Disconnect_Voice_Call_Delegete>("Disconnect_Voice_Call");
                    if (this._disconnectVoiceCall == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "GSM_Answer_Call";
                    this._gsmAnswerCall = GetFunc<GSM_Answer_Call_Delegete>("GSM_Answer_Call");
                    if (this._gsmAnswerCall == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "GSM_Dial_NO";
                    this._gsmDialNO = GetFunc<GSM_Dial_NO_Delegete>("GSM_Dial_NO");
                    if (this._gsmDialNO == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "CallerID_Enable";
                    this._callerIDEnable = GetFunc<CallerID_Enable_Delegete>("CallerID_Enable");
                    if (this._callerIDEnable == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                if (result)
                {
                    this._errorMessage = "CallIn_Number";
                    this._callInNumber = GetFunc<CallIn_Number_Delegete>("CallIn_Number");
                    if (this._callInNumber == null)
                    {
                        this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                        result = false;
                    }
                }
                //End : Call Functions
                */

                //End : Get functions
            }
            catch (Exception ex)
            {
                this._errorCode = ERROR_CODE.DLL_FUNC_LOAD_FAIL;
                this._errorMessage = ex.Message;
                result = false;
            }
            if (result)
            {
                this._errorMessage = "";
            }
            return result;
        }
        //End : load Dll
    }
}
