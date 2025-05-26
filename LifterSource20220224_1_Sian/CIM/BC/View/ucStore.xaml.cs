using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;


namespace CIM.BC.View
{
    /// <summary>
    /// Interaction logic for ucStore.xaml
    /// </summary>
    public partial class ucStore : UserControl
    {
        RadioButton rdbtnDeli;
        public bool bFromAuto = false;
        public ucStore()
        {
            InitializeComponent();
            
        }
        public void Initial(RadioButton rdbtnDeli_)
        {
            rdbtnDeli = rdbtnDeli_;
            ucManual.Initial();
            ucAuto.Initial(btnManual, rdbtnDeli);
            btnManual.Tag = "0";
            btnAuto.Tag = "1";
            ucAuto.Visibility = Visibility.Visible;
            btnManual.DataContext = App.DS;
            btnAuto.DataContext = App.DS;
            App.Bc.SetStatusStore(DeliStore.ActMode.Store, DeliStore.InputWay.Auto);
        }

        private void btnManual_Click(object sender, RoutedEventArgs e)
        {
            App.Bc.SetStatusStore(DeliStore.ActMode.Store, DeliStore.InputWay.Manual);
            btnManual.Tag = "1";
            btnAuto.Tag = "0";
            ucManual.Visibility = Visibility.Visible;
            ucAuto.Visibility = Visibility.Collapsed;
            ucManual.dataGrid1.ItemsSource = null;
        }
        private void btnAuto_Click(object sender, RoutedEventArgs e)
        {
            App.Bc.SetStatusStore(DeliStore.ActMode.Store, DeliStore.InputWay.Auto);
            btnManual.Tag = "0";
            btnAuto.Tag = "1";
            ucManual.Visibility = Visibility.Collapsed;
            ucAuto.Visibility = Visibility.Visible;
            ucAuto.dataGrid1.ItemsSource = null;
		}
    }
}
