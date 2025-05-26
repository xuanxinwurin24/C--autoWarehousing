using CIM;
using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.BC
{
    public class NikonPsetup
    {
        bool psetupState = false;
        public bool PsetupState
        {
            get { return psetupState; }
            set
            {
                if (value == psetupState) { return; }
                psetupState = value;
                StateFilterT.Enable(true);
                App.Bc.LogFile.AddString(App.SysPara.NikonID + ",PsetupState:" + psetupState.ToString());
            }
        }
        int StateFilterT_Event(ThreadTimer t_)
        {
            App.SecsMain.SecsCenter.S6F111_EqStatusChange(true, psetupState == true ? "P" : "R");//S6F11 Change to S6F111                
            return 0;
        }
        ThreadTimer StateFilterT;

        public TagItem LoadHandMaskDectOn;
        public TagItem UnloadHandMaskStage;
        public TagItem UnloadHandRotationTurnPosi;
        public TagItem UnloadHandMaskDectOn;
        public TagItem UnloadHandRotationTurnOpen;

        public NikonPsetup(BC bc_, Sensor[] s_)
        {
            try
            {
                LoadHandMaskDectOn = bc_.GetIt(s_[0].IOModuleNo, s_[0].Addr);
                UnloadHandMaskDectOn = bc_.GetIt(s_[1].IOModuleNo, s_[1].Addr);
                UnloadHandRotationTurnPosi = bc_.GetIt(s_[2].IOModuleNo, s_[2].Addr);
                UnloadHandMaskStage = bc_.GetIt(s_[3].IOModuleNo, s_[3].Addr);
                UnloadHandRotationTurnOpen = bc_.GetIt(s_[4].IOModuleNo, s_[4].Addr);

                LoadHandMaskDectOn.ValChangeEvent += new TagItem.ValChangeEventHandler(Item_ValChangeEvent);
                UnloadHandMaskStage.ValChangeEvent += new TagItem.ValChangeEventHandler(Item_ValChangeEvent);
                UnloadHandRotationTurnPosi.ValChangeEvent += new TagItem.ValChangeEventHandler(Item_ValChangeEvent);
                UnloadHandMaskDectOn.ValChangeEvent += new TagItem.ValChangeEventHandler(Item_ValChangeEvent);
                UnloadHandRotationTurnOpen.ValChangeEvent += new TagItem.ValChangeEventHandler(Item_ValChangeEvent);

                LoadHandMaskDectOn.Hint = "LoadHandMaskDectOn";
                UnloadHandMaskDectOn.Hint = "UnloadHandMaskDectOn";
                UnloadHandRotationTurnPosi.Hint = "UnloadHandRotationTurnPosi";
                UnloadHandMaskStage.Hint = "UnloadHandMaskStage";
                UnloadHandRotationTurnOpen.Hint = "UnloadHandRotationTurnOpen";

                StateFilterT = App.MainThread.TimerCreate("StateFilterT", 1000, StateFilterT_Event, ThreadTimer.eType.Hold);
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public void Initial()
        {
            StateFilterT.Enable(true);
        }
        int Item_ValChangeEvent(TagItem Item_, ushort[] New_, ushort[] Old_, ushort[] Curn_)
        {
            try
            {
                if (LoadHandMaskDectOn.BinValue == 0 && UnloadHandMaskDectOn.BinValue == 0 && UnloadHandRotationTurnPosi.BinValue == 1 && UnloadHandMaskStage.BinValue == 1)
                {
                    PsetupState = true;
                }
                if (LoadHandMaskDectOn.BinValue == 1 && UnloadHandMaskDectOn.BinValue == 1 && UnloadHandRotationTurnPosi.BinValue == 0 && UnloadHandMaskStage.BinValue == 1)
                {
                    PsetupState = false;
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
            return 0;
        }
    }
    public class CoaterPsetup
    {
        RecipeBody recipeBody;
        public RecipeBody RecipeBody
        {
            get { return recipeBody; }
            set
            {
                if (value == recipeBody)
                { ChangeRecipe(recipeBody, false); return; }
                recipeBody = value;
                ChangeRecipe(recipeBody);
                App.Bc.LogFile.AddString("RecipeNo:" + recipeBody.ID);
            }

        }


        bool psetupEnable = true;
        public bool psetupState = false;
        public bool PsetupState
        {
            get { return psetupState; }
            set
            {
                if (value == psetupState) { return; }
                psetupState = value;
                StateFilterT.Enable(true);
                App.Bc.LogFile.AddString(App.SysPara.TLCDID + ",PsetupState:" + psetupState.ToString());
            }
        }
        protected int StateFilterT_Event(ThreadTimer t_)
        {
            if (psetupEnable == true)
            { App.SecsMain.SecsCenter.S6F111_EqStatusChange(false, psetupState == true ? "P" : "R"); }//S6F11 Change to S6F111                
            return 0;
        }
        ThreadTimer StateFilterT;

        public List<TagItem> SensorIt;// = new List<TagItem>();

        public CoaterPsetup(BC bc_, List<TagItem> lst_)
        {
            try
            {
                int i = 0;
                SensorIt = lst_;
                foreach (TagItem it in SensorIt)
                {
                    it.ValChangeEvent += new TagItem.ValChangeEventHandler(Item_ValChangeEvent);
                    ThreadTimer t = App.MainThread.TimerCreate("Sensors" + i.ToString(), 9999000, PSetUpT_Event, ThreadTimer.eType.Hold);
                    it.Tag = t;
                    t.Tag = it;
                }
                StateFilterT = App.MainThread.TimerCreate("StateFilterT", 3000, StateFilterT_Event, ThreadTimer.eType.Hold);
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        public void Initial()
        {
            StateFilterT.Enable(true);
            foreach (TagItem it in SensorIt)
            {
                ThreadTimer t = (ThreadTimer)it.Tag;
                t.Enable(true);
                t.ExtraData[0] = 0;                     //0:Psetup false
            }
        }
        int PSetUpT_Event(ThreadTimer t_)
        {
            try
            {
                t_.ExtraData[0] = 1;
                foreach (TagItem it in SensorIt)
                {
                    ThreadTimer t = (ThreadTimer)it.Tag;
                    if (t.ExtraData[0] == 0 && t.ExtraData[1] == 1)            //0:Psetup false && PsetUpEnable == true
                    { PsetupState = false; return 0; }
                }
                PsetupState = true;
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
            return 0;
        }
        int Item_ValChangeEvent(TagItem Item_, ushort[] New_, ushort[] Old_, ushort[] Curn_)
        {
            try
            {
                ThreadTimer t = (ThreadTimer)Item_.Tag;
                if (Item_.ExtraData[1] == 0) //PsetupEnable == false 
                    return 0;
                if (Item_.BinValue == 1 )
                {
                    PsetupState = false;
                    t.ExtraData[0] = 0;                 //0:Psetup false
              
                    t.Enable(false);
                }
                else
                {
                    //t.ExtraData[0] = 1;               //1:Psetup true  NOT CORRECT
                    t.Enable(true);
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
            return 0;
        }
        void ChangeRecipe(RecipeBody rcpBody_, bool RstT_ = true)
        {
            try
            {
                foreach (TagItem it in SensorIt)
                {
                    ThreadTimer t = (ThreadTimer)it.Tag;
                    if (RstT_)
                    { t.Enable(false); }
                }

                psetupEnable = rcpBody_.PsetupEnable;
                for (int i = 0; i < SensorIt.Count; i++)
                {
                    ThreadTimer t = (ThreadTimer)(SensorIt[i].Tag);
                    t.Interval = rcpBody_.PsetupTime[i] * 1000;
                    SensorIt[i].ExtraData[1] = rcpBody_.PsetEnable[i];
                    t.ExtraData[1] = rcpBody_.PsetEnable[i];
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
    }
}
