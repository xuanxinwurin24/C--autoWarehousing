using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ASEWEB.Models
{
    public class Download_CarouselUtility_Modify_History
    {
        public string Command_ID  { get; set; }
        public string Show_Carousel_ID { get; set; }
        public string Temp_UPPER { get; set; }
        public string Temp_LOWER { get; set; }
        public string Hum_UPPER { get; set; }
        public string Hum_LOWER { get; set; }
        public string TURN_ON_N2 { get; set; }
        public string TURN_OFF_N2 { get; set; }
        public Download_CarouselUtility_Modify_History()
        {
            Command_ID = string.Empty;
            Show_Carousel_ID = string.Empty;
            Temp_UPPER = string.Empty;
            Temp_LOWER = string.Empty;
            Hum_UPPER = string.Empty;
            Hum_LOWER = string.Empty;
            TURN_ON_N2 = string.Empty;
            TURN_OFF_N2 = string.Empty;
        }
        public Download_CarouselUtility_Modify_History(string Command_ID_,string Show_Carousel_ID_, string Temp_UPPER_,string Temp_LOWER_,string Hum_UPPER_,string Hum_LOWER_,string TURN_ON_N2_,string TURN_OFF_N2_)
        {
            Command_ID = Command_ID_;
            Show_Carousel_ID = Show_Carousel_ID_;
            Temp_UPPER = Temp_UPPER_;
            Temp_LOWER = Temp_LOWER_;
            Hum_UPPER = Hum_UPPER_;
            Hum_LOWER = Hum_LOWER_;
            TURN_ON_N2 = TURN_ON_N2_;
            TURN_OFF_N2 = TURN_OFF_N2_;
        }
    }
}