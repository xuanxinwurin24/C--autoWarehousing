using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CIM.BC.View
{
    /// <summary>
    /// Interaction logic for fmDelivery.xaml
    /// </summary>

    public partial class uDeliNormal : UserControl
    {
        private bool bFindBatchNo = false;
        private List<BatchList> delisearch = new List<BatchList>();
        private List<string> BoxIDs = new List<string>();
        private bool bUnSearch = true;
        public bool bRefreshClick = true;
        public bool bConfirmClick = false;
        public uDeliNormal()
        {
            InitializeComponent();
        }

        public void Initial()
        { 
            ITreply.DataContext = App.DS.Itreply;
            delisearch = App.DS.DeliList_NConf;
        }
        private void btnEmpty_Click(object sender, RoutedEventArgs e)
		{
            App.DS.Delivery_HasReq = true;
            App.DS.NowReqFrame = DeliStore.ucFrame.NormalDelivery;
            App.DS.NowDeliFrame = DeliStore.ucFrame.NormalDelivery;
            App.DS.EmptyBoxOut();
		}
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            bRefreshClick = true;
            App.DS.bUpdateDeliFame = true;
            dataGrid1.ItemsSource = App.DS.DeliList_NConf;
        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bConfirmClick = true;
            bool bCheckedItemExsit = false;
            if (bUnSearch)
            {
                delisearch = App.DS.DeliList_NConf;
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
                //deli.bCheckBoxWork = false;
                App.DS.bCheckBoxWork = false;
            }

            if (!bCheckedItemExsit)
            {
                App.DS.CIMMessage("Normal Delivery Message", "請勾選項目");
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
                App.DS.NowReqFrame = DeliStore.ucFrame.NormalDelivery;
                App.DS.NowDeliFrame = DeliStore.ucFrame.NormalDelivery;
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
                        deli.bIsChecked = true;
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
                    foreach (BatchList deli in App.DS.DeliList_NConf)
                    {
                        deli.bIsChecked = true;
                    }
                    dataGrid1.ItemsSource = App.DS.DeliList_NConf;
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
                    foreach (BatchList deli in App.DS.DeliList_NConf)
                    {
                        deli.bIsChecked = false;
                    }
                    dataGrid1.ItemsSource = App.DS.DeliList_NConf;
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
                foreach (BatchList Deli in App.DS.DeliList_NConf)//
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
                    App.DS.CIMMessage("Normal Delivery Message", "輸入資料不存在");
                    return;
                }
                else
                {
                    bUnSearch = false;
                }
            }
            else
            {
                delisearch = App.DS.DeliList_NConf;
                dataGrid1.ItemsSource = delisearch;
            }
        }

        private void txBatchNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txBatchNo.Text.Trim() != "")
            {
                delisearch = new List<BatchList>();
            }
            else
            {
                delisearch = App.DS.DeliList_NConf;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            App.DS.UpdateDelivery = false;
        }
    }
}
