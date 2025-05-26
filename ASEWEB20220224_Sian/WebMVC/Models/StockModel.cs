using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace ASEWEB.Models
{
    public class StockModel
    {
        public string Batch_No { get; set; }
        public string Batch_No1 { get; set; }
        public string Batch_No2 { get; set; }
        public string NO { get; set; }
        public string NO1 { get; set; }
        public string NO2 { get; set; }
        public string Soteria { get; set; }
        public string Customer_ID { get; set; }
        public string BOX_ID { get; set; }
        public StockModel()
        {
            NO = string.Empty;
            Batch_No = string.Empty;
            Soteria = string.Empty;
            Customer_ID = string.Empty;
            BOX_ID = string.Empty;
            Batch_No1 = string.Empty;
            Batch_No2 = string.Empty;
            NO1 = string.Empty;
            NO2 = string.Empty;
        }
        public StockModel(string _NO,string _Batch_No,string _Soteria,string _Customer_ID,string _BOX_ID)
        {
            NO = string.Empty;
            Batch_No = string.Empty;
            Soteria = string.Empty;
            Customer_ID = string.Empty;
            BOX_ID = string.Empty;
            Batch_No1 = string.Empty;
            Batch_No2 = string.Empty;
            NO1 = string.Empty;
            NO2 = string.Empty;
            NO = _NO;
            Batch_No = _Batch_No;
            Soteria = _Soteria;
            Customer_ID = _Customer_ID;
            BOX_ID = _BOX_ID;
        }
        public List<string> Set_BatchNO(string _Batch_No1, string _Batch_No2)
        {
            Batch_No1 = _Batch_No1;
            Batch_No2 = _Batch_No2;
            return Stock_String();
        }
        public List<string> Set_No( string _NO1, string _NO2)
        {
            NO1 = _NO1;
            NO2 = _NO2;
            return Stock_String();
        }
        public List<string> Stock_String()
        {
            var C = new ASEWEB.Models.SQLContext();
            var Stock = C.Stock_Select_List(Batch_No1,Batch_No2, NO1,NO2);
            return Stock;
        }
    }
}