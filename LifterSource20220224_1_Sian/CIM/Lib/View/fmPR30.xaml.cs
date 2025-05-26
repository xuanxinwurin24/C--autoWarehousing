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
using System.Windows.Shapes;

namespace CIM.Lib.View
{
    /// <summary>
    /// Interaction logic for fmET7000.xaml
    /// </summary>
    public partial class fmPR30 : Window
    {
        public fmPR30()
        {
            InitializeComponent();
            NewTabPages_ForDevMems();
        }
        void NewTabPages_ForDevMems()
        {
            try
            {
                TabCtrl.Items.Clear();
                foreach (MemGroup mg in App.Bc.PR30DevMg)
                {
                    MemGroupControl mgUserCtrl = new MemGroupControl(mg);
                    TabItem tabPage = new TabItem();
                    tabPage.Content = new Grid();

                    tabPage.Header =mg.Device.Name +"-" + mg.Name;
                    tabPage.Content = mgUserCtrl;
                    TabCtrl.Items.Add(tabPage);
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
