using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using ASEWEB.Models;
namespace ASEWEB.Models
{
	public class Carousel_UtilityModel
	{
		public string Carousel_ID{ get; set; }
		public string Temp_UPPER { get; set; }
		public string Temp_LOWER { get; set; }
		public string Hum_UPPER { get; set; }
		public string Hum_LOWER { get; set; }
		public string TURN_ON_N2 { get; set; }
		public string TURN_OFF_N2 { get; set; }
		public string CELL_ID_Front { get; set; }
		public string Show_Carousel_ID { get; set; }
		
		public Carousel_UtilityModel(string rCSID)
		{
			Carousel_ID = string.Empty;
			Temp_UPPER = string.Empty;
			Temp_LOWER = string.Empty;
			Hum_UPPER = string.Empty;
			Hum_LOWER = string.Empty;
			TURN_ON_N2 = string.Empty;
			TURN_OFF_N2 = string.Empty;
			CELL_ID_Front = string.Empty;
			Carousel_ID = rCSID;
			var C = new ASEWEB.Models.SQLContext();
			List<string> CU_String = C.CU_string(rCSID);
            Carousel_ID = CU_String[0].Trim();
            //CELL_ID_Front = CU_String[0].Trim();
            Temp_UPPER = CU_String[1].Trim();
			Temp_LOWER = CU_String[2].Trim();
			Hum_UPPER = CU_String[3].Trim();
			Hum_LOWER = CU_String[4].Trim();
			TURN_ON_N2 = CU_String[5].Trim();
			TURN_OFF_N2 = CU_String[6].Trim();
			Show_Carousel_ID = C.Show_CSID_Transfer(rCSID);
		}
	}
}
