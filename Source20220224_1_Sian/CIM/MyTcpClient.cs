using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using Strong;

namespace CIM
{
    public class MyTcpClient
    {
		private static LogWriter logWriter = new LogWriter(Environment.CurrentDirectory + @"\LogFile\TcpIp", "TcpIp", 50000);

		private string _TcpIpAddress;
        public string TcpIpAddress
        {
            get { return _TcpIpAddress; }
            set { _TcpIpAddress = value; }
        }

        private int _Port;
        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        public MyTcpClient(string strTcpIpAddress, int strPort)
        {
            _TcpIpAddress = strTcpIpAddress;
            _Port = strPort;
        }

        public MyTcpClient()
        {
            _TcpIpAddress = "127.0.0.1";
            _Port = 8000;
        }

        private TcpClient myTcpClient;

        public bool Connected = false;
        public bool Connecting = false;

        public void ConnectToServer()
        {
            try
            {
                myTcpClient = new TcpClient();

                Connecting = true;
                myTcpClient.Connect(_TcpIpAddress, _Port);

                //沒有excetion代表連線成功
                Connecting = false;
				Connected = true;
            }
            catch (Exception ex)
            {
                Connecting = false;
                Connected = false;
				logWriter.AddString(ex.ToString());
			}
        }

        public void CheckConnect()
        {
            byte[] testByte = new byte[1];

            try
            {
                //使用Peek測試連線是否仍存在
                if (myTcpClient.Connected && myTcpClient.Client.Poll(0, SelectMode.SelectRead))
                    Connected = myTcpClient.Client.Receive(testByte, SocketFlags.Peek) != 0;
            }
            catch (SocketException ex)
            {
                Connected = false;
				logWriter.AddString(ex.ToString());
			}
        }

        public bool SendData(string str_SendData)
        {
            bool bSendSuccess = false;

            try
            {
                NetworkStream myStream;
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(str_SendData);

                myStream = myTcpClient.GetStream(); // 取得clientstream
                if (myStream.CanWrite)
                {
                    myStream.Write(data, 0, data.Length); // 送出資料給Server
                    bSendSuccess = true;
                }
            }
            catch (Exception ex)
            {
                myTcpClient.Close();
				logWriter.AddString(ex.ToString());
			}

            return bSendSuccess;
        }

        public string ReceiveData()
        {
            string str_RecieveData = string.Empty;

            try
            {
                byte[] ReceiveBytes = new byte[myTcpClient.ReceiveBufferSize];
                int NumberOfBytesRead = 0;
                NetworkStream myStream;

                myStream = myTcpClient.GetStream();

                if (myStream.DataAvailable)
                {
                    NumberOfBytesRead = myStream.Read(ReceiveBytes, 0, myTcpClient.ReceiveBufferSize);
                    str_RecieveData = Encoding.Default.GetString(ReceiveBytes, 0, NumberOfBytesRead);
                }
            }
            catch (Exception ex)
            {
                myTcpClient.Close();
				logWriter.AddString(ex.ToString());
			}

            return str_RecieveData;
        }

        public void ClientClose()
        {
            try
            {
                myTcpClient.Close();
                Connected = false;
            }
            catch (Exception ex)
            {
				logWriter.AddString(ex.ToString());
			}
        }
    }
}