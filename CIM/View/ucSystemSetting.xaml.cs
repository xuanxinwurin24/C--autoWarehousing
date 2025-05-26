using CIM.BC;
using CIM.Lib.Model;
using Strong;
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
	/// ucSystemSetting.xaml 的互動邏輯
	/// </summary>
	public partial class ucSystemSetting : UserControl
	{
		WebServiceSetting webSetting;
		public List<CarouselMonitor> CarouselUtilitys = new List<CarouselMonitor>();
		public ucSystemSetting()
		{
			InitializeComponent();
#if DEBUG
			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
#endif
			webSetting = Common.DeserializeXMLFileToObject<WebServiceSetting>(App.sSysDir + "\\Ini\\WebService.xml");

			UserChanged(Password.CurnUser.Group.Level);
			Password.LogInOutEvent += LogInOutEventCallBackFunc;

			tb_LogOutTimer.Text = (App.SysPara.LogOutTime / 60 / 1000).ToString();
			tb_StockInWaitTime.Text = (App.SysPara.StockInWaitTime / 60 / 1000).ToString();
			tb_UsageReportTimer.Text = (App.SysPara.UsageReportTime / 60 / 1000).ToString();

			#region Carousel Utility Initial
			txtbox_MonitorPeriod.Text = App.SysPara.Carousel_Utility_Monitoring_Period.ToString();
			DataTable Result = new DataTable();
			string sql = $"SELECT [CAROUSEL_ID] , [TEMPERATER_UPPER_LIMIT] , [TEMPERATER_LOWER_LIMIT] , [HUMIDITY_UPPER_LIMIT] , [HUMIDITY_LOWER_LIMIT] " +
				$", [TURN_ON_N2_HUMIDITY],[TURN_OFF_N2_HUMIDITY] FROM [MONITOR_SETTING]";
			App.STK_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string carouselid = dr["CAROUSEL_ID"].ToString().Trim();
				double temp_up_lim, temp_low_lim, humi_up_lim, humi_low_lim, turn_on_n2_humi, turn_off_n2_humi;
				double.TryParse(dr["TEMPERATER_UPPER_LIMIT"].ToString(), out temp_up_lim);
				double.TryParse(dr["TEMPERATER_LOWER_LIMIT"].ToString(), out temp_low_lim);
				double.TryParse(dr["HUMIDITY_UPPER_LIMIT"].ToString(), out humi_up_lim);
				double.TryParse(dr["HUMIDITY_LOWER_LIMIT"].ToString(), out humi_low_lim);
				double.TryParse(dr["TURN_ON_N2_HUMIDITY"].ToString(), out turn_on_n2_humi);
				double.TryParse(dr["TURN_OFF_N2_HUMIDITY"].ToString(), out turn_off_n2_humi);

				CarouselUtilitys.Add(new CarouselMonitor
				{
					CAROUSEL_ID = carouselid,
					TEMPERATURE_UPPER_LIMIT = temp_up_lim,
					TEMPERATURE_LOWER_LIMIT = temp_low_lim,
					HUMIDITY_UPPER_LIMIT = humi_up_lim,
					HUMIDITY_LOWER_LIMIT = humi_low_lim,
					TURN_ON_N2_HUMIDITY = turn_on_n2_humi,
					TURN_OFF_N2_HUMIDITY = turn_off_n2_humi

				});
			}
			EqUtilityDataGrid.ItemsSource = CarouselUtilitys;
			#endregion Carousel Utility Initial
		}

		void LogInOutEventCallBackFunc(string sOldUserName_, string sNewUserName_)
		{
			Application.Current.Dispatcher.BeginInvoke(new Action(() =>
			{
				MainWindow parentWindow = (MainWindow)Window.GetWindow(this);
				UserChanged(Password.CurnUser.Group.Level);
			}));
		}

		public void UserChanged(int iLevel_)
		{
			try
			{
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void btn_ApplyWebSetting_Click(object sender, RoutedEventArgs e)
		{
			Common.SerializeXMLObjToFile<WebServiceSetting>(App.sSysDir + "\\Ini\\WebService.xml", webSetting);
			App.WS_Setting = Common.DeserializeXMLFileToObject<WebServiceSetting>(App.sSysDir + "\\Ini\\WebService.xml");
			UpdateMMI();
		}

		private void btn_CancelWebSetting_Click(object sender, RoutedEventArgs e)
		{
			webSetting = Common.DeserializeXMLFileToObject<WebServiceSetting>(App.sSysDir + "\\Ini\\WebService.xml");
			UpdateMMI();

		}
		void UpdateMMI()
		{
		}

		private void btn_SaveTimer_Click(object sender, RoutedEventArgs e)
		{
			int logoutT = -1, stockInWT = -1, UsageWT = -1;
			if (int.TryParse(tb_LogOutTimer.Text, out logoutT) == false)
			{
				MessageBox.Show("格式錯誤");
				return;
			}
			if (int.TryParse(tb_StockInWaitTime.Text, out stockInWT) == false)
			{
				MessageBox.Show("格式錯誤");
				return;
			}
			if (int.TryParse(tb_UsageReportTimer.Text, out UsageWT) == false)
			{
				MessageBox.Show("格式錯誤");
				return;
			}
			App.SysPara.LogOutTime = logoutT * 60 * 1000;
			App.SysPara.StockInWaitTime = stockInWT * 60 * 1000;
			App.SysPara.UsageReportTime = UsageWT * 60 * 1000;

			Common.SerializeXMLObjToFile<SysPara>(SysPara.FileName, App.SysPara);

			App.Bc.TimerChange();
		}

		private void btn_Default_Click(object sender, RoutedEventArgs e)
		{
			App.SysPara.LogOutTime = 10 * 60 * 1000;
			App.SysPara.StockInWaitTime = 15 * 60 * 1000;
			App.SysPara.UsageReportTime = 30 * 60 * 1000;

			Common.SerializeXMLObjToFile<SysPara>(SysPara.FileName, App.SysPara);

			App.Bc.TimerChange();
		}

		private void btn_Cancel_Click(object sender, RoutedEventArgs e)
		{
			tb_LogOutTimer.Text = (App.SysPara.LogOutTime / 60 / 1000).ToString();
			tb_StockInWaitTime.Text = (App.SysPara.StockInWaitTime / 60 / 1000).ToString();
			tb_UsageReportTimer.Text = (App.SysPara.UsageReportTime / 60 / 1000).ToString();
		}

		private void btn_SetUtility_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (MessageBox.Show("確認是否設定為新的監控數值 ?", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
				{
					return;
				}
				int imonitor_period;
				int.TryParse(txtbox_MonitorPeriod.Text, out imonitor_period);
				App.SysPara.Carousel_Utility_Monitoring_Period = imonitor_period;
				App.SysPara.SaveSysParaToFile();

				foreach (CarouselMonitor carousel in CarouselUtilitys)
				{
					string sql = $"update [MONITOR_SETTING] set TEMPERATER_UPPER_LIMIT = {carousel.TEMPERATURE_UPPER_LIMIT} " +
						$", TEMPERATER_LOWER_LIMIT = {carousel.TEMPERATURE_LOWER_LIMIT} " +
						$", HUMIDITY_UPPER_LIMIT = {carousel.HUMIDITY_UPPER_LIMIT} " +
						$", HUMIDITY_LOWER_LIMIT = {carousel.HUMIDITY_LOWER_LIMIT} " +
						$", TURN_ON_N2_HUMIDITY = {carousel.TURN_ON_N2_HUMIDITY} " +
						$", TURN_OFF_N2_HUMIDITY = {carousel.TURN_OFF_N2_HUMIDITY} " +
						$" where CAROUSEL_ID = '{carousel.CAROUSEL_ID}' ";
					App.STK_SQLServer.NonQuery(sql);
				}
				App.Bc.STK.C050_CMD((uint)imonitor_period);
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void btn_CancelUtility_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (MessageBox.Show("是否取消設定，回復原先設定值 ?", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
				{
					return;
				}
				DataTable Result = new DataTable();
				string sql = $"SELECT [CAROUSEL_ID] , [TEMPERATER_UPPER_LIMIT] , [TEMPERATER_LOWER_LIMIT] , [HUMIDITY_UPPER_LIMIT] , [HUMIDITY_LOWER_LIMIT] " +
					$", [TURN_ON_N2_HUMIDITY],[TURN_OFF_N2_HUMIDITY] FROM [MONITOR_SETTING]";
				App.STK_SQLServer.Query(sql, ref Result);
				foreach (DataRow dr in Result.Rows)
				{
					string carouselid = dr["CAROUSEL_ID"].ToString().Trim();
					double temp_up_lim, temp_low_lim, humi_up_lim, humi_low_lim, turn_on_n2_humi, turn_off_n2_humi;
					double.TryParse(dr["TEMPERATER_UPPER_LIMIT"].ToString(), out temp_up_lim);
					double.TryParse(dr["TEMPERATER_LOWER_LIMIT"].ToString(), out temp_low_lim);
					double.TryParse(dr["HUMIDITY_UPPER_LIMIT"].ToString(), out humi_up_lim);
					double.TryParse(dr["HUMIDITY_LOWER_LIMIT"].ToString(), out humi_low_lim);
					double.TryParse(dr["TURN_ON_N2_HUMIDITY"].ToString(), out turn_on_n2_humi);
					double.TryParse(dr["TURN_OFF_N2_HUMIDITY"].ToString(), out turn_off_n2_humi);

					CarouselMonitor carousel = CarouselUtilitys.Find(x => x.CAROUSEL_ID == carouselid);

					if (carousel != null)
					{
						carousel.TEMPERATURE_UPPER_LIMIT = temp_up_lim;
						carousel.TEMPERATURE_LOWER_LIMIT = temp_low_lim;
						carousel.HUMIDITY_UPPER_LIMIT = humi_up_lim;
						carousel.HUMIDITY_LOWER_LIMIT = humi_low_lim;
						carousel.TURN_ON_N2_HUMIDITY = turn_on_n2_humi;
						carousel.TURN_OFF_N2_HUMIDITY = turn_off_n2_humi;

					}
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
	}
}
