using CIM.BC;
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
using System.Windows.Threading;

namespace CIM.View
{
	/// <summary>
	/// ucCarousel.xaml 的互動邏輯
	/// </summary>
	public partial class fmCarousel : CustomWindow
	{
		public List<CELL_STATUS> cells = new List<CELL_STATUS>();
		public cCarousel Current_Carousel = null;
		DispatcherTimer Timer1 = new System.Windows.Threading.DispatcherTimer();
		public fmCarousel(cCarousel carousel)
		{
			InitializeComponent();
#if DEBUG
			if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
#endif
			if (carousel == null) return;
			Current_Carousel = carousel;
			DataTable Result = new DataTable();
			string sCarouselID = Current_Carousel.Carousel_ID, scellid = string.Empty;
			App.Bc.Transferto_ViewID_CarouselCell(ref sCarouselID, ref scellid);

			string sql = $"SELECT * FROM [CELL_STATUS] WHERE [CAROUSEL_ID] = '{sCarouselID}' ORDER BY [CELL_ID] ASC";
			App.STK_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string carousel_id = dr["CAROUSEL_ID"].ToString().Trim();
				string cell_id = dr["CELL_ID"].ToString().Trim();
				App.Bc.Transfer_CarouselCell_to_ViewID(ref carousel_id, ref cell_id); //轉換成SHOW的ID
				string batch_no = dr["BATCH_NO"].ToString().Trim();
				string box_id = dr["BOX_ID"].ToString().Trim();
				string group_no = dr["GROUP_NO"].ToString().Trim();
				string soteria = dr["SOTERIA"].ToString().Trim();
				string customer_id = dr["CUSTOMER_ID"].ToString().Trim();
				DateTime store_time;
				DateTime.TryParse(dr["STORED_TIME"].ToString(), out store_time);
				int status, check_result;
				int.TryParse(dr["STATUS"].ToString(), out status);
				string sRealCarouselID = carousel_id;
				string sRealCellId = cell_id;
				App.Bc.Transferto_ViewID_CarouselCell(ref sRealCarouselID, ref sRealCellId);
				DataTable dt_FindTaskTarget = App.Local_SQLServer.SelectDB("*", "[TASK]", $"[TAR_POS] = '{sRealCarouselID}' AND [TAR_CELL_ID] = '{sRealCellId}'");
				if (dt_FindTaskTarget.Rows.Count != 0) //這個Cell 已經有任務指定目標了
				{
					status = 5;
				}
				int.TryParse(dr["CHECK_RESULT"].ToString(), out check_result);
				CELL_STATUS cell = new CELL_STATUS
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
				cells.Add(cell);
				//wrapCell
				StackPanel cellstackpanel = new StackPanel();
				cellstackpanel.Width = 120;
				cellstackpanel.Margin = new Thickness(10, 0, 10, 5);

				Label lb = new Label();
				lb.Content = cell_id;
				lb.FontSize = 16;
				lb.FontWeight = FontWeights.Bold;
				lb.Margin = new Thickness(0);
				lb.Padding = new Thickness(0);
				lb.HorizontalContentAlignment = HorizontalAlignment.Center;
				cellstackpanel.Children.Add(lb);
				Button btn = new Button();
				btn.DataContext = cell;
				btn.Margin = new Thickness(0);
				btn.Height = 30;
				btn.FontSize = 14;
				btn.BorderThickness = new Thickness(1);
				Binding boxid_binding = new Binding("BOX_ID");
				btn.SetBinding(Button.ContentProperty, boxid_binding);
				cellstackpanel.Children.Add(btn);
				wrapCell.Children.Add(cellstackpanel);
			}

			Timer1.Tick += new EventHandler(Timer1_Tick);
			Timer1.Interval = new TimeSpan(0, 0, 0, 0, 500);
			Timer1.Start();
		}

		private void Timer1_Tick(object sender, EventArgs e)
		{
			Timer1.Stop();
			try
			{
				if (Current_Carousel == null) return;
				DataTable Result = new DataTable();
				string sql = $"SELECT * FROM [CELL_STATUS] WHERE [CAROUSEL_ID] = '{Current_Carousel.Carousel_ID}' ORDER BY [CELL_ID] ASC";
				App.STK_SQLServer.Query(sql, ref Result);
				foreach (DataRow dr in Result.Rows)
				{
					string carousel_id = dr["CAROUSEL_ID"].ToString().Trim();
					string cell_id = dr["CELL_ID"].ToString().Trim();
					App.Bc.Transfer_CarouselCell_to_ViewID(ref carousel_id, ref cell_id); //轉換成SHOW的ID
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

					string sRealCarouselID = carousel_id;
					string sRealCellId = cell_id;
					App.Bc.Transferto_ViewID_CarouselCell(ref sRealCarouselID, ref sRealCellId);
					DataTable dt_FindTaskTarget = App.Local_SQLServer.SelectDB("*", "[TASK]", $"[TAR_POS] = '{sRealCarouselID}' AND [TAR_CELL_ID] = '{sRealCellId}'");
					if (dt_FindTaskTarget.Rows.Count != 0) //這個Cell 已經有任務指定目標了
					{
						status = 5;
					}
					CELL_STATUS curncell = cells.Find(x => x.CAROUSEL_ID == carousel_id && x.CELL_ID == cell_id);
					if (curncell != null)
					{
						curncell.STATUS = status;
						curncell.BATCH_NO = batch_no;
						curncell.BOX_ID = box_id;
						curncell.GROUP_NO = group_no;
						curncell.SOTERIA = soteria;
						curncell.CUSTOMER_ID = customer_id;
						curncell.STORED_TIME = store_time;
						curncell.CHECK_RESULT = check_result;
					}
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
			Timer1.Start();
		}
		

		private void Cell_info_button_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Button btn = (Button)sender;
				if (btn.DataContext.GetType().Name != "CELL_STATUS") return;
				fmCellInformation vw = new fmCellInformation((CELL_STATUS)btn.DataContext);
				vw.Owner = Window.GetWindow(this);
				vw.ShowDialog();
				vw = null;
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

        private void CustomWindow_Closed(object sender, EventArgs e)
        {
			if (Timer1 != null)
			{
				Timer1.Stop();
				Timer1 = null;
			}
			if (cells != null)
            {
				cells.Clear();
				cells = null;
			}
			if (Current_Carousel != null)
				Current_Carousel = null;
			GC.Collect();
		}
    }
}
