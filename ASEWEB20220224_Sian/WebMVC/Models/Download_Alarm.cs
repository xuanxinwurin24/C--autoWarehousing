using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ASEWEB.Models
{
    public class Download_Alarm
    {
        public string ID  { get; set; }
        public string Unit_Name  { get; set; }
        public string Occured_Time { get; set; }
        public string Reset_Time { get; set; }
        public string Message { get; set; }

        public Download_Alarm()
        {
            ID = string.Empty;
            Unit_Name = string.Empty;
            Occured_Time = string.Empty;
            Reset_Time = string.Empty;
            Message = string.Empty;

        }
        public Download_Alarm(string ID_, string Unit_Name_,string Occured_Time_,string Reset_Time_,string Message_)
        {
            ID = ID_;
            Unit_Name = Unit_Name_;
            Occured_Time = Occured_Time_;
            Reset_Time = Reset_Time_;
            Message = Message_;
        }
    }
}