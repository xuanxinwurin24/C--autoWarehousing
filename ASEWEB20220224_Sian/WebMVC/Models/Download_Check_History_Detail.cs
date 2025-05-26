using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ASEWEB.Models
{
    public class Download_Check_History_Detail
    {
        public string Command_ID  { get; set; }
        public string Carousel_ID { get; set; }
        public string Cell_ID { get; set; }
        public string Batch_No { get; set; }
        public string BOX_ID { get; set; }
        public string Group_No { get; set; }
        public string Soteria { get; set; }
        public string Customer_ID { get; set; }
        public string Check_Result { get; set; }
        public string User_ID { get; set; }
        public Download_Check_History_Detail()
        {
            Command_ID = string.Empty;
            Carousel_ID = string.Empty;
            Cell_ID = string.Empty;
            Batch_No = string.Empty;
            BOX_ID = string.Empty;
            Group_No = string.Empty;
            Soteria = string.Empty;
            Customer_ID = string.Empty;
            Check_Result = string.Empty;
            User_ID = string.Empty;
        }
        public Download_Check_History_Detail(string Command_ID_,string Carousel_ID_, string Cell_ID_,string Batch_No_,string BOX_ID_,string Group_No_,string Soteria_,string Customer_ID_,string Check_Result_,string User_ID_)
        {
            var C =new ASEWEB.Models.SQLContext();
            Command_ID = Command_ID_;
            Carousel_ID = Carousel_ID_;
            Carousel_ID = C.Show_CSID_Transfer(Carousel_ID);
            Cell_ID = Cell_ID_;
            Cell_ID = C.Show_CEID_Transfer(Cell_ID);
            Batch_No = Batch_No_;
            BOX_ID = BOX_ID_;
            Group_No = Group_No_;
            Soteria = Soteria_;
            Customer_ID = Customer_ID_;
            if (Check_Result_ == "0")
                Check_Result = "OK";
            else
                Check_Result = "NG";
            User_ID = User_ID_;
        }
    }
}