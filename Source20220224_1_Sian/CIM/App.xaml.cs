using CIM.BC;
using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Net.NetworkInformation;
using System.Collections;
using System.Xml;
using CIM.UILog;

namespace CIM
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public enum enLogType
		{
			Binding, Stock_In, Stock_Out, Clean_Out, Operation
		}
		public const string sVer = "Ver:2022/02/22.01";
		public static string sSysDir = Environment.CurrentDirectory;
		public const uint Level_OP = 3;
		public const uint Level_ENG = 6;
		public const uint Level_SUPER = 9;

		public static SysPara SysPara;
		public static Alarm Alarm = new Alarm();
		public static LogWriter LogFile;
        public static SQL_DB STK_SQLServer;
        public static SQL_DB Local_SQLServer;

        public static ThreadComp MainThread;
		public static ThreadComp DatabaseThread;
		public static UnDevice CCLinkDevice;
		public static UnDevice SimuDevice;
		public static WebServiceSetting WS_Setting = new WebServiceSetting();

		public static uint IdleCnt;

		public static CIM.BC.BC Bc;
		static App()
		{
			LoadResource.Register();
		}
		void app_Startup(object sender, StartupEventArgs e)
		{
			string _starturi = "MainWindow.xaml";

			try
			{
				if (isRunning())
				{
					Environment.Exit(Environment.ExitCode);
				}

				NewSysUnit();
				NewDevice();
				NewUnitWithMemGroup();

				Initial();

				Current.StartupUri = new Uri(_starturi, UriKind.Relative);

				EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseMoveEvent, new MouseEventHandler(OnPreviewMouseDown));
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		static void OnPreviewMouseDown(object sender, MouseEventArgs e)
		{
			IdleCnt = 0;
			//Trace.WriteLine("Clicked!!");
		}

		protected override void OnExit(ExitEventArgs e)
		{
			//App.Bc.SaveLastData();	//改成存資料庫
			LogFile.AddString("Program stop. " + sVer);
			//SecsMain.StatusSave();
		}

		public static void NewSysUnit()
		{
			try
			{
				//建立App Log
				LogFile = new LogWriter(sSysDir + @"\LogFile\AppLog", "App", 500000);
				LogFile.AddString("Program start. " + sVer);

				//讀取參數
				SysPara = Common.DeserializeXMLFileToObject<SysPara>(SysPara.FileName);
				WS_Setting = Common.DeserializeXMLFileToObject<WebServiceSetting>(App.sSysDir + "\\Ini\\WebService.xml");
				SQL_Parameter STK_SQL_Para = Common.DeserializeXMLFileToObject<SQL_Parameter>(App.sSysDir + "\\Ini\\STK_SQL_Parameter.xml");
				SQL_Parameter Local_SQL_Para = Common.DeserializeXMLFileToObject<SQL_Parameter>(App.sSysDir + "\\Ini\\Local_SQL_Parameter.xml");
				Alarm.LoadFromFile();
				Password.Initial();

				//建立Thread
				MainThread = new ThreadComp("MainThreadComp");
				MainThread.CycleRunEvent += MainThreadComp_CycleRunEvent;

				DatabaseThread = new ThreadComp("DatabaseThread");
				DatabaseThread.CycleRunEvent += DatabaseThread_CycleRunEvent;

                //建立SQL連線
                STK_SQLServer = new SQL_DB(STK_SQL_Para);
                Local_SQLServer = new SQL_DB(Local_SQL_Para);
				//if (STK_SQLServer.ConnectToDB() == false)
				//	LogWriter.LogAndExit("STK DataBase連線失敗，請確認!");
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public static void NewDevice()
		{
			try
			{
				//-------Simulation-------
				SimuDevice = new UnDevice("SimuDevice", MainThread);
				SimuDevice.DevLogFile.PathName = sSysDir + @"\LogFile\Device Log\SimuDevice";
				SimuDevice.DevLogFile.sHead = "SimuDevice";
				SimuDevice.Simulation = true;
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		static void NewUnitWithMemGroup()           //new unit that included MemGroup
		{
			try
			{
				Bc = new BC.BC();
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
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

				//建立 TCP 連線
				//if (PlcDevice.Simulation == false)
				//	OCR_Reader.ConnectToServer();

				//--------------Initial TagItem Value for unit-----------
				Bc.Initial();
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
		public static void Start()
		{
			try
			{
				MainThread.Start();
				DatabaseThread.Start();
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
		private static void MainThreadComp_CycleRunEvent(ThreadComp Sender_)
		{
			try
			{
                App.Bc.TaskCtrl.TaskListCheck();
                App.Bc.Check_WebReport();
                if (Sender_.ScanTime > 1000)
					LogFile.AddString(string.Format("{0} CycleTime : {1}", Sender_.Name, Sender_.ScanTime));
				
				//Thread.Sleep(10);
				SpinWait.SpinUntil(() => false, 10);
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
		public static bool bFirstRun = true;
		private static void DatabaseThread_CycleRunEvent(ThreadComp Sender_)
		{
			try
			{
				if (Sender_.ScanTime > 1000)
					LogFile.AddString(string.Format("{0} CycleTime : {1}", Sender_.Name, Sender_.ScanTime));
				UpdateDatabase();
				SpinWait.SpinUntil(() => false, 500);
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private static DataTable Result = new DataTable();
		private static void UpdateDatabase()
		{
			string sql = string.Empty;
			try
			{
				sql = $"SELECT * FROM [CAROUSEL_STATUS]";
				App.STK_SQLServer.Query(sql, ref Result, true);
				string scellid = string.Empty;
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
					App.Bc.Transfer_CarouselCell_to_ViewID(ref carouselid, ref scellid); //轉換成SHOW的ID

					cCarousel carousel = App.Bc.Carousels?.Find(x => x.Carousel_ID == carouselid);
					if (carousel != null)
					{
						carousel.Status = status;
						carousel.Store_Status = store_status;
						carousel.N2_Status = n2_status;
						carousel.Temperature = temperature;
						carousel.Temperature_Status = temp_status;
						carousel.Humidity = humidity;
						carousel.Humidity_Status = humi_status;
						carousel.Command_ID = cmdid;
						carousel.Check_Status = check_status;
					}
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
	}
}