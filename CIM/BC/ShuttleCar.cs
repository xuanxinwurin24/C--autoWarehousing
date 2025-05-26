using CIM.UILog;
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
	public class ShuttleCar : INotifyPropertyChanged
	{
		/// <INotifyPropertyChanged>
		public event PropertyChangedEventHandler PropertyChanged;
		void NotifyPropertyChanged([CallerMemberName] string propertyName_ = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName_));
		}
		/// </INotifyPropertyChanged>

		#region Declare
		public frmEqLog frmLog;
		private LogWriter LogFile;
		public void LOG(string sLog_)
		{
			frmLog.Log(sLog_);
			//frmLog.Delete_SQL_Log("Shuttle");
			string total = DateTime.Now.ToString("yyyyMMddHHmmss").Trim();
			string SQL_Temp = $"INSERT INTO [LOG_RECORD] VALUES('Shuttle','{total}','{sLog_}')";
			App.Local_SQLServer.NonQuery(SQL_Temp);
		}
		int iCarNo;
		private uint Unit = 0;
		public uint AlarmOffset = 0;
		private TcpIp TcpIpConnect;
		ThreadTimer ReadTimer, ConnectTimer;
		ThreadTimer[] Timeout_Timer;
		private bool bconnected;
		public bool bConnected
		{
			get { return bconnected; }
			set
			{
				if (bconnected == value) return;
				bconnected = value;
				App.Bc.wAux["ShuttleCar_Connect"].BinValue = Convert.ToUInt16(bconnected);
				NotifyPropertyChanged();
				C001_CMD("ONLINE");
			}
		}

		private bool bonline = false;
		public bool bONLINE
        {
			get { return bonline; }
			set
			{
				bonline = value;
				App.Bc.wAux["ShuttleCar_ONLINE"].BinValue = Convert.ToUInt16(bonline);
				NotifyPropertyChanged();
			}
        }
		#endregion Declare

		public ShuttleCar(int iCarNo_)
        {
			iCarNo = iCarNo_;
			frmLog = MainWindow.ucUILog.ShuttleLog;
			LogFile = new LogWriter(App.sSysDir + @"\LogFile\Eq\Unit\", "ShuttleCar" + iCarNo.ToString(), 5000 * 1000);

			ReadTimer = App.MainThread.TimerCreate("ReadTimer" + iCarNo.ToString(), 500, ReadTimer_Event, ThreadTimer.eType.Cycle);
			Timeout_Timer = new ThreadTimer[Enum.GetNames(typeof(eC_CMD_NO)).Count()];
			for (int i = 0; i < Timeout_Timer.Length; i++)
			{
				Timeout_Timer[i] = App.MainThread.TimerCreate(((eC_CMD_NO)i).ToString() + "Timeout_Timer" + iCarNo.ToString(), 60000, Timeout_Timer_Event, ThreadTimer.eType.Hold);
				Timeout_Timer[i].Tag = i + 1; // (eC_CMD_NO)i;
			}
			TcpIpConnect = new TcpIp();
			if (iCarNo == 1)
				TcpIpConnect.Initial(true, App.SysPara.ShuttleCar1_Port, App.SysPara.ShuttleCar1_IP);
			else
				TcpIpConnect.Initial(true, App.SysPara.ShuttleCar2_Port, App.SysPara.ShuttleCar2_IP);

			TcpIpConnect.bCommunicationRawDataLog = true;
			TcpIpConnect.RawLogFile.MaxSize = 5000000;
			TcpIpConnect.RawLogFile.PathName = "./LogFile/ToShuttleCar" + iCarNo.ToString();
			TcpIpConnect.RawLogFile.sHead = "ToShuttleCar" + iCarNo.ToString();
			TcpIpConnect.ConnectedCallBack += new TcpIp.ConnectCallBackHandler(ConnectCallBackEvent);
			ConnectTimer = App.MainThread.TimerCreate("ConnectTimer" + iCarNo.ToString(), 5000, ConnectTimer_Event, ThreadTimer.eType.Cycle);
			ConnectTimer.Enable(true);
			//TcpIpConnect.Start();
		}

		private void ConnectCallBackEvent(bool bConnected_)
		{
			bConnected = bConnected_;
			ReadTimer.Enable(bConnected_);
			LOG(bConnected ? $"Car{iCarNo} TCP/IP Connected" : $"Car{iCarNo} TCP/IP Disconnect");
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

		private int ConnectTimer_Event(ThreadTimer Sender)
		{
			if (bConnected == false)
				TcpIpConnect.Start();
			return 0;
		}

		private int Timeout_Timer_Event(ThreadTimer Sender)
		{
			App.Alarm.Set(1, /*AlarmOffset +*/ uint.Parse(Sender.Tag.ToString()), true, $"Shuttle Car {((eC_CMD_NO)(Sender.Tag)).ToString()} Timeout");
			return 0;
		}

		#region ShuttleCar_TCPIP_Function
		public void SendCMD(string sSendMsg_)
		{
			if (!bConnected)
				return;
			byte[] Value = Encoding.ASCII.GetBytes(sSendMsg_);
			TcpIpConnect.Send(Value, Value.Length);
			LOG("Send : " + sSendMsg_);
		}

		public enum eC_CMD_NO
		{
			C001, C002, C010, C020, C030, C031, C040
		}

		public void CheckS_Cmd_No(string sAckMsg)
		{
			List<string> sLsTemp = new List<string>();
			FromShuttleCar_ITEM RcvItem;// = new FromShuttleCar_ITEM();
			try
			{
				sLsTemp = sAckMsg.Split('}').ToList();
				for (int i = 0; i < sLsTemp.Count - 1; i++)
				{
					LOG("Received : " + sLsTemp[i].ToString() + "}");
					RcvItem = JSON.DeSerialize<FromShuttleCar_ITEM>(sLsTemp[i] + "}");
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
							S050_EVENT(RcvItem);
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
			ToShuttleCar_ITEM C001_JSON;
			try
			{
				C001_JSON = new ToShuttleCar_ITEM()
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
		public void S001_ACK(FromShuttleCar_ITEM RcvItem_)
		{
			try
			{
				Timeout_Timer[(int)eC_CMD_NO.C001].Enable(false);
				if (RcvItem_.REPLY == "ACCEPT")
				{
					bONLINE = true;
					if (App.Bc.wAux["isOnlyDemo"].BinValue == 1)
					{
						string sql = $"UPDATE [CAR_STATUS] SET [STATUS] = 'IDLE' WHERE [CAR_ID]='CAR00{iCarNo}'";
						App.Local_SQLServer.NonQuery(sql);
					}
				}
				else
				{
					bONLINE = false;
					if (Properties.Settings.Default.DefaultCulture.Name == "zh-TW")
						App.Alarm.Set(Unit, AlarmOffset + 201, true, "請求穿梭車狀態變為 " + RcvItem_.CMD + " 但穿梭車拒絕 原因是 : " + RcvItem_.DETAIL);
					else
						App.Alarm.Set(Unit, AlarmOffset + 201, true, "Request Shuttle Car Change State To " + RcvItem_.CMD + " but Shuttle Car REJECT DETAIL : " + RcvItem_.DETAIL);
				}
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
			}
		}
		public void S002_EVENT(FromShuttleCar_ITEM RcvItem_)
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
			ToShuttleCar_ITEM C002_JSON;
			try
			{
				C002_JSON = new ToShuttleCar_ITEM()
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
			ToShuttleCar_ITEM C010_JSON;
			try
			{
				C010_JSON = new ToShuttleCar_ITEM()
				{
					CMD_NO = "C010",
					DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
					COMMAND_ID = sCommandID,
					ACTION = carACTION.ACTION,
					CAR_ID = carACTION.CAR_ID,
					BOX_ID = carACTION.BOX_ID,
					POSITION = carACTION.POSITION,
					SOURCE = carACTION.SOURCE,
					TARGET = carACTION.TARGET
				};
				SendCMD(JSON.Serialize(C010_JSON));

				Timeout_Timer[(int)eC_CMD_NO.C010].Enable(true);
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
			}
		}
		public void S010_ACK(FromShuttleCar_ITEM RcvItem_)
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
					sql = $"UPDATE [CAR_STATUS] SET ACTION='{action}',CURRENT_POSITION='{pos}',BOX_ID='{boxid}',STATUS='{status}' where CAR_ID='{carid}'";
					App.Local_SQLServer.NonQuery(sql);

					sql = $"UPDATE [CAR_CMD] SET [SHUTTLECAR_CMD_RESULT] = '1' WHERE COMMAND_ID = '{RcvItem_.COMMAND_ID}'";
					App.Local_SQLServer.NonQuery(sql);
					
					if (App.Bc.wAux["isOnlyDemo"].BinValue == 1)
					{
						sql = $"UPDATE [CAR_CMD] SET [STK_CMD_RESULT] = '1' WHERE COMMAND_ID='{cmdid}'";
						App.Local_SQLServer.NonQuery(sql);
					}
				}
				else
				{
					LOG($"STK拒絕任務,CMD ID:{RcvItem_.COMMAND_ID},拒絕原因:{detail}");
					sql = $"UPDATE [TASK] SET NG_REASON='{detail}' WHERE BOX_ID='{boxid}'";
					App.Local_SQLServer.NonQuery(sql);
					sql = $"UPDATE [CAR_CMD] SET [SHUTTLECAR_CMD_RESULT] = '2' WHERE COMMAND_ID = '{RcvItem_.COMMAND_ID}'";
					App.Local_SQLServer.NonQuery(sql);
				}
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
			}
		}
		public void S011_ACK(FromShuttleCar_ITEM RcvItem_)
		{
			try
			{
				string sql = $"UPDATE [CAR_STATUS] SET ACTION='{RcvItem_.ACTION}',CURRENT_POSITION='{RcvItem_.POSITION}',BOX_ID='{RcvItem_.BOX_ID}',STATUS='{RcvItem_.STATUS}' where CAR_ID='{RcvItem_.CAR_ID}'";
				App.Local_SQLServer.NonQuery(sql);
				
				if (RcvItem_.STATUS == "COMPLETE") //動作完成
                {
					if (RcvItem_.TARGET == "LIFTER_A") //放到LIFTER1 = LIFTER A 完成
                    {
						SendBoxToLifter(RcvItem_.BOX_ID, "A"); //要告訴Lifter A 放進去的BOX ID
						sql = $"UPDATE [TASK] SET STATUS='{(int)TaskCtrl.eTASK_Flow.OutToLifter}' WHERE COMMAND_ID = '{RcvItem_.COMMAND_ID}'";
						App.Local_SQLServer.NonQuery(sql); 
						if (App.Bc.wAux["isOnlyDemo"].BinValue == 1) //入庫平台而且是Demo模式，直接當作已入庫
						{
							DataTable dt = App.Local_SQLServer.SelectDB("*", "TASK", $"[COMMAND_ID] = '{RcvItem_.COMMAND_ID}'");
							if (dt.Rows.Count > 0)
							{
								sql = $"UPDATE [CELL_STATUS] SET BOX_ID='', BATCH_NO='' WHERE [CAROUSEL_ID] = '{dt.Rows[0]["TAR_POS"].ToString().Trim()}' AND [CELL_ID] = '{dt.Rows[0]["TAR_CELL_ID"].ToString().Trim()}'";
								App.STK_SQLServer.NonQuery(sql);
							}
						}
					}
					else if (RcvItem_.TARGET == "LIFTER_B") //放到LIFTER2 = LIFTER B 完成
					{
						SendBoxToLifter(RcvItem_.BOX_ID, "B"); //要告訴Lifter B 放進去的BOX ID
						sql = $"UPDATE [TASK] SET STATUS='{(int)TaskCtrl.eTASK_Flow.OutToLifter}' WHERE COMMAND_ID = '{RcvItem_.COMMAND_ID}'";
						App.Local_SQLServer.NonQuery(sql);
						if (App.Bc.wAux["isOnlyDemo"].BinValue == 1) //入庫平台而且是Demo模式，直接當作已入庫
						{
							DataTable dt = App.Local_SQLServer.SelectDB("*", "TASK", $"[COMMAND_ID] = '{RcvItem_.COMMAND_ID}'");
							if (dt.Rows.Count > 0)
							{
								sql = $"UPDATE [CELL_STATUS] SET BOX_ID='', BATCH_NO='' WHERE [CAROUSEL_ID] = '{dt.Rows[0]["TAR_POS"].ToString().Trim()}' AND [CELL_ID] = '{dt.Rows[0]["TAR_CELL_ID"].ToString().Trim()}'";
								App.STK_SQLServer.NonQuery(sql);
							}
						}
					}
					else if (RcvItem_.TARGET == "EXCHANGE") //放到EXCHANGE平台
					{ //要將狀態改回IDLE，重新派送從交換平台放到STK平台
						sql = $"UPDATE [TASK] SET STATUS='1', [USE_TEMP_STATUS] = '{(int)TaskCtrl.eTempStage_Status.eInTemp}' WHERE COMMAND_ID = '{RcvItem_.COMMAND_ID}'";
						App.Local_SQLServer.NonQuery(sql);
					}
					else if (App.Bc.wAux["isOnlyDemo"].BinValue == 1) //入庫平台而且是Demo模式，直接當作已入庫
					{
						DataTable dt = App.Local_SQLServer.SelectDB("*", "TASK" , $"[COMMAND_ID] = '{RcvItem_.COMMAND_ID}'");
						if (dt.Rows.Count > 0)
						{
							sql = $"UPDATE [CELL_STATUS] SET BOX_ID='{dt.Rows[0]["BOX_ID"].ToString().Trim()}', BATCH_NO='{dt.Rows[0]["BATCH_NO"].ToString().Trim()}' WHERE [CAROUSEL_ID] = '{dt.Rows[0]["TAR_POS"].ToString().Trim()}' AND [CELL_ID] = '{dt.Rows[0]["TAR_CELL_ID"].ToString().Trim()}'";
							App.STK_SQLServer.NonQuery(sql);
						}
						sql = $"UPDATE [TASK] SET STATUS='{(int)TaskCtrl.eTASK_Flow.Complete}' WHERE COMMAND_ID = '{RcvItem_.COMMAND_ID}'";
						App.Local_SQLServer.NonQuery(sql);
					}
				}
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
			}
		}
		//刪帳
		public void S030_EVENT(FromShuttleCar_ITEM RcvItem_)
		{
			try
			{
				string date = RcvItem_.DATE;
				string carid = RcvItem_.CAR_ID;
				string boxid = RcvItem_.BOX_ID;

				C030_REPLY(RcvItem_);
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
			}
		}
		public void C030_REPLY(FromShuttleCar_ITEM RcvItem_)
		{
			ToShuttleCar_ITEM C030_JSON;
			try
			{
				string sReply = string.Empty;
				string sDetail = string.Empty;
				C030_JSON = new ToShuttleCar_ITEM()
				{
					CMD_NO = "C030",
					DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
					CAR_ID = RcvItem_.CAR_ID,
					BOX_ID = RcvItem_.BOX_ID,
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
		public void C031_CMD(string sCarID, string sBoxID)
		{
			ToShuttleCar_ITEM C031_JSON;
			try
			{
				C031_JSON = new ToShuttleCar_ITEM()
				{
					CMD_NO = "C031",
					DATE = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
					CAR_ID = sCarID,
					BOX_ID = sBoxID
				};
				SendCMD(JSON.Serialize(C031_JSON));
				Timeout_Timer[(int)eC_CMD_NO.C031].Enable(true);
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
			}
		}
		public void S031_ACK(FromShuttleCar_ITEM RcvItem_)
		{
			try
			{
				Timeout_Timer[(int)eC_CMD_NO.C031].Enable(false);

				string date = RcvItem_.DATE;
				string carid = RcvItem_.CAR_ID;
				string boxid = RcvItem_.BOX_ID;
				string reply = RcvItem_.REPLY;
				string detail = RcvItem_.DETAIL;
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
			}
		}
		//異常狀態回報
		public void S040_EVENT(FromShuttleCar_ITEM RcvItem_)
		{
			try
			{
				string date = RcvItem_.DATE;
				string alarmpos = RcvItem_.ALARM_POS;
				string alarmcode = RcvItem_.ALARM_CODE;
				string alarmlevel = RcvItem_.ALARM_LEVEL;
				string alarmdesc = RcvItem_.ALARM_DESC;
				string alarmstatus = RcvItem_.ALARM_STATUS;
				App.Alarm.Set(Unit, uint.Parse(alarmcode), alarmstatus == "S", alarmdesc);
				C040_REPLY(RcvItem_);
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
			}
		}
		public void C040_REPLY(FromShuttleCar_ITEM RcvItem_)
		{
			ToShuttleCar_ITEM C040_JSON;
			try
			{
				string sReply = string.Empty;
				string sDetail = string.Empty;
				C040_JSON = new ToShuttleCar_ITEM()
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
		//車子狀態回報
		public void S050_EVENT(FromShuttleCar_ITEM RcvItem_)
		{
			try
			{
				string sCommandID = string.Empty;
				string sBatchNo = string.Empty;
				if (RcvItem_.BOX_ID != "")
                {
					DataTable dt = App.Local_SQLServer.SelectDB("COMMAND_ID, BATCH_NO", "TASK", $"[BOX_ID = {RcvItem_.BOX_ID}]");
					if (dt.Rows.Count != 0)
                    {
						sCommandID = dt.Rows[0]["COMMAND_ID"].ToString().Trim();
						sBatchNo = dt.Rows[0]["BATCH_NO"].ToString().Trim();
					}
                }
				string sql = $"UPDATE [CAR_STATUS] SET CURRENT_POSITION='{RcvItem_.CURN_POS}',BOX_EXIST='{RcvItem_.BOX_EXIST}',BOX_ID='{RcvItem_.BOX_ID}',STATUS='{RcvItem_.STATUS}', COMMAND_ID = '{sCommandID}', BATCH_NO = '{sBatchNo}' where CAR_ID='{RcvItem_.CAR_ID}'";
				App.Local_SQLServer.NonQuery(sql);
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
			}
		}
		#endregion ShuttleCar_TCPIP_Function

		#region SendBoxToLifter_Function
		public void SendBoxToLifter(string sBoxID_, string sLifterAB_)
        {
			App.Bc.TaskCtrl.LIFTER_SQLHS.Req("ShuttleCar"+ sLifterAB_, sBoxID_);
		}
		#endregion SendBoxToLifter_Function
	}
	#region Json Format
	public class ToShuttleCar_ITEM
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
		public string POSITION { get; set; }
		public string SOURCE { get; set; }
		public string TARGET { get; set; }
		public string ALARM_POS { get; set; }
		public string ALARM_CODE { get; set; }
		public string ALARM_LEVEL { get; set; }
		public string ALARM_DESC { get; set; }
		public string ALARM_STATUS { get; set; }
	}
	public class FromShuttleCar_ITEM
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
		public string POSITION { get; set; }
		public string SOURCE { get; set; }
		public string TARGET { get; set; }
		public string STATUS { get; set; }
		public string ALARM_POS { get; set; }
		public string ALARM_CODE { get; set; }
		public string ALARM_LEVEL { get; set; }
		public string ALARM_DESC { get; set; }
		public string ALARM_STATUS { get; set; }
		public string CURN_POS { get; set; }
		public string BOX_EXIST { get; set; }
	}
	#endregion Json Format
}
