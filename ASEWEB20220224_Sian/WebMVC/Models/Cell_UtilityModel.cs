using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using ASEWEB.Models;
namespace ASEWEB.Models
{
	public class Cell_UtilityModel
	{
		public string Carousel_ID{ get; set; }
		public string Cell_ID { get; set; }
		public string State { get; set; }

		public Cell_UtilityModel(string _Carousel_ID, string _Cell_ID, string _State)
		{
			Carousel_ID = string.Empty;
			Cell_ID = string.Empty;
			State = string.Empty;
			Carousel_ID = _Carousel_ID;
			Cell_ID = _Cell_ID;
			State = _State;
		}
	}
}
