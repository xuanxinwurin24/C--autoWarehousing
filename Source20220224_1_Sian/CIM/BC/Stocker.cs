using CIM.Lib.Model;
using CIM.UILog;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json.Linq;
using Strong;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CIM.BC
{
    public class Stocker : INotifyPropertyChanged
    {
        /// <INotifyPropertyChanged>
        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName_ = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName_));
        }
        /// </INotifyPropertyChanged>
        ///

        //Hub
        //定義SignalR代理,廣播服務連線相關
        private static IHubProxy HubProxy { get; set; }
        //定義SignalR 連線
        public static HubConnection HubConn { get; set; } 

        #region Declare
        public frmEqLog frmLog;
        private LogWriter LogFile = new LogWriter(App.sSysDir + @"\LogFile\Eq\Unit\", "OHS", 5000 * 1000);
        public void LOG(string sLog_)
        {
            frmLog.Log(sLog_);
            //frmLog.Delete_SQL_Log("Stocker");
            string total = DateTime.Now.ToString("yyyyMMddHHmmss").Trim();
            string SQL_Temp = $"INSERT INTO [LOG_RECORD] VALUES('Stocker','{total}','{sLog_}')";
            App.Local_SQLServer.NonQuery(SQL_Temp);
            /*LogFile.AddString(sLog_);*/
        }
        private uint Unit = 0;
        public uint AlarmOffset = 0;
        ThreadTimer ConnectTimer, AutoONLINETimer;
#if Stocker_Simu
        ThreadTimer ReadTimer;
#endif
        ThreadTimer[] Timeout_Timer;
        private bool bconnected;
        public bool bConnected
        {
            get { return bconnected; }
            set
            {
                if (bconnected == value) return;
                bconnected = value;
                if (App.Bc != null)
                    App.Bc.wAux["STK_Connect"].BinValue = Convert.ToUInt16(bconnected);
                NotifyPropertyChanged();
                AutoONLINETimer.Enable(true);
            }
        }
        //20211230 Sian Modify huburl to FAB's
        private string huburl = @"http://172.22.244.47/signalr/hubs";
        public string HubURL
        {
            get { return huburl; }
            set
            {
                if (huburl == value) return;
                huburl = value;
                NotifyPropertyChanged();
            }
        }

        private string hubpwd = "CPC";//密碼先寫死為 CPC 後續再修改雙方交握密碼

        public string HubPWD
        {
            get { return hubpwd; }
            set
            {
                if (hubpwd == value) return;
                hubpwd = value;
                NotifyPropertyChanged();
            }
        }
#if Stocker_Simu
        private TcpIp TcpIpConnect;
#endif
        private bool bonline = false;
        public bool bONLINE
        {
            get { return bonline; }
            set
            {
                if (value == bonline) return;
                bonline = value;
                App.Bc.wAux["STK_ONLINE"].BinValue = Convert.ToUInt16(bonline);
                NotifyPropertyChanged();
            }
        }
        #endregion Declare
        public Stocker()
        {
            frmLog = MainWindow.ucUILog.StockerLog;

            //ReadTimer = App.MainThread.TimerCreate("ReadTimer", 500, ReadTimer_Event, ThreadTimer.eType.Cycle);
            Timeout_Timer = new ThreadTimer[Enum.GetNames(typeof(eC_CMD_NO)).Count()];
            for (int i = 0; i < Timeout_Timer.Length; i++)
            {
                Timeout_Timer[i] = App.MainThread.TimerCreate(((eC_CMD_NO)i).ToString() + "Timeout_Timer", 60000, Timeout_Timer_Event, ThreadTimer.eType.Hold);
                Timeout_Timer[i].Tag = i;
            }
            ConnectTimer = App.MainThread.TimerCreate("ConnectTimer", 20000, ConnectTimer_Event, ThreadTimer.eType.Cycle);
            ConnectTimer.Enable(true);
            AutoONLINETimer = App.MainThread.TimerCreate("ConnectTimer", 3000, AutoONLINETimer_Event, ThreadTimer.eType.Hold);

        }

        public void Initial()
        {
#if Stocker_Simu
            ReadTimer = App.MainThread.TimerCreate("ReadTimer", 500, ReadTimer_Event, ThreadTimer.eType.Cycle);
            TcpIpConnect = new TcpIp();//new TcpIp(true, App.SysPara.OHS_Port, App.SysPara.OHS_IP, false);
            TcpIpConnect.Initial(true, 6002, "127.0.0.1");
            TcpIpConnect.RawLogFile.MaxSize = 5000000;
            TcpIpConnect.RawLogFile.PathName = "./LogFile/ToStocker";
            TcpIpConnect.RawLogFile.sHead = "ToStocker";
            TcpIpConnect.ConnectedCallBack += new TcpIp.ConnectCallBackHandler(ConnectCallBackEvent);
            TcpIpConnect.Start();
#else
            HubConnect();
#endif
            App.Bc.wAux["STK_Connect"].BinValue = Convert.ToUInt16(bconnected);
        }

#if Stocker_Simu
        private void ConnectCallBackEvent(bool bConnected_)
        {
            bConnected = bConnected_;
            ReadTimer.Enable(bConnected_);
            LOG(bConnected ? "TCP/IP Connected" : "TCP/IP Disconnect");
            if (!bConnected)
                bONLINE = false;
        }
        private int ReadTimer_Event(ThreadTimer Sender)
        {
            if (TcpIpConnect.RcvData.Len == 0)
                return 0;
            CheckS_Cmd_No(Encoding.ASCII.GetString(TcpIpConnect.RcvData.Buff));
            TcpIpConnect.RcvData.Clear();
            Array.Clear(TcpIpConnect.RcvData.Buff, 0, TcpIpConnect.RcvData.Buff.Length);
            return 0;
        }
#endif
        private int ConnectTimer_Event(ThreadTimer Sender)
        {
            if (bConnected == false)
            {
#if Stocker_Simu
                TcpIpConnect.Start();
#else
                HubConnect();
#endif
            }
            return 0;
        }

        private int AutoONLINETimer_Event(ThreadTimer Sender)
        {
            C001_CMD("ONLINE");
            return 0;
        }

        

        private int Timeout_Timer_Event(ThreadTimer Sender)
        {
            App.Alarm.Set(1, /*AlarmOffset +*/ uint.Parse(Sender.Tag.ToString()), true);
            return 0;
        }

        private void HubConnect()
        {
            //0929
            if (HubConn != null)
            { 
                HubConn.Stop();
                HubConn.Dispose();
            }
            HubConn = new HubConnection(HubURL);
            HubProxy = HubConn.CreateHubProxy("StockHub");

            //接收broadcastMessage 訊息 Function僅 1 string
            HubProxy.On<string>("broadcastMessage", CheckS_Cmd_No);//接收

            try
            {
                //建立連線
                HubConn.Start().ContinueWith((Action<Task>)(task =>
                {
                    if (!task.IsFaulted)
                    {
                        //連線成功
                        bConnected = true;
                        //LOG(string.Format("HubConnecct :連線成功 {0}", (object)this.HubURL));
                    }
                    else
                    {
                        //連線失敗
                        bConnected = false;
                        //LOG(string.Format("HubConnecct :連線失敗 {0}", (object)this.HubURL));
                        bONLINE = false;
                    }
                })).Wait();
            }
            catch (Exception ex)
            {
                //連線失敗
                bConnected = false;
                LOG(string.Format("HubConnecct :連線失敗 {0}", ex.Message));
                bONLINE = false;
            }
        }

#region STK_TCPIP_Function

        //private void RecvMsg(string sRcvMsg)
        //{
        //顯示訊息
        //txtRecv.AppendText(string.Format("{0},{1}，\r\n", DateTime.Now.ToString("MM-dd HH:mm:ss"), sRcvMsg));
        //}
        public void SendCMD(string sSendMsg_)
        {
            //原來
            if (!bConnected)
                return;
            LOG("Send : " + sSendMsg_);

            //0929
            try
            {
#if Stocker_Simu
                byte[] Value = Encoding.ASCII.GetBytes(sSendMsg_);
                TcpIpConnect.Send(Value, Value.Length);
#else
                //密碼先寫死為 CPC 後續再修改雙方交握密碼
                //傳遞Send 訊息 Function 密碼,JsonData
                //密碼錯誤不會接收資料
                HubProxy.Invoke("Send", new object[] { HubPWD, sSendMsg_ });
#endif
            }
            catch (Exception ex)
            {
                LogExcept.LogException(ex);
                LOG(string.Format("{0}:{1}", "Send Failed", sSendMsg_));
                //傳遞失敗
                //txtRecv.AppendText("傳遞失敗" + ex.Message + "\r\n");
            }
        }

        public enum eC_CMD_NO
        {
            C001, C002, C010, C020, C030, C031, C040, C050
        }

        public void CheckS_Cmd_No(string sAckMsg)
        {
            if (sAckMsg == null)
                return;
            //List<string> sLsTemp = new List<string>();
            FromSTK_ITEM RcvItem;// = new FromSTK_ITEM();
            try
            {
                LOG("Received : " + sAckMsg.Trim());
                RcvItem = JSON.DeSerialize<FromSTK_ITEM>(sAckMsg.Trim());
                if (RcvItem.CMD_NO.Contains("S"))
                {
                    switch (RcvItem.CMD_NO)
                    {
                        case "S001":
                            S001_ACK(RcvItem);
                            break;
                        case "S002":
                            S002_EVENT(RcvItem);
                            break;
                        case "S010":
                            S010_ACK(RcvItem);
                            break;
                        case "S011":
                            S011_ACK(RcvItem);
                            break;
                        case "S020":
                            S020_ACK(RcvItem);
                            break;
                        case "S021":
                            S021_ACK(RcvItem);
                            break;
                        case "S030":
                            S030_EVENT(RcvItem);
                            break;
                        case "S031":
                            S031_ACK(RcvItem);
                            break;
                        case "S040":
                            S040_EVENT(RcvItem);
                            break;
                        case "S050":
                            S050_ACK(RcvItem);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        //ONLINE_OFFLINE
        public void C001_CMD(string sCMD) //sCMD = ONLINE/OFFLINE
        {
            ToSTK_ITEM C001_JSON;
            try
            {
                C001_JSON = new ToSTK_ITEM()
                {
                    CMD_NO = "C001",
                    DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    CMD = sCMD
                };
                SendCMD(JSON.Serialize(C001_JSON));
                Timeout_Timer[(int)eC_CMD_NO.C001].Enable(true);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void S001_ACK(FromSTK_ITEM RcvItem_)
        {
            try
            {
                Timeout_Timer[(int)eC_CMD_NO.C001].Enable(false);
                if (RcvItem_.REPLY == "ACCEPT")
                {
                    if (RcvItem_.CMD == "ONLINE")
                        bONLINE = true;
                    else
                        bONLINE = false;
                }
                else
                {
                    if (Properties.Settings.Default.DefaultCulture.Name == "zh-TW")
                        App.Alarm.Set(Unit, AlarmOffset + 201, true, "請求STK 狀態變為 " + RcvItem_.CMD + " 但STK拒絕 原因是 : " + RcvItem_.DETAIL);
                    else
                        App.Alarm.Set(Unit, AlarmOffset + 201, true, "Request STK Change State To " + RcvItem_.CMD + " but STK REJECT DETAIL : " + RcvItem_.DETAIL);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void S002_EVENT(FromSTK_ITEM RcvItem_)
        {
            string sReply = string.Empty;
            string sResult = string.Empty;
            try
            {
                if (RcvItem_.CMD == "ONLINE")
                {
                    sReply = "ACCEPT";
                    bONLINE = true;
                }
                else
                {
                    sReply = "ACCEPT";
                    bONLINE = false;
                }
                C002_REPLY(RcvItem_.CMD, sReply, sResult);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void C002_REPLY(string sCMD, string sREPLY, string sDETAIL) //sCMD = ONLINE/OFFLINE
        {
            ToSTK_ITEM C002_JSON;
            try
            {
                C002_JSON = new ToSTK_ITEM()
                {
                    CMD_NO = "C002",
                    DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    CMD = sCMD,
                    REPLY = sREPLY,
                    DETAIL = sDETAIL
                };
                SendCMD(JSON.Serialize(C002_JSON));
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        //車子FromTo動作
        public void C010_CMD(string sCommandID, CAR_ACTION_CMD carACTION)
        {
            ToSTK_ITEM C010_JSON;
            try
            {
                C010_JSON = new ToSTK_ITEM()
                {
                    CMD_NO = "C010",
                    DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    COMMAND_ID = sCommandID,
                    ACTION = carACTION.ACTION,
                    CAR_ID = carACTION.CAR_ID,
                    BOX_ID = carACTION.BOX_ID,
                    BATCH_NO = carACTION.BATCH_NO,
                    SOTERIA = carACTION.SOTERIA,
                    CUSTOMER_ID = carACTION.CUSTOMER_ID,
                    POSITION = carACTION.POSITION,
                    SOURCE = carACTION.SOURCE,
                    S_CELL_ID = carACTION.S_CELL_ID,
                    TARGET = carACTION.TARGET,
                    T_CELL_ID = carACTION.T_CELL_ID
                };
                SendCMD(JSON.Serialize(C010_JSON));

                Timeout_Timer[(int)eC_CMD_NO.C010].Enable(true);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void S010_ACK(FromSTK_ITEM RcvItem_)
        {
            try
            {
                Timeout_Timer[(int)eC_CMD_NO.C010].Enable(false);
                string date = RcvItem_.DATE;
                string cmdid = RcvItem_.COMMAND_ID;
                string action = RcvItem_.ACTION;
                string carid = RcvItem_.CAR_ID;
                string boxid = RcvItem_.BOX_ID;
                string pos = RcvItem_.POSITION;
                string source = RcvItem_.SOURCE;
                string target = RcvItem_.TARGET;
                string status = RcvItem_.REPLY;
                string detail = RcvItem_.DETAIL;
                string sql;
                if (RcvItem_.REPLY == "ACCEPT")
                {
                    LOG($"STK接受任務,CMD ID:{RcvItem_.COMMAND_ID}");
                    sql = $"UPDATE [CAR_CMD] SET [STK_CMD_RESULT] = '1' WHERE COMMAND_ID='{cmdid}'";
                    App.Local_SQLServer.NonQuery(sql);
                }
                else
                {
                    LOG($"STK拒絕任務,CMD ID:{RcvItem_.COMMAND_ID},拒絕原因:{detail}");
                    sql = $"UPDATE [CAR_CMD] SET [STK_CMD_RESULT] = '2' WHERE COMMAND_ID='{cmdid}'";
                    App.Local_SQLServer.NonQuery(sql);

                    sql = $"UPDATE [TASK] SET [NG_REASON] = '{detail}' WHERE COMMAND_ID='{cmdid}'";
                    App.Local_SQLServer.NonQuery(sql);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void S011_ACK(FromSTK_ITEM RcvItem_)
        {
            try
            {
                string sql;
                if (RcvItem_.STATUS == "COMPLETE")
                {
                    if (RcvItem_.TARGET.IndexOf("B-") != -1) //放到STAGE上
                    {
                        sql = $"UPDATE [TASK] SET OUT_STOCKER='1' WHERE BOX_ID='{RcvItem_.BOX_ID}'";
                        App.Local_SQLServer.NonQuery(sql);
                    }
                    else //表示放Carousel放完，任務完成
                    {
                        sql = $"UPDATE [TASK] SET [STATUS] = '{(int)TaskCtrl.eTASK_Flow.Complete}' WHERE BOX_ID='{RcvItem_.BOX_ID}'";
                        App.Local_SQLServer.NonQuery(sql);

                        string sTransCarouselID = RcvItem_.TARGET;
                        string sTransCellID = RcvItem_.T_CELL_ID;
                        App.Bc.Transfer_CarouselCell_to_ViewID(ref sTransCarouselID, ref sTransCellID);
                        string sCurn_Bin = $"{sTransCarouselID}-{sTransCellID}-{RcvItem_.BOX_ID}";

                        sql = $"UPDATE [BATCH_LIST] SET [CURN_BIN] = '{sCurn_Bin}',[END_TIME]='{DateTime.Now.ToString("yyyyMMddHHmmssfff")}' WHERE BOX_ID='{RcvItem_.BOX_ID}'";
                        App.Local_SQLServer.NonQuery(sql);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        //盤點
        public void C020_CMD(string sCommandID_, string sAction_)
        {
            ToSTK_ITEM C020_JSON;
            try
            {
                string sCarouselList = string.Empty;
                List<CAROUSEL_LIST_ITEM> carousel_list = new List<CAROUSEL_LIST_ITEM>();
                JArray Json_Array = new JArray();
                string sCurnCarouselID = string.Empty;
                string sCells = string.Empty;
                DataTable dt = App.Local_SQLServer.SelectDB("*", "[CAROUSEL_CHECK_LIST_DETAIL]", $"[COMMAND_ID] = '{sCommandID_}'");
                CAROUSEL_LIST_ITEM item;
                foreach (DataRow dr in dt.Rows)
                {
                    sCurnCarouselID = dr["CAROUSEL_ID"].ToString().Trim();
                    if ((item = carousel_list.Find(x => x != null && x.CAROUSEL_ID == sCurnCarouselID)) != null) //已經存在
                    {
                        item.CELL_ID += "," + dr["CELL_ID"].ToString().Trim();
                    }
                    else
                    {
                        carousel_list.Add(new CAROUSEL_LIST_ITEM()
                        {
                            CAROUSEL_ID = sCurnCarouselID,
                            CELL_ID = dr["CELL_ID"].ToString().Trim()
                        });
                    }
                }

                C020_JSON = new ToSTK_ITEM()
                {
                    CMD_NO = "C020",
                    DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    COMMAND_ID = sCommandID_,
                    CAROUSEL_LIST = carousel_list,
                    ACTION = sAction_
                };
                SendCMD(JSON.Serialize(C020_JSON));

                Timeout_Timer[(int)eC_CMD_NO.C020].Enable(true);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void S020_ACK(FromSTK_ITEM RcvItem_)
        {
            try
            {
                Timeout_Timer[(int)eC_CMD_NO.C020].Enable(false);
                if (RcvItem_.REPLY == "ACCEPT")
                {
                    string sSQL = $"UPDATE [CAROUSEL_CHECK_LIST] SET [STATUS] = {(int)BC.eCarousel_Check_Task_STATUS.eAction}, [START_TIME] = '{DateTime.Now.ToString("yyyyMMddHHmmssfff")}' WHERE [COMMAND_ID] = '{RcvItem_.COMMAND_ID}'";
                    App.Local_SQLServer.NonQuery(sSQL);
                }
                else
                {
                    string sSQL = $"UPDATE [CAROUSEL_CHECK_LIST] SET [STATUS] = {(int)BC.eCarousel_Check_Task_STATUS.eFINISH}, [START_TIME] = '{DateTime.Now.ToString("yyyyMMddHHmmssfff")}', [RESULT] = 'NG', [NG_REASON] = 'STK Reject : {RcvItem_.DETAIL}' WHERE [COMMAND_ID] = '{RcvItem_.COMMAND_ID}'";
                    App.Local_SQLServer.NonQuery(sSQL);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void S021_ACK(FromSTK_ITEM RcvItem_)
        {
            try
            {
				string sSQL = $"UPDATE [CAROUSEL_CHECK_LIST] SET [STATUS] = {(int)BC.eCarousel_Check_Task_STATUS.eFINISH}, [RESULT] = '{RcvItem_.RESULT}', [NG_REASON] = '{RcvItem_.DETAIL}' WHERE [COMMAND_ID] = '{RcvItem_.COMMAND_ID}'"; //將盤點任務狀態改成完畢
				App.Local_SQLServer.NonQuery(sSQL);
                for(int i = 0; i < RcvItem_.CAROUSEL_LIST.Count; i++)
				{
                    string sSQL_tmp = $"SELECT CHECK_RESULT FROM [CELL_STATUS] WHERE CAROUSEL_ID='{RcvItem_.CAROUSEL_LIST[i].CAROUSEL_ID}' AND CELL_ID='{RcvItem_.CAROUSEL_LIST[i].CELL_ID}'";
                    DataTable dt_tmp = new DataTable();
                    App.STK_SQLServer.Query(sSQL_tmp, ref dt_tmp);
                    sSQL = $"UPDATE [CAROUSEL_CHECK_LIST_DETAIL] SET [CHECK_RESULT] ={dt_tmp.Rows[0]["CHECK_RESULT"].ToString().Trim()} WHERE [COMMAND_ID]='{RcvItem_.COMMAND_ID}'";
                    App.Local_SQLServer.NonQuery(sSQL);
				}
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        //刪帳
        public void S030_EVENT(FromSTK_ITEM RcvItem_)
        {
            try
            {
                string date = RcvItem_.DATE;
                string carouselid = RcvItem_.CAROUSEL_ID;
                string cellid = RcvItem_.CELL_ID;

                C030_REPLY(RcvItem_);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void C030_REPLY(FromSTK_ITEM RcvItem_)
        {
            ToSTK_ITEM C030_JSON;
            try
            {
                string sReply = "ACCEPT";
                string sDetail = string.Empty;
                C030_JSON = new ToSTK_ITEM()
                {
                    CMD_NO = "C030",
                    DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    CAROUSEL_ID = RcvItem_.CAROUSEL_ID,
                    CELL_ID = RcvItem_.CELL_ID,
                    REPLY = sReply,
                    DETAIL = sDetail
                };
                SendCMD(JSON.Serialize(C030_JSON));
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void C031_CMD(string sCarouselID, string sCellID)
        {
            ToSTK_ITEM C031_JSON;
            try
            {
                C031_JSON = new ToSTK_ITEM()
                {
                    CMD_NO = "C031",
                    DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    CAROUSEL_ID = sCarouselID,
                    CELL_ID = sCellID
                };
                SendCMD(JSON.Serialize(C031_JSON));
                Timeout_Timer[(int)eC_CMD_NO.C031].Enable(true);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void S031_ACK(FromSTK_ITEM RcvItem_)
        {
            try
            {
                Timeout_Timer[(int)eC_CMD_NO.C031].Enable(false);

                string date = RcvItem_.DATE;
                string carouselid = RcvItem_.CAROUSEL_ID;
                string cellid = RcvItem_.CELL_ID;
                string reply = RcvItem_.REPLY;
                string detail = RcvItem_.DETAIL;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        //異常狀態回報
        public void S040_EVENT(FromSTK_ITEM RcvItem_)
        {
            try
            {
                string date = RcvItem_.DATE;
                string alarmpos = RcvItem_.ALARM_POS;
                string alarmcode = RcvItem_.ALARM_CODE;
                string alarmlevel = RcvItem_.ALARM_LEVEL;
                string alarmdesc = RcvItem_.ALARM_DESC;
                string alarmstatus = RcvItem_.ALARM_STATUS;
                C040_REPLY(RcvItem_);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void C040_REPLY(FromSTK_ITEM RcvItem_)
        {
            ToSTK_ITEM C040_JSON;
            try
            {
                C040_JSON = new ToSTK_ITEM()
                {
                    CMD_NO = "C040",
                    DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    ALARM_POS = RcvItem_.ALARM_POS,
                    ALARM_CODE = RcvItem_.ALARM_CODE,
                    ALARM_LEVEL = RcvItem_.ALARM_LEVEL,
                    ALARM_DESC = RcvItem_.ALARM_DESC,
                    ALARM_STATUS = RcvItem_.ALARM_STATUS
                };
                SendCMD(JSON.Serialize(C040_JSON));
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        //更新監測設定
        public void C050_CMD(uint uPeriod)
        {
            ToSTK_ITEM C050_JSON;
            try
            {
                C050_JSON = new ToSTK_ITEM()
                {
                    CMD_NO = "C050",
                    DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    MONITOR_PERIOD = uPeriod.ToString()
                };
                SendCMD(JSON.Serialize(C050_JSON));

                Timeout_Timer[(int)eC_CMD_NO.C050].Enable(true);
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void S050_ACK(FromSTK_ITEM RcvItem_)
        {
            try
            {
                Timeout_Timer[(int)eC_CMD_NO.C050].Enable(false);

                string date = RcvItem_.DATE;
                string cmdid = RcvItem_.COMMAND_ID;
                string monitorperiod = RcvItem_.MONITOR_PERIOD;
                string reply = RcvItem_.REPLY;
                string detail = RcvItem_.DETAIL;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
#endregion STK_TCPIP_Function
    }
#region Json Format
    public class ToSTK_ITEM
    {
        public string CMD_NO { get; set; }
        public string DATE { get; set; }
        public string CMD { get; set; }
        public string REPLY { get; set; }
        public string DETAIL { get; set; }
        public string COMMAND_ID { get; set; }
        public string ACTION { get; set; }
        public string CAR_ID { get; set; }
        public string BOX_ID { get; set; }
        public string BATCH_NO { get; set; }
        public string SOTERIA { get; set; }
        public string CUSTOMER_ID { get; set; }
        public string POSITION { get; set; }
        public string SOURCE { get; set; }
        public string S_CELL_ID { get; set; }
        public string TARGET { get; set; }
        public string T_CELL_ID { get; set; }
        public string CAROUSEL_ID { get; set; }
        public List<CAROUSEL_LIST_ITEM> CAROUSEL_LIST { get; set; }
        public string CELL_ID { get; set; }
        public string ALARM_POS { get; set; }
        public string ALARM_CODE { get; set; }
        public string ALARM_LEVEL { get; set; }
        public string ALARM_DESC { get; set; }
        public string ALARM_STATUS { get; set; }
        public string MONITOR_PERIOD { get; set; }
    }
    public class FromSTK_ITEM
    {
        public string CMD_NO { get; set; }
        public string DATE { get; set; }
        public string CMD { get; set; }
        public string REPLY { get; set; }
        public string DETAIL { get; set; }
        public string COMMAND_ID { get; set; }
        public string ACTION { get; set; }
        public string CAR_ID { get; set; }
        public string BOX_ID { get; set; }
        public string BATCH_NO { get; set; }
        public string SOTERIA { get; set; }
        public string POSITION { get; set; }
        public string SOURCE { get; set; }
        public string S_CELL_ID { get; set; }
        public string TARGET { get; set; }
        public string T_CELL_ID { get; set; }
        public string STATUS { get; set; }
        public string CAROUSEL_ID { get; set; }
        public List<CAROUSEL_LIST_ITEM> CAROUSEL_LIST { get; set; }
        public string RESULT { get; set; }
        public string CELL_ID { get; set; }
        public string ALARM_POS { get; set; }
        public string ALARM_CODE { get; set; }
        public string ALARM_LEVEL { get; set; }
        public string ALARM_DESC { get; set; }
        public string ALARM_STATUS { get; set; }
        public string MONITOR_PERIOD { get; set; }
    }
    public class CAROUSEL_LIST_ITEM
    {
        public string CAROUSEL_ID { get; set; }
        public string CELL_ID { get; set; }
    }
#endregion Json Format

    public class Car
    {
        public string CAR_ID { get; set; }
        public string BOX_ID { get; set; }
        public string BATCH_NO { get; set; }
    }

    public class CAR_ACTION_CMD : Car
    {
        public string ACTION { get; set; }
        public string SOTERIA { get; set; }
        public string CUSTOMER_ID { get; set; }
        public string POSITION { get; set; }
        public string SOURCE { get; set; }
        public string S_CELL_ID { get; set; }
        public string TARGET { get; set; }
        public string T_CELL_ID { get; set; }
        public string STATUS { get; set; }
    }

    public class cCarousel : INotifyPropertyChanged
    {
        /// <INotifyPropertyChanged>
        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName_ = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName_));
        }
        /// </INotifyPropertyChanged>

        private string carousel_ID;

        public string Carousel_ID
        {
            get { return carousel_ID; }
            set
            {
                if (carousel_ID == value) return;
                carousel_ID = value;
                NotifyPropertyChanged();
            }
        }

        private int status;

        public int Status
        {
            get { return status; }
            set
            {
                if (status == value) return;
                status = value;
                NotifyPropertyChanged();
            }
        }

        private int store_Status;

        public int Store_Status
        {
            get { return store_Status; }
            set
            {
                if (store_Status == value) return;
                store_Status = value;
                NotifyPropertyChanged();
            }
        }

        private int n2_Status;

        public int N2_Status
        {
            get { return n2_Status; }
            set
            {
                if (n2_Status == value) return;
                n2_Status = value;
                NotifyPropertyChanged();
            }
        }

        private double temperature;

        public double Temperature
        {
            get { return temperature; }
            set
            {
                if (temperature == value) return;
                temperature = value;
                NotifyPropertyChanged();
            }
        }

        private int temperature_Status;

        public int Temperature_Status
        {
            get { return temperature_Status; }
            set
            {
                if (temperature_Status == value) return;
                temperature_Status = value;
                NotifyPropertyChanged();
            }
        }

        private double humidity;

        public double Humidity
        {
            get { return humidity; }
            set
            {
                if (humidity == value) return;
                humidity = value;
                NotifyPropertyChanged();
            }
        }

        private int humidity_Status;

        public int Humidity_Status
        {
            get { return humidity_Status; }
            set
            {
                if (humidity_Status == value) return;
                humidity_Status = value;
                NotifyPropertyChanged();
            }
        }

        private string command_ID;

        public string Command_ID
        {
            get { return command_ID; }
            set
            {
                if (command_ID == value) return;
                command_ID = value;
                NotifyPropertyChanged();
            }
        }

        private int check_Status;

        public int Check_Status
        {
            get { return check_Status; }
            set
            {
                if (check_Status == value) return;
                check_Status = value;
                NotifyPropertyChanged();
            }
        }

    }

    public class CarouselMonitor : INotifyPropertyChanged
    {
        /// <INotifyPropertyChanged>
        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName_ = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName_));
        }
        /// </INotifyPropertyChanged>

        private string carousel_ID;

        public string CAROUSEL_ID
        {
            get { return carousel_ID; }
            set
            {
                if (carousel_ID == value) return;
                carousel_ID = value;
                NotifyPropertyChanged();
            }
        }

        private double temperature_upper_limit;

        public double TEMPERATURE_UPPER_LIMIT
        {
            get { return temperature_upper_limit; }
            set
            {
                if (temperature_upper_limit == value) return;
                temperature_upper_limit = value;
                NotifyPropertyChanged();
            }
        }

        private double temperature_lower_limit;

        public double TEMPERATURE_LOWER_LIMIT
        {
            get { return temperature_lower_limit; }
            set
            {
                if (temperature_lower_limit == value) return;
                temperature_lower_limit = value;
                NotifyPropertyChanged();
            }
        }

        private double humidity_upper_limit;

        public double HUMIDITY_UPPER_LIMIT
        {
            get { return humidity_upper_limit; }
            set
            {
                if (humidity_upper_limit == value) return;
                humidity_upper_limit = value;
                NotifyPropertyChanged();
            }
        }

        private double humidity_lower_limit;

        public double HUMIDITY_LOWER_LIMIT
        {
            get { return humidity_lower_limit; }
            set
            {
                if (humidity_lower_limit == value) return;
                humidity_lower_limit = value;
                NotifyPropertyChanged();
            }
        }
        //
        private double turn_on_n2_humidity;

        public double TURN_ON_N2_HUMIDITY
        {
            get { return turn_on_n2_humidity; }
            set
            {
                if (turn_on_n2_humidity == value) return;
                turn_on_n2_humidity = value;
                NotifyPropertyChanged();
            }
        }

        private double turn_off_n2_humidity;

        public double TURN_OFF_N2_HUMIDITY
        {
            get { return turn_off_n2_humidity; }
            set
            {
                if (turn_off_n2_humidity == value) return;
                turn_off_n2_humidity = value;
                NotifyPropertyChanged();
            }
        }
    }
}
