using CIM;
using CIM.BC;
using CIM.Lib.Model;
using Strong;
using System;
using System.ComponentModel;
using static CIM.BC.Glass;

namespace CIM.BC
{
    public class Port : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name_)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name_));
            }
        }
        public uint PortNo { get; set; }//same as position
        int slotCount = 20;
        public int SlotCount { get; set; }
        public Glass[] Glasses = null;
        public RecipeBody RecipeBody;
        public TagItem SensorIt;
        public string JobStatus;

        public string LOTID;
        public string PRODUCTID;
        public string RECIPEID;
        public string ROUTEID;
        public string OWNERID;
        public string OPERATIONID;
        public string CASSETTEID;
        public string OPERATOR;
        public string CLMDATE;
        public string CLMTIME;
        //public string PORTID;
        public string PROCESSINFO;
        public string RESERVE1;
        public string RESERVE2;
        public string RESERVE3;
        public string RESERVE4;
        public string RESERVE5;
        public string RESERVE6;
        public string RESERVE7;

        public Port(uint portNo_)
        {
            try
            {
                PortNo = portNo_;
                SlotCount = slotCount;
                Glasses = new Glass[SlotCount];
                JobStatus = "null";
                for (int i = 0; i < SlotCount; i++)
                {
                    uint slot = (uint)i + 1;
                    Glasses[i] = new Glass(this, slot);
                }
            }//try
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        public int CstInfoDown()
        {
            int ircpNo = int.TryParse(RECIPEID.Trim(), out ircpNo) == true ? ircpNo : 0;
            RecipeBody = App.Rcp.GetObj(ircpNo);
            if (RecipeBody == null)
            {
                App.Alarm.Set(1, 150 + 111, true, RECIPEID + " recipe no found");
                return 1;
            }

            for (int i = 0; i < SlotCount; i++)
            {
                Glasses[i].Reset();
                Glasses[i].RcpBody = RecipeBody;
                Glasses[i].Sampling = PROCESSINFO[i] == 'Y' ? true : false;
                Glasses[i].Status = Glasses[i].Sampling == true ? eglsStatus.Idle : eglsStatus.Null;

                Glasses[i].ProcTime = new long[RecipeBody.Steps.Count];
                Glasses[i].DwellTime = new long[RecipeBody.Steps.Count];
            }
            JobStatus = "Proc";
            return 0;
        }
        public Glass FindGlass(string glsID_)
        {
            foreach (Glass gls in Glasses)
            {
                if (gls.GLASSID == glsID_)
                { return gls; }
            }
            return null;
        }

        public Glass GetNextGlass()
        {
            if (JobStatus == "null")
            { return null; }
            foreach (Glass gls in Glasses)
            {
                if (gls.Status == eglsStatus.Idle)
                { return gls; }
            }
            JobStatus = "null";
            return null;
        }
        public static Port FindPort(string pNo_)
        {
            int pNo = int.TryParse(pNo_, out pNo) == true ? pNo : 0;
            foreach (Port p in App.Bc.Ports)
            {
                if (p.PortNo == pNo)
                { return p; }
            }
            return null;
        }
    }
}
