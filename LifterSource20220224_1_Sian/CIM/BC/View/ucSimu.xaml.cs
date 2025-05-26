using CIM.Lib.Model;
using CIM.Lib.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using static CIM.BC.BC;

namespace CIM.BC.View
{
    /// <summary>
    /// Interaction logic for fmDelivery.xaml
    /// </summary>
    public partial class ucSimu : System.Windows.Controls.UserControl
    {
        string BoxID = "BoxID";
        string BatchNo = "DUMPKG";
        string BatchNoMiddel = "||DUMPKG|82027|4161066||X|NA||DUM||N|1";
        int iboxid = 0;
        int ibatchno = 0;
        int iCnt = 0;
        List<string> BoxIDs = new List<string>();
        public ucSimu()
        {
            InitializeComponent();
            tbRFID.DataContext = App.RFIDReader;
        }
        private void btnDatetime_Click(object sender, RoutedEventArgs e)
        {
            App.Bc.Req_Resp_OnOff(App.Bc.bCPC["CPCDateTimeRpt"], true, true);
        }

        private void Auto_Click(object sender, RoutedEventArgs e)
        {
            add(1);
            string sBoxID = BoxID + iboxid.ToString().PadLeft(3, '0');
            string sBatchno = BatchNo + ibatchno.ToString().PadLeft(4, '0') + BatchNoMiddel + (ibatchno % 10).ToString();
            App.Eq.StockInReport["BoxID"].StringValue = sBoxID;
            App.Eq.StockInReport["FOSBBatchNo"].StringValue = sBatchno;
            iCnt = 0;
            BoxIDs.Add(sBoxID);
        }
        private void Manual_Click(object sender, RoutedEventArgs e)
        {
            if (iCnt == 0)
            {
                ++iboxid;
            }
            ++iCnt;
            add(1, true);
            string sBoxID = BoxID + iboxid.ToString().PadLeft(3, '0');
            string sBatchno = BatchNo + ibatchno.ToString().PadLeft(4, '0') + BatchNoMiddel + (ibatchno % 10).ToString();
            App.Eq.StockInReport["BoxID"].StringValue = sBoxID;
            App.Eq.StockInReport["FOSBBatchNo"].StringValue = sBatchno;
            BoxIDs.Add(sBoxID);
        }
        private void FilledBatchList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void add(int times, bool bManual = false)
        {
            if (bManual)
            {
                for (int i = 0; i < times; i++)
                {
                    ++ibatchno;
                }
            }
            else
            {
                for (int i = 0; i < times; i++)
                {
                    ++iboxid;
                    ++ibatchno;
                }
            }
        }

        private void Online_Click(object sender, RoutedEventArgs e)
        {
            bool bConnect = App.Local_SQLServer.ConnectToDB();
            if (App.Local_SQLServer.ConnectToDB())
            {
                string sql = $"IF EXISTS(SELECT * FROM WEBSERVICE_CONNECTION WHERE NAME LIKE '%CPC%')" +
                        $"UPDATE WEBSERVICE_CONNECTION SET STATUS = 'ONLINE' WHERE NAME LIKE '%CPC%'" +
                        $"ELSE INSERT INTO WEBSERVICE_CONNECTION(NAME,STATUS)VALUES('CPC', 'ONLINE')";
               int k = App.Local_SQLServer.NonQuery(sql);
            }
            else
            {
                App.DS.bLifterIsOnline = true;
            }
        }

        private void Offline_Click(object sender, RoutedEventArgs e)
        {
            if (App.Local_SQLServer.ConnectToDB())
            {
                string sql = $"IF EXISTS(SELECT * FROM WEBSERVICE_CONNECTION WHERE NAME LIKE '%CPC%')" +
                    $"UPDATE WEBSERVICE_CONNECTION SET STATUS = 'OFFLINE' WHERE NAME LIKE '%CPC%'" +
                    $"ELSE INSERT INTO WEBSERVICE_CONNECTION(NAME,STATUS)VALUES('CPC', 'OFFLINE')";
                App.Local_SQLServer.NonQuery(sql);
            }
            else
            {
                App.DS.bLifterIsOnline = false;
            }
            
        }

        private void Store_Click(object sender, RoutedEventArgs e)
        {
            App.Bc.SetCrownToLifter(storeBoxID.Text.Trim());
        }
    }
}
