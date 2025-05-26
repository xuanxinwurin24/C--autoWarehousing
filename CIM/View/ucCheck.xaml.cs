using CIM.Lib.Model;
using CIM.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace CIM.View
{
	/// <summary>
	/// ucCheck.xaml 的互動邏輯
	/// </summary>
	public partial class ucCheck : UserControl
	{
		List<CHECK_HISTORY> check_historys = new List<CHECK_HISTORY>();
		List<CHECK_HISTORY_DETAIL> check_details = new List<CHECK_HISTORY_DETAIL>();
		enum eWeek { Monday = 1, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday }
		public ucCheck()
		{
			InitializeComponent();

			for (int i = 0; i < 24; i++)
				cbox_Hour.Items.Add(i);
			for (int i = 0; i < 60; i++)
				cbox_Minute.Items.Add(i);
			for (int i = 1; i <= 7; i++)
			{
				ComboBoxItem it = new ComboBoxItem();
				it.SetResourceReference(ComboBoxItem.ContentProperty, $"lang_{(eWeek)i}");
				cbox_Week.Items.Add(it);
			}
			for (int i = 1; i <= 31; i++)
				cbox_Day.Items.Add(i);

			HistoryCheckDataGrid.ItemsSource = check_historys;
			CheckResultDataGrid.ItemsSource = check_details;
		}

		private void btn_ManualCheck_Click(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("確定進行手動盤點 ?", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
			{
				return;
			}
			string cmdid = string.Format("Manual_Check_{0}", DateTime.Now.ToString("yyyyMMddHHmmss"));
			App.Bc.STK.C020_CMD(cmdid, "START");
		}

		private void btn_CheckHistoryListRefresh_Click(object sender, RoutedEventArgs e)
		{
			check_historys.Clear();
			DataTable Result = new DataTable();
			string sql = $"SELECT [COMMAND_ID],[RESULT],[START_TIME],[END_TIME] FROM [CAROUSEL_CHECK_HISTORY]";
			App.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string cmdid = dr["COMMAND_ID"].ToString().Trim();
				string result = dr["RESULT"].ToString().Trim();
				DateTime starttime, endtime;
				DateTime.TryParse(dr["START_TIME"].ToString().Trim(), out starttime);
				DateTime.TryParse(dr["END_TIME"].ToString().Trim(), out endtime);

				check_historys.Add(new CHECK_HISTORY
				{
					Command_ID = cmdid,
					Result = result,
					Start_Time = starttime,
					End_Time = endtime
				});
			}
			HistoryCheckDataGrid_Refresh();
		}
		public virtual void HistoryCheckDataGrid_Refresh()
		{
			try
			{
				ICollectionView view = CollectionViewSource.GetDefaultView(check_historys);
				if (view != null)
				{ view.Refresh(); }
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void HistoryCheckDataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			try
			{
				DataGridRow row = (DataGridRow)sender;
				CHECK_HISTORY checkresult = (CHECK_HISTORY)row.DataContext;

				check_details.Clear();
				DataTable Result = new DataTable();
				string sql = $"SELECT [CAROUSEL_ID],[CELL_ID],[BATCH_NO],[BOX_ID],[BATCH_NO],[SOTERIA],[CUSTOMER_ID],[CHECK_RESULT] " +
					$"FROM [CAROUSEL_CHECK_HISTORY_DETAIL] WHERE [COMMAND_ID] = '{checkresult.Command_ID}' ";
				App.Local_SQLServer.Query(sql, ref Result);
				foreach (DataRow dr in Result.Rows)
				{
					string carousel_id = dr["CAROUSEL_ID"].ToString().Trim();
					string cell_id = dr["CELL_ID"].ToString().Trim();
					string batch_no = dr["BATCH_NO"].ToString().Trim();
					string box_id = dr["BOX_ID"].ToString().Trim();
					string group_no = dr["GROUP_NO"].ToString().Trim();
					string soteria = dr["SOTERIA"].ToString().Trim();
					string customer_id = dr["CUSTOMER_ID"].ToString().Trim();
					int check_result;
					int.TryParse(dr["CHECK_RESULT"].ToString(), out check_result);
					CHECK_HISTORY_DETAIL check_detail = new CHECK_HISTORY_DETAIL
					{
						CAROUSEL_ID = carousel_id,
						CELL_ID = cell_id,
						BATCH_NO = batch_no,
						BOX_ID = box_id,
						GROUP_NO = group_no,
						SOTERIA = soteria,
						CUSTOMER_ID = customer_id,
						CHECK_RESULT = check_result
					};
					check_details.Add(check_detail);
				}
				CheckResultDataGrid_Refresh();
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
		public virtual void CheckResultDataGrid_Refresh()
		{
			try
			{
				ICollectionView view = CollectionViewSource.GetDefaultView(check_details);
				if (view != null)
				{ view.Refresh(); }
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
	}
}
