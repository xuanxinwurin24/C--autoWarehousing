using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using ASEWEB.Models;
namespace ASEWEB.Models
{
    public class Cell_DetailModel
    {
        public string Carousel_ID { get; set; }
        public string Cell_ID { get; set; }
        public string BOX_ID { get; set; }
        public List<string> Detail { get; set; }
        public Cell_DetailModel()
        {
            Carousel_ID = string.Empty;
            Cell_ID = string.Empty;
            BOX_ID = string.Empty;
        }
        public Cell_DetailModel(string rCSID)
        {
            Carousel_ID = rCSID;
        }
        public Cell_DetailModel(string rCSID,string rCEID)
        {
            Carousel_ID = string.Empty;
            Cell_ID = string.Empty;
            BOX_ID = string.Empty;
            Carousel_ID = rCSID;
            Cell_ID = rCEID;
            var C = new ASEWEB.Models.SQLContext();
            Detail = C.CELL_Detail_string(Carousel_ID, Cell_ID);
        }
        
    }
}