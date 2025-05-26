using System;
using Strong;
using System.Data.SqlClient;
using System.Data;
using CIM.Lib.Model;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CIM
{
	public class SQL_DB
	{
		private LogWriter logWriter;
		//private SqlConnection sqlConn;
		private SqlConnectionStringBuilder connStringBuilder;
		public SQL_Parameter SQLPara;
		public SQL_DB(SQL_Parameter connPara)
		{
			SQLPara = connPara;
			logWriter = new LogWriter(Environment.CurrentDirectory + @"\LogFile\SQL", "SQL", 500 * 1000);
			connStringBuilder = new SqlConnectionStringBuilder();
			//sqlConn = new SqlConnection();
			ConnectToDB();
		}

		public bool ConnectToDB()
		{
			bool Result = false;
			try
			{
				//初始化SQL連接參數
				connStringBuilder.DataSource = SQLPara.DataSource;
				connStringBuilder.InitialCatalog = SQLPara.InitialCatalog;
				connStringBuilder.MultipleActiveResultSets = true;
				if (string.IsNullOrEmpty(SQLPara.UserID))
				{
					connStringBuilder.IntegratedSecurity = true;
				}
				else
				{
					connStringBuilder.IntegratedSecurity = false;
					connStringBuilder.UserID = SQLPara.UserID;
					connStringBuilder.Password = SQLPara.Password;
				}

				//將參數設定寫入連接端
				//sqlConn.ConnectionString = connStringBuilder.ToString();

				//進行重新連線
				//if (sqlConn.State != ConnectionState.Open) sqlConn.Close();
				//sqlConn.Open();
				Result = true;
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
			return Result;
		}
		public bool Query(string SqlStr_, ref DataTable Table_, bool logSkip_ = false)
		{
			//將DataTable裡的資料清除乾淨
			Table_.Clear();
			Table_.Columns.Clear();
			Table_.Rows.Clear();
			bool Result = true;
			//SqlConnection sqlConn = new SqlConnection(string.Format(@"data source = {0}; initial catalog = {1}; Uid = {2}; Pwd = {3};",
			//												App.SysPara.SqlDB_data_source,
			//												App.SysPara.SqlDB_catalog,
			//												App.SysPara.SqlDB_uid,
			//												App.SysPara.SqlDB_password
			//											));
			SqlConnection sqlConn = new SqlConnection();
			sqlConn.ConnectionString = connStringBuilder.ToString();
			SqlCommand sqlCmd = new SqlCommand(SqlStr_, sqlConn);
			try
			{
				if (sqlConn.State != ConnectionState.Open) sqlConn.Open();
				if (sqlConn.State != ConnectionState.Open) return false;

				SqlDataReader dataReader = sqlCmd.ExecuteReader();

				if (dataReader.HasRows)
				{
					for (int i = 0; i < dataReader.FieldCount; i++)
						Table_.Columns.Add(dataReader.GetName(i), typeof(string));

					while (dataReader.Read())
					{
						object[] tmpObject = new object[dataReader.FieldCount];
						for (int i = 0; i < dataReader.FieldCount; i++)
							tmpObject[i] = dataReader[i];

						Table_.Rows.Add(tmpObject);
					}
				}
				if (logSkip_ == false)
					logWriter.AddString("SQL: " + SqlStr_);
			}
			catch (Exception ex)
			{
				Result = false;
				logWriter.AddString("SQL Exception: " + SqlStr_ + "\r\n" + ex.ToString());
			}
			finally
			{
			    sqlCmd?.Cancel();
				sqlConn?.Close();
				//sqlConn.Dispose();
				//sqlConn = null;
			}
			return Result;
		}

		public int NonQuery(string SqlStr_, bool logSkip_ = false)
		{
			int AffectedCount = 0;
			//SqlConnection sqlConn = new SqlConnection(string.Format(@"data source = {0}; initial catalog = {1}; Uid = {2}; Pwd = {3};",
			//												App.SysPara.SqlDB_data_source,
			//												App.SysPara.SqlDB_catalog,
			//												App.SysPara.SqlDB_uid,
			//												App.SysPara.SqlDB_password
			//											));
			SqlConnection sqlConn = new SqlConnection();
			sqlConn.ConnectionString = connStringBuilder.ToString();
			SqlCommand sqlCmd = new SqlCommand(SqlStr_, sqlConn);
			try
			{
				if (sqlConn.State != ConnectionState.Open) sqlConn.Open();
				if (sqlConn.State != ConnectionState.Open) return 0;

				AffectedCount = sqlCmd.ExecuteNonQuery();
				if (logSkip_ == false)
					logWriter.AddString("SQL: " + SqlStr_);
			}
			catch (Exception ex)
			{
				AffectedCount = 0;
				logWriter.AddString("SQL Exception: " + SqlStr_ + "\r\n" + ex.ToString());
			}
			finally
			{
				sqlCmd?.Cancel();
				//sqlConn?.Close();
				//sqlConn.Dispose();
				//sqlConn = null;
			}
			return AffectedCount;
		}

		public DataTable SelectDB(string key, string tableName, string condition, bool logSkip_ = true)
		{
			DataTable dt = new DataTable();

			//if (App.DB_bypass) return dt;   //for test

			string SQL = $"select {key} from {tableName}";

			if (condition != "")
			{
				SQL += $" where {condition}";
			}

			Query(SQL, ref dt, logSkip_);
			return dt;
		}

	}
	public class SQL_Parameter
	{
		//Data Source = LAPTOP - 51PK35Q2\SQLEXPRESS;Initial Catalog = ASE; Persist Security Info=True;User ID = strong; Password=5999011
		public string DataSource { get; set; }
		public string InitialCatalog { get; set; }
		public string UserID { get; set; }
		public string Password { get; set; }
	}


	public class SQL_HS
	{
		ThreadTimer ReqTimer;
		private LogWriter LogWriter;
		private List<string> SetFindReq;     //B(Req=1) -> self
		private List<string> SetFindAck;     //B(Ack=1) -> self
		private List<string> SetFindSelfAck; //self(Ack=2) -> self(Ack=1)
		public SQL_HS()
		{
			ReqTimer = App.MainThread.TimerCreate("Req_Timer", 100, HS_Timer_Event, ThreadTimer.eType.Cycle);
			ReqTimer.Enable(true);
			LogWriter = new LogWriter(Environment.CurrentDirectory + @"\LogFile\SQL_HS", "SQL_HS", 500 * 1000);

			SetFindReq = new List<string> { "StoreA", "StoreB", "DeliveryA", "DeliveryB", "OrderNoA", "OrderNoB", "AlarmRptA", "AlarmRptB", "UserIDReqA", "UserIDReqB","StorageChange" };
			SetFindAck = new List<string> { "ShuttleCarA", "ShuttleCarB", "StorageChange" };
			SetFindSelfAck = new List<string> { };
		}


		public void Req(string sHSName_, string sRptData_)//主動
		{
			//Store or Delivery or ShuttleCar A/B
			string sql = $"UPDATE HS SET REQ = 1, REQDATA = '{sRptData_}' WHERE HS_NAME = '{sHSName_}'";

			int AffectRow = App.Local_SQLServer.NonQuery(sql);
			LogWriter.AddString(string.Format("SQL_HS REQDATA = {0}, AffectRoew = {1}", sRptData_, AffectRow));
		}
		public void Ack(string sHSName_, string sAck_, string sAckData_)//主動
		{
			string sAck = sAck_; //1 or 2
			string sql = $"UPDATE HS SET ACK = {sAck}, ACKDATA = '{sAckData_}' WHERE HS_NAME = '{sHSName_}'";

			int AffectRow = App.Local_SQLServer.NonQuery(sql);
			LogWriter.AddString(string.Format("SQL_HS AckData = {0}, AffectRow = {1}", sAckData_, AffectRow));
		}

		private void Clear(string sName_)
		{
			string sHSName = sName_;
			string sql = $"UPDATE HS SET REQ = 0, REQDATA = '', ACK = 0, ACKDATA = '' WHERE HS_NAME = '{sHSName}'";
			App.Local_SQLServer.NonQuery(sql);

		}

		private int HS_Timer_Event(ThreadTimer threadTimer_)
		{
			DataTable Result = App.Local_SQLServer.SelectDB("*", "HS", "[REQ] > 0"); //只抓有發交握的
			int iReq = 0, iAck = 0;
			string sHSName = string.Empty;

			foreach (DataRow dr in Result.Rows)
			{
				sHSName = dr["HS_NAME"].ToString().Trim();
				int.TryParse(dr["ACK"].ToString().Trim(), out iAck);
				int.TryParse(dr["REQ"].ToString().Trim(), out iReq);
				
				if (iAck == 2)
				{
					if (SetFindSelfAck.Contains(sHSName))
					{
						//已經向WebServer詢問過資料
					}
				}
				else if (iAck == 1)
				{
					if (SetFindAck.Contains(sHSName))
					{
						//一次進去一筆
						Ack_Passive(dr);
					}
				}
				else if (iReq == 1)
				{
					if (SetFindReq.Contains(sHSName))
					{
						//一次進去一筆
						Req_Passive(dr);
					}
				}
			}
			return 0;
		}
		public void Req_Passive(DataRow drData_)//Rev Req=1, Ack=0
		{
			//READATA 格式 = BOXID
			string sData = drData_["REQDATA"].ToString().Trim();
			string sHS_Name = drData_["HS_NAME"].ToString().Trim();
			string sLifter = string.Empty;
			string sBoxID = string.Empty;
			string sUserID = string.Empty;
			string sDirection = string.Empty;
			List<string> sTargetCarousel = new List<string>();
			List<string> sBoxIDList = new List<string>();
			List<string> sUserIDList = new List<string>();
			string sWAY = drData_["WAY"].ToString().Trim();
			LogWriter.AddString(string.Format("SQL_HS Rev {0} Req Data= {1} WAY={2}", sHS_Name, sData, sWAY));
            try
            {
				switch (sHS_Name)
				{
					case "StoreA":
					case "StoreB":
					case "DeliveryA":
					case "DeliveryB":
					case "StorageChange":
						if (sHS_Name == "StoreA")
						{
							sDirection = "IN";
							sLifter = "LIFTER_A";
						}
						else if (sHS_Name == "StoreB")
						{
							sDirection = "IN";
							sLifter = "LIFTER_B";
						}
						else if (sHS_Name == "DeliveryA")
						{
							sDirection = "OUT";
							sLifter = "LIFTER_A";
						}
						else if (sHS_Name == "DeliveryB")
						{
							sDirection = "OUT";
							sLifter = "LIFTER_B";
						}
						else if (sHS_Name == "StorageChange")
						{
							sDirection = "MOVE";
							int i = 0;
							string[] sData_tmp = sData.Split(',');
							while (i < sData_tmp.Length)
							{
								sBoxIDList.Add(sData_tmp[i].ToString().Trim());
								sUserIDList.Add(sData_tmp[i+1].ToString().Trim());
								sTargetCarousel.Add(sData_tmp[i+2].ToString().Trim());
								i+=3;
							}
						}
						else if (sHS_Name == "WebDelivery")
						{
							sDirection = "OUT";
							int i = 0;
							string[] sData_tmp = sData.Split(',');
							while (i < sData_tmp.Length)
							{
								sBoxIDList.Add(sData_tmp[i].ToString().Trim());
								sUserIDList.Add(sData_tmp[i + 1].ToString().Trim());
								i += 2;
							}
						}
						sBoxID = sData.Split(',')[0];
						sUserID = sData.Split(',')[1];

						//WebSerivce 詢問 IT 相關資料，收到回覆後寫到資料庫內，這邊再去撈資料庫
						//如果IT回覆NG，這一盒還是得進去，但是打NG
						if (sWAY == "NormalDelivery" ) //一般出庫，先把領料單號清掉，避免有殘留的領料單產品被用一般出庫叫出來，導致上報錯誤
						{
							string sSQL = $"UPDATE [BATCH_LIST] SET [ORDER_NO] = '',[END_TIME] = '{DateTime.Now.ToString("yyyyMMddHHmmssfff")}' WHERE [BOX_ID] = '{sBoxID}'";
							App.Local_SQLServer.NonQuery(sSQL);
						}
						else if(sWAY == "WebDelivery")
						{
							string endtime=DateTime.Now.ToString("yyyyMMddHHmmssfff");
							for (int i = 0; i < sBoxIDList.Count; i++)
							{
								string sSQL = $"UPDATE [BATCH_LIST] SET [ORDER_NO] = '',[END_TIME] = '{endtime}' WHERE [BOX_ID] = '{sBoxIDList[i]}'";
								App.Local_SQLServer.NonQuery(sSQL);
							}
						}
						DataTable dt_BoxData = App.Local_SQLServer.SelectDB("*", "[BATCH_LIST]", $"[BOX_ID] = '{sBoxID}'");
						DataTable dt_Task = App.Local_SQLServer.SelectDB("*", "[TASK]", $"[BOX_ID] = '{sBoxID}'");
						if (dt_Task.Rows.Count > 0)
						{
							Ack(drData_["HS_NAME"].ToString().Trim(), "1", "NG,BOX ID Task already Exist");
						}
						else if (dt_BoxData.Rows.Count > 0)
						{
							//抓取任務優先權
							DataTable dt_Priority = App.Local_SQLServer.SelectDB("[VALUE]", "[SYSTEM_SETTING]", $"[NAME] = '{sWAY}_Priority'");
							int iPriority = 0;
							if (dt_Priority.Rows.Count > 0)
								iPriority = dt_Priority.Rows[0]["VALUE"].ToString().Trim().ToIntDef(1);
							else
								iPriority = 5; //預設5
							if (sHS_Name.Contains("WebDelivery"))
							{
								for(int i = 0; i < sBoxIDList.Count; i++)
								{
									App.Bc.TaskCtrl.Create_Task(sBoxIDList[i], dt_BoxData.Rows[0]["CUSTOMER_ID"].ToString().Trim(), sDirection, sUserIDList[i], iPriority, sLifter);
								}
								
								if (App.Bc.isOnline()) //Online模式，要問Web Service
								{
									Ack(drData_["HS_NAME"].ToString().Trim(), "2", "");
									App.Bc.StockExistCheck_Request(sBoxID, sData);
								}
								else //OFFLine模式，直接OK
								{
									Ack(drData_["HS_NAME"].ToString().Trim(), "1", "OK");
								}
							}
							else if (App.Bc.TaskCtrl.Create_Task(sBoxID, dt_BoxData.Rows[0]["CUSTOMER_ID"].ToString().Trim(), sDirection, sUserID, iPriority, sLifter))
							{
								if (App.Bc.isOnline()) //Online模式，要問Web Service
								{
									Ack(drData_["HS_NAME"].ToString().Trim(), "2", "");
									App.Bc.StockExistCheck_Request(sBoxID, sData);
								}
								else //OFFLine模式，直接OK
								{
									Ack(drData_["HS_NAME"].ToString().Trim(), "1", "OK");
								}
							}
							else if (sHS_Name.Contains("Storage"))//App.Bc.TaskCtrl.Create_Task(sBoxID, dt_BoxData.Rows[0]["CUSTOMER_ID"].ToString().Trim(), sDirection, sUserID, iPriority,sTargetCarousel))
							{
								int i = 0;
								while (i < sBoxIDList.Count){
									List<string> temp = new List<string>();
									temp.Add(sTargetCarousel[i]);
									App.Bc.TaskCtrl.Create_Task(sBoxIDList[i], dt_BoxData.Rows[0]["CUSTOMER_ID"].ToString().Trim(), sDirection, sUserIDList[i], iPriority,temp);
									i++;
								}
								Ack(drData_["HS_NAME"].ToString().Trim(), "1", "OK");
							}
							else //TASK創建失敗，回NG  無法進入STK
							{
								Ack(drData_["HS_NAME"].ToString().Trim(), "1", "NG,Task Create Failed");
							}
						}
						else //找不到這一箱，無法進入STK
						{
							int iPriority = 0;
							DataTable dt_Priority = App.Local_SQLServer.SelectDB("[VALUE]", "[SYSTEM_SETTING]", $"[NAME] = '{sWAY}_Priority'");
							if (dt_Priority.Rows.Count > 0)
								iPriority = dt_Priority.Rows[0]["VALUE"].ToString().Trim().ToIntDef(1);
							else
								iPriority = 5; //預設5
							Ack(drData_["HS_NAME"].ToString().Trim(), "1", "NG,Can not Find BoxID Data In DataBase BATCH_LIST");
							App.Bc.TaskCtrl.Create_Task(sBoxID, dt_BoxData.Rows[0]["CUSTOMER_ID"].ToString().Trim(), sDirection, sUserID, iPriority, sLifter);
						}
							
						break;

					case "AlarmRptA": //Format = AlarmID,1(Set)/0(Reset)
						if (sWAY == "All")
						{
							App.Alarm.ResetAllAlarm();
							Ack(drData_["HS_NAME"].ToString().Trim(), "1", "");
							break;
						}
						App.Alarm.Set(2, (uint)sData.Split(',')[0].ToIntDef(0), sData.Split(',')[1].ToIntDef(0) == 1);
						Ack(drData_["HS_NAME"].ToString().Trim(), "1", "");
						break;
					case "AlarmRptB":
						if (sWAY == "All")
						{
							App.Alarm.ResetAllAlarm();
							Ack(drData_["HS_NAME"].ToString().Trim(), "1", "");
							break;
						}
						App.Alarm.Set(3, (uint)sData.Split(',')[0].ToIntDef(0), sData.Split(',')[1].ToIntDef(0) == 1);
						Ack(drData_["HS_NAME"].ToString().Trim(), "1", "");
						break;

					case "OrderNoA":
					case "OrderNoB":
						string sOrderNo = sData.Split(',')[0];
						sUserID = sData.Split(',')[1];
						App.Bc.OrderNo_Request(sOrderNo, sUserID); //Web Service 問領料單號
						Ack(drData_["HS_NAME"].ToString().Trim(), "2", "");
						break;

					case "UserIDReqA":
					case "UserIDReqB":
						if (App.Bc.isOnline())
						{
							string sCarID = sData.Split(',')[0];
							Ack(drData_["HS_NAME"].ToString().Trim(), "2", "");
							App.Bc.UserIDRequest(sHS_Name, sCarID);
						}
						else
						{
							Ack(drData_["HS_NAME"].ToString().Trim(), "1", "NG,,");
						}
						break;
				}

			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void Ack_Passive(DataRow drData_)//Rev Req=1, Ack=1
		{
			//ACKDATA 格式 = OK,x / NG,NGMSG
			string sReq = string.Empty;
			string sAck = string.Empty;

			sAck = drData_["ACKDATA"].ToString().Trim();
			string[] Acks = sAck.Split(',');
			if (Acks[0] == "NG" && Acks.Length > 1)
            {
				string sSQL = $"UPDATE [TASK] SET [NG_REASON] = '{Acks[1]}', WHERE [BOX_ID] = '{drData_["REQDATA"].ToString().Trim()}'";
				App.Local_SQLServer.NonQuery(sSQL);
            }

			sReq = drData_["REQDATA"].ToString().Trim();
			LogWriter.AddString(string.Format("SQL_HS ReqData = {0}, RevAck = {1}", sReq, sAck));
			Clear(drData_["HS_NAME"].ToString().Trim());
		}
	}
}