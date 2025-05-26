using CIM.Lib.Model;
using CIM.ViewModel;
using Strong;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CIM.View
{
	/// <summary>
	/// fmCellInformation.xaml 的互動邏輯
	/// </summary>
	public partial class fmCellInformation : Window
	{
		CELL_STATUS CurnCellInfo;
		public fmCellInformation(CELL_STATUS cell)
		{
			InitializeComponent();
			CurnCellInfo = cell;

			List<CELL_STATUS> cells = new List<CELL_STATUS>();
			cells.Add(cell);
			CellInfo_datagrid.ItemsSource = cells;
			carouselid_textblock.DataContext = CurnCellInfo;
			cellid_textblock.DataContext = CurnCellInfo;
		}

		private void btn_Modify_Click(object sender, RoutedEventArgs e)
		{

		}

		private void btn_Close_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
