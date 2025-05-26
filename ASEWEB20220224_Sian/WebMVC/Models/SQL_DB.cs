using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ASEWEB.Models;
//using Strong;
namespace ASEWEB.Models
{
	public class SQL_DB
	{
		public string test1 = "true";
		//private LogWriter logWriter;
		private SqlConnection sqlConn;
		private SqlConnectionStringBuilder connStringBuilder;
		public SQL_Parameter SQLPara;
		public SQL_DB(SQL_Parameter connPara)
		{
			SQLPara = connPara;
			//logWriter = new LogWriter(Environment.CurrentDirectory + @"\LogFile\SQL", "SQL", 500 * 1000);
			connStringBuilder = new SqlConnectionStringBuilder();
			sqlConn = new SqlConnection();
			//ConnectToDB();
		}

		public bool ConnectToDB()
		{
			bool Result = false;
			try
			{
				//初始化SQL連接參數
				connStringBuilder.DataSource = SQLPara.DataSource;
				connStringBuilder.InitialCatalog = SQLPara.InitialCatalog;
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
				sqlConn.ConnectionString = connStringBuilder.ToString();

				//進行重新連線
				if (sqlConn.State != ConnectionState.Open) sqlConn.Close();
				sqlConn.Open();
				Result = true;
			}
			catch (Exception ex)
			{
				//LogExcept.LogException(ex);
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
				//if (logSkip_ == false)
					//logWriter.AddString("SQL: " + SqlStr_);
			}
			catch (Exception ex)
			{
				Result = false;
				//logWriter.AddString("SQL Exception: " + SqlStr_ + "\r\n" + ex.ToString());
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
			SqlCommand sqlCmd = new SqlCommand(SqlStr_, sqlConn);
			try
			{
				if (sqlConn.State != ConnectionState.Open) sqlConn.Open();
				if (sqlConn.State != ConnectionState.Open) return 0;

				AffectedCount = sqlCmd.ExecuteNonQuery();
				//if (logSkip_ == false)
					//logWriter.AddString("SQL: " + SqlStr_);
			}
			catch (Exception ex)
			{
				AffectedCount = 0;
				//logWriter.AddString("SQL Exception: " + SqlStr_ + "\r\n" + ex.ToString());
			}
			finally
			{
				sqlCmd?.Cancel();
				sqlConn?.Close();
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
	public class SQL_Parameter
	{
		//Data Source = LAPTOP - 51PK35Q2\SQLEXPRESS;Initial Catalog = ASE; Persist Security Info=True;User ID = strong; Password=5999011
		public string DataSource { get; set; }
		public string InitialCatalog { get; set; }
		public string UserID { get; set; }
		public string Password { get; set; }
	}
}