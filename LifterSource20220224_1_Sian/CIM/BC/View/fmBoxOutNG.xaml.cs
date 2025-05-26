using CIM.Lib.Model;
using CIM.Lib.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using static CIM.BC.DeliStore;

namespace CIM.BC.View
{
    /// <summary>
    /// Interaction logic for fmDelivery.xaml
    /// </summary>
    public partial class fmBoxOutNG : Window
    {
        public fmBoxOutNG()
        {
            InitializeComponent();
            this.Title = "Box ID Retry";
            Topmost = true;
        }
        private void btnKeyInClick(object sender, RoutedEventArgs e)
        {
            KeyInWindow.Visibility = Visibility.Visible;
        }
        private void btnRetryClick(object sender, RoutedEventArgs e)
        {
            App.Bc.BoxOutReply(1,"");
            Close();
        }

        private void tbKeyIn_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                App.Bc.BoxOutReply(2, tbKeyIn.Text.Trim());
            }
            KeyInWindow.Visibility = Visibility.Hidden;
            tbKeyIn.Text = "";
            Close();
        }
    }
}
