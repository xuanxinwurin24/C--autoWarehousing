using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CIM.BC.View
{
	/// <summary>
	/// Interaction logic for fmDelivery.xaml
	/// </summary>
	public partial class uStoreAuto : UserControl
	{
		Button btnManual;
		RadioButton rdbtnDeli;
		string ReqData;
		string Refresh_Box;
		List<BatchList> AllList = new List<BatchList>();
		private BatchList BatchIn = new BatchList();
		public string Frame = "";
		public uStoreAuto()
		{
			InitializeComponent();
		}

		public void Initial(Button btn_, RadioButton rdbtn_)
		{
			ITreply.DataContext = App.DS.Itreply;
			btnManual = btn_;
			rdbtnDeli = rdbtn_;
			txESBID.DataContext = App.Eq.storeEq;
			txBatchNo.DataContext = App.Eq.storeEq;
			txOperatorID.DataContext = App.DS.OP;
		}

		public void GetITReply_Event()
		{
			AllList = new List<BatchList>();
			dataGrid1.ItemsSource = null;
			dataGrid1.ItemsSource = AllList;
			App.DS.Itreply.Clear();
			txESBID.DataContext = "";
			txBatchNo.DataContext = "";
			App.Eq.storeEq.Clear_Box_BatchNo();
		}

		private void Refresh_Click(object sender, RoutedEventArgs e)
		{
			txESBID.DataContext = "";
			txBatchNo.DataContext = "";
		}
	}
}