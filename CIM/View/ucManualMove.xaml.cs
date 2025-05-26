using CIM.Lib.Model;
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

namespace CIM.View
{
	/// <summary>
	/// ucManualMove.xaml 的互動邏輯
	/// </summary>
	public partial class ucManualMove : UserControl
	{
		ObservableCollection<Cell_Move> ManualMoveList = new ObservableCollection<Cell_Move>();
		public ucManualMove()
		{
			InitializeComponent();

			DataGrid_ManualMoveList.ItemsSource = ManualMoveList;
			cbox_Manual_Target_Carousel.ItemsSource = App.Bc.Carousels;
		}

		private void btn_SearchMoveCell_Click(object sender, RoutedEventArgs e)
		{
			string sql = string.Empty;
			string sqlCondition = string.Empty;
			try
			{
				ManualMoveList.Clear();
				if (txb_BatchNo.IsEnabled)
				{
					if (string.IsNullOrEmpty(sqlCondition))
						sqlCondition += " WHERE ";
					else
						sqlCondition += " AND ";
					sqlCondition += $"[BATCH_NO] = '{txb_BatchNo.Text.Trim()}'";
				}
				if (txb_CarouselID.IsEnabled)
				{
					if (string.IsNullOrEmpty(sqlCondition))
						sqlCondition += " WHERE ";
					else
						sqlCondition += " AND ";
					sqlCondition += $"[CAROUSEL_ID] = '{txb_CarouselID.Text.Trim()}'";
				}
				if (txb_Soteria.IsEnabled)
				{
					if (string.IsNullOrEmpty(sqlCondition))
						sqlCondition += " WHERE ";
					else
						sqlCondition += " AND ";
					sqlCondition += $"[SOTERIA] = '{txb_Soteria.Text.Trim()}'";
				}
				if (txb_CustomerID.IsEnabled)
				{
					if (string.IsNullOrEmpty(sqlCondition))
						sqlCondition += " WHERE ";
					else
						sqlCondition += " AND ";
					sqlCondition += $"[CUSTOMER_ID] = '{txb_CustomerID.Text.Trim()}'";
				}

				DataTable Result = new DataTable();
				sql = $"SELECT * FROM [CELL_STATUS] {sqlCondition}";
				App.STK_SQLServer.Query(sql, ref Result);
				foreach (DataRow dr in Result.Rows)
				{
					string carousel_id = dr["CAROUSEL_ID"].ToString().Trim();
					string cell_id = dr["CELL_ID"].ToString().Trim();
					string batch_no = dr["BATCH_NO"].ToString().Trim();
					string box_id = dr["BOX_ID"].ToString().Trim();
					string group_no = dr["BATCH_NO"].ToString().Trim();
					string soteria = dr["SOTERIA"].ToString().Trim();
					string customer_id = dr["CUSTOMER_ID"].ToString().Trim();
					DateTime store_time;
					DateTime.TryParse(dr["STORED_TIME"].ToString(), out store_time);
					int status, check_result;
					int.TryParse(dr["STATUS"].ToString(), out status);
					int.TryParse(dr["CHECK_RESULT"].ToString(), out check_result);

					if (string.IsNullOrEmpty(box_id) && string.IsNullOrEmpty(batch_no) && string.IsNullOrEmpty(group_no) && string.IsNullOrEmpty(soteria) && string.IsNullOrEmpty(customer_id))
						continue;
					Cell_Move cell = new Cell_Move
					{
						CAROUSEL_ID = carousel_id,
						CELL_ID = cell_id,
						STATUS = status,
						BATCH_NO = batch_no,
						BOX_ID = box_id,
						GROUP_NO = group_no,
						SOTERIA = soteria,
						CUSTOMER_ID = customer_id,
						STORED_TIME = store_time,
						CHECK_RESULT = check_result
					};
					ManualMoveList.Add(cell);
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void btn_Manual_Move_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
