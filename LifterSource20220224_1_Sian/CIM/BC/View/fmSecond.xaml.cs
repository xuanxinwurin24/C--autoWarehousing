using CIM.Lib.Model;
using CIM.Lib.View;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using static CIM.BC.DeliStore;

namespace CIM.BC.View
{
    /// <summary>
    /// Interaction logic for fmDelivery.xaml
    /// </summary>
    public partial class fmSecond : Window, INotifyPropertyChanged
    {
        //bool bCount = true;

        public event PropertyChangedEventHandler PropertyChanged;
        public fmSecond()
        {
            InitializeComponent();
            DeviceMemoryView.MenuForMemStatus(MainMenu);
            rbtnDelivery.DataContext = App.Eq;
            rbtnStore.DataContext = App.Eq;
            lbDateTime.DataContext = this;
            lbUserName.DataContext = App.DS.OP;
            lbLevel.DataContext = App.DS.OP;

            uStore.Initial(rbtnDelivery);
            Lifter_Status_Change($"{App.SysPara.EqID}", "Store");
        }
        void OnPropertyChanged(string name_)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name_));
            }
        }

        string dateTime;
        public string DateTimeValue
        {
            get
            {
                return dateTime;
            }
            set
            {
                if (dateTime == value) return;
                dateTime = value;
                OnPropertyChanged("DateTimeValue");
            }
        }

        private void mnSysPar_Click(object sender, RoutedEventArgs e)
        {
            fmSysPar fm = new fmSysPar();
            fm.ShowDialog();
        }

        private void mnOffline_Click(object sender, RoutedEventArgs e)
        {
            fmOfflineSeting fm = new fmOfflineSeting();
            fm.ShowDialog();
        }

        private void mnLog_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Log.Show();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
            Password.Logout();
            App.DS.OP.Logout();
        }

        private void rbtnStore_Click(object sender, RoutedEventArgs e)
        {
            App.Bc.SetStatusStore(ActMode.Store, InputWay.Manual);
            uStore.Initial(rbtnDelivery);
            uStore.Visibility = Visibility.Visible;
            uDelivery.Visibility = Visibility.Collapsed;
            uSimu.Visibility = Visibility.Collapsed;
            App.DS.UpdateDelivery = false;
            Lifter_Status_Change($"{App.SysPara.EqID}", "Store");
        }

        private void rbtnDelivery_Click(object sender, RoutedEventArgs e)
        {
            App.Bc.SetStatusStore(DeliStore.ActMode.Delivery, DeliStore.InputWay.None);
            App.DS.UpdateDelivery = true;
            uDelivery.Initial();
            uStore.Visibility = Visibility.Collapsed;
            uDelivery.Visibility = Visibility.Visible;
            uSimu.Visibility = Visibility.Collapsed;
            uDelivery.btnConfidence.Visibility = App.DS.OP.bSoteriaSet ? Visibility.Visible : Visibility.Collapsed;
            Lifter_Status_Change($"{App.SysPara.EqID}", "Delivery");
        }
        public void Lifter_Status_Change(string lifter, string mode)
        {
            string sql = $"UPDATE [STATUS] SET MODE='{mode}' WHERE NAME='{lifter}'";
            App.Local_SQLServer.NonQuery(sql);
        }
        private void rbtnSimu_Click(object sender, RoutedEventArgs e)
        {
            uStore.Visibility = Visibility.Collapsed;
            uDelivery.Visibility = Visibility.Collapsed;
            uSimu.Visibility = Visibility.Visible;
        }

        public void ReqIT_Event()
        {
            ucFrame Frame = App.DS.NowReqFrame;
            switch (Frame)
            {
                case ucFrame.StoreManual:
                    uStore.ucManual.btnConfirm.IsEnabled = false;
                    App.DS.bStoreAutoBtn = false;
                    App.DS.bWaitAck_DeliStore = false;
                    //rbtnDelivery.IsEnabled = false;
                    break;
                case ucFrame.StoreAuto:
                    App.DS.bStoreManualBtn = false;
                    App.DS.bWaitAck_DeliStore = false;
                    //rbtnDelivery.IsEnabled = false;
                    break;
                case ucFrame.NormalDelivery:
                    uDelivery.ucNormal.btnPickUp.IsEnabled = false;
                    uDelivery.ucNormal.btnConfirm.IsEnabled = false;
                    uDelivery.ucNormal.btnRefresh.IsEnabled = false;
                    uDelivery.btnIssue.IsEnabled = false;
                    uDelivery.btnProb.IsEnabled = false;
                    uDelivery.btnConfidence.IsEnabled = false;
                    App.DS.bWaitAck_DeliStore = false;
                    //rbtnStore.IsEnabled = false;
                    break;
                case ucFrame.OrderDelivery:
                    if (App.DS.bAskOrderNo)
                    {
                        uDelivery.ucIssue.txOrderNo.IsReadOnly = true;
                        uDelivery.ucIssue.btnFind.IsEnabled = false;
                    }
                    else
                    {
                        uDelivery.ucIssue.txBatchNo.IsReadOnly = true;
                        uDelivery.ucIssue.btnPickUp.IsEnabled = false;
                        uDelivery.ucIssue.btnConfirm.IsEnabled = false;
                    }
                    uDelivery.ucIssue.btnRefresh.IsEnabled = false;
                    uDelivery.btnNormal.IsEnabled = false;
                    uDelivery.btnProb.IsEnabled = false;
                    uDelivery.btnConfidence.IsEnabled = false;
                    App.DS.bWaitAck_DeliStore = false;
                    //rbtnStore.IsEnabled = false;
                    break;
                case ucFrame.ProbDelivery:
                    uDelivery.ucProb.txBatchNo.IsReadOnly = true;
                    uDelivery.ucProb.btnPickUp.IsEnabled = false;
                    uDelivery.ucProb.btnConfirm.IsEnabled = false;
                    uDelivery.ucProb.btnRefresh.IsEnabled = false;
                    uDelivery.btnNormal.IsEnabled = false;
                    uDelivery.btnIssue.IsEnabled = false;
                    uDelivery.btnConfidence.IsEnabled = false;
                    App.DS.bWaitAck_DeliStore = false;
                    //rbtnStore.IsEnabled = false;
                    break;
                case ucFrame.SecretDelivery:
                    if (uDelivery.ucConf.IsBatchNo)
                    {
                        uDelivery.ucConf.txBatchNo.IsReadOnly = true;

                    }
                    if (App.DS.bAskOrderNo)
                    {
                        uDelivery.ucConf.txOrderNo.IsReadOnly = true;
                        uDelivery.ucConf.btnFind.IsEnabled = false;
                    }
                    uDelivery.ucConf.rdBatchNo.IsEnabled = false;
                    uDelivery.ucConf.rdOrderNo.IsEnabled = false;
                    uDelivery.ucConf.btnPickUp.IsEnabled = false;
                    uDelivery.ucConf.btnConfirm.IsEnabled = false;
                    uDelivery.ucConf.btnRefresh.IsEnabled = false;
                    uDelivery.btnNormal.IsEnabled = false;
                    uDelivery.btnIssue.IsEnabled = false;
                    uDelivery.btnProb.IsEnabled = false;
                    App.DS.bWaitAck_DeliStore = false;
                    //rbtnStore.IsEnabled = false;
                    break;
            }
            App.DS.NowReqFrame = ucFrame.None;
        }

        public void ITAck_Event()
        {
            ucFrame Frame = App.DS.NowActFrame;
            switch (Frame)
            {
                case ucFrame.StoreManual:
                    uStore.ucManual.GetITReply_Event();
                    App.DS.bStoreAutoBtn = true;
                    App.DS.bWaitAck_DeliStore = true;
                    break;
                case ucFrame.StoreAuto:
                    uStore.ucAuto.GetITReply_Event();
                    App.DS.bStoreManualBtn = true;
                    App.DS.bWaitAck_DeliStore = true;
                    break;
                case ucFrame.NormalDelivery:
                    uDelivery.ucNormal.GetITReply_Event();
                    if (!App.DS.Delivery_HasReq)
                    {
                        //IT 已經回覆完所有出庫要求
                        uDelivery.ucNormal.btnPickUp.IsEnabled = true;
                        uDelivery.ucNormal.btnConfirm.IsEnabled = true;
                        uDelivery.ucNormal.btnRefresh.IsEnabled = true;
                        uDelivery.ucNormal.bConfirmClick = false;
                        uDelivery.btnIssue.IsEnabled = true;
                        uDelivery.btnProb.IsEnabled = true;
                        uDelivery.btnConfidence.IsEnabled = true;
                        App.DS.bWaitAck_DeliStore = true;
                        //rbtnStore.IsEnabled = true;
                    }
                    break;
                case ucFrame.OrderDelivery:
                    if (App.DS.bAskOrderNo)
                    {
                        uDelivery.ucIssue.GetOrderNoReply_Event();
                        break;
                    }
                    if (App.DS.Delivery_HasReq)
                    {
                        uDelivery.ucIssue.GetITReply_Event();
                        break;
                    }
                    else
                    {
                        //IT 已經回覆完所有出庫要求
                        App.DS.ASE_Orderno = new List<BatchList>();
                        uDelivery.ucIssue.dataGrid1.ItemsSource = App.DS.ASE_Orderno;
                        uDelivery.ucIssue.txOrderNo.Text = "";
                        uDelivery.ucIssue.txOrderNo.IsReadOnly = false;
                        uDelivery.ucIssue.btnPickUp.IsEnabled = false;
                        uDelivery.ucIssue.txBatchNo.IsEnabled = false;
                        uDelivery.ucIssue.txBatchNo.Text = "";
                        uDelivery.ucIssue.btnFind.IsEnabled = true;
                        uDelivery.ucIssue.btnConfirm.IsEnabled = false;
                        uDelivery.ucIssue.btnRefresh.IsEnabled = false;
                        uDelivery.btnNormal.IsEnabled = true;
                        uDelivery.btnProb.IsEnabled = true;
                        uDelivery.btnConfidence.IsEnabled = true;
                        App.DS.bWaitAck_DeliStore = true;
                        //rbtnStore.IsEnabled = true;
                    }
                    break;
                case ucFrame.ProbDelivery:
                    uDelivery.ucProb.GetITReply_Event();
                    if (!App.DS.Delivery_HasReq)
                    {
                        //IT 已經回覆完所有出庫要求
                        uDelivery.ucProb.txBatchNo.IsReadOnly = false;
                        uDelivery.ucProb.txBatchNo.Text = "";
                        uDelivery.ucProb.btnPickUp.IsEnabled = true;
                        uDelivery.ucProb.btnConfirm.IsEnabled = true;
                        uDelivery.ucProb.btnRefresh.IsEnabled = true;
                        uDelivery.btnNormal.IsEnabled = true;
                        uDelivery.btnIssue.IsEnabled = true;
                        uDelivery.btnConfidence.IsEnabled = true;
                        App.DS.bWaitAck_DeliStore = true;
                        //rbtnStore.IsEnabled = true;
                    }
                    break;
                case ucFrame.SecretDelivery:
                    if (App.DS.bAskOrderNo)
                    {
                        uDelivery.ucConf.GetOrderNoReply_Event();
                        break;
                    }
                    if (App.DS.Delivery_HasReq)
                    {
                        uDelivery.ucConf.GetITReply_Event();
                        break;
                    }
                    if (!App.DS.Delivery_HasReq)
                    {
                        if (uDelivery.ucConf.IsBatchNo)
                        {
                            uDelivery.ucConf.txBatchNo.IsReadOnly = false;
                            uDelivery.ucConf.btnPickUp.IsEnabled = true;
                            uDelivery.ucConf.btnFind.IsEnabled = false;
                            uDelivery.ucConf.btnConfirm.IsEnabled = true;
                            uDelivery.ucConf.btnRefresh.IsEnabled = true;
                            uDelivery.ucConf.rdOrderNo.IsEnabled = true;
                        }
                        else
                        {
                            uDelivery.ucConf.txBatchNo.IsReadOnly = true;
                            uDelivery.ucConf.btnPickUp.IsEnabled = false;
                            uDelivery.ucConf.btnFind.IsEnabled = true;
                            uDelivery.ucConf.btnConfirm.IsEnabled = false;
                            uDelivery.ucConf.btnRefresh.IsEnabled = false;
                            uDelivery.ucConf.rdBatchNo.IsEnabled = true;
                            uDelivery.ucConf.dataGrid1.ItemsSource = null;
                        }
                        //IT 已經回覆完所有出庫要求
                        uDelivery.btnNormal.IsEnabled = true;
                        uDelivery.btnIssue.IsEnabled = true;
                        uDelivery.btnProb.IsEnabled = true;
                        App.DS.bWaitAck_DeliStore = true;
                        //rbtnStore.IsEnabled = true;
                    }
                    break;

            }
        }
        public void OrderNoReplyNG()
        {
            ucFrame Frame = App.DS.NowActFrame;
            switch (Frame)
            {
                case ucFrame.OrderDelivery:
                    //IT 已經回覆完所有出庫要求
                    App.DS.ASE_Orderno = new List<BatchList>();
                    uDelivery.ucIssue.txOrderNo.Text = "";
                    uDelivery.ucIssue.dataGrid1.ItemsSource = App.DS.ASE_Orderno;
                    uDelivery.ucIssue.txOrderNo.IsReadOnly = false;
                    uDelivery.ucIssue.btnPickUp.IsEnabled = false;
                    uDelivery.ucIssue.txBatchNo.IsEnabled = false;
                    uDelivery.ucIssue.txBatchNo.Text = "";
                    uDelivery.ucIssue.btnFind.IsEnabled = true;
                    uDelivery.ucIssue.btnConfirm.IsEnabled = false;
                    uDelivery.btnNormal.IsEnabled = true;
                    uDelivery.btnProb.IsEnabled = true;
                    uDelivery.btnConfidence.IsEnabled = true;
                    App.DS.bWaitAck_DeliStore = true;
                    //rbtnStore.IsEnabled = true;
                    break;
                case ucFrame.SecretDelivery:
                    uDelivery.ucConf.txBatchNo.IsReadOnly = true;
                    uDelivery.ucConf.btnPickUp.IsEnabled = false;
                    uDelivery.ucConf.btnFind.IsEnabled = true;
                    uDelivery.ucConf.btnConfirm.IsEnabled = false;
                    uDelivery.ucConf.rdBatchNo.IsEnabled = true;
                    uDelivery.btnNormal.IsEnabled = true;
                    uDelivery.btnIssue.IsEnabled = true;
                    uDelivery.btnProb.IsEnabled = true;
                    App.DS.bWaitAck_DeliStore = true;
                    //rbtnStore.IsEnabled = true;
                    uDelivery.ucConf.dataGrid1.ItemsSource = null;
                    break;
            }
            App.DS.bAskOrderReplyNG = false;
        }

        public void RenewDeliDataGrid()
        {
            uDelivery.ucNormal.dataGrid1.DataContext = App.DS;
        }

        public void OfflineViewSet(bool bOnline_)
        {
            if (bOnline_)
            {
                if (App.DS.bSetBtn_LifterTransOnline)
                {
                    uDelivery.btnIssue.IsEnabled = true;
                    App.DS.bSetBtn_LifterTransOnline = false;
                }
                uDelivery.ucConf.rdOrderNo.IsEnabled = true;
            }
            else
            {
                uDelivery.btnIssue.IsEnabled = false;
                uDelivery.ucConf.rdOrderNo.IsEnabled = false;
                uDelivery.ucConf.rdBatchNo.IsChecked = true;
                uDelivery.ucConf.btnFind.IsEnabled = false;
                uDelivery.ucConf.txOrderNo.IsEnabled = false;
            }
        }

        public void StoreNGchangeToManual()
        {
            uStore.ucAuto.Visibility = Visibility.Collapsed;
            uStore.ucManual.Visibility = Visibility.Visible;
            App.DS.CIMMessage("StoreAutoNG", "自動入庫讀取BatchNo失敗，切成手動入庫");
            App.DS.bStoreAutoBtn = false;
            App.Eq.bStoreNGchangeToManual = false;
        }
    }
}
