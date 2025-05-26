using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static CIM.BC.Glass;

namespace CIM.BC
{
    public abstract class EqUnit : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public static LogWriter LogFile = new LogWriter(App.sSysDir + @"\LogFile\EqUint", "EqUint", 5000000);
        public enum eState { Idle = 0, ExRq, Run, UdRq };

        public UnitPara Para;
        public ThreadTimer StayT;
        public ThreadTimer StateFilterT;

        Glass curnGlass = null;
        public Glass CurnGlass
        {
            get { return curnGlass; }
            set
            {
                if (curnGlass == value) { return; }
                curnGlass = value;
                GlassState = getGlassState();

                string str;
                if (curnGlass != null)
                { str = string.Format("Unit:{0} Curn Glass:{1} Port:{2} Slot:{3}", Para.ID, CurnGlass.GLASSID, CurnGlass.PortID, CurnGlass.SlotNo); }
                else
                { str = string.Format("Unit:{0} Curn Glass:null", Para.ID); }
                LogFile.AddString(str);
            }
        }

        uint preUintState = (uint)eState.Idle;
        protected eState PreUintState
        {
            get { return (eState)preUintState; }
            set
            {
                if (value == (eState)preUintState) { return; }
                preUintState = (uint)value;
                StateFilterT.Enable(true);
            }
        }
        uint unitState = (uint)eState.Idle;
        public eState UnitState
        {
            get { return (eState)unitState; }
            set
            {
                if (value == (eState)unitState) { return; }
                StateChanged(value, (eState)unitState);
                unitState = (uint)value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("UnitState"));
                }
                string str = string.Format("Unit:{0} UnitState:{1}", Para.ID, unitState);
                LogFile.AddString(str);
            }
        }

        int glassState = 0;
        public int GlassState
        {
            get { return glassState; }
            set
            {
                if (glassState == value) { return; }
                glassState = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("GlassState"));
                }
                string str = string.Format("Unit:{0} GlassState:{1:D2}", Para.ID, GlassState);
                LogFile.AddString(str);
            }
        }
        bool glassExist = false;
        protected bool GlassExist
        {
            get { return glassExist; }
            set
            {
                if (glassExist == value) { return; }
                glassExist = value;
                GlassState = getGlassState();
            }
        }

        public EqUnit(BC bc_, UnitPara para_)
        {
            try
            {
                Para = para_;
                StayT = App.MainThread.TimerCreate("StayT", 5000, StayT_Event, ThreadTimer.eType.Hold);
                StateFilterT = App.MainThread.TimerCreate("StateFilterT", 3000, StateFilterT_Event, ThreadTimer.eType.Hold);
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public void Initial()
        {
        }

        protected virtual void PreUnitState_Update(TagItem Item_)//call after sensor change
        {
        }
        protected virtual void StateChanged(eState newSt_, eState oldSt_)//call after eqstage change
        {
            try
            {
                if (newSt_ == eState.Run)
                {
                    GlassIn();
                    StayT.Enable(true, 0x7FFFFFFFFFFFFFFF);
                }
                else  //eState.Idle
                {
                    if (CurnGlass != null && CurnGlass.StepNo >= 0)
                    {
                        CurnGlass.ProcTime[CurnGlass.StepNo] = StayT.AccTime;//record  DwellTime
                    }
                    StayT.Enable(false);//disable DwellTime
                    GlassOut();
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        bool isLastUnit()
        {
            if (App.Bc.CurnPort == null)
            { return false; }
            RecipeBody rcpBdy = App.Bc.CurnPort.RecipeBody;
            RcpStep lastStep = rcpBdy.Steps[rcpBdy.Steps.Count - 1];
            if (lastStep.Group == Para.Group)
            { return true; }
            return false;
        }
        bool isFirstUnit()
        {
            if (App.Bc.CurnPort == null)
            { return false; }
            RecipeBody rcpBdy = App.Bc.CurnPort.RecipeBody;
            RcpStep istStep = rcpBdy.Steps[0];
            if (istStep.Group == Para.Group)
            { return true; }
            return false;
        }
        protected void GlassIn()
        {
            try
            {
                if (isFirstUnit() == true)      //EX SCR
                {
                    Port p = App.Bc.CurnPort;
                    if (p != null)
                    {
                        Glass gls = p.GetNextGlass();
                        if (gls != null)
                        {
                            gls.EnterUint(this);//gls.State = Run;
                        }
                        //else if (App.Bc.LastPort != null)   //search LastPort,maybe curn port is robot put-back-action,not get-action
                        //{
                        //    p = App.Bc.LastPort;
                        //    gls = p.GetNextGlass();
                        //    if (gls != null)
                        //    {
                        //        gls.EnterUint(this);//gls.State = Run;
                        //    }
                        //}
                    }
                }
                else
                {
                    Glass gls = SearchGlassFromFrontEQ();//without Run state
                    if (gls != null)
                    {
                        gls.EnterUint(this);
                    }
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        protected void GlassOut()
        {
            try
            {
                if (CurnGlass != null)//glass put to port
                {
                    CurnGlass.Status = eglsStatus.OffUint;
                    //CurnGlass = null;
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        int getGlassState()//csall after sensor change or glass statechange
        {
            if (glassExist == true && curnGlass != null)
            { return 3; }//Green
            else if (glassExist == true && curnGlass == null)
            { return 2; }//clYellow
            else if (glassExist == false && curnGlass != null)
            { return 1; }//clRed
            else //if (glassExist == false && curnGlass == null)
            { return 0; }//clWhite
        }
        Glass SearchGlassFromFrontEQ()
        {
            Glass maxOldGls = null;
            ulong minTime = 0xFFFFFFFFFFFFFFFF;
            foreach (Glass gls in App.Bc.InlineGlasses)
            {
                if (gls.isNextUint(this) == false)
                { continue; }
                else if (gls.Status == eglsStatus.OffUint)
                {
                    if (gls.OffTime < minTime)
                    {
                        maxOldGls = gls;
                        minTime = gls.OffTime;
                    }
                }
            }
            return maxOldGls;
        }
        public virtual string HintStr()
        {
            return " ";
        }
        protected int Item_ValChangeEvent(TagItem Item_, ushort[] New_, ushort[] Old_, ushort[] Curn_)
        {
            PreUnitState_Update(Item_);
            return 0;
        }
        protected int StayT_Event(ThreadTimer t_)
        {
            int no = Para.IOModuleNo * 10 + Para.GlassExistAddr;
            App.Alarm.Set(1, (uint)no, true, Para.ID + " " + Para.Group + " time over");
            return 0;
        }
        protected int StateFilterT_Event(ThreadTimer t_)
        {
            UnitState = PreUintState;
            return 0;
        }
    }
    public class Unit_GlassExist : EqUnit
    {
        public TagItem GlassExistIt;
        public Unit_GlassExist(BC bc_, UnitPara para_)
        : base(bc_, para_)
        {
            try
            {
                GlassExistIt = bc_.GetIt(Para.IOModuleNo, Para.GlassExistAddr);
                GlassExistIt.ValChangeEvent += new TagItem.ValChangeEventHandler(Item_ValChangeEvent);
                GlassExistIt.Hint = para_.ID + ".GlassExist";
                if (Para.Group.EndsWith("SCR") == true)
                { StateFilterT.Interval = 3000; }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        protected override void PreUnitState_Update(TagItem Item_)//call after sensor change
        {
            GlassExist = GlassExistIt.BinValue == 1;
            if (GlassExist == true)
            { PreUintState = eState.Run; }
            else
            { PreUintState = eState.Idle; }
        }
        public override string HintStr()
        {
            string io = string.Format("IOModule:{0}", Para.IOModuleNo);
            io += string.Format("\nGlassExist:{0}", Para.GlassExistAddr);
            return io;
        }
    }
    public class Unit_Pin : EqUnit
    {
        public TagItem PinIt;
        public Unit_Pin(BC bc_, UnitPara para_)
        : base(bc_, para_)
        {
            try
            {
                PinIt = bc_.GetIt(Para.IOModuleNo, Para.PinAddr);
                PinIt.ValChangeEvent += new TagItem.ValChangeEventHandler(Item_ValChangeEvent);
                PinIt.Hint = para_.ID + ".Pin";
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        protected override void PreUnitState_Update(TagItem Item_)//call after sensor change
        {
            GlassExist = PinIt.BinValue == 0;
            if (GlassExist == true)
            { PreUintState = eState.Run; }
            else
            { PreUintState = eState.Idle; }
        }
        public override string HintStr()
        {
            string io = string.Format("IOModule:{0}", Para.IOModuleNo);
            io += string.Format("\nPin:{0}", Para.PinAddr);
            return io;
        }
    }
    public class Unit_DP : EqUnit
    {
        public TagItem Cab1It;
        public TagItem Cab2It;

        public Unit_DP(BC bc_, UnitPara para_)
             : base(bc_, para_)
        {
            try
            {
                Cab1It = bc_.GetIt(Para.IOModuleNo, Para.Cab1Addr);
                Cab2It = bc_.GetIt(Para.IOModuleNo, Para.Cab2Addr);
                Cab1It.ValChangeEvent += new TagItem.ValChangeEventHandler(Item_ValChangeEvent);
                Cab2It.ValChangeEvent += new TagItem.ValChangeEventHandler(Item_ValChangeEvent);
                Cab1It.Hint = para_.ID + ".Cab1 GlassExist";
                Cab2It.Hint = para_.ID + ".Cab2 GlassExist";
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        protected override void PreUnitState_Update(TagItem Item_)
        {
            if (Item_ == Cab1It && Item_.BinValue == 0)                     //robot put glass to DP
            { GlassExist = true; }
            else if (Item_ == Cab2It && Item_.BinValue == 1)                //robot get glass from DP
            { GlassExist = false; }

            if (GlassExist == true)
            { PreUintState = eState.Run; }
            else
            { PreUintState = eState.Idle; }
        }
        public override string HintStr()
        {
            string io = string.Format("IOModule:{0}", Para.IOModuleNo);
            io += string.Format("\nCab1:{0}", Para.Cab1Addr);
            io += string.Format("\nCab2:{0}", Para.Cab2Addr);
            return io;
        }
    }
    public class Unit_Shutter : EqUnit
    {
        enum eShutter { Open = 0, Close = 1 }
        enum ePin { Down = 0, Up = 1 }

        public TagItem ShutterIt;
        public TagItem PinIt;
        public Unit_Shutter(BC bc_, UnitPara para_)
             : base(bc_, para_)
        {
            try
            {
                ShutterIt = bc_.GetIt(Para.IOModuleNo, Para.ShutterAddr);
                PinIt = bc_.GetIt(Para.IOModuleNo, Para.PinAddr);
                ShutterIt.ValChangeEvent += new TagItem.ValChangeEventHandler(Item_ValChangeEvent);
                PinIt.ValChangeEvent += new TagItem.ValChangeEventHandler(Item_ValChangeEvent);

                ShutterIt.Hint = para_.ID + ".Shutter";
                PinIt.Hint = para_.ID + ".Pin";
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        protected override void PreUnitState_Update(TagItem Item_)
        {
            try
            {
                if (PinIt.BinValue == (uint)ePin.Down && ShutterIt.BinValue == (uint)eShutter.Close)
                { GlassExist = true; }
                else
                { GlassExist = false; }

                bool pinUp = PinIt.BinValue == (uint)ePin.Up;
                bool doorClose = ShutterIt.BinValue == (uint)eShutter.Close;

                if (pinUp == true && doorClose == true)//11
                { PreUintState = eState.Idle; }
                else if (pinUp == true && doorClose == false)//10
                { PreUintState = eState.ExRq; }

                else if (pinUp == false && doorClose == true)//01
                { PreUintState = eState.Run; }
                //else//00
                //{ PreUintState = eState.UdRq; }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        protected override void StateChanged(eState newSt_, eState oldSt_)
        {
            try
            {
                if (newSt_ == eState.Run)       //01
                {
                    if (CurnGlass != null && CurnGlass.StepNo >= 0)
                    {
                        CurnGlass.DwellTime[CurnGlass.StepNo] = StayT.AccTime;//record  DwellTime
                    }
                    StayT.Enable(false);        //disable DwellTime
                    GlassOut();//for exchange


                    GlassIn();
                    StayT.Enable(true, 0x7FFFFFFFFFFFFFFF);//enabled ProcTime
                }
                else if (newSt_ == eState.ExRq) //(10)
                {
                    if (CurnGlass != null && CurnGlass.StepNo >= 0)
                    {
                        CurnGlass.ProcTime[CurnGlass.StepNo] = StayT.AccTime;//record  ProcTime
                        StayT.Enable(true, CurnGlass.RcpBody.Steps[CurnGlass.StepNo].StayOverTime);////enabled DwellTime
                    }
                }
                else //Idle(11)
                {
                    if (CurnGlass != null && CurnGlass.StepNo >= 0)
                    {
                        CurnGlass.DwellTime[CurnGlass.StepNo] = StayT.AccTime; //record  DwellTime
                    }
                    StayT.Enable(false);//disable DwellTime
                    GlassOut();
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public override string HintStr()
        {
            string io = string.Format("IOModule:{0}", Para.IOModuleNo);
            io += string.Format("\nShutter:{0}", Para.ShutterAddr);
            io += string.Format("\nPin:{0}", Para.PinAddr);
            return io;
        }
    }
}
