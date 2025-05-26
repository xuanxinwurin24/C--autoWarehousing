using CIM;
using CIM.BC;
using Strong;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CIM
{
	public class SQL_DB
	{
		private LogWriter logWriter;
		private SqlConnectionStringBuilder connStringBuilder;
		public SQL_Parameter SQLPara;
		public bool bConnect;
		public SQL_DB(SQL_Parameter connPara)
		{
			SQLPara = connPara;
			logWriter = new LogWriter(Environment.CurrentDirectory + @"\LogFile\SQL", "SQL", 500 * 1000);
			connStringBuilder = new SqlConnectionStringBuilder();
			bConnect = ConnectToDB();
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
				LogWriter.LogException(ex);
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

		public DataTable SelectDB(string key, string tableName, string condition, bool bLog = true)
		{
			DataTable dt = new DataTable();

			//if (App.DB_bypass) return dt;   //for test

			string SQL = $"select {key} from {tableName}";

			if (condition != "")
			{
				SQL += $" where {condition}";
			}
			Query(SQL, ref dt, bLog);
			return dt;
		}
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
	ThreadTimer Clean_Timer;
	private LogWriter LogWriter;
	private List<string> SetFindReq;     //B(Req=1) -> self
	private List<string> SetFindAck;     //B(Ack=1) -> self
	private List<string> SetFindSelfAck; //self(Ack=2) -> self(Ack=1)
	private string sPort = "";


	public SQL_HS(string sPort_)
	{
		ReqTimer = App.MainThread.TimerCreate("Req_Timer", 100, HS_Timer_Event, ThreadTimer.eType.Cycle);
		ReqTimer.Enable(true);
		LogWriter = new LogWriter(Environment.CurrentDirectory + @"\LogFile\SQL_HS", "SQL_HS", 500 * 1000);
		Clean_Timer = App.MainThread.TimerCreate("Clean_Timer", App.SysPara.CleanITReplyTime * 1000, Clean_Timer_Event, ThreadTimer.eType.Cycle);
		sPort = sPort_;
		if (sPort == "A")
		{
			SetFindReq = new List<string> { "ShuttleCarA" };
			SetFindAck = new List<string> { "StoreA", "DeliveryA", "OrderNoA", "AlarmRptA","UserIDReqA"};
		}
		else if (sPort == "B")
		{
			SetFindReq = new List<string> { "ShuttleCarB" };
			SetFindAck = new List<string> { "StoreB", "DeliveryB", "OrderNoB", "AlarmRptB","UserIDReqB" };
		}
		SetFindSelfAck = new List<string> { };
	}


	public void Req(string sHSName_, string sRptData_, DeliStore.ucFrame Frame_)//主動
	{
		//Store or Delivery or ShuttleCar A/B
		string sFrame = Frame_.ToString();
		string sHS_Name = sHSName_ + sPort;
		//string sql = $"IF EXISTS(SELECT* FROM HS WHERE HS_NAME = '{sHS_Name}')" +
		//			$"UPDATE HS SET REQ = 1, REQDATA = '{sRptData_}', WAY = '{Frame_}' WHERE HS_NAME = '{sHS_Name}'" +
		//			$"ELSE INSERT INTO HS(HS_NAME,REQ,REQDATA,ACK,ACKDATA,WAY)VALUES('{sHS_Name}', 1, '{sRptData_}', 0, '','{sFrame}')";
		string sql = $"UPDATE HS SET REQ = 1, REQDATA = '{sRptData_}', WAY = '{Frame_}' WHERE HS_NAME = '{sHS_Name}'";
		int AffectRow = App.Local_SQLServer.NonQuery(sql);
		LogWriter.AddString(string.Format("SQL_HS REQDATA = {0}, AffectRow = {1}, Frame = {2}", sRptData_, AffectRow, Frame_.ToString()));

	}
	public void Ack(string sHSName_, string sAck_, string sAckData_)//主動
	{
		string sAck = sAck_; //1 or 2
		string sql = $"IF EXISTS(SELECT* FROM HS WHERE HS_NAME = '{sHSName_}')" +
					$"UPDATE HS SET ACK = {sAck}, ACKDATA = '{sAckData_}' WHERE HS_NAME = '{sHSName_}'";

		int AffectRow = App.Local_SQLServer.NonQuery(sql);
		LogWriter.AddString(string.Format("SQL_HS AckData = {0}, AffectRow = {1}", sAckData_, AffectRow));
	}

	private void Clear(string sName_)
	{
		string sHSName = sName_;
		string sql = $"IF EXISTS(SELECT* FROM HS WHERE HS_NAME = '{sHSName}')" +
					$"UPDATE HS SET REQ = 0, REQDATA = '', ACK = 0, ACKDATA = '', WAY='' WHERE HS_NAME = '{sHSName}'";
		int k = App.Local_SQLServer.NonQuery(sql);
	}
	private int Clean_Timer_Event(ThreadTimer threadTimer_)
	{
		Clean_Timer.Enable(false);
		return 0;
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
			else
			if (iAck == 1)
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
		string sFrom = string.Empty;
		//Do Here
		//Get Shuttle -> Lifter BOXID
		string sBoxID = sData;
		App.Bc.SetCrownToLifter(sData);
		//
		LogWriter.AddString(string.Format("SQL_HS Rev {0} Req Data= {1}", sHS_Name, sData));
		Ack(sHS_Name, "1", ""); //Lifter

		//Ack(sHS_Name, "2", ""); //要問WebService資料，先給2，避免重複觸發 MPC
	}

	private void Ack_Passive(DataRow drData_)//Rev Req=1, Ack=1
	{
		//ACKDATA 格式 = OK,x / NG,NGMSG
		string sReq = string.Empty;
		string sAck = string.Empty;
		string sFrame = string.Empty;
		string sHSName = string.Empty;

		sHSName = drData_["HS_NAME"].ToString().Trim();
		sReq = drData_["REQDATA"].ToString().Trim();
		sAck = drData_["ACKDATA"].ToString().Trim();

		if (sAck != "")
		{
			if (sReq.Contains(","))
			{
				string[] Reqs = sReq.Split(',');
				App.DS.Itreply.BoxID = Reqs[0];
			}
			else
			{
				App.DS.Itreply.BoxID = sReq;
			}
			string[] Acks = sAck.Split(',');
			App.DS.Itreply.OKorNG = Acks[0]; //Lifter
			if (Acks.Length > 1)
			{
				App.DS.Itreply.Msg = App.DS.Itreply.BoxID + ":" + Acks[1];
			}
			//Do Here
			if (sHSName.Contains("Store"))
			{
				if (Acks.Length > 1)
				{
					//BatchList_NO(App.DS.Itreply.BoxID, Acks[0], Acks[1]); //之後需要主程式回傳的msg時使用這行
					BatchList_NO(App.DS.Itreply.BoxID, Acks[0]);
				}
				else
				{
					BatchList_NO(App.DS.Itreply.BoxID, Acks[0]);
				}
			}
			else if (sHSName.Contains("UserIDReq"))
			{
				if (Acks[0].Trim() == "OK")
				{
					DataTable Result = new DataTable();
					string sql = $"SELECT USER_ID,USER_NAME,AUTHORITY_TYPE FROM [ACCOUNT] WHERE USER_ID='{Acks[1]}'";
					App.Local_SQLServer.Query(sql, ref Result);
					foreach(DataRow dr in Result.Rows)
					{
						App.DS.OP.UserID = dr["USER_ID"].ToString().Trim();
						App.DS.OP.UserName = dr["USER_NAME"].ToString().Trim();
						App.DS.OP.Level = dr["AUTHORITY_TYPE"].ToString().Trim();
						App.DS.OP.LogIn = true;
						break;
					}
					App.DS.OP.ToUser="No Account";
					//App.DS.OP.UserID = Acks[1];
					//App.DS.OP.UserName = Acks[2];
					//App.DS.OP.LogIn = true;
				}
                else
                {
					App.DS.OP.LogIn = false;
					App.DS.OP.Level = "";
					App.DS.OP.ToUser = "Login Error";
                }
			}
		}

		sFrame = drData_["WAY"].ToString().Trim();
		FrameToEnframe(sFrame);
		//
		sReq = drData_["REQDATA"].ToString().Trim();
		LogWriter.AddString(string.Format("SQL_HS ReqData={0}, RevAck = {1}", sReq, sAck));
		Clean_Timer.Enable(true);
		Clear(drData_["HS_NAME"].ToString().Trim());
	}

	private void FrameToEnframe(string sFrame_)
	{
		if (sFrame_ == "StoreManual")
		{
			App.DS.NowActFrame = DeliStore.ucFrame.StoreManual;
			return;
		}
		else if (sFrame_ == "StoreAuto")
		{
			App.DS.NowActFrame = DeliStore.ucFrame.StoreAuto;
			return;
		}
		else if (sFrame_ == "NormalDelivery")
		{
			App.DS.NowActFrame = DeliStore.ucFrame.NormalDelivery;
			return;
		}
		else if (sFrame_ == "OrderDelivery")
		{
			App.DS.NowActFrame = DeliStore.ucFrame.OrderDelivery;
			return;
		}
		else if (sFrame_ == "ProbDelivery")
		{
			App.DS.NowActFrame = DeliStore.ucFrame.ProbDelivery;
			return;
		}
		else if (sFrame_ == "SecretDelivery")
		{
			App.DS.NowActFrame = DeliStore.ucFrame.SecretDelivery;
			return;
		}
	}
	public void SimuDeli()
	{

		for (int i = 0; i < 30; i++)
		{
			string sESB = "BOX_ID" + i.ToString().PadLeft(3, '0');
			string sBatch = "Batch" + (i % 10 + 1).ToString().PadLeft(2, '0');
			string sResult = i % 7 == 0 ? "NG" : "OK";
			string sOrder_No = (14573 + i * 17).ToString();
			string soteria = i % 9 == 0 ? "S" : "N";
			string customer_id = "ASE";
			string sql = $"INSERT INTO BATCH_LIST(BOX_ID,BATCH_NO,BATCH_NO_DETAIL,ORDER_NO,SOTERIA,CUSTOMER_ID,READY_DELIVERY)" +
				$"VALUES('{sESB}', '{sBatch}','{sResult}','{sOrder_No}','{soteria}','{customer_id}',0)";
			int A = App.Local_SQLServer.NonQuery(sql);
		}
	}

	public void BatchList_NO(string sBOXID_, string BatchNoResult_, string Msg_ = "")
	{
		//待修改
		string sql = string.Empty;
		try
		{
			//Result Y:Success N:Fault
			string sBatchResult = BatchNoResult_ == "OK" ? "Y" : "F";

			sql = $"UPDATE BATCH_LIST SET BATCH_NO_RESULT = '{sBatchResult}', BATCH_NO_MSG = '{Msg_}' WHERE BOX_ID = '{sBOXID_}'";
			int k = App.Local_SQLServer.NonQuery(sql);
		}
		catch (Exception ex)
		{
			LogWriter.LogException(ex);
		}
	}
}