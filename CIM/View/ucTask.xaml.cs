using CIM.BC;
using CIM.Lib.Model;
using CIM.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Strong;
using System.Windows.Input;

namespace CIM.View
{
	/// <summary>
	/// ucTask.xaml 的互動邏輯
	/// </summary>
	public partial class ucTask : UserControl
	{
		ObservableCollection<TaskViewItem> TaskViewList = new ObservableCollection<TaskViewItem>();
		DispatcherTimer Timer1 = new DispatcherTimer(DispatcherPriority.Send);

		public ucTask()
		{
			InitializeComponent();

			DataGrid_TaskList.ItemsSource = TaskViewList;

			AssignLED(led_STKConnect, App.Bc.wAux["STK_Connect"]);
			AssignLED(led_ShuttleCarConnect, App.Bc.wAux["ShuttleCar_Connect"]);
			AssignLED(led_STKControl, App.Bc.wAux["STK_ONLINE"]);
			AssignLED(led_ShuttleCarControl, App.Bc.wAux["ShuttleCar_ONLINE"]);
			AssignLED(led_OnlyDemo, App.Bc.wAux["isOnlyDemo"]);
			


			Timer1.Tick += new EventHandler(Timer1_Tick);
			Timer1.Interval = new TimeSpan(0, 0, 0, 5, 0);
			Timer1.Start();
		}
		private void AssignLED(Lib.Led led_, TagItem bItem_)
		{
			Binding b = new Binding("BinValue");
			b.Mode = BindingMode.OneWay;
			b.Delay = 500;
			b.Source = bItem_;
			b.Converter = new BoolTo01Converter();
			
			led_.SetBinding(Lib.Led.IsActiveProperty, b);
		}
		private void Timer1_Tick(object sender, EventArgs e)
		{
			Timer1.Stop();

			DataTable Result = App.Local_SQLServer.SelectDB("*", "[TASK]", "");
			
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
						App.Bc.Transfer_CarouselCell_to_ViewID(ref sSrcPos, ref sSrcCell);
						App.Bc.Transfer_CarouselCell_to_ViewID(ref sTarPos, ref sTarCell);

						string sSoteria = dr["SOTERIA"].ToString().Trim() == "S" ? "機密" : "非機密";
						switch (dr["DIRECTION"].ToString().Trim())
						{
							case "IN": sDirection = "入庫"; break;
							case "OUT": sDirection = "出庫"; break;
							case "MOVE": sDirection = "調儲"; break;
						}
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
							STARTTIME = dr["START_TIME"].ToString().Trim(),
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
		private void grid_TaskList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				DataGridCell cell = (DataGridCell)sender;
				cell.ContextMenu = null;

				ContextMenu menu = new ContextMenu();
				MenuItem item = new MenuItem();
				item.Header = (string)Application.Current.FindResource("lang_EditTaskPriority");
				item.DataContext = cell.DataContext;
                item.Click += EditPriority_Click;
				menu.Items.Add(item);
				MenuItem StoreCommand = new MenuItem();
				StoreCommand.Header = "入出庫命令重下";
				StoreCommand.DataContext = cell.DataContext;
				StoreCommand.Click += StoreDeliCommand_Click;
				menu.Items.Add(StoreCommand);
				cell.ContextMenu = menu;
			}
			catch (Exception ex)
			{
			}
		}
		private void EditPriority_Click(object sender, RoutedEventArgs e)
        {
			try
			{
				MenuItem item = (MenuItem)sender;
				TaskViewItem task = item.DataContext as TaskViewItem;
				
				string confirmButtonText = "確定";//(string)Application.Current.FindResource("lang_Confirm");

				Window window = new Window();
				StackPanel panel = new StackPanel();
				TextBox textBox = new TextBox();
				Button button = new Button();

				panel.Width = double.NaN;
				panel.Height = double.NaN;
				panel.Orientation = Orientation.Vertical;

				textBox.FontSize = 20.0d;
				textBox.Width = 200.0d;
				textBox.Height = 30.0d;
				textBox.Margin = new Thickness(5.0d);
				textBox.VerticalContentAlignment = VerticalAlignment.Center;
				textBox.MaxLength = 1;

				button.FontSize = 20.0d;
				button.Width = 120.0d;
				button.Height = 30.0d;
				button.Content = confirmButtonText;
				button.Margin = new Thickness(5.0d);

				button.Click += (s, ee) =>
				{
					window.Tag = textBox.Text.Trim().ToIntDef(0);
					window.DialogResult = true;
				};

				panel.Children.Add(textBox);
				panel.Children.Add(button);

				window.Owner = Application.Current.MainWindow;
				window.ResizeMode = ResizeMode.NoResize;
				window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				window.Content = panel;
				window.Title = "請輸入優先權(1~9)";
				window.SizeToContent = SizeToContent.WidthAndHeight;

				window.ShowDialog();

				if (window.Tag != null)
                {
					int priority = (int)window.Tag;

					if (priority != 0)
					{
						string sSQL = $"UPDATE [TASK] SET [PRIORITY] = {priority} WHERE [COMMAND_ID] = '{task.COMMANDID}'";
						App.Local_SQLServer.NonQuery(sSQL);
						task.PRIORITY = priority;
					}
					else
					{
						MessageBox.Show("優先權輸入錯誤");
					}
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
		private void StoreDeliCommand_Click(object sender,RoutedEventArgs e)
		{
			try
			{
				MenuItem item = (MenuItem)sender;
				TaskViewItem task = item.DataContext as TaskViewItem;

				string confirmButtonText = "確定";//(string)Application.Current.FindResource("lang_Confirm");

				Window window = new Window();
				StackPanel panel = new StackPanel();
				Button button = new Button();

				panel.Width = double.NaN;
				panel.Height = double.NaN;
				panel.Orientation = Orientation.Vertical;

				button.FontSize = 20.0d;
				button.Width = 60.0d;
				button.Height = 30.0d;
				button.Content = confirmButtonText;
				button.Margin = new Thickness(5.0d);

				button.Click += (s, ee) =>
				{
					window.Tag = true;
					window.DialogResult = true;
				};

				panel.Children.Add(button);

				window.Owner = Application.Current.MainWindow;
				window.ResizeMode = ResizeMode.NoResize;
				window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
				window.Content = panel;
				window.Title = "入出庫重新下令";
				window.SizeToContent = SizeToContent.WidthAndHeight;

				window.ShowDialog();

				if (window.Tag != null)
				{
					string dir = task.DIRECTION;
					if (dir == "IN")
					{
						string sSQL = $"UPDATE [TASK] SET [STATUS] = '2' WHERE [COMMAND_ID] = '{task.COMMANDID}'";
						App.Local_SQLServer.NonQuery(sSQL);
						task.STATUS = "2";
					}
					else
					{
						string sSQL = $"UPDATE [TASK] SET [STATUS] = '1' WHERE [COMMAND_ID] = '{task.COMMANDID}'";
						App.Local_SQLServer.NonQuery(sSQL);
						task.STATUS = "1";
					}
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (App.Bc.wAux["isOnlyDemo"].BinValue == 1)
				App.Bc.wAux["isOnlyDemo"].BinValue = 0;
			else
				App.Bc.wAux["isOnlyDemo"].BinValue = 1;
		}
	}
	public class BoolTo01Converter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			uint val = (uint)value;
			if ((val) == 0)
			{ return false; }
			else
			{ return true; }
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool bl = (bool)value;
			return bl == true ? 1 : 0;
		}
	}
}
