using CIM.BC;
using Strong;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace CIM.Lib.Model
{
	[XmlRoot("Root")]
	public class Alarm
	{
		public static string FileName = Environment.CurrentDirectory + @"\Ini\Alarm.xml";

		public delegate void AlarmEventHandler(AlarmBody AlmObj_, bool bFinal_);

		public SortedList<uint, AlarmBody> BodyList = new SortedList<uint, AlarmBody>();
		public SortedList<uint, string> UnitNames = new SortedList<uint, string>();

		public AlarmEventHandler AlarmEvent;
		public bool bAlarming = false;
		public bool bWarming = false;

		public void LoadFromFile()
		{
			try
			{
				if (File.Exists(FileName) == false)
				{
					MessageBox.Show(FileName + " not exist");
					Environment.Exit(Environment.ExitCode);
				}
				List<AlarmBody> list = Common.DeserializeXMLFileToObject<List<AlarmBody>>(FileName);

				BodyList.Clear();
				foreach (AlarmBody obj in list)
				{
					if (BodyList.ContainsKey((obj.Unit << 16) + obj.ID) == false)
					{
						BodyList.Add((obj.Unit << 16) + obj.ID, obj);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(FileName + " File read error," + ex.ToString());
			}
		}

		public int OccuredCount()
		{
			int Cnt = 0;
			foreach (KeyValuePair<uint, AlarmBody> obj in BodyList)
			{
				if (obj.Value.bOccured == true) Cnt++;
			}
			return Cnt;
		}

		public int EnebledCount()
		{
			int Cnt = 0;
			foreach (KeyValuePair<uint, AlarmBody> obj in BodyList)
			{
				if (obj.Value.bEnabled == true) Cnt++;
			}
			return Cnt;
		}

		public void UnitNameRegister(uint Unit, string sName_)
		{
			if (UnitNames.ContainsKey(Unit) == false)
			{
				UnitNames.Add(Unit, sName_);
			}
		}

		public AlarmBody Set(uint unit_, uint almID_, bool Occur_, string Msg_ = "")
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + almID_, out obj) == false)
			{
				obj = new AlarmBody(unit_, almID_);
				BodyList.Add((obj.Unit << 16) + obj.ID, obj);
			}

			if (Msg_.Trim().Length != 0)
			{
				obj.Message = Msg_;
			}
			//obj.bOccured = Occur_;
			SetOccur(obj, Occur_);
			return obj;
		}

		void SetOccur(AlarmBody obj_, bool value_)
		{
			if (obj_.bOccured == value_) return;
			if (obj_.bEnabled == false) return;

			obj_.bOccured = value_;

			if (value_ == true)
				obj_.OccurTime = DateTime.Now;
			else
			{
				//if (isAlarmExist() == false)
				//App.SecsMain.SecsCenter.S6F11_EqStatusChange("R", 0);
				obj_.DisOccurTime = DateTime.Now;
			}

			bool bAlm = obj_.isAlarmLevel();
			bool bAlmFinal = false;

			if (obj_.bOccured == true)
			{
				if (bAlm == true)//alarm happen
				{
					bAlarming = true;
					//App.SecsMain.SecsCenter.S6F11_EqStatusChange("D",0,obj_);
				}
				else//warning happen
				{
					bWarming = true;
				}
			}
			else
			{
				if (bAlm == true)   //alarm disapear
				{
					if (isAlarmExist() == false)
					{
						bAlarming = false;
						bAlmFinal = true;
					}
				}
				else                //warning disapear
				{
					if (isWarningExist() == false)
					{
						bWarming = false;
						bAlmFinal = true;
					}
				}
			}

			AlarmEvent?.Invoke(obj_, bAlmFinal);
		}

		public void Acknowledge(uint unit_, uint ID_)
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + ID_, out obj) == false) return;

			if (obj.NeedAck == true)
			{
				//obj.bOccured = false;
				SetOccur(obj, false);
			}
		}

		public void ResetAllAlarm(uint eqType_ = 0)
		{
			foreach (KeyValuePair<uint, AlarmBody> obj in BodyList)
			{
				bool bClr = (eqType_ == 0) || (eqType_ == obj.Value.EqType);
				if (bClr == true)
				{
					//obj.Value.bOccured = false;
					SetOccur(obj.Value, false);
				}
			}
		}
		public bool isAlarmExist()
		{
			bool bExist = false;
			try
			{
				foreach (KeyValuePair<uint, AlarmBody> obj in BodyList)
				{
					if (obj.Value.bOccured == true && obj.Value.isAlarmLevel() == true)
					{
						bExist = true;
					}
				}

			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
			return bExist;
		}

		public void AcknowledgeAll()
		{
			foreach (KeyValuePair<uint, AlarmBody> obj in BodyList)
			{
				if (obj.Value.NeedAck == true)
				{
					//obj.Value.bOccured = false;
					SetOccur(obj.Value, false);
				}
			}
		}

		#region GetFuncs
		public List<AlarmBody> GetUnitAlarmObjs(uint unit_)
		{
			List<AlarmBody> lst = new List<AlarmBody>();
			foreach (KeyValuePair<uint, AlarmBody> obj in BodyList)
			{
				uint unit = obj.Value.Unit;
				if (unit == unit_)
				{
					lst.Add(obj.Value);
				}
			}
			return lst;
		}
		public List<AlarmBody> GetCurnAlarmObjs(uint unit_)
		{
			List<AlarmBody> lst = new List<AlarmBody>();
			foreach (KeyValuePair<uint, AlarmBody> obj in BodyList)
			{
				if (obj.Value.bOccured == false) continue;
				uint unit = obj.Value.Unit >> 16;
				if (unit == unit_)
				{
					lst.Add(obj.Value);
				}
			}
			return lst;
		}
		public AlarmBody GetObj(uint unit_, uint ID_)
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + ID_, out obj) == true)
			{ return obj; }
			else
			{ return null; }
		}

		public uint GetCode(uint unit_, uint ID_)
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + ID_, out obj) == true)
			{ return obj.Code; }
			else
			{ return 0; }
		}

		public uint GetLevel(uint unit_, uint ID_)
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + ID_, out obj) == true)
			{ return obj.Level; }
			else
			{ return 9; }
		}

		public string GetMessage(uint unit_, uint ID_)
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + ID_, out obj) == true)
			{ return obj.Message; }
			else
			{ return "Alarm ID No Found"; }
		}

		public bool GetEqStop(uint unit_, uint ID_)
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + ID_, out obj) == true)
			{ return obj.EqStop; }
			else
			{ return true; }
		}

		public bool GetRbStop(uint unit_, uint ID_)
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + ID_, out obj) == true)
			{ return obj.RobotStop; }
			else
			{ return true; }
		}

		public bool GetSECSRpt(uint unit_, uint ID_)
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + ID_, out obj) == true)
			{ return obj.SecsReport; }
			else
			{ return true; }
		}

		public uint GetEqType(uint unit_, uint ID_)
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + ID_, out obj) == true)
			{ return obj.EqType; }
			else
			{ return 0; }
		}

		public string GetUnitName(uint unit_)
		{
			string sName;
			if (UnitNames.TryGetValue(unit_, out sName) == true)
			{ return sName; }
			else
			{ return "No Unit Name"; }
		}
		#endregion GetFuncs

		#region isFuncs

		public bool isEnabled(uint unit_, uint ID_)
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + ID_, out obj) == false) return false;

			return obj.bEnabled;
		}

		public bool isOccured(uint unit_, uint ID_)
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + ID_, out obj) == false) return false;

			return obj.bOccured;
		}

		public bool isAlarmLevel(uint unit_, uint ID_)
		{
			AlarmBody obj = null;
			if (BodyList.TryGetValue((unit_ << 16) + ID_, out obj) == false) return false;

			return obj.isAlarmLevel();
		}

		public bool isAlarmExist(uint eqType_ = 0)
		{
			foreach (KeyValuePair<uint, AlarmBody> obj in BodyList)
			{
				if (obj.Value.bOccured == false) continue;
				if (obj.Value.isAlarmLevel() == true)
				{
					if (eqType_ == 0 || eqType_ == obj.Value.EqType) return true;
				}
			}
			return false;
		}

		public bool isWarningExist(uint eqType_ = 0)
		{
			foreach (KeyValuePair<uint, AlarmBody> obj in BodyList)
			{
				if (obj.Value.bOccured == false) continue;
				if (obj.Value.isWarningLevel() == true)
				{
					if (eqType_ == 0 || eqType_ == obj.Value.EqType) return true;
				}
			}
			return false;
		}
		#endregion isFuncs
	}
	public class AlarmBody
	{
		#region xml Property
		[XmlElement(typeof(uint), ElementName = "ID")]
		public uint ID { get; set; }

		[XmlElement(typeof(uint), ElementName = "Code")]
		public uint Code { get; set; }

		int hexCode;
		[XmlElement(typeof(string), ElementName = "HexCode")]
		public string HexCode
		{
			get
			{
				return hexCode.ToString("X4");
			}
			set
			{
				hexCode = int.Parse(value, System.Globalization.NumberStyles.HexNumber);
			}
		}

		[XmlElement(typeof(uint), ElementName = "Unit")]
		public uint Unit { get; set; }

		[XmlElement(typeof(uint), ElementName = "Level")]
		public uint Level { get; set; }

		[XmlElement(typeof(uint), ElementName = "EqType")]//0:EQ 1:CIM ASSIGN 2:CST 3:CIM
		public uint EqType { get; set; }

		[XmlElement(typeof(bool), ElementName = "SecsReport")]//0:EQ 1:CIM ASSIGN 2:CST 3:CIM
		public bool SecsReport { get; set; }

		[XmlElement(typeof(bool), ElementName = "EqStop")]
		public bool EqStop { get; set; }

		[XmlElement(typeof(bool), ElementName = "RobotStop")]
		public bool RobotStop { get; set; }

		[XmlElement(typeof(bool), ElementName = "NeedAck")]
		public bool NeedAck { get; set; }

		[XmlElement(typeof(string), ElementName = "Message")]
		public string Message { get; set; }

		[XmlElement(typeof(byte), ElementName = "ALCD")]
		public byte ALCD { get; set; }

		[XmlElement(typeof(string), ElementName = "OccurTimeTmp")]
		public string OccurTimeTmp { get; set; }

		[XmlElement(typeof(string), ElementName = "DisOccurTimeTmp")]
		public string DisOccurTimeTmp { get; set; }

		#endregion xml Property

		[XmlIgnore]
		public DateTime OccurTime { get; set; }
		[XmlIgnore]
		public DateTime DisOccurTime { get; set; }
		[XmlIgnore]
		public string OccurTimeStr { get { return OccurTime.ToString("yyyy/MM/dd-HH:mm:ss:fff"); } }
		[XmlIgnore]
		public string DisOccurTimeStr { get { return DisOccurTime.ToString("yyyy/MM/dd-HH:mm:ss:fff"); } }

		bool fbOccured;
		[XmlIgnore]
		public bool bOccured
		{
			get
			{
				return fbOccured;
			}
			set
			{
				if (fbOccured == value) return;
				if (fbEnabled == false) return;
				fbOccured = value;
				//OccurTime = DateTime.Now;
			}
		}
		bool fbEnabled = true;
		[XmlIgnore]
		public bool bEnabled
		{
			get
			{
				return fbEnabled;
			}
			set
			{
				if (fbEnabled == value) return;
				fbEnabled = value;
				if (fbEnabled == true && fbOccured == true)
				{
					//SecsCenter.S5F1(FbOccured,ID);		//SecsCenter.S6F11(19);
				}
			}
		}

		public AlarmBody()
		{
			ID = 0;
			Code = 0;
			Unit = 0;
			Level = 2;            //0:Can not recovery 1:Can 2:Warn
			EqType = 0;           //0:EQ 1:CIM 2:CST
			SecsReport = true;
			EqStop = true;
			RobotStop = true;
			NeedAck = true;
			//ALCD = 2;

			Message = "Alarm ID Not Found";
			fbOccured = false;
			fbEnabled = true;
			OccurTime = DateTime.Now;
			DisOccurTime = new DateTime(0);
		}
		public AlarmBody(uint unit_, uint ID_) : this()
		{
			ID = ID_;
			Unit = unit_;
		}
		//AlarmCode()//if has this constructor exist,datagrid auto add row function cannot work
		//    : this(0, 0)
		//{

		public bool isAlarmLevel()
		{
			return (Level == 0 || Level == 1);
		}

		public bool isWarningLevel()
		{
			return (Level >= 2);
		}
		public void Copy(AlarmBody AlmObj_)
		{
			ID = AlmObj_.ID;
			Code = AlmObj_.Code;
			Unit = AlmObj_.Unit;
			Level = AlmObj_.Level;            //0:Can not recovery 1:Can 2:Warn
											  //EqType = 0;           //0:EQ 1:CIM 2:CST
											  //SecsReport = true;
											  //EqStop = true;
											  //RobotStop = true;
											  //NeedAck = true;
											  //ALCD = 2;

			Message = AlmObj_.Message;
			//fbOccured = false;
			//fbEnabled = true;
			OccurTime = AlmObj_.OccurTime;
			DisOccurTime = AlmObj_.DisOccurTime;
		}
	}
}