using CIM.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using System.Windows.Threading;

namespace CIM.UILog
{
	/// <summary>
	/// UserControl1.xaml 的互動邏輯
	/// </summary>
	public partial class frmTaskLog : UserControl
	{
		ObservableCollection<TaskViewItem> TaskViewList = new ObservableCollection<TaskViewItem>();
		DispatcherTimer Timer1 = new DispatcherTimer(DispatcherPriority.Send);
		public DataTable dt_Carousel_Trans;
		public frmTaskLog()
		{
			InitializeComponent();

			DataGrid_TaskList.ItemsSource = TaskViewList;
			dt_Carousel_Trans = App.Local_SQLServer.SelectDB("*", "[CAROUSEL_TRANSFER]", "");

			Timer1.Tick += new EventHandler(Timer1_Tick);
			Timer1.Interval = new TimeSpan(0, 0, 0, 5, 0);
			Timer1.Start();
		}
		private void Timer1_Tick(object sender, EventArgs e)
		{
			Timer1.Stop();
			string sql = $"SELECT * FROM [TASK_HISTORY] ORDER BY END_TIME DESC";
			DataTable Result=new DataTable();
			App.Local_SQLServer.Query(sql,ref Result);
			string sDirection = string.Empty;
			foreach (DataRow dr in Result.Rows)
			{
				try
				{
					TaskViewItem OriTask = TaskViewList.FirstOrDefault(x => x.COMMANDID == dr["COMMAND_ID"].ToString().Trim());
					if (OriTask != null)
					{
						OriTask.STATUS = dr["STATUS"].ToString().Trim();
					}
					else
					{
						string sSrcPos = dr["SRC_POS"].ToString().Trim();
						string sSrcCell = dr["SRC_CELL_ID"].ToString().Trim();
						string sTarPos = dr["TAR_POS"].ToString().Trim();
						string sTarCell = dr["TAR_CELL_ID"].ToString().Trim();
						Transfer_CarouselCell_to_ViewID(ref sSrcPos, ref sSrcCell);
						Transfer_CarouselCell_to_ViewID(ref sTarPos, ref sTarCell);

						string sSoteria = dr["SOTERIA"].ToString().Trim() == "S" ? "機密" : "非機密";
						switch (dr["DIRECTION"].ToString().Trim())
						{
							case "IN": sDirection = "入庫"; break;
							case "OUT": sDirection = "出庫"; break;
							case "MOVE": sDirection = "調儲"; break;
						}
						string total = dr["START_TIME"].ToString().Trim();
						string year = total.Substring(0, 4);
						string month = total.Substring(4, 2);
						string date = total.Substring(6, 2);
						string hour = total.Substring(8, 2);
						string minute = total.Substring(10, 2);
						string second = total.Substring(12, 2);
						string time = hour + ":" + minute + ":" + second;
						total = year+"/"+month+"/"+date+" " + time;
						TaskViewItem task = new TaskViewItem
						{
							SRCPOS = sSrcPos,
							SRCCELL = sSrcCell,
							TARPOS = sTarPos,
							TARCELL = sTarCell,
							BOXID = dr["BOX_ID"].ToString().Trim(),
							BATCH_NO = dr["BATCH_NO"].ToString().Trim(),
							STATUS = dr["STATUS"].ToString().Trim(),
							DIRECTION = sDirection,
							PRIORITY = dr["PRIORITY"].ToString().Trim().ToIntDef(0),
							COMMANDID = dr["COMMAND_ID"].ToString().Trim(),
							STARTTIME = total,
							SOTERIA = sSoteria,
							CUSTOMER_ID = dr["CUSTOMER_ID"].ToString().Trim(),
						};
						TaskViewList.Add(task);
					}
				}
				catch { };
			}
			for (int i = TaskViewList.Count - 1; i >= 0; i--)
			{
				if (Result.Rows.Count == 0)
				{
					TaskViewList.RemoveAt(i);
				}
				else if (Result.Select($"COMMAND_ID = '{TaskViewList[i].COMMANDID}'").Length == 0)
				{
					TaskViewList.RemoveAt(i);
				}
			}
			Timer1.Start();
		}
		public void Transfer_CarouselCell_to_ViewID(ref string sCarouselID_, ref string sCellID_) //丟進來原本的名稱，輸出搜尋到的對應名稱
		{
			string sCondition = $"CAROUSEL_ID = '{sCarouselID_}'";
			if (!string.IsNullOrEmpty(sCellID_))
			{
				sCondition += $" AND CELL_ID = '{sCellID_}'";
			}
			DataRow[] dr = dt_Carousel_Trans.Select(sCondition);
			if (dr.Length > 0)
			{
				sCarouselID_ = dr[0]["SHOW_CAROUSEL_ID"].ToString().Trim();
				if (!string.IsNullOrEmpty(sCellID_))
					sCellID_ = dr[0]["SHOW_CELL_ID"].ToString().Trim();
			}
		}
	}
}
