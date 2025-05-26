using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;

namespace ASEWEB.Models
{
    public class Modify_UserModel
    {
        public string UserGroup { get; set; }
        public string UserAuthority { get; set; }
        public string UserSystemSetting { get; set; }
        public Modify_UserModel()
        {
            UserGroup = string.Empty;
            UserAuthority = string.Empty;
            UserSystemSetting = string.Empty;
        }
        public Modify_UserModel(string _UserGroup)
		{
            UserGroup = _UserGroup;
            try
			{
                var C = new ASEWEB.Models.SQLContext();
                UserAuthority = C.UserAuth(UserGroup)[0];
                UserSystemSetting = C.UserAuth(UserGroup)[1];
            }
			catch(Exception ex)
            {
                UserAuthority = "";
                UserSystemSetting = "";
            }
            

        }
        public Modify_UserModel(string _UserGroup,string _UserAuthority, string _UserSystemSetting)
        {
            UserGroup = string.Empty;
            UserAuthority = string.Empty;
            UserSystemSetting = string.Empty;
            UserGroup = _UserGroup;
            UserAuthority = _UserAuthority;
            UserSystemSetting = _UserSystemSetting;
        }
        //public List<string> UserSetting_List() 
        //{
        //    var C = new ASEWEB.Models.SQLContext();
        //}
        public void Update_Group_Authority()
        {
            var C = new ASEWEB.Models.SQLContext();
            C.Update_Group_Authority(UserGroup, UserAuthority, UserSystemSetting);
        }
    }
}