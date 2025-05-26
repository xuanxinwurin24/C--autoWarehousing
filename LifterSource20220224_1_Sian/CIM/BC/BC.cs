using CIM.Lib.Model;
using Strong;
using System;
using System.IO;
using static CIM.BC.DeliStore;

namespace CIM.BC
{
    public class BC
    {
        #region Property
        //public string MemoryPath = App.sSysDir + @"\Ini\Memory\BC";
        public string MemoryPath = "CIM.BC.Memory.";
        public LogWriter LogFile = new LogWriter();
        public ActMode eLiftMode = ActMode.None;
        public InputWay eStockInMode = InputWay.None;

        string ID = "BC";

        #endregion Property
        ThreadTimer CPC_Alive_Timer;

        #region INIT
        public BC()
        {
            try
            {
                LogFile.PathName = App.sSysDir + @"\LogFile\BC\";
                LogFile.sHead = ID;

                MemAddrInitial();

                HSMSpar HSMSpara;
                HSMSpara = Common.DeserializeXMLFileToObject<HSMSpar>(HSMSpar.FileName);

                CPC_Alive_Timer = App.MainThread.TimerCreate("CPC_Alive_Timer", 4000, CPC_Alive_Timer_Event, ThreadTimer.eType.Cycle);
                CPC_Alive_Timer.Enable(true);
                App.Alarm.AlarmEvent += new Alarm.AlarmEventHandler(AlarmEventCallBack);
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        ~BC() { }

        #region MEMGRUOP
        public MemGroup bCPC;
        public MemGroup CPCStatus;
        public MemGroup CPCDateTime;
        public MemGroup CrownLifter;
        public MemGroup BoxIDRetry;
        int AddrBase = 0x0000;

        void MemAddrInitial()
        {
            try
            {
                bCPC = App.PlcDevice.AddMemGroup_CSVStream(ID, "CPC Bit", LoadStreamResouce(MemoryPath + "HB01.csv"), MemBank.eBank.Bbit, AddrBase, true);
                CPCStatus = App.PlcDevice.AddMemGroup_CSVStream(ID, "CPC Status", LoadStreamResouce(MemoryPath + "HW01.csv"), MemBank.eBank.Wreg, AddrBase, true);
                CPCDateTime = App.PlcDevice.AddMemGroup_CSVStream(ID, "CPC DateTime", LoadStreamResouce(MemoryPath + "HW02.csv"), MemBank.eBank.Wreg, AddrBase, true);
                CrownLifter = App.PlcDevice.AddMemGroup_CSVStream(ID, "Crown to Lifter", LoadStreamResouce(MemoryPath + "HW03.csv"), MemBank.eBank.Wreg, AddrBase, true);
                BoxIDRetry = App.PlcDevice.AddMemGroup_CSVStream(ID, "Box ID Retry", LoadStreamResouce(MemoryPath + "HW04.csv"), MemBank.eBank.Wreg, AddrBase, true);
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
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
        #endregion
        public void Initial()
        {
            uint Node = 1;
            App.Alarm.UnitNameRegister(Node, ID);

            try
            {
                bCPC.MgValChangeEvent += BCPC_MgValChangeEvent;
                CPCStatus.MgValChangeEvent += BCPC_MgValChangeEvent;
                //CPCDateTime.MgValChangeEvent += BCPC_MgValChangeEvent;
                CrownLifter.MgValChangeEvent += BCPC_MgValChangeEvent;
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        private void BCPC_MgValChangeEvent(MemGroup mg_, TagItem Item_)
        {
            if (Item_.Name != "Alive")
                LogFile.AddString(string.Format("{0}:{1}={2}", Item_.Mg.Name, Item_.Name, Item_.StringValue));
        }
        #endregion INIT 

        private int CPC_Alive_Timer_Event(ThreadTimer threadTimer_)
        {
            uint Index = CPCStatus["Alive"].BinValue + 1; //避免0
            Index = Index == 0 ? 1 : Index;
            CPCStatus["Alive"].BinValue = Index;
            return 0;
        }
        public void SetStatusStore(ActMode act_, InputWay way_)
        {
            //App.Eq.bCanDeli = true; // (uint)act_ == 2 ? true : false;
            //App.Eq.bCanStore = true;// !App.Eq.canDeli;
            eLiftMode = act_;
            eStockInMode = way_;
            uint uLiftMode_ = (uint)act_;
            uint uStockInMode_ = (uint)way_;
            CPCStatus["LiftMode"].BinValue = uLiftMode_;
            CPCStatus["StockInMode"].BinValue = uStockInMode_;
        }

        public void SetDateTime()
        {
            CPCDateTime["YY"].StringValue = DateTime.Now.ToString("yy");
            CPCDateTime["MM"].StringValue = DateTime.Now.ToString("MM");
            CPCDateTime["DD"].StringValue = DateTime.Now.ToString("dd");
            CPCDateTime["HH"].StringValue = DateTime.Now.ToString("HH");
            CPCDateTime["NN"].StringValue = DateTime.Now.ToString("mm");
            CPCDateTime["SS"].StringValue = DateTime.Now.ToString("ss");
            Req_Resp_OnOff(bCPC["CPCDateTimeRpt"], true, true);
        }

        public void SetCrownToLifter(string sBoxID_)
        {
            //HW03
            CrownLifter["BoxID"].StringValue = sBoxID_;
            Req_Resp_OnOff(bCPC["CrownLifterRpt"], true, true);
        }

        public void DateTime_AckFromEq(TagItem it_)//HH03
        {
            try
            {
                if (it_.BinValue == 1)
                {
                    Req_Resp_OnOff(bCPC["CPCDateTimeRpt"], false);
                }
                if (it_.BinValue == 0)
                { return; }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public void ManualStockInStart_AckFromEq(TagItem it_)//HH03
        {
            try
            {
                if (it_.BinValue == 1)
                {
                    Req_Resp_OnOff(bCPC["ManualStockInStartRpt"], false);
                }
                if (it_.BinValue == 0)
                { return; }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public void CrownLifter_AckFromEq(TagItem it_)//HH03
        {
            try
            {
                if (it_.BinValue == 1)
                {
                    Req_Resp_OnOff(bCPC["CrownLifterRpt"], false);
                }
                if (it_.BinValue == 0)
                { return; }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        public void AlarmWarningReply(bool rising_)
        {
            try
            {
                if (rising_ == true)
                {
                    Req_Resp_OnOff(bCPC["AlarmWarningAck"], true, true);
                }
                else
                {
                    Req_Resp_OnOff(bCPC["AlarmWarningAck"], false);
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public void StockInReply(bool rising_)
        {
            try
            {
                if (rising_ == true)
                {
                    Req_Resp_OnOff(bCPC["StockInAck"], true, true);
                }
                else
                {
                    Req_Resp_OnOff(bCPC["StockInAck"], false);
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public void StockOutReply(bool rising_)
        {
            try
            {
                if (rising_ == true)
                {
                    Req_Resp_OnOff(bCPC["StockOutAck"], true, true);
                }
                else
                {
                    Req_Resp_OnOff(bCPC["StockOutAck"], false);
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public void LiftCompleteReply(bool rising_)
        {
            try
            {
                if (rising_ == true)
                {
                    Req_Resp_OnOff(bCPC["LiftCompleteAck"], true, true);
                }
                else
                {
                    Req_Resp_OnOff(bCPC["LiftCompleteAck"], false);
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        public void StockInReadDataNGReply(bool rising_)
        {
            try
            {
                if (rising_ == true)
                {
                    Req_Resp_OnOff(bCPC["StockInReadDataNGAck"], true, true);
                }
                else
                {
                    Req_Resp_OnOff(bCPC["StockInReadDataNGAck"], false);
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public void BoxInReply(uint action_)
        {
            try
            {
                BoxIDRetry["BoxIDInReadNG"].BinValue = action_;//action_=1:retry;2:force in
                IndexAdd1(BoxIDRetry["BoxIDInReadNGIndex"]);
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public void BoxOutReply(uint action_,string KeyIn)
        {
            try
            {
                BoxIDRetry["BoxIDOutReadNG"].BinValue = action_;//action_=1:retry;2:key in
                BoxIDRetry["BoxIDOutKeyIn"].StringValue = KeyIn;
                IndexAdd1(BoxIDRetry["BoxIDOutReadNGIndex"]);
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public void BoxOutCompareReply(uint action_)
		{
			try
			{
                BoxIDRetry["BoxIDOutCompareNG"].BinValue = action_;
                IndexAdd1(BoxIDRetry["BoxIDOutCompareNGIndex"]);
			}
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public void IndexAdd1(TagItem item_, bool bTEnable_ = false)
        {
            try
            {
                uint val = item_.BinValue;
                item_.SyncBinValue = (ushort)(val >= 0xFFFF ? 1 : val + 1);
                if (bTEnable_ == false) return;
                ThreadTimer timer = (ThreadTimer)(item_.Tag);
                if (timer == null) return;
                timer.Enable(true);//for time out
                LogFile.AddString(string.Format("{0}:{1}={2}", item_.Mg.Name.Trim(), item_.Name.Trim(), item_.StringValue.Trim()));
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        public void Req_Resp_OnOff(TagItem item_, bool on_, bool bTEnable_ = false)
        {
            try
            {
                if (on_ == true)
                {
                    item_.SyncBinValue = 1;
                    item_.SyncPreDlyTime = 500;
                    if (bTEnable_ == false) return;
                    ThreadTimer t_ = (ThreadTimer)(item_.Tag);
                    if (t_ == null) return;
                    t_.Enable(true);            //for time out
                }
                else
                {
                    item_.BinValue = 0;
                    ThreadTimer t_ = (ThreadTimer)(item_.Tag);
                    if (t_ == null) return;
                    t_.Enable(false);            //for time out
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }

        void AlarmEventCallBack(AlarmBody AlmObj_, bool bFinalClear_)
        {
            string sSet = AlmObj_.bOccured == true ? "1" : "0";
            string sReqData = string.Format("{0},{1}", AlmObj_.ID, sSet);
            App.SQL_HS.Req("AlarmRpt", sReqData, ucFrame.None);
        }
    }
}
