using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ASEWEB.Models
{
    public class Download_Check_History
    {
        public string User  { get; set; }
        public string Command_ID  { get; set; }
        public string Check_Result { get; set; }
        public string Start_Time { get; set; }
        public string End_Time { get; set; }
        public string NG_Reason { get; set; }

        public Download_Check_History()
        {
            User = string.Empty;
            Command_ID = string.Empty;
            Check_Result = string.Empty;
            Start_Time = string.Empty;
            End_Time = string.Empty;
            NG_Reason = string.Empty;
            
        }
        public Download_Check_History(string Command_ID_,string Check_Result_,string Start_Time_,string End_Time_, string User_,string NG_Reason_)
        {
            User = string.Empty;
            Command_ID = Command_ID_;
            Check_Result = Check_Result_;
            Start_Time = Start_Time_;
            End_Time = End_Time_;
            NG_Reason = NG_Reason_;
            User = User_;
        }
    }
}