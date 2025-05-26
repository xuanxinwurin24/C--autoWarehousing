using CIM.BC;
using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Strong.MxComp;
using System.Globalization;

namespace CIM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string sSysDir = Environment.CurrentDirectory;
        public static string sVer = " Ver. 2022/01/19_01";

        public static SysPara SysPara;
        public static SQL_DB Local_SQLServer;
        public static SQL_DB STK_SQLServer;
        public static SQL_HS SQL_HS;

        public static Alarm Alarm = new Alarm();
        public static LogWriter LogFile;

        public static ThreadComp MainThread;
        public static ThreadComp DatabaseThread;

        public static uint IdleCnt;

        //public static UnDevice SimuDevice;
        public static UnDevice PlcDevice;
        public static BC.BC Bc;
        public static EQ Eq;
        public static RFIDReader RFIDReader;
        public static DeliStore DS;

        public static bool bRemindOffline = false;
        public static bool bFristRun = true;

        static App()
        {
            LoadResource.Register();
        }

        void app_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                if (App.isRunning() == true)
                { Environment.Exit(Environment.ExitCode); }

                NewSysUnit();
                NewDevice();
                NewUnitWithMemGroup();

                Initial();

                EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseMoveEvent, new MouseEventHandler(OnPreviewMouseDown));
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        static void OnPreviewMouseDown(object sender, MouseEventArgs e)
        {
            IdleCnt = 0;
            //Trace.WriteLine("Clicked!!");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //LogFile.AddString("program stop");
            //SecsMain.StatusSave();
        }

        public static void NewSysUnit()
        {
            try
            {
                LogFile = new LogWriter(sSysDir + @"\LogFile\AppLog", "App", 50000);
                LogFile.AddString("program start " + sVer);

                SysPara = Common.DeserializeXMLFileToObject<SysPara>(SysPara.FileName);
                SQL_Parameter Local_SQL_Para = Common.DeserializeXMLFileToObject<SQL_Parameter>(App.sSysDir + "\\Ini\\Local_SQL_Parameter.xml");
                SQL_Parameter STK_SQL_Para = Common.DeserializeXMLFileToObject<SQL_Parameter>(App.sSysDir + "\\Ini\\STK_SQL_Parameter.xml");
                //Alarm.UnitNameRegister(1, "BC");
                Alarm.LoadFromFile();

                Password.Initial(sSysDir + @"\Ini", "user.ini");
                MainThread = new ThreadComp("MainThreadComp");
                MainThread.CycleRunEvent += MainThreadComp_CycleRunEvent;

                DatabaseThread = new ThreadComp("DatabaseThread");
                DatabaseThread.CycleRunEvent += DatabaseThread_CycleRunEvent;

                //建立SQL連線
                Local_SQLServer = new SQL_DB(Local_SQL_Para);
                if (!Local_SQLServer.bConnect)
                {
                    LogWriter.LogAndExit("Local_SQLServer連線失敗，請確認IP位址或防火牆");
                }

                STK_SQLServer = new SQL_DB(STK_SQL_Para);
                if (!STK_SQLServer.bConnect)
                {
                    LogWriter.LogAndExit("STK_SQLServer連線失敗，請確認IP位址或防火牆");
                }

                //SQL_HS
                SQL_HS = new SQL_HS(App.SysPara.SqlDB_LifterUnit);
                //if (Local_SQLServer.ConnectToDB() == false)
                //	LogWriter.LogAndExit("Local DataBase連線失敗，請確認!");
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        public static void NewDevice()
        {
            try
            {
                //SimuDevice = new UnDevice("SimuDevice", MainThread);
                //SimuDevice.DevLogFile.PathName = sSysDir + @"\LogFile\Device Log\SimuDevice";
                //SimuDevice.DevLogFile.sHead = "SimuDevice";
                //SimuDevice.Simulation = true;

                ActEtherMxComp.Para Par = new ActEtherMxComp.Para();
                Par.DevName = "Plc";
                Par.Thread = MainThread;
                Par.DevLogFilePath = sSysDir + @"\LogFile\DeviceLog\Plc";
                Par.DevLogFileHead = "Plc";
                Par.comType = ActEtherMxComp.eComType.Q_EASYIF;
                Par.StationNo = 1;
                PlcDevice = new ActEtherMxComp(Par);
                PlcDevice.MaxWordCntPerCmd = 1000;
                PlcDevice.WordAddrInterleave = 500;
                PlcDevice.DevLogFile.PathName = sSysDir + @"\LogFile\DeviceLog\PlcDevice";
                PlcDevice.DevLogFile.sHead = "PlcDevice";
                PlcDevice.Simulation = SysPara.Simulation;
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        static void NewUnitWithMemGroup()           //new unit that included MemGroup
        {
            try
            {
                Bc = new BC.BC();
                Eq = new EQ();
                DS = new DeliStore();

                if (!App.SysPara.Simulation)
                {
                    RFIDReader = new RFIDReader(1);
                    RFIDReader.PortOpen();
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        public static bool isRunning()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return false;
            }
            int procCount = 0;
            Process[] pa = Process.GetProcesses();
            foreach (Process proc in pa)
            {
                //LogWriter.stAddString(proc.ProcessName);
                if (proc.ProcessName == Process.GetCurrentProcess().ProcessName)
                {
                    if (++procCount > 1)
                    {
                        LogWriter.LogAndExit(Process.GetCurrentProcess().ProcessName + "程式已經在執行！");
                        return true;
                    }
                }
            }
            return false;
        }

        public static void Initial()
        {
            try
            {
                //--------------Assign MemGroup's Device for module that included MemGroup-----------
                MainThread.DeviceInit();
                DatabaseThread.DeviceInit();

                //--------------Initial TagItem Value for unit-----------
                Bc.Initial();
                Eq.Initial();
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        public static void Start()
        {
            try
            {
                MainThread.Start();
                DatabaseThread.Start();
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        private static void MainThreadComp_CycleRunEvent(ThreadComp Sender_)
        {
            try
            {
                if (MainThread.ScanTime > 1500)
                {
                    CIM.MainWindow.Log.frmSysLog.Log(string.Format("Scan Time:{0}", MainThread.ScanTime));
                }
                Thread.Sleep(10);
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        private static void DatabaseThread_CycleRunEvent(ThreadComp Sender_)
        {
            try
            {
                if (Sender_.ScanTime > 50) //1000
                    LogFile.AddString(string.Format("{0} CycleTime : {1}", Sender_.Name, Sender_.ScanTime));
                UpdateTask();
                if (DS.UpdateDelivery)
                {
                    if (DS.LastAckFrame != DeliStore.ucFrame.None)
                    {
                        DS.NowActFrame = DS.LastAckFrame;
                    }
                    UpdateDeli(bFristRun);
                    UpdateDeliProb();

                }
                if (DS.Delivery_HasReq && !DS.DeliWaitAck)
                {
                    //存在等待MPC回覆的出庫要求 (DeliWaitAck)
                    //需送出的出庫要求(Delivery_HasReq)
                    UpdateDeli_Req();
                }
                UpdateOfflineSet();
                SpinWait.SpinUntil(() => false, 500);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }


        private static void UpdateTask()
        {
            string sql = string.Empty;
            try
            {
                DataTable Result = new DataTable();
                sql = $"SELECT BOX_ID,TAR_POS FROM [{App.SysPara.SqlDB_catalog}].[dbo].[TASK]";
                Local_SQLServer.Query(sql, ref Result, true);
                DS.BoxIDInTask = new List<string>();
                App.DS.bLifterHasTask = false;
                foreach (DataRow dr in Result.Rows)
                {
                    string sBoxID, sTarPos;

                    //有出庫任務的BoxID
                    sBoxID = dr["BOX_ID"].ToString().Trim();
                    DS.BoxIDInTask.Add(sBoxID);

                    if (!App.DS.bLifterHasTask)
                    {
                        //Lifter還有任務待執行
                        sTarPos = dr["TAR_POS"].ToString().Trim();
                        if (sTarPos == "LIFTER_" + App.SysPara.SqlDB_LifterUnit.ToUpper())
                        {
                            App.DS.bLifterHasTask = true;
                        }
                        else
                            App.DS.bLifterHasTask = false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }

        private static void UpdateDeli(bool bFirstTime_)
        {
            string sql = string.Empty;
            try
            {
                if (bFirstTime_)
                {
                    //將出庫尚未詢問IT的紀錄刪除，人員須重新勾選
                    sql = $"IF EXISTS(SELECT * FROM BATCH_LIST WHERE READY_DELIVERY = 1 OR READY_DELIVERY = 2)" +
                        $"UPDATE BATCH_LIST SET READY_DELIVERY = 0 WHERE READY_DELIVERY = 1 OR READY_DELIVERY = 2";
                    Local_SQLServer.NonQuery(sql);
                    bFristRun = false;
                }
                DataTable Result = new DataTable();
                sql = $"SELECT * FROM BATCH_LIST WHERE READY_DELIVERY = 0 ORDER BY END_TIME DESC";
                bool bResult = Local_SQLServer.Query(sql, ref Result, true);
                DS.DeliList_Conf = new List<BatchList>();
                DS.DeliList_NConf = new List<BatchList>();
                bool bCanDeli;

                foreach (DataRow dr in Result.Rows)
                {
                    string sBoxID, sBatchNo, sOrderNo, sSOTERIA, sCustomerNo, sBatchResult;
                    bool bcbEnable = true;
                    sBoxID = dr["BOX_ID"].ToString().Trim();

                    bCanDeli = true;
                    foreach (string boxid in DS.BoxIDInTask)
                    {
                        if (boxid == sBoxID)
                            bCanDeli = false;
                    }

                    sBatchResult = dr["BATCH_NO_RESULT"].ToString().Trim();
                    if (!bcbEnable)
                        bCanDeli = false;

                    if (bCanDeli)
                    {
                        sBatchNo = dr["BATCH_NO"].ToString().Trim();
                        sOrderNo = dr["ORDER_NO"].ToString().Trim();
                        sSOTERIA = dr["SOTERIA"].ToString().Trim();
                        sCustomerNo = dr["CUSTOMER_ID"].ToString().Trim();

                        BatchList deli = new BatchList();

                        deli.sESB = sBoxID;
                        deli.sBatchNo = sBatchNo;
                        deli.sOrderNo = sOrderNo;
                        deli.sSOTERIA = sSOTERIA;
                        deli.sCUSTOMERNO = sCustomerNo;
                        deli.bIsChecked = false;
                        deli.bCheckBoxWork = bcbEnable;

                        if (App.DS.OP.bSoteriaSet)
                        {
                            if (sSOTERIA == "S")
                                DS.DeliList_Conf.Add(deli);
                            else if (sSOTERIA == "N")
                                DS.DeliList_NConf.Add(deli);
                            else
                            {
                                if (App.SysPara.TEST)
                                    DS.DeliList_NConf.Add(deli);
                            }
                        }
                        else
                        {
                            if (sSOTERIA == "N")
                                DS.DeliList_NConf.Add(deli);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }


        private static void UpdateDeli_Req()
        {
            string sql = string.Empty;
            try
            {
                DataTable Result = new DataTable();
                sql = $"SELECT * FROM BATCH_LIST WHERE READY_DELIVERY = 1 ORDER BY END_TIME DESC";
                Local_SQLServer.Query(sql, ref Result, true);
                foreach (DataRow dr in Result.Rows)
                {
                    string sBoxID = dr["BOX_ID"].ToString().Trim();
                    string ReqData = sBoxID + "," + DS.OP.UserName;
                    App.DS.DeliveryStep(sBoxID, 2);
                    SQL_HS.Req("Delivery", ReqData, DS.NowDeliFrame);
                    DS.NowReqFrame = DS.NowDeliFrame;
                    DS.DeliWaitAck = true;
                    return;
                }
                DS.NowDeliFrame = DeliStore.ucFrame.None;
                DS.Delivery_HasReq = false;
                DS.UpdateDelivery = true;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        private static void UpdateDeliProb()
        {
            string sql_STK = string.Empty;
            string sql_BatchList = string.Empty;
            try
            {
                DataTable ResultSTK = new DataTable();
                DataTable ResultBatchList = new DataTable();

                sql_STK = $"SELECT * FROM CELL_STATUS WHERE STATUS = 3 OR STATUS = 4 OR CHECK_RESULT != 0";
                sql_BatchList = $"SELECT * FROM BATCH_LIST WHERE BATCH_NO_RESULT NOT LIKE '%Y%' OR MATCH_RESULT LIKE '%F%' ORDER BY END_TIME DESC";

                bool bstk = STK_SQLServer.Query(sql_STK, ref ResultSTK, true);
                bool blocal = Local_SQLServer.Query(sql_BatchList, ref ResultBatchList, true);

                DS.DeliSTK_Prob = new List<BatchList>();
                DS.DeliList_Prob = new List<BatchList>();

                bool bCanDeli;
                bool bSoteria_CouldBe_s = App.DS.OP.bSoteriaSet ? true : false;
                foreach (DataRow dr_STK in ResultSTK.Rows)
                {
                    string sBoxID, sBatchNo, sSOTERIA, sCustomerNo;
                    int iStatus, iCheckResult;

                    sBoxID = dr_STK["BOX_ID"].ToString().Trim();
                    bCanDeli = true;

                    if (sBoxID == "")
                        bCanDeli = false;

                    foreach (string boxid in DS.BoxIDInTask)
                    {
                        if (boxid == sBoxID)
                            bCanDeli = false;
                    }

                    if (bCanDeli)
                    {
                        bool bRun = false;
                        sSOTERIA = dr_STK["SOTERIA"].ToString().Trim();
                        if (bSoteria_CouldBe_s)
                            bRun = true;
                        else
                        {
                            if (sSOTERIA != "S")
                                bRun = true;
                        }
                        if (bRun)
                        {
                            sBatchNo = dr_STK["BATCH_NO"].ToString().Trim();
                            sCustomerNo = dr_STK["CUSTOMER_ID"].ToString().Trim();
                            int.TryParse(dr_STK["STATUS"].ToString(), out iStatus);
                            int.TryParse(dr_STK["CHECK_RESULT"].ToString(), out iCheckResult);

                            BatchList deli_STK = new BatchList();

                            deli_STK.sESB = sBoxID;
                            deli_STK.sBatchNo = sBatchNo;
                            deli_STK.sSOTERIA = sSOTERIA;
                            deli_STK.sCUSTOMERNO = sCustomerNo;
                            deli_STK.bIsChecked = false;
                            deli_STK.bCheckBoxWork = true;

                            switch (iStatus)
                            {
                                case 3:
                                    deli_STK.sProbMsg = "無帳有料、";
                                    break;
                                case 4:
                                    deli_STK.sProbMsg = "NG帳料、";
                                    break;
                            }

                            switch (iCheckResult)
                            {
                                case 1:
                                    deli_STK.sProbMsg += "有BOX無資料、";
                                    break;
                                case 2:
                                    deli_STK.sProbMsg += "有資料無BOX、";
                                    break;
                                case 3:
                                    deli_STK.sProbMsg += "資料與讀取不相符、";
                                    break;
                            }
                            deli_STK.sProbMsg = deli_STK.sProbMsg.Substring(0, deli_STK.sProbMsg.Length - 1);
                            DS.DeliSTK_Prob.Add(deli_STK);
                        }
                    }
                }
                foreach (DataRow dr in ResultBatchList.Rows)
                {
                    string sBoxID, sBatchNo, sSOTERIA, sCustomerNo, sBatchResult;
                    int iReady_Delivery;

                    sBoxID = dr["BOX_ID"].ToString().Trim();

                    bCanDeli = true;
                    foreach (string boxid in DS.BoxIDInTask)
                    {
                        if (boxid == sBoxID)
                            bCanDeli = false;
                    }
                    int.TryParse(dr["READY_DELIVERY"].ToString().Trim(), out iReady_Delivery);

                    if (bCanDeli)
                    {
                        if (iReady_Delivery == 0)
                        {
                            bool bRun = false;
                            sSOTERIA = dr["SOTERIA"].ToString().Trim();
                            if (bSoteria_CouldBe_s)
                                bRun = true;
                            else
                            {
                                if (sSOTERIA != "S")
                                    bRun = true;
                            }
                            if (bRun)
                            {
                                sBatchNo = dr["BATCH_NO"].ToString().Trim();
                                sCustomerNo = dr["CUSTOMER_ID"].ToString().Trim();
                                sBatchResult = dr["BATCH_NO_RESULT"].ToString().Trim();

                                BatchList deli = new BatchList();

                                deli.sESB = sBoxID;
                                deli.sBatchNo = sBatchNo;
                                deli.sSOTERIA = sSOTERIA;
                                deli.sCUSTOMERNO = sCustomerNo;
                                deli.bIsChecked = false;
                                deli.bCheckBoxWork = true;
                                deli.sProbMsg = sBatchResult;
                                DS.DeliList_Prob.Add(deli);
                            }
                        }
                    }
                }
                DS.CompareSTKrobs();
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        private static void UpdateOfflineSet()
        {
            string sql = string.Empty;
            string sql_Real = string.Empty;
            try
            {
                DataTable Result = new DataTable();
                DataTable Result_Real = new DataTable();

                //Set
                sql = $"SELECT * FROM WEBSERVICE_CONNECTION WHERE NAME LIKE '%SET%'";
                Local_SQLServer.Query(sql, ref Result, true);
                string sStartTime, sEndTime;
                foreach (DataRow dr in Result.Rows)
                {
                    sStartTime = dr["START_TIME"].ToString().Trim();
                    sEndTime = dr["END_TIME"].ToString().Trim();
                    DateTime dtStart = DateTime.ParseExact(sStartTime, "yyyyMMddHHmm", CultureInfo.InvariantCulture);
                    DateTime dtEnd = DateTime.ParseExact(sEndTime, "yyyyMMddHHmm", CultureInfo.InvariantCulture);

                    TimeSpan tspan_start = dtStart - DateTime.Now;
                    if (tspan_start.TotalMinutes < 0)
                    {
                        if (sEndTime != "")
                        {
                            TimeSpan tspan_end = dtEnd - DateTime.Now;
                            if (tspan_end.TotalMinutes > 0)
                            {
                                DS.bLifterTransStatus = false;
                                //Remind Message
                                if (tspan_end.TotalMinutes < 5 && bRemindOffline == false)
                                {
                                    App.DS.CIMMessage("CIM Message", "將在五分鐘內切換為連線流程");
                                    bRemindOffline = true;
                                }
                                return;
                            }
                            else
                            {
                                sql = $"DELETE FROM WEBSERVICE_CONNECTION WHERE START_TIME LIKE '%{sStartTime}%' AND END_TIME LIKE '%{sEndTime}%'";
                                Local_SQLServer.NonQuery(sql);
                            }
                        }
                        else
                        {
                            DS.bLifterTransStatus = false;
                            return;
                        }
                    }
                }
                //Real
                sql_Real = $"SELECT * FROM WEBSERVICE_CONNECTION WHERE NAME LIKE '%CPC%'";
                Local_SQLServer.Query(sql_Real, ref Result_Real, true);
                string sStatus = string.Empty;
                foreach (DataRow dr in Result_Real.Rows)
                {
                    sStatus = dr["STATUS"].ToString().Trim();

                    if (sStatus.Trim() == "ONLINE")
                    {
                        DS.bLifterTransStatus = true;
                        bRemindOffline = false;
                        return;
                    }
                    else
                    {
                        DS.bLifterTransStatus = false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
    }

    public class BoolStr2BoolValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = ((string)value).Trim();
            return str == "0" ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bl = (bool)value;
            return bl == true ? "1" : "0";
        }
    }
}
