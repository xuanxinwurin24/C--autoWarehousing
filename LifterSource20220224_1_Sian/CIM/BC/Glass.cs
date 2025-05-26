using CIM;
using CIM.BC;
using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CIM.BC
{
    public class Glass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public static LogWriter LogFile = new LogWriter(App.sSysDir + @"\LogFile\Glass", "Glass", 5000000);
        public enum eglsStatus : int { Null, Idle, InUnit, OffUint, ProcEnd };

        public Port port;
        public uint SlotNo { get; set; }
        public uint PortID
        {
            get
            {
                if (port == null)
                { return 0; }
                return port.PortNo;
            }
        }
        public string GLASSID { get; set; }
        bool sampling;
        public bool Sampling
        {
            get { return sampling; }
            set
            {
                if (value == sampling) return;
                sampling = value;
                OnPropertyChanged("Sampling");
            }
        }

        eglsStatus status = eglsStatus.Null;
        public eglsStatus Status
        {
            get { return (eglsStatus)status; }
            set
            {
                if (value == status) return;
                status = value;
                if (status == eglsStatus.OffUint)
                {
                    OffTime = ThreadComp.GetTickCount64();
                }
                else
                {
                    OffTime = 0xFFFFFFFFFFFFFFFF;
                }
                OnPropertyChanged("Status");
                string str = string.Format("Glass Port:{0} Slot:{1} Status:{2}", port.PortNo, SlotNo, Status);
                Glass.LogFile.AddString(str);
            }
        }
        public RecipeBody RcpBody;

        void OnPropertyChanged(string name_)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name_));
            }
        }

        int stepNo = -1;
        public int StepNo
        {
            get { return stepNo; }
            set
            {
                if (stepNo == value) { return; }
                stepNo = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("StepNo"));
                }
                string str = string.Format("Glass Port:{0} Slot:{1} at StepNo:{2}", port.PortNo, SlotNo, stepNo);
                if (Unit != null)
                { str += string.Format(" Unit:{0} Group:{1} ", Unit.Para.ID, Unit.Para.Group); }
                else
                { str = string.Format(" at Unit = NULL"); }
                Glass.LogFile.AddString(str);
            }
        }
        EqUnit unit;
        public EqUnit Unit
        {
            get { return unit; }
            set
            {
                if (unit == value) { return; }
                unit = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UnitID"));
                }
            }
        }
        public string UnitID
        {
            get
            {
                if (Unit != null)
                { return Unit.Para.ID; }
                return "null";
            }
        }

        public long[] ProcTime;
        public long[] DwellTime;
        public ulong OffTime = 0xFFFFFFFFFFFFFFFF;
        //---------------------------------------------------------------------------
        public Glass(Port port_, uint slotNo_)
        {
            try
            {
                if (slotNo_ <= 0)
                { slotNo_ = 1; }

                port = port_;
                SlotNo = slotNo_;
            }//try
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        public bool isNextUint(EqUnit unit_)
        {
            if (StepNo + 1 >= RcpBody.Steps.Count)
            { return false; }
            string group = RcpBody.Steps[StepNo + 1].Group;
            if (unit_.Para.Group == group)
            { return true; }
            return false;
        }
        public void EnterUint(EqUnit newUint_)
        {
            if (Unit != null && Unit.UnitState != EqUnit.eState.Run)
            {
                Unit.CurnGlass = null;
            }
            newUint_.CurnGlass = this;

            if (StepNo == -1)//first step
            {
                App.Bc.InlineGlasses.Add(this);
                App.Bc.UpdateList(App.Bc.InlineGlasses);
            }

            Status = eglsStatus.InUnit;
            Unit = newUint_;
            StepNo++;
        }

        public EDC EDC;
        public void GlassDataDown(EDC edc_)
        {
            EDC = edc_;
            if (Status == eglsStatus.OffUint)
            {
                Status = eglsStatus.ProcEnd;
                App.Bc.InlineGlasses.Remove(this);
                App.Bc.UpdateList(App.Bc.InlineGlasses);
                if (Unit != null && Unit.UnitState != EqUnit.eState.Run)
                {
                    Unit.CurnGlass = null;
                }

                EDC.SaveFile();
                Reset();
            }
            else { }//alarm
        }

        public void Reset()
        {
            Sampling = false;
            Status = eglsStatus.Null;
            StepNo = -1;
            unit = null;
            OffTime = 0xFFFFFFFFFFFFFFFF;
        }
    }
    public class EDC
    {
        public static string EDCDIR { get; set; }
        public void SaveFile()
        {
            try
            {
                string fname = App.SysPara.EDCDIR + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + App.SysPara.EqID + "_" + glass_id + ".xml";
                Common.SerializeXMLObjToFile<EDC>(fname, this);
                if (File.Exists(fname) == true)
                {
                    List<string> dst = new List<string>();
                    dst.AddRange(System.IO.File.ReadAllLines(fname, System.Text.Encoding.Default));
                    dst[1] = "<EDC>";
                    System.IO.File.WriteAllLines(fname, dst, System.Text.Encoding.Default);
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public EDC()
        {
            datas = new List<EDC.iary>();
            eqpid = App.SysPara.EqID;
        }
        [XmlIgnore]
        public string PORTID { get; set; }

        public string glass_id { get; set; }
        public string group_id { get; set; }
        public string lot_id { get; set; }
        public string product_id { get; set; }
        public string pfcd { get; set; }//?
        public string eqpid { get; set; }
        public string ec_code { get; set; }//?

        public string RECIPEID { get; set; }
        public string ROUTEID { get; set; }
        public string OWNER { get; set; }

        public string OPERATIONID { get; set; }
        public string CASSETTEID { get; set; }
        public string OPERATORID { get; set; }
        public string CLDATE { get; set; }
        public string CLTIME { get; set; }
        public string MESLINKKEY { get; set; }
        public string REWORKCOUNT { get; set; }
        public string CHAMBERHISTORY { get; set; }
        public List<iary> datas { get; set; }
        public class iary
        {
            public string DVNAME { get; set; }//item_name
            public string DVVAL { get; set; }//<item_type
            public string DVTYPE { get; set; }//item_value
        }
    }
}
//---------------------------------------------------------------------------

