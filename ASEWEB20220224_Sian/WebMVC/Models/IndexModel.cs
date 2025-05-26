using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using ASEWEB.Models;
namespace ASEWEB.Models
{
    public class IndexModel
    {
        public string Carousel_ID { get; set; }
        public string Carousel_Show_ID { get; set; }
        public string Temperature { get; set; }
        public string Humidity { get; set; }
        public string CELL_ID { get; set; }
        public string Store_Status { get; set; }
        public string All_Cell { get; set; }
        public string Use_Cell { get; set; }
        public IndexModel()
        {

        }
        public IndexModel(string rCSID_Show)
        {
            Carousel_ID = string.Empty;
            Temperature = string.Empty;
            Humidity = string.Empty;
            CELL_ID = string.Empty;
            All_Cell = "-";
            Use_Cell = "-";
            Carousel_Show_ID = rCSID_Show;
            var TH = new ASEWEB.Models.SQLContext();
            Carousel_ID = TH.CSID_Transfer(Carousel_Show_ID);
            List<string> THS = TH.THS_String(Carousel_Show_ID);
            Temperature = THS[0];
            Humidity = THS[1];
            Store_Status = THS[2];
            Use_Cell = THS[3];
            All_Cell = THS[4];
        }
        public List<string> Carousel_Setting()
        {
            var C = new ASEWEB.Models.SQLContext();
            return C.Carousel_setting();
        }
    }
}