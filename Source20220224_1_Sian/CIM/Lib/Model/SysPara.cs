using CIM.BC;
using Strong;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Documents;
using System.Xml.Serialization;

namespace CIM.Lib.Model
{
	public class SysPara
	{
		public enum enSoftType
		{
			CPC, StockIn, StockOut
		}

		public static string FileName = App.sSysDir + @"\Ini\SysPara.xml";
		public string SoftVersion { get; set; }
		public string ModelName { get; set; }
		public string InLineID { get; set; }
		public string EqID { get; set; }
		public bool Client { get; set; }
		public enSoftType SoftType { get; set; }
		public long StockInWaitTime { get; set; } = 15 * 60 * 1000;
		public long LogOutTime { get; set; } = 10 * 60 * 1000;
		public long UsageReportTime { get; set; } = 30 * 60 * 1000;
		public bool bSimuPLC { get; set; }
		public string ShuttleCar1_IP { get; set; }
		public int ShuttleCar1_Port { get; set; }
		public string ShuttleCar2_IP { get; set; }
		public int ShuttleCar2_Port { get; set; }
		public int Carousel_Utility_Monitoring_Period { get; set; }

		public int iWebService_MaxRetryCount { get; set; }
		public SysPara()
		{
		}
		public void SaveSysParaToFile()
		{
			try
			{
				Common.SerializeXMLObjToFile<SysPara>(SysPara.FileName, this);
			}
			catch (Exception ex)
			{
			}
		}
	}

	public class BcPara
	{
		public static string FileName = App.sSysDir + @"\Ini\BcSetting.xml";

		[XmlElement("EqPara")]
		public List<EqPara> Eqs = new List<EqPara>();
		public BcPara()
		{
		}
	}

	public class EqPara
	{
		public string ID { get; set; }
		public uint Node { get; set; }
		[XmlIgnore]
		public int addrBase = 0;
		[XmlElement("AddrBase")]
		public string sAddrBase
		{
			get
			{
				string str = "0x" + addrBase.ToString("X8");
				return str;
			}
			set
			{
				try
				{
					string str = value.Trim();
					if (str != null && (str.StartsWith("0x") || str.StartsWith("0X")))
					{
						addrBase = int.Parse(str.Substring(2), NumberStyles.HexNumber);
					}
					else
					{
						addrBase = int.Parse(str.Trim());
					}
				}
				catch
				{
					string str = string.Format("AddrBaseStr = {0} error in EQ = {1}", value, ID);
					LogWriter.LogAndExit(str);
				}
			}
		}
		public uint MemLength { get; set; }
		public EqPara()
		{

		}
	}
}