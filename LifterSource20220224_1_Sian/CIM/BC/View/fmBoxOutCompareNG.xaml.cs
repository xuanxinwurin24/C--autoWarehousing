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
    public partial class fmBoxOutCompareNG : Window
    {
        public fmBoxOutCompareNG()
        {
			InitializeComponent();
            this.Title = "Box ID Retry";
            Topmost = true;
        }
        private void btnReadClick(object sender, RoutedEventArgs e)
        {
            App.Bc.BoxOutCompareReply(2);
            Close();
        }
        private void btnBatchClick(object sender, RoutedEventArgs e)
        {
            App.Bc.BoxOutCompareReply(1);
            Close();
        }
    }
}
