using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.ViewModel
{
	public class CHECK_HISTORY
	{
		public string Command_ID { get; set; }
		public string Result { get; set; }
		public DateTime Start_Time { get; set; }
		public DateTime End_Time { get; set; }
	}
	public class CHECK_HISTORY_DETAIL : CELL_STATUS
	{
		public string Command_ID { get; set; }
	}
}
