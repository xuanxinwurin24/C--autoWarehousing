using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CIM.View
{
	/// <summary>
	/// ucUILog.xaml 的互動邏輯
	/// </summary>
	public partial class ucUILog : UserControl
	{
		public ucUILog()
		{
			InitializeComponent();
			//Path
			SystemLog.LogFile.PathName = Environment.CurrentDirectory + @"\LogFile\UI Log";
			BCLog.LogFile.PathName = Environment.CurrentDirectory + @"\LogFile\UI Log";
			StockerLog.LogFile.PathName = Environment.CurrentDirectory + @"\LogFile\UI Log";
			ShuttleLog.LogFile.PathName = Environment.CurrentDirectory + @"\LogFile\UI Log";
			frmAlarmHistoryLog.LogFile.PathName = Environment.CurrentDirectory + @"\LogFile\UI Log";
			frmWebServiceLog.LogFile.PathName = Environment.CurrentDirectory + @"\LogFile\UI Log";
			//Head
			SystemLog.LogFile.sHead = "System";
			BCLog.LogFile.sHead = "BC";
			StockerLog.LogFile.sHead = "Stocker";
			ShuttleLog.LogFile.sHead = "Shuttle";
			frmAlarmHistoryLog.LogFile.sHead = "Alarm History";
			frmWebServiceLog.LogFile.sHead = "Web Service";
			//Size
			SystemLog.LogFile.MaxSize = 500 * 1000;
			BCLog.LogFile.MaxSize = 500 * 1000;
			StockerLog.LogFile.MaxSize = 500 * 1000;
			ShuttleLog.LogFile.MaxSize = 500 * 1000;
			frmAlarmHistoryLog.LogFile.MaxSize = 500 * 1000;
			frmWebServiceLog.LogFile.MaxSize = 500 * 1000;
		}
	}
}
