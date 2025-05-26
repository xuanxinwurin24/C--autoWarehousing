using Strong;
using System;
using System.Collections.Generic;
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

namespace CIM.Lib.View
{
	/// <summary>
	/// Interaction logic for BC_IFSig.xaml
	/// </summary>
	public partial class ucIFSig : UserControl
	{
		public int HoriGap = 5;
		public int VertGap = 8;

		MemGroup rbMg_b, eqMg_b, workDataMg;
		public ucIFSig()
		{
			InitializeComponent();
		}
		public void SetReadOnly(bool true_)
		{
			stackMain.IsHitTestVisible = true_;
			//gbRobot.IsHitTestVisible = true_;
			//txtWorkData.IsHitTestVisible = true_;
			//for (int i = 0; i < rbGrid.Children.Count; i++)
			//{
			//    if (rbGrid.Children[i].GetType().Name == "Grid")
			//    {
			//        Grid gd = rbGrid.Children[i] as Grid;
			//        for (int j = 0; j < gd.Children.Count; j++)
			//        {
			//            if (gd.Children[i].GetType().Name == "Led")
			//            {
			//                gd.Children[i].IsHitTestVisible = true_;
			//            }
			//        }
			//    }
			//}
		}
		public void Initial(Style ledStyle_, MemGroup rbMg_b_, MemGroup eqMg_b_, MemGroup workDataMg_)
		{
			rbMg_b = rbMg_b_;
			eqMg_b = eqMg_b_;
			workDataMg = workDataMg_;

			rbGrid.Children.Clear();
			eqGrid.Children.Clear();
			BitMg2LedMap(ledStyle_, rbGrid, rbMg_b_, HoriGap, 4);
			BitMg2LedMap(ledStyle_, eqGrid, eqMg_b_, HoriGap, 4);
			txtWorkData.DataContext = workDataMg["Data"];
		}
		public static void BitMg2LedMap(Style ledStyle_, Grid mainGrid_, MemGroup mg_, int horiGap_, int colCount_)
		{
			List<TagItem> Items = new List<TagItem>();
			foreach (TagItem it in mg_.ItemList)
			{
				//int w = int.TryParse(it.Hint.Trim(), out w) == true ? w : 0;
				//if (w == -1) continue;
				if (it.Length > 1) continue;
				Items.Add(it);
			}
			int rowCount = Items.Count / colCount_;
			if ((Items.Count % colCount_) != 0) rowCount++;

			List<Grid> subGrids = Grid_Create(mainGrid_, mg_, horiGap_, colCount_, rowCount);

			for (int c = 0, itCnt = 0; c < colCount_; c++)
			{
				for (int r = 0; r < rowCount && itCnt < Items.Count; r++, itCnt++)
				{
					TextBlock_Crete(subGrids[c], r, c, Items[itCnt]);
					Led_Crete(ledStyle_, subGrids[c], r, c, Items[itCnt]);
				}
			}
		}
		public static List<Grid> Grid_Create(Grid mainGrid_, MemGroup mg_, int horiGap_, int colCount_, int rowCount_)
		{
			mainGrid_.Children.Clear();
			List<Grid> subGrids = new List<Grid>();
			//--------main grid
			for (int c = 0; c < colCount_; c++)
			{
				mainGrid_.ColumnDefinitions.Add(new ColumnDefinition());
				//mainGrid_.ColumnDefinitions[c].Width = new System.Windows.GridLength(400);
			}
			mainGrid_.RowDefinitions.Add(new RowDefinition());
			//--------sub grid
			for (int c = 0; c < colCount_; c++)
			{
				Grid g = new Grid();
				mainGrid_.Children.Add(g);
				Grid.SetRow(g, 0);
				Grid.SetColumn(g, c);
				subGrids.Add(g);
				//g.ShowGridLines = true;
				g.Margin = new Thickness(2, horiGap_ / 2, 2, horiGap_ / 2);
				g.ColumnDefinitions.Add(new ColumnDefinition());//textblock
				g.ColumnDefinitions.Add(new ColumnDefinition());//textbox
				for (int r = 0; r < rowCount_; r++)
				{
					g.RowDefinitions.Add(new RowDefinition());
				}
				//g.ShowGridLines = true;
			}
			return subGrids;
		}
		public static void TextBlock_Crete(Grid glid_, int row_, int col_, TagItem item_)
		{
			//--------new text block-----------
			//Thickness th = new Thickness(50, 0, 5, 0);
			TextBlock tb = new TextBlock();
			//tb.Margin = th;

			tb.HorizontalAlignment = HorizontalAlignment.Right;
			tb.VerticalAlignment = VerticalAlignment.Center;

			glid_.Children.Add(tb);
			Grid.SetRow(tb, row_);
			Grid.SetColumn(tb, 0);

			//tb.Tag = Items[itCnt].Hint.Trim();
			//tb.Text = Items[itCnt].Hint.Trim().Length == 0 ? Items[itCnt].Name : Items[itCnt].Hint;
			tb.Text = item_.Name;
			tb.Margin = glid_.Margin;
		}
		public static void Led_Crete(Style ledStyle_, Grid glid_, int row_, int col_, TagItem item_)
		{
			////th = new Thickness(HoriGap, VertGap, 0, 0);
			Led led = new Led();
			led.Style = ledStyle_;

			glid_.Children.Add(led);
			Grid.SetRow(led, row_);
			Grid.SetColumn(led, 1);

			Binding b = new Binding("BinValue");
			b.Mode = BindingMode.TwoWay;
			b.Delay = 500;
			b.Source = item_;
			b.Converter = new BoolTo01Converter();
			led.SetBinding(Led.IsActiveProperty, b);
			led.Margin = glid_.Margin;
		}
		public static void TextBox_Crete(Grid glid_, int row_, int col_, TagItem item_, int width_)
		{
			TextBox ed = new TextBox();
			ed.Width = width_;
			//ed.Margin = new Thickness(10);

			//ed.HorizontalAlignment = HorizontalAlignment.Center;
			//ed.VerticalAlignment = VerticalAlignment.Center;
			ed.HorizontalContentAlignment = HorizontalAlignment.Center;

			glid_.Children.Add(ed);
			Grid.SetRow(ed, row_);
			Grid.SetColumn(ed, 1);

			//Binding b = new Binding(item_.StrType == TagItemStrType.ASC ? "StringValue" : "BinValue");
			Binding b = new Binding("StringValue");
			b.Mode = BindingMode.TwoWay;
			b.Delay = 500;
			b.Source = item_;
			//b.Converter = new BoolTo01Converter();
			ed.SetBinding(TextBox.TextProperty, b);
		}
		private void txtWorkData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			//fmWorkData fm = new fmWorkData(workDataMg);
			//fm.ShowDialog();
		}
	}
	//public class BoolTo01Converter : IValueConverter
	//{
	//    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	//    {
	//        uint val = (uint)value;
	//        if ((val) == 0)
	//        { return false; }
	//        else
	//        { return true; }
	//    }

	//    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	//    {
	//        bool bl = (bool)value;
	//        return bl == true ? 1 : 0;
	//    }
	//}
}
