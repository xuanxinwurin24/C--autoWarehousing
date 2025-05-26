using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace ASEWEB.Models
{
    public class StorageModel
    {
        public string Batch_No1 { get; set; }
        public string Batch_No2 { get; set; }
        public string Carousel_ID1 { get; set; }
        public string Carousel_ID2 { get; set; }
        public string Soteria1 { get; set; }
        public string Soteria2 { get; set; }
        public string Customer_ID1 { get; set; }
        public string Customer_ID2 { get; set; }
        public string id { get; set; }
        public string BOX_ID { get; set; }
        public string Batch_No { get; set; }
        public string Group_No { get; set; }
        public string Soteria { get; set; }
        public string Customer_ID { get; set; }
        public string Carousel_ID { get; set; }
        public string Cell_ID { get; set; }
        public string Target_Carousel_ID { get; set; }
        public string Target_Cell_ID { get; set; }
        public StorageModel()
        {
            Batch_No1 = string.Empty;
            Batch_No2 = string.Empty;
            Carousel_ID1 = string.Empty;
            Carousel_ID2 = string.Empty;
            Soteria1 = string.Empty;
            Soteria2 = string.Empty;
            Customer_ID1 = string.Empty;
            Customer_ID2 = string.Empty;
        }
        public StorageModel(string _BOX_ID,string _Batch_No,string _Group_No,string _Soteria,string _Customer_ID,string _Carousel_ID,string _Cell_ID)
        {
            BOX_ID = _BOX_ID;
            Batch_No = _Batch_No;
            Group_No = _Group_No;
            Soteria = _Soteria;
            Customer_ID = _Customer_ID;
            Carousel_ID = _Carousel_ID;
            Cell_ID = _Cell_ID;
        }
        public StorageModel(string _Batch_No1, string _Batch_No2, string _Carousel_ID1, string _Carousel_ID2, string _Soteria1, string _Soteria2, string _Customer_ID1, string _Customer_ID2)
        {
            Batch_No1 = _Batch_No1;
            Batch_No2 = _Batch_No2;
            Carousel_ID1 = _Carousel_ID1;
            Carousel_ID2 = _Carousel_ID2;
            Soteria1 = _Soteria1;
            Soteria2 = _Soteria2;
            Customer_ID1 = _Customer_ID1;
            Customer_ID2 = _Customer_ID2;
        }
        public List<string> Storage_string()
        {
            var S = new ASEWEB.Models.SQLContext();
            return S.Storage_Context(Batch_No1, Batch_No2, Carousel_ID1, Carousel_ID2, Soteria1, Soteria2, Customer_ID1, Customer_ID2);
        }
    }
}