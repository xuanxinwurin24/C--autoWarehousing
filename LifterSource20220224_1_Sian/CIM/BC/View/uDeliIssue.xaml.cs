using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace CIM.BC.View
{
    /// <summary>
    /// Interaction logic for fmDelivery.xaml
    /// </summary>
    public partial class uDeliIssue : UserControl
    {
        private bool bFindBatchNo = false;
        private List<BatchList> delisearch = new List<BatchList>();
        private List<string> BoxIDs = new List<string>();
        private bool bUnSearch = true;
        public bool bRefreshClick = false;
        public bool bConfirmClick = false;
        private int iCBenable;

        public uDeliIssue()
        {
            InitializeComponent();
            btnPickUp.IsEnabled = false;
            txBatchNo.IsEnabled = false;
            btnConfirm.IsEnabled = false;
        }

        public void Initial()
        {
            delisearch = App.DS.ASE_Orderno;
            ITreply.DataContext = App.DS.Itreply;
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            bRefreshClick = true;
            App.DS.bUpdateDeliFame = true;
            App.DS.OrderNoSelect(txOrderNo.Text.Trim(), "S");
            dataGrid1.ItemsSource = App.DS.ASE_Orderno;



        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bConfirmClick = true;
            btnRefresh.IsEnabled = false;
            bool bCheckedItemExsit = false;
            if (bUnSearch)
            {
                delisearch = App.DS.ASE_Orderno;
            }
            iCBenable = 0;
            foreach (BatchList deli in delisearch)
            {
                if (deli.bIsChecked)
                {
                    string sBoxID = deli.sESB;
                    if (!BoxIDs.Contains(sBoxID))
                    {
                        //沒重複BoxID
                        App.DS.DeliveryStep(deli.sESB, 1);
                    }
                    bCheckedItemExsit = true;
                }
                if (deli.bCheckBoxWork)
                {
                    ++iCBenable;
                }
                deli.bCheckBoxWork = false;
                App.DS.bCheckBoxWork = false;
            }
            if (!bCheckedItemExsit)
            {
                if (iCBenable == 0)
                {
                    App.DS.NowActFrame = DeliStore.ucFrame.OrderDelivery;
                    return;
                }
                App.DS.CIMMessage("Order Delivery Message", "請勾選項目");
                bConfirmClick = false;
                return;
            }
            else
            {
                App.DS.Delivery_HasReq = true;
                App.DS.NowDeliFrame = DeliStore.ucFrame.OrderDelivery;
            }
        }
        public void GetITReply_Event()
        {
            bool bResult = App.DS.Itreply.OKorNG == "OK" ? true : false;
            string sBoxID = App.DS.Itreply.BoxID;

            foreach (BatchList deli in App.DS.DeliList_NConf)
            {
                if (deli.sESB == sBoxID)
                {
                    if (bResult)
                    {
                        App.DS.DeliveryStep(deli.sESB, 3);
                    }
                    else
                    {
                        App.DS.DeliveryStep(deli.sESB, 0);
                        deli.bIsChecked = false;
                    }
                }
            }
            App.DS.Itreply.Clear();
            App.DS.DeliWaitAck = false;
        }

        private void DataGridChecked(object sender, RoutedEventArgs e)
        {
            if (App.DS.bAskOrderNo)
            {
                //NotWork
            }
            else
            {
                if (!bConfirmClick)
                {
                    App.DS.UpdateDelivery = false;
                    if (bUnSearch)
                    {
                        foreach (BatchList deli in App.DS.ASE_Orderno)
                        {
                            deli.bIsChecked = true;
                        }
                        dataGrid1.ItemsSource = App.DS.ASE_Orderno;
                    }
                    else
                    {
                        foreach (BatchList deli in delisearch)
                        {
                            deli.bIsChecked = true;
                        }
                        dataGrid1.ItemsSource = delisearch;
                    }
                }
            }
        }

        private void DataGridUnchecked(object sender, RoutedEventArgs e)
        {
            if (App.DS.bAskOrderNo)
            {
                //Not Work
            }
            else
            {
                if (!bConfirmClick)
                {
                    App.DS.UpdateDelivery = false;
                    if (bUnSearch)
                    {
                        foreach (BatchList deli in App.DS.ASE_Orderno)
                        {
                            deli.bIsChecked = false;
                        }
                        dataGrid1.ItemsSource = App.DS.ASE_Orderno;
                    }
                    else
                    {
                        foreach (BatchList deli in delisearch)
                        {
                            deli.bIsChecked = false;
                        }
                        dataGrid1.ItemsSource = delisearch;
                    }
                }

            }
        }

        private void btnPickUp_Click(object sender, RoutedEventArgs e)
        {
            App.DS.UpdateDelivery = false;
            string sBatchNo = txBatchNo.Text.Trim();
            bFindBatchNo = false;
            if (sBatchNo != "")
            {
                delisearch = new List<BatchList>();
                foreach (BatchList Deli in App.DS.ASE_Orderno)
                {
                    if (Deli.sBatchNo.Contains(sBatchNo))
                    {
                        bFindBatchNo = true;
                        delisearch.Add(Deli);
                    }
                }
                dataGrid1.ItemsSource = null;
                dataGrid1.ItemsSource = delisearch;
                if (!bFindBatchNo)
                {
                    App.DS.CIMMessage("Order Delivery Message", "輸入資料不存在");
                    return;
                }
                else
                {
                    bUnSearch = false;
                }
            }
            else
            {
                delisearch = App.DS.ASE_Orderno;
                dataGrid1.ItemsSource = delisearch;
            }
        }
        public void GetOrderNoReply_Event()
        {
            App.DS.bAskOrderNo = false;
            bool bResult = App.DS.Itreply.OKorNG == "OK" ? true : false;

            if (bResult)
            {
                App.DS.OrderNoSelect(txOrderNo.Text.Trim(), "N");
                dataGrid1.ItemsSource = App.DS.ASE_Orderno;
                txBatchNo.IsEnabled = true;
                btnPickUp.IsEnabled = true;
                btnConfirm.IsEnabled = true;
                btnRefresh.IsEnabled = true;
            }
            else
            {
                App.DS.bAskOrderReplyNG = true;
                App.DS.NowActFrame = DeliStore.ucFrame.OrderDelivery;
            }
            App.DS.Itreply.Clear();
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            if (txOrderNo.Text == "")
            {
                App.DS.CIMMessage("Order Delivery Message", "請填寫領料單號");
            }
            else
            {
                App.SQL_HS.Req("OrderNo", txOrderNo.Text + "," + App.DS.OP.UserName, DeliStore.ucFrame.OrderDelivery);
                App.DS.NowReqFrame = DeliStore.ucFrame.OrderDelivery;
                App.DS.bAskOrderNo = true;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            App.DS.UpdateDelivery = false;
        }

        private void txBatchNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txBatchNo.Text != "")
            {
                delisearch = new List<BatchList>();
            }
            else
            {
                delisearch = App.DS.ASE_Orderno;
            }
        }
    }
}
