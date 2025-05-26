using Microsoft.EntityFrameworkCore;
using ASEWEB.Models;
using System.Data;
using System;
using System.Collections.Generic;
using System.Threading;
namespace ASEWEB.Models
{
 //注意 已將盤點與調儲的回傳Carousel和Cell 轉為show
 // 尤其注意STK_SQL_Server部分 這部分要討論所以進SQLContext時尚未轉換Show與原版
    public class SQLContext
    {
        public static string sql;
        public static SysPara SysPara;
        private DataTable Result = new DataTable();
        #region Alarm
        public List<string> Alarm_List()
        {
			sql = string.Empty;
			List<string> Context = new List<string>(0);
			sql = "SELECT * FROM [ALARM] ";
			bool n = Startup.Local_SQLServer.Query(sql, ref Result);
			foreach(DataRow dr in Result.Rows)
            {
				string id = dr["ID"].ToString().Trim();
				string unitname = dr["UNIT_NAME"].ToString().Trim();
				string time = dr["TIME"].ToString().Trim();
				string message = dr["MESSAGE"].ToString().Trim();

				Context.Add(id);
				Context.Add(unitname);
				Context.Add(time);
				Context.Add(message);
            }
			return Context;
        }
		public List<string> Alarm_History_List()
        {
			sql = string.Empty;
			List<string> Context = new List<string>();
			sql = "SELECT TOP(1000) [ID],[UNIT_NAME],[OCCURED_TIME],[RESET_TIME],[MESSAGE] FROM [ALARM_HISTORY] ORDER BY RESET_TIME DESC";
			bool n = Startup.Local_SQLServer.Query(sql, ref Result);
			foreach(DataRow dr in Result.Rows)
            {
				string id = dr["ID"].ToString().Trim();
				string unitname = dr["UNIT_NAME"].ToString().Trim();
				string occurtime= dr["OCCURED_TIME"].ToString().Trim();
				string resettime= dr["RESET_TIME"].ToString().Trim();
				string message= dr["MESSAGE"].ToString().Trim();

				Context.Add(id);
				Context.Add(unitname);
				Context.Add(occurtime);
				Context.Add(resettime);
				Context.Add(message);
            }
			return Context;
        }
        #endregion Alarm
        #region DownLoad
		public List<string> Download_Account_List()
        {
			List<string> Context = new List<string>();
			sql = "SELECT * FROM [ACCOUNT]";
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach(DataRow dr in Result.Rows)
            {
				string userid = dr["USER_ID"].ToString().Trim();
				string username = dr["USER_NAME"].ToString().Trim();
				string userpass = dr["USER_PASS"].ToString().Trim();
				string usergroup = dr["USER_GROUP"].ToString().Trim();
				string authority = dr["AUTHORITY_TYPE"].ToString().Trim();
				string usersystemsetting = dr["USER_SYSTEMSETTING"].ToString().Trim();

				Context.Add(userid);
				Context.Add(username);
				Context.Add(userpass);
				Context.Add(usergroup);
				Context.Add(authority);
				Context.Add(usersystemsetting);
			}
			return Context;
        }
		public List<string> Download_BatchList(string time1_,string time2_)
        {
			List<string> Context = new List<string>();
			sql = "SELECT * FROM [BATCH_LIST]";
            if (time1_.Contains("%") || time1_.Contains("?"))
            {
				sql += $" WHERE END_TIME LIKE '{time1_}%'";
            }
			else if (time2_ != "")
            {
				sql += $" WHERE END_TIME BETWEEN '{time1_}000000000' AND '{time2_}235959999'";
            }
			else if(time2_=="" && time1_ != "")
            {
				sql += $" WHERE END_TIME BETWEEN '{time1_}000000000' AND '{time1_}235959999'";
            }
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach(DataRow dr in Result.Rows)
            {
				string boxid = dr["BOX_ID"].ToString().Trim();
				string batchno = dr["BATCH_NO"].ToString().Trim();
				string orderno = dr["ORDER_NO"].ToString().Trim();
				string soteria = dr["SOTERIA"].ToString().Trim();
				string endtime = dr["END_TIME"].ToString().Trim();

				Context.Add(boxid);
				Context.Add(batchno);
				Context.Add(orderno);
				Context.Add(soteria);
				Context.Add(endtime);
            }
			return Context;
        }
		public List<string> Download_CarouselUtility_History(string time1_, string time2_)
		{
			List<string> Context = new List<string>();
			sql = "SELECT * FROM [MONITOR_SETTING_HISTORY]";
			if (time1_.Contains("%") || time1_.Contains("?"))
			{
				sql += $" WHERE COMMAND_ID LIKE '{time1_}%'";
			}
			else if (time2_ != "")
			{
				sql += $" WHERE COMMAND_ID BETWEEN '{time1_}000000000' AND '{time2_}235959999'";
			}
			else if (time2_ == "" && time1_ != "")
			{
				sql += $" WHERE COMMAND_ID BETWEEN '{time1_}000000000' AND '{time1_}235959999'";
			}
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string commandid = dr["COMMAND_ID"].ToString().Trim();
				string userid = dr["USER_ID"].ToString().Trim();


				Context.Add(commandid);
				Context.Add(userid);
			}
			return Context;
		}
		public List<string> Download_CarouselUtility_History_Detail(string rCMD_ID)
        {
			List<string> Context = new List<string>();
			sql = $"SELECT * FROM [MONITOR_SETTING_HISTORY_DETAIL] WHERE COMMAND_ID='{rCMD_ID}'";
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string commandid = dr["COMMAND_ID"].ToString().Trim();
				string carouselid = dr["CAROUSEL_ID"].ToString().Trim();
				string tul= dr["TEMPERATER_UPPER_LIMIT"].ToString().Trim();
				string tll = dr["TEMPERATER_LOWER_LIMIT"].ToString().Trim();
				string hul= dr["HUMIDITY_UPPER_LIMIT"].ToString().Trim();
				string hll = dr["HUMIDITY_LOWER_LIMIT"].ToString().Trim();
				string ton=dr["TURN_ON_N2_HUMIDITY"].ToString().Trim();
				string toff = dr["TURN_OFF_N2_HUMIDITY"].ToString().Trim();

				carouselid = Show_CSID_Transfer(carouselid);
				Context.Add(commandid);
				Context.Add(carouselid);
				Context.Add(tul);
				Context.Add(tll);
				Context.Add(hul);
				Context.Add(hll);
				Context.Add(ton);
				Context.Add(toff);
			}
			return Context;
		}
		public List<string> Download_Check_History(string time1_, string time2_)
		{
			List<string> Context = new List<string>();
			sql = "SELECT * FROM [CAROUSEL_CHECK_HISTORY]";
			if (time1_.Contains("%") || time1_.Contains("?"))
			{
				sql += $" WHERE END_TIME LIKE '{time1_}%'";
			}
			else if (time2_ != "")
			{
				sql += $" WHERE END_TIME BETWEEN '{time1_}000000000' AND '{time2_}235959999'";
			}
			else if (time2_ == "" && time1_ != "")
			{
				sql += $" WHERE END_TIME BETWEEN '{time1_}000000000' AND '{time1_}235959999'";
			}
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string commandid = dr["COMMAND_ID"].ToString().Trim();
				string checkresult = dr["RESULT"].ToString().Trim();
				string starttime = dr["START_TIME"].ToString().Trim();
				string endtime = dr["END_TIME"].ToString().Trim();
				string userid = dr["USER_ID"].ToString().Trim();
				string ngreason = dr["NG_REASON"].ToString().Trim();

				Context.Add(commandid);
				Context.Add(checkresult);
				Context.Add(starttime);
				Context.Add(endtime);
				Context.Add(userid);
				Context.Add(ngreason);
			}
			return Context;
		}
		public List<string> Download_Check_History_Detail(string command_id,string user_id)//string[] command_id,string[] user_id
		{
			List<string> Context = new List<string>();
			sql = $"SELECT * FROM [CAROUSEL_CHECK_HISTORY_DETAIL] WHERE [COMMAND_ID]='{command_id}'";//[i]
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string commandid = dr["COMMAND_ID"].ToString().Trim();
				string checkresult = dr["CHECK_RESULT"].ToString().Trim();
				string carouselid = dr["CAROUSEL_ID"].ToString().Trim();
				string cellid = dr["CELL_ID"].ToString().Trim();
				string batchno = dr["BATCH_NO"].ToString().Trim();
				string boxid = dr["BOX_ID"].ToString().Trim();
				string groupno = dr["GROUP_NO"].ToString().Trim();
				string soteria = dr["SOTERIA"].ToString().Trim();
				string customer = dr["CUSTOMER_ID"].ToString().Trim();

				Context.Add(commandid);
				Context.Add(carouselid);
				Context.Add(cellid);
				Context.Add(batchno);
				Context.Add(boxid);
				Context.Add(groupno);
				Context.Add(soteria);
				Context.Add(customer);
				Context.Add(checkresult);
				Context.Add(user_id);//[i]
			}
		return Context;
		}
		public List<string> Download_Alarm_History(string time1_,string time2_)
		{
			List<string> Context = new List<string>();
			sql = "SELECT * FROM [ALARM_HISTORY]";
			if (time1_.Contains("%") || time1_.Contains("?"))
			{
				sql += $" WHERE RESET_TIME LIKE '{time1_}%'";
			}
			else if (time2_ != "")
			{
				sql += $" WHERE RESET_TIME BETWEEN '{time1_}-00:00:00:000' AND '{time2_}-99:99:99:999'";
			}
			else if (time2_ == "" && time1_ != "")
			{
				sql += $" WHERE RESET_TIME BETWEEN '{time1_}-00:00:00:000' AND '{time1_}-99:99:99:999'";
			}
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string id = dr["ID"].ToString().Trim();
				string unitname = dr["UNIT_NAME"].ToString().Trim();
				string occuredtime = dr["OCCURED_TIME"].ToString().Trim();
				string resettime = dr["RESET_TIME"].ToString().Trim();
				string message = dr["MESSAGE"].ToString().Trim();

				Context.Add(id);
				Context.Add(unitname);
				Context.Add(occuredtime);
				Context.Add(resettime);
				Context.Add(message);
			}
			return Context;
		}
		public List<string> Download_Task_History(string time1_, string time2_)
		{
			List<string> Context = new List<string>();
			sql = "SELECT * FROM [TASK_HISTORY]";
			if (time1_.Contains("%") || time1_.Contains("?"))
			{
				sql += $" WHERE END_TIME LIKE '{time1_}%'";
			}
			else if (time2_ != "")
			{
				sql += $" WHERE END_TIME BETWEEN '{time1_}000000000' AND '{time2_}235959999'";
			}
			else if (time2_ == "" && time1_ != "")
			{
				sql += $" WHERE END_TIME BETWEEN '{time1_}000000000' AND '{time1_}235959999'";
			}
			sql += "ORDER BY END_TIME DESC";
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string boxid = dr["BOX_ID"].ToString().Trim();
				string batchno = dr["BATCH_NO"].ToString().Trim();
				string srcpos = dr["SRC_POS"].ToString().Trim();
				string srccell = dr["SRC_CELL_ID"].ToString().Trim();
				string tarpos = dr["TAR_POS"].ToString().Trim();
				string tarcell = dr["TAR_CELL_ID"].ToString().Trim();
				string status = dr["STATUS"].ToString().Trim();
				string direction = dr["DIRECTION"].ToString().Trim();
				string ngreason=dr["NG_REASON"].ToString().Trim();
				string starttime=dr["START_TIME"].ToString().Trim();
				string endtime = dr["END_TIME"].ToString().Trim();
				string userid=dr["USER_ID"].ToString().Trim();

				Context.Add(boxid);
				Context.Add(batchno);
				Context.Add(srcpos);
				Context.Add(srccell);
				Context.Add(tarpos);
				Context.Add(tarcell);
				Context.Add(status);
				Context.Add(direction);
				Context.Add(ngreason);
				Context.Add(starttime);
				Context.Add(endtime);
				Context.Add(userid);
			}
			return Context;
		}
		public List<string> Download_STK_Carousel_Daily_History(string time1_, string time2_)
		{
			List<string> Context = new List<string>();
			sql = "SELECT * FROM [CAROUSEL_DALIY_LOG]";
			if (time1_.Contains("%") || time1_.Contains("?"))
			{
				sql += $" WHERE [CREATION_DATE] LIKE '{time1_}%'";
			}
			else if (time2_ != "")
			{
				sql += $" WHERE [CREATION_DATE] BETWEEN '{time1_}' AND '{time2_}'";
			}
			else if (time2_ == "" && time1_ != "")
			{
				sql += $" WHERE [CREATION_DATE] BETWEEN '{time1_}' AND '{time1_}'";
			}
			sql += "ORDER BY [CREATION_DATE] DESC";
			Startup.STK_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string carouselid = dr["CAROUSEL_ID"].ToString().Trim();
				string maxt = dr["MAX_T"].ToString().Trim();
				string mint = dr["MIN_T"].ToString().Trim();
				string avgt = dr["AVG_T"].ToString().Trim();
				string maxh = dr["MAX_H"].ToString().Trim();
				string minh = dr["MIN_H"].ToString().Trim();
				string avgh = dr["AVG_H"].ToString().Trim();
				string creationdate = dr["CREATION_DATE"].ToString().Trim();
				string optimes = dr["OP_TIMES"].ToString().Trim();
				string totalsec = dr["TOTAL_SEC"].ToString().Trim();

				Context.Add(carouselid);
				Context.Add(maxt);
				Context.Add(mint);
				Context.Add(avgt);
				Context.Add(maxh);
				Context.Add(minh);
				Context.Add(avgh);
				Context.Add(creationdate);
				Context.Add(optimes);
				Context.Add(totalsec);
			}
			return Context;
		}
		public List<string> Download_STK_Door_History(string time1_, string time2_)
		{
			List<string> Context = new List<string>();
			sql = "SELECT * FROM [CAROUSEL_DOOR_LOG]";
			if (time1_.Contains("%") || time1_.Contains("?"))
			{
				sql += $" WHERE [DOOR_OPEN_TIME] LIKE '{time1_}%'";
			}
			else if (time2_ != "")
			{
				sql += $" WHERE [DOOR_OPEN_TIME] BETWEEN '{time1_} 00:00:00' AND '{time2_} 23:59:59'";
			}
			else if (time2_ == "" && time1_ != "")
			{
				sql += $" WHERE [DOOR_OPEN_TIME] BETWEEN '{time1_} 00:00:00' AND '{time1_} 23:59:59'";
			}
			sql += "ORDER BY [DOOR_CLOSE_TIME] DESC";
			Startup.STK_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string carouselid = dr["CAROUSEL_ID"].ToString().Trim();
				string dooropentime = dr["DOOR_OPEN_TIME"].ToString().Trim();
				string doorclosetime = dr["DOOR_CLOSE_TIME"].ToString().Trim();
				string opsec = dr["OP_SEC"].ToString().Trim();

				Context.Add(carouselid);
				Context.Add(dooropentime);
				Context.Add(doorclosetime);
				Context.Add(opsec);
			}
			return Context;
		}
		#endregion
		public void TransferRecover()
		{
			string sql_temp = "SELECT DISTINCT CAROUSEL_ID FROM [CAROUSEL_TRANSFER]";
			DataTable dt_temp = new DataTable();
			Startup.Local_SQLServer.Query(sql_temp, ref dt_temp);
			foreach (DataRow dr_temp in dt_temp.Rows)
			{
				string sql = $"UPDATE [CAROUSEL_TRANSFER] SET SHOW_CAROUSEL_ID='{dr_temp["CAROUSEL_ID"].ToString().Trim()}' WHERE CAROUSEL_ID='{dr_temp["CAROUSEL_ID"].ToString().Trim()}'";
				Startup.Local_SQLServer.NonQuery(sql);
			}
			sql_temp = "SELECT DISTINCT CELL_ID FROM [CAROUSEL_TRANSFER]";
			Startup.Local_SQLServer.Query(sql_temp, ref dt_temp);
			foreach (DataRow dr_temp in dt_temp.Rows)
			{
				string sql = $"UPDATE [CAROUSEL_TRANSFER] SET SHOW_CELL_ID='{dr_temp["CELL_ID"].ToString().Trim()}' WHERE CELL_ID='{dr_temp["CELL_ID"].ToString().Trim()}'";
				Startup.Local_SQLServer.NonQuery(sql);
			}
		}
		public List<string> Log_Record_List(string type)
        {
			sql = string.Empty;
			List<string> Context = new List<string>();
			sql = $"SELECT * FROM [LOG_RECORD] WHERE TYPE='{type}' ORDER BY TIME DESC";
			bool n = Startup.Local_SQLServer.Query(sql, ref Result);
			foreach(DataRow dr in Result.Rows)
            {
				string totaltime = dr["TIME"].ToString().Trim();
				string year = totaltime.Substring(0, 4);
				string month = totaltime.Substring(4, 2);
				string date = totaltime.Substring(6, 2);
				string hour = totaltime.Substring(8, 2);
				string minute = totaltime.Substring(10, 2);
				string second = totaltime.Substring(12, 2);
				string time = year + "/" + month + "/" + date + " " + hour + ":" + minute + ":" + second;
				string message = dr["MESSAGE"].ToString().Trim();

				Context.Add(time);
				Context.Add(message);
            }
			return Context;
        }
        #region Login
        public List<string> Auth_String(string rUser)
		{
			sql = string.Empty;
			List<string> Context = new List<string>();
			sql = $"SELECT TOP 1 [AUTHORITY] FROM [ACCOUNT] WHERE USER_ID='{rUser}'";
			bool n = Startup.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string authtype = dr["AUTHORITY_TYPE"].ToString().Trim();
				string[] authtype_list = authtype.Split(',');
				for (int i = 0; i < authtype_list.Length; i++)
				{
					Context.Add(authtype_list[i]);
				}
			}
			return Context;
		}
		public string Login_Name(string USER_ID,string USER_PWD)
        {
			sql= $"SELECT * FROM [ACCOUNT] WHERE [USER_ID]='{USER_ID}' AND [USER_PASS]='{USER_PWD}'";
			Startup.Local_SQLServer.Query(sql, ref Result);
			int i = 0;
			string name = string.Empty;
			foreach(DataRow dr in Result.Rows)
            {
				if (dr["USER_ID"].ToString().Trim() != USER_ID || dr["USER_PASS"].ToString().Trim() != USER_PWD)
					break;
				i++;
				name = dr["USER_NAME"].ToString().Trim();
            }
			if (i == 0) return " ";
			return name;
		}
		public string Login_Authority(string USER_ID,string USER_PWD)
        {
			sql = $"SELECT * FROM [ACCOUNT] WHERE [USER_ID]='{USER_ID}' AND [USER_PASS]='{USER_PWD}'";
			Startup.Local_SQLServer.Query(sql, ref Result);
			int i = 0;
			string authority=string.Empty;
			foreach (DataRow dr in Result.Rows)
            {
				if (dr["USER_ID"].ToString().Trim() != USER_ID || dr["USER_PASS"].ToString().Trim() != USER_PWD)
					break;
				i++;
				authority = dr["AUTHORITY_TYPE"].ToString().Trim();
            }
			if (i == 0) return "0";
			return authority;
        }
		public string Login_Group(string USER_ID,string USER_PWD)
        {
			sql= $"SELECT * FROM [ACCOUNT] WHERE [USER_ID]='{USER_ID}' AND [USER_PASS]='{USER_PWD}'";
			int i = 0;
			string group = string.Empty;
			foreach (DataRow dr in Result.Rows)
			{
				if (dr["USER_ID"].ToString().Trim() != USER_ID || dr["USER_PASS"].ToString().Trim() != USER_PWD)
					break;
				i++;
				group = dr["USER_GROUP"].ToString().Trim();
			}
			if (i == 0) return "0";
			return group;
		}
		public string Login_SystemSetting(string USER_ID, string USER_PWD)
		{
			sql = $"SELECT * FROM [ACCOUNT] WHERE [USER_ID]='{USER_ID}' AND [USER_PASS]='{USER_PWD}'";
			int i = 0;
			string systemsetting = string.Empty;
			foreach (DataRow dr in Result.Rows)
			{
				if (dr["USER_ID"].ToString().Trim() != USER_ID || dr["USER_PASS"].ToString().Trim() != USER_PWD)
					break;
				i++;
				systemsetting = dr["USER_SYSTEMSETTING"].ToString().Trim();
			}
			if (i == 0) return "0";
			return systemsetting;
		}
		#endregion Login
		#region Carousel_ID
		public List<string> CSID_String()
		{
			sql = string.Empty;
			sql = "SELECT DISTINCT CAROUSEL_ID,SHOW_CAROUSEL_ID FROM [CAROUSEL_TRANSFER] ORDER BY 1 DESC";
			List<string> Context = new List<string>();
			Context.Clear();
			ASEWEB.Startup.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string carouselidshow = dr["SHOW_CAROUSEL_ID"].ToString().Trim();
				Context.Add(carouselidshow);
			}
			return Context;
		}
		public String CSID_Transfer(string Show_CSID)
		{
			string Context = string.Empty;
			DataTable resulttemp = new DataTable();
			string sqltemp = $"SELECT DISTINCT CAROUSEL_ID FROM [CAROUSEL_TRANSFER] WHERE SHOW_CAROUSEL_ID='{Show_CSID}'";
			Startup.Local_SQLServer.Query(sqltemp, ref resulttemp);
			foreach (DataRow dr in resulttemp.Rows)
			{
				Context = dr["CAROUSEL_ID"].ToString().Trim();
			}
			return Context;
		}
		public List<string> Vogue_CSID_Transfer(string Show_CSID1, string Show_CSID2)//給模糊搜尋專用
		{
			List<string> Context = new List<string>();
			Show_CSID1 = Show_CSID1.Trim();
			int temp;
			temp= Show_CSID1.IndexOf(',');
			string temp_str = temp.ToString();
			if (temp!=-1)
			{
				string[] Show_CSID1_List = Show_CSID1.Split(',');
				
				foreach (string CSID1 in Show_CSID1_List)
				{
					sql = $"SELECT DISTINCT CAROUSEL_ID FROM [CAROUSEL_TRANSFER] WHERE SHOW_CAROUSEL_ID LIKE '{CSID1}' OR SHOW_CAROUSEL_ID='{CSID1}'";
					Startup.Local_SQLServer.Query(sql, ref Result);
					foreach (DataRow dr in Result.Rows)
					{
						Context.Add(dr["CAROUSEL_ID"].ToString().Trim());
					}
				}
			}
			else
			{
				if (Show_CSID2 != "" && Show_CSID2 != null)
				{
					sql = $"SELECT DISTINCT CAROUSEL_ID FROM [CAROUSEL_TRANSFER] WHERE SHOW_CAROUSEL_ID BETWEEN '{Show_CSID1}' AND '{Show_CSID2}'";
				}
				else
				{
					sql = $"SELECT DISTINCT CAROUSEL_ID FROM [CAROUSEL_TRANSFER] WHERE SHOW_CAROUSEL_ID LIKE '{Show_CSID1}' OR SHOW_CAROUSEL_ID='{Show_CSID1}'";
				}
				Startup.Local_SQLServer.Query(sql, ref Result);
				foreach (DataRow dr in Result.Rows)
				{
					Context.Add(dr["CAROUSEL_ID"].ToString().Trim());
				}
			}

			return Context;
		}
		public String Show_CSID_Transfer(string CSID)
		{
			string Context = string.Empty;
			DataTable Result1=new DataTable();
			sql = $"SELECT DISTINCT SHOW_CAROUSEL_ID FROM [CAROUSEL_TRANSFER] WHERE CAROUSEL_ID='{CSID}'";
			Startup.Local_SQLServer.Query(sql, ref Result1);
			foreach (DataRow dr in Result1.Rows)
			{
				Context = dr["SHOW_CAROUSEL_ID"].ToString().Trim();
			}
			return Context;
		}
		#endregion
		#region Cell_ID
		public List<string> Cell_string(string rCSID, string rCEID)
		{
			List<string> Context = new List<string>();
			sql = string.Empty;
			//rCSID = CSID_Transfer(rCSID);
			//rCEID = CEID_Transfer(rCEID);
			try
			{
				sql = $"SELECT * FROM [CELL_STATUS] WHERE CAROUSEL_ID='{rCSID}' AND CELL_ID='{rCEID}'";
				bool n = Startup.STK_SQLServer.Query(sql, ref Result, true);

				foreach (DataRow dr in Result.Rows)
				{
					string boxid = dr["BOX_ID"].ToString().Trim();
					string status = dr["STATUS"].ToString().Trim();
					Context.Add(boxid);
					Context.Add(status);
				}
			}
			catch (Exception ex)
			{

			}
			return Context;
		}

		public List<string> CEID_String(string rCSID)
		{
			//rCSID = CSID_Transfer(rCSID);
			sql = string.Empty;
			sql = $"SELECT CELL_ID,SHOW_CELL_ID FROM [CAROUSEL_TRANSFER] WHERE CAROUSEL_ID='{rCSID}' ORDER BY 1 ASC";
			List<string> Context = new List<string>();
			Context.Clear();
			bool n = Startup.Local_SQLServer.Query(sql, ref Result, true);
			foreach (DataRow dr in Result.Rows)
			{
				string cellid = dr["CELL_ID"].ToString().Trim();
				string cellidshow = dr["SHOW_CELL_ID"].ToString().Trim();
				Context.Add(cellid);
				Context.Add(cellidshow);

			}
			return Context;
		}
		public List<string> CELL_Detail_string(string rCSID, string rCEID)
		{
			List<string> Context = new List<string>();
			sql = string.Empty;
			try
			{
				//rCSID = CSID_Transfer(rCSID);
				//rCEID = CEID_Transfer(rCEID);
				sql = $"SELECT * FROM [CELL_STATUS] WHERE CAROUSEL_ID='{rCSID}' AND CELL_ID='{rCEID}'";
				bool n = Startup.STK_SQLServer.Query(sql, ref Result);
				foreach (DataRow dr in Result.Rows)
				{
					string boxid = dr["BOX_ID"].ToString().Trim();
					string batchno = dr["BATCH_NO"].ToString().Trim();
					string soteria = dr["SOTERIA"].ToString().Trim();
					string customerid = dr["CUSTOMER_ID"].ToString().Trim();

					Context.Add(boxid);
					Context.Add(batchno);
					Context.Add(soteria);
					Context.Add(customerid);
				}
			}
			catch (Exception ex)
			{

			}
			return Context;

		}
		public String CEID_Transfer(string Show_CEID)
		{
			string Context = string.Empty;
			sql = $"SELECT DISTINCT CELL_ID FROM [CAROUSEL_TRANSFER] WHERE SHOW_CELL_ID='{Show_CEID}'";
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				Context = dr["CELL_ID"].ToString().Trim();
			}
			return Context;
		}
		public List<string> Vogue_CEID_Transfer(string Show_CEID1, string Show_CEID2)
		{
			List<string> Context = new List<string>();
			if (Show_CEID1.Contains(','))
			{
				string[] Show_CEID1_List = Show_CEID1.Split(',');
				foreach (string CEID1 in Show_CEID1_List)
				{
					sql = $"SELECT DISTINCT CELL_ID FROM [CAROUSEL_TRANSFER] WHERE SHOW_CELL_ID LIKE '{CEID1}' OR SHOW_CELL_ID = '{CEID1}'";
					Startup.Local_SQLServer.Query(sql, ref Result);
					foreach (DataRow dr in Result.Rows)
					{
						Context.Add(dr["CELL_ID"].ToString().Trim());
					}
				}
			}
			else
			{
				if (Show_CEID2 != "" && Show_CEID2 != null)
				{
					sql = $"SELECT DISTINCT CELL_ID FROM [CAROUSEL_TRANSFER] WHERE SHOW_CELL_ID BETWEEN '{Show_CEID1}' AND '{Show_CEID2}'";
				}
				else
				{
					sql = $"SELECT DISTINCT CELL_ID FROM [CAROUSEL_TRANSFER] WHERE SHOW_CELL_ID LIKE '{Show_CEID1}' OR SHOW_CELL_ID = '{Show_CEID1}'";
				}
				Startup.Local_SQLServer.Query(sql, ref Result);
				foreach (DataRow dr in Result.Rows)
				{
					Context.Add(dr["CELL_ID"].ToString().Trim());
				}
			}
			return Context;
		}
		public String Show_CEID_Transfer(string CEID)
		{
			string Context = string.Empty;
			DataTable Result1 = new DataTable();
			sql = $"SELECT DISTINCT SHOW_CELL_ID FROM [CAROUSEL_TRANSFER] WHERE CELL_ID ='{CEID}'";
			Startup.Local_SQLServer.Query(sql, ref Result1);
			foreach (DataRow dr in Result1.Rows)
			{
				Context = dr["SHOW_CELL_ID"].ToString().Trim();
			}
			return Context;
		}
		#endregion
		#region Carousel_Utility
		public List<string> CU_CSID()
		{
			sql = string.Empty;
			List<string> Context = new List<string>();
			sql = $"SELECT CAROUSEL_ID FROM [MONITOR_SETTING]";
			bool n = Startup.STK_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string carouselid = (dr["CAROUSEL_ID"].ToString().Trim());
				Context.Add(carouselid);
			}
			return Context;
		}
		public List<string> CU_string(string rCSID)
		{
			List<string> Context = new List<string>();
			sql = string.Empty;
			try
			{
				sql = $"SELECT * FROM [MONITOR_SETTING] WHERE CAROUSEL_ID='{rCSID}'";
				bool n = Startup.STK_SQLServer.Query(sql, ref Result, true);
				foreach (DataRow dr in Result.Rows)
				{
					string carouselid = (dr["CAROUSEL_ID"].ToString().Trim());
					string tempupper = dr["TEMPERATER_UPPER_LIMIT"].ToString().Trim();
					string templower = dr["TEMPERATER_LOWER_LIMIT"].ToString().Trim();
					string humupper = dr["HUMIDITY_UPPER_LIMIT"].ToString().Trim();
					string humlower = dr["HUMIDITY_LOWER_LIMIT"].ToString().Trim();
					string onn2 = dr["TURN_ON_N2_HUMIDITY"].ToString().Trim();
					string offn2 = dr["TURN_OFF_N2_HUMIDITY"].ToString().Trim();

					Context.Add(carouselid);
					Context.Add(tempupper);
					Context.Add(templower);
					Context.Add(humupper);
					Context.Add(humlower);
					Context.Add(onn2);
					Context.Add(offn2);
				}
			}
			catch (Exception ex)
			{

			}
			return Context;
		}
		#endregion Carousel_Utility
		#region Cell_Utility
		public List<string> Cell_Utility_String(string rCSID)
		{
			List<string> Context = new List<string>();
			rCSID = CSID_Transfer(rCSID);
			sql = $"SELECT * FROM [CELL_STATUS] WHERE CAROUSEL_ID='{rCSID}' ORDER BY 2";
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string carouselid = (dr["CAROUSEL_ID"].ToString().Trim());
				string cellid = dr["CELL_ID"].ToString().Trim();
				string state = dr["STATE"].ToString().Trim();
				carouselid = Show_CSID_Transfer(carouselid);
				cellid = Show_CEID_Transfer(cellid);

				Context.Add(carouselid);
				Context.Add(cellid);
				Context.Add(state);
			}
			return Context;
		}
		#endregion
		#region Carousel_Setting
		public List<string> THS_String(string rCSID)
		{
			sql = string.Empty;
			List<string> Context = new List<string>();
			Context.Clear();
			rCSID = CSID_Transfer(rCSID);
			sql = $"SELECT * FROM [CAROUSEL_STATUS]";
			sql += $" WHERE CAROUSEL_ID='{rCSID}'";
			bool n = Startup.STK_SQLServer.Query(sql, ref Result, true);
			foreach (DataRow dr in Result.Rows)
			{
				string temperature = dr["TEMPERATER"].ToString().Trim();
				string humidity = dr["HUMIDITY"].ToString().Trim();
				string storestatus = dr["STORE_STATUS"].ToString().Trim();
				string usespace=dr["USE_SPACE"].ToString().Trim();
				string totalspace=dr["TOTAL_SPACE"].ToString().Trim();
				Context.Add(temperature);
				Context.Add(humidity);
				Context.Add(storestatus);
				Context.Add(usespace);
				Context.Add(totalspace);
			}
			return Context;
		}
		public List<string> Carousel_setting()
		{
			List<string> Context = new List<string>();
			sql = string.Empty;
			try
			{
				sql = $"SELECT * FROM [MONITOR_SETTING]";
				bool n = Startup.STK_SQLServer.Query(sql, ref Result, true);
				foreach (DataRow dr in Result.Rows)
				{
					string carouselid = (dr["CAROUSEL_ID"].ToString().Trim());
					string temperaterupper = dr["TEMPERATER_UPPER_LIMIT"].ToString().Trim();
					string temperaterlower = dr["TEMPERATER_LOWER_LIMIT"].ToString().Trim();
					string humidityupper = dr["HUMIDITY_UPPER_LIMIT"].ToString().Trim();
					string humiditylower = dr["HUMIDITY_LOWER_LIMIT"].ToString().Trim();
					string turnon = dr["TURN_ON_N2_HUMIDITY"].ToString().Trim();
					string turnoff = dr["TURN_OFF_N2_HUMIDITY"].ToString().Trim();
					carouselid = Show_CSID_Transfer(carouselid);

					Context.Add(carouselid);
					Context.Add(temperaterupper);
					Context.Add(temperaterlower);
					Context.Add(humidityupper);
					Context.Add(humiditylower);
					Context.Add(turnon);
					Context.Add(turnoff);
				}
			}
			catch (Exception ex)
			{

			}
			return Context;
		}
		#endregion Carousel_Setting
		#region Storage
		public List<string> Storage_Context(string rBN1, string rBN2, string rCSID1, string rCSID2, string rSTR1, string rSTR2, string rCMID1, string rCMID2)
		{
			List<string> Context = new List<string>();
			int sqlcount = 0;
			sql = string.Empty;
			Context.Clear();
			string rBN = string.Empty;
			string rSTR = string.Empty;
			string rCMID = string.Empty;
			try
			{
				if ((rCSID1 != null && rCSID1 != "") || (rCSID2 != null && rCSID2 != ""))
				{
					List<string> CSID_List = Vogue_CSID_Transfer(rCSID1, rCSID2);
					if ((rBN1 != null && rBN1 != "") && (rBN2 != null && rBN2 != "")) rBN = $"BATCH_NO between '{rBN1}' and '{rBN2}'";
					if ((rSTR1 != null && rSTR1 != "") && (rSTR2 != null && rSTR2 != "")) rSTR = $"SOTERIA between '{rSTR1}' and '{rSTR2}'";
					if ((rCMID1 != null && rCMID1 != "") && (rCMID2 != null && rCMID2 != "")) rCMID = $"CUSTERM_ID between '{rCMID1}' and '{rCMID2}'";
					if (rBN1 != null && rBN1!="" && (rBN2 == null || rBN2==""))
					{
						if (rBN1.IndexOf(',') == -1)
						{ rBN = $"(BATCH_NO LIKE '{rBN1}' OR BATCH_NO ='{rBN1}')"; }
						else
						{
							string[] rBN1arr = rBN1.Split(',');
							for (int i = 0; i < rBN1arr.Length; i++)
							{
								if (i > 0)
									rBN += " OR ";
								rBN += $"(BATCH_NO LIKE '{rBN1arr[i]}' OR BATCH_NO ='{rBN1arr[i]}')";
							}
						}
					}
					if (rCMID1 != null && rCMID1!="" && (rCMID2 == null || rCMID2==""))
					{
						if (rCMID1.IndexOf(',') == -1)
						{ rCMID = $"(CUSTOMER_ID LIKE '{rCMID1}' OR CUSTOMER_ID ='{rCMID1}')"; }
						else
						{
							string[] rCMID1arr = rCMID1.Split(',');
							for (int i = 0; i < rCMID1arr.Length; i++)
							{
								if (i > 0)
									rCMID += " OR ";
								rCMID += $"(CUSTOMER_ID LIKE '{rCMID1arr[i]}' OR CUSTOMER_ID = '{rCMID1arr[i]}')";
							}
						}
					}
					if (rSTR1 != null && rSTR1 != ""  && (rSTR2 == null || rSTR2==""))
					{
						if (rSTR1.IndexOf(',') == -1)
						{ rSTR = $"(SOTERIA LIKE '{rSTR1}' OR SOTERIA = '{rSTR1}')"; }
						else
						{
							string[] rSTR1arr = rSTR1.Split(',');
							for (int i = 0; i < rSTR1arr.Length; i++)
							{
								if (i > 0)
									rSTR += " OR ";
								rSTR += $"(SOTERIA LIKE '{rSTR1arr[i]}' OR SOTERIA = '{rSTR1arr[i]}')";
							}
						}
					}
					sql = $"SELECT * FROM [KDX_STORAGE_STATUS] WHERE ";
					if (rBN != "" || rCMID != "" || rSTR != "")
					{
						if (rBN != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rBN;
							sqlcount++;
						}
						if (rSTR != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rSTR;
							sqlcount++;
						}
						if (rCMID != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rCMID;
							sqlcount++;
						}
					}
					string sql_temp = sql;
					for (int i = 0; i < CSID_List.Count; i++)
					{
						sql = sql_temp;
						if (sqlcount != 0)
						{
							sql += $" AND CAROUSEL_ID='{CSID_List[i]}'";
							sql += " AND BOX_ID!=''";
							sql += " ORDER BY CAROUSEL_ID,CELL_ID";
						}
						else
						{
							sql += $"CAROUSEL_ID='{CSID_List[i]}'";
							sql += " AND BOX_ID!=''";
							sql += " ORDER BY CAROUSEL_ID,CELL_ID";
						}
							
						Startup.STK_SQLServer.Query(sql, ref Result, true);
						foreach (DataRow dr in Result.Rows)
						{
							string boxid = dr["BOX_ID"].ToString().Trim();
							string batchno = dr["BATCH_NO"].ToString().Trim();
							string groupno = dr["GROUP_NO"].ToString().Trim();
							string custermid = dr["CUSTOMER_ID"].ToString().Trim();
							string soteria = dr["SOTERIA"].ToString().Trim();
							string carouselid = dr["CAROUSEL_ID"].ToString().Trim();
							string cellid = dr["CELL_ID"].ToString().Trim();
							carouselid = Show_CSID_Transfer(carouselid);
							cellid = Show_CEID_Transfer(cellid);

							Context.Add(boxid);
							Context.Add(batchno);
							Context.Add(groupno);
							Context.Add(soteria);
							Context.Add(custermid);
							Context.Add(carouselid);
							Context.Add(cellid);
						}
					}
				}
				else
				{
					if ((rBN1 != null && rBN1 != "") && (rBN2 != null && rBN2 != "")) rBN = $"BATCH_NO between '{rBN1}' and '{rBN2}'";
					if ((rSTR1 != null && rSTR1 != "") && (rSTR2 != null && rSTR2 != "")) rSTR = $"SOTERIA between '{rSTR1}' and '{rSTR2}'";
					if ((rCMID1 != null && rCMID1 != "") && (rCMID2 != null && rCMID2 != "")) rCMID = $"CUSTERM_ID between '{rCMID1}' and '{rCMID2}'";
					if (rBN1 != null && rBN1 != "" && (rBN2 == null || rBN2 == ""))
					{
						if (rBN1.IndexOf(',') == -1)
						{ rBN = $"(BATCH_NO LIKE '{rBN1}' OR BATCH_NO = '{rBN1}')"; }
						else
						{
							string[] rBN1arr = rBN1.Split(',');
							for (int i = 0; i < rBN1arr.Length; i++)
							{
								if (i > 0)
									rBN += " OR ";
								rBN += $"(BATCH_NO LIKE '{rBN1arr[i]}' OR BATCH_NO ='{rBN1arr[i]}')";
							}
						}
					}
					if (rCMID1 != null && rCMID1 != "" && (rCMID2 == null || rCMID2 == ""))
					{
						if (rCMID1.IndexOf(',') == -1)
						{ rCMID = $"(CUSTOMER_ID LIKE '{rCMID1}' OR CUSTOMER_ID = '{rCMID1}')"; }
						else
						{
							string[] rCMID1arr = rCMID1.Split(',');
							for (int i = 0; i < rCMID1arr.Length; i++)
							{
								if (i > 0)
									rCMID += " OR ";
								rCMID += $"(CUSTOMER_ID LIKE '{rCMID1arr[i]}' OR CUSTOMER_ID = '{rCMID1arr[i]}')";
							}
						}
					}
					if (rSTR1 != null && rSTR1 != "" && (rSTR2 == null || rSTR2 == ""))
					{
						if (rSTR1.IndexOf(',') == -1)
						{ rSTR = $"(SOTERIA LIKE '{rSTR1}' OR SOTERIA = '{rSTR1}')"; }
						else
						{
							string[] rSTR1arr = rSTR1.Split(',');
							for (int i = 0; i < rSTR1arr.Length; i++)
							{
								if (i > 0)
									rSTR += " OR ";
								rSTR += $"(SOTERIA LIKE '{rSTR1arr[i]}' OR SOTERIA = '{rSTR1arr[i]}')";
							}
						}
					}

					sql = $"SELECT * FROM [KDX_STORAGE_STATUS] WHERE ";
					if (rBN != "" || rCMID != "" || rSTR != "")
					{
						if (rBN != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rBN;
							sqlcount++;
						}
						if (rSTR != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rSTR;
							sqlcount++;
						}
						if (rCMID != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rCMID;
							sqlcount++;
						}
					}
					if (sqlcount != 0)
					{
						sql = sql + " ORDER BY 1";
						bool n = Startup.STK_SQLServer.Query(sql, ref Result, true);
						foreach (DataRow dr in Result.Rows)
						{
							string boxid = dr["BOX_ID"].ToString().Trim();
							string batchno = dr["BATCH_NO"].ToString().Trim();
							string groupno = dr["GROUP_NO"].ToString().Trim();
							string custermid = dr["CUSTOMER_ID"].ToString().Trim();
							string soteria = dr["SOTERIA"].ToString().Trim();
							string carouselid = dr["CAROUSEL_ID"].ToString().Trim();
							string cellid = dr["CELL_ID"].ToString().Trim();
							carouselid = Show_CSID_Transfer(carouselid);
							cellid = Show_CEID_Transfer(cellid);

							Context.Add(boxid);
							Context.Add(batchno);
							Context.Add(groupno);
							Context.Add(soteria);
							Context.Add(custermid);
							Context.Add(carouselid);
							Context.Add(cellid);
						}
					}
				}
			}
			catch (Exception ex)
			{

			}
			return Context;
		}
		#endregion

		#region Task
		public List<string> Task_Context()
        {
			List<string> Context = new List<string>();
			sql = string.Empty;
			try
			{
				sql = $"SELECT * FROM [TASK]";
				bool n=Startup.Local_SQLServer.Query(sql, ref Result, true);
				
				foreach (DataRow dr in Result.Rows)
				{
					string tasktype = dr["DIRECTION"].ToString().Trim();
					string boxid = dr["BOX_ID"].ToString().Trim();
					string priority = dr["PRIORITY"].ToString().Trim();
					string no = dr["ORDER_NO"].ToString().Trim();
					string batchno = dr["BATCH_NO"].ToString().Trim();
					string soteria = dr["SOTERIA"].ToString().Trim();
					string customerid = dr["CUSTOMER_ID"].ToString().Trim();
					string srcpos = dr["SRC_POS"].ToString().Trim();
					string srccell = dr["SRC_CELL_ID"].ToString().Trim();
					string tarpos = dr["TAR_POS"].ToString().Trim();
					string tarcell = dr["TAR_CELL_ID"].ToString().Trim();
					string commandid = dr["COMMAND_ID"].ToString().Trim();
					string status = dr["STATUS"].ToString().Trim();
					Context.Add(tasktype);
					Context.Add(boxid);
					Context.Add(batchno);
					Context.Add(soteria);
					Context.Add(customerid);
					Context.Add(srcpos);
					Context.Add(srccell);
					Context.Add(tarpos);
					Context.Add(tarcell);
					Context.Add(priority);
					Context.Add(commandid);
					Context.Add(status);
				}
			}
			catch (Exception ex)
			{

			}
			return Context;
		}
		public List<string> Task_Priority(string col)
        {
			List<string> Context = new List<string>();
            if (col == "Name")
            {
				sql = "SELECT NAME FROM [SYSTEM_SETTING]";
				Startup.Local_SQLServer.Query(sql, ref Result);
				foreach(DataRow dr in Result.Rows)
                {
					Context.Add(dr["NAME"].ToString().Trim());
                }
			return Context;
			}
            else
            {
				sql = "SELECT VALUE FROM [SYSTEM_SETTING]";
				Startup.Local_SQLServer.Query(sql, ref Result);
				foreach (DataRow dr in Result.Rows)
				{
					Context.Add(dr["VALUE"].ToString().Trim());
				}
				return Context;
            }
        }
		public List<string> Task_Priority_List(string CMD_ID)
        {
			List<string> Context = new List<string>();
            try
            {
				DataTable dt = Startup.Local_SQLServer.SelectDB("*", "[TASK]", $"COMMAND_ID='{CMD_ID}'");
				foreach (DataRow dr_temp in dt.Rows)
				{
					string priority = dr_temp["PRIORITY"].ToString().Trim();
					string commandid = dr_temp["COMMAND_ID"].ToString().Trim();

					Context.Add(commandid);
					Context.Add(priority);
				}
			}
			catch(Exception ex)
            {

            }
			return Context;
        }
        #endregion


		#region Check_Inventory
		public List<string> Check_inventory_Context(string rBN1, string rBN2, string rCSID1, string rCSID2, string rSTR1, string rSTR2, string rCMID1, string rCMID2, string rDaysApart1, string rDaysApart2)
		{
			List<string> Context = new List<string>();
			int sqlcount = 0;
			sql = string.Empty;
			string rBN = string.Empty;
			string rSTR = string.Empty;
			string rCMID = string.Empty;
			string rDaysApart = string.Empty;
			try
			{
				if ((rCSID1 != null && rCSID1 != "") || (rCSID2 != null && rCSID2 != ""))
				{
					List<string> CSID_List = Vogue_CSID_Transfer(rCSID1, rCSID2);
					if ((rBN1 != null && rBN1 != "") && (rBN2 != null && rBN2 != "")) rBN = $"BATCH_NO between '{rBN1}' and '{rBN2}'";
					if ((rSTR1 != null && rSTR1 != "") && (rSTR2 != null && rSTR2 != "")) rSTR = $"SOTERIA between '{rSTR1}' and '{rSTR2}'";
					if ((rCMID1 != null && rCMID1 != "") && (rCMID2 != null && rCMID2 != "")) rCMID = $"CUSTERM_ID between '{rCMID1}' and '{rCMID2}'";
					if (rBN1 != null && rBN1 != "" && (rBN2 == null || rBN2 == ""))
					{
						if (rBN1.IndexOf(',') == -1)
						{ rBN = $"(BATCH_NO LIKE '{rBN1}' OR BATCH_NO ='{rBN1}')"; }
						else
						{
							string[] rBN1arr = rBN1.Split(',');
							for (int i = 0; i < rBN1arr.Length; i++)
							{
								if (i > 0)
									rBN += " OR ";
								rBN += $"(BATCH_NO LIKE '{rBN1arr[i]}' OR BATCH_NO ='{rBN1arr[i]}')";
							}
						}
					}
					if (rCMID1 != null && rCMID1 != "" && (rCMID2 == null || rCMID2 == ""))
					{
						if (rCMID1.IndexOf(',') == -1)
						{ rCMID = $"(CUSTOMER_ID LIKE '{rCMID1}' OR CUSTOMER_ID='{rCMID1}')"; }
						else
						{
							string[] rCMID1arr = rCMID1.Split(',');
							for (int i = 0; i < rCMID1arr.Length; i++)
							{
								if (i > 0)
									rCMID += " OR ";
								rCMID += $"(CUSTOMER_ID LIKE '{rCMID1arr[i]}' OR CUSTOMER_ID = '{rCMID1arr[i]}')";
							}
						}
					}
					if (rSTR1 != null && rSTR1 != "" && (rSTR2 == null || rSTR2 == ""))
					{
						if (rSTR1.IndexOf(',') == -1)
						{ rSTR = $"(SOTERIA LIKE '{rSTR1}')"; }
						else
						{
							string[] rSTR1arr = rSTR1.Split(',');
							for (int i = 0; i < rSTR1arr.Length; i++)
							{
								if (i > 0)
									rSTR += " OR ";
								rSTR += $"(SOTERIA LIKE '{rSTR1arr[i]}' OR SOTERIA = '{rSTR1arr[i]}')";
							}
						}
					}
					if (rDaysApart1 != "" && rDaysApart1 != null)
					{
						rDaysApart = "DATEDIFF(day,[LAST_UPDATE_DATE], CONVERT(date, GETDATE()))";
						if (rDaysApart2 == "M")
							rDaysApart += " > ";
						else
							rDaysApart += "<";
						rDaysApart += $" '{rDaysApart1}' ";
					}

					sql = $"SELECT *,DATEDIFF(day,[LAST_UPDATE_DATE], CONVERT(date, GETDATE())) [DAYS] FROM [KDX_STORAGE_STATUS] WHERE ";
					if (rBN != "" || rCMID != "" || rSTR != "" || rDaysApart != "")
					{
						if (rBN != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rBN;
							sqlcount++;
						}
						if (rSTR != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rSTR;
							sqlcount++;
						}
						if (rCMID != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rCMID;
							sqlcount++;
						}
						if (rDaysApart != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rDaysApart;
							sqlcount++;
						}

					}
					for (int i = 0; i < CSID_List.Count; i++)
					{
						if (sqlcount != 0)
							sql += $" AND CAROUSEL_ID='{CSID_List[i]}'";
						else
							sql += $" CAROUSEL_ID='{CSID_List[i]}'";
						sql += " ORDER BY CAROUSEL_ID,CELL_ID";
						bool n = Startup.STK_SQLServer.Query(sql, ref Result, true);
						foreach (DataRow dr in Result.Rows)
						{
							string carouselid = (dr["CAROUSEL_ID"].ToString().Trim());
							string cellid = dr["CELL_ID"].ToString().Trim();
							string boxid = dr["BOX_ID"].ToString().Trim();
							string batchno = dr["BATCH_NO"].ToString().Trim();
							string checkresult = dr["CHECK_RESULT"].ToString().Trim();
							string soteria = dr["SOTERIA"].ToString().Trim();
							string customerid = dr["CUSTOMER_ID"].ToString().Trim();
							string days = dr["DAYS"].ToString().Trim();
							carouselid = Show_CSID_Transfer(carouselid);
							cellid = Show_CEID_Transfer(cellid);

							Context.Add(batchno);
							Context.Add(carouselid);
							Context.Add(cellid);
							Context.Add(boxid);
							Context.Add(soteria);
							Context.Add(customerid);
							Context.Add(days);
							Context.Add(checkresult);
						}
					}
				}
				else
				{
					if ((rBN1 != null && rBN1 != "") && (rBN2 != null && rBN2 != "")) rBN = $"BATCH_NO between '{rBN1}' and '{rBN2}'";
					if ((rSTR1 != null && rSTR1 != "") && (rSTR2 != null && rSTR2 != "")) rSTR = $"SOTERIA between '{rSTR1}' and '{rSTR2}'";
					if ((rCMID1 != null && rCMID1 != "") && (rCMID2 != null && rCMID2 != "")) rCMID = $"CUSTERM_ID between '{rCMID1}' and '{rCMID2}'";
					if (rBN1 != null && rBN2 == null)
					{
						if (rBN1.IndexOf(',') == -1)
						{ rBN = $"([BATCH_NO] LIKE '{rBN1}' OR [BATCH_NO] = '{rBN1}')"; }
						else
						{
							string[] rBN1arr = rBN1.Split(',');
							for (int i = 0; i < rBN1arr.Length; i++)
							{
								if (i > 0)
									rBN += " OR ";
								rBN += $"(BATCH_NO LIKE '{rBN1arr[i]}' OR BATCH_NO = '{rBN1arr[i]}')";
							}
						}
					}
					if (rCMID1 != null && rCMID2 == null)
					{
						if (rCMID1.IndexOf(',') == -1)
						{ rCMID = $"(CUSTOMER_ID LIKE '{rCMID1}' OR CUSTOMER_ID = '{rCMID1}')"; }
						else
						{
							string[] rCMID1arr = rCMID1.Split(',');
							for (int i = 0; i < rCMID1arr.Length; i++)
							{
								if (i > 0)
									rCMID += " OR ";
								rCMID += $"(CUSTOMER_ID LIKE '{rCMID1arr[i]}' OR CUSTOMER_ID = '{rCMID1arr[i]}')";
							}
						}
					}
					if (rSTR1 != null && rSTR2 == null)
					{
						if (rSTR1.IndexOf(',') == -1)
						{ rSTR = $"(SOTERIA LIKE '{rSTR1}' OR SOTERIA = '{rSTR1}')"; }
						else
						{
							string[] rSTR1arr = rSTR1.Split(',');
							for (int i = 0; i < rSTR1arr.Length; i++)
							{
								if (i > 0)
									rSTR += " OR ";
								rSTR += $"(SOTERIA LIKE '{rSTR1arr[i]}' OR SOTERIA = '{rSTR1arr[i]}')";
							}
						}
					}
					if (rDaysApart1 != "" && rDaysApart1 != null)
					{
						rDaysApart = "DATEDIFF(day,[LAST_UPDATE_DATE], CONVERT(date, GETDATE()))";
						if (rDaysApart2 == "M")
							rDaysApart += " > ";
						else
							rDaysApart += "<";
						rDaysApart += $" '{rDaysApart1}' ";
					}

					sql = $"SELECT *,DATEDIFF(day,[LAST_UPDATE_DATE], CONVERT(date, GETDATE())) [DAYS] FROM [KDX_STORAGE_STATUS] WHERE ";
					if (rBN != "" || rCMID != "" || rSTR != "" || rDaysApart != "")
					{
						if (rBN != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rBN;
							sqlcount++;
						}
						if (rSTR != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rSTR;
							sqlcount++;
						}
						if (rCMID != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rCMID;
							sqlcount++;
						}
						if (rDaysApart != "")
						{
							if (sqlcount != 0) sql = sql + " AND ";
							sql = sql + rDaysApart;
							sqlcount++;
						}

					}
					if (sqlcount != 0)
					{
						sql = sql + " ORDER BY 1,2";
						bool n = Startup.STK_SQLServer.Query(sql, ref Result, true);
						foreach (DataRow dr in Result.Rows)
						{
							string carouselid = (dr["CAROUSEL_ID"].ToString().Trim());
							string cellid = dr["CELL_ID"].ToString().Trim();
							string boxid = dr["BOX_ID"].ToString().Trim();
							string batchno = dr["BATCH_NO"].ToString().Trim();
							string checkresult = dr["CHECK_RESULT"].ToString().Trim();
							string soteria = dr["SOTERIA"].ToString().Trim();
							string customerid = dr["CUSTOMER_ID"].ToString().Trim();
							string days = dr["DAYS"].ToString().Trim();
							carouselid = Show_CSID_Transfer(carouselid);
							cellid = Show_CEID_Transfer(cellid);

							Context.Add(batchno);
							Context.Add(carouselid);
							Context.Add(cellid);
							Context.Add(boxid);
							Context.Add(soteria);
							Context.Add(customerid);
							Context.Add(days);
							Context.Add(checkresult);
						}
					}
				}
			}
			catch (Exception ex)
			{

			}
			return Context;
		}
		public List<string> Check_Inventory_History()
		{
			List<string> Context = new List<string>();
			sql = string.Empty;
			try
			{
				sql = $"SELECT * FROM [CAROUSEL_CHECK_HISTORY]";
				bool n = Startup.Local_SQLServer.Query(sql, ref Result, true);
				foreach (DataRow dr in Result.Rows)
				{
					string result = dr["RESULT"].ToString().Trim();
					string commandid = dr["COMMAND_ID"].ToString().Trim();
					string starttime = dr["START_TIME"].ToString().Trim();
					string endtime = dr["END_TIME"].ToString().Trim();

					Context.Add(commandid);
					Context.Add(result);
					Context.Add(starttime);
					Context.Add(endtime);
				}
			}
			catch (Exception ex)
			{

			}
			return Context;
		}
		public List<string> Check_Inventory_History_Detail(string commandid)
        {
			List<string> Context = new List<string>();
			sql = string.Empty;
			try
			{
				sql = $"SELECT * FROM [CAROUSEL_CHECK_HISTORY_DETAIL] WHERE COMMAND_ID='{commandid}'";
				bool n = Startup.Local_SQLServer.Query(sql, ref Result, true);
				foreach (DataRow dr in Result.Rows)
				{
					string carouselid = (dr["CAROUSEL_ID"].ToString().Trim());
					string cellid = dr["CELL_ID"].ToString().Trim();
					string boxid = dr["BOX_ID"].ToString().Trim();
					string checkresult = dr["CHECK_RESULT"].ToString().Trim();
					carouselid = Show_CSID_Transfer(carouselid);
					cellid = Show_CEID_Transfer(cellid);
					string result = string.Empty;
					switch (checkresult)
					{
						case "0":
							result = "正常";
							break;
						case "1":
							result = "有料無帳";
							break;
						case "2":
							result = "有帳無料";
							break;
						case "3":
							result = "與讀取不符";
							break;
					}

					Context.Add(carouselid);
					Context.Add(cellid);
					Context.Add(boxid);
					Context.Add(result);
				}
			}
			catch (Exception ex)
			{

			}
			return Context;
		}
		public List<string> Check_Inventory_Task()
        {
			sql = string.Empty;
			List<string> Context = new List<string>();
			sql = "SELECT * FROM [CAROUSEL_CHECK_LIST]";
			bool n = Startup.Local_SQLServer.Query(sql, ref Result);
			foreach(DataRow dr in Result.Rows)
            {
				string commandid = dr["COMMAND_ID"].ToString().Trim();
				string checkresult = dr["RESULT"].ToString().Trim();
				string status = dr["STATUS"].ToString().Trim();
				Context.Add(commandid);
				Context.Add(checkresult);
				Context.Add(status);
            }
			return Context;

		}
		public List<string> Check_Inventory_Task_Detail(string commandid)
        {
			List<string> Context = new List<string>();
			sql = string.Empty;
			try
            {
				sql = $"SELECT * FROM [CAROUSEL_CHECK_LIST_DETAIL] WHERE COMMAND_ID='{commandid}'";
				bool n = Startup.Local_SQLServer.Query(sql, ref Result, true);
				foreach (DataRow dr in Result.Rows)
				{
					string carouselid = (dr["CAROUSEL_ID"].ToString().Trim());
					string cellid = dr["CELL_ID"].ToString().Trim();
					string boxid = dr["BOX_ID"].ToString().Trim();
					carouselid = Show_CSID_Transfer(carouselid);
					cellid = Show_CEID_Transfer(cellid);

					Context.Add(carouselid);
					Context.Add(cellid);
					Context.Add(boxid);
				}
			}
            catch (Exception ex)
            {

            }
			return Context;
		}
        #endregion Check_Inventory

		#region Stock
		public List<string> Stock_Select_List(string rBN1,string rBN2,string rN1,string rN2)
		{
			sql = string.Empty;
			List<string> Context = new List<string>();
			string rBN = string.Empty;
			string rN = string.Empty;
			try
            {
				if ((rBN1 != null && rBN1 != "") && (rBN2 != null && rBN2 != "")) rBN = $"BATCH_NO between '{rBN1}' and '{rBN2}'";
				if (rBN1 != null && rBN1!="" && (rBN2 == null||rBN2==""))
				{
					if (rBN1.IndexOf(',') == -1)
					{
						if (rBN1.IndexOf('%') != -1 || rBN1.IndexOf('_') != -1)
							rBN = $"(BATCH_NO LIKE '{rBN1}')";
						else
							rBN = $"BATCH_NO = '{rBN1}'";
					}
					else
					{
						string[] rBN1arr = rBN1.Split(',');
						for (int i = 0; i < rBN1arr.Length; i++)
						{
							if (i > 0)
								rBN += " OR ";
							rBN += $"BATCH_NO = '{rBN1arr[i]}'";
						}
					}
				}
				if ((rN1 != null && rN1 != "") && (rN2 != null && rN2 != "")) rN = $"ORDER_NO BETWEEN '{rN1}' AND '{rN2}'";
				if (rN1 != null && rN1!="" && (rN2 == null || rN2 == ""))
				{
					if (rN1.IndexOf(',') == -1)
					{
						if (rN1.IndexOf('%') != -1 || rN1.IndexOf('_') != -1)
							rN = $"(ORDER_NO LIKE '{rN1}')";
						else
							rN = $"ORDER_NO = '{rN1}'";
					}
					else
					{
						string[] rN1arr = rN1.Split(',');
						for (int i = 0; i < rN1arr.Length; i++)
						{
							if (i > 0)
								rN += " OR ";
							rN += $"ORDER_NO = '{rN1arr[i]}'";
						}
					}
				}
				if(rBN!="")
					sql = $"SELECT * FROM [BATCH_LIST] WHERE {rBN}";
				if(rN!=null && rN!="")
					sql = $"SELECT * FROM [BATCH_LIST] WHERE {rN}";
				bool n = Startup.Local_SQLServer.Query(sql, ref Result);
				foreach (DataRow dr in Result.Rows)
				{
					string no = dr["ORDER_NO"].ToString().Trim();
					string batchno = dr["BATCH_NO"].ToString().Trim();
					string soteria = dr["SOTERIA"].ToString().Trim();
					string customerid = dr["CUSTOMER_ID"].ToString().Trim();
					string boxid = dr["BOX_ID"].ToString().Trim();
					Context.Add(no);
					Context.Add(batchno);
					Context.Add(soteria);
					Context.Add(customerid);
					Context.Add(boxid);
				}
			}
			catch(Exception ex)
            {
				List<string> error = new List<string>();
				return error;
            }
			return Context;
        }
		public List<string> Stock_Select_List()
        {
			sql = string.Empty;
			List<string> Context = new List<string>();
			sql = $"SELECT * FROM [BATCH_LIST] WHERE [READY_DELIVERY]='0'";
			bool n = Startup.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				string no = dr["ORDER_NO"].ToString().Trim();
				string batchno = dr["BATCH_NO"].ToString().Trim();
				string soteria = dr["SOTERIA"].ToString().Trim();
				string customerid = dr["CUSTOMER_ID"].ToString().Trim();
				string boxid = dr["BOX_ID"].ToString().Trim();
				Context.Add(no);
				Context.Add(batchno);
				Context.Add(soteria);
				Context.Add(customerid);
				Context.Add(boxid);
			}
			return Context;

		}
		public List<string> Stock_Task_List()
        {
			sql = string.Empty;
			List<string> Context = new List<string>();
            try
            {
				sql = "SELECT * FROM [TASK]";
				bool n = Startup.Local_SQLServer.Query(sql, ref Result);
				foreach(DataRow dr in Result.Rows)
                {
					string tasktype = dr["DIRECTION"].ToString().Trim();
					string boxid = dr["BOX_ID"].ToString().Trim();
					string priority = dr["PRIORITY"].ToString().Trim();
					string no = dr["ORDER_NO"].ToString().Trim();
					string batchno = dr["BATCH_NO"].ToString().Trim();
					string soteria = dr["SOTERIA"].ToString().Trim();
					string customerid = dr["CUSTOMER_ID"].ToString().Trim();
					string srcpos = dr["SRC_POS"].ToString().Trim();
					string srccell = dr["SRC_CELL_ID"].ToString().Trim();
					string tarpos = dr["TAR_POS"].ToString().Trim();
					string tarcell = dr["TAR_CELL_ID"].ToString().Trim();
					string commandid = dr["COMMAND_ID"].ToString().Trim();
					string status = dr["STATUS"].ToString().Trim();
					Context.Add(tasktype);
					Context.Add(boxid);
					Context.Add(batchno);
					Context.Add(soteria);
					Context.Add(customerid);
					Context.Add(srcpos);
					Context.Add(srccell);
					Context.Add(tarpos);
					Context.Add(tarcell);
					Context.Add(priority);
					Context.Add(commandid);
					Context.Add(status);
				}
            }
			catch(Exception ex)
            {
				List<string> err = new List<string>();
				return err;
            }
			return Context;
        }
		public string Lifter_Check()
		{
			string context = "0";
			string sqltemp = "SELECT COUNT(MODE) MODE FROM [STATUS] WHERE MODE='Delivery' and STATUS='OK'";
			DataTable result_temp = new DataTable();
			Startup.Local_SQLServer.Query(sqltemp, ref result_temp);
			foreach(DataRow drtemp in result_temp.Rows)
			{
				context = drtemp["MODE"].ToString().Trim();
			}
			return context;
		}
		public string Lifter_Select()
		{
			string context = string.Empty;
			string sqltemp = "SELECT TOP(1) NAME FROM [STATUS] WHERE MODE='Delivery' and STATUS='OK'";
			DataTable result_temp = new DataTable();
			Startup.Local_SQLServer.Query(sqltemp, ref result_temp);
			foreach (DataRow drtemp in result_temp.Rows)
			{
				context = drtemp["NAME"].ToString().Trim();
			}
			return context;
		}
		#endregion Stock
		#region UserSetting
		public List<string> User_String()
        {
			sql = string.Empty;
			List<string> Context = new List<string>();
			sql = "SELECT * FROM [ACCOUNT]";
			bool n = Startup.Local_SQLServer.Query(sql, ref Result);
			foreach(DataRow dr in Result.Rows)
            {
				string userid = dr["USER_ID"].ToString().Trim();
				string usergroup = dr["USER_GROUP"].ToString().Trim();

				Context.Add(userid);
				Context.Add(usergroup);
            }
			return Context;
        }
		public string CPC_Status()
        {
			sql = "SELECT STATUS FROM [WEBSERVICE_CONNECTION] WHERE NAME='CPC'";
			string status = string.Empty;
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach(DataRow dr in Result.Rows)
            {
				status = dr["STATUS"].ToString().Trim();
            }
			return status;
		}
		public List<string> UserAuth(string UG)
		{
			List<string> Context = new List<string>();
			try
			{
				string sqltemp = $"SELECT TOP(1) [AUTHORITY_TYPE],[USER_SYSTEMSETTING] FROM [ACCOUNT] WHERE USER_GROUP='{UG}'";
				DataTable dt_temp = new DataTable();
				Startup.Local_SQLServer.Query(sqltemp, ref dt_temp);
				foreach(DataRow dr_temp in dt_temp.Rows)
					{
						string auth = dr_temp["AUTHORITY_TYPE"].ToString().Trim();
						string systemsetting = dr_temp["USER_SYSTEMSETTING"].ToString().Trim();

						Context.Add(auth);
						Context.Add(systemsetting);
					}
				return Context;
			}
			catch(Exception ex)
			{
				return Context;
			}

		}
		#endregion UserSetting
		#region Delete
		public void Delete_Alarm(string rID,string rType)
        {
			sql = string.Empty;
			DataTable dt_temp = new DataTable();
			if (rType == "One")
			{
				sql = $"SELECT * FROM [ALARM] WHERE ID='{rID}'";
				Startup.Local_SQLServer.Query(sql, ref dt_temp);
				foreach(DataRow dr in dt_temp.Rows)
				{
					string sql_temp= $"INSERT INTO [ALARM_HISTORY] VALUES('{rID}','{dr["UNIT_NAME"].ToString().Trim()}','{dr["TIME"].ToString().Trim()}','{DateTime.Now}','{dr["MESSAGE"].ToString().Trim()}')";
					Startup.Local_SQLServer.NonQuery(sql_temp);
					sql_temp= $"DELETE FROM [ALARM] WHERE ID='{rID}' AND UNIT_NAME='{dr["UNIT_NAME"].ToString().Trim()}' AND TIME='{dr["TIME"].ToString().Trim()}'";
					Startup.Local_SQLServer.NonQuery(sql_temp);
					sql_temp = $"UPDATE [HS] SET [REQData]='{rID},0',[REQ]='1' WHERE [HS_NAME]='AlarmRptB'";
					Startup.Local_SQLServer.NonQuery(sql_temp);
				}
			}
			else
			{
				sql = "SELECT [ID],[UNIT_NAME],[TIME],[MESSAGE] FROM [ALARM]";
				Startup.Local_SQLServer.Query(sql, ref dt_temp);
				DateTime dt = DateTime.Now;
				foreach (DataRow dr in dt_temp.Rows)
				{
					string sql_temp = $"INSERT INTO [ALARM_HISTORY] ([ID],[UNIT_NAME],[OCCURED_TIME],[RESET_TIME],[MESSAGE]) VALUES ('{dr["ID"].ToString().Trim()}','{dr["UNIT_NAME"].ToString().Trim()}',{dr["TIME"]},'{dt}','{dr["MESSAGE"].ToString().Trim()}') ";
					Startup.Local_SQLServer.NonQuery(sql_temp);
				}
				string sql_ = $"DELETE FROM [ALARM]";
				Startup.Local_SQLServer.NonQuery(sql_);
				sql_ = $"UPDATE [HS] SET [REQData]='{rID},0',[REQ]='1',[WAY]='All' WHERE [HS_NAME]='AlarmRptB'";
				Startup.Local_SQLServer.NonQuery(sql_);
			}
		}
		public void Delete_User(string UN,string UG)
        {
			sql = string.Empty;
			sql = $"DELETE FROM [ACCOUNT] WHERE USER_ID='{UN}' AND USER_GROUP='{UG}'";
			Startup.Local_SQLServer.NonQuery(sql);
        }
		#endregion Delete
		#region Update
		public void Update_Priority(string P_Name, string P_Value) 
		{
			sql = $"UPDATE [SYSTEM_SETTING] SET VALUE='{P_Value}' WHERE NAME='{P_Name}'";
			Startup.Local_SQLServer.NonQuery(sql);
		}
		public void Update_Stock(string BOX,string Lifter)
		{
			string[] BOX_List = BOX.Split(',');
			Lifter = Lifter.Substring(Lifter.Length-1);
			sql = string.Empty;
			for(int i = 0; i < BOX_List.Length; i++)
			{
				DataTable dt = new DataTable();
				bool continue_flag = false;
				sql = $"UPDATE [HS] SET REQ='1',REQData='{BOX}',WAY='WebDelivery' WHERE HS_NAME='Delivery{Lifter}'";
				Startup.Local_SQLServer.NonQuery(sql);
				sql = $"SELECT ACKData FROM [HS] WHERE HS_NAME='Delivery{Lifter}'";
				Startup.Local_SQLServer.Query(sql, ref dt);
				foreach(DataRow dr in dt.Rows)
				{
					if (dr["ACKData"].ToString().Trim() != "OK")
						continue_flag = true;
				}
				if (continue_flag)
					continue;
				sql = $"UPDATE BATCH_LIST SET READY_DELIVERY = '3' WHERE BOX_ID = '{BOX}'";
				Startup.Local_SQLServer.NonQuery(sql);
			}

		}
        public string Update_Storage(string data)/*string rBOX,string rTCSID,string rTCEID,*/
		{
			string msg = "NG,Task to CPC Created Failed";
			try
			{
				sql = string.Empty;
				sql = $"UPDATE [HS] SET ";
				sql += $"REQ='1', ";
				sql += $"REQData='{data}' ";
				sql += $"WHERE HS_NAME='StorageChange'";
				Startup.Local_SQLServer.NonQuery(sql, true);
				msg = "Complete";
				return msg;
			}
			catch(Exception ex)
			{

			}
			return msg;
        }
		public void Update_Carousel_Utility(string[] rCSID,string[] rTU,string[] rTL,string[] rHU,string[] rHL,string[] rTON,string[] rTOFF,string rUser)
        {
			string time = DateTime.Now.ToString("yyyyMMddHHmmssfff");
			for (int i = 0; i < rCSID.Length; i++)
            {
				string sql_temp = $"SELECT * FROM [MONITOR_SETTING] WHERE CAROUSEL_ID='{rCSID[i]}' AND TEMPERATER_UPPER_LIMIT='{rTU[i]}' AND TEMPERATER_LOWER_LIMIT='{rTL[i]}' AND HUMIDITY_UPPER_LIMIT='{rHU[i]}' AND HUMIDITY_LOWER_LIMIT='{rHL[i]}' AND TURN_ON_N2_HUMIDITY='{rTON[i]}' AND TURN_OFF_N2_HUMIDITY='{rTOFF[i]}'";
				DataTable dt_temp = new DataTable();
				Startup.STK_SQLServer.Query(sql_temp, ref dt_temp);
				if (dt_temp.Rows.Count > 0)
					continue;
				sql = $"UPDATE [MONITOR_SETTING] SET TEMPERATER_UPPER_LIMIT='{rTU[i]}', TEMPERATER_LOWER_LIMIT='{rTL[i]}',HUMIDITY_UPPER_LIMIT='{rHU[i]}',HUMIDITY_LOWER_LIMIT='{rHL[i]}',TURN_ON_N2_HUMIDITY='{rTON[i]}',TURN_OFF_N2_HUMIDITY='{rTOFF[i]}' WHERE CAROUSEL_ID='{rCSID[i]}'";
				Startup.STK_SQLServer.NonQuery(sql);
				sql = $"INSERT INTO [MONITOR_SETTING_HISTORY_DETAIL] (COMMAND_ID,TEMPERATER_UPPER_LIMIT, TEMPERATER_LOWER_LIMIT,HUMIDITY_UPPER_LIMIT,HUMIDITY_LOWER_LIMIT,TURN_ON_N2_HUMIDITY,TURN_OFF_N2_HUMIDITY,CAROUSEL_ID) VALUES('{time}','{rTU[i]}','{rTL[i]}','{rHU[i]}','{rHL[i]}','{rTON[i]}','{rTOFF[i]}' ,'{rCSID[i]}')";
				Startup.Local_SQLServer.NonQuery(sql);
				sql = $"INSERT INTO [MONITOR_SETTING_HISTORY] VALUES ('{rUser}','{time}')";
				Startup.Local_SQLServer.NonQuery(sql);
			}
			
			

        }
		public void Update_Cell_Utility(string rCSID,string rCEID,string rState)
        {
			rCSID = CSID_Transfer(rCSID);
			rCEID = CEID_Transfer(rCEID);
			sql = $"UPDATE [CELL_STATUS] SET STATE='{rState}' WHERE CAROUSEL_ID='{rCSID}' AND CELL_ID='{rCEID}'";
			Startup.Local_SQLServer.NonQuery(sql);
        }
		public void Update_Group_Authority(string UserGroup,string UserGroup_Authority,string UserGroup_SystemSetting)
        {
            if (UserGroup == "Administrator")
            {
				UserGroup_Authority = "Admin";
				UserGroup_SystemSetting = "Admin";
            }
			sql = $"UPDATE [ACCOUNT] SET AUTHORITY_TYPE='{UserGroup_Authority}',USER_SYSTEMSETTING='{UserGroup_SystemSetting}' WHERE USER_GROUP='{UserGroup}'";
			Startup.Local_SQLServer.NonQuery(sql, true);
        }
		public void Update_CPC_Status(string status)
        {
			sql = $"UPDATE [WEBSERVICE_CONNECTION] SET STATUS='{status}' WHERE NAME='CPC'";
			Startup.Local_SQLServer.NonQuery(sql,true);
        }
		public string Update_ShowCSID(string rSCSID,string rCSID)
        {
			sql = string.Empty;
			DataTable dt = Startup.Local_SQLServer.SelectDB("*","[CAROUSEL_TRANSFER]",$"WHERE CAROUSEL_ID='{rCSID}' AND SHOW_CAROUSEL_ID='{rSCSID}'");
			if (dt.Rows.Count > 0)
				return "true";

			sql = $"UPDATE [CAROUSEL_TRANSFER] SET ";
			sql += $"SHOW_CAROUSEL_ID='{rSCSID}' ";
			sql += $"WHERE CAROUSEL_ID='{rCSID}' ;";
			Startup.Local_SQLServer.NonQuery(sql, true);
			return "true";
        }
		public string Update_ShowCEID_ForCarousel(string rSCEID_Front,string rCSID)
        {
			sql = string.Empty;
			if (rSCEID_Front == "")
				return "true";
			string sql_temp = $"SELECT CELL_ID FROM [CAROUSEL_TRANSFER] WHERE CAROUSEL_ID='{rCSID}'";
			DataTable dt_temp = new DataTable();
			Startup.Local_SQLServer.Query(sql_temp, ref dt_temp);
			foreach(DataRow dr_temp in dt_temp.Rows)
			{
				sql = $"UPDATE [CAROUSEL_TRANSFER] SET ";
				sql += $"SHOW_CELL_ID='{rSCEID_Front+dr_temp["CELL_ID"].ToString().Trim()}' ";
				sql += $"WHERE CAROUSEL_ID='{rCSID}' AND CELL_ID='{dr_temp["CELL_ID"].ToString().Trim()}' ;";
				Startup.Local_SQLServer.NonQuery(sql);
            }
			return "true";
		}
		public string Update_ShowCEID(string rSCEID,string rCEID)
        {
			sql = string.Empty;
			sql = $"UPDATE [CAROUSEL_TRANSFER] SET ";
			sql += $"SHOW_CELL_ID='{rSCEID}' ";
			sql += $"WHERE CELL_ID='{rCEID}' ;";
			Startup.Local_SQLServer.NonQuery(sql, true);
			return "true";
		}
		public string Update_Task_Priority(string rCMD_ID,string rPriority)
        {
			DataTable dt = Startup.Local_SQLServer.SelectDB("*", "[TASK]", $"COMMAND_ID='{rCMD_ID}'");
            try
            {
				if (dt.Rows[0]["STATUS"].ToString().Trim() != "0")
					return "非待命中任務";
				else
				{
					sql = $"UPDATE [TASK] SET PRIORITY='{rPriority}' WHERE COMMAND_ID='{rCMD_ID}'";
					Startup.Local_SQLServer.NonQuery(sql);
					return "優先權更新成功";
				}
			}
			catch(Exception ex)
            {

            }
			return "優先權更新失敗";
        }
        #endregion Update
        #region Insert
		public string Insert_User_ForCSV(List<string> data)
        {
			sql = string.Empty;
			string toCheck = data[0].ToLower();
			string action_ = data[data.Count - 1];
			if (toCheck=="userid")
				return "Continue";
			if (action_ == "1")
			{
				DataTable dt_temp = Startup.Local_SQLServer.SelectDB("*", "[ACCOUNT]", $"USER_ID='{data[0]}'");
				if (dt_temp.Rows.Count == 0)
				{
					sql = $"INSERT INTO [ACCOUNT] VALUES ('{data[0]}','{data[1]}','{data[2]}','{data[3]}','{data[4]}','{data[5]}')";
					Startup.Local_SQLServer.NonQuery(sql);
					return "新增成功";
				}
				else
				{
					return "已存在該用戶";
				}
			}
			else if (action_ == "2")
			{
				sql = $"DELETE FROM [ACCOUNT] WHERE [USER_ID]='{data[0]}'";
				Startup.Local_SQLServer.NonQuery(sql);
				return "刪除成功";
			}
			else if (action_ == "3")
			{
				sql = $"UPDATE [ACCOUNT] SET USER_ID='{data[0]}',USER_NAME='{data[1]}',USER_PASS='{data[2]}',USER_GROUP='{data[3]}', AUTHORITY_TYPE='{data[4]}',USER_SYSTEMSETTING='{data[5]}' WHERE USER_ID='{data[0]}'";
				Startup.Local_SQLServer.NonQuery(sql);
				return "更新成功";
			}
			return "請檢查狀態欄是否有錯誤";
		}
        public string Insert_User(string UID,string UN,string UP,string UG)
        {
			sql = string.Empty;
			string UA = string.Empty;
			string USS = string.Empty;
			DataTable dt = Startup.Local_SQLServer.SelectDB("*", "[ACCOUNT]", $"[USER_ID]='{UID}'");
			if (dt.Rows.Count > 0)
			{
				sql = $"UPDATE [ACCOUNT] SET [USER_ID]='{UID}',[USER_NAME]='{UN}',[USER_PASS]='{UP}',[USER_GROUP]='{UG}'";
				Startup.Local_SQLServer.NonQuery(sql);
				return "修改成功";
			}
			sql = $"INSERT INTO [ACCOUNT] VALUES ('{UID}','{UN}','{UP}','{UG}','{UA}','{USS}')";
			Startup.Local_SQLServer.NonQuery(sql);
			return "新增成功";
        }
		public string Insert_Check(string rNow, List<string> rT, List<string> rDS, List<string> rWS, string user)
        {
			string T = string.Empty;
			string commandid = string.Empty;
			List<string> DS = new List<string>();
			List<string> WS = new List<string>();
			for (int i = 0; i < rT.Count; i++)
			{
				//rT[i] = rT[i].TrimStart('0');
				if (Int32.Parse(rT[i]) < 10) rT[i] = "0" + rT[i];
				if (Int32.Parse(rT[i]) == 0) rT[i] = "0" + rT[i];
				T+=(rT[i]);
			}
			for (int i = 0; i < rDS.Count; i++)
			{
				//rDS[i] = rDS[i].TrimStart('0');
				if (Int32.Parse(rDS[i]) < 10) rDS[i] = "0" + rDS[i];
				if (Int32.Parse(rDS[i]) == 0) rDS[i] = "0" + rDS[i];
				DS.Add(rDS[i]);
			}
			for (int i = 0; i < rWS.Count; i++)
			{
				//rWS[i] = rWS[i].TrimStart('0');
				if (i != 0)
				{
					if (Int32.Parse(rWS[i]) < 10) rWS[i] = "0" + rWS[i];
					if (Int32.Parse(rWS[i]) == 0) rWS[i] = "0" + rWS[i];
				}
				WS.Add(rWS[i]);
			}
			sql = string.Empty;
			if (DS.Count != 0 || WS.Count != 0)
			{
				sql = "SELECT MAX([SCHEDULE_INDEX]) AS SCHEDULE_INDEX FROM [CAROUSEL_CHECK_SCHEDULE]";
				string scheduleindex = "0";
				string period = string.Empty;
				Startup.Local_SQLServer.Query(sql, ref Result);
				foreach (DataRow dr in Result.Rows)
				{
					scheduleindex = dr["SCHEDULE_INDEX"].ToString().Trim();
				}
				scheduleindex = (Int32.Parse(scheduleindex) + 1).ToString();
				if (DS.Count == 0)
				{
					period = "WEEK";
					sql = $"INSERT INTO [CAROUSEL_CHECK_SCHEDULE] VALUES('{scheduleindex}','{period}','{WS[0]}','{WS[1] + WS[2]}','','{user}') ";
					Startup.Local_SQLServer.NonQuery(sql);

				}
				else
				{
					period = "MONTH";
					sql = $"INSERT INTO [CAROUSEL_CHECK_SCHEDULE] VALUES('{scheduleindex}','{period}','NULL','NULL','{DS[0] + DS[1] + DS[2]}','{user}') ";
					Startup.Local_SQLServer.NonQuery(sql);
				}


			}
            if (T!= "" || rNow != "")
            {
				string Time = string.Empty;
				if (T == "")
					Time = rNow;
				else
					Time = T;
				commandid = DateTime.Now.ToString("yyyyMMddHHmmssfff");
				sql = $"INSERT INTO [CAROUSEL_CHECK_LIST] VALUES('{commandid}','{Time}','{user}','','','')";
				Startup.Local_SQLServer.NonQuery(sql);
			}
			return commandid;
		}
		public void Insert_Check_List_Detail(string rCSID, string rCEID, string rBOX_ID,string rBatch_No,string rSoteria,string rCustomer,string commandid)
		{
			rCSID = CSID_Transfer(rCSID);
			rCEID = CEID_Transfer(rCEID);
			sql = $"INSERT INTO [CAROUSEL_CHECK_LIST_DETAIL] VALUES ('{commandid}','{rCSID}','{rCEID}','{rBatch_No}','{rBOX_ID}','','{rSoteria}','{rCustomer}','')";
			Startup.Local_SQLServer.NonQuery(sql);
		}
		public void Insert_Check_Schedule_Detail(string rCSID,string rCEID,string rBOX_ID)
		{
			rCSID = CSID_Transfer(rCSID);
			rCEID = CEID_Transfer(rCEID);
			sql = "SELECT MAX([SCHEDULE_INDEX]) AS SCHEDULE_INDEX FROM [CAROUSEL_CHECK_SCHEDULE]";
			string scheduleindex = "0";
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach (DataRow dr in Result.Rows)
			{
				scheduleindex = dr["SCHEDULE_INDEX"].ToString().Trim();
			}
			scheduleindex = (Int32.Parse(scheduleindex) + 1).ToString();
			sql = $"INSERT INTO [CAROUSEL_CHECK_SCHEDULE_DETAIL] VALUES ('{scheduleindex}','{rCSID}','{rCEID}')";
			Startup.Local_SQLServer.NonQuery(sql);
		}
		public void Insert_Stock_Out_Task(string BN,string BOX,string NO,string CUS,string STR)
        {
			sql = string.Empty;
			sql = "SELECT replace(convert(varchar, getdate(),111),'/','') + replace(convert(varchar, getdate(),114),':','') AS T, MAX(PRIORITY) AS P FROM [TASK]";
			string priority=string.Empty;
			string commandid = string.Empty;
			Startup.Local_SQLServer.Query(sql, ref Result);
			foreach(DataRow dr in Result.Rows)
            {
				priority = dr["P"].ToString().Trim();
				priority = (Int32.Parse(priority) + 1).ToString();
				commandid = dr["T"].ToString().Trim();
			}
			sql = $"INSERT INTO [TASK] VALUES('{BOX}','{BN}','','','LIFTER_B','','0','3','OUT','1','0','{NO}','{priority}','{commandid}','','{STR}','{CUS}','{commandid}','','2','STRONG')";
			Startup.Local_SQLServer.NonQuery(sql);
        }
        #endregion Insert
    }
}