using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace ASEWEB.Models
{
    public class Log_RecordModel
    {
        public string Type { get; set; }
        public string Time { get; set; }
        public string Message { get; set; }
        public List<string> TM_List { get; set; }
        public Log_RecordModel(string _Type)
        {
            Type = string.Empty;
            Time = string.Empty;
            Message = string.Empty;
            TM_List = new List<string>();
            Type = _Type;
            var C = new ASEWEB.Models.SQLContext();
            TM_List = C.Log_Record_List(Type);
        }
        public List<string> Log_Record_List()
        {
            var C = new ASEWEB.Models.SQLContext();

            return C.Log_Record_List(Type);
        }
    }
}