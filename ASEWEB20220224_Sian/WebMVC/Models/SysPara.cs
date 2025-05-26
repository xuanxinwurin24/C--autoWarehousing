
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Documents;
using System.Xml.Serialization;

namespace ASEWEB.Models
{
	public class SysPara
	{
		public enum enSoftType
		{
			CPC, StockIn, StockOut
		}

		public static string FileName = Startup.sSysDir + @"\Ini\SysPara.xml";
		public string SoftVersion { get; set; }
		public string ModelName { get; set; }
		public string InLineID { get; set; }
		public string EqID { get; set; }
		public bool Client { get; set; }
		public string SqlDB_data_source { get; set; }
		public string SqlDB_catalog { get; set; }
		public string SqlDB_uid { get; set; }
		public string SqlDB_password { get; set; }
		public enSoftType SoftType { get; set; }
		public long StockInWaitTime { get; set; } = 15 * 60 * 1000;
		public long LogOutTime { get; set; } = 10 * 60 * 1000;
		public long UsageReportTime { get; set; } = 30 * 60 * 1000;
		public bool bSimuPLC { get; set; }
		public bool bSimuPLC2 { get; set; }
		public int Carousel_Utility_Monitoring_Period { get; set; }
		public SysPara()
		{
		}
		public void SaveSysParaToFile()
		{
			try
			{
				//Common.SerializeXMLObjToFile<SysPara>(SysPara.FileName, this);
			}
			catch (Exception ex)
			{
			}
		}
	}

	/*public class ReaderPara
    {
        public static string FileName = App.sSysDir + @"\Ini\ReaderSetting.xml";

        [XmlElement("CCDPara")]
        public List<CCDPara> CCDs = new List<CCDPara>();
        public ReaderPara()
        {
        }
    }*/

	/*public class CCDPara
    {
        public string EqName { get; set; }
        public bool EqSide { get; set; }
        public bool Client { get; set; }
        public int Port { get; set; }
        public string ServerIP { get; set; }

        public CCDPara()
        {
        }
    }*/

	public class BcPara
	{
		public static string FileName = Startup.sSysDir + @"\Ini\BcSetting.xml";

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
					//LogWriter.LogAndExit(str);
				}
			}
		}
		public uint MemLength { get; set; }
		public EqPara()
		{

		}
	}
}