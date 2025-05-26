using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CIM.BC.View
{
    /// <summary>
    /// Interaction logic for fmDelivery.xaml
    /// </summary>
    public partial class uStoreManual : UserControl
    {
        string ReqData;
        public string Frame = "";
        private List<BatchList> AllList = new List<BatchList>();
        private BatchList BatchIn = new BatchList();

        public uStoreManual()
        {
            InitializeComponent();
        }

        public void Initial()
        {
            ITreply.DataContext = App.DS.Itreply;
            txESBID.DataContext = App.Eq.storeEq;
            txOperatorID.DataContext = App.DS.OP;
            btnConfirm.IsEnabled = false;
        }

        public void GetITReply_Event()
        {

            if (AllList.Count != 0)
                App.Bc.Req_Resp_OnOff(App.Bc.bCPC["ManualStockInStartRpt"], true, true);


            AllList = new List<BatchList>();
            dataGrid1.ItemsSource = null;
            dataGrid1.ItemsSource = AllList;
            App.DS.Itreply.Clear();
            App.Eq.storeEq.Clear_Box_BatchNo();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (AllList.Count > 0)
            {
                ReqData = AllList[0].sESB + "," + App.DS.OP.UserName; // ReqData=BoxID
                foreach (BatchList batch in AllList)
                {
                    App.DS.InsertStoreBatchList(batch);
                }
                App.SQL_HS.Req("Store", ReqData, DeliStore.ucFrame.StoreManual);
                App.DS.NowReqFrame = DeliStore.ucFrame.StoreManual;
                txBatchNo.Text = "";
                txESBID.Text = "";
                return;
            }
            App.DS.CIMMessage("Store Manual Message", "請讀取Batch No.");
            
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txBatchNo.Text.Trim().Contains("ERROR"))
            {
                App.LogFile.AddString("txBathcnNo = " + txBatchNo.Text.Trim());
                return;
            }
            else if(txBatchNo.Text.Trim()=="" || txESBID.Text.Trim() == "" )//|| txBatchNo.DataContext.ToString().Trim() == "" || txESBID.DataContext.ToString().Trim() == ""

            {
                App.LogFile.AddString("txBathcnNo = " + txBatchNo.Text.Trim()+", txESBID = "+txESBID.Text.Trim());
                App.DS.CIMMessage("Store Manual Message", "請檢查Batch No與BoxID是否有輸入完整");
                return;
            }
            if(txBatchNo.Text.Trim().ToUpper()!="EMPTY")
                BatchIn = App.DS.StoreBatchNo(txESBID.Text.Trim(), txBatchNo.Text.Trim());
			else
			{
                BatchIn.sESB = txESBID.Text.Trim();
                BatchIn.sBatchNo = "EMPTY";
			}
            if (BatchIn.sBatchNo != "")
            {
                foreach (BatchList batchList in AllList)
                {
                    if (batchList.sBatchNo == BatchIn.sBatchNo)
                    {
                        AllList.Remove(batchList);
                        dataGrid1.ItemsSource = null;
                        dataGrid1.ItemsSource = AllList;
                        txBatchNo.Text = "";
                        txESBID.Text = "";
                        return;
                    }
                }
                AllList.Add(BatchIn);
                btnConfirm.IsEnabled = true;
                dataGrid1.ItemsSource = null;
                dataGrid1.ItemsSource = AllList;
                txBatchNo.Text = "";
                txESBID.Text = "";
            }
            txBatchNo.IsReadOnly = false;
        }
        private void btnEmpty_Click(object sender,RoutedEventArgs e)
		{
            txBatchNo.Text = "EMPTY";
            txBatchNo.IsReadOnly = true;
		}
        private void btnDelet_Click(object sender, RoutedEventArgs e)
        {
            BatchIn = App.DS.StoreBatchNo(txESBID.Text.Trim(), txBatchNo.Text.Trim());
            if (BatchIn.sBatchNo != "")
            {
                foreach (BatchList batchList in AllList)
                {
                    if (batchList.sBatchNo == BatchIn.sBatchNo)
                    {
                        AllList.Remove(batchList);
                        dataGrid1.ItemsSource = null;
                        dataGrid1.ItemsSource = AllList;
                        txBatchNo.Text = "";
                        return;
                    }
                }
                App.DS.CIMMessage("Store Manual Message", "Batch No.不存在");
            }
            txBatchNo.IsReadOnly = false;
        }
    }
}