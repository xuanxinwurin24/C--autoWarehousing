using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ASEWEB.Models
{
    public class Download_STK_DoorHistory
    {
        public string Carousel_ID  { get; set; }
        public string OP_Time { get; set; }
        public string Close_Time { get; set; }
        public string OP_Sec { get; set; }
        public Download_STK_DoorHistory()
        {
            Carousel_ID = string.Empty;
            OP_Time = string.Empty;
            Close_Time = string.Empty;
            OP_Sec = string.Empty;
        }
        public Download_STK_DoorHistory(string Carousel_ID_,string OP_Time_,string Close_Time_, string OP_Sec_)
        {
            var C = new ASEWEB.Models.SQLContext();
            Carousel_ID = Carousel_ID_;
            Carousel_ID = C.Show_CSID_Transfer(Carousel_ID);
            OP_Time = OP_Time_;
            Close_Time=Close_Time_;
            OP_Sec=OP_Sec_;
        }
    }
}