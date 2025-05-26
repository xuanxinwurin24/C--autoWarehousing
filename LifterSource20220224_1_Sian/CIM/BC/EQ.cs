using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Data;

namespace CIM.BC
{
    public class EQ : INotifyPropertyChanged
    {
        #region Property
        //public string MemoryPath = App.sSysDir + @"\Ini\Memory\BC";
        public string MemoryPath = "CIM.BC.Memory.";
        public LogWriter LogFile = new LogWriter();

        string ID = "EQ";

        #endregion Property
        ThreadTimer Eq_Alive_Timer;
        ThreadTimer Eq_AliveSimu_Timer;
        ThreadTimer ClearStockInText_Timer;
        public bool Eq_Alive = false;


        public int AddrBase = 0x0; //Index_event()
        public uint MemLength = 0x1FFF;
        public uint EQST;

        public Eqinput storeEq = new Eqinput();
        public Eqinput deliEq = new Eqinput();
        public uint[] PosList = new uint[7];
        public bool bEqAuto = false;
        public bool bStoreNGchangeToManual = false;
        public bool bPLCMode = false;
        #region INIT
        public EQ()
        {
            try
            {
                LogFile.PathName = App.sSysDir + @"\LogFile\EQ\";
                LogFile.sHead = ID;

                MemAddrInitial();

                HSMSpar HSMSpara;
                HSMSpara = Common.DeserializeXMLFileToObject<HSMSpar>(HSMSpar.FileName);

                Eq_Alive_Timer = App.MainThread.TimerCreate("Eq_Alive_Timer", 10000, Eq_Alive_Timer_Event, ThreadTimer.eType.Cycle);
                Eq_Alive_Timer.Enable(true);

                Eq_AliveSimu_Timer = App.MainThread.TimerCreate("Eq_AliveSimu_Timer", 300, Eq_SimuTimer_Event, ThreadTimer.eType.Cycle);
                Eq_AliveSimu_Timer.Enable(true);

                ClearStockInText_Timer = App.MainThread.TimerCreate("ClearStockInText_Timer", 300, ClearStockInTextTimer_Event, ThreadTimer.eType.Cycle);
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        ~EQ()
        {
        }

        #region MEMGRUOP
        public MemGroup bPLC;
        public MemGroup EqStatus;
        public MemGroup AlarmWarning;
        public MemGroup StockInReport;
        public MemGroup StockOutReport;
        public MemGroup LiftCompleteRpt;
        public MemGroup PositionData;
        void MemAddrInitial()
        {
            try
            {
                bPLC = App.PlcDevice.AddMemGroup_CSVStream(ID, "PLC Bit", LoadStreamResouce(MemoryPath + "EB01.csv"), MemBank.eBank.Bbit, AddrBase, true);
                EqStatus = App.PlcDevice.AddMemGroup_CSVStream(ID, "EQ Status", LoadStreamResouce(MemoryPath + "EW01.csv"), MemBank.eBank.Wreg, AddrBase, true);
                AlarmWarning = App.PlcDevice.AddMemGroup_CSVStream(ID, "Alarm Warning", LoadStreamResouce(MemoryPath + "EW02.csv"), MemBank.eBank.Wreg, AddrBase, true);
                StockInReport = App.PlcDevice.AddMemGroup_CSVStream(ID, "Stock In Report", LoadStreamResouce(MemoryPath + "EW03.csv"), MemBank.eBank.Wreg, AddrBase, true);
                StockOutReport = App.PlcDevice.AddMemGroup_CSVStream(ID, "Stock Out Report", LoadStreamResouce(MemoryPath + "EW04.csv"), MemBank.eBank.Wreg, AddrBase, true);
                LiftCompleteRpt = App.PlcDevice.AddMemGroup_CSVStream(ID, "Lift Complete Report", LoadStreamResouce(MemoryPath + "EW05.csv"), MemBank.eBank.Wreg, AddrBase, true);
                PositionData = App.PlcDevice.AddMemGroup_CSVStream(ID, "Position Data", LoadStreamResouce(MemoryPath + "EW10.csv"), MemBank.eBank.Wreg, AddrBase, true);
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public static Stream LoadStreamResouce(string file_)           //new unit that included MemGroup
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(file_);
                return stream;
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
            return null;
        }
        #endregion
        public void Initial()
        {
            uint Node = 2;
            App.Alarm.UnitNameRegister(Node, ID);

            bPLC.MgValChangeEvent += BPLC_MgValChangeEvent;
            EqStatus.MgValChangeEvent += EqStatus_MgValChangeEvent;
            PositionData.MgValChangeEvent += PositionData_MgValChangeEvent;
            AlarmWarning.MgValChangeEvent += EqMG_MgValChangeEvent;
            StockInReport.MgValChangeEvent += EqMG_MgValChangeEvent;
            StockOutReport.MgValChangeEvent += EqMG_MgValChangeEvent;
            LiftCompleteRpt.MgValChangeEvent += EqMG_MgValChangeEvent;
        }

        private void EqMG_MgValChangeEvent(MemGroup Mg_, TagItem Item_)
        {
            LogFile.AddString(string.Format("{0}:{1}={2}", Mg_.Name, Item_.Name, Item_.StringValue));
        }
        #endregion INIT 

        //Master To Local

        private int ClearStockInTextTimer_Event(ThreadTimer threadTimer_)
        {
            storeEq.sESB = "";
            storeEq.sBatchNo = "";
            ClearStockInText_Timer.Enable(false);
            return 0;
        }

        private int Eq_SimuTimer_Event(ThreadTimer threadTimer_)
        {
            ++EqStatus["Alive"].BinValue;
            return 0;
        }
        private int Eq_Alive_Timer_Event(ThreadTimer threadTimer_)
        {
            App.Alarm.Set(0, 100, true, "EQ is not alive");
            Eq_Alive = false;
            return 0;
        }

        private void BPLC_MgValChangeEvent(MemGroup mg_, TagItem item_)
        {
            LogFile.AddString(string.Format("{0}:{1}={2}", mg_.Name, item_.Name, item_.StringValue));

            if (item_.Name.StartsWith("CPCDateTimeAck"))
            {
                App.Bc.DateTime_AckFromEq(item_);
            }
            else if (item_.Name.StartsWith("ManualStockInStartAck"))
            {
                App.Bc.ManualStockInStart_AckFromEq(item_);
            }
            else if (item_.Name.StartsWith("AlarmWarningRpt"))
            {
                AlarmWarningReport_Event(item_);
            }
            else if (item_.Name.StartsWith("StockInRpt"))
            {
                StockInReport_Event(item_);
            }
            else if (item_.Name.StartsWith("StockOutRpt"))
            {
                StockOutReport_Event(item_);
            }
            else if (item_.Name.StartsWith("LiftCompleteRpt"))
            {
                LiftCompleteReport_Event(item_);
            }
            else if (item_.Name.StartsWith("CrownLifterAck"))
            {
                App.Bc.CrownLifter_AckFromEq(item_);
            }
            else if (item_.Name.StartsWith("StockInReadDataNGRpt"))
            {
                StockInReadDataNGReport_Event(item_);
            }
            else if (item_.Name.StartsWith("StockInReadDataNGScreen"))
            {
                StockInReadDataNGScreen_Event(item_);
            }
            else if (item_.Name.StartsWith("StockOutReadDataNGScreen"))
            {
                StockOutReadDataNGScreen_Event(item_);
            }
            else if (item_.Name.StartsWith("StockOutCompareDataNGScreen"))
            {
                StockOutCompareDataNGScreen_Event(item_);
            }
        }

        private void EqStatus_MgValChangeEvent(MemGroup mg_, TagItem item_)//只要EqStatus中有任何變化就會觸發
        {
            if (item_.Name == "Alive")
            {
                Eq_Alive = true;
                Eq_Alive_Timer.Enable(false);
                Eq_Alive_Timer.Enable(true);
                App.Alarm.Set(0, 100, false); //可以清除alarm
            }
            else
            {
                LogFile.AddString(string.Format("{0}:{1}={2}", mg_.Name, item_.Name, item_.StringValue));
                if (item_.Name == "Mode")
                {
                    uint uMode = EqStatus["Mode"].BinValue; //0:Manual 1:Auto
                    if (uMode == 0)
                    {
                        bEqAuto = false;
                    }
                    else
                        bEqAuto = true;
                }
                else if(item_.Name== "EquipmentLocalRemoteMode")
				{
                    uint uPLCMode = EqStatus["EquipmentLocalRemoteMode"].BinValue;//0:Local 1:Remote
                    if (uPLCMode == 0)
                        bPLCMode = false;
                    else
                        bPLCMode = true;
                        
				}
                else if (item_.Name == "Status")
                {
                    uint uStatus = EqStatus["Status"].BinValue; //1:Idle 2:Run 3:Down
                    EQST = uStatus;
                }
                else if (item_.Name == "WaitShuttleCartoTakeOut")
                {
                    uint uTakeOut = EqStatus["WaitShuttleCartoTakeOut"].BinValue; //0:None 1:Wait ShuttleCar to Take
                    if (uTakeOut == 1)
                    {
                        string boxid = PositionData["CVPos6BoxID"].StringValue.Trim(); //因為PosBoxID 來不及填寫
                        DataTable Result = new DataTable();
                        string sql = $"UPDATE TASK set OUT_LIFTER = {uTakeOut}" +
                            $" WHERE BOX_ID = '{boxid}'";

                        App.Local_SQLServer.NonQuery(sql);
                    }
                }
                else
                {
                    uint uHasAlarm = EqStatus["HasWarning"].BinValue;
                    uint uHasWarning = EqStatus["HasAlarm"].BinValue;
                }
            }
        }
        private void PositionData_MgValChangeEvent(MemGroup mg_, TagItem item_)
        {
            LogFile.AddString(string.Format("{0}:{1}={2}", mg_.Name, item_.Name, item_.StringValue));
            string BoxNameST;
            uint No;
            if (item_.Name.Contains("Exists"))
            {
                if (item_.Name.Contains("CV"))
                {
                    BoxNameST = item_.Name.Substring(0, 6); //CVPos1~6
                    No = uint.Parse(BoxNameST.Substring(BoxNameST.Length - 1)) - 1;
                }
                else
                {
                    BoxNameST = item_.Name.Substring(0, 3); //Pos
                    No = 6;
                }

                if (item_.BinValue == 1)//HasBox
                {
                    string CVPosBoxID = PositionData[BoxNameST + "BoxID"].StringValue.Trim();
                    PosList[No] = 1;
                    //output CVPosBoxID
                }
                else
                {
                    //on [BoxName] = CVPos1~6 / Pos
                    // output no box
                    PosList[No] = 0;
                }
            }
            bool bFound = false;
            int iSumEmptyPlace = 0;
            for (int i = 0; i < 7; i++)
            {
                if (PosList[i] == 1)
                {
                    bFound = true;
                    ++iSumEmptyPlace;
                }
            }
            bBOXExist = bFound;

            bHasPlace = iSumEmptyPlace < 7 ? true : false;
            LogFile.AddString(string.Format("POS bBOXEXIST = {0}", bBOXExist));
            LogFile.AddString(string.Format("POS iSumEmptyPlace = {0}", iSumEmptyPlace));
        }
        void AlarmWarningReport_Event(TagItem it_)
        {
            try
            {
                if (it_.BinValue == 1)
                {
                    List<string> AlarmID = new List<string>();
                    List<uint> SetSt = new List<uint>();
                    List<uint> Unit = new List<uint>();
                    String AlarmSet;
                    String AlarmName;
                    String AlarmUnit;
                    for (int i = 0; i < 8; i++)
                    {
                        AlarmSet = "AlarmWarning" + (i + 1).ToString() + "_Set";
                        AlarmName = "AlarmWarning" + (i + 1).ToString() + "_Code";
                        AlarmUnit = "AlarmWarning" + (i + 1).ToString() + "_Unit";
                        SetSt.Add(AlarmWarning[AlarmSet].BinValue);
                        AlarmID.Add(AlarmWarning[AlarmName].StringValue.Trim());
                        Unit.Add(AlarmWarning[AlarmUnit].BinValue);
                    }
                    int idx = 0;
                    uint alarmunit = 0;
                    foreach (string Almcode in AlarmID.ToArray())
                    {
                        if (Almcode != null)
                        {
                            alarmunit = Unit[idx];
                            if (SetSt[idx] == 0) //RESET  AlarmID.IndexOf(Almcode)
                            {
                                App.Alarm.Set(alarmunit, uint.Parse(Almcode), false);
                            }
                            else //SET
                            {
                                App.Alarm.Set(alarmunit, uint.Parse(Almcode), true);
                            }
                        }
                        ++idx;
                    }
                    App.Bc.AlarmWarningReply(true);
                    return;
                }
                else
                {
                    App.Bc.AlarmWarningReply(false);
                    return;
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        void StockInReport_Event(TagItem it_)
        {
            try
            {
                if (it_.BinValue == 1)
                {
                    storeEq.sESB = StockInReport["BoxID"].StringValue.Trim();
                    storeEq.sBatchNo = StockInReport["FOSBBatchNo"].StringValue.Trim();
                    storeEq.sBatchNo = storeEq.sBatchNo.ToUpper();
                    App.Bc.StockInReply(true);
                    if (storeEq.sBatchNo != "ERROR" && storeEq.sBatchNo != "")
                    {
                        if (App.Bc.eStockInMode == DeliStore.InputWay.Auto)
                        {
                            BatchList BatchIn = new BatchList();

                            BatchIn = App.DS.StoreBatchNo(storeEq.sESB, storeEq.sBatchNo);
                            App.DS.InsertStoreBatchList(BatchIn);

                            string ReqData = BatchIn.sESB + "," + App.DS.OP.UserName;
                            App.SQL_HS.Req("Store", ReqData, DeliStore.ucFrame.StoreAuto);

                            ClearStockInText_Timer.Enable(true);
                        }
                    }
                    else if (storeEq.sBatchNo == "EMPTY")
					{
                        if (App.Bc.eStockInMode == DeliStore.InputWay.Auto)
                        {
                            BatchList BatchIn = new BatchList();

                            App.DS.InsertStoreBatchList(BatchIn);
                            BatchIn.sESB = storeEq.sESB;
                            BatchIn.sBatchNo = storeEq.sBatchNo;
                            string ReqData = BatchIn.sESB + "," + App.DS.OP.UserName;
                            App.SQL_HS.Req("Store", ReqData, DeliStore.ucFrame.StoreAuto);

                            ClearStockInText_Timer.Enable(true);
                        }
                    }
					//if (App.Bc.eStockInMode == DeliStore.InputWay.Auto)
					//{
     //                   DataTable dt=new DataTable();
     //                   BatchList BatchIn = new BatchList();
     //                   string SQL = $"SELECT TOP(5) * FROM [WaferBank_DB].[dbo].[BATCH_LIST] WHERE BATCH_NO = 'EMPTY'";
     //                   App.Local_SQLServer.Query(SQL, ref dt);
     //                   foreach()
					//}
                    if(App.Bc.eStockInMode == DeliStore.InputWay.Manual)
                    {
                        
                    }
                    return;
                }
                else
                {
                    App.Bc.StockInReply(false);
                    return;
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        void StockOutReport_Event(TagItem it_)
        {
            try
            {
                if (it_.BinValue == 1)
                {
                    string oldBoxID = PositionData["CVPos6BoxID"].StringValue.Trim(); //因為PosBoxID 柴不及填寫
                    deliEq.sESB = StockOutReport["BoxID"].StringValue.Trim();
                    LogFile.AddString(string.Format("POS = "));

                    if (deliEq.sESB != "")
                    {
                        string sql = $"UPDATE TASK SET STATUS = 77 WHERE BOX_ID LIKE '%{oldBoxID}%' AND STATUS = 88";
                        App.Local_SQLServer.NonQuery(sql);
                    }
                    if (deliEq.sESB != oldBoxID)
                    {
                        //最後確認BoxID的時候，發現問題
                    }

                    App.Bc.StockOutReply(true);
                    return;
                }
                else
                {
                    App.Bc.StockOutReply(false);
                    return;
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        void LiftCompleteReport_Event(TagItem it_)
        {
            try
            {
                if (it_.BinValue == 1)
                {
                    //1:SendCompelte 2:RecvComplete
                    uint uAction = LiftCompleteRpt["Action"].BinValue;
                    string sBoxID = LiftCompleteRpt["BoxID"].StringValue.Trim();

                    App.Bc.LiftCompleteReply(true);
                    return;
                }
                else
                {
                    App.Bc.LiftCompleteReply(false);
                    return;
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        void StockInReadDataNGReport_Event(TagItem it_)
        {
            try
            {
                if (it_.BinValue == 1)
                {
                    //畫面切到手動入庫
                    //Show視窗，因自動入庫讀取BatchNo失敗
                    bStoreNGchangeToManual = true;
                    App.Bc.StockInReadDataNGReply(true);
                    return;
                }
                else
                {
                    App.Bc.StockInReadDataNGReply(false);
                    return;
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        void StockInReadDataNGScreen_Event(TagItem it_)
        {
            try
            {
                if (it_.BinValue == 1)
                {
                    App.DS.BoxInNG("Box ID入庫讀取失敗");
                    return;
                }
                else
                {
                    App.Bc.StockInReadDataNGReply(false);
                    return;
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        void StockOutReadDataNGScreen_Event(TagItem it_)
        {
            try
            {
                if (it_.BinValue == 1)
                {
                    App.DS.BoxOutNG("Box ID出庫讀取失敗");
                    return;
                }
                else
                {
                    return;
                }
            }
            catch(Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        void StockOutCompareDataNGScreen_Event(TagItem it_)
        {
            try
            {
                if (it_.BinValue == 1)
                {
                    App.DS.BoxOutCompareNG("Box ID出庫比對失敗");
                    return;
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        void UpdateLifterState()
        {
            uint uMode = App.Bc.CPCStatus["LiftMode"].BinValue;
            string sMode = string.Empty;
            if (uMode == 1)
                sMode = "Store";
            else if (uMode == 2)
                sMode = "Delivery";

            string sStatus = string.Empty;
            if (bHasPlace)
            {
                sStatus = "OK";
            }
            else
            {
                sStatus = "FULL";
            }

            string sql = $"IF EXISTS(SELECT* FROM STATUS WHERE NAME = 'Lifter{App.SysPara.SqlDB_LifterUnit}')" +
                        $"UPDATE STATUS SET MODE = '{sMode}', STATUS = '{sStatus}' WHERE NAME = 'Lifter{App.SysPara.SqlDB_LifterUnit}' " +
                        $"ELSE INSERT INTO STATUS(NAME,MODE,STATUS)VALUES('Lifter{App.SysPara.SqlDB_LifterUnit}', '{sMode}','{sStatus}')";
            App.Local_SQLServer.NonQuery(sql);
        }


        public void SetCanDeliStore()
        {
            //bBoxExist true:流道有箱子
            //DS.bWaitAck true:操作人員已經得到人員回覆
            if (bBOXExist != true && App.DS.bWaitAck_DeliStore == true && App.DS.bLifterHasTask == false)
            {
                bCanStore = true;
                bCanDeli = true;
            }
            else
            {
                bCanStore = false;
                bCanDeli = false;
            }
        }

        private bool bstore = true;

        public bool bCanStore
        {
            get
            { return bstore; }
            set
            {
                if (bstore == value) return;
                bstore = value;
                OnPropertyChanged();
            }
        }


        private bool bdeli = true;
        public bool bCanDeli
        {
            get
            { return bdeli; }
            set
            {
                if (bdeli == value) return;
                bdeli = value;
                OnPropertyChanged();
            }
        }

        private bool bboxexist = false;

        public bool bBOXExist
        {
            get { return bboxexist; }
            set
            {
                if (bboxexist == value) return;
                bboxexist = value;
                SetCanDeliStore();
                OnPropertyChanged();
            }
        }

        private bool bhasplace = false;
        public bool bHasPlace
        {
            get { return bhasplace; }
            set
            {
                if (bhasplace == value) return;

                bhasplace = value;

                //更新LocalDB_LIFTER_STATUS
                UpdateLifterState();
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name_ = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name_));
        }
    }

    public class Eqinput : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name_ = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name_));
        }

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
        private string batchno;

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
        public void Clear_Box_BatchNo()
        {
            esb = "";
            batchno = "";
        }
        private int want_amount;
        public int Input_Amount
		{
			get { return want_amount; }
			set
			{
                if (want_amount == value) return;
                want_amount = value;
                OnPropertyChanged();
			}
		}

    }
}
