using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CIM.BC.View
{
    /// <summary>
    /// Interaction logic for fmDelivery.xaml
    /// </summary>
    public partial class uDeliProb : UserControl
    {
        private bool bFindBatchNo = false;
        private List<BatchList> delisearch = new List<BatchList>();
        private bool bRefreshClick = false;//以防之後無限刷新 事先宣告
        private bool bConfirmClick = false;
        private bool bUnSearch = true;
        private List<string> BoxIDs = new List<string>();
        public uDeliProb()
        {
            InitializeComponent();
        }

        public void Initial()
        {
            delisearch = App.DS.DeliSTK_Prob;
            ITreply.DataContext = App.DS.Itreply;
        }
        private void btnRefresh_Click(object sender,RoutedEventArgs e)
        {
            bRefreshClick = true;
            App.DS.UpdateDelivery = true;
            dataGrid1.ItemsSource = App.DS.DeliSTK_Prob;
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bConfirmClick = true;
            bool bCheckedItemExsit = false;
            if (bUnSearch)
            {
                delisearch = App.DS.DeliSTK_Prob;
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
                App.DS.CIMMessage("Abnormal Delivery Message", "請勾選項目");
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
                App.DS.NowDeliFrame = DeliStore.ucFrame.ProbDelivery;
            }
        }
        public void GetITReply_Event()
        {
            bool bResult = App.DS.Itreply.OKorNG == "OK" ? true : false;
            string sBoxID = App.DS.Itreply.BoxID;

            foreach (BatchList deli in App.DS.DeliSTK_Prob)
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
            if (!bConfirmClick)
            {
                App.DS.UpdateDelivery = false;
                if (bUnSearch)
                {
                    foreach (BatchList deli in App.DS.DeliSTK_Prob)
                    {
                        deli.bIsChecked = true;
                    }
                    dataGrid1.ItemsSource = App.DS.DeliSTK_Prob;
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
                    foreach (BatchList deli in App.DS.DeliSTK_Prob)
                    {
                        deli.bIsChecked = false;
                    }
                    dataGrid1.ItemsSource = App.DS.DeliSTK_Prob;
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
            string sBatchNo = txBatchNo.Text.Trim();
            bFindBatchNo = false;
            if (sBatchNo != "")
            {
                delisearch = new List<BatchList>();
                foreach (BatchList Deli in App.DS.DeliSTK_Prob)//
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
                    App.DS.CIMMessage("Abnormal Delivery Message", "輸入資料不存在");
                    return;
                }
                else
                {
                    bUnSearch = false;
                }
            }
            else
            {
                delisearch = App.DS.DeliSTK_Prob;
                dataGrid1.ItemsSource = delisearch;
            }
        }
        private void txBatchNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txBatchNo.Text != "")
            {
                delisearch = new List<BatchList>();
            }
            else
            {
                delisearch = App.DS.DeliSTK_Prob;
            }
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            App.DS.UpdateDelivery = false;
        }
    }
}
