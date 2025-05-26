using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ASEWEB.Models
{
    public class Download_CSVModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Group { get; set; }
        public string Auth { get; set; }
        public string UserSystemSetting { get; set; }
        public List<string> TM_List { get; set; }
        public Download_CSVModel()
        {
            ID = string.Empty;
            Name = string.Empty;
            Password = string.Empty;
            Group = string.Empty;
            Auth = string.Empty;
            UserSystemSetting = string.Empty;
        }
        public Download_CSVModel(string _ID,string _Name,string _Password,string _Group,string _Auth,string _UserSystemSetting)
        {
            ID = _ID;
            Name = _Name;
            Password = _Password;
            Group = _Group;
            Auth = _Auth;
            UserSystemSetting = _UserSystemSetting;
        }
    }
}