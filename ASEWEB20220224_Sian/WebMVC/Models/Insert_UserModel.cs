using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace ASEWEB.Models
{
    public class Insert_UserModel
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string UserPass { get; set; }
        public string UserGroup { get; set; }
        public Insert_UserModel(string _UserID,string _UserName,string _UserPass,string _UserGroup)
        {
            UserID = string.Empty;
            UserName = string.Empty;
            UserPass = string.Empty;
            UserGroup = string.Empty;

            UserID = _UserID;
            UserName = _UserName;
            UserPass = _UserPass;
            UserGroup = _UserGroup;

        }
        public string Insert_User() 
        {
            var C = new ASEWEB.Models.SQLContext();
            return C.Insert_User(UserID,UserName,UserPass,UserGroup);
        }
    }
}