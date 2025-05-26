using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using ASEWEB.Models;
namespace ASEWEB.Models
{
    public class CellModel
    {
        public string Carousel_ID { get; set; }
        public string Carousel_Show_ID { get; set; }
        public string Cell_ID { get; set; }
        public string Cell_Show_ID { get; set; }
        public string BOX_ID { get; set; }
        public string Status { get; set; }
        public string Store_Status { get; set; }
        public List<string> Detail { get; set; }
        public CellModel()
        {
            Carousel_ID = string.Empty;
            Cell_ID = string.Empty;
            BOX_ID = string.Empty;
            Status = string.Empty;
            Carousel_Show_ID = string.Empty;
            Cell_Show_ID = string.Empty;
        }
        public CellModel(string rCSID)
        {
            Carousel_ID = rCSID;
        }
        public CellModel(string rCSID,string rCSID_Show,string rCEID,string rCEID_Show)
        {
            Carousel_ID = rCSID;
            Carousel_Show_ID = rCSID_Show;
            Cell_ID = rCEID;
            Cell_Show_ID = rCEID_Show;
            var C = new ASEWEB.Models.SQLContext();
            
            BOX_ID = C.Cell_string(rCSID,rCEID)[0].Trim();
            Status = C.Cell_string(rCSID, rCEID)[1].Trim();
        }
        
    }
}