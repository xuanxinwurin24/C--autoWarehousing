using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	/// ucAlarm.xaml 的互動邏輯
	/// </summary>
	public partial class ucAlarm : UserControl
	{
		public const int MaxLogRowCunt = 1000;

		public LogWriter LogFile;
		ObservableCollection<AlarmBody> MsgLst = new ObservableCollection<AlarmBody>();
		protected object _syncLock = new object();

		public ucAlarm()
		{
			InitializeComponent();
			LogFile = new LogWriter(Environment.CurrentDirectory + @"\LogFile", GetType().Name, 50000)
			{
				PathName = Environment.CurrentDirectory + @"\LogFile\UI Log",
				sHead = "Alarm",
				MaxSize = 500 * 1000
			};
			MsgLst.Clear();
			BindingOperations.EnableCollectionSynchronization(MsgLst, _syncLock);
			lv.ItemsSource = MsgLst;
			App.Alarm.AlarmEvent += new Alarm.AlarmEventHandler(AlarmEventCallBack);
		}

		void AlarmEventCallBack(AlarmBody AlmObj_, bool bFinal_)
		{
			Write2Log(AlmObj_);
			UpdateList(AlmObj_);
			if (AlmObj_.bOccured == true)
			{
				//Application.Current.Dispatcher.Invoke(new Action(() =>
				//{
				//    ShowTop(MainWindow.ucUILog.tbAlarm);
				//}));

				//20190102 mark
				//Application.Current.Dispatcher.BeginInvoke(new Action(() =>
				//{
				//    ShowTop(MainWindow.ucUILog.tbAlarm);
				//}));
			}
		}

		void UpdateMMI()
		{
			try
			{
				ICollectionView view = CollectionViewSource.GetDefaultView(MsgLst);
				if (view != null)
				{
					view.Refresh();
				}

				//MainWindow.ucUILog.Visibility = Visibility.Visible;
				//if (MainWindow.ucUILog.WindowState == WindowState.Minimized)
				//{ MainWindow.ucUILog.WindowState = WindowState.Normal; }
				//MainWindow.ucUILog.TabCtrl.SelectedItem = MainWindow.ucUILog.tbAlarm;
			}
			catch (Exception e_) { Lib.Model.LogExcept.LogException(e_); }
		}

		private void UpdateList(AlarmBody AlmObj_)
		{
			lock (_syncLock)
			{
				try
				{
					if (AlmObj_.bOccured == true)
					{
						MsgLst.Insert(0, AlmObj_);
					}
					else
					{
						MsgLst.Remove(AlmObj_);
					}
					if (MsgLst.Count > MaxLogRowCunt)
					{
						MsgLst.RemoveAt(MsgLst.Count - 1);
					}
				}
				catch (Exception e_) { Lib.Model.LogExcept.LogException(e_); }
			}
		}

		int SortFunc(AlarmBody alm0_, AlarmBody alm1_)
		{
			if (alm0_.OccurTime > alm1_.OccurTime)
			{
				return -1;
			}
			if (alm0_.OccurTime < alm1_.OccurTime)
			{
				return 1;
			}
			return 0;
		}

		void Write2Log(AlarmBody almObj_)
		{
			if (almObj_.bOccured == false) return;

			//string uname = App.alarm.GetUnitName(almObj_.Unit);
			string Str = string.Format("{0},{1},{2},{3},{3:X4},{4}", almObj_.OccurTimeStr, almObj_.Unit, App.Alarm.GetUnitName(almObj_.Unit), almObj_.ID, almObj_.Message);
			LogFile.AddString(Str, false);
			string sql = $"INSERT INTO [ALARM] VALUES('{almObj_.ID.ToString()}','{App.Alarm.GetUnitName(almObj_.Unit)}','{almObj_.OccurTimeStr}','{almObj_.Message}')";
			App.Local_SQLServer.NonQuery(sql);
		}

		private void BtnResetOneAlarm_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (lv.SelectedIndex != -1)
				{
					AlarmBody body = (AlarmBody)lv.Items[lv.SelectedIndex];
					string sql = $"INSERT INTO [ALARM_HISTORY] VALUES('{body.ID}','{App.Alarm.GetUnitName(body.Unit)}','{body.OccurTimeStr}','{DateTime.Now.ToString("yyyyMMddHHmmssfff")}','{body.Message}')";
					App.Local_SQLServer.NonQuery(sql);
					App.Alarm.Set(body.Unit, body.ID, false);   //App.Alarm.SetOccur(body, false);
					sql = $"DELETE FROM [ALARM] WHERE ID='{body.ID}' AND UNIT_NAME='{App.Alarm.GetUnitName(body.Unit)}'";
					App.Local_SQLServer.NonQuery(sql);
					
				}
			}
			catch (Exception e_) { LogExcept.LogException(e_); }
		}

		private void BtnReset_Click(object sender, RoutedEventArgs e)
		{
			App.Alarm.ResetAllAlarm();
			string sql_temp = "SELECT [ID],[UNIT_NAME],[TIME],[MESSAGE] FROM [ALARM]";
			DataTable dt_temp = new DataTable();
			App.Local_SQLServer.Query(sql_temp,ref dt_temp);
			DateTime dt = DateTime.Now;
			foreach(DataRow dr in dt_temp.Rows)
			{
				string sql = $"INSERT INTO [ALARM_HISTORY] ([ID],[UNIT_NAME],[OCCURED_TIME],[RESET_TIME],[MESSAGE]) VALUES ('{dr["ID"].ToString().Trim()}','{dr["UNIT_NAME"].ToString().Trim()}',{dr["TIME"]},'{dt}','{dr["MESSAGE"].ToString().Trim()}') ";
				App.Local_SQLServer.NonQuery(sql);
			}
			
			
			sql_temp = $"DELETE FROM [ALARM]";
			App.Local_SQLServer.NonQuery(sql_temp);
		}
	}
	public class UnitNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string unitname = string.Empty;
			try
			{
				uint unitno = (uint)value;
				if (App.Alarm != null)
				{
					unitname = App.Alarm.GetUnitName(unitno);
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
			return unitname;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	public class AlarmConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				if (value.ToString() == "en-US" && parameter.ToString() == "EN")
					return 500;
				else if (value.ToString() == "zh-TW" && parameter.ToString() == "zhTW")
					return 500;
				else
					return 0;
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
			return 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
