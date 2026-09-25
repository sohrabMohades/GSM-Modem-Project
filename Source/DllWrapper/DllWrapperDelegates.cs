using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GSM.Core.Source.DllWrapper
{

    //Delegate definition

    //Common Main function
    internal delegate short SMS_Device_Count_Delegete();
    //End : Common Main function

    //Device connection
    internal delegate void SMS_Select_Device_Delegete(short DeviceNumber);
    internal delegate void SMS_SelectDevice_ByIndex_Delegete(short DeviceIndex);
    //End : Device connection

    //Device Hardware setting
    internal delegate bool GSM_Set_Device_NO_Delegete(int DeviceNumber);
    internal delegate int GSM_Get_Device_NO_Delegete();
    internal delegate void R_IMEI_Delegete([MarshalAs(UnmanagedType.BStr)] out string IMEI);
    internal delegate bool GSM_ON_Delegete();
    internal delegate bool GSM_OFF_Delegete();
    internal delegate bool R_GSM_Pow_Delegete();
    internal delegate bool GSM_Ready_Delegete();
    internal delegate bool GSM_Test_Connect_Delegete();
    internal delegate void SMS_Dev_INFO_Delegete([MarshalAs(UnmanagedType.BStr)] out string DeviceInfo);
    internal delegate void SMS_Soft_Ver_Delegete([MarshalAs(UnmanagedType.BStr)] out string SoftwareVersion);
    internal delegate void Get_GSM_Message_Delegete(out GSM_Message_Struct Gsm_Msg);
    internal delegate void R_GSM_Date_Time_Delegete([MarshalAs(UnmanagedType.BStr)] out string DateTime);
    internal delegate bool Set_GSM_Date_Time_Delegete(int GSM_Year, int GSM_Month, int GSM_Day, int GSM_Hour, int GSM_Minute, int GSM_Second);
    //End : Device Hardware setting

    //Network setting
    internal delegate void R_Natttional_Mobile_ID_Delegete([MarshalAs(UnmanagedType.BStr)] out string NMobileID);
    internal delegate void R_SIM_ID_Delegete([MarshalAs(UnmanagedType.BStr)] out string SimID);
    internal delegate void Selected_Network_Delegete([MarshalAs(UnmanagedType.BStr)] out string Network);
    internal delegate void R_SIM_PIN_Delegete([MarshalAs(UnmanagedType.BStr)] out string SimPin);
    internal delegate void GSM_Signal_Delegete([MarshalAs(UnmanagedType.BStr)] out string Signal);
    internal delegate bool Select_SIM_Network_Delegete();
    internal delegate bool Insert_PIN_Delegete([MarshalAs(UnmanagedType.BStr)] string Pin);
    internal delegate void R_SMS_Center_Delegete([MarshalAs(UnmanagedType.BStr)] out string Result);
    internal delegate bool Change_SMS_Center_Delegete([MarshalAs(UnmanagedType.BStr)] string SMS_Center);
    internal delegate void SMS_PDU_Mode_Delegete(bool PDU);
    internal delegate bool SMS_Char_Mode_Delegete(int CharMode);
    //End : Network setting

    //SMS & AT Command setting
    internal delegate void Res_AT_Delay_Delegete(int Delay);
    //End : SMS & AT Command setting

    //SMS Send
    internal delegate void S_SMS3_Delegete([MarshalAs(UnmanagedType.BStr)] out string Result, [MarshalAs(UnmanagedType.BStr)] string Phone_NO,
                                   [MarshalAs(UnmanagedType.BStr)] string SMS_Text, bool Request_Delivery_Report, int Send_Interval, int Retry_Times,
                                   int SMS_Validity, int SMS_Reference, [MarshalAs(UnmanagedType.BStr)] string SMS_Center);
    //Send_Text_SMS_Delegete -> NOT WORK
    internal delegate bool Send_Text_SMS_Delegete(bool PDU, [MarshalAs(UnmanagedType.BStr)] string Country_Code, [MarshalAs(UnmanagedType.BStr)] string Phone_NO,
                                         [MarshalAs(UnmanagedType.BStr)] string SMS_TXT);
    internal delegate void R_SMS_Ref_Delegete([MarshalAs(UnmanagedType.BStr)] out string Result);
    internal delegate void Send_SMS_Error_Code_Delegete([MarshalAs(UnmanagedType.BStr)] out string Result);
    //End : SMS Send

    //AT Command
    internal delegate void GSM_Send_AT_Delegete([MarshalAs(UnmanagedType.BStr)] string AT_Input, ref int AtOutput_Length, [MarshalAs(UnmanagedType.BStr)] out string AT_OutPut);
    //End : AT Command

    //Buffer Manager
    internal delegate Int16 Last_Buf_Index_Delegete();
    internal delegate void GSM_MSG_Delegete([MarshalAs(UnmanagedType.BStr)] out string Result, ref int GSM_CMD, ref int GSM_Len);
    internal delegate void Clear_Buf_Delegete();
    //End : Buffer Manager

    //SMS Inbox
    //New SMS Inbox Event , ACtive/DeActive , IndexOfSMS/SMS Number
    internal delegate bool Active_New_SMS_Event_Delegete(bool NS_Active);
    internal delegate bool Recieved_New_SMS_Delegete(ref int Index_From, ref int Index_To);
    //End : New SMS Inbox Event , ACtive/DeActive , IndexOfSMS/SMS Number
    //Inbox Storage
    internal delegate int Get_SMS_NO_Delegete();
    internal delegate void Get_SMS_Space_Delegete(out SMS_Space_Struct SMS_Space);
    //End : Inbox Storage
    //SMS Inbox Delete
    internal delegate bool SMS_Delete_Delegete(int Index, int Delete_type);
    //End : SMS Inbox Delete
    //SMS Inbox raed
    internal delegate bool R_SMS4_Delegete([MarshalAs(UnmanagedType.BStr)] out string Result, bool PDU, int Index, [MarshalAs(UnmanagedType.BStr)] out string SMS_Center,
                      [MarshalAs(UnmanagedType.BStr)] out string SMS_Type, [MarshalAs(UnmanagedType.BStr)] out string Tel_Number,
                      [MarshalAs(UnmanagedType.BStr)] out string Time_stamp, [MarshalAs(UnmanagedType.BStr)] out string S_Discharge,
                      [MarshalAs(UnmanagedType.BStr)] out string SMS_Text, [MarshalAs(UnmanagedType.BStr)] out string TP_PID,
                      [MarshalAs(UnmanagedType.BStr)] out string TP_DCS, [MarshalAs(UnmanagedType.BStr)] out string TP_VP,
                      [MarshalAs(UnmanagedType.BStr)] out string TP_ST, [MarshalAs(UnmanagedType.BStr)] out string S_Reference,
                      [MarshalAs(UnmanagedType.BStr)] out string All_Part, [MarshalAs(UnmanagedType.BStr)] out string Part_NO);
    //End : SMS Inbox raed
    //End : SMS Inbox


    /*
    //Group SMS
    internal delegate void Add_Group_SMS_Buffer_Delegete([MarshalAs(UnmanagedType.BStr)] string Phone_NO, [MarshalAs(UnmanagedType.BStr)] string SMS_Context);
    internal delegate void SMS_Buf_Clear_Delegete(bool Stop);
    internal delegate void SMS_Buf_Start_Delegete(bool Start);
    internal delegate int Get_SMS_Buf_Indx_Delegete();
    internal delegate void Group_SMS_Setting_Delegete(bool SMS_Delivery, int SMS_Validity, int SMS_NO_Change_Sim, int Repeat_NO,
                                                     int Send_Interval, [MarshalAs(UnmanagedType.BStr)] string SMS_Center);
    internal delegate bool Get_SMS_Buf_Status_Delegete(int index, ref bool Send_Done);
    internal delegate void SMS_Buf_Pause_Delegete();
    internal delegate int Get_SMS_Buf_Sended_Delegete();
    //End : Group SMS
    */

    /*
    //Offline SMS -> Not Work
    internal delegate int Total_Oflline_SMS_NO_Delegete();
    internal delegate void Show_Oflline_SMS_Delegete(out OffLine_SMS_Struct OffLine_SMS, int Index);
    internal delegate bool Clear_Oflline_SMS_NO_Delegete();
    internal delegate bool G_Offline_SMS_Active_Delegete();
    internal delegate void Offline_SMS_Active_Delegete(bool Active);
    //End : Offline SMS -> Not Work
    */

    /*
    //Call Functions
    internal delegate bool Disconnect_Voice_Call_Delegete();
    internal delegate bool GSM_Answer_Call_Delegete();
    internal delegate bool GSM_Dial_NO_Delegete(out GSM_Dial_Struct Gsm_Dial, [MarshalAs(UnmanagedType.BStr)] string CountryCode, [MarshalAs(UnmanagedType.BStr)] string Phone_NO);
    internal delegate bool CallerID_Enable_Delegete(bool Active);
    internal delegate void CallIn_Number_Delegete([MarshalAs(UnmanagedType.BStr)] out string CallNumber);
    //End : Call Functions
    */

    //End : Delegate definition

}
