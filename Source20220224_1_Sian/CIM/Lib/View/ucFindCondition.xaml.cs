using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CIM.Lib.View
{
    /// <summary>
    /// Interaction logic for ucFindCondition.xaml
    /// </summary>
    /// public partial class ucStatus : UserControl
    public partial class ucFindCondition : UserControl
    {
        //public Port Port;
        public ucFindCondition()
        {
            InitializeComponent();
            dtFrom.SelectedDate = DateTime.Now;
            dtTo.SelectedDate = DateTime.Now;
        }
        //bool2BrushConverter bool2BrushConverter = new bool2BrushConverter();
        public void Initial()
        {
            //Port = p_;
            //txtPortNo.Text = Port.PortNo.ToString();
            //txtCstStatus.DataContext = Port.EC01["CstStatus"];
            //txtPortStatus.DataContext = Port.EC01["PortStatus"];
            //txtPortMode.DataContext = Port.EC01["PortMode"];
            //edWorkCount.DataContext = Port.EC01["WorkCount"];
            //edCstID.DataContext = Port.EC01["CstID"];
            //edEQBindID.DataContext = App.BcHs.BC_MG["Port"+Port.PortNo+"Bind"];

            //btnDisplay.SetBinding(Button.BackgroundProperty, new Binding { Source = Port,Path = new PropertyPath("ProdInfoPopReady"),Converter = bool2BrushConverter });
        }

		private void btnQury_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}