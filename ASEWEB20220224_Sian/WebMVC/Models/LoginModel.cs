using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace ASEWEB.Models
{
    public class LoginModel
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string UserPWD { get; set; }
        public string Authority { get; set; }
        public string UserGroup { get; set; }
        public string UserSystemSetting { get; set; }
        public LoginModel()
        {
            UserID = string.Empty;
            UserPWD = string.Empty;
            Authority = string.Empty;
        }
        public LoginModel(string _UserID, string _UserPWD)
        {
            UserID = string.Empty;
            UserName = string.Empty;
            UserPWD = string.Empty;
            Authority = string.Empty;
            UserGroup = string.Empty;
            UserID = _UserID;
            UserPWD = _UserPWD;
            
        }
        public string Authority_string()
        {
            var C = new ASEWEB.Models.SQLContext();
            Authority = C.Login_Authority(UserID, UserPWD);
            UserGroup = C.Login_Group(UserID, UserPWD);
            UserName = C.Login_Name(UserID, UserPWD);
            UserSystemSetting = C.Login_SystemSetting(UserID, UserPWD);
            return Authority;
        }
    }
}