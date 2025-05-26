using CIM.UILog;
using Strong;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Collections.ObjectModel;
using System.Windows;
using CIM.Lib.Model;
using System.Threading.Tasks;

namespace CIM.BC
{
    public class EQ_HS
    {
        public enum eEqStatus : int { LOST, UP, TEST, DOWN, PM, FAC, OFF };

        public enum eState
        {
            LOST = 0,
            UP = 1,
            TEST = 2,
            DOWN = 3,
            PM = 4,
            FAC = 5,
            OFF = 6
        }

        public class State
        {
            public eState state { get; set; }
            public int time { get; set; }
        }

        /// <summary>
        /// PLC1, 2, 3...
        /// </summary>
        public string Name { set; get; }

        #region INIT
        #region Property
        public string MemoryPath = "CIM.BC.Memory.";
        public LogWriter LogFile = new LogWriter();
        public uint EqNo = 0;
        public uint Node { get; set; }
        public uint AddrBase = 0x0;
        public uint MemLength = 0x1FFF;
        public frmEqLog frmEqLog;
        public BC Bc;
        #endregion Property

        public EQ_HS(BC bc, uint eqNo, string ID, uint node, uint addrBase, uint memLength)
        {
            try
            {
                Name = ID;
                Bc = bc;
                EqNo = eqNo;
                Node = node;
                AddrBase = addrBase;
                MemLength = memLength;
                LogFile.PathName = App.sSysDir + $@"\LogFile\EQ\{Name}";
                LogFile.sHead = Name;

                MemAddrInitial();
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex);
            }
        }
        ~EQ_HS()
        {
        }

        #region MEMGRUOP
        public MemGroup DateTime;               //ES01
        public MemGroup EqStatus;               //ES02
        public MemGroup UnitStatus;             //ES03
        public MemGroup AlarmReport;            //ES04
        public MemGroup WorkDataReceiveReport;  //EW03
        public MemGroup WorkDataSendReport;     //EW04
        public MemGroup WorkDataRemoveReport;   //EW07
        public MemGroup WorkDataUpdateReport;   //EW08
        public MemGroup UtilityData;            //ED01
        public MemGroup ProcessDataReport;      //ED03
        public MemGroup RcpDataReport;          //ED04
        public MemGroup PositionData;           //EP01


        private void MemAddrInitial()
        {
            try
            {
                DateTime = App.PlcDevice.AddMemGroup_CSVStream(Name, "Data Time", LoadStreamResouce(MemoryPath + "ES01.csv"), MemBank.eBank.Wreg, (int)AddrBase, true);
                EqStatus = App.PlcDevice.AddMemGroup_CSVStream(Name, "Equipment Status", LoadStreamResouce(MemoryPath + "ES02.csv"), MemBank.eBank.Wreg, (int)AddrBase, true);
                UnitStatus = App.PlcDevice.AddMemGroup_CSVStream(Name, "Unit Status", LoadStreamResouce(MemoryPath + "ES03.csv"), MemBank.eBank.Wreg, (int)AddrBase, true);
                AlarmReport = App.PlcDevice.AddMemGroup_CSVStream(Name, "Alarm Report", LoadStreamResouce(MemoryPath + "ES04.csv"), MemBank.eBank.Wreg, (int)AddrBase, true);
                WorkDataReceiveReport = App.PlcDevice.AddMemGroup_CSVStream(Name, "Work Data Receive Report", LoadStreamResouce(MemoryPath + "EW03.csv"), MemBank.eBank.Wreg, (int)AddrBase, true);
                WorkDataSendReport = App.PlcDevice.AddMemGroup_CSVStream(Name, "Work Data Send Report", LoadStreamResouce(MemoryPath + "EW04.csv"), MemBank.eBank.Wreg, (int)AddrBase, true);
                WorkDataRemoveReport = App.PlcDevice.AddMemGroup_CSVStream(Name, "Work Data Remove Report", LoadStreamResouce(MemoryPath + "EW07.csv"), MemBank.eBank.Wreg, (int)AddrBase, true);
                WorkDataUpdateReport = App.PlcDevice.AddMemGroup_CSVStream(Name, "Work Data Update Report", LoadStreamResouce(MemoryPath + "EW08.csv"), MemBank.eBank.Wreg, (int)AddrBase, true);
                UtilityData = App.PlcDevice.AddMemGroup_CSVStream(Name, "Utility Data", LoadStreamResouce(MemoryPath + "ED01.csv"), MemBank.eBank.Wreg, (int)AddrBase, true);
                ProcessDataReport = App.PlcDevice.AddMemGroup_CSVStream(Name, "Process Data Report", LoadStreamResouce(MemoryPath + "ED03.csv"), MemBank.eBank.Wreg, (int)AddrBase, true);
                RcpDataReport = App.PlcDevice.AddMemGroup_CSVStream(Name, "Recipe Data Report", LoadStreamResouce(MemoryPath + "ED04.csv"), MemBank.eBank.Wreg, (int)AddrBase, true);
                PositionData = App.PlcDevice.AddMemGroup_CSVStream(Name, "Position Data", LoadStreamResouce(MemoryPath + "EP01.csv"), MemBank.eBank.Wreg, (int)AddrBase, true);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex);
            }
        }

        public static Stream LoadStreamResouce(string filename)     //new unit that included MemGroup
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(filename);
                return stream;
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex);
            }
            return null;
        }
        #endregion MEMGRUOP

        public void Initial()
        {
            try
            {
                App.Alarm.UnitNameRegister(Node, Name);
                LinkIndexEvent();
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex);
            }
        }

        private void LinkIndexEvent()
        {
            try
            {
                List<TagItem> ItemList = new List<TagItem>();
                foreach (MemGroup mg in App.PlcDevice.MemGroupList.FindAll(group => group.Owner == Name))
                {
                    if (mg.AddrBase < AddrBase) continue;
                    if (mg.AddrBase > (MemLength + AddrBase - 1)) continue;
                    if (mg.MemBank().Bank != MemBank.eBank.Wreg) continue;

                    ItemList.Clear();
                    mg.TagSearchWTPreFix("Index", ItemList);

                    foreach (TagItem Item in ItemList)
                    {
                        Item.ValChangeEvent += Idx_Event;   //for Index
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex);
            }
        }
        #endregion INIT

        private int Idx_Event(TagItem item, ushort[] newValue, ushort[] oldValue, ushort[] curnValue)
        {
            if (item.Mg.Owner == Name)
            {
                CommonMethods.MGLog(LogFile, item); //BC的 Idx_Event 包含CommonMethods.cs
            }

            try
            {
                if (item.Mg == AlarmReport)     //Alarm Report 事件
                {
                    foreach (TagItem alarmCode in AlarmReport.ItemList.FindAll(tagItem => tagItem.Name.EndsWith("Code")))
                    {
                        //只抓有填 Alarm Code 的出來
                        if (alarmCode.BinValue == 0) continue;

                        //取得相關的 Item 名稱
                        string itemPrefix = alarmCode.Name.Split('_')[0];
                        string itemName_Unit = $"{itemPrefix}_Unit";
                        string itemName_Set = $"{itemPrefix}_Set";

                        bool bOccured = AlarmReport[itemName_Set].BinValue == 1;    //1: Set	2: Clear

                        App.Alarm.Set(AlarmReport[itemName_Unit].BinValue, alarmCode.BinValue, bOccured);

                        string EqpID = Name;
                        string status;      //是給1,2 還是Set, Clear??
                        string Message = null;
                        string UserName = "username";
                        
                        switch (AlarmReport[itemName_Set].BinValue)
                        {
                            case 1:
                                status = "Set";
                                break;
                            case 2:
                                status = "Clear";
                                break;
                            default:
                                status = "Default status";
                                break;
                        }
                        
                        Task<(bool? success, string message)> task = WcfClient.EqpAlarmReport(itemName_Unit, status, Message, UserName);
                        task.ContinueWith(delegate
                        {
                            MessageBox.Show($"{task.Result.success}{Environment.NewLine}{task.Result.message}");
                        });
                    }
                }
                //其他 Memory Group
                else if (item.Mg == DateTime)     //ES01
                {
                    string sDateTime = DateTime["DateTime"].StringValue;
                }

                else if (item.Mg == EqStatus)     //ES02
                {
                    uint uInlineMode = EqStatus["InlineMode"].BinValue;
                    uint uOperationMode = EqStatus["OperationMode"].BinValue;
                    uint uPlcLowBattery = EqStatus["PlcLowBattery"].BinValue;
                    uint uStopReceive = EqStatus["StopReceive"].BinValue;
                    uint uStatus = EqStatus["Status"].BinValue;
                    uint uHasWarning = EqStatus["HasWarning"].BinValue;
                    uint uHasAlarm = EqStatus["HasAlarm"].BinValue;
                    uint uCurnRcpNo = EqStatus["CurnRcpNo"].BinValue;
                    uint uTactTime = EqStatus["TactTime"].BinValue;
                    uint uWorkCount = EqStatus["WorkCount"].BinValue;
                    uint uHistoryCount = EqStatus["HistoryCount"].BinValue;

                    //bool bOperationMode = uOperationMode == 0;     //0:Manual  1:Auto
                    //bool bInlineMode = uInlineMode == 0;     //0:Offline  1:Inline
                    //bool bStopReceive = uStopReceive == 0;     //0:Release   1:Prohibit

                    //string sEqpID = Name;
                    //string username = "user";
                    //string Memo = null;     //紀錄變化??
                    //string status = null; //是給1,2,3 還是Idle, Run, Down??

                    //switch (uStatus)
                    //{
                    //    case 1:
                    //        status = "Idle";
                    //        break;
                    //    case 2:
                    //        status = "Run";
                    //        break;
                    //    case 3:
                    //        status = "Down";
                    //        break;
                    //    default:
                    //        Console.WriteLine("Default status");
                    //        break;
                    //}

                    //Task<(bool? success, string message)> task = WcfClient.EqpStatusChange(sEqpID, status, username, Memo);
                    //task.ContinueWith(delegate
                    //{
                    //    MessageBox.Show($"{task.Result.success}{Environment.NewLine}{task.Result.message}");
                    //});

                }

                else if (item.Mg == UnitStatus)     //ES03
                {
                    foreach (TagItem unitStatus in UnitStatus.ItemList)
                    {
                        string sUnitName = unitStatus.Name.Split('_')[0];   //Unit1
                        string sStatusName = unitStatus.Name;
                        uint uState = UnitStatus[sStatusName].BinValue;    //1:Idle    2:Run   3:Down
                        //??
                    }
                }

                else if (item.Mg == WorkDataReceiveReport)     //EW03 Work Data Send Report
                {
                    uint uTransferInfo = WorkDataReceiveReport["TransferInfo"].BinValue;
                    uint uWorkNo = WorkDataReceiveReport["WorkNo"].BinValue;
                    uint uCstSeq = WorkDataReceiveReport["CstSeq"].BinValue;
                    uint uSlotNo = WorkDataReceiveReport["SlotNo"].BinValue;
                    uint uGropuNo = WorkDataReceiveReport["GropuNo"].BinValue;
                    string sWorkID = WorkDataReceiveReport["WorkID"].StringValue;
                    string sLotID = WorkDataReceiveReport["LotID"].StringValue;
                    uint uRcpNo = WorkDataReceiveReport["RcpNo"].BinValue;
                    uint uQty = WorkDataReceiveReport["Qty"].BinValue;

                    string stageName = Name;
                    string startTime = DateTime.ToString();     //??
                    string endTime = DateTime.ToString();     //??
                    string username = "user";
                    string LensID = "1";    //??

                    Task<(bool? success, string message)> task = WcfClient.LotLensTrackingReport(sLotID, LensID, stageName, username, 1, string.Empty, startTime, endTime);
                    task.ContinueWith(delegate
                    {
                        MessageBox.Show($"{task.Result.success}{Environment.NewLine}{task.Result.message}");
                    });

                    // Data add to DB
                    SQLiteClient sc = new SQLiteClient(); 
                    sc.AddData(sLotID, LensID, Name, "IN", startTime);      //LOG_TIME 要填什麼時間??
                }

                else if (item.Mg == WorkDataSendReport)     //EW04 Wrok Transfer-Send
                {
                    uint uTransferInfo = WorkDataSendReport["TransferInfo"].BinValue;
                    uint uWorkNo = WorkDataSendReport["WorkNo"].BinValue;
                    uint uCstSeq = WorkDataSendReport["CstSeq"].BinValue;
                    uint uSlotNo = WorkDataSendReport["SlotNo"].BinValue;
                    uint uGropuNo = WorkDataSendReport["GropuNo"].BinValue;
                    string sWorkID = WorkDataSendReport["WorkID"].StringValue;
                    string sLotID = WorkDataSendReport["LotID"].StringValue;
                    uint uRcpNo = WorkDataSendReport["RcpNo"].BinValue;
                    uint uQty = WorkDataSendReport["Qty"].BinValue;

                    string stageName = Name;
                    string startTime = DateTime.ToString();     //??
                    string endTime = DateTime.ToString();     //??
                    string username = "user";
                    string LensID = "1";

                    Task<(bool? success, string message)> task = WcfClient.LotLensTrackingReport(sLotID, LensID, stageName, username, 1, string.Empty, startTime, endTime);
                    task.ContinueWith(delegate
                    {
                        MessageBox.Show($"{task.Result.success}{Environment.NewLine}{task.Result.message}");

                    });

                    // Data add to DB
                    SQLiteClient sc = new SQLiteClient();
                    sc.AddData(sLotID, LensID, Name, "OUT", startTime);
                }

                else if (item.Mg == WorkDataRemoveReport)     //EW07
                {
                    uint uAction = WorkDataRemoveReport["Action"].BinValue;
                    uint uWorkNo = WorkDataRemoveReport["WorkNo"].BinValue;
                    uint uCstSeq = WorkDataRemoveReport["CstSeq"].BinValue;
                    uint uSlotNo = WorkDataRemoveReport["SlotNo"].BinValue;
                    uint uGropuNo = WorkDataRemoveReport["GropuNo"].BinValue;
                    string sWorkID = WorkDataRemoveReport["WorkID"].StringValue;
                    string sLotID = WorkDataRemoveReport["LotID"].StringValue;
                    uint uRcpNo = WorkDataRemoveReport["RcpNo"].BinValue;
                    uint uQty = WorkDataRemoveReport["Qty"].BinValue;
                    string sOperatorID = WorkDataRemoveReport["OperatorID"].StringValue;
                    uint uReasonCode = WorkDataRemoveReport["ReasonCode"].BinValue;
                }

                else if (item.Mg == WorkDataUpdateReport)     //EW08
                {
                    uint uWorkNo = WorkDataUpdateReport["WorkNo"].BinValue;
                    uint uCstSeq = WorkDataUpdateReport["CstSeq"].BinValue;
                    uint uSlotNo = WorkDataUpdateReport["SlotNo"].BinValue;
                    uint uGropuNo = WorkDataUpdateReport["GropuNo"].BinValue;
                    string sWorkID = WorkDataUpdateReport["WorkID"].StringValue;
                    string sLotID = WorkDataUpdateReport["LotID"].StringValue;
                    uint uRcpNo = WorkDataUpdateReport["RcpNo"].BinValue;
                    uint uQty = WorkDataUpdateReport["Qty"].BinValue;
                }

                else if (item.Mg == UtilityData)     //ED01
                {
                    uint uItems = UtilityData["Items"].BinValue;
                }

                else if (item.Mg == ProcessDataReport)     //ED03
                {
                    uint uWorkNo = ProcessDataReport["WorkNo"].BinValue;
                    uint uCstSeq = ProcessDataReport["CstSeq"].BinValue;
                    uint uSlotNo = ProcessDataReport["SlotNo"].BinValue;
                    uint uGropuNo = ProcessDataReport["GropuNo"].BinValue;
                    string sWorkID = ProcessDataReport["WorkID"].StringValue;
                    string sLotID = ProcessDataReport["LotID"].StringValue;
                    uint uRcpNo = ProcessDataReport["RcpNo"].BinValue;
                    uint uQty = ProcessDataReport["Qty"].BinValue;
                    uint uItems = ProcessDataReport["Items"].BinValue;
                }

                else if (item.Mg == RcpDataReport)     //ED04
                {
                    uint uReportType = RcpDataReport["ReportType"].BinValue;     //1:Reply   2:Modify
                    uint uModifyStatus = RcpDataReport["ModifyStatus"].BinValue;     //1:Add New   2:Change   3.Delete (Recipe)
                    uint uRcpNo = RcpDataReport["RcpNo"].BinValue;
                    uint uItems = RcpDataReport["Items"].BinValue;
                    //??
                }

                else if (item.Mg == PositionData)     //EP01
                {
                    foreach (TagItem positionData in PositionData.ItemList)
                    {
                        string sworkNumber = positionData.Name;
                        uint uWorkNo = PositionData[sworkNumber].BinValue;
                        //這裡不用再分 Cst Seq., Group No., Slot No.??
                        //Excel中有細分
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(ex);
            }
            return 0;
        }
    }
}
