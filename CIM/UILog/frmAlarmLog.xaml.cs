using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CIM.UILog
{
	public partial class frmAlarmLog : UserControl
	{
		public int MaxLogRowCunt = 1000;

		public LogWriter LogFile;
		List<AlarmBody> MsgLst = new List<AlarmBody>();

		protected object LockObj = new object();
		protected System.Windows.Threading.DispatcherTimer RefreshTimer;

		public frmAlarmLog()
		{
			LogFile = new LogWriter(Environment.CurrentDirectory + @"\LogFile", GetType().Name, 50000);

			InitializeComponent();
			RefreshTimer = new System.Windows.Threading.DispatcherTimer();
			RefreshTimer.Tick += new EventHandler(RefreshTimer_Tick);
			RefreshTimer.Interval = new TimeSpan(0, 0, 1);

			lv.ItemsSource = MsgLst;
			App.Alarm.AlarmEvent += new Alarm.AlarmEventHandler(AlarmEventCallBack);
		}
		void AlarmEventCallBack(AlarmBody AlmObj_, bool bFinal_)
		{
			Write2Log(AlmObj_);
			RefreshMMI();
			return;
			if (AlmObj_.bOccured == true)
			{
				//Application.Current.Dispatcher.Invoke(new Action(() =>
				//{
				//    ShowTop(MainWindow.Log.tbAlarm);
				//}));
				Application.Current.Dispatcher.BeginInvoke(new Action(() =>
				{
					//ShowTop(MainWindow.Log.tbAlarm);
				}));
			}
		}
		internal void ShowTop(TabItem Page_)
		{
			try
			{
				//MainWindow.Log.TabCtrl.SelectedValue = Page_;
				//MainWindow.Log.Show();
				//MainWindow.Log.Topmost = true;
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
		void RefreshMMI()
		{
			RefreshTimer.Stop();
			RefreshTimer.Start();
		}
		private void RefreshTimer_Tick(object sender, EventArgs e)
		{ UpdateMMI(); }
		void UpdateMMI()
		{
			try
			{
				RefreshTimer.Stop();
				UpdateList();
				ICollectionView view = CollectionViewSource.GetDefaultView(MsgLst);
				if (view != null)
				{ view.Refresh(); }

				//MainWindow.Log.Visibility = Visibility.Visible;
				//if (MainWindow.Log.WindowState == WindowState.Minimized)
				//{ MainWindow.Log.WindowState = WindowState.Normal; }
				//MainWindow.Log.TabCtrl.SelectedItem = MainWindow.Log.tbAlarm;
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
		private void UpdateList()
		{
			lock (LockObj)
			{
				try
				{
					MsgLst.Clear();
					foreach (KeyValuePair<uint, AlarmBody> Obj in App.Alarm.BodyList)
					{
						if (Obj.Value.bOccured == false) continue;
						MsgLst.Add(Obj.Value);
					}
					MsgLst.Sort(SortFunc);
					if (MsgLst.Count > MaxLogRowCunt)
					{
						MsgLst.RemoveRange(MaxLogRowCunt - 20, MsgLst.Count - MaxLogRowCunt + 20);
					}
				}
				catch (Exception ex)
				{
					LogExcept.LogException(ex);
				}
			}
		}
		int SortFunc(AlarmBody alm0_, AlarmBody alm1_)
		{
			if (alm0_.OccurTime > alm1_.OccurTime)
				return -1;
			if (alm0_.OccurTime < alm1_.OccurTime)
				return 1;
			return 0;
		}
		void Write2Log(AlarmBody almObj_)
		{
			if (almObj_.bOccured == false)
			{ return; }
			////string uname = App.alarm.GetUnitName(almObj_.Unit);
			string Str = string.Format("{0}\t{1}\t{2}\t{3}\t{3:X4}\t{4}", almObj_.OccurTimeStr, almObj_.Unit, App.Alarm.GetUnitName(almObj_.Unit), almObj_.ID, almObj_.Message);
			string sql = $"INSERT INTO [ALARM] VALUES('{almObj_.ID.ToString()}','{App.Alarm.GetUnitName(almObj_.Unit)}','{almObj_.OccurTimeStr}','{almObj_.Message}')";
			App.Local_SQLServer.NonQuery(sql);
			LogFile.AddString(Str, false);
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
			Visibility = Visibility.Hidden;
		}
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			App.Alarm.ResetAllAlarm();
		}
	}

}
