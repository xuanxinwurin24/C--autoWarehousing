using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ASEWEB.Models
{
    public class Download_CarouselUtility_Modify
    {
        public string User  { get; set; }
        public string Command_ID  { get; set; }

        public Download_CarouselUtility_Modify()
        {
            User = string.Empty;
            Command_ID = string.Empty;

        }
        public Download_CarouselUtility_Modify(string Command_ID_, string User_)
        {
            User = User_;
            Command_ID = Command_ID_;
        }
    }
}