using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Strong;
using System.Text.RegularExpressions;
using System.Windows;
using CIM.BC.View;

namespace CIM.BC
{
    public class DeliStore : INotifyPropertyChanged
    {
        public bool bBackToMainWindow = false;
        public ITreply Itreply = new ITreply();
        public bool UpdateDelivery = false; //更新DeliveryList
        public bool bAskOrderNo = false; //true: 正在詢問OrderNo; false:已經問到OrderNo
        public bool Delivery_HasReq = false;
        public bool DeliWaitAck = false;
        public DataTable dt_Carousel_Trans;
        public bool bUpdateDeliFame = false;
        public bool bAskOrderReplyNG = false; //OrderReplyNG;
        public bool bSetBtn_LifterTransOnline = false;
        public List<string> BoxIDInTask = new List<string>();
        public List<BatchList> StoreList = new List<BatchList>();
        public bool WebServiceDemo = false;
        LogWriter CimMsgLogFile = new LogWriter();
        LogWriter BoxInNGRetryLogFile = new LogWriter();
        public DeliStore()
        {
            dt_Carousel_Trans = App.Local_SQLServer.SelectDB("*", "[CAROUSEL_TRANSFER]", "");
            CimMsgLogFile.PathName = App.sSysDir + @"\LogFile\CimMsg\";
            CimMsgLogFile.sHead = "CimMsg";
            BoxInNGRetryLogFile.PathName= App.sSysDir + @"\LogFile\BoxInRetry\";
            BoxInNGRetryLogFile.sHead = "BoxInRetry";
        }
        ~DeliStore()
        { }
        public enum ActMode : uint
        {
            None = 0,
            Store = 1,
            Delivery
        }
        public enum InputWay : uint
        {
            None = 0,
            Manual,
            Auto
        }
        public enum ucFrame
        {
            None = 0, StoreManual = 1, StoreAuto,
            NormalDelivery, OrderDelivery, ProbDelivery, SecretDelivery,
            Simu
        };
        public ucFrame NowReqFrame = ucFrame.None;
        public ucFrame NowActFrame = ucFrame.None;
        public ucFrame NowDeliFrame = ucFrame.None;
        public ucFrame LastAckFrame = ucFrame.None;

        private Operator operator_deliStore = new Operator();
        public Operator OP
        {
            get { return operator_deliStore; }
            set
            {
                if (operator_deliStore == value) return;
                operator_deliStore = value;
                OnPropertyChanged();
            }
        }


        private List<BatchList> delilistnconf = new List<BatchList>();
        public List<BatchList> DeliList_NConf
        {
            get
            {
                return delilistnconf;
            }
            set
            {
                if (delilistnconf == value) return;
                delilistnconf = value;
                OnPropertyChanged();
            }
        }

        private List<BatchList> delilistconf = new List<BatchList>();

        public List<BatchList> DeliList_Conf
        {
            get
            {
                return delilistconf;
            }
            set
            {
                if (delilistconf == value) return;
                delilistconf = value;
                OnPropertyChanged();
            }
        }
        private List<BatchList> deliorderno = new List<BatchList>();

        public List<BatchList> DeliList_Orderno
        {
            get
            {
                return deliorderno;
            }
            set
            {
                if (deliorderno == value) return;
                deliorderno = value;
                OnPropertyChanged();
            }
        }
        private List<BatchList> aseorderlist = new List<BatchList>();

        public List<BatchList> ASE_Orderno
        {
            get
            {
                return aseorderlist;
            }
            set
            {
                if (aseorderlist == value) return;
                aseorderlist = value;
                OnPropertyChanged();
            }
        }
        private List<BatchList> deliprob = new List<BatchList>();

        public List<BatchList> DeliList_Prob
        {
            get
            {
                return deliprob;
            }
            set
            {
                if (deliprob == value) return;
                deliprob = value;
                OnPropertyChanged();
            }
        }
        private List<BatchList> delistkprob = new List<BatchList>();

        public List<BatchList> DeliSTK_Prob
        {
            get
            {
                return delistkprob;
            }
            set
            {
                if (delistkprob == value) return;
                delistkprob = value;
                OnPropertyChanged();
            }
        }
        private bool checkboxwork = true;
        public bool bCheckBoxWork
        {
            get { return checkboxwork; }
            set
            {
                if (checkboxwork == value) return;
                checkboxwork = value;
                OnPropertyChanged();
            }
        }
        private bool blifterisonline = false;
        public bool bLifterIsOnline
        {
            get { return blifterisonline; }
            set
            {
                if (blifterisonline == value) return;
                blifterisonline = value;
                if (blifterisonline)
                {
                    bSetBtn_LifterTransOnline = true;
                }
            }
        }
        public bool bliftertransstatus = false;
        public bool bLifterTransStatus
        {
            get { return bliftertransstatus; }
            set
            {
                if (bliftertransstatus == value) return;
                bliftertransstatus = value;
                if (bliftertransstatus != bLifterIsOnline)
                    DecideChangeLifterStatus();
            }
        }
        private bool waitack = true;

        public bool bWaitAck_DeliStore
        {
            get { return waitack; }
            set
            {
                if (waitack == value) return;
                waitack = value;
                App.Eq.SetCanDeliStore();
                OnPropertyChanged();
            }
        }

        private bool lifterHastask = false;
        public bool bLifterHasTask
        {
            get { return lifterHastask; }
            set
            {
                if (lifterHastask == value) return;
                lifterHastask = value;
                App.Eq.SetCanDeliStore();
                OnPropertyChanged();
            }
        }
        private bool sotoremanualbtn = true;
        public bool bStoreManualBtn
        {
            //StoreManual Button IsEnable
            get { return sotoremanualbtn; }
            set
            {
                if (sotoremanualbtn == value) return;
                sotoremanualbtn = value;
                OnPropertyChanged();
            }
        }

        private bool sotoreautobtn = true;
        public bool bStoreAutoBtn
        {
            //StoreAuto Button IsEnable
            get { return sotoreautobtn; }
            set
            {
                if (sotoreautobtn == value) return;
                sotoreautobtn = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name_ = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name_));
        }

        public BatchList StoreBatchNo(string sBoxID_, string sTxBatchNo_)
        {
            //DUMPKG0001||DUMPKG|82027|4161066||X|NA||DUM||N|1
            //DUMPKG0477||DUMPKG|75618|5272077||X|NA||DUM||N|9
            //DUMPKG0512||DUMPKG|75618|5272077||X|NA||DUM||N|8
            //DUMPKG0666||DUMPKG|75618|5272077||X|NA||DUM||N|8
            //DUMPKG0841||DUMPKG|75618|5272077||X|NA||DUM||N|3
            BatchList batchin = new BatchList();
            string sBatchNoIn = sTxBatchNo_.Trim();
			if (sBoxID_ == "")
			{
                App.DS.CIMMessage("Store Message", "請先讀取BoxID");
				return batchin;
			}
			if (sBatchNoIn == "")
			{
                batchin.sESB = sBoxID_;
                batchin.sOPERATORNO = App.DS.operator_deliStore.UserName;
                batchin.sLabelInfo = sBatchNoIn;
                batchin.sMatchResult = "";
            }
            else if (sBatchNoIn == "EMPTY")
			{
                batchin.sESB = sBoxID_;
                batchin.sOPERATORNO = App.DS.operator_deliStore.UserName;
                batchin.sLabelInfo = sBatchNoIn;
                batchin.sMatchResult = "E";
            }
			else if (sBatchNoIn != "" && sBatchNoIn!="EMPTY")
            {
                if (!sTxBatchNo_.Contains("|"))
                    return batchin;

                batchin.sESB = sBoxID_;
                batchin.sOPERATORNO = App.DS.operator_deliStore.UserName;
                string[] arrStr = null;
                batchin.sLabelInfo = sBatchNoIn;
                arrStr = sBatchNoIn.Split('|');
                //之後 '|'這個個數要可以設定
                if (arrStr.Length == 13)
                {
                    string sBatchOut = arrStr[0];
                    int iCheckCode = int.Parse(arrStr[12]);
                    string sBatchCode = Regex.Replace(sBatchOut, "[^0-9]", ""); //僅保留數字

                    batchin.sBatchNo = sBatchOut;
                    int iCode = 0, iDigit = 0;
                    string sCode, sDigit;
                    foreach (char c in sBatchCode)
                    {
                        iCode += int.Parse(c.ToString());
                    }
                    sCode = iCode.ToString();
                    sDigit = sCode.Substring(sCode.Length - 1, 1);
                    iDigit = int.Parse(sDigit);

                    if (iDigit == iCheckCode)
                        batchin.sMatchResult = "Y";
                    else
                        batchin.sMatchResult = "F";
                }
            }
            return batchin;
        }

        public void InsertStoreBatchList(BatchList itemList_)
        {
            string sql = string.Empty;
            sql = $"INSERT INTO BATCH_LIST(BOX_ID, BATCH_NO, LABELINFO, READY_DELIVERY,MATCH_RESULT,END_TIME)" +
                    $"VALUES('{itemList_.sESB}','{itemList_.sBatchNo}', '{itemList_.sLabelInfo}', 0,'{itemList_.sMatchResult}','{DateTime.Now.ToString("yyyyMMddHHmmssfff")}')";
            App.Local_SQLServer.NonQuery(sql);
        }

        public void OrderNoSelect(string sOrderNo_, string sSoteria_)
        {
            ASEOrderNoSelect(sOrderNo_, sSoteria_);
            DataTable Result = new DataTable();
            string sql = $"SELECT * FROM BATCH_LIST WHERE ORDER_NO LIKE '%{sOrderNo_}%' ORDER BY END_TIME DESC";
            bool bk = App.Local_SQLServer.Query(sql, ref Result, true);
            string sSOTERIA, sBoxID, sBatchNo, sOrderNo, sCustomerNo;
            foreach (DataRow dr in Result.Rows)
            {
                sSOTERIA = dr["SOTERIA"].ToString().Trim();
                bool bRun = sSOTERIA == sSoteria_ ? true : false;
                if (bRun)
                {
                    sBoxID = dr["BOX_ID"].ToString().Trim();
                    bool bCanDeli = true;
                    foreach (string boxid in BoxIDInTask)
                    {
                        if (boxid == sBoxID)
                            bCanDeli = false;
                    }

                    if (bCanDeli)
                    {
                        sBatchNo = dr["BATCH_NO"].ToString().Trim();
                        sOrderNo = dr["ORDER_NO"].ToString().Trim();
                        sSOTERIA = dr["SOTERIA"].ToString().Trim();
                        sCustomerNo = dr["CUSTOMER_ID"].ToString().Trim();

                        BatchList deli = new BatchList();

                        DataTable dt_Batch = App.Local_SQLServer.SelectDB("BOX_ID", "[BATCH_LIST]", $"[BATCH_NO] = '{sBatchNo}'");

                        deli.sESB = sBoxID;
                        deli.sBatchNo = sBatchNo;
                        deli.sOrderNo = sOrderNo;
                        deli.sSOTERIA = sSOTERIA;
                        deli.sCUSTOMERNO = sCustomerNo;

                        App.DS.DeliList_Orderno.Add(deli);
                    }
                }
            }
            CompareASEOrderNos();
        }
        public void ASEOrderNoSelect(string sOrderNo_, string sSoteria_)
        {
            DataTable Result = new DataTable();
            string sql = $"SELECT * FROM ORDER_BATCHLIST WHERE ORDER_NO LIKE '%{sOrderNo_}%' AND REQ_FROM LIKE '%{App.SysPara.SqlDB_LifterUnit}%'";
            bool bk = App.Local_SQLServer.Query(sql, ref Result, true);
            string sSOTERIA, sBatchNo, sOrderNo, sCustomerNo, sBoxID;
            foreach (DataRow dr in Result.Rows)
            {
                sSOTERIA = dr["SOTERIA"].ToString().Trim();
                if (App.SysPara.Simulation)
                {
                    if (sSOTERIA == "")
                        sSOTERIA = sSoteria_;
                }
                bool bRun = sSOTERIA == sSoteria_ ? true : false;
                if (bRun)
                {
                    sBatchNo = dr["BATCH_NO"].ToString().Trim();
                    sOrderNo = dr["ORDER_NO"].ToString().Trim();
                    sSOTERIA = dr["SOTERIA"].ToString().Trim();
                    sCustomerNo = dr["CUSTOMER_ID"].ToString().Trim();

                    DataTable dt_Batch = App.Local_SQLServer.SelectDB("BOX_ID", "[BATCH_LIST]", $"[BATCH_NO] = '{sBatchNo}'");
                    if (dt_Batch.Rows.Count > 0)
                    {
                        sBoxID = dt_Batch.Rows[0]["BOX_ID"].ToString().Trim();
                    }
                    else
                        sBoxID = "";
                    BatchList deli = new BatchList();

                    deli.sBatchNo = sBatchNo;
                    deli.sOrderNo = sOrderNo;
                    deli.sSOTERIA = sSOTERIA;
                    deli.sCUSTOMERNO = sCustomerNo;
                    deli.sESB = sBoxID;
                    App.DS.ASE_Orderno.Add(deli);
                }
            }
        }

        public void CompareASEOrderNos()
        {
            List<string> ASE_BatchNos = new List<string>();
            List<string> US_BatchNos = new List<string>();

            foreach (BatchList batchlist in App.DS.ASE_Orderno)
            {
                ASE_BatchNos.Add(batchlist.sBatchNo);
            }

            foreach (BatchList batchlist in App.DS.DeliList_Orderno)
            {
                US_BatchNos.Add(batchlist.sBatchNo);
            }

            //dif1 ASE有，LocalDB BatchList中沒有
            //dif2 ASE沒有，LocalDB BatchList中有
            //same 兩邊都有
            var dif1 = ASE_BatchNos.Where(a => !US_BatchNos.Any(a1 => a1 == a));
            var dif2 = US_BatchNos.Where(a => !ASE_BatchNos.Any(a1 => a1 == a));
            var same = ASE_BatchNos.Where(a => US_BatchNos.Any(a1 => a1 == a));

            foreach (BatchList deli in App.DS.ASE_Orderno)
            {
                if (dif1.Contains(deli.sBatchNo))
                {
                    deli.bCompareOderNos = false;
                    deli.sCompareOderNoMsg = "NOTIN BatchList but IN ASE";
                    deli.bCheckBoxWork = false;
                }
                else
                {
                    deli.bCompareOderNos = true;
                    deli.sCompareOderNoMsg = "";
                }
            }
            foreach (BatchList deli in App.DS.DeliList_Orderno)
            {
                if (dif2.Contains(deli.sBatchNo))
                {
                    deli.bCompareOderNos = false;
                    deli.sCompareOderNoMsg = "NOTIN ASE but IN BatchList";
                    deli.bCheckBoxWork = false;
                    App.DS.ASE_Orderno.Add(deli);
                }
            }
        }
        public void CompareSTKrobs()
        {
            List<string> STK_Prob = new List<string>();
            List<string> US_Prob = new List<string>();

            foreach (BatchList batchlist in App.DS.DeliSTK_Prob)
            {
                STK_Prob.Add(batchlist.sBatchNo);
            }

            foreach (BatchList batchlist in App.DS.DeliList_Prob)
            {
                US_Prob.Add(batchlist.sBatchNo);
            }
            //dif_Normal_1 STK有，LocalDB BatchList中沒有
            //dif_Normal_2 STK沒有，LocalDB BatchList中有
            //same 兩邊都有
            var dif1 = STK_Prob.Where(a => !US_Prob.Any(a1 => a1 == a));
            var dif2 = US_Prob.Where(a => !STK_Prob.Any(a1 => a1 == a));
            var same = STK_Prob.Where(a => US_Prob.Any(a1 => a1 == a));

            foreach (BatchList deli in App.DS.DeliSTK_Prob)
            {
                if (dif1.Contains(deli.sBatchNo))
                {
                    deli.bCompareProb = false;
                    deli.sCompareProbMsg = "NOProbIN BatchList but IN STK";
                }
            }
            foreach (BatchList deli in App.DS.DeliList_Prob)
            {
                if (dif2.Contains(deli.sBatchNo))
                {
                    deli.bCompareProb = false;
                    deli.sCompareProbMsg = "NOProbIN STK but IN BatchList";
                    App.DS.DeliSTK_Prob.Add(deli);
                }
            }
            foreach (BatchList deli in App.DS.DeliSTK_Prob)
            {
                string sBatchList_ProbMsg = string.Empty;
                if (same.Contains(deli.sBatchNo))
                {
                    deli.bCompareProb = true;
                    foreach (BatchList delibatch in App.DS.DeliList_Prob)
                    {
                        if (delibatch.sBatchNo == deli.sBatchNo)
                        {
                            sBatchList_ProbMsg = delibatch.sProbMsg;
                            break;
                        }
                    }
                    if (sBatchList_ProbMsg != string.Empty)
                        deli.sProbMsg += "、" + sBatchList_ProbMsg;
                }
            }
        }

        public void DeliveryStep(string sBoxID_, int iStep_)
        {
            //iStep = 0:None
            //        1:Ready to Deilvery
            //        2:Now AskIT
            //        3:Got Reply
            string sql = $"UPDATE BATCH_LIST SET READY_DELIVERY = {iStep_} WHERE BOX_ID = '{sBoxID_}'";
            App.Local_SQLServer.NonQuery(sql);
        }

        public ulong OfflineFormatTrans(string sTime_)
        {
            ulong uTime;
            sTime_ = sTime_.Trim();
            sTime_ = sTime_.Replace("/", "");
            sTime_ = sTime_.Replace(" ", "");
            sTime_ = sTime_.Replace(":", "");
            ulong.TryParse(sTime_, out uTime);
            return uTime;
        }

        private void DecideChangeLifterStatus()
        {
            //if (App.SysPara.Simulation)
            //{
            //    bLifterTransStatus = !bLifterIsOnline;
            //}
            if (bLifterIsOnline != bLifterTransStatus)
            {
                if (NowReqFrame != ucFrame.None || NowDeliFrame != ucFrame.None)
                {
                    if (NowReqFrame.ToString().Contains("Deli"))
                    {
                        //出庫正在等待CPC回覆
                        if (App.DS.Delivery_HasReq)
                        {
                            //出庫還有尚未詢問IT的BoxID，不改變連線狀態

                        }
                        else if (App.DS.DeliWaitAck)
                        {
                            //出庫尚在等待IT回覆，不改變連線狀態

                            //入庫可以直接切連線狀態
                        }
                        else
                        {
                            if (!App.SysPara.Simulation)
                            {
                                bLifterIsOnline = bLifterTransStatus;
                            }
                            OP.Logout();
                            bBackToMainWindow = true;
                            LifterStatusChange();
                        }
                    }
                    else
                    {
                        //入庫正在等待CPC回覆
                    }
                }
                else
                {
                    //if (!App.SysPara.Simulation)
                    //{
                    //    bLifterIsOnline = bLifterTransStatus;
                    //}
                    OP.Logout();
                    bBackToMainWindow = true;
                    LifterStatusChange();
                }
            }
        }

        private void LifterStatusChange()
        {
            //Lifter Stauts
            string sql = string.Empty;
            string sStatus = string.Empty;
            try
            {
                if (bLifterIsOnline)
                {
                    sStatus = "OFFLINE";
                    bLifterIsOnline = false;
                }
                else
                {
                    sStatus = "ONLINE";
                    bLifterIsOnline = true;
                }
                sql = $"IF EXISTS(SELECT* FROM WEBSERVICE_CONNECTION WHERE NAME = 'LIFTER{App.SysPara.SqlDB_LifterUnit}')" +
                        $"UPDATE WEBSERVICE_CONNECTION SET STATUS = '{sStatus}' WHERE NAME = 'LIFTER{App.SysPara.SqlDB_LifterUnit}')" +
                        $"ELSE INSERT INTO WEBSERVICE_CONNECTION(NAME,STATUS)VALUES('LIFTER{App.SysPara.SqlDB_LifterUnit}'), '{sStatus}')";

                int k = App.Local_SQLServer.NonQuery(sql);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void Transfer_CarouselCell_to_ViewID(ref string sCarouselID_, ref string sCellID_) //丟進來原本的名稱，輸出搜尋到的對應名稱
        {
            string sCondition = $"CAROUSEL_ID = '{sCarouselID_}'";
            if (!string.IsNullOrEmpty(sCellID_))
            {
                sCondition += $" AND CELL_ID = '{sCellID_}'";
            }
            DataRow[] dr = dt_Carousel_Trans.Select(sCondition);
            if (dr.Length > 0)
            {
                sCarouselID_ = dr[0]["SHOW_CAROUSEL_ID"].ToString().Trim();
                if (!string.IsNullOrEmpty(sCellID_))
                    sCellID_ = dr[0]["SHOW_CELL_ID"].ToString().Trim();
            }
        }
        public void CIMMessage(string sTitle_, string msg_)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                fmCimMessageBox fm = new fmCimMessageBox(sTitle_,msg_);
                fm.Show();
                CimMsgLogFile.AddString(string.Format("{0}:{1}",sTitle_,msg_));
            }));
        }
        public void BoxInNG(string msg_)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                fmBoxInNG fm = new fmBoxInNG();
                fm.Show();
                string sTime_ = DateTime.Now.ToString().Trim();
                BoxInNGRetryLogFile.AddString(string.Format("{0}:{1}", sTime_, msg_));
            }));
        }
        public void BoxOutNG(string msg_)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                fmBoxOutNG fm = new fmBoxOutNG();
                fm.Show();
                string sTime_ = DateTime.Now.ToString().Trim();
                BoxInNGRetryLogFile.AddString(string.Format("{0}:{1}", sTime_, msg_));
            }));
        }
        public void BoxOutCompareNG(string msg_)
		{
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                fmBoxOutCompareNG fm = new fmBoxOutCompareNG();
                fm.Show();
                string sTime_ = DateTime.Now.ToString().Trim();
                BoxInNGRetryLogFile.AddString(string.Format("{0}:{1}", sTime_, msg_));
            }));
        }
        public void EmptyBoxOut()
		{
            DataTable dt_temp = App.Local_SQLServer.SelectDB("*", "BATCH_LIST", "BOX_ID='B000101' OR BOX_ID='B000102'");
			if (dt_temp.Rows.Count > 1)
			{
                string ReqData = dt_temp.Rows[0]["BOX_ID"].ToString().Trim() + "," + OP.UserName;
                App.SQL_HS.Req("Delivery", ReqData, NowDeliFrame);
            }
		}
    }
    public class BatchList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name_ = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name_));
        }

        public BatchList() { }

        private string esb = "";
        public string sESB
        {
            get { return esb; }
            set
            {
                if (esb == value) return;
                esb = value;
                OnPropertyChanged();
            }
        }

        private string batchno = "";
        public string sBatchNo
        {
            get { return batchno; }
            set
            {
                if (batchno == value) return;
                batchno = value;
                OnPropertyChanged();
            }
        }

        private string labelinfo = "";
        public string sLabelInfo
        {
            get { return labelinfo; }
            set
            {
                if (labelinfo == value) return;
                labelinfo = value;
                OnPropertyChanged();
            }
        }
        private string matchresult = "";
        public string sMatchResult
        {
            get { return matchresult; }
            set
            {
                if (matchresult == value) return;
                matchresult = value;
                OnPropertyChanged();
            }
        }
        private string batchnomsg = "";
        public string sBatchNoMsg
        {
            get { return batchnomsg; }
            set
            {
                if (batchnomsg == value) return;
                batchnomsg = value;
                OnPropertyChanged();
            }
        }

        private string orderno;
        public string sOrderNo
        {
            get { return orderno; }
            set
            {

                if (orderno == value) return;
                orderno = value;
                OnPropertyChanged();
            }
        }

        private long groupno = 0;
        public long GroupNo
        {
            get { return groupno; }
            set
            {
                if (groupno == value) return;
                groupno = value;
                OnPropertyChanged();
            }
        }

        private string soteria = "";
        public string sSOTERIA
        {
            get { return soteria; }
            set
            {
                if (soteria == value) return;
                soteria = value;
                OnPropertyChanged();
            }
        }

        private string customerno = "";
        public string sCUSTOMERNO
        {
            get { return customerno; }
            set
            {
                if (customerno == value) return;
                customerno = value;
                OnPropertyChanged();
            }
        }

        private string operatorno = "";
        public string sOPERATORNO
        {
            get { return operatorno; }
            set
            {
                if (operatorno == value) return;
                operatorno = value;
                OnPropertyChanged();
            }
        }

        private bool isChecked = false;
        public bool bIsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked == value) return;
                isChecked = value;
                OnPropertyChanged();
            }
        }
        private bool checkboxwork = true;
        public bool bCheckBoxWork
        {
            get { return checkboxwork; }
            set
            {
                if (checkboxwork == value) return;
                checkboxwork = value;
                OnPropertyChanged();
            }
        }

        private bool compareodernos = true;
        public bool bCompareOderNos
        {
            get { return compareodernos; }
            set
            {
                if (compareodernos == value) return;
                compareodernos = value;
                if (!compareodernos)
                {
                    bCheckBoxWork = false;
                }
                OnPropertyChanged();
            }
        }
        private string compareodernormsg = "";
        public string sCompareOderNoMsg
        {
            get { return compareodernormsg; }
            set
            {
                if (compareodernormsg == value) return;
                compareodernormsg = value;
                OnPropertyChanged();
            }
        }
        private bool compareprob = false;
        public bool bCompareProb
        {
            get { return compareodernos; }
            set
            {
                if (compareprob == value) return;
                compareprob = value;
                if (!compareprob)
                {
                    bCheckBoxWork = false;
                }
                OnPropertyChanged();
            }
        }
        private string compareprobmsg = "";
        public string sCompareProbMsg
        {
            get { return compareprobmsg; }
            set
            {
                if (compareprobmsg == value) return;
                compareprobmsg = value;
                OnPropertyChanged();
            }
        }

        private string probmsg = "";
        public string sProbMsg
        {
            get { return probmsg; }
            set
            {
                if (probmsg == value) return;
                probmsg = value;
                OnPropertyChanged();
            }
        }

    }

    public class Operator : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name_ = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name_));
        }

        public Operator() { }

        private string username;

        public string UserName
        {
            get { return username; }
            set
            {
                if (username == value) return;
                username = value;
                //connect DB find username & level & password
                OnPropertyChanged();
            }
        }

        private string level;

        public string Level
        {
            get { return level; }
            set
            {
                if (level == value) return;
                level = value;
                bOfflineset = (level.Contains("Offlineset")||level.Contains("Admin")) ? true : false;//level > App.SysPara.OfflineSetLevel
                bSoteriaSet = (level.Contains("Soteria") || level.Contains("Admin")) ? true : false;//level > App.SysPara.SoteriaLevel
                OnPropertyChanged();
            }
        }

        private bool offlineset;

        public bool bOfflineset
        {
            get { return offlineset; }
            set
            {
                if (offlineset == value) return;
                offlineset = value;
                OnPropertyChanged();
            }
        }


        private bool soteriaset;

        public bool bSoteriaSet
        {
            get { return soteriaset; }
            set
            {
                if (soteriaset == value) return;
                soteriaset = value;
                OnPropertyChanged();
            }
        }
        private string userID;
        
        public string UserID
        {
            get { return userID; }
            set
            {
                if (userID == value) return;
                userID = value;
                OnPropertyChanged();
            }
        }

        public string ToUser {
            get { return ""; }
            set
            {
                if (value.Trim() == "") return;

                App.DS.CIMMessage("Operator Login Message", value);
            }
        }
        public bool LogIn { get; set; }
        public void Logout()
        {
            UserName = "GUEST";
            Level = "";//0;
        }


    }
    public class ITreply : INotifyPropertyChanged
    {
        ThreadTimer Clean_Timer;
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name_ = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name_));
        }
        public ITreply()
        {
            Clean_Timer = App.MainThread.TimerCreate("CleanITReply_Timer", App.SysPara.CleanITReplyTime * 1000, Clean_Timer_Event, ThreadTimer.eType.Cycle);
        }

        private string okorng = "";

        public string OKorNG
        {
            get { return okorng; }
            set
            {
                if (okorng == value.Trim().ToUpper()) return;
                okorng = value.Trim().ToUpper();
                OnPropertyChanged();
            }
        }

        private string msg = "";

        public string Msg
        {
            get { return msg; }
            set
            {
                if (msg == value.Trim()) return;
                msg = value.Trim();
                OnPropertyChanged();
            }
        }
        private string delivery_boxid;

        public string BoxID
        {
            get { return delivery_boxid; }
            set { delivery_boxid = value; }
        }

        public void Clear()
        {
            Clean_Timer.Enable(true);
        }
        private int Clean_Timer_Event(ThreadTimer threadTimer_)
        {
            OKorNG = "";
            Msg = "";
            BoxID = "";
            Clean_Timer.Enable(false);
            return 0;
        }
    }
    class Offline
    {
        public int No { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool bDeletBtn { get; set; }
    }
}
