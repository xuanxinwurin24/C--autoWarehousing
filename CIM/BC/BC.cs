using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows;
using System.Text;
using System.Data;
using System.Linq;
using System.Net;
using System.IO;
using System.Windows.Threading;
using System.Timers;
using System.Threading;
using System.Windows.Documents;
using System.Collections;
using System.Runtime.CompilerServices;

namespace CIM.BC
{
	public class BC : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		void NotifyPropertyChanged([CallerMemberName] string propertyName_ = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName_));
		}
		public List<cCarousel> Carousels;

		public enum StockInMessage
		{
			BINDING_LOT_NOT_FOUND,
			STK_NO_ENOUGH_SPACE,
			BUNDLE_ID_READ_FAILED,
			BINDING_LOT_STATUS_AT_PAUSE,
			STOCK_IN_LOT_COUNT_OVER_SETTING,
			STK_NO_MORE_SPACE,
			BUNDLE_ALREADY_EXIST
		}
		public enum eCarousel_Check_Task_STATUS
        {
			eWAIT = 0, eSendToSTK, eAction, eFINISH
        };

		#region Property

		string ID = "BC";
		public BcPara BcPara;
		public LogWriter LogFile = new LogWriter();
		public Stocker STK;
		public ShuttleCar ShuttleCar1, ShuttleCar2;
		public TaskCtrl TaskCtrl;

		public DataTable dt_Carousel_Trans;
		public MemGroup wAux;

		private ThreadTimer Carousel_Check_CycleT;

		private string webservice_mode;
		public string WEBSERVICE_MODE
        {
			get { return webservice_mode; }
			set
            {
				if (value == webservice_mode) return;
				webservice_mode = value;
				NotifyPropertyChanged();
				string sSQL = $"UPDATE [WEBSERVICE_CONNECTION] SET STATUS = '{webservice_mode}' WHERE NAME = 'CPC'";
				App.Local_SQLServer.NonQuery(sSQL);
			}
        }
		#endregion Property

		#region INIT
		public BC()
		{
			try
			{
				LogFile.PathName = App.sSysDir + @"\LogFile\BC\BcEq";
				LogFile.MaxSize = 500 * 1000;
				LogFile.sHead = ID;

				BcPara = Common.DeserializeXMLFileToObject<BcPara>(App.sSysDir + @"\Ini\BcSetting.xml");

				STK = new Stocker();
				ShuttleCar1 = new ShuttleCar(1);
				//ShuttleCar2 = new ShuttleCar(2);
				TaskCtrl = new TaskCtrl();

				Carousels = new List<cCarousel>();
				dt_Carousel_Trans = App.Local_SQLServer.SelectDB("*", "[CAROUSEL_TRANSFER]", "");
				wAux = App.SimuDevice.AddMemGroup_CSVStream(nameof(BC), nameof(wAux), LoadStreamResouce("CIM.BC.Memory.wAux.csv"), MemBank.eBank.Wreg, 0, false);

				Carousel_Check_CycleT = App.MainThread.TimerCreate("Carousel_Check_CycleT", 60000, Carousel_Check_CycleT_Event, ThreadTimer.eType.Hold);
				Carousel_Check_CycleT.Enable(true, 5000); //程式開啟後五秒後觸發
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
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

		~BC()
		{
		}

		public void Initial()
		{
			uint Node = 1;
			App.Alarm.UnitNameRegister(Node, ID);
			App.Alarm.UnitNameRegister(Node + 2, "PLC");
			STK.Initial();
			Carousels.Clear();
			DataTable Result = new DataTable();
			DataTable dt_WebConnect = App.Local_SQLServer.SelectDB("[STATUS]", "[WEBSERVICE_CONNECTION]", "NAME = 'CPC'");
			WEBSERVICE_MODE = dt_WebConnect.Rows[0]["STATUS"].ToString().Trim();

			string sql = $"SELECT [CAROUSEL_ID],[STATUS],[STORE_STATUS],[N2_STATUS],[TEMPERATER],[TEMPERATER_STATUS],[HUMIDITY],[HUMIDITY_STATUS],[COMMAND_ID],[CHECK_STATUS]" +
			$" FROM [CAROUSEL_STATUS]";
			App.STK_SQLServer.Query(sql, ref Result);
			wAux["isOnlyDemo"].BinValue = 0;//No Stocker Demo=1,Stocker Demo=0;
			string scellID = string.Empty;
			foreach (DataRow dr in Result.Rows)
			{
				string carouselid = dr["CAROUSEL_ID"].ToString().Trim();
				string cmdid = dr["COMMAND_ID"].ToString().Trim();
				int status, store_status, n2_status, temp_status, humi_status, check_status;
				int.TryParse(dr["STATUS"].ToString(), out status);
				int.TryParse(dr["STORE_STATUS"].ToString(), out store_status);
				int.TryParse(dr["N2_STATUS"].ToString(), out n2_status);
				int.TryParse(dr["TEMPERATER_STATUS"].ToString(), out temp_status);
				int.TryParse(dr["HUMIDITY_STATUS"].ToString(), out humi_status);
				int.TryParse(dr["CHECK_STATUS"].ToString(), out check_status);
				double temperature, humidity;
				double.TryParse(dr["TEMPERATER"].ToString(), out temperature);
				double.TryParse(dr["HUMIDITY"].ToString(), out humidity);
				App.Bc.Transfer_CarouselCell_to_ViewID(ref carouselid, ref scellID); //轉換成SHOW的ID
				Carousels.Add(new cCarousel
				{
					Carousel_ID = carouselid,
					Status = status,
					Store_Status = store_status,
					N2_Status = n2_status,
					Temperature = temperature,
					Temperature_Status = temp_status,
					Humidity = humidity,
					Humidity_Status = humi_status,
					Command_ID = cmdid,
					Check_Status = check_status
				});
			}
		}

		#endregion INIT 

		#region AUX 

		private int Idx_Event(TagItem Item_, ushort[] New_, ushort[] Old_, ushort[] Curn_)
		{
			CommonMethods.MGLog(LogFile, Item_);
			return 0;
		}

		public void IndexAdd1(TagItem Item_, bool bTEnable_ = false)
		{
			try
			{
				uint val = Item_.BinValue;
				Item_.SyncBinValue = (ushort)(val >= 0xFFFF ? 1 : val + 1);
				if (bTEnable_ == false) return;
				ThreadTimer timer = (ThreadTimer)(Item_.Tag);
				if (timer == null) return;
				timer.Enable(true);            //for time out
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public void Reply_OnOff(TagItem Item_, bool On_, bool TimerEnable_ = false)
		{
			try
			{
				if (On_ == true)
				{
					Item_.SyncBinValue = 1;
					if (TimerEnable_ == false) return;
					ThreadTimer timer = (ThreadTimer)(Item_.Tag);
					if (timer == null) return;
					//timer.Enable(true);     //for time out
				}
				else
				{
					Item_.BinValue = 0;
					ThreadTimer timer = (ThreadTimer)(Item_.Tag);
					if (timer == null) return;
					timer.Enable(false);    //for time out
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}


		#endregion AUX

		#region Event
		#endregion Event

		#region Function

		public void TimerChange()
		{
		}

		public string GetNewCOMMAND_ID()
		{
			DataTable dt = App.Local_SQLServer.SelectDB("COMMAND_ID", "TASK", "WHERE [COMMAND_ID] < '10000'");
			List<int> CommandIDList = new List<int>();
			foreach (DataRow dr in dt.Rows)
			{
				CommandIDList.Add(dr["COMMAND_ID"].ToString().ToIntDef());
			}
			int iMax = CommandIDList.Max();
			if (iMax >= 10000)
				iMax = 9999;
			int iNewMax = iMax + 1 >= 10000 ? 1 : iMax + 1;
			while (CommandIDList.Contains(iNewMax) && iNewMax != iMax)
			{
				iNewMax++;
			}
			return iNewMax.ToString("0000");
		}

		public int GetCarousel_LineNo(string sCarousel_ID_)
		{
			DataTable dt = App.STK_SQLServer.SelectDB("LINE_NO", "CAROUSEL_STATUS", $"[CAROUSEL_ID] = '{sCarousel_ID_}'");
			return (dt.Rows.Count == 0) ? 0 : dt.Rows[0]["LINE_NO"].ToString().ToIntDef(0);
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

		public void Transferto_ViewID_CarouselCell(ref string sCarouselID_, ref string sCellID_) //丟進來顯示的名稱，輸出原本名稱
		{
			string sCondition = $"SHOW_CAROUSEL_ID = '{sCarouselID_}'";
			if (!string.IsNullOrEmpty(sCellID_))
			{
				sCondition += $" AND SHOW_CELL_ID = '{sCellID_}'";
			}
			DataRow[] dr = dt_Carousel_Trans.Select(sCondition);
			if (dr.Length > 0)
			{
				sCarouselID_ = dr[0]["CAROUSEL_ID"].ToString().Trim();
				if (!string.IsNullOrEmpty(sCellID_))
					sCellID_ = dr[0]["CELL_ID"].ToString().Trim();
			}
		}
		#endregion Function

		#region WaferBank WebService
		public bool isOnline()
        {
			return WEBSERVICE_MODE == "ONLINE";
        }
		public void Check_WebReport()
        {
			if (!isOnline())
				return;
			//20220223 Sian Edited to no check for batch but use CardID Webservice
			DataTable dt = App.Local_SQLServer.SelectDB("*", "[WEBSERVICE_SENDLIST]", "[STATUS] = 0 AND [ConnectMode]='ONLINE'");
			string sSQL = string.Empty;
			foreach (DataRow dr in dt.Rows)
			{
				switch (dr["HandlerId"].ToString().Trim())
                {
					case "BinInReq": //入庫/調儲/一般出庫-庫存檢查
                        #region BinInReq
                        DataTable dt_BatchList = App.Local_SQLServer.SelectDB("*", "[WEBSERVICE_BATCHLIST]", $"[BatchListNo] = {dr["BatchListNo"]}");
						List< ASE_WebService.Batch> BatchTemp = new List<ASE_WebService.Batch>();

						foreach (DataRow dr_Batch in dt_BatchList.Rows)
						{
							ASE_WebService.Batch EachBatch = new ASE_WebService.Batch()
							{
								Rtno = dr_Batch["Rtno"].ToString().Trim()
							};
							BatchTemp.Add(EachBatch);
						}

						ASE_WebService.Batch[] BatchList = BatchTemp.ToArray();

						ASE_WebService.InParam inParam = new ASE_WebService.InParam
						{
							HandlerId = ASE_WebService.HandlerType.BinInReq,
							MissionID = dr["MissionID"].ToString().Trim(),
							EventTime = dr["EventTime"].ToString().Trim(),
							UserId = dr["UserId"].ToString().Trim(),
							BatchList = BatchList,
							ConnectMode = WEBSERVICE_MODE
						};

						MainWindow.ucUILog.frmWebServiceLog.Log("Send : " + JSON.Serialize<ASE_WebService.InParam>(inParam));
						var task = WebService.CallWebService(inParam);
						sSQL = $"UPDATE [WEBSERVICE_SENDLIST] SET STATUS = 1 WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //更新此筆命令狀態為已送出
						App.Local_SQLServer.NonQuery(sSQL);
						task.ContinueWith(delegate
						{
							ASE_WebService.OutParam outParam = null;

							try
							{
								outParam = task.Result;
							}
							catch { }

							if (outParam == null)
							{
								App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log($"Received NULL , Curn Retry Times = {dr["RETRY_COUNT"].ToString().ToIntDef(0)}"));
								if (dr["RETRY_COUNT"].ToString().ToIntDef(0) < App.SysPara.iWebService_MaxRetryCount)
								{
									sSQL = $"UPDATE [WEBSERVICE_SENDLIST] SET STATUS = 0, [RETRY_COUNT] = '{dr["RETRY_COUNT"].ToString().ToIntDef(0) + 1}' WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //此命令未收到正確回復，重送且重送次數+1
									App.Local_SQLServer.NonQuery(sSQL);
								}
								else
                                {
									sSQL = $"DELETE [WEBSERVICE_BATCHLIST] WHERE [BatchListNo] = '{dr["BatchListNo"].ToString().Trim()}'";
									App.Local_SQLServer.NonQuery(sSQL);

									sSQL = $"DELETE [WEBSERVICE_SENDLIST] WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'";
									App.Local_SQLServer.NonQuery(sSQL);

									DataTable dt_HSReq = App.Local_SQLServer.SelectDB("HS_NAME", "[HS]", $"ACK = 2 AND [REQDATA] = '{dr["HS_DATA"].ToString().Trim()}'");
									if (dt_HSReq.Rows.Count > 0)
									{
										App.Bc.TaskCtrl.ChangeTargetCarousel(dr["MissionID"].ToString().Trim(), "Error"); //重新指向到暫存儲格
										App.Bc.TaskCtrl.LIFTER_SQLHS.Ack(dt_HSReq.Rows[0]["HS_NAME"].ToString().Trim(), "1", "NG,WebService No Reply");
									}
								}
							}
							else
							{
								App.Current.Dispatcher.Invoke(() =>  MainWindow.ucUILog.frmWebServiceLog.Log("Received : " + JSON.Serialize<ASE_WebService.OutParam>(outParam)));
								DataTable dt_HSReq = App.Local_SQLServer.SelectDB("HS_NAME", "[HS]", $"ACK = 2 AND [REQDATA] = '{dr["HS_DATA"].ToString().Trim()}'");
								bool bSecret = false;
								if (outParam.ReturnCode == "Y") //成功
								{
									foreach (ASE_WebService.Batch b in outParam.BatchList)
									{
										sSQL = $"UPDATE [BATCH_LIST] SET " +
											   $"[CUSTOMER_ID] = '{b.Customer}', " +
											   $"[SOTERIA] = '{b.SecurityFlag}', " +
											   $"[ENGINEER_FLAG] = '{b.EnginnerFlag}', " +
											   $"[BATCH_NO_RESULT] = '{b.BatchResult}', " +
											   $"[BATCH_NO_MSG] = '{b.BatchMsg}', " +
											   $"[CURN_BIN] = '{b.FromBin}' " +
											   $"[END_TIME] ='{DateTime.Now.ToString("yyyyMMddHHmmssfff")} '"+
											   $"WHERE BATCH_NO = '{b.Rtno}' ";
										App.Local_SQLServer.NonQuery(sSQL);
										if (b.SecurityFlag == "Y")
											bSecret = true;
									}

									if (dt_HSReq.Rows.Count > 0)
									{
										App.Bc.TaskCtrl.LIFTER_SQLHS.Ack(dt_HSReq.Rows[0]["HS_NAME"].ToString().Trim(), "1", "OK");
										if (bSecret) //是機密入庫
											App.Bc.TaskCtrl.ChangeTargetCarousel(dr["MissionID"].ToString().Trim(), "Secret"); //重新指向到機密儲格
									}
								}
								else
								{
									App.Alarm.Set(1, uint.Parse(DateTime.Now.ToString("HHmmssfff")), true, $"Web Service BinInReq ReturnCode = Failed ! ");
									if (dt_HSReq.Rows.Count > 0)
									{
										App.Bc.TaskCtrl.ChangeTargetCarousel(dr["MissionID"].ToString().Trim(), "Error"); //重新指向到暫存儲格
										App.Bc.TaskCtrl.LIFTER_SQLHS.Ack(dt_HSReq.Rows[0]["HS_NAME"].ToString().Trim(), "1", "NG," + outParam.ReturnMsg);
									}
								}

								sSQL = $"DELETE [WEBSERVICE_BATCHLIST] WHERE [BatchListNo] = '{dr["BatchListNo"].ToString().Trim()}'";
								App.Local_SQLServer.NonQuery(sSQL);

								sSQL = $"DELETE [WEBSERVICE_SENDLIST] WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'";
								App.Local_SQLServer.NonQuery(sSQL);
							}
						});
						break;
					#endregion BinInReq
					case "BinInWork": //入庫完成/調儲完成
						#region BinInWork
						dt_BatchList = App.Local_SQLServer.SelectDB("*", "[WEBSERVICE_BATCHLIST]", $"[BatchListNo] = {dr["BatchListNo"]}");

						BatchTemp = new List<ASE_WebService.Batch>();
						foreach (DataRow dr_Batch in dt_BatchList.Rows)
						{
							ASE_WebService.Batch EachBatch = new ASE_WebService.Batch()
							{
								Rtno = dr_Batch["Rtno"].ToString().Trim(),
								FromBin = dr_Batch["FromBin"].ToString().Trim(),
								ToBin = dr_Batch["ToBin"].ToString().Trim()
							};
							BatchTemp.Add(EachBatch);
						}
						BatchList = BatchTemp.ToArray();

						inParam = new ASE_WebService.InParam
						{
							HandlerId = ASE_WebService.HandlerType.BinInWork,
							MissionID = dr["MissionID"].ToString().Trim(),
							EventTime = dr["EventTime"].ToString().Trim(),
							UserId = dr["UserId"].ToString().Trim(),
							BatchList = BatchList,
							ConnectMode = WEBSERVICE_MODE
						};

						MainWindow.ucUILog.frmWebServiceLog.Log("Send : " + JSON.Serialize<ASE_WebService.InParam>(inParam));
						task = WebService.CallWebService(inParam);
						sSQL = $"UPDATE [WEBSERVICE_SENDLIST] SET STATUS = 1 WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //更新此筆命令狀態為已送出
						App.Local_SQLServer.NonQuery(sSQL);
						task.ContinueWith(delegate
						{
							ASE_WebService.OutParam outParam = null;

							try
							{
								outParam = task.Result;
							}
							catch { }

							if (outParam == null)
							{
								App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log($"Received NULL , Curn Retry Times = {dr["RETRY_COUNT"].ToString().ToIntDef(0)}"));
								if (dr["RETRY_COUNT"].ToString().ToIntDef(0) < App.SysPara.iWebService_MaxRetryCount)
								{
									sSQL = $"UPDATE [WEBSERVICE_SENDLIST] SET STATUS = 0, [RETRY_COUNT] = '{dr["RETRY_COUNT"].ToString().ToIntDef(0) + 1}' WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //此命令未收到正確回復，重送且重送次數+1
									App.Local_SQLServer.NonQuery(sSQL);
								}
								else
								{
									sSQL = $"DELETE [WEBSERVICE_BATCHLIST] WHERE [BatchListNo] = '{dr["BatchListNo"].ToString().Trim()}'";
									App.Local_SQLServer.NonQuery(sSQL);

									sSQL = $"DELETE [WEBSERVICE_SENDLIST] WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'";
									App.Local_SQLServer.NonQuery(sSQL);
								}
							}
							else
							{
								App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log("Received : " + JSON.Serialize<ASE_WebService.OutParam>(outParam)));
								if (outParam.ReturnCode == "Y") //成功
								{
									foreach (ASE_WebService.Batch b in outParam.BatchList)
									{
										sSQL = $"UPDATE [BATCH_LIST] SET " +
											   $"[CUSTOMER_ID] = '{b.Customer}', " +
											   $"[SOTERIA] = '{b.SecurityFlag}', " +
											   $"[ENGINEER_FLAG] = '{b.EnginnerFlag}', " +
											   $"[BATCH_NO_RESULT] = '{b.BatchResult}', " +
											   $"[BATCH_NO_MSG] = '{b.BatchMsg}' " +
											   $"[END_TIME] ='{DateTime.Now.ToString("yyyyMMddHHmmssfff")} '"+
											   $"WHERE BATCH_NO = '{b.Rtno}' AND [CURN_BIN] = '{b.ToBin}'";
										App.Local_SQLServer.NonQuery(sSQL);
									}
								}
								else
									App.Alarm.Set(1, uint.Parse(DateTime.Now.ToString("HHmmssfff")), true, $"Web Service BinInWork ReturnCode = Failed ! ");

								sSQL = $"DELETE [WEBSERVICE_BATCHLIST] WHERE [BatchListNo] = '{dr["BatchListNo"].ToString().Trim()}'"; //刪除WEBSERVICE_BATCHLIST內的資料
								App.Local_SQLServer.NonQuery(sSQL);
								sSQL = $"DELETE [WEBSERVICE_SENDLIST] WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //刪除WEBSERVICE_SENDLIST內的資料
								App.Local_SQLServer.NonQuery(sSQL);
							}
							//收到 ASE 回覆
						});
						break;
					#endregion BinInWork
					case "BankOutNormalWork": //一般出庫完成
						#region BankOutNormalWork
						dt_BatchList = App.Local_SQLServer.SelectDB("*", "[WEBSERVICE_BATCHLIST]", $"[BatchListNo] = {dr["BatchListNo"]}");

						BatchTemp = new List<ASE_WebService.Batch>();
						foreach (DataRow dr_Batch in dt_BatchList.Rows)
						{
							ASE_WebService.Batch EachBatch = new ASE_WebService.Batch()
							{
								Rtno = dr_Batch["Rtno"].ToString().Trim(),
								FromBin = dr_Batch["FromBin"].ToString().Trim()
							};
							BatchTemp.Add(EachBatch);
						}
						BatchList = BatchTemp.ToArray();

						inParam = new ASE_WebService.InParam
						{
							HandlerId = ASE_WebService.HandlerType.BankOutNormalWork,
							MissionID = dr["MissionID"].ToString().Trim(),
							TagId = dr["TagId"].ToString().Trim(),
							EventTime = dr["EventTime"].ToString().Trim(),
							UserId = dr["UserId"].ToString().Trim(),
							BatchList = BatchList,
							ConnectMode = WEBSERVICE_MODE
						};

						MainWindow.ucUILog.frmWebServiceLog.Log("Send : " + JSON.Serialize<ASE_WebService.InParam>(inParam));
						task = WebService.CallWebService(inParam);
						sSQL = $"UPDATE [WEBSERVICE_SENDLIST] SET STATUS = 1 WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //更新此筆命令狀態為已送出
						App.Local_SQLServer.NonQuery(sSQL);
						task.ContinueWith(delegate
						{
							ASE_WebService.OutParam outParam = null;

							try
							{
								outParam = task.Result;
							}
							catch { }

							if (outParam == null)
							{
								App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log($"Received NULL , Curn Retry Times = {dr["RETRY_COUNT"].ToString().ToIntDef(0)}"));
								if (dr["RETRY_COUNT"].ToString().ToIntDef(0) < App.SysPara.iWebService_MaxRetryCount)
								{
									sSQL = $"UPDATE [WEBSERVICE_SENDLIST] SET STATUS = 0, [RETRY_COUNT] = '{dr["RETRY_COUNT"].ToString().ToIntDef(0) + 1}' WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //此命令未收到正確回復，重送且重送次數+1
									App.Local_SQLServer.NonQuery(sSQL);
								}
								else
								{
									sSQL = $"DELETE [WEBSERVICE_BATCHLIST] WHERE [BatchListNo] = '{dr["BatchListNo"].ToString().Trim()}'";
									App.Local_SQLServer.NonQuery(sSQL);

									sSQL = $"DELETE [WEBSERVICE_SENDLIST] WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'";
									App.Local_SQLServer.NonQuery(sSQL);
								}
							}
							else
							{
								App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log("Received : " + JSON.Serialize<ASE_WebService.OutParam>(outParam)));
								if (outParam.ReturnCode == "Y") //成功
								{
									foreach (ASE_WebService.Batch b in outParam.BatchList)
									{
										if (b.BatchResult == "Y") //出庫成功，刪除BATCH_LIST內的資料
										{
											sSQL = $"DELETE [BATCH_LIST] WHERE BATCH_NO = '{b.Rtno}' AND [CURN_BIN] = '{b.FromBin}'";
											App.Local_SQLServer.NonQuery(sSQL);
										}
										else if (b.BatchResult == "F") //出庫失敗，刪除BATCH_LIST內的資料，並同時更新TASK_HISTORY那一筆任務是NG
										{
											sSQL = $"UPDATE [TASK_HISTORY] SET STATUS = '{(int)TaskCtrl.eTASK_Flow.NG}', [NG_REASON] = 'BankOutNormalWork Reply NG = {b.BatchMsg}' " +
											$"WHERE COMMAND_ID = '{dr["MissionID"].ToString().Trim()}'";
											App.Local_SQLServer.NonQuery(sSQL);
										}
									}
								}
								else
									App.Alarm.Set(1, uint.Parse(DateTime.Now.ToString("HHmmssfff")), true, $"Web Service BankOutNormalWork ReturnCode = Failed ! ");

								sSQL = $"DELETE [WEBSERVICE_BATCHLIST] WHERE [BatchListNo] = '{dr["BatchListNo"].ToString().Trim()}'"; //刪除WEBSERVICE_BATCHLIST內的資料
								App.Local_SQLServer.NonQuery(sSQL);
								sSQL = $"DELETE [WEBSERVICE_SENDLIST] WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //刪除WEBSERVICE_SENDLIST內的資料
								App.Local_SQLServer.NonQuery(sSQL);
							}
							//收到 ASE 回覆
						});
						break;
					#endregion BankOutNormalWork
					case "OrderIssueReq": //出庫下架-詢問領料清單
						#region OrderIssueReq
						inParam = new ASE_WebService.InParam
						{
							HandlerId = ASE_WebService.HandlerType.OrderIssueReq,
							OrderIssue = dr["OrderIssue"].ToString().Trim(),
							EventTime = dr["EventTime"].ToString().Trim(),
							UserId = dr["UserId"].ToString().Trim(),
							ConnectMode = WEBSERVICE_MODE
						};

						DataTable dt_HS = App.Local_SQLServer.SelectDB("HS_NAME", "[HS]", $"ACK = 2 AND [REQDATA] = '{dr["OrderIssue"].ToString().Trim()},{dr["UserId"].ToString().Trim()}'");
						string sReqHS_Name = string.Empty;
						if (dt_HS.Rows.Count > 0)
						{
							sReqHS_Name = dt_HS.Rows[0]["HS_NAME"].ToString().Trim();
						}

						MainWindow.ucUILog.frmWebServiceLog.Log("Send : " + JSON.Serialize<ASE_WebService.InParam>(inParam));
						task = WebService.CallWebService(inParam);
						sSQL = $"UPDATE [WEBSERVICE_SENDLIST] SET STATUS = 1 WHERE [OrderIssue] = '{dr["OrderIssue"].ToString().Trim()}'"; //更新此筆命令狀態為已送出
						App.Local_SQLServer.NonQuery(sSQL);
						task.ContinueWith(delegate
						{
							ASE_WebService.OutParam outParam = null;

							try
							{
								outParam = task.Result;
							}
							catch { }

							if (outParam == null)
							{
								App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log($"Received NULL , Curn Retry Times = {dr["RETRY_COUNT"].ToString().ToIntDef(0)}"));
								if (dr["RETRY_COUNT"].ToString().ToIntDef(0) < App.SysPara.iWebService_MaxRetryCount)
								{
									sSQL = $"UPDATE [WEBSERVICE_SENDLIST] SET STATUS = 0, [RETRY_COUNT] = '{dr["RETRY_COUNT"].ToString().ToIntDef(0) + 1}' WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //此命令未收到正確回復，重送且重送次數+1
									App.Local_SQLServer.NonQuery(sSQL);
								}
								else
								{
									sSQL = $"DELETE [WEBSERVICE_SENDLIST] WHERE [OrderIssue] = '{dr["OrderIssue"].ToString().Trim()}'";
									App.Local_SQLServer.NonQuery(sSQL);
								}
							}
							else
							{
								App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log("Received : " + JSON.Serialize<ASE_WebService.OutParam>(outParam)));
								if (outParam.ReturnCode == "Y") //成功
								{
									string sAddBatchList = "INSERT INTO [ORDER_BATCHLIST] (BATCH_NO, ORDER_NO, SOTERIA, CUSTOMER_ID, ENGINEER_FLAG, CCLNOGROUP, PRCTR, BATCH_RESULT, BATCH_MSG, REQ_FROM) VALUES ";
									
									if (outParam.BatchList != null)
									{
										foreach (ASE_WebService.Batch b in outParam.BatchList)
										{
											sSQL = $"UPDATE [BATCH_LIST] SET " +
												   $"[CUSTOMER_ID] = '{b.Customer}', " +
												   $"[SOTERIA] = '{b.SecurityFlag}', " +
												   $"[ENGINEER_FLAG] = '{b.EnginnerFlag}', " +
												   $"[BATCH_NO_RESULT] = '{b.BatchResult}', " +
												   $"[BATCH_NO_MSG] = '{b.BatchMsg}' " +
												   $"[END_TIME] ='{DateTime.Now.ToString("yyyyMMddHHmmssfff")} '" +
												   $"WHERE BATCH_NO = '{b.Rtno}'";
											App.Local_SQLServer.NonQuery(sSQL);

											sAddBatchList += $"( '{b.Rtno}', '{dr["OrderIssue"].ToString().Trim()}', '{b.SecurityFlag}', '{b.Customer}', '{b.EnginnerFlag}', '{b.CclnoGroup}', '{b.Prctr}' , '{b.BatchResult}', '{b.BatchMsg}', '{sReqHS_Name}'),";

											string sUpdateBatchList_OrderNo = $"UPDATE [BATCH_LIST] SET [ORDER_NO] = '{dr["OrderIssue"].ToString().Trim()}' WHERE [BATCH_NO] = '{b.Rtno.Trim()}'";
											App.Local_SQLServer.NonQuery(sUpdateBatchList_OrderNo);
										}
										sAddBatchList = sAddBatchList.Remove(sAddBatchList.LastIndexOf(','), 1);
										App.Local_SQLServer.NonQuery(sAddBatchList);

										App.Bc.TaskCtrl.LIFTER_SQLHS.Ack(sReqHS_Name, "1", "OK");
									}
									else //Batch List is NULL
									{
										App.Bc.TaskCtrl.LIFTER_SQLHS.Ack(sReqHS_Name, "1", "NG,Batch List is NULL");
									}
								}
								else
								{
									App.Bc.TaskCtrl.LIFTER_SQLHS.Ack(sReqHS_Name, "1", $"NG,{outParam.ReturnMsg}");
									App.Alarm.Set(1, uint.Parse(DateTime.Now.ToString("HHmmssfff")), true, $"Web Service OrderIssueReq ReturnCode = Failed ! ");
								}
								sSQL = $"DELETE [WEBSERVICE_SENDLIST] WHERE [OrderIssue] = '{dr["OrderIssue"].ToString().Trim()}'"; //刪除WEBSERVICE_SENDLIST內的資料
								App.Local_SQLServer.NonQuery(sSQL);
							}
						});
						break;
					#endregion OrderIssueReq
					case "BankOutIssueWork": //出庫下架-發料出庫完成
						#region BankOutIssueWork
						dt_BatchList = App.Local_SQLServer.SelectDB("*", "[WEBSERVICE_BATCHLIST]", $"[BatchListNo] = {dr["BatchListNo"]}");

						BatchTemp = new List<ASE_WebService.Batch>();
						foreach (DataRow dr_Batch in dt_BatchList.Rows)
						{
							ASE_WebService.Batch EachBatch = new ASE_WebService.Batch()
							{
								Rtno = dr_Batch["Rtno"].ToString().Trim(),
								FromBin = dr_Batch["FromBin"].ToString().Trim()
							};
							BatchTemp.Add(EachBatch);
						}
						BatchList = BatchTemp.ToArray();

						inParam = new ASE_WebService.InParam
						{
							HandlerId = ASE_WebService.HandlerType.BankOutIssueWork,
							MissionID = dr["MissionID"].ToString().Trim(),
							OrderIssue = dr["OrderIssue"].ToString().Trim(),
							TagId = dr["TagId"].ToString().Trim(),
							EventTime = dr["EventTime"].ToString().Trim(),
							UserId = dr["UserId"].ToString().Trim(),
							BatchList = BatchList,
							ConnectMode = WEBSERVICE_MODE
						};

						MainWindow.ucUILog.frmWebServiceLog.Log("Send : " + JSON.Serialize<ASE_WebService.InParam>(inParam));
						task = WebService.CallWebService(inParam);
						sSQL = $"UPDATE [WEBSERVICE_SENDLIST] SET STATUS = 1 WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //更新此筆命令狀態為已送出
						App.Local_SQLServer.NonQuery(sSQL);
						task.ContinueWith(delegate
						{
							ASE_WebService.OutParam outParam = null;

							try
							{
								outParam = task.Result;
							}
							catch { }

							if (outParam == null)
							{
								App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log($"Received NULL , Curn Retry Times = {dr["RETRY_COUNT"].ToString().ToIntDef(0)}"));
								if (dr["RETRY_COUNT"].ToString().ToIntDef(0) < App.SysPara.iWebService_MaxRetryCount)
								{
									sSQL = $"UPDATE [WEBSERVICE_SENDLIST] SET STATUS = 0, [RETRY_COUNT] = '{dr["RETRY_COUNT"].ToString().ToIntDef(0) + 1}' WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //此命令未收到正確回復，重送且重送次數+1
									App.Local_SQLServer.NonQuery(sSQL);
								}
								else
								{
									sSQL = $"DELETE [WEBSERVICE_BATCHLIST] WHERE [BatchListNo] = '{dr["BatchListNo"].ToString().Trim()}'";
									App.Local_SQLServer.NonQuery(sSQL);

									sSQL = $"DELETE [WEBSERVICE_SENDLIST] WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'";
									App.Local_SQLServer.NonQuery(sSQL);
								}
							}
							else
							{
								App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log("Received : " + JSON.Serialize<ASE_WebService.OutParam>(outParam)));
								if (outParam.ReturnCode == "Y") //成功
								{
									foreach (ASE_WebService.Batch b in outParam.BatchList)
									{
										if (b.BatchResult == "Y") //出庫成功，刪除BATCH_LIST內的資料
										{
											sSQL = $"DELETE [BATCH_LIST] WHERE BATCH_NO = '{b.Rtno}' AND [CURN_BIN] = '{b.FromBin}'";
											App.Local_SQLServer.NonQuery(sSQL);
										}
										else if (b.BatchResult == "F") //出庫失敗，刪除BATCH_LIST內的資料，並同時更新TASK_HISTORY那一筆任務是NG
										{
											sSQL = $"DELETE [BATCH_LIST] WHERE BATCH_NO = '{b.Rtno}' AND [CURN_BIN] = '{b.FromBin}'";
											App.Local_SQLServer.NonQuery(sSQL);

											sSQL = $"UPDATE [TASK_HISTORY] SET STATUS = '{(int)TaskCtrl.eTASK_Flow.NG}', [NG_REASON] = 'BankOutNormalWork Reply NG = {b.BatchMsg}' " +
											$"WHERE COMMAND_ID = '{dr["MissionID"].ToString().Trim()}'";
											App.Local_SQLServer.NonQuery(sSQL);
										}
									}
								}
								else
									App.Alarm.Set(1, uint.Parse(DateTime.Now.ToString("HHmmssfff")), true, $"Web Service BankOutIssueWork ReturnCode = Failed ! ");

								sSQL = $"DELETE [WEBSERVICE_BATCHLIST] WHERE [BatchListNo] = '{dr["BatchListNo"].ToString().Trim()}'"; //刪除WEBSERVICE_BATCHLIST內的資料
								App.Local_SQLServer.NonQuery(sSQL);
								sSQL = $"DELETE [WEBSERVICE_SENDLIST] WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //刪除WEBSERVICE_SENDLIST內的資料
								App.Local_SQLServer.NonQuery(sSQL);
							}
						});
						break;
					#endregion BankOutIssueWork
					case "InventoryWork": //盤點完成
						#region InventoryWork
						dt_BatchList = App.Local_SQLServer.SelectDB("*", "[WEBSERVICE_BATCHLIST]", $"[BatchListNo] = {dr["BatchListNo"]}");

						BatchTemp = new List<ASE_WebService.Batch>();
						foreach (DataRow dr_Batch in dt_BatchList.Rows)
						{
							ASE_WebService.Batch EachBatch = new ASE_WebService.Batch()
							{
								LabelInfo = dr_Batch["LabelInfo"].ToString().Trim(),
								ToBin = dr_Batch["ToBin"].ToString().Trim()
							};
							BatchTemp.Add(EachBatch);
						}
						BatchList = BatchTemp.ToArray();

						inParam = new ASE_WebService.InParam
						{
							HandlerId = ASE_WebService.HandlerType.InventoryWork,
							MissionID = dr["MissionID"].ToString().Trim(),
							EventTime = dr["EventTime"].ToString().Trim(),
							UserId = dr["UserId"].ToString().Trim(),
							BatchList = BatchList,
							ConnectMode = WEBSERVICE_MODE
						};

						MainWindow.ucUILog.frmWebServiceLog.Log("Send : " + JSON.Serialize<ASE_WebService.InParam>(inParam));
						task = WebService.CallWebService(inParam);
						sSQL = $"UPDATE [WEBSERVICE_SENDLIST] SET STATUS = 1 WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //更新此筆命令狀態為已送出
						App.Local_SQLServer.NonQuery(sSQL);
						task.ContinueWith(delegate
						{
							ASE_WebService.OutParam outParam = null;

							try
							{
								outParam = task.Result;
							}
							catch { }

							if (outParam == null)
							{
								App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log($"Received NULL , Curn Retry Times = {dr["RETRY_COUNT"].ToString().ToIntDef(0)}"));
								if (dr["RETRY_COUNT"].ToString().ToIntDef(0) < App.SysPara.iWebService_MaxRetryCount)
								{
									sSQL = $"UPDATE [WEBSERVICE_SENDLIST] SET STATUS = 0, [RETRY_COUNT] = '{dr["RETRY_COUNT"].ToString().ToIntDef(0) + 1}' WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //此命令未收到正確回復，重送且重送次數+1
									App.Local_SQLServer.NonQuery(sSQL);
								}
								else
								{
									sSQL = $"DELETE [WEBSERVICE_BATCHLIST] WHERE [BatchListNo] = '{dr["BatchListNo"].ToString().Trim()}'";
									App.Local_SQLServer.NonQuery(sSQL);

									sSQL = $"DELETE [WEBSERVICE_SENDLIST] WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'";
									App.Local_SQLServer.NonQuery(sSQL);
								}
							}
							else
							{
								App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log("Received : " + JSON.Serialize<ASE_WebService.OutParam>(outParam)));
								if (outParam.ReturnCode == "Y") //成功
								{
									foreach (ASE_WebService.Batch b in outParam.BatchList)
									{
										sSQL = $"UPDATE [BATCH_LIST] SET " +
												$"[CUSTOMER_ID] = '{b.Customer}', " +
												$"[SOTERIA] = '{b.SecurityFlag}', " +
												$"[ENGINEER_FLAG] = '{b.EnginnerFlag}', " +
												$"[BATCH_NO_RESULT] = '{b.BatchResult}', " +
												$"[BATCH_NO_MSG] = '{b.BatchMsg}', " +
												$"[CCL_GROUP] = '{b.CclnoGroup}', " +
												$"[PRCTR] = '{b.Prctr}', " +
												$"[MATCH_RESULT] = '{b.MatchResult}' " +
												$"[END_TIME] ='{DateTime.Now.ToString("yyyyMMddHHmmssfff")} '" +
												$"WHERE BATCH_NO = '{b.Rtno}' AND [CURN_BIN] = '{b.FromBin}'";
										App.Local_SQLServer.NonQuery(sSQL);
										//sSQL=$"UPDATE [CHECK_HISTORY]"
									}
									
								}
								else
								{
									App.Alarm.Set(1, uint.Parse(DateTime.Now.ToString("HHmmssfff")), true, $"Web Service InventoryWork ReturnCode = Failed ! ");
								}
								sSQL = $"DELETE [WEBSERVICE_BATCHLIST] WHERE [BatchListNo] = '{dr["BatchListNo"].ToString().Trim()}'"; //刪除WEBSERVICE_BATCHLIST內的資料
								App.Local_SQLServer.NonQuery(sSQL);
								sSQL = $"DELETE [WEBSERVICE_SENDLIST] WHERE [MissionID] = '{dr["MissionID"].ToString().Trim()}'"; //刪除WEBSERVICE_SENDLIST內的資料
								App.Local_SQLServer.NonQuery(sSQL);
							}
						});
						break;
						#endregion InventoryWork
				}
			}
		}

		private int GetBatchListNo()
		{
			DataTable dt_SendList_Max = new DataTable();
			string sSQL = $"SELECT MAX (BatchListNo) [BATCHLISTNO_MAX] FROM [WEBSERVICE_BATCHLIST]";
			App.Local_SQLServer.Query(sSQL, ref dt_SendList_Max, true);
			int iNewBatchListNo = 1;
			if (dt_SendList_Max.Rows.Count > 0)
				iNewBatchListNo = dt_SendList_Max.Rows[0]["BATCHLISTNO_MAX"].ToString().ToIntDef(0) + 1;
			return iNewBatchListNo;
		}
		
		public void StockExistCheck_Request(string sBoxID_, string sHS_Data_) //入庫/調儲/一般出庫-庫存檢查
		{
			try
			{
				DataTable dt_Task = App.Local_SQLServer.SelectDB("*", "[TASK]", $" [BOX_ID] = '{sBoxID_}'");

				if (dt_Task.Rows.Count > 0)
				{
					//撈WEBSERVICE_BATCHLIST內，最大的BatchListNo + 1 避免重複
					int iNewBatchListNo = GetBatchListNo();

					//將所有抓到要上報的Batch 塞到WEBSERVICE_BATCHLIST內
					DataTable dt_Batchs = App.Local_SQLServer.SelectDB("*", "[BATCH_LIST]", $" [BOX_ID] = '{sBoxID_}'");
					string sSQL = $"INSERT INTO [WEBSERVICE_BATCHLIST] ([BatchListNo], [Rtno]) VALUES ";
					foreach (DataRow dr in dt_Batchs.Rows)
					{
						sSQL += $"({iNewBatchListNo},'{dr["BATCH_NO"].ToString().Trim()}'),";
					}
					sSQL = sSQL.Remove(sSQL.LastIndexOf(','), 1);
					App.Local_SQLServer.NonQuery(sSQL);

					//新增到WEBSERVICE_SENDLIST，由另外流程發送
					sSQL = $"INSERT INTO [WEBSERVICE_SENDLIST] ([STATUS],[HandlerId],[MissionID],[UserId],[EventTime],[ConnectMode],[BatchListNo],[HS_DATA]) VALUES " +
						$"(0, 'BinInReq', '{dt_Task.Rows[0]["COMMAND_ID"]}', '{dt_Task.Rows[0]["USER_ID"]}', '{DateTime.Now.ToString("yyyyMMddHHmmss")}', '{WEBSERVICE_MODE}', '{iNewBatchListNo}', '{sHS_Data_}')";
					App.Local_SQLServer.NonQuery(sSQL);
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public void StockInComp_Report(string sBoxID_) //入庫完成/調儲完成
		{
			try
			{
				DataTable dt_Task = App.Local_SQLServer.SelectDB("*", "[TASK]", $" [BOX_ID] = '{sBoxID_}'");

				if (dt_Task.Rows.Count > 0)
				{
					//撈WEBSERVICE_BATCHLIST內，最大的BatchListNo + 1 避免重複
					int iNewBatchListNo = GetBatchListNo();

					//將所有抓到要上報的Batch 塞到WEBSERVICE_BATCHLIST內
					DataTable dt_Batchs = App.Local_SQLServer.SelectDB("*", "[BATCH_LIST]", $" [BOX_ID] = '{sBoxID_}'");
					string sSQL = $"INSERT INTO [WEBSERVICE_BATCHLIST] ([BatchListNo], [Rtno], [FromBin], [ToBin]) VALUES ";
					string sFromBin, sToBin, sFromCarousel, sFromCellID;

					if (dt_Task.Rows[0]["SRC_POS"].ToString().Trim().IndexOf("LIFTER") != -1) //從LIFTER入庫
					{
						sFromBin = dt_Batchs.Rows[0]["CURN_BIN"].ToString().Trim();
					}
					else
					{
						sFromCarousel = dt_Task.Rows[0]["SRC_POS"].ToString().Trim();
						sFromCellID = dt_Task.Rows[0]["SRC_CELL_ID"].ToString().Trim();
						Transfer_CarouselCell_to_ViewID(ref sFromCarousel, ref sFromCellID);
						sFromBin = $"{sFromCarousel}-{sFromCellID}-{sBoxID_}";
					}

					sFromCarousel = dt_Task.Rows[0]["TAR_POS"].ToString().Trim();
					sFromCellID = dt_Task.Rows[0]["TAR_CELL_ID"].ToString().Trim();
					Transfer_CarouselCell_to_ViewID(ref sFromCarousel, ref sFromCellID);
					sToBin = $"{sFromCarousel}-{sFromCellID}-{sBoxID_}";

					foreach (DataRow dr in dt_Batchs.Rows)
					{
						sSQL += $"({iNewBatchListNo},'{dr["BATCH_NO"].ToString().Trim()}','{sFromBin}','{sToBin}'),";
					}
					sSQL = sSQL.Remove(sSQL.LastIndexOf(','), 1);
					App.Local_SQLServer.NonQuery(sSQL);

					//新增到WEBSERVICE_SENDLIST，由另外流程發送
					sSQL = $"INSERT INTO [WEBSERVICE_SENDLIST] ([STATUS],[HandlerId],[MissionID],[UserId],[EventTime],[ConnectMode],[BatchListNo]) VALUES " +
						$"(0, 'BinInWork', '{dt_Task.Rows[0]["COMMAND_ID"]}', '{dt_Task.Rows[0]["USER_ID"]}', '{DateTime.Now.ToString("yyyyMMddHHmmss")}', '{WEBSERVICE_MODE}', '{iNewBatchListNo}')";
					App.Local_SQLServer.NonQuery(sSQL);
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public void StockOutComp_Report(string sBoxID_) //出庫下架-一般出庫完成
		{
			try
			{
				DataTable dt_Task = App.Local_SQLServer.SelectDB("*", "[TASK]", $" [BOX_ID] = '{sBoxID_}'");

				if (dt_Task.Rows.Count > 0)
				{
					//撈WEBSERVICE_BATCHLIST內，最大的BatchListNo + 1 避免重複
					int iNewBatchListNo = GetBatchListNo();

					//將所有抓到要上報的Batch 塞到WEBSERVICE_BATCHLIST內
					DataTable dt_Batchs = App.Local_SQLServer.SelectDB("*", "[BATCH_LIST]", $" [BOX_ID] = '{sBoxID_}'");
					string sSQL = $"INSERT INTO [WEBSERVICE_BATCHLIST] ([BatchListNo], [Rtno], [FromBin]) VALUES ";

					string sFromCarousel = dt_Task.Rows[0]["SRC_POS"].ToString().Trim();
					string sFromCellID = dt_Task.Rows[0]["SRC_CELL_ID"].ToString().Trim();
					Transfer_CarouselCell_to_ViewID(ref sFromCarousel, ref sFromCellID);
					string sFromBin = $"{sFromCarousel}-{sFromCellID}-{sBoxID_}";

					foreach (DataRow dr in dt_Batchs.Rows)
					{
						sSQL += $"({iNewBatchListNo},'{dr["BATCH_NO"].ToString().Trim()}','{sFromBin}'),";
					}
					sSQL = sSQL.Remove(sSQL.LastIndexOf(','), 1);
					App.Local_SQLServer.NonQuery(sSQL);

					DataTable dt_Priority = App.Local_SQLServer.SelectDB("[VALUE]", "[SYSTEM_SETTING]", $"[NAME] = 'TagId'");
					string sTagId = "AutoID";
					if (dt_Priority.Rows.Count > 0)
						sTagId = dt_Priority.Rows[0]["VALUE"].ToString().Trim();

					//新增到WEBSERVICE_SENDLIST，由另外流程發送
					sSQL = $"INSERT INTO [WEBSERVICE_SENDLIST] ([STATUS],[HandlerId],[MissionID],[UserId],[EventTime],[ConnectMode],[BatchListNo],[TagId]) VALUES " +
						$"(0, 'BankOutNormalWork', '{dt_Task.Rows[0]["COMMAND_ID"]}', '{dt_Task.Rows[0]["USER_ID"]}', '{DateTime.Now.ToString("yyyyMMddHHmmss")}', '{WEBSERVICE_MODE}', '{iNewBatchListNo}', '{sTagId}')";
					App.Local_SQLServer.NonQuery(sSQL);
					//測試 無WebService詢問的出庫完整流程(有Stocker) 2022/01/21 Sian Edited
					//if (App.Bc.wAux["isOnlyDemo"].BinValue == 1)
					//{
						sSQL = $"DELETE [BATCH_LIST] WHERE [BOX_ID] = '{sBoxID_}'";
						App.Local_SQLServer.NonQuery(sSQL);
					//}
					//2022/01/21 Sian Edited
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public void OrderNo_Request(string sOrderNo_, string sUserID_) //詢問領料清單
		{
			try
			{
				//新增到WEBSERVICE_SENDLIST，由另外流程發送
				string sSQL = $"DELETE [ORDER_BATCHLIST]";
				App.Local_SQLServer.NonQuery(sSQL);
				
				sSQL = $"INSERT INTO [WEBSERVICE_SENDLIST] ([STATUS],[HandlerId],[OrderIssue],[UserId],[EventTime],[ConnectMode]) VALUES " +
					$"(0, 'OrderIssueReq', '{sOrderNo_}', '{sUserID_}', '{DateTime.Now.ToString("yyyyMMddHHmmss")}', '{WEBSERVICE_MODE}')";
				App.Local_SQLServer.NonQuery(sSQL);
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public void OrderStockOutComp_Report(string sBoxID_, string sOrderNo_) //出庫下架-發料出庫完成
		{
			try
			{
				DataTable dt_Task = App.Local_SQLServer.SelectDB("*", "[TASK]", $" [BOX_ID] = '{sBoxID_}'");

				if (dt_Task.Rows.Count > 0)
				{
					//撈WEBSERVICE_BATCHLIST內，最大的BatchListNo + 1 避免重複
					int iNewBatchListNo = GetBatchListNo();

					//將所有抓到要上報的Batch 塞到WEBSERVICE_BATCHLIST內
					DataTable dt_Batchs = App.Local_SQLServer.SelectDB("*", "[BATCH_LIST]", $" [BOX_ID] = '{sBoxID_}'");
					string sSQL = $"INSERT INTO [WEBSERVICE_BATCHLIST] ([BatchListNo], [Rtno], [FromBin]) VALUES ";

					string sFromCarousel = dt_Task.Rows[0]["SRC_POS"].ToString().Trim();
					string sFromCellID = dt_Task.Rows[0]["SRC_CELL_ID"].ToString().Trim();
					Transfer_CarouselCell_to_ViewID(ref sFromCarousel, ref sFromCellID);
					string sFromBin = $"{sFromCarousel}-{sFromCellID}-{sBoxID_}";

					foreach (DataRow dr in dt_Batchs.Rows)
					{
						sSQL += $"({iNewBatchListNo},'{dr["BATCH_NO"].ToString().Trim()}','{sFromBin}'),";
					}
					sSQL = sSQL.Remove(sSQL.LastIndexOf(','), 1);
					App.Local_SQLServer.NonQuery(sSQL);

					DataTable dt_Priority = App.Local_SQLServer.SelectDB("[VALUE]", "[SYSTEM_SETTING]", $"[NAME] = 'TagId'");
					string sTagId = "AutoID";
					if (dt_Priority.Rows.Count > 0)
						sTagId = dt_Priority.Rows[0]["VALUE"].ToString().Trim();

					//新增到WEBSERVICE_SENDLIST，由另外流程發送
					sSQL = $"INSERT INTO [WEBSERVICE_SENDLIST] ([STATUS],[HandlerId],[MissionID],[UserId],[EventTime],[ConnectMode],[BatchListNo],[TagId],[OrderIssue]) VALUES " +
						$"(0, 'BankOutIssueWork', '{dt_Task.Rows[0]["COMMAND_ID"]}', '{dt_Task.Rows[0]["USER_ID"]}', '{DateTime.Now.ToString("yyyyMMddHHmmss")}', '{WEBSERVICE_MODE}', '{iNewBatchListNo}', '{sTagId}', '{sOrderNo_}')";
					App.Local_SQLServer.NonQuery(sSQL);

					if (App.Bc.wAux["isOnlyDemo"].BinValue == 1)
					{
						sSQL = $"DELETE [BATCH_LIST] WHERE [BOX_ID] = '{sBoxID_}'";
						App.Local_SQLServer.NonQuery(sSQL);
					}
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public void InventoryComp_Report(string sCommandID_) //盤點完成
		{
			try
			{
				DataTable dt_Task = App.Local_SQLServer.SelectDB("*", "[CAROUSEL_CHECK_HISTORY]", $"[COMMAND_ID] = '{sCommandID_}'");

				if (dt_Task.Rows.Count > 0)
				{
					//撈WEBSERVICE_BATCHLIST內，最大的BatchListNo + 1 避免重複
					int iNewBatchListNo = GetBatchListNo();

					//將所有抓到要上報的Batch 塞到WEBSERVICE_BATCHLIST內
					DataTable dt_Batchs = new DataTable();

					string sSQL = $"SELECT check_history.*,batch_list.[CURN_BIN],batch_list.[LABELINFO] FROM [CAROUSEL_CHECK_HISTORY_DETAIL] check_history join [BATCH_LIST] batch_list on check_history.[BOX_ID] = batch_list.[BOX_ID] " +
						$"WHERE check_history.[COMMAND_ID] = '{sCommandID_}'";
					App.Local_SQLServer.Query(sSQL, ref dt_Batchs);


					sSQL = $"INSERT INTO [WEBSERVICE_BATCHLIST] ([BatchListNo], [LabelInfo], [ToBin]) VALUES ";
					foreach (DataRow dr in dt_Batchs.Rows)
					{
						sSQL += $"({iNewBatchListNo},'{dr["LABELINFO"].ToString().Trim()}', '{dr["CURN_BIN"].ToString().Trim()}'),";
					}
					sSQL = sSQL.Remove(sSQL.LastIndexOf(','), 1);
					App.Local_SQLServer.NonQuery(sSQL);

					//新增到WEBSERVICE_SENDLIST，由另外流程發送
					sSQL = $"INSERT INTO [WEBSERVICE_SENDLIST] ([STATUS],[HandlerId],[MissionID],[UserId],[EventTime],[ConnectMode],[BatchListNo]) VALUES " +
						$"(0, 'InventoryWork', '{dt_Task.Rows[0]["COMMAND_ID"]}', '{dt_Task.Rows[0]["USER_ID"]}', '{DateTime.Now.ToString("yyyyMMddHHmmss")}', '{WEBSERVICE_MODE}', '{iNewBatchListNo}')";
					App.Local_SQLServer.NonQuery(sSQL);
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public void UserIDRequest(string sReqLifter_, string sCard_ID_)
        {
			MainWindow.ucUILog.frmWebServiceLog.Log("Send GetCardInfo,  Card ID = " + sCard_ID_);
			var task = WebService.Call_UserID_WebService(sCard_ID_);

			task.ContinueWith(delegate
			{
				try
				{
					string str = string.Empty;
					try
					{
						str = task.Result;
						CARD_ID_RESPONSE card_id = JSON.DeSerialize<CARD_ID_RESPONSE>(task.Result);
						if (card_id.Msg == "Pass") //Card ID 要求成功
						{
							string sData = card_id.Result.Split(',')[1];
							string sUserID = sData.Split('|')[0];
							string sUserName = sData.Split('|')[1];
							App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log("Get = " + str));
							App.Bc.TaskCtrl.LIFTER_SQLHS.Ack(sReqLifter_, "1", $"OK,{sUserID},{sUserName}");
						}
						else //失敗
						{
							App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log("Get = " + str));
							App.Bc.TaskCtrl.LIFTER_SQLHS.Ack(sReqLifter_, "1", $"NG,,");
						}
					}
					catch (Exception Ex_) { LogWriter.LogException(Ex_.InnerException); }
					//if (task != null)
					//{

					//	App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log("Get = " + str));
					//	App.Bc.TaskCtrl.LIFTER_SQLHS.Ack(sReqLifter_, "1", $"NG,,");
					//}
					//else
					//{

					//	App.Current.Dispatcher.Invoke(() => MainWindow.ucUILog.frmWebServiceLog.Log("task is null"));
					//	App.Bc.TaskCtrl.LIFTER_SQLHS.Ack(sReqLifter_, "1", $"NG,,");
					//}


				}
				catch (Exception e_) 
				{
					LogWriter.LogException(e_);
					App.Bc.TaskCtrl.LIFTER_SQLHS.Ack(sReqLifter_, "1", $"NG,,");
				}

			});
		}

		#endregion WaferBank WebService

		#region Carousel_Check
		private int Carousel_Check_CycleT_Event(ThreadTimer Sender)
		{
			//檢查CAROUSEL_CHECK_SCHEDULE內有沒有要成立的盤點任務，一分鐘一次
			Carousel_Check_CycleT.Enable(false);
			DataTable dt = App.Local_SQLServer.SelectDB("*", "[CAROUSEL_CHECK_SCHEDULE]", "");
			string sSQL, sBatchNo, sBoxID, sGroupNo, sSoteria, sCustomerID;
			foreach (DataRow dr in dt.Rows)
            {
				string sPeriod = dr["PERIOD"].ToString().Trim();
				if (sPeriod == "WEEK") //週期是每個禮拜一次
                {
					DayOfWeek CheckDay_PerWeek = (DayOfWeek)dr["BY_WEEK_DAY"].ToString().ToIntDef(0); //禮拜日到六 對應0 ~ 6
					if (CheckDay_PerWeek == DateTime.Now.DayOfWeek) //今天星期X 就是那一天
                    {
						if (dr["BY_WEEK_TIME"].ToString().Trim() == DateTime.Now.ToString("HHmm")) //BY_WEEK_TIME HHmm , 時間相符
                        {
							DataTable dt_Detail = App.Local_SQLServer.SelectDB("*", "[CAROUSEL_CHECK_SCHEDULE_DETAIL]", $"SCHEDULE_INDEX = {dr["SCHEDULE_INDEX"].ToString().ToIntDef(0)}");
							if (dt_Detail.Rows.Count > 0)
                            {
								string sCOMMAND_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
								sSQL = $"INSERT INTO [CAROUSEL_CHECK_LIST_DETAIL] ([COMMAND_ID],[CAROUSEL_ID],[CELL_ID],[BATCH_NO],[BOX_ID],[GROUP_NO],[SOTERIA],[CUSTOMER_ID]) VALUES "; //新增此次任務有哪些儲格
								foreach (DataRow dr_Detail in dt_Detail.Rows)
								{
									//--從STK資料庫抓到Carousel & Cell對應的Batch，再找BATCH_LIST來填入CAROUSEL_CHECK_LIST_DETAIL
									DataTable dt_CELL_STATUS = App.STK_SQLServer.SelectDB("*", "[CELL_STATUS]", $"[CAROUSEL_ID] = '{dr_Detail["CAROUSEL_ID"].ToString().Trim()}' AND [CELL_ID] = '{dr_Detail["CELL_ID"].ToString().Trim()}'");
									if (dt_CELL_STATUS.Rows.Count > 0)
									{
										sBatchNo = dt_CELL_STATUS.Rows[0]["BATCH_NO"].ToString().Trim();
										sBoxID = dt_CELL_STATUS.Rows[0]["BOX_ID"].ToString().Trim();
										sGroupNo = dt_CELL_STATUS.Rows[0]["GROUP_NO"].ToString().Trim();
										sSoteria = dt_CELL_STATUS.Rows[0]["SOTERIA"].ToString().Trim();
										sCustomerID = dt_CELL_STATUS.Rows[0]["CUSTOMER_ID"].ToString().Trim();
									}
									else
									{
										sBatchNo = "";
										sBoxID = "";
										sGroupNo = "";
										sSoteria = "";
										sCustomerID = "";
									}

									sSQL += $"('{sCOMMAND_ID}', '{dr_Detail["CAROUSEL_ID"]}', '{dr_Detail["CELL_ID"]}', '{sBatchNo}', '{sBoxID}', '{sGroupNo}', '{sSoteria}', '{sCustomerID}'),";
								}
								sSQL = sSQL.Remove(sSQL.LastIndexOf(','), 1);
								App.Local_SQLServer.NonQuery(sSQL);

								sSQL = $"INSERT INTO [CAROUSEL_CHECK_LIST] ([COMMAND_ID],[USER_ID],[STATUS]) VALUES" + //新增此次任務
									$"('{sCOMMAND_ID}', '{dr["USER_ID"]}', {(int)eCarousel_Check_Task_STATUS.eWAIT})";
								App.Local_SQLServer.NonQuery(sSQL);
							}
						}
					}
				}
				else if (sPeriod == "MONTH") //週期是每個月一次
				{
					if (dr["BY_MONTH_DATETIME"].ToString().Trim() == DateTime.Now.ToString("ddHHmm")) //今天日期時間 是否與任務相符
					{
						DataTable dt_Detail = App.Local_SQLServer.SelectDB("*", "[CAROUSEL_CHECK_SCHEDULE_DETAIL]", $"SCHEDULE_INDEX = {dr["SCHEDULE_INDEX"].ToString().ToIntDef(0)}");
						if (dt_Detail.Rows.Count > 0)
						{
							string sCOMMAND_ID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
							sSQL = $"INSERT INTO [CAROUSEL_CHECK_LIST_DETAIL] ([COMMAND_ID],[CAROUSEL_ID],[CELL_ID],[BATCH_NO],[BOX_ID],[GROUP_NO],[SOTERIA],[CUSTOMER_ID]) VALUES "; //新增此次任務有哪些儲格
							foreach (DataRow dr_Detail in dt_Detail.Rows)
							{
								//--從STK資料庫抓到Carousel & Cell對應的Batch，再找BATCH_LIST來填入CAROUSEL_CHECK_LIST_DETAIL
								DataTable dt_CELL_STATUS = App.STK_SQLServer.SelectDB("*", "[CELL_STATUS]", $"[CAROUSEL_ID] = '{dr_Detail["CAROUSEL_ID"].ToString().Trim()}' AND [CELL_ID] = '{dr_Detail["CELL_ID"].ToString().Trim()}'");
								if (dt_CELL_STATUS.Rows.Count > 0)
                                {
									sBatchNo = dt_CELL_STATUS.Rows[0]["BATCH_NO"].ToString().Trim();
									sBoxID = dt_CELL_STATUS.Rows[0]["BOX_ID"].ToString().Trim();
									sGroupNo = dt_CELL_STATUS.Rows[0]["GROUP_NO"].ToString().Trim();
									sSoteria = dt_CELL_STATUS.Rows[0]["SOTERIA"].ToString().Trim();
									sCustomerID = dt_CELL_STATUS.Rows[0]["CUSTOMER_ID"].ToString().Trim();
								}
								else
								{
									sBatchNo = "";
									sBoxID = "";
									sGroupNo = "";
									sSoteria = "";
									sCustomerID = "";
								}

								sSQL += $"('{sCOMMAND_ID}', '{dr_Detail["CAROUSEL_ID"]}', '{dr_Detail["CELL_ID"]}', '{sBatchNo}', '{sBoxID}', '{sGroupNo}', '{sSoteria}', '{sCustomerID}'),";
							}
							sSQL = sSQL.Remove(sSQL.LastIndexOf(','), 1);
							App.Local_SQLServer.NonQuery(sSQL);

							sSQL = $"INSERT INTO [CAROUSEL_CHECK_LIST] ([COMMAND_ID],[USER_ID],[STATUS]) VALUES" + //新增此次任務
								$"('{sCOMMAND_ID}', '{dr["USER_ID"]}', {(int)eCarousel_Check_Task_STATUS.eWAIT})";
							App.Local_SQLServer.NonQuery(sSQL);
						}
					}
				}
            }
			Carousel_Check_Task();
			Carousel_Check_CycleT.Enable(true, 60000);
			return 0;
		}

		public void Carousel_Check_Task()
        {
			//掃[CAROUSEL_CHECK_LIST] 檢查有沒有還沒執行的盤點任務
			DataTable dt = App.Local_SQLServer.SelectDB("*", "[CAROUSEL_CHECK_LIST]", "");
			string sSQL;
			foreach(DataRow dr in dt.Rows)
			{
				string sCOMMAND_ID = dr["COMMAND_ID"].ToString().Trim();
				switch ((eCarousel_Check_Task_STATUS)dr["STATUS"].ToString().Trim().ToIntDef(0))
                {
					case eCarousel_Check_Task_STATUS.eWAIT:
						if (UInt64.Parse(sCOMMAND_ID) < UInt64.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff"))) //任務時間開始已到
						{
							App.Bc.STK.C020_CMD(sCOMMAND_ID, "START"); //下命令給STK執行
							sSQL = $"UPDATE [CAROUSEL_CHECK_LIST] SET [STATUS] = '{(int)eCarousel_Check_Task_STATUS.eSendToSTK}' WHERE [COMMAND_ID] = '{sCOMMAND_ID}'"; //將盤點狀態改成1，等待STK回覆
							App.Local_SQLServer.NonQuery(sSQL);
						}
						break;
					case eCarousel_Check_Task_STATUS.eAction:
						break;
					case eCarousel_Check_Task_STATUS.eFINISH:
						sSQL = $"INSERT INTO [CAROUSEL_CHECK_HISTORY] ([COMMAND_ID],[RESULT],[START_TIME],[END_TIME],[USER_ID],[NG_REASON]) VALUES " +
							$" ('{dr["COMMAND_ID"]}', '{dr["RESULT"]}', '{dr["START_TIME"]}', '{DateTime.Now.ToString("yyyyMMddHHmmssfff")}',' {dr["USER_ID"]}', '{dr["NG_REASON"]}') ";
						//App.Local_SQLServer.NonQuery(sSQL);


						sSQL += $"INSERT INTO [CAROUSEL_CHECK_HISTORY_DETAIL] SELECT * FROM [CAROUSEL_CHECK_LIST_DETAIL] WHERE [COMMAND_ID] = '{sCOMMAND_ID}' ";
						sSQL += $"DELETE [CAROUSEL_CHECK_LIST] WHERE [COMMAND_ID] = '{sCOMMAND_ID}' ";
						sSQL += $"DELETE [CAROUSEL_CHECK_LIST_DETAIL] WHERE [COMMAND_ID] = '{sCOMMAND_ID}' ";
						App.Local_SQLServer.NonQuery(sSQL);
						InventoryComp_Report(sCOMMAND_ID);
						break;
				}
            }
		}
		#endregion Carousel_Check
	}

    public class JSON_RESPONSE
	{
		public string RETURN_CODE { get; set; }
		public string RETURN_MSG { get; set; }
	}
	public class CARD_ID_RESPONSE
	{
		public string Result { get; set; }
		public string Msg { get; set; }
	}
	#region Stock In
	public class STOCK_IN_REQUEST
	{
		public string MODE { get; set; }
		public string BATCH_NO { get; set; }
		public string BOX_ID { get; set; }
		public string OPERATOR { get; set; }
	}
	public class STOCK_IN_RESPONSE : JSON_RESPONSE
	{
		public string BATCH_NO { get; set; }
		public string BOX_ID { get; set; }
		public string SOTERIA { get; set; }
		public string CUSTOMER_ID { get; set; }
		public string FACTORY { get; set; }
	}
	#endregion Stock In

	#region Manual Stock Out
	public class MANUAL_STOCK_OUT_REQUEST
	{
		public string MODE { get; set; }
		public string COUNT { get; set; }
		public string OPERATOR { get; set; }
		public string BATCH_NO { get; set; }
		public string GROUP { get; set; }
		public string BOX_ID { get; set; }
	}
	#endregion Manual Stock Out

	#region Pack Stock Out
	public class PACK_STOCK_OUT_REQUEST
	{
		public string MODE { get; set; }
		public string PACK_LIST { get; set; }
	}
	public class PACK_STOCK_OUT_RESPONSE : JSON_RESPONSE
	{
		public string COUNT { get; set; }
		public string BATCH_NO { get; set; }
		public string GROUP { get; set; }
		public string SOTERIA { get; set; }
	}
	#endregion Pack Stock Out

	#region EQ Status Report
	public class EQ_STATUS_REPORT
	{
		public string STK_ID { get; set; }
		public string STATUS { get; set; }
	}
	#endregion EQ Status Report

	#region Alarm Report
	public class ALARM_REPORT
	{
		public string STK_ID { get; set; }
		public string ALARM_ID { get; set; }
		public string ALARM_DESC { get; set; }
	}
	#endregion Alarm Report

	#region Store Fetch Report
	public class STORE_FETCH_REPORT
	{
		public string ACTION { get; set; }
		public string BATCH_NO { get; set; }
		public string GROUP { get; set; }
		public string BOX_ID { get; set; }
		public string STK_ID { get; set; }
		public string STK_POS_NAME { get; set; }
	}
	#endregion Store Fetch Report

	#region Stock Out Report
	public class STOCK_OUT_REPORT
	{
		public string BATCH_NO { get; set; }
		public string GROUP { get; set; }
		public string BOX_ID { get; set; }
	}
	#endregion Stock Out Report

	#region Prepare Out Request
	public class PREPARE_OUT_REQUEST
	{
		public string PACK_LIST { get; set; }
		public string COUNT { get; set; }
		public string BATCH_NO { get; set; }
		public string GROUP { get; set; }
		public string SOTERIA { get; set; }
	}
	#endregion Prepare Out Request
}