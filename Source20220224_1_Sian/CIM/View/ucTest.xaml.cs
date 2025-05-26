using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CIM.View
{
	/// <summary>
	/// ucTest.xaml 的互動邏輯
	/// </summary>
	public partial class ucTest : UserControl
	{
		public ucTest()
		{
			InitializeComponent();
            tbHub.DataContext = App.Bc.STK;
            tbHubPWD.DataContext = App.Bc.STK;
        }

		private void btn_Test1_Click(object sender, RoutedEventArgs e)
		{
			App.Bc.wAux["isOnlyDemo"].BinValue = App.Bc.wAux["isOnlyDemo"].BinValue == 0 ? (uint)1 : (uint)0;
		}

		private void btn_Test2_Click(object sender, RoutedEventArgs e)
		{
			BC.CAR_ACTION_CMD cmd = new BC.CAR_ACTION_CMD();
			App.Bc.STK.C010_CMD("123", cmd);
		}


		private void btn_Test3_Click(object sender, RoutedEventArgs e)
		{
			string vSendData = "{\"CMD_NO\": \"C001\",\"DATE\": \"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\",\"CMD\": \"ONLINE\"}";
			App.Bc.ShuttleCar1.SendCMD(vSendData);
		}
		#region shuttle Car

		private void btnShuttleCar_Click(object sender, RoutedEventArgs e)
		{
			BC.CAR_ACTION_CMD cmd = new BC.CAR_ACTION_CMD();
			cmd.ACTION = tbAction.Text.Trim();
			cmd.CAR_ID = tbCarID.Text.Trim();
			cmd.BOX_ID = tbBoxID.Text.Trim();
			cmd.BATCH_NO = tbBatchNo.Text.Trim();
			cmd.POSITION = tbPosition.Text.Trim();
			cmd.SOURCE = tbSource.Text.Trim();
			cmd.TARGET = tbTarget.Text.Trim();
			if (App.Bc.ShuttleCar1.bConnected)
				App.Bc.ShuttleCar1.C010_CMD(tbCmdID.Text.Trim(), cmd);
		}
        private void btnOnline_Click(object sender, RoutedEventArgs e)
        {
			if (App.Bc.ShuttleCar1.bConnected)
				App.Bc.ShuttleCar1.C001_CMD("ONLINE");
			/*
			if (App.Bc.ShuttleCar2.bConnected)
				App.Bc.ShuttleCar2.C001_CMD("ONLINE");
			*/
		}

        private void btnOffline_Click(object sender, RoutedEventArgs e)
		{
			if (App.Bc.ShuttleCar1.bConnected)
				App.Bc.ShuttleCar1.C001_CMD("OFFLINE");
			//if (App.Bc.ShuttleCar2.bConnected)
			//	App.Bc.ShuttleCar2.C001_CMD("OFFLINE");
		}

        private void btndelShuttleCar_Click(object sender, RoutedEventArgs e)
		{
			if (App.Bc.ShuttleCar1.bConnected)
				App.Bc.ShuttleCar1.C031_CMD(tbDeletCarID.Text.Trim(),tbDeletBoxID.Text.Trim());
        }
        #endregion shuttle Car

        #region Stoker
        private void btnSTKOnline_Click(object sender, RoutedEventArgs e)
        {
			App.Bc.STK.C001_CMD("ONLINE");
        }

        private void btnSTKOffline_Click(object sender, RoutedEventArgs e)
        {
			App.Bc.STK.C001_CMD("OFFLINE");
		}
        #endregion Stoker

        private void btnSTK_Click(object sender, RoutedEventArgs e)
        {
			BC.CAR_ACTION_CMD cmd = new BC.CAR_ACTION_CMD();
			cmd.ACTION = tbSTKAction.Text.Trim();
			cmd.CAR_ID = tbSTKCarID.Text.Trim();
			cmd.BOX_ID = tbSTKBoxID.Text.Trim();
			cmd.BATCH_NO = tbSTKBatchNo.Text.Trim();
			cmd.SOTERIA = tbSTKSoteria.Text.Trim();
			cmd.CUSTOMER_ID = tbSTKCustomer.Text.Trim();
			cmd.POSITION = tbSTKPosition.Text.Trim();
			cmd.SOURCE = tbSTKSource.Text.Trim();
			cmd.S_CELL_ID = tbSTKSCellID.Text.Trim();
			cmd.TARGET = tbSTKTarget.Text.Trim();
			cmd.T_CELL_ID = tbSTKTCellID.Text.Trim();
			App.Bc.STK.C010_CMD(tbSTKCMDID.Text.Trim(),cmd);
        }

        private void btnSTKC031_Click(object sender, RoutedEventArgs e)
        {
			App.Bc.STK.C031_CMD(tbSTKCarouselID.Text.Trim(),tbSTKCellID.Text.Trim());
		}

        private void btnSTKC050_Click(object sender, RoutedEventArgs e)
        {
			App.Bc.STK.C050_CMD(2);
		}

        private void btnSTKC020_Click(object sender, RoutedEventArgs e)
        {
			App.Bc.STK.C020_CMD(tbSTKCarouselID.Text, "START");
        }


		private void btn_Test4_Click(object sender, RoutedEventArgs e)
		{
			ASE_WebService.InParam param = new ASE_WebService.InParam();
			param.HandlerId = ASE_WebService.HandlerType.BinInReq;

			var task = WebService.CallWebService(param);
			task.ContinueWith(delegate
			{
				MessageBox.Show(task.Result.ReturnCode);
			});
		}

        private void btn_StockExistCheck_Request_Click(object sender, RoutedEventArgs e)
        {
			App.Bc.StockExistCheck_Request(tb_BOXID.Text, "");
        }

        private void btn_StockInComp_Report_Click(object sender, RoutedEventArgs e)
		{
			App.Bc.StockInComp_Report(tb_BOXID.Text);
		}

        private void btn_StockOutComp_Report_Click(object sender, RoutedEventArgs e)
		{
			App.Bc.StockOutComp_Report(tb_BOXID.Text);
		}

        private void btn_OrderNo_Request_Click(object sender, RoutedEventArgs e)
		{
			App.Bc.OrderNo_Request(tb_OrderNo.Text, tb_USERID.Text);
		}

        private void btn_OrderStockOutComp_Report_Click(object sender, RoutedEventArgs e)
		{
			App.Bc.OrderStockOutComp_Report(tb_BOXID.Text, tb_OrderNo.Text);
		}

        private void btn_InventoryComp_Report_Click(object sender, RoutedEventArgs e)
		{
			App.Bc.InventoryComp_Report(tb_CommandID.Text);
		}
    }
}
