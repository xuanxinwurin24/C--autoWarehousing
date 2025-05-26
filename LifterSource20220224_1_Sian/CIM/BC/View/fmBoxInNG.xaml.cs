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
    public partial class fmBoxInNG : Window
    {
        public fmBoxInNG()
        {
            InitializeComponent();
            this.Title = "Box ID Retry";
            Topmost = true;
        }
        private void btnForceInClick(object sender, RoutedEventArgs e)
        {
            App.Bc.BoxInReply(2);
            Close();
        }
        private void btnRetryClick(object sender, RoutedEventArgs e)
        {
            App.Bc.BoxInReply(1);
            Close();
        }
    }
}
