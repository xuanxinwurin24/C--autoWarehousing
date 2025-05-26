using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ASEWEB.Models
{
    public class Download_STK_CarouselDaily_History
    {
        public string Carousel_ID  { get; set; }
        public string Max_T { get; set; }
        public string Min_T { get; set; }
        public string Avg_T { get; set; }
        public string Max_H { get; set; }
        public string Min_H { get; set; }
        public string Avg_H { get; set; }
        public string Creation_Date { get; set; }
        public string OP_Times  { get; set; }
        public string Total_Sec { get; set; }


        public Download_STK_CarouselDaily_History()
        {
            Carousel_ID = string.Empty;
            Max_T = string.Empty;
            Min_T= string.Empty;
            Avg_T= string.Empty;
            Max_H= string.Empty;
            Min_H = string.Empty;
            Avg_H= string.Empty;
            Creation_Date = string.Empty;
            OP_Times = string.Empty;
            Total_Sec = string.Empty;

        }
        public Download_STK_CarouselDaily_History(string Carousel_ID_, string Max_T_, string Min_T_, string Avg_T_, string Max_H_, string Min_H_, string Avg_H_, string Creation_Date_, string OP_Times_, string Total_Sec_)
        {
            var C = new ASEWEB.Models.SQLContext();
            Carousel_ID = Carousel_ID_;
            Carousel_ID = C.Show_CSID_Transfer(Carousel_ID);
            Max_T = Max_T_;
            Min_T = Min_T_;
            Avg_T = Avg_T_;
            Max_H = Max_H_;
            Min_H = Min_H_;
            Avg_H = Avg_H_;
            Creation_Date = Creation_Date_;
            OP_Times = OP_Times_;
            Total_Sec = Total_Sec_;
        }
    }
}