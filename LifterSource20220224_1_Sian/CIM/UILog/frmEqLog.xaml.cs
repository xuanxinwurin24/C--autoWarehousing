using Strong;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CIM.UILog
{
    /// <summary>
    /// Interaction logic for frmLog.xaml
    /// </summary>
    public partial class frmEqLog : UserControl
    {
        public int MaxLogRowCunt = 500;
        public LogWriter LogFile;
        protected List<Item> MsgLst = new List<Item>();

        protected object LockObj = new object();
        protected System.Windows.Threading.DispatcherTimer RefreshTimer;

        public frmEqLog()
        {
            LogFile = new LogWriter(Environment.CurrentDirectory + @"\LogFile", GetType().Name, 50000);//?

            InitializeComponent();

            RefreshTimer = new System.Windows.Threading.DispatcherTimer();
            RefreshTimer.Tick += new EventHandler(RefreshTimer_Tick);
            RefreshTimer.Interval = new TimeSpan(0, 0, 1);

            lv.ItemsSource = MsgLst;
        }
        public virtual void Log(string msg_, bool bDate_ = true)
        {
            try
            {
                lock (LockObj)
                {
                    Item itm = new Item()
                    { Dt = DateTime.Now, Msg = msg_ };
                    MsgLst.Insert(0, itm);
                    LogFile.AddString(itm.ToString(), false);

                    if (MsgLst.Count > MaxLogRowCunt)
                    {
                        MsgLst.RemoveRange(MaxLogRowCunt - 20, MsgLst.Count - MaxLogRowCunt + 20);
                    }
                    //if (FreezeMenuItem.Checked == false)
                    { RefreshMMI(); }
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        public void RefreshMMI()
        {
            RefreshTimer.Stop();
            RefreshTimer.Start();
        }
        private void RefreshTimer_Tick(object sender, EventArgs e)
        { UpdateMMI(); }
        public virtual void UpdateMMI()
        {
            try
            {
                RefreshTimer.Stop();
                ICollectionView view = CollectionViewSource.GetDefaultView(MsgLst);
                if (view != null)
                { view.Refresh(); }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
        protected class Item
        {
            public DateTime Dt;
            public string TimeStr
            {
                get { return Dt.ToString("yyyyMMdd-hh:mm:ss:fff"); }
            }
            public string Msg { get; set; }
            public override string ToString()
            {
                return string.Format("Time={0},Msg={1}", TimeStr, Msg);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string n1 = Name;
        }
    }
}
