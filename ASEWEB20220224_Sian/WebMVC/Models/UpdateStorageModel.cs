using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace ASEWEB.Models
{
    public class UpdateStorageModel
    {
        public string BOX_ID { get; set; }
        //public string Batch_No { get; set; }
        //public string Group_No { get; set; }
        //public string Soteria { get; set; }
        //public string Customer_ID { get; set; }
        public string Carousel_ID { get; set; }
        public string Cell_ID { get; set; }
        public string Target_Carousel_ID { get; set; }
        public string Target_Cell_ID { get; set; }
        public string User { get; set; }
        public UpdateStorageModel()
        {
            BOX_ID = string.Empty;
            //Batch_No = string.Empty;
            //Group_No = string.Empty;
            //Soteria = string.Empty;
            //Customer_ID = string.Empty;
            Carousel_ID = string.Empty;
            Cell_ID = string.Empty;
            Target_Carousel_ID = string.Empty;
            Target_Cell_ID = string.Empty;
            
        }
        public UpdateStorageModel(string _BOX_ID,string _Target_Carousel_ID,string _Target_Cell_ID,string _User)//,string _Batch_No,string _Group_No, string _Soteria, string _Customer_ID,string _Carousel_ID,string _Cell_ID
        {
            BOX_ID = _BOX_ID;
            //Batch_No = _Batch_No;
            //Group_No = _Group_No;
            //Soteria = _Soteria;
            //Customer_ID = _Customer_ID;
            //Carousel_ID = _Carousel_ID;
            //Cell_ID = _Cell_ID;
            Target_Carousel_ID = _Target_Carousel_ID;
            Target_Cell_ID = _Target_Cell_ID;
            User = _User;
        }
    }
}