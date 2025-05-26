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
using System.Web.Configuration;
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
	/// ucMain.xaml 的互動邏輯
	/// </summary>
	public partial class ucStocker : UserControl
	{
		public List<CarouselView> carousels;
		public ucStocker()
		{
			InitializeComponent();
#if DEBUG
			if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
#endif
			example_carousel_grid.Visibility = Visibility.Collapsed;
			Initial();
		}

		public void Initial()
		{
			carousels = new List<CarouselView>();

			DataTable Result = new DataTable();
			string sql = $"SELECT [CAROUSEL_ID],[ViewColumn],[ViewRow] FROM [CAROUSEL_VIEW]";
			//sql = $"SELECT statustable.[CAROUSEL_ID],[STATUS],[STORE_STATUS],[N2_STATUS],[TEMPERATURE],[TEMPERATURE_STATUS],[HUMIDITY],[HUMIDITY_STATUS],[COMMAND_ID],[CHECK_STATUS],[ViewColumn],[ViewRow]" +
			//$" FROM [CAROUSEL_STATUS] statustable join [CAROUSEL_VIEW] viewtable on statustable.[CAROUSEL_ID] = viewtable.[CAROUSEL_ID]";
			App.Local_SQLServer.Query(sql, ref Result);
			string scellid = string.Empty;
			foreach (DataRow dr in Result.Rows)
			{
				string carouselid = dr["CAROUSEL_ID"].ToString().Trim();
				//string cmdid = dr["COMMAND_ID"].ToString().Trim();
				//int status, store_status, n2_status, temp_status, humi_status, check_status;
				//int.TryParse(dr["STATUS"].ToString(), out status);
				//int.TryParse(dr["STORE_STATUS"].ToString(), out store_status);
				//int.TryParse(dr["N2_STATUS"].ToString(), out n2_status);
				//int.TryParse(dr["TEMPERATURE_STATUS"].ToString(), out temp_status);
				//int.TryParse(dr["HUMIDITY_STATUS"].ToString(), out humi_status);
				//int.TryParse(dr["CHECK_STATUS"].ToString(), out check_status);
				//double temperature, humidity;
				//double.TryParse(dr["TEMPERATURE"].ToString(), out temperature);
				//double.TryParse(dr["HUMIDITY"].ToString(), out humidity);
				int col, row;
				int.TryParse(dr["ViewColumn"].ToString(), out col);
				int.TryParse(dr["ViewRow"].ToString(), out row);

				App.Bc.Transfer_CarouselCell_to_ViewID(ref carouselid, ref scellid); //轉換成SHOW的ID
				carousels.Add(new CarouselView
				{
					Carousel_ID = carouselid,
					Column = col,
					Row = row
				});
			}

			foreach (CarouselView view in carousels)
			{
				cCarousel current_carousel = App.Bc.Carousels.Find(x => x.Carousel_ID == view.Carousel_ID);
				if (current_carousel == null) continue;
				while (view.Column + 1 > gridMain.ColumnDefinitions.Count)
				{
					gridMain.ColumnDefinitions.Add(new ColumnDefinition());
				}
				while (view.Row + 1 > gridMain.RowDefinitions.Count)
				{
					gridMain.RowDefinitions.Add(new RowDefinition());
				}
				Grid carouselgrid = new Grid();
				carouselgrid.Width = 150;
				carouselgrid.Margin = new Thickness(0, 0, 10, 0);
				carouselgrid.RowDefinitions.Add(new RowDefinition
				{
					Height = new GridLength(1, GridUnitType.Auto)
				});
				carouselgrid.RowDefinitions.Add(new RowDefinition
				{
					Height = new GridLength(1, GridUnitType.Star)
				});
				gridMain.Children.Add(carouselgrid);
				carouselgrid.DataContext = current_carousel;
				//Temperature
				StackPanel temp_sp = new StackPanel();
				temp_sp.Orientation = Orientation.Horizontal;
				temp_sp.Margin = new Thickness(10, 0, 0, 0);
				temp_sp.VerticalAlignment = VerticalAlignment.Top;
				temp_sp.HorizontalAlignment = HorizontalAlignment.Left;
				TextBlock temp_txt1 = new TextBlock();
				//temp_txt1.Text = "25.5";
				//Binding temperature
				Binding binding = new Binding("Temperature");
				binding.Mode = BindingMode.TwoWay;
				binding.Delay = 500;
				temp_txt1.SetBinding(TextBlock.TextProperty, binding);
				temp_sp.Children.Add(temp_txt1);
				TextBlock temp_txt2 = new TextBlock();
				temp_txt2.Text = " °C";
				temp_sp.Children.Add(temp_txt2);
				carouselgrid.Children.Add(temp_sp);
				//Humidity
				StackPanel humi_sp = new StackPanel();
				humi_sp.Orientation = Orientation.Horizontal;
				humi_sp.Margin = new Thickness(0, 0, 10, 0);
				humi_sp.VerticalAlignment = VerticalAlignment.Top;
				humi_sp.HorizontalAlignment = HorizontalAlignment.Right;
				TextBlock humi_txt1 = new TextBlock();
				//humi_txt1.Text = "30";
				//Binding humidity
				Binding binding2 = new Binding("Humidity");
				binding2.Mode = BindingMode.TwoWay;
				binding2.Delay = 500;
				humi_txt1.SetBinding(TextBlock.TextProperty, binding2);
				humi_sp.Children.Add(humi_txt1);
				TextBlock humi_txt2 = new TextBlock();
				humi_txt2.Text = " %";
				humi_sp.Children.Add(humi_txt2);
				carouselgrid.Children.Add(humi_sp);
				//Status
				Button carousel_info_button = new Button();
				carousel_info_button.BorderBrush = Brushes.Gray;
				carousel_info_button.BorderThickness = new Thickness(1);
				carousel_info_button.Background = Brushes.Lime;
				carousel_info_button.Margin = new Thickness(0, 5, 0, 0);
				carousel_info_button.Content = view.Carousel_ID;
				carousel_info_button.Padding = new Thickness(0, 5, 0, 5);
				carousel_info_button.FontWeight = FontWeights.Bold;
				carousel_info_button.DataContext = current_carousel;
				carouselgrid.Children.Add(carousel_info_button);
				Grid.SetRow(carousel_info_button, 1);

				Grid.SetRow(carouselgrid, view.Row);
				Grid.SetColumn(carouselgrid, view.Column);
			}
		}

		private void Carousel_info_button_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Button btn = (Button)sender;
				if (btn.DataContext.GetType().Name != "cCarousel") return;
				fmCarousel vw = new fmCarousel((cCarousel)btn.DataContext);
				vw.Owner = Window.GetWindow(this);
				vw.ShowDialog();
				vw = null;
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
	}
}
