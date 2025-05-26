using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CIM.UILog
{
	/// <summary>
	/// Interaction logic for frmLog.xaml
	/// </summary>
	public partial class frmEqLog : UserControl
	{
		public int MaxLogRowCunt = 500;
		public LogWriter LogFile;
		protected ObservableCollection<Item> MsgLst = new ObservableCollection<Item>();

		protected object LockObj = new object();
		protected System.Windows.Threading.DispatcherTimer RefreshTimer;

		public frmEqLog()
		{
			LogFile = new LogWriter(Environment.CurrentDirectory + @"\LogFile", GetType().Name, 50 * 1000);//?

			InitializeComponent();
			BindingOperations.EnableCollectionSynchronization(MsgLst, LockObj);

			RefreshTimer = new System.Windows.Threading.DispatcherTimer();
			RefreshTimer.Tick += new EventHandler(RefreshTimer_Tick);
			RefreshTimer.Interval = new TimeSpan(0, 0, 1);

            MsgLst.CollectionChanged += MsgLst_CollectionChanged;

			lv.ItemsSource = MsgLst;
		}

        private void MsgLst_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
			{
				App.Current.Dispatcher.Invoke((Action)(() =>
				{
					foreach (var col in (lv.View as GridView).Columns)
					{
						if (double.IsNaN(col.Width))
						{
							col.Width = col.ActualWidth;
						}
						col.Width = double.NaN;
					}
				}));

			}
			catch (Exception ex)
            {
				LogExcept.LogException(ex);
            }
        }

        public virtual void Log(string msg_, bool bDate_ = true)
		{
			try
			{
				lock (LockObj)
				{
					Item itm = new Item()
					{ Dt = DateTime.Now, Msg = msg_ };
					MsgLst.Insert(0, itm);
					LogFile.AddString(itm.ToString(), false);

					if (MsgLst.Count > MaxLogRowCunt)
					{
						MsgLst.RemoveAt(MsgLst.Count - 1);
						//MsgLst.RemoveRange(MaxLogRowCunt - 20, MsgLst.Count - MaxLogRowCunt + 20);
					}
					//if (FreezeMenuItem.Checked == false)
					//{ RefreshMMI(); }
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
		public void Delete_SQL_Log(string type_)
        {
            try
            {
				string SQL_Temp = $"SELECT * FROM [LOG_RECORD] WHERE TYPE='{type_}'";
				DataTable dt_Temp = new DataTable();
				App.Local_SQLServer.Query(SQL_Temp, ref dt_Temp);
                if (dt_Temp.Rows.Count > 40)
                {
					SQL_Temp = $"DELETE FROM BATCH_LIST WHERE TYPE IN(SELECT TYPE FROM (SELECT TOP({dt_Temp.Rows.Count - 40}) TYPE FROM LOG_RECORD WHERE TYPE = '{type_}' ORDER BY TIME ASC) a)";
					App.Local_SQLServer.NonQuery(SQL_Temp);
				}
				
			}
			catch (Exception ex)
            {
				LogExcept.LogException(ex);
			}

		}
		public void RefreshMMI()
		{
			RefreshTimer.Stop();
			RefreshTimer.Start();
		}
		private void RefreshTimer_Tick(object sender, EventArgs e)
		{ UpdateMMI(); }
		public virtual void UpdateMMI()
		{
			try
			{
				//RefreshTimer.Stop();
				//ICollectionView view = CollectionViewSource.GetDefaultView(MsgLst);
				//if (view != null)
				//{ view.Refresh(); }
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
			Visibility = Visibility.Hidden;
		}
		protected class Item
		{
			public DateTime Dt;
			public string TimeStr
			{
				get { return Dt.ToString("yyyyMMdd-HH:mm:ss:fff"); }
			}
			public string Msg { get; set; }
			public override string ToString()
			{
				return string.Format("Time={0},Msg={1}", TimeStr, Msg);
			}
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			string n1 = Name;
		}
	}
}
