using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace ASEWEB.Models
{
    public class AlarmModel
    {
        public string ID { get; set; }
        public string Unit_Name { get; set; }
        public string Time { get; set; }
        public string Message { get; set; }
        public List<string> TM_List { get; set; }
        public AlarmModel()
        {

        }
        public AlarmModel(string _ID,  string _Unit_Name, string _Time,string _Message)
        {
            ID = string.Empty;
            Time = string.Empty;
            Unit_Name = string.Empty;
            Message = string.Empty;
            ID = _ID;
            Time = _Time;
            Unit_Name = _Unit_Name;
            Message = _Message;
        }

    }
}