using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
	public partial class ucRecover : UserControl
	{
		public List<string> STK_BoxID;
		public List<string> STK_BatchNo;
		public List<string> STK_Source_List;
		public List<string> STK_Source_Cell_List;
		public List<string> STK_Target_List;
		public List<string> STK_Target_Cell_List;
		public List<string> STK_ID_List;
		public List<string> Shuttle_BoxID;
		public List<string> Shuttle_BatchNo;
		public List<string> Shuttle_Point_List;
		public List<string> Shuttle_ID_List;
		public List<string> Car_Action;
		public List<string> Command_ID;
		public List<string> Soteria;
		public List<string> Customer;
		public ucRecover()
		{
			InitializeComponent();
            tbHub.DataContext = App.Bc.STK;
            tbHubPWD.DataContext = App.Bc.STK;

		}
		public void Recover_Initial()
        {
			STK_BoxID = new List<string>();
			STK_BatchNo = new List<string>();
			STK_Source_List = new List<string>();
			STK_Source_Cell_List = new List<string> { "" };
			STK_Target_List = new List<string>();
			STK_Target_Cell_List = new List<string> { "" };
			STK_ID_List = new List<string>();
			Shuttle_BoxID = new List<string>();
			Shuttle_BatchNo = new List<string>();
			Shuttle_Point_List = new List<string>();
			Shuttle_ID_List = new List<string>();
			Car_Action = new List<string>();
			Command_ID = new List<string>();
			Soteria = new List<string> { "" };
			Customer = new List<string> { "" };
			List_Initial();
			tbSTKAction.ItemsSource = Car_Action;
			tbSTKCarID.ItemsSource = STK_ID_List;
			tbSTKBoxID.ItemsSource = STK_BoxID;
			tbSTKBatchNo.ItemsSource = STK_BatchNo;
			tbSTKSoteria.ItemsSource = Soteria;
			tbSTKCustomer.ItemsSource = Customer;
			tbSTKSource.ItemsSource = STK_Source_List;
			tbSTKSCellID.ItemsSource = STK_Source_Cell_List;
			tbSTKTarget.ItemsSource = STK_Target_List;
			tbSTKCMDID.ItemsSource = Command_ID;

			tbCmdID.ItemsSource = Command_ID;
			tbAction.ItemsSource = Car_Action;
			tbCarID.ItemsSource = Shuttle_ID_List;
			tbBoxID.ItemsSource = Shuttle_BoxID;
			tbBatchNo.ItemsSource = Shuttle_BatchNo;
			tbSource.ItemsSource = Shuttle_Point_List;
			tbTarget.ItemsSource = Shuttle_Point_List;
			string sSQL= "SELECT * FROM [TASK] WHERE STATUS='99'";
			DataTable dt = new DataTable();
			App.Local_SQLServer.Query(sSQL, ref dt);
            if (dt.Rows.Count == 0)
            {
				btnShuttleCar.IsEnabled = false;
				btnSTK.IsEnabled = false;
            }

		}
		public void List_Initial()
        {
			string sSQL = string.Empty;
			DataTable dt_temp = new DataTable();
			string[] Shuttle_Point = { "LIFTER_A", "LIFTER_B", "STAGE1", "STAGE2", "STAGE3", "STAGE4", "STAGE5", "STAGE6", "STAGE7", "EXCHANGE" };
			sSQL = "SELECT CAR_ID FROM [CAR_STATUS] ORDER BY 1";
			App.STK_SQLServer.Query(sSQL, ref dt_temp);
			foreach(DataRow dr_temp in dt_temp.Rows)
            {
				STK_ID_List.Add(dr_temp["CAR_ID"].ToString().Trim());
            }
			for(int i = 2; i < 9; i++)
            {
				STK_Source_List.Add(Shuttle_Point[i]);
				STK_Target_List.Add(Shuttle_Point[i]);
			}
			sSQL = "SELECT * FROM [TASK] WHERE STATUS='99'";
			App.Local_SQLServer.Query(sSQL, ref dt_temp);
			foreach(DataRow dr_temp in dt_temp.Rows)
            {
				STK_BoxID.Add(dr_temp["BOX_ID"].ToString().Trim());
				STK_BatchNo.Add(dr_temp["BATCH_NO"].ToString().Trim());
				Shuttle_BoxID.Add(dr_temp["BOX_ID"].ToString().Trim());
				Shuttle_BatchNo.Add(dr_temp["BATCH_NO"].ToString().Trim());
				Command_ID.Add(dr_temp["COMMAND_ID"].ToString().Trim());
				Customer.Add(dr_temp["CUSTOMER_ID"].ToString().Trim());
				if (dr_temp["SRC_POS"].ToString().Trim() != "LIFTER_A" && dr_temp["SRC_POS"].ToString().Trim() != "LIFTER_B" && dr_temp["SRC_POS"].ToString().Trim() != "")
					STK_Source_List.Add(dr_temp["SRC_POS"].ToString().Trim());
				if (dr_temp["TAR_POS"].ToString().Trim() != "LIFTER_A" && dr_temp["TAR_POS"].ToString().Trim() != "LIFTER_B" && dr_temp["TAR_POS"].ToString().Trim() != "")
					STK_Source_List.Add(dr_temp["TAR_POS"].ToString().Trim());
                if (dr_temp["SRC_CELL_ID"].ToString().Trim()!="")
					STK_Source_Cell_List.Add(dr_temp["SRC_CELL_ID"].ToString().Trim());
				if(dr_temp["TAR_CELL_ID"].ToString().Trim() != "")
					STK_Source_Cell_List.Add(dr_temp["TAR_CELL_ID"].ToString().Trim());
			}
			sSQL = "SELECT CAROUSEL_ID FROM [CAROUSEL_STATUS] WHERE STATUS!='5' ORDER BY 1";
			App.STK_SQLServer.Query(sSQL, ref dt_temp);
			foreach (DataRow dr_temp in dt_temp.Rows)
            {
				STK_Target_List.Add(dr_temp["CAROUSEL_ID"].ToString().Trim());
            }

			foreach(string Point in Shuttle_Point)
            {
				Shuttle_Point_List.Add(Point);
            }
			Shuttle_ID_List.Add("CAR001");
			Shuttle_ID_List.Add("CAR002");
			Car_Action.Add("TRANSFER");
			Car_Action.Add("MOVE");
			Car_Action.Add("CANCEL");
			Soteria.Add("Y");
			Soteria.Add("N");
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
			DataTable dt_FindTaskTarget = App.Local_SQLServer.SelectDB("*", "[TASK]", $"[TAR_POS] = '{tbSTKTarget.Text.Trim()}' AND [TAR_CELL_ID] = '{tbSTKTCellID.Text.Trim()}'");
			if (dt_FindTaskTarget.Rows.Count != 0) 
			{
				MessageBox.Show("有任務指定該儲格 請使用其他儲格");
				return;
			}
			string tbSTKSource_Carousel = tbSTKSource.Text.Trim();
			string tbSTKTarget_Carousel = tbSTKTarget.Text.Trim();
			if(tbSTKSource_Carousel.Contains("STAGE"))
				tbSTKSource_Carousel= "B-00" + tbSTKSource_Carousel.Substring(tbSTKSource_Carousel.Length - 1);
			if (tbSTKTarget_Carousel.Contains("STAGE"))
				tbSTKTarget_Carousel = "B-00" + tbSTKTarget_Carousel.Substring(tbSTKTarget_Carousel.Length - 1);
			BC.CAR_ACTION_CMD cmd = new BC.CAR_ACTION_CMD();
			cmd.ACTION = tbSTKAction.Text.Trim();
			cmd.CAR_ID = tbSTKCarID.Text.Trim();
			cmd.BOX_ID = tbSTKBoxID.Text.Trim();
			cmd.BATCH_NO = tbSTKBatchNo.Text.Trim();
			cmd.SOTERIA = tbSTKSoteria.Text.Trim();
			cmd.CUSTOMER_ID = tbSTKCustomer.Text.Trim();
			cmd.POSITION = tbSTKPosition.Text.Trim();
			cmd.SOURCE = tbSTKSource_Carousel;
			cmd.S_CELL_ID = tbSTKSCellID.Text.Trim();
			cmd.TARGET = tbSTKTarget_Carousel;
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

        private void tbSTKTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			string STK_Target=tbSTKTarget.SelectedItem.ToString().Trim();
			string sSQL = $"SELECT DISTINCT CELL_ID FROM [CELL_STATUS] WHERE STATUS='0' AND CAROUSEL_ID='{STK_Target}'  ORDER BY 1";
			DataTable dt_temp=new DataTable();
			App.STK_SQLServer.Query(sSQL, ref dt_temp);
			foreach (DataRow dr_temp in dt_temp.Rows)
			{
				STK_Target_Cell_List.Add(dr_temp["CELL_ID"].ToString().Trim());
			}
			tbSTKTCellID.ItemsSource = STK_Target_Cell_List;
		}
    }
}
