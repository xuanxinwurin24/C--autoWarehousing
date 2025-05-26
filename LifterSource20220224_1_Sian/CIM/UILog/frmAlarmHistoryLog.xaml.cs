using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CIM.UILog
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class frmAlarmHistoryLog : UserControl
    {
        public int MaxLogRowCunt = 1000;

        public LogWriter LogFile;
        protected List<AlarmBody> MsgLst = new List<AlarmBody>();

        protected object LockObj = new object();
        protected System.Windows.Threading.DispatcherTimer RefreshTimer;

        public frmAlarmHistoryLog()
        {
            LogFile = new LogWriter(Environment.CurrentDirectory + @"\LogFile", GetType().Name, 50000);

            InitializeComponent();
            RefreshTimer = new System.Windows.Threading.DispatcherTimer();
            RefreshTimer.Tick += new EventHandler(RefreshTimer_Tick);
            RefreshTimer.Interval = new TimeSpan(0, 0, 1);

            
            if (File.Exists(Environment.CurrentDirectory + @"\Ini\AlarmHistory.xml") == true)
                MsgLst = Common.DeserializeXMLFileToObject<List<AlarmBody>>(Environment.CurrentDirectory + @"\Ini\AlarmHistory.xml");
            lv.ItemsSource = MsgLst;
            App.Alarm.AlarmEvent += new Alarm.AlarmEventHandler(AlarmEventCallBack);

        }
        void AlarmEventCallBack(AlarmBody AlmObj_, bool bFinal_)
        {
            if (AlmObj_.bOccured == false)
            {
                Write2Log(AlmObj_);
                //Insert(AlmObj_);
                Modify(AlmObj_);

                RefreshMMI();
            }
            else if(AlmObj_.bOccured == true)
            {
                Insert(AlmObj_);

                RefreshMMI();
            }
        }
        private void Insert(AlarmBody AlmObj_)
        {
            lock (LockObj)
            {
                try
                {
                    AlarmBody bdy = new AlarmBody();
                    bdy.Copy(AlmObj_);
                    bdy.OccurTimeTmp = bdy.OccurTimeStr;
                    MsgLst.Insert(0, bdy);
                    if (MsgLst.Count > MaxLogRowCunt)
                    {
                        MsgLst.RemoveRange(MaxLogRowCunt - 20, MsgLst.Count - MaxLogRowCunt + 20);
                        
                    }
                   
                }
                catch (Exception e_) { LogWriter.LogException(e_); }
            }
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
        public void Write2Log(AlarmBody almObj_)
        {
            if (almObj_.bOccured == true)
            { return; }
            //string uname = App.alarm.GetUnitName(almObj_.Unit);
            string Str = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{4:X4}\t{5}", almObj_.OccurTimeStr, almObj_.DisOccurTimeStr, almObj_.Unit, almObj_.Unit, almObj_.ID, almObj_.Message);
            LogFile.AddString(Str, false);


        }
        public void Modify(AlarmBody AlmObj_)
        {
            AlarmBody AlmObj = MsgLst.Find(x => x.Code == AlmObj_.Code && x.OccurTimeStr == AlmObj_.OccurTimeStr);
            if(AlmObj!=null)
            {
                AlmObj.DisOccurTime = DateTime.Now;
                AlmObj.DisOccurTimeTmp = AlmObj.DisOccurTimeStr;
            }
        }
        public void SaveToFile()
        {
            Common.SerializeXMLObjToFile<List<AlarmBody>>(Environment.CurrentDirectory + @"\Ini\AlarmHistory.xml", MsgLst);
        }
      
    }    
}
