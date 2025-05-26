using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CIM.BC.View
{
    /// <summary>
    /// Interaction logic for fmDelivery.xaml
    /// </summary>
    public partial class uDeliConfidence : UserControl
    {
        private bool bFindBatchNo = false;
        public bool IsBatchNo = true;
        private List<BatchList> delisearch = new List<BatchList>();
        private bool bConfirmClick = false;
        private bool bUnSearch = true;
        private List<string> BoxIDs = new List<string>();

        public uDeliConfidence()
        {
            InitializeComponent();
            btnFind.IsEnabled = false;
        }
        public void Initial()
        {
            delisearch = App.DS.DeliList_Conf;
            ITreply.DataContext = App.DS.Itreply;
        }

        private void rdBatchNo_Click(object sender, RoutedEventArgs e)
        {
            if (rdBatchNo.IsChecked == true)
            {
                txBatchNo.IsEnabled = true;
                txBatchNo.IsReadOnly = false;
                txOrderNo.IsEnabled = false;
                txOrderNo.Text = "";
                btnConfirm.IsEnabled = true;
                btnPickUp.IsEnabled = true;
                btnFind.IsEnabled = false;
                IsBatchNo = true;
                foreach(BatchList bl in App.DS.DeliList_Conf)
                {
                    bl.bIsChecked = false;
                }
                delisearch = App.DS.DeliList_Conf;
                dataGrid1.ItemsSource = delisearch;
            }
        }

        private void rdOrderNo_Click(object sender, RoutedEventArgs e)
        {
            if (rdOrderNo.IsChecked == true)
            {
                txBatchNo.IsEnabled = false;
                txOrderNo.IsEnabled = true;
                txOrderNo.IsReadOnly = false;
                txBatchNo.Text = "";
                btnConfirm.IsEnabled = false;
                btnPickUp.IsEnabled = false;
                btnFind.IsEnabled = true;
                IsBatchNo = false;
                delisearch = new List<BatchList>();
                dataGrid1.ItemsSource = delisearch;
            }
        }
        private void btnRefresh_Click(object sender,RoutedEventArgs e)
        {
            App.DS.bUpdateDeliFame = true;
            if (rdOrderNo.IsChecked==true)
            {
                App.DS.OrderNoSelect(txOrderNo.Text.Trim(), "S");
                dataGrid1.ItemsSource=App.DS.ASE_Orderno;
            }
            else if(rdBatchNo.IsChecked==true)
            {
                delisearch = App.DS.DeliList_Conf;
                dataGrid1.ItemsSource = delisearch;
            }
            
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bConfirmClick = true;
            bool bCheckedItemExsit = false;
            if (bUnSearch)
            {
                if (IsBatchNo)
                    delisearch = App.DS.DeliList_Conf;
                else
                    delisearch = App.DS.DeliList_Orderno;
            }
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
                deli.bCheckBoxWork = false;
                App.DS.bCheckBoxWork = false;
            }

            if (!bCheckedItemExsit)
            {
                App.DS.CIMMessage("Secret Delivery Message", "請勾選項目");
                foreach (BatchList deli in delisearch)
                {
                    deli.bCheckBoxWork = true;
                }
                App.DS.bCheckBoxWork = true;
                bConfirmClick = false;
                return;
            }
            else
            {
                App.DS.Delivery_HasReq = true;
                App.DS.NowDeliFrame = DeliStore.ucFrame.SecretDelivery;
            }
        }

        private void DataGridChecked(object sender, RoutedEventArgs e)
        {
            if (!bConfirmClick)
            {
                App.DS.UpdateDelivery = false;
                if (bUnSearch)
                {
                    if (IsBatchNo)
                    {
                        foreach (BatchList deli in App.DS.DeliList_Conf)
                        {
                            deli.bIsChecked = true;
                        }
                        dataGrid1.ItemsSource = App.DS.DeliList_Conf;
                    }
                    else
                    {
                        foreach (BatchList deli in App.DS.DeliList_Orderno)
                        {
                            deli.bIsChecked = true;
                        }
                        dataGrid1.ItemsSource = App.DS.DeliList_Orderno;
                    }
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

        private void DataGridUnchecked(object sender, RoutedEventArgs e)
        {
            if (!bConfirmClick)
            {
                App.DS.UpdateDelivery = false;
                if (bUnSearch)
                {
                    if (IsBatchNo)
                    {
                        foreach (BatchList deli in App.DS.DeliList_Conf)
                        {
                            deli.bIsChecked = false;
                        }
                        dataGrid1.ItemsSource = App.DS.DeliList_Conf;
                    }
                    else
                    {
                        foreach (BatchList deli in App.DS.DeliList_Orderno)
                        {
                            deli.bIsChecked = false;
                        }
                        dataGrid1.ItemsSource = App.DS.DeliList_Orderno;
                    }
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

        private void btnPickUp_Click(object sender, RoutedEventArgs e)
        {
            App.DS.UpdateDelivery = false;
            if (IsBatchNo)
            {
                bUnSearch = true;
            }
            string sBatchNo = txBatchNo.Text.Trim();
            bFindBatchNo = false;
            if (sBatchNo != "")
            {
                delisearch = new List<BatchList>();
                foreach (BatchList Deli in App.DS.DeliList_Conf)
                {
                    if (Deli.sBatchNo.Contains(sBatchNo))
                    {
                        bFindBatchNo = true;
                        delisearch.Add(Deli);
                    }
                }
                dataGrid1.ItemsSource = delisearch;
                if (!bFindBatchNo)
                {
                    App.DS.CIMMessage("Secret Delivery Message", "輸入資料不存在");
                    return;
                }
                else
                {
                    bUnSearch = false;
                }
            }
            else
            {
                delisearch = App.DS.DeliList_Conf;
                dataGrid1.ItemsSource = delisearch;
            }
        }
        public void GetOrderNoReply_Event()
        {
            App.DS.bAskOrderNo = false;
            bool bResult = App.DS.Itreply.OKorNG == "OK" ? true : false;

            if (bResult)
            {
                dataGrid1.ItemsSource = null;
                App.DS.OrderNoSelect(txOrderNo.Text.Trim(), "S");
                dataGrid1.ItemsSource = App.DS.DeliList_Orderno;
                btnPickUp.IsEnabled = false;
                btnFind.IsEnabled = false;
                btnConfirm.IsEnabled = true;
            }
            else
            {
                App.DS.bAskOrderReplyNG = true;
                App.DS.NowActFrame = DeliStore.ucFrame.SecretDelivery;
            }
            App.DS.Itreply.Clear();
        }
        public void GetITReply_Event()
        {
            bool bResult = App.DS.Itreply.OKorNG == "OK" ? true : false;
            string sBoxID = App.DS.Itreply.BoxID;

            foreach (BatchList deli in App.DS.DeliList_Conf)
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
                deli.bCheckBoxWork = true;
            }
            App.DS.Itreply.Clear();
            App.DS.DeliWaitAck = false;
        }

        private void txBatchNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txBatchNo.Text != "")
            {
                delisearch = new List<BatchList>();
            }
            else
            {
                delisearch = App.DS.DeliList_Conf;
            }
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            App.DS.UpdateDelivery = false;
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            if (txOrderNo.Text == "")
            {
                App.DS.CIMMessage("Secret Delivery Message", "請填寫領料單號");
            }
            else
            {
                App.SQL_HS.Req("OrderNo", txOrderNo.Text, DeliStore.ucFrame.SecretDelivery);
                App.DS.NowReqFrame = DeliStore.ucFrame.SecretDelivery;
                App.DS.bAskOrderNo = true;
            }
        }
    }
}