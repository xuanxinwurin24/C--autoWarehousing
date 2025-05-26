using CIM;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Runtime.CompilerServices;
using System.Data;
namespace Strong
{
	public class RFIDReader : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string name_ = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name_));
		}

		private string operatorID = "";
		
		public string sOperatorID
		{
			get { return operatorID; }
			set
			{
				if (operatorID == value) return;
				operatorID = value;
				RFIDLogFile.AddString("RFID: " + operatorID);
				OnPropertyChanged();

				App.SQL_HS.Req("UserIDReq", operatorID, CIM.BC.DeliStore.ucFrame.None);
				//暫時
                //App.DS.OP.UserName = operatorID;
                //App.DS.OP.Level = "9";
            }
		}


		#region INIT
		public Rs232 ComPort;
		public LogWriter RawLogFile;
		public LogWriter RFIDLogFile;
		private ThreadTimer ReadTimer;

		//public enum eState : uint
		//{
		//    eNone = 0, eIDLE, eREAD_START, eRCV_WAITMSG, eRCV_COMP, eSEND_READY, eSEND, eSEND_COMP
		//};
		public enum eResult : uint
		{
			eNone = 0, eSuccess, eFailed
		};
		public eResult ReadResult = eResult.eNone;

		public const int InPortRFID_No = 0;
		public const int OutPortRFID_No = 1;

		public RFIDReader(int iComPort_)
		{
			SerialPort serialPort = new SerialPort();
			serialPort.PortName = "COM" + iComPort_.ToString();
			serialPort.BaudRate = 9600;
			serialPort.Parity = Parity.None;//0:None 1:Odd 2:Even 3:Mark 4:Space
			serialPort.DataBits = 8;
			serialPort.StopBits = StopBits.One;//0=0bit 1=1bit 2=2bit 3=1.5bit
			serialPort.DtrEnable = true;
			serialPort.RtsEnable = true;

			RFIDLogFile = new LogWriter(App.sSysDir + @"\LogFile\RFIDLog\", "RFIDLog", 50000);

			ComPort = new Rs232(serialPort);
			ComPort.bCommunicationRawDataLog = true;
			ComPort.RawLogFile.MaxSize = 5000000;
			ComPort.RawLogFile.PathName = App.sSysDir + @"\LogFile\RFIDLog\HEX\";
			ComPort.RawLogFile.sHead = "HEX";

			ReadTimer = App.MainThread.TimerCreate("RFID_ReadTimer", 1000, ReadTimer_Event, ThreadTimer.eType.Cycle);
			ReadTimer.Enable();
		}
		public void PortOpen()
		{
			try
			{
				bool bResult = ComPort.Start();
				RFIDLogFile.AddString("ComPort Start: " + bResult);
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
			}
		}
		~RFIDReader()
		{
			ComPort.Stop();
		}

		#endregion INIT	   

		public bool Send(byte[] data_, int len_ = 0)
		{
			return ComPort.Send(data_, len_);
		}

		#region Timer
		private int ReadTimer_Event(ThreadTimer Sender)
		{
			if (ComPort.RcvData.Len == 0) return 0;

			//bool bError = false;
			string sResult = string.Empty;
			string sRFID_No = string.Empty;

			lock (ComPort.RcvData.LockObj)
			{

				string sData = Encoding.ASCII.GetString(ComPort.RcvData.Buff).Replace("\0", ""); //sData

				RFIDLogFile.AddString($"Read Raw Data = {sData}");
				string[] options = new string[] {"\r\n" };
				string[] str = sData.Split(options, StringSplitOptions.None);
				sOperatorID = str[0].Substring(1);
				// \u0002 36134727191011332 \r\u 0003 空白是自行添加的
				//00000034377087028
				ComPort.RcvData.Clear();
			}
			return 0;
		}

		#endregion Timer
	}
}
