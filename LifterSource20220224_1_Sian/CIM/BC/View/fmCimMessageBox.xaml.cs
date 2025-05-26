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
    public partial class fmCimMessageBox : Window
    {
        public fmCimMessageBox(string sTitle_, string msg_)
        {
            InitializeComponent();
            this.Title = sTitle_;
            txt.Text = msg_;
            Topmost = true;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(7);
            timer.Tick += TimerTick;
            timer.Start();
        }
        private void TimerTick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
