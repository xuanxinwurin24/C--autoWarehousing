using CIM.BC;
using CIM.View;
using CIM.Lib.Model;
using CIM.Lib.View;
using CIM.UILog;
using CIM.ViewModel;
using Strong;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using System.Windows.Data;
using System.Timers;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Collections;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace CIM
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public ucStocker ucMain;
		public ucTask ucTaskView;
		public ucSystemSetting ucSetting;
		public ucUserGroup ucUser;
		public ucAlarm ucAlarm;
		public static ucUILog ucUILog = new ucUILog();
		public ucCheck ucCheck;
		public ucManualMove ucManualMove;
		public ucTest ucTest;
        public ucRecover ucRecover;

        DispatcherTimer Timer1 = new DispatcherTimer(DispatcherPriority.Send);

		public static void ScreenPosition(Window window_)
		{
			//筆電螢幕過小，用來將主畫面顯示於副螢幕用
			System.Windows.Forms.Screen[] _screens = System.Windows.Forms.Screen.AllScreens;
			if (_screens.Length > 1)
			{
				System.Windows.Forms.Screen s = System.Windows.Forms.Screen.AllScreens[1];
				System.Drawing.Rectangle rect = s.WorkingArea;
				window_.Left = rect.Left;
				window_.Top = rect.Top;
			}
			else
			{
				System.Windows.Forms.Screen s = System.Windows.Forms.Screen.AllScreens[0];
				System.Drawing.Rectangle rect = s.WorkingArea;
				window_.Left = rect.Left;
				window_.Top = rect.Top;
			}
			//
		}
		public MainWindow()
		{
			InitializeComponent();
			//DeviceMemoryView.MenuForMemStatus(MainMenu);
			InitializeViews();
			InitializeUI();
			ScreenPosition(this);
			if (System.Diagnostics.Debugger.IsAttached)
			{
				Password.Login("strong", "5999011");
			}
			UserChanged(Password.CurnUser.Group.Level);
			Password.LogInOutEvent += LogInOutEventCallBackFunc;

			App.Start();

			Timer1.Tick += new EventHandler(Timer1_Tick);
			Timer1.Interval = new TimeSpan(0, 0, 0, 0, 500);
			Timer1.Start();
		}

		private void Timer1_Tick(object sender, EventArgs e)
		{
			Timer1.Stop();

			CIMStatusBar.lbDateTime.GetBindingExpression(ContentProperty).UpdateTarget();   //更新畫面顯示時間

			//lbWindowScanTime.Content = string.Format("Window: {0}", ts.TotalMilliseconds);
			//lbScanTime.Content = string.Format("Main: {0}", App.MainThread.ScanTime);
			//lbDatabaseScanTime.Content = string.Format("Database: {0}", App.DatabaseThread.ScanTime);

			foreach (CustomMenuItem it in ListViewMenu.Items)
			{
				if (it.Screen == ucAlarm)
				{
					if (App.Alarm.bAlarming || App.Alarm.bWarming)
					{
						if (it.Tag?.ToString() == "0" || ListViewMenu.SelectedItem == it)
						{
							it.Tag = 1;
							it.Background = Brushes.Red;
						}
						else
						{
							it.Tag = 0;
							it.Background = Brushes.Transparent;
						}
					}
					else
					{
						it.Tag = 0;
						it.Background = Brushes.Transparent;
					}

				}
			}

			Timer1.Start();
		}

		void LogInOutEventCallBackFunc(string sOldUserName_, string sNewUserName_)
		{
			if (App.MainThread != null)
			{
				App.MainThread.PasswordLevel = Password.CurnUser.Group.Level;
			}
			Application.Current.Dispatcher.BeginInvoke(new Action(() =>
			{
				UserChanged(Password.CurnUser.Group.Level);    //PasswordChanged(Password.CurnLevel);
			}));
		}

		public void UserChanged(int iLevel_)
		{
			try
			{
				int idx = -1;
				foreach (CustomMenuItem it in ListViewMenu.Items)
				{
					if (it.Screen == ucTest)
					{
						it.Visibility = iLevel_ >= App.Level_SUPER ? Visibility.Visible : Visibility.Collapsed;
					}
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public void PasswordChanged(int iLevel_)
		{
			try
			{
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (MessageBox.Show("Program End ?", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
			{
				e.Cancel = false;
				//App.BcMain.BackupData();
				//MainWindow.frmAlarmHistoryLog.SaveToFile();
				Application.Current.Shutdown();
			}
			else
			{
				ListViewMenu.SelectedIndex = last_listviewmunu_selectedindex;
				e.Cancel = true;
			}
		}
		List<CustomMenuItem> menus = new List<CustomMenuItem>();

		void InitializeViews()
		{
			try
			{
				ucMain = new ucStocker();
				ucTaskView = new ucTask();
				ucSetting = new ucSystemSetting();
				ucUser = new ucUserGroup();
				ucAlarm = new ucAlarm();
				ucCheck = new ucCheck();
				ucManualMove = new ucManualMove();
				ucTest = new ucTest();
                ucRecover = new ucRecover();
                //CIMStatusBar.DataContext = Password;
            }
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		void InitializeUI()
		{
			try
			{
				ListViewMenu.Items.Add(new CustomMenuItem("Main", ucMain));
				ListViewMenu.Items.Add(new CustomMenuItem("Task", ucTaskView));
				ListViewMenu.Items.Add(new CustomMenuItem("System", ucSetting));
				ListViewMenu.Items.Add(new CustomMenuItem("UserManagemant", ucUser, new CustomMenuItem.ClickEventHandler(ucUser.InitializeParameter)));
				ListViewMenu.Items.Add(new CustomMenuItem("Alarm", ucAlarm));
				ListViewMenu.Items.Add(new CustomMenuItem("Log", ucUILog));
                ListViewMenu.Items.Add(new CustomMenuItem("Recover", ucRecover,new CustomMenuItem.ClickEventHandler(ucRecover.Recover_Initial)));
                //ListViewMenu.Items.Add(new CustomMenuItem("SearchCheck", ucCheck));
                //ListViewMenu.Items.Add(new CustomMenuItem("ManualMove", ucManualMove));
                ListViewMenu.Items.Add(new CustomMenuItem("Test", ucTest));
				ListViewMenu.Items.Add(new CustomMenuItem("Exit", null, new CustomMenuItem.ClickEventHandler(Close)));
				ListViewMenu.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}


		int last_listviewmunu_selectedindex = 0;
		private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (((ListView)sender).SelectedItem.GetType().Name != "CustomMenuItem") return;
			UserControl uc = ((CustomMenuItem)((ListView)sender).SelectedItem).Screen;
			CustomMenuItem.ClickEventHandler handler = ((CustomMenuItem)((ListView)sender).SelectedItem).ClickEvent;
			if (uc != null)
			{
				GridMain.Children.Clear();
				GridMain.Children.Add(uc);
			}
			if (handler != null)
			{
				handler();
			}
			if (last_listviewmunu_selectedindex != ListViewMenu.SelectedIndex)
				last_listviewmunu_selectedindex = ListViewMenu.SelectedIndex;
		}
	}
}