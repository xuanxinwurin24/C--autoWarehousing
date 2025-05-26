using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Data;
using CIM.Lib.Model;

namespace CIM.BC
{
    public class TaskCtrl : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName_ = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName_));
        }
        #region declare
        public enum eTASK_Flow { eNone = 0, eSendToSTK, IDLE, SendCmd, Action, Complete = 77, OutToLifter = 88, NG = 99 };
        public enum eTempStage_Status { eNone = 0, eAssign, eInTemp, eOutTemp };
        public enum eShuttleCarStatus { eIDLE = 1, eBUSY, eDOWN, eCHARGING };

        public SQL_HS LIFTER_SQLHS;
        #endregion declare


        public TaskCtrl()
        {
            LIFTER_SQLHS = new SQL_HS();
        }
        
        public void TaskListCheck()
        {
            if (!App.Bc.ShuttleCar1.bONLINE) // || !App.Bc.STK.bONLINE 
                return;

            DataTable dt_Task = App.Local_SQLServer.SelectDB("*", "[TASK] order by [PRIORITY] DESC", ""); //先抓所有任務
            string sSQL;
            foreach (DataRow dr_Task in dt_Task.Rows)
            {
                string sBOX_ID = dr_Task["BOX_ID"].ToString().Trim();
                string sCOMMAND_ID = dr_Task["COMMAND_ID"].ToString().Trim();
                string sCAR_ID = "", sFind_ShuttleCar = "";
                string STK_CMD_RESULT = "0", SHUTTLECAR_CMD_RESULT = "0";
                DataTable dt_temp;
                DataTable dt_stocker;
                string sSTK_Src = "", sSTK_SrcCell = "", sSTK_Tar = "", sSTK_TarCell = "";
                string sShuttleCar_Src = "", sShuttleCar_Tar = "";
                switch ((eTASK_Flow)dr_Task["STATUS"].ToString().ToIntDef())
                {
                    case eTASK_Flow.eSendToSTK: //只有出庫需要跑到這一步，先丟給STK，在跳到IDLE等待出STK
                        #region SendToSTK
                        bool eSendToSTK_Flag = false;
                        string commandid1 = string.Empty;
                        string commandid2 = string.Empty;
                        int iSrcLineNo_temp = App.Bc.GetCarousel_LineNo(dr_Task["SRC_POS"].ToString().Trim());
                        string tSQL = $"SELECT ACTION FROM [CAR_STATUS] WHERE CAR_ID='CAR00{iSrcLineNo_temp}'";
                        DataTable dt_STKCar=new DataTable();
                        App.STK_SQLServer.Query(tSQL, ref dt_STKCar);
                        foreach(DataRow dr_STKCar in dt_STKCar.Rows)
						{
							if (dr_STKCar["ACTION"].ToString().Trim() != "0")//查看該線該車是否為IDLE
							{
                                eSendToSTK_Flag = true;
                                break;
							}
						}
                        DataTable dt_Temp = App.Local_SQLServer.SelectDB("*", "[TASK] order by [PRIORITY] DESC", "");//刷新任務判定資料表
                        foreach (DataRow dr_temp in dt_Temp.Rows)
						{
                            commandid1 = dr_temp["BOX_ID"].ToString().Trim();
                            commandid2 = dr_Task["BOX_ID"].ToString().Trim();
                            if (dr_temp != dr_Task && (eTASK_Flow)dr_temp["STATUS"].ToString().ToIntDef() > eTASK_Flow.eSendToSTK) //如果任務列表中有不是當前任務的其他任務 且任務執行中
                            {
                                if(dr_temp["DIRECTION"].ToString().Trim() == "OUT" && dr_temp["OUT_STOCKER"].ToString().Trim() == "0")//出庫且OUT_STOCKER為0
                                {
                                    eSendToSTK_Flag = true;
                                    break;
                                }
                                else if(dr_temp["DIRECTION"].ToString().Trim() == "MOVE")//調儲
								{
                                    eSendToSTK_Flag = true;
                                    break;
                                }
                            }
						}
                        if (eSendToSTK_Flag)
                            break;
                        if (dr_Task["DIRECTION"].ToString().Trim() == "OUT") //出庫任務
                        {
                            if (App.Bc.wAux["isOnlyDemo"].BinValue == 1) //模擬模式，出庫直接當作已經出STK
                            {
                                sSQL = $"UPDATE [TASK] SET OUT_STOCKER='1', [STATUS] = '{(int)eTASK_Flow.IDLE}' WHERE BOX_ID='{sBOX_ID}'";
                                App.Local_SQLServer.NonQuery(sSQL);
                                break;
                            }
                            int iSrcLineNo = App.Bc.GetCarousel_LineNo(dr_Task["SRC_POS"].ToString().Trim());
                            //---給STK的命令 從STAGE放到目標Carousel & Cell---
                            CAR_ACTION_CMD STK_Car_Cmd = new CAR_ACTION_CMD()
                            {
                                ACTION = "TRANSFER",
                                CAR_ID = $"CAR00{iSrcLineNo}",
                                BOX_ID = dr_Task["BOX_ID"].ToString().Trim(),
                                SOURCE = dr_Task["SRC_POS"].ToString().Trim(), //來源Carousel
                                S_CELL_ID = dr_Task["SRC_CELL_ID"].ToString().Trim(),
                                TARGET = $"B-00{iSrcLineNo}", //目標Stage
                                T_CELL_ID = "001", //目標Stage固定001
                                BATCH_NO = dr_Task["BATCH_NO"].ToString().Trim(),
                                SOTERIA = dr_Task["SOTERIA"].ToString().Trim(),
                                CUSTOMER_ID = dr_Task["CUSTOMER_ID"].ToString().Trim()
                            };
                            App.Bc.STK.C010_CMD(dr_Task["COMMAND_ID"].ToString().Trim(), STK_Car_Cmd);
                            //---給STK的命令End---
                        }
                        else if (dr_Task["DIRECTION"].ToString().Trim() == "MOVE") //調儲任務
                        {
                            int iSrcLineNo = App.Bc.GetCarousel_LineNo(dr_Task["SRC_POS"].ToString().Trim());
                            int iTarLineNo = App.Bc.GetCarousel_LineNo(dr_Task["TAR_POS"].ToString().Trim());
                            if (iSrcLineNo == iTarLineNo) //目標是同一條Line的Carousel
                            {
                                if (App.Bc.wAux["isOnlyDemo"].BinValue == 1) //模擬模式，沒有STK，不用下
                                {
                                    break;
                                }
                                //---給STK的命令 從STAGE放到目標Carousel & Cell---
                                CAR_ACTION_CMD STK_Car_Cmd = new CAR_ACTION_CMD()
                                {
                                    ACTION = "TRANSFER",
                                    CAR_ID = $"CAR00{iSrcLineNo}",
                                    BOX_ID = dr_Task["BOX_ID"].ToString().Trim(),
                                    SOURCE = dr_Task["SRC_POS"].ToString().Trim(), //來源Carousel
                                    S_CELL_ID = dr_Task["SRC_CELL_ID"].ToString().Trim(),
                                    TARGET = dr_Task["TAR_POS"].ToString().Trim(), //目標Stage
                                    T_CELL_ID = dr_Task["TAR_CELL_ID"].ToString().Trim(), //目標Stage固定001
                                    BATCH_NO = dr_Task["BATCH_NO"].ToString().Trim(),
                                    SOTERIA = dr_Task["SOTERIA"].ToString().Trim(),
                                    CUSTOMER_ID = dr_Task["CUSTOMER_ID"].ToString().Trim()
                                };
                                App.Bc.STK.C010_CMD(dr_Task["COMMAND_ID"].ToString(), STK_Car_Cmd);
                                //---給STK的命令End---
                            }
                            else //不同Line Carousel , 要送到外面給穿梭車載到另外一條Line
                            {
                                if (App.Bc.wAux["isOnlyDemo"].BinValue == 1) //模擬模式，出庫直接當作已經出STK
                                {
                                    sSQL = $"UPDATE [TASK] SET OUT_STOCKER='1', [STATUS] = '{(int)eTASK_Flow.IDLE}' WHERE BOX_ID='{sBOX_ID}'";
                                    App.Local_SQLServer.NonQuery(sSQL);
                                    break;
                                }
                                //---給STK的命令 從STAGE放到目標Carousel & Cell---
                                CAR_ACTION_CMD STK_Car_Cmd = new CAR_ACTION_CMD()
                                {
                                    ACTION = "TRANSFER",
                                    CAR_ID = $"CAR00{iSrcLineNo}",
                                    BOX_ID = dr_Task["BOX_ID"].ToString().Trim(),
                                    SOURCE = dr_Task["SRC_POS"].ToString().Trim(), //來源Carousel
                                    S_CELL_ID = dr_Task["SRC_POS_CELL"].ToString().Trim(),
                                    TARGET = $"B-00{iSrcLineNo}", //目標Stage
                                    T_CELL_ID = "001", //目標Stage固定001
                                    BATCH_NO = dr_Task["BATCH_NO"].ToString().Trim(),
                                    SOTERIA = dr_Task["SOTERIA"].ToString().Trim(),
                                    CUSTOMER_ID = dr_Task["CUSTOMER_ID"].ToString().Trim()
                                };
                                App.Bc.STK.C010_CMD(dr_Task["COMMAND_ID"].ToString(), STK_Car_Cmd);
                                //---給STK的命令End---
                            }
                        }
                        sSQL = $"UPDATE [TASK] SET [STATUS] = '{(int)eTASK_Flow.IDLE}' WHERE [BOX_ID] = '{sBOX_ID}'";
                        App.Local_SQLServer.NonQuery(sSQL);
                        #endregion SendToSTK
                        break;
                    case eTASK_Flow.IDLE:
                        #region 入庫_IN
                        if (dr_Task["DIRECTION"].ToString().Trim() == "IN") //入庫任務
                        {
                            if (dr_Task["OUT_LIFTER"].ToString().Trim() == "1") //BOX已抵達LIFTER出貨
                            {
                                int iTarget_LineNo = App.Bc.GetCarousel_LineNo(dr_Task["TAR_POS"].ToString().Trim());

                                string sSrcPos = dr_Task["SRC_POS"].ToString().Trim();
                                if (sSrcPos == "LIFTER_A" && isB_Area(iTarget_LineNo)) //從LIFTER A入庫 但要去1~4 B車區域
                                {
                                    if ((eTempStage_Status)dr_Task["USE_TEMP_STATUS"].ToString().ToIntDef(0) == eTempStage_Status.eInTemp)
                                    {
                                        sFind_ShuttleCar = "B"; //已經在暫存位置，要從暫存區往B區 找B車
                                        sCAR_ID = $"CAR00{iTarget_LineNo}"; //要放到STK，對應的RGV車
                                        sSTK_Src = $"B-00{iTarget_LineNo}"; //STK STAGE來源
                                        sSTK_SrcCell = "001"; //STAGE CELL = 001
                                        sSTK_Tar = dr_Task["TAR_POS"].ToString().Trim();
                                        sSTK_TarCell = dr_Task["TAR_CELL_ID"].ToString().Trim();

                                        sShuttleCar_Src = "EXCHANGE"; //從交換平台
                                        sShuttleCar_Tar = $"STAGE{iTarget_LineNo}"; //放到STAGE
                                    }
                                    else
                                    {
                                        DataRow[] dr_OtherUseTempTASK = dt_Task.Select($"USE_TEMP_STATUS > {(int)eTempStage_Status.eNone} AND USE_TEMP_STATUS < {(int)eTempStage_Status.eOutTemp}");
                                        if (dr_OtherUseTempTASK.Length > 0) //有其他指定用到暫存區的任務，先不執行
                                            continue;
                                        sFind_ShuttleCar = "A"; //還沒到暫存區，先從LIFTER放到暫存區找A車
                                        sShuttleCar_Src = "LIFTER_A"; //從LIFTER
                                        sShuttleCar_Tar = "EXCHANGE"; //放到交換平台
                                        STK_CMD_RESULT = "1"; //還沒要放到STK，此次命令Pass Check STK Cmd
                                    }
                                }
                                else if (sSrcPos == "LIFTER_B" && isA_Area(iTarget_LineNo)) //從LIFTER B入庫 但要去5~7 A車區域
                                {
                                    if ((eTempStage_Status)dr_Task["USE_TEMP_STATUS"].ToString().ToIntDef(0) == eTempStage_Status.eInTemp)
                                    {
                                        sFind_ShuttleCar = "A"; //已經在暫存位置，要從暫存區往A區 找A車
                                        sCAR_ID = $"CAR00{iTarget_LineNo}"; //要放到STK，對應的RGV車
                                        sSTK_Src = $"B-00{iTarget_LineNo}"; //STK STAGE來源
                                        sSTK_SrcCell = "001"; //STAGE CELL = 001
                                        sSTK_Tar = dr_Task["TAR_POS"].ToString().Trim();
                                        sSTK_TarCell = dr_Task["TAR_CELL_ID"].ToString().Trim();

                                        sShuttleCar_Src = "EXCHANGE"; //從交換平台
                                        sShuttleCar_Tar = $"STAGE{iTarget_LineNo}"; //放到STAGE
                                    }
                                    else
                                    {
                                        DataRow[] dr_OtherUseTempTASK = dt_Task.Select($"USE_TEMP_STATUS > {(int)eTempStage_Status.eNone} AND USE_TEMP_STATUS < {(int)eTempStage_Status.eOutTemp}");
                                        if (dr_OtherUseTempTASK.Length > 0) //有其他指定用到暫存區的任務，先不執行
                                            continue;
                                        sFind_ShuttleCar = "B"; //還沒到暫存區，先從LIFTER放到暫存區找B車
                                        sShuttleCar_Src = "LIFTER_B"; //從LIFTER
                                        sShuttleCar_Tar = "EXCHANGE"; //放到交換平台
                                        STK_CMD_RESULT = "1"; //還沒要放到STK，此次命令Pass Check STK Cmd
                                    }
                                }
                                else //LIFTER_A 放A車 5~7區域 , LIFTER_B放B車1~4區域
                                {
                                    if (isB_Area(iTarget_LineNo)) //B車區域
                                    {
                                        sFind_ShuttleCar = "B";
                                        sShuttleCar_Src = "LIFTER_B"; //從LIFTER
                                        sShuttleCar_Tar = $"STAGE{iTarget_LineNo}"; //放到STAGE
                                    }
                                    else if(isA_Area(iTarget_LineNo)) //A車區域
                                    {
                                        sFind_ShuttleCar = "A";
                                        sShuttleCar_Src = "LIFTER_A"; //從LIFTER
                                        sShuttleCar_Tar = $"STAGE{iTarget_LineNo}"; //放到STAGE
                                    }
                                    else //找不到對應
                                    {
                                        sSQL = $"UPDATE TASK SET [STATUS] = '{(int)eTASK_Flow.NG}', [NG_REASON] = 'CAN NOT FOUND TARGET LINE NO' WHERE [COMMAND_ID] = '{sCOMMAND_ID}' AND [BOX_ID] = '{sBOX_ID}'";
                                        App.Local_SQLServer.NonQuery(sSQL);
                                        continue;
                                    }

                                    sCAR_ID = $"CAR00{iTarget_LineNo}"; //要放到STK，對應的RGV車
                                    sSTK_Src = $"B-00{iTarget_LineNo}"; //STK STAGE來源
                                    sSTK_SrcCell = "001"; //STAGE CELL = 001
                                    sSTK_Tar = dr_Task["TAR_POS"].ToString().Trim();
                                    sSTK_TarCell = dr_Task["TAR_CELL_ID"].ToString().Trim();
                                }

                                if (sCAR_ID != "" && STK_CMD_RESULT != "1") //有要放到STK而且此次命令是要放到STK(而不是到暫存區而已)，要確認STK是否忙碌
                                {
                                    if (dt_Task.Select($"DIRECTION = 'OUT' AND LINE_NO = {iTarget_LineNo}").Count() != 0) //搜尋任務列表內，同一個LineNo有無出庫任務, !=0表示STK有出庫任務在忙碌中
                                        continue;
                                }

                                dt_temp = App.Local_SQLServer.SelectDB("*", "CAR_STATUS", $"[WITH_LIFTER] = '{sFind_ShuttleCar}' AND [COMMAND_ID] = '' AND [STATUS] = 'IDLE' AND [BOX_ID] = ''"); //確認穿梭車是否IDLE無任務
                                dt_stocker = App.STK_SQLServer.SelectDB("*", "CAR_STATUS", $"[CAR_ID]='CAR00{iTarget_LineNo}' AND [ACTION]='0'");//
								if (dt_temp.Rows.Count == 0) //表示穿梭車有任務在忙碌中
                                    continue;
								//else if (dt_stocker.Rows.Count == 0 && App.Bc.wAux["isOnlyDemo"].BinValue!=1)//表示Stocker有任務在忙碌中
								//	continue;
								string sShuttleCar_ID = dt_temp.Rows[0]["CAR_ID"].ToString().Trim(); //穿梭車ID
                                

                                //---給穿梭車的命令 從暫存區放到Target Stage---
                                CAR_ACTION_CMD ShuttleCarCmd = new CAR_ACTION_CMD()
                                {
                                    ACTION = "TRANSFER",
                                    CAR_ID = sShuttleCar_ID,
                                    BOX_ID = sBOX_ID,
                                    SOURCE = sShuttleCar_Src,
                                    TARGET = sShuttleCar_Tar
                                };
                                /*
                                if (sShuttleCar_ID.IndexOf("1") != -1)
                                    App.Bc.ShuttleCar1.C010_CMD(sCOMMAND_ID, ShuttleCarCmd);
                                else if (sShuttleCar_ID.IndexOf("2") != -1)
                                    App.Bc.ShuttleCar2.C010_CMD(sCOMMAND_ID, ShuttleCarCmd);
                                */
                                App.Bc.ShuttleCar1.C010_CMD(sCOMMAND_ID, ShuttleCarCmd);
                                //---給穿梭車的命令End---
                                if (App.Bc.wAux["isOnlyDemo"].BinValue == 0) //非模擬模式，才要下命令給STK
                                {
                                    if (sCAR_ID != "")
                                    {
                                        //---給STK的命令 從STAGE放到目標Carousel & Cell---
                                        CAR_ACTION_CMD STK_Car_Cmd = new CAR_ACTION_CMD()
                                        {
                                            ACTION = "TRANSFER",
                                            CAR_ID = sCAR_ID,
                                            BOX_ID = sBOX_ID,
                                            SOURCE = sSTK_Src, //來源平台
                                            S_CELL_ID = sSTK_SrcCell, //交換平台固定001
                                            TARGET = sSTK_Tar, //目標Carousel
                                            T_CELL_ID = sSTK_TarCell, //目標CELL
                                            BATCH_NO = dr_Task["BATCH_NO"].ToString().Trim(),
                                            SOTERIA = dr_Task["SOTERIA"].ToString().Trim(),
                                            CUSTOMER_ID = dr_Task["CUSTOMER_ID"].ToString().Trim(),
                                        };
                                        App.Bc.STK.C010_CMD(sCOMMAND_ID, STK_Car_Cmd);
                                        //---給STK的命令End---
                                    }
                                }

                                //---更新到車輛命令列表---
                                sSQL = $"INSERT INTO [dbo].[CAR_CMD] ([COMMAND_ID], [SHUTTLECAR_SRC], [SHUTTLECAR_TAR], [SHUTTLECAR_CAR_ID], [SHUTTLECAR_CMD_RESULT], [STK_SRC], [STK_SRC_CELL], [STK_TAR], [STK_TAR_CELL], [STK_CAR_ID], [STK_CMD_RESULT]) " +
                                    $"VALUES (" +
                                    $"'{sCOMMAND_ID}', " +
                                    $"'{sShuttleCar_Src}', " +
                                    $"'{sShuttleCar_Tar}', " +
                                    $"'{sShuttleCar_ID}', " +
                                    $"'{SHUTTLECAR_CMD_RESULT}', " +
                                    $"'{sSTK_Src}', " +
                                    $"'{sSTK_SrcCell}', " +
                                    $"'{sSTK_Tar}', " +
                                    $"'{sSTK_TarCell}', " +
                                    $"'{sCAR_ID}', " +
                                    $"'{STK_CMD_RESULT}' )";
                                App.Local_SQLServer.NonQuery(sSQL);

                                //---更新任務狀態 RESET STK 與 SHUTTLE CAR命令狀態--
                                sSQL = $"UPDATE TASK SET [STATUS] = '{(int)eTASK_Flow.SendCmd}' WHERE [COMMAND_ID] = '{sCOMMAND_ID}' AND [BOX_ID] = '{sBOX_ID}'";
                                App.Local_SQLServer.NonQuery(sSQL);
                            }
                        }
                        #endregion 入庫_IN
                        #region 出庫_OUT
                        else if (dr_Task["DIRECTION"].ToString().Trim() == "OUT") //出庫任務
                        {
                            if (dr_Task["OUT_STOCKER"].ToString() == "1") //BOX已抵達STOCKER出貨位置
                            {
                                int iSource_LineNo = App.Bc.GetCarousel_LineNo(dr_Task["SRC_POS"].ToString().Trim());

                                string sTarPos = dr_Task["TAR_POS"].ToString().Trim();
                                if (sTarPos == "LIFTER_A" && isB_Area(iSource_LineNo)) //從1~4出庫 但要去LIFTER A
                                {
                                    if ((eTempStage_Status)dr_Task["USE_TEMP_STATUS"].ToString().ToIntDef(0) == eTempStage_Status.eInTemp)
                                    {
                                        sFind_ShuttleCar = "A"; //已經在暫存位置，要從暫存區往LIFTER_A 找A車

                                        sShuttleCar_Src = "EXCHANGE"; //從交換平台
                                        sShuttleCar_Tar = sTarPos; //放到LIFTER
                                    }
                                    else
                                    {
                                        DataRow[] dr_OtherUseTempTASK = dt_Task.Select($"USE_TEMP_STATUS > {(int)eTempStage_Status.eNone} AND USE_TEMP_STATUS < {(int)eTempStage_Status.eOutTemp}");
                                        if (dr_OtherUseTempTASK.Length > 0) //有其他指定用到暫存區的任務，先不執行
                                            continue;

                                        sFind_ShuttleCar = "B"; //還沒到暫存區，先從LIFTER放到暫存區找B車
                                        sShuttleCar_Src = $"STAGE{iSource_LineNo}"; //STAGE
                                        sShuttleCar_Tar = "EXCHANGE"; //放到交換平台
                                    }
                                }
                                else if (sTarPos == "LIFTER_B" && isA_Area(iSource_LineNo)) //從LIFTER B入庫 但要去5~7 A車區域
                                {
                                    if ((eTempStage_Status)dr_Task["USE_TEMP_STATUS"].ToString().ToIntDef(0) == eTempStage_Status.eInTemp)
                                    {
                                        sFind_ShuttleCar = "B"; //已經在暫存位置，要從暫存區往LIFTER_B 找B車
                                        sShuttleCar_Src = "EXCHANGE"; //從交換平台
                                        sShuttleCar_Tar = sTarPos; //放到LIFTER B
                                    }
                                    else
                                    {
                                        DataRow[] dr_OtherUseTempTASK = dt_Task.Select($"USE_TEMP_STATUS > {(int)eTempStage_Status.eNone} AND USE_TEMP_STATUS < {(int)eTempStage_Status.eOutTemp}");
                                        if (dr_OtherUseTempTASK.Length > 0) //有其他指定用到暫存區的任務，先不執行
                                            continue;
                                        sFind_ShuttleCar = "A"; //還沒到暫存區，先從LIFTER放到暫存區找B車
                                        sShuttleCar_Src = $"STAGE{iSource_LineNo}"; //從STAGE
                                        sShuttleCar_Tar = "EXCHANGE"; //放到交換平台
                                    }
                                }
                                else //LIFTER_A 放A車 5~7區域 , LIFTER_B放B車1~4區域
                                {
                                    if (isB_Area(iSource_LineNo)) //B車區域
                                    {
                                        sFind_ShuttleCar = "B";
                                        sShuttleCar_Src = $"STAGE{iSource_LineNo}"; //從STAGE
                                        sShuttleCar_Tar = sTarPos; //放到LIFTER
                                    }
                                    else if (isA_Area(iSource_LineNo)) //A車區域
                                    {
                                        sFind_ShuttleCar = "A";
                                        sShuttleCar_Src = $"STAGE{iSource_LineNo}"; //從STAGE
                                        sShuttleCar_Tar = sTarPos; //放到LIFTER
                                    }
                                    else //找不到對應
                                    {
                                        sSQL = $"UPDATE TASK SET [STATUS] = '{(int)eTASK_Flow.NG}', [NG_REASON] = 'CAN NOT FOUND TARGET LINE NO' WHERE [COMMAND_ID] = '{sCOMMAND_ID}' AND [BOX_ID] = '{sBOX_ID}'";
                                        App.Local_SQLServer.NonQuery(sSQL);
                                        continue;
                                    }
                                }
                                STK_CMD_RESULT = "1"; //出庫已出STK，此次命令Pass Check STK Cmd
                                dt_temp = App.Local_SQLServer.SelectDB("*", "CAR_STATUS", $"[WITH_LIFTER] = '{sFind_ShuttleCar}' AND [COMMAND_ID] = '' AND [STATUS] = 'IDLE' AND [BOX_ID] = ''"); //確認穿梭車是否IDLE無任務
								dt_stocker = App.STK_SQLServer.SelectDB("*", "CAR_STATUS", $"[CAR_ID]='CAR00{iSource_LineNo}' AND [ACTION]='0'");//確認Stocker是否IDLE
								if (dt_temp.Rows.Count == 0) //表示穿梭車有任務在忙碌中
                                    continue;
								//else if (dt_stocker.Rows.Count == 0 && App.Bc.wAux["isOnlyDemo"].BinValue != 1)//表示Stocker有任務在忙碌中
								//	continue;
								if (sShuttleCar_Tar != "EXCHANGE") //不是放到暫存區的，要確認LIFTER是否可以收，再搬
                                {
                                    DataTable dt_CheckLifter = App.Local_SQLServer.SelectDB("*", "STATUS", $"[NAME] = '{sShuttleCar_Tar}' AND [STATUS] = 'OK'"); //確認LIFTER可以收
                                    if (dt_CheckLifter.Rows.Count == 0) //表示LIFTER目前不能收
                                        continue;
                                }

                                string sShuttleCar_ID = dt_temp.Rows[0]["CAR_ID"].ToString().Trim(); //穿梭車ID

                                //---給穿梭車的命令 從暫存區放到Target Stage---
                                CAR_ACTION_CMD ShuttleCarCmd = new CAR_ACTION_CMD()
                                {
                                    ACTION = "TRANSFER",
                                    CAR_ID = sShuttleCar_ID,
                                    BOX_ID = sBOX_ID,
                                    SOURCE = sShuttleCar_Src,
                                    TARGET = sShuttleCar_Tar
                                };
                                /*
                                if (sShuttleCar_ID.IndexOf("1") != -1)
                                    App.Bc.ShuttleCar1.C010_CMD(sCOMMAND_ID, ShuttleCarCmd);
                                else if (sShuttleCar_ID.IndexOf("2") != -1)
                                    App.Bc.ShuttleCar2.C010_CMD(sCOMMAND_ID, ShuttleCarCmd);
                                */
                                App.Bc.ShuttleCar1.C010_CMD(sCOMMAND_ID, ShuttleCarCmd);
                                //---給穿梭車的命令End---

                                //---更新到車輛命令列表---
                                sSQL = $"INSERT INTO [dbo].[CAR_CMD] ([COMMAND_ID], [SHUTTLECAR_SRC], [SHUTTLECAR_TAR], [SHUTTLECAR_CAR_ID], [SHUTTLECAR_CMD_RESULT], [STK_SRC], [STK_SRC_CELL], [STK_TAR], [STK_TAR_CELL], [STK_CAR_ID], [STK_CMD_RESULT]) " +
                                    $"VALUES (" +
                                    $"'{sCOMMAND_ID}', " +
                                    $"'{sShuttleCar_Src}', " +
                                    $"'{sShuttleCar_Tar}', " +
                                    $"'{sShuttleCar_ID}', " +
                                    $"'{SHUTTLECAR_CMD_RESULT}', " +
                                    $"'{sSTK_Src}', " +
                                    $"'{sSTK_SrcCell}', " +
                                    $"'{sSTK_Tar}', " +
                                    $"'{sSTK_TarCell}', " +
                                    $"'{sCAR_ID}', " +
                                    $"'{STK_CMD_RESULT}' )";
                                App.Local_SQLServer.NonQuery(sSQL);

                                //---更新任務狀態 RESET STK 與 SHUTTLE CAR命令狀態--
                                sSQL = $"UPDATE TASK SET [STATUS] = '{(int)eTASK_Flow.SendCmd}' WHERE [COMMAND_ID] = '{sCOMMAND_ID}' AND [BOX_ID] = '{sBOX_ID}'";
                                App.Local_SQLServer.NonQuery(sSQL);
                            }
                        }
                        #endregion 出庫_OUT
                        #region 調儲_MOVE
                        else if (dr_Task["DIRECTION"].ToString().Trim() == "MOVE")//調儲 or 備貨  Carousel->Carousel
                        {
                            if (dr_Task["OUT_STOCKER"].ToString() == "1") //BOX已抵達STOCKER出貨位置
                            {
                                int iSource_LineNo = App.Bc.GetCarousel_LineNo(dr_Task["SRC_POS"].ToString().Trim());
                                int iTarget_LineNo = App.Bc.GetCarousel_LineNo(dr_Task["TAR_POS"].ToString().Trim());

                                string sTarPos = dr_Task["TAR_POS"].ToString().Trim();
                                if (isB_Area(iSource_LineNo) && isA_Area(iTarget_LineNo)) //從B 1~4出庫 但要去A 5~7區
                                {
                                    if ((eTempStage_Status)dr_Task["USE_TEMP_STATUS"].ToString().ToIntDef(0) == eTempStage_Status.eInTemp)
                                    {
                                        sFind_ShuttleCar = "A"; //已經在暫存位置，要從暫存區往A區 找A車

                                        sShuttleCar_Src = "EXCHANGE"; //從交換平台
                                        sShuttleCar_Tar = $"STAGE{iTarget_LineNo}"; //放到STK

                                        sCAR_ID = $"CAR00{iTarget_LineNo}"; //要放到STK，對應的RGV車
                                        sSTK_Src = $"B-00{iTarget_LineNo}"; //STK STAGE來源
                                        sSTK_SrcCell = "001"; //STAGE CELL = 001
                                        sSTK_Tar = dr_Task["TAR_POS"].ToString().Trim();
                                        sSTK_TarCell = dr_Task["TAR_CELL_ID"].ToString().Trim();
                                    }
                                    else
                                    {
                                        DataRow[] dr_OtherUseTempTASK = dt_Task.Select($"USE_TEMP_STATUS > {(int)eTempStage_Status.eNone} AND USE_TEMP_STATUS < {(int)eTempStage_Status.eOutTemp}");
                                        if (dr_OtherUseTempTASK.Length > 0) //有其他指定用到暫存區的任務，先不執行
                                            continue;

                                        sFind_ShuttleCar = "B"; //還沒到暫存區，找B車

                                        sShuttleCar_Src = $"STAGE{iTarget_LineNo}"; //從STAGE
                                        sShuttleCar_Tar = "EXCHANGE"; //放到交換平台
                                        STK_CMD_RESULT = "1"; //已出STK，此次命令Pass Check STK Cmd
                                    }
                                }
                                else if (isA_Area(iSource_LineNo) && isB_Area(iTarget_LineNo)) //從A 5~7出庫 但要去B 1~4區
                                {
                                    if ((eTempStage_Status)dr_Task["USE_TEMP_STATUS"].ToString().ToIntDef(0) == eTempStage_Status.eInTemp)
                                    {
                                        sFind_ShuttleCar = "B"; //已經在暫存位置，要從暫存區往B區 找B車

                                        sShuttleCar_Src = "EXCHANGE"; //從交換平台
                                        sShuttleCar_Tar = $"STAGE{iTarget_LineNo}"; //放到STK

                                        sCAR_ID = $"CAR00{iTarget_LineNo}"; //要放到STK，對應的RGV車
                                        sSTK_Src = $"B-00{iTarget_LineNo}"; //STK STAGE來源
                                        sSTK_SrcCell = "001"; //STAGE CELL = 001
                                        sSTK_Tar = dr_Task["TAR_POS"].ToString().Trim();
                                        sSTK_TarCell = dr_Task["TAR_CELL_ID"].ToString().Trim();
                                    }
                                    else
                                    {
                                        DataRow[] dr_OtherUseTempTASK = dt_Task.Select($"USE_TEMP_STATUS > {(int)eTempStage_Status.eNone} AND USE_TEMP_STATUS < {(int)eTempStage_Status.eOutTemp}");
                                        if (dr_OtherUseTempTASK.Length > 0) //有其他指定用到暫存區的任務，先不執行
                                            continue;
                                        sFind_ShuttleCar = "A"; //還沒到暫存區，找A車

                                        sShuttleCar_Src = $"STAGE{iTarget_LineNo}"; //從STAGE
                                        sShuttleCar_Tar = "EXCHANGE"; //放到交換平台
                                        STK_CMD_RESULT = "1"; //已出STK，此次命令Pass Check STK Cmd
                                    }
                                }
                                else //A車 5~7區域 , B車1~4區域 同區域
                                {
                                    if (isB_Area(iSource_LineNo)) //B車區域
                                    {
                                        sFind_ShuttleCar = "B";

                                    }
                                    else if (isA_Area(iSource_LineNo)) //A車區域
                                    {
                                        sFind_ShuttleCar = "A";
                                    }
                                    else //找不到對應
                                    {
                                        sSQL = $"UPDATE TASK SET [STATUS] = '{(int)eTASK_Flow.NG}', [NG_REASON] = 'CAN NOT FOUND TARGET LINE NO' WHERE [COMMAND_ID] = '{sCOMMAND_ID}' AND [BOX_ID] = '{sBOX_ID}'";
                                        App.Local_SQLServer.NonQuery(sSQL);
                                        continue;
                                    }
                                    sShuttleCar_Src = $"STAGE{iSource_LineNo}"; //從STAGE
                                    sShuttleCar_Tar = $"STAGE{iTarget_LineNo}"; //放到STAGE

                                    sCAR_ID = $"CAR00{iTarget_LineNo}"; //要放到STK，對應的RGV車
                                    sSTK_Src = $"B-00{iTarget_LineNo}"; //STK STAGE來源
                                    sSTK_SrcCell = "001"; //STAGE CELL = 001
                                    sSTK_Tar = dr_Task["TAR_POS"].ToString().Trim();
                                    sSTK_TarCell = dr_Task["TAR_CELL_ID"].ToString().Trim();
                                }

                                if (sCAR_ID != "" && STK_CMD_RESULT != "1") //有要放到STK而且此次命令是要放到STK(而不是到暫存區而已)，要確認STK是否忙碌
                                {
                                    if (dt_Task.Select($"DIRECTION = 'OUT' AND LINE_NO = {iTarget_LineNo}").Count() != 0) //搜尋任務列表內，同一個LineNo有無出庫任務, !=0表示STK有出庫任務在忙碌中
                                        continue;
                                }
                                dt_temp = App.Local_SQLServer.SelectDB("*", "CAR_STATUS", $"[WITH_LIFTER] = '{sFind_ShuttleCar}' AND [COMMAND_ID] = '' AND [STATUS] = 'IDLE' AND [BOX_ID] = ''"); //確認穿梭車是否IDLE無任務
                                dt_stocker = App.STK_SQLServer.SelectDB("*", "CAR_STATUS", $"[CAR_ID]='CAR00{iTarget_LineNo}' AND [ACTION]='0'");//確認Stocker是否IDLE
                                if (dt_temp.Rows.Count == 0) //表示穿梭車有任務在忙碌中
                                    continue;
                                string sShuttleCar_ID = dt_temp.Rows[0]["CAR_ID"].ToString().Trim(); //穿梭車ID
								//if (dt_stocker.Rows.Count == 0 && App.Bc.wAux["isOnlyDemo"].BinValue != 1)//表示Stocker有任務在忙碌中
								//	continue;
								//---給穿梭車的命令 從暫存區放到Target Stage---
								CAR_ACTION_CMD ShuttleCarCmd = new CAR_ACTION_CMD()
                                {
                                    ACTION = "TRANSFER",
                                    CAR_ID = sShuttleCar_ID,
                                    BOX_ID = sBOX_ID,
                                    SOURCE = sShuttleCar_Src,
                                    TARGET = sShuttleCar_Tar
                                };
                                /*
                                if (sShuttleCar_ID.IndexOf("1") != -1)
                                    App.Bc.ShuttleCar1.C010_CMD(sCOMMAND_ID, ShuttleCarCmd);
                                else if (sShuttleCar_ID.IndexOf("2") != -1)
                                    App.Bc.ShuttleCar2.C010_CMD(sCOMMAND_ID, ShuttleCarCmd);
                                */
                                App.Bc.ShuttleCar1.C010_CMD(sCOMMAND_ID, ShuttleCarCmd);
                                //---給穿梭車的命令End---

                                if (sCAR_ID != "")
                                {
                                    //---給STK的命令 從STAGE放到目標Carousel & Cell---
                                    CAR_ACTION_CMD STK_Car_Cmd = new CAR_ACTION_CMD()
                                    {
                                        ACTION = "TRANSFER",
                                        CAR_ID = sCAR_ID,
                                        BOX_ID = sBOX_ID,
                                        SOURCE = sSTK_Src, //來源平台
                                        S_CELL_ID = sSTK_SrcCell, //交換平台固定001
                                        TARGET = sSTK_Tar, //目標Carousel
                                        T_CELL_ID = sSTK_TarCell, //目標CELL
                                        BATCH_NO = dr_Task["BATCH_NO"].ToString().Trim(),
                                        SOTERIA = dr_Task["SOTERIA"].ToString().Trim(),
                                        CUSTOMER_ID = dr_Task["CUSTOMER_ID"].ToString().Trim(),
                                    };
                                    App.Bc.STK.C010_CMD(sCOMMAND_ID, STK_Car_Cmd);
                                    //---給STK的命令End---
                                }
                                //---更新到車輛命令列表---
                                sSQL = $"INSERT INTO [dbo].[CAR_CMD] ([COMMAND_ID], [SHUTTLECAR_SRC], [SHUTTLECAR_TAR], [SHUTTLECAR_CAR_ID], [SHUTTLECAR_CMD_RESULT], [STK_SRC], [STK_SRC_CELL], [STK_TAR], [STK_TAR_CELL], [STK_CAR_ID], [STK_CMD_RESULT]) " +
                                    $"VALUES (" +
                                    $"'{sCOMMAND_ID}', " +
                                    $"'{sShuttleCar_Src}', " +
                                    $"'{sShuttleCar_Tar}', " +
                                    $"'{sShuttleCar_ID}', " +
                                    $"'{SHUTTLECAR_CMD_RESULT}', " +
                                    $"'{sSTK_Src}', " +
                                    $"'{sSTK_SrcCell}', " +
                                    $"'{sSTK_Tar}', " +
                                    $"'{sSTK_TarCell}', " +
                                    $"'{sCAR_ID}', " +
                                    $"'{STK_CMD_RESULT}' )";
                                App.Local_SQLServer.NonQuery(sSQL);

                                //---更新任務狀態 RESET STK 與 SHUTTLE CAR命令狀態--
                                sSQL = $"UPDATE TASK SET [STATUS] = '{(int)eTASK_Flow.SendCmd}' WHERE [COMMAND_ID] = '{sCOMMAND_ID}' AND [BOX_ID] = '{sBOX_ID}'";
                                App.Local_SQLServer.NonQuery(sSQL);
                                
                            }
                        }
                        #endregion 調儲_MOVE
                        break;
                    case eTASK_Flow.SendCmd: //命令已在IDLE送出，確認ACK
                        #region 確認穿梭車/STK ACK
                        DataTable dt_CarCMD = App.Local_SQLServer.SelectDB("*", "CAR_CMD", $"[COMMAND_ID] = {sCOMMAND_ID}");
                        if (dt_CarCMD.Rows.Count == 0)
                        {
                            sSQL = $"UPDATE TASK SET [STATUS] = '{(int)eTASK_Flow.NG}', [NG_REASON] = 'NOT FOUND CAR Command But Task Flow In SendCmd' WHERE [COMMAND_ID] = '{sCOMMAND_ID}' AND [BOX_ID] = '{sBOX_ID}'";
                            App.Local_SQLServer.NonQuery(sSQL);
                            continue;
                        }
                        DataRow dr_cmd = dt_CarCMD.Rows[0];
                        if (dr_cmd["STK_CMD_RESULT"].ToString().ToIntDef(0) == 0 || dr_cmd["SHUTTLECAR_CMD_RESULT"].ToString().ToIntDef(0) == 0)
                            break;
                        if (dr_cmd["STK_CMD_RESULT"].ToString().ToIntDef(0) == 1 && dr_cmd["SHUTTLECAR_CMD_RESULT"].ToString().ToIntDef(0) == 1)
                        {
                            //任務都被接受，狀態更新為動作中
                            sSQL = $"UPDATE TASK SET [STATUS] = '{(int)eTASK_Flow.Action}' WHERE [COMMAND_ID] = '{sCOMMAND_ID}' AND [BOX_ID] = '{sBOX_ID}'";
                            App.Local_SQLServer.NonQuery(sSQL);
                        }
                        else
                        {
                            if (dr_cmd["STK_CMD_RESULT"].ToString().ToIntDef(0) != 1 && dr_cmd["SHUTTLECAR_CMD_RESULT"].ToString().ToIntDef(0) == 1) //STK拒絕命令，穿梭車接受命令
                            {
                                //要對穿梭車CANCEL命令
                                CAR_ACTION_CMD ShuttleCarCmd = new CAR_ACTION_CMD()
                                {
                                    ACTION = "CANCEL",
                                    CAR_ID = dr_cmd["SHUTTLECAR_CAR_ID"].ToString().Trim(),
                                    BOX_ID = sBOX_ID,
                                    SOURCE = dr_cmd["SHUTTLECAR_SRC"].ToString().Trim(),
                                    TARGET = dr_cmd["SHUTTLECAR_TAR"].ToString().Trim(),
                                };
                                /*
                                if (dr_cmd["SHUTTLECAR_CAR_ID"].ToString().Trim().IndexOf("1") != -1)
                                    App.Bc.ShuttleCar1.C010_CMD(dr_cmd["COMMAND_ID"].ToString().Trim(), ShuttleCarCmd);
                                else if (dr_cmd["SHUTTLECAR_CAR_ID"].ToString().Trim().IndexOf("2") != -1)
                                    App.Bc.ShuttleCar2.C010_CMD(dr_cmd["COMMAND_ID"].ToString().Trim(), ShuttleCarCmd);
                                */
                                App.Bc.ShuttleCar1.C010_CMD(sCOMMAND_ID, ShuttleCarCmd);
                                //---給穿梭車的命令End---
                                //要對STK CANCEL命令
                                CAR_ACTION_CMD STK_Car_Cmd = new CAR_ACTION_CMD()
                                {
                                    ACTION = "CANCEL",
                                    CAR_ID = dr_cmd["STK_CAR_ID"].ToString().Trim(),
                                    BOX_ID = sBOX_ID,
                                    SOURCE = dr_cmd["STK_SRC"].ToString().Trim(),
                                    S_CELL_ID = dr_cmd["STK_SRC_CELL"].ToString().Trim(),
                                    TARGET = dr_cmd["STK_TAR"].ToString().Trim(),
                                    T_CELL_ID = dr_cmd["STK_TAR_CELL"].ToString().Trim(),
                                    BATCH_NO = dr_Task["BATCH_NO"].ToString().Trim(),
                                    SOTERIA = dr_Task["SOTERIA"].ToString().Trim(),
                                    CUSTOMER_ID = dr_Task["CUSTOMER_ID"].ToString().Trim()
                                };
                                App.Bc.STK.C010_CMD(sCOMMAND_ID, STK_Car_Cmd);
                                //---STK命令END---
                            }
                            else if (dr_cmd["STK_CMD_RESULT"].ToString().ToIntDef(0) == 1 && dr_cmd["SHUTTLECAR_CMD_RESULT"].ToString().ToIntDef(0) != 1) //STK接受命令，穿梭車拒絕命令
                            {
                                //要對STK CANCEL命令
                                CAR_ACTION_CMD STK_Car_Cmd = new CAR_ACTION_CMD()
                                {
                                    ACTION = "CANCEL",
                                    CAR_ID = dr_cmd["STK_CAR_ID"].ToString().Trim(),
                                    BOX_ID = sBOX_ID,
                                    SOURCE = dr_cmd["STK_SRC"].ToString().Trim(),
                                    S_CELL_ID = dr_cmd["STK_SRC_CELL"].ToString().Trim(),
                                    TARGET = dr_cmd["STK_TAR"].ToString().Trim(),
                                    T_CELL_ID = dr_cmd["STK_TAR_CELL"].ToString().Trim(),
                                    BATCH_NO = dr_Task["BATCH_NO"].ToString().Trim(),
                                    SOTERIA = dr_Task["SOTERIA"].ToString().Trim(),
                                    CUSTOMER_ID = dr_Task["CUSTOMER_ID"].ToString().Trim()
                                };
                                App.Bc.STK.C010_CMD(sCOMMAND_ID, STK_Car_Cmd);
                            }
                        }
                        //刪除CAR_CMD內的這筆命令
                        sSQL = $"delete [CAR_CMD] WHERE COMMAND_ID = '{sCOMMAND_ID}'";
                        App.Local_SQLServer.NonQuery(sSQL);
                        break;
                    #endregion 確認穿梭車/STK ACK
                    case eTASK_Flow.Action:
                        break;

                    case eTASK_Flow.NG:
                        if (dr_Task["DIRECTION"].ToString().Trim() == "OUT") //出庫任務
                        {
                            DataTable dt_Batch = App.Local_SQLServer.SelectDB("[ORDER_NO]", "[BATCH_LIST]", $"[BOX_ID] = '{sBOX_ID}'");
                            if (dt_Batch.Rows.Count > 0)
                            {
                                if (dt_Batch.Rows[0]["ORDER_NO"].ToString().Trim() != "") //有領料單號，是發料出庫
                                    App.Bc.OrderStockOutComp_Report(sBOX_ID, dt_Batch.Rows[0]["ORDER_NO"].ToString().Trim()); //發料出庫完成，上報WebService
                                else
                                    App.Bc.StockOutComp_Report(sBOX_ID); //出庫完成，上報WebService
                            }
                        }
                        else //入庫or調儲
                        {
                            App.Bc.StockInComp_Report(sBOX_ID); //入庫or調儲完成，上報WebService
                        }
                        sSQL= $"UPDATE [TASK] SET [STATUS] = '99' WHERE [BOX_ID] = '{sBOX_ID}'";
                        App.Local_SQLServer.NonQuery(sSQL);
                        break;
                    case eTASK_Flow.Complete:
                        if (dr_Task["DIRECTION"].ToString().Trim() == "OUT") //出庫任務
                        {
                            DataTable dt_Batch = App.Local_SQLServer.SelectDB("[ORDER_NO]", "[BATCH_LIST]", $"[BOX_ID] = '{sBOX_ID}'");
                            if (dt_Batch.Rows.Count > 0)
                            {
                                if (dt_Batch.Rows[0]["ORDER_NO"].ToString().Trim() != "") //有領料單號，是發料出庫
                                    App.Bc.OrderStockOutComp_Report(sBOX_ID, dt_Batch.Rows[0]["ORDER_NO"].ToString().Trim()); //發料出庫完成，上報WebService
                                else
                                    App.Bc.StockOutComp_Report(sBOX_ID); //出庫完成，上報WebService
                            }
                        }
                        else //入庫or調儲
                        {
                            App.Bc.StockInComp_Report(sBOX_ID); //入庫or調儲完成，上報WebService
                        }

                        //TASK搬到歷史紀錄去
                        string endtime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        sSQL = $"UPDATE [TASK] SET [END_TIME] = '{DateTime.Now.ToString("yyyyMMddHHmmssfff")}' WHERE [BOX_ID] = '{sBOX_ID}'";
                        App.Local_SQLServer.NonQuery(sSQL);
                        sSQL = $"INSERT INTO [TASK_HISTORY] ([BOX_ID], [BATCH_NO], [SRC_POS], [SRC_CELL_ID], [TAR_POS], [TAR_CELL_ID], [STATUS], [DIRECTION], [OUT_STOCKER], [OUT_LIFTER], [PRIORITY], [COMMAND_ID], [NG_REASON], [SOTERIA], [CUSTOMER_ID], [START_TIME], [END_TIME], [USE_TEMP_STATUS], [USER_ID]) " +
                            $"SELECT [BOX_ID], [BATCH_NO], [SRC_POS], [SRC_CELL_ID], [TAR_POS], [TAR_CELL_ID], [STATUS], [DIRECTION], [OUT_STOCKER], [OUT_LIFTER], [PRIORITY], [COMMAND_ID], [NG_REASON], [SOTERIA], [CUSTOMER_ID], [START_TIME], [END_TIME], [USE_TEMP_STATUS], [USER_ID] " +
                            $"FROM [TASK] WHERE [BOX_ID] = '{sBOX_ID}'";
                        App.Local_SQLServer.NonQuery(sSQL);

                        sSQL = $"Delete FROM [TASK] Where [BOX_ID] = '{sBOX_ID}'";
                        App.Local_SQLServer.NonQuery(sSQL);
                        break;
                }
            }
        }

        #region Function

        private bool isA_Area(int iLineNo_)
        {
            return iLineNo_ > 4 && iLineNo_ <= 7;
        }
        private bool isB_Area(int iLineNo_)
        {
            return iLineNo_ >= 1 && iLineNo_ <= 4;
        }

        public bool Create_Task(string sBoxID_, string sCustomerID_, string sDirection_, string sUserID_, int iPriority_, object Obj_) //sDirection_ = IN 入庫/OUT出庫/MOVE調儲 , Ojb_在入出庫時是LIFTER_A / LIFTER_B，調儲是目標儲格List
        {
            string sSQL, sSrcPos, sSrcCell, sTarPos, sTarCell, sBatchNo, sSoteria, sCustomerID, sOrderNo;
            int iSrcLineNo = 0, iTarLineNo = 0;
            DataTable dt_temp;
            try
            {
                switch(sDirection_)
                {
                    case "IN":
                        sSrcPos = Obj_.ToString();
                        sSrcCell = sBatchNo = sSoteria = sCustomerID = sOrderNo = string.Empty;

                        if (Dispatch_TargetCarousel(sSrcPos, sCustomerID_, out sTarPos, out sTarCell, null)) //指派目標Carousel & CELL
                        {
                            sSrcCell = string.Empty;
                            iTarLineNo = App.Bc.GetCarousel_LineNo(sTarPos);
                            dt_temp = App.Local_SQLServer.SelectDB("*", "BATCH_LIST", $"[BOX_ID] = '{sBoxID_}'");

                            if (dt_temp.Rows.Count > 0)
                            {
                                sBatchNo = "";
                                foreach (DataRow dr in dt_temp.Rows)
                                {
                                    //20220222 Sian modify , to ;
                                    sBatchNo += dr["BATCH_NO"].ToString().Trim() + ";";
                                }
                                sSoteria = dt_temp.Rows[0]["SOTERIA"].ToString().Trim();
                                sCustomerID = dt_temp.Rows[0]["CUSTOMER_ID"].ToString().Trim();
                                sBatchNo = sBatchNo.Remove(sBatchNo.LastIndexOf(';'), 1);
                            }
                            string sCommandID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            sSQL = $"INSERT INTO [TASK] ([BOX_ID], [BATCH_NO], [SRC_POS], [SRC_CELL_ID], [TAR_POS], [TAR_CELL_ID], [LINE_NO], [STATUS], [DIRECTION], [OUT_STOCKER], [OUT_LIFTER], [PRIORITY], [COMMAND_ID], [NG_REASON], [SOTERIA], [CUSTOMER_ID], [START_TIME], [END_TIME], [USE_TEMP_STATUS], [USER_ID], [ORDER_NO]) " +
                                $"VALUES ('{sBoxID_}','{sBatchNo}', '{sSrcPos}','{sSrcCell}','{sTarPos}','{sTarCell}','{iTarLineNo}','{(int)eTASK_Flow.IDLE}','IN','0','0',{iPriority_},'{sCommandID}','','{sSoteria}', '{sCustomerID}','{sCommandID}','','0','{sUserID_}','')";
                            App.Local_SQLServer.NonQuery(sSQL);
                            //string sSQL_temp = $"SELECT TOP(1) BOX_ID,BATCH_NO,CAROUSEL_ID,CELL_ID,SOTERIA FROM [CELL_STATUS] WHERE BATCH_NO='EMPTY' AND CAROUSEL_ID='{sTarPos}'";
                            //App.STK_SQLServer.Query(sSQL_temp, ref dt_temp);

                            //if (dt_temp.Rows.Count > 0)
                            //{
                            //    string sSrcPos_temp = dt_temp.Rows[0]["CAROUSEL_ID"].ToString().Trim();
                            //    string sSrcCell_temp = dt_temp.Rows[0]["CELL_ID"].ToString().Trim();
                            //    string sSoteria_temp = dt_temp.Rows[0]["SOTERIA"].ToString().Trim();
                            //    string sBoxID_temp = dt_temp.Rows[0]["BOX_ID"].ToString().Trim();
                            //    string sBatchNo_temp = dt_temp.Rows[0]["BATCH_NO"].ToString().Trim();
                            //    string sTarPos_temp = string.Empty;
                            //    string sTarCell_temp = string.Empty;
                            //    if (Dispatch_TargetCarousel_RandomMove(sSrcPos_temp, "", out sTarPos_temp, out sTarCell_temp, sSrcPos_temp))
                            //    {
                            //        iSrcLineNo = App.Bc.GetCarousel_LineNo(sSrcPos_temp);
                            //        iTarLineNo = App.Bc.GetCarousel_LineNo(sTarPos_temp);
                            //        sCommandID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            //        sSQL = $"INSERT INTO [TASK] ([BOX_ID], [BATCH_NO], [SRC_POS], [SRC_CELL_ID], [TAR_POS], [TAR_CELL_ID], [LINE_NO], [STATUS], [DIRECTION], [OUT_STOCKER], [OUT_LIFTER], [PRIORITY], [COMMAND_ID], [NG_REASON], [SOTERIA], [CUSTOMER_ID], [START_TIME], [END_TIME], [USE_TEMP_STATUS], [USER_ID], [ORDER_NO]) " +
                            //        $"VALUES ('{sBoxID_temp}','{sBatchNo_temp}', '{sSrcPos_temp}','{sSrcCell_temp}','{sTarPos_temp}','{sTarCell_temp}','{iSrcLineNo}','{(int)TaskCtrl.eTASK_Flow.eSendToSTK}','MOVE','0','0',{iPriority_},'{sCommandID}','','{sSoteria}', '{""}','{sCommandID}','','0','{sUserID_}','')";
                            //        App.Local_SQLServer.NonQuery(sSQL);
                            //    }
                            //}

                        }
                        else
                        {
                            string sCommandID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            sSQL = $"INSERT INTO [TASK] ([BOX_ID], [BATCH_NO], [SRC_POS], [SRC_CELL_ID], [TAR_POS], [TAR_CELL_ID], [LINE_NO], [STATUS], [DIRECTION], [OUT_STOCKER], [OUT_LIFTER], [PRIORITY], [COMMAND_ID], [NG_REASON], [SOTERIA], [CUSTOMER_ID], [START_TIME], [END_TIME], [USE_TEMP_STATUS], [USER_ID], [ORDER_NO]) " +
                                $"VALUES ('{sBoxID_}','{sBatchNo}', '{sSrcPos}','{sSrcCell}','{sTarPos}','{sTarCell}','{iTarLineNo}','{(int)TaskCtrl.eTASK_Flow.NG}','IN','0','0',{iPriority_},'{sCommandID}','CAN NOT FIND TARGET CAROUSEL & CELL','{sSoteria}', '{sCustomerID}','{sCommandID}','','0','{sUserID_}','')";
                            App.Local_SQLServer.NonQuery(sSQL);
                            return false;
                        }
                        break;
                    case "OUT":
                        sTarPos = Obj_.ToString();
                        sTarCell = sOrderNo = string.Empty;
                        dt_temp = App.STK_SQLServer.SelectDB("*", "[CELL_STATUS]", $"[BOX_ID] = '{sBoxID_}'"); //找儲格內這箱BOX
                        if (dt_temp.Rows.Count > 0)
                        {
                            sSrcPos = dt_temp.Rows[0]["CAROUSEL_ID"].ToString().Trim();
                            sSrcCell = dt_temp.Rows[0]["CELL_ID"].ToString().Trim();
                            iSrcLineNo = App.Bc.GetCarousel_LineNo(sSrcPos);
                            sBatchNo = dt_temp.Rows[0]["BATCH_NO"].ToString().Trim();
                            sSoteria = dt_temp.Rows[0]["SOTERIA"].ToString().Trim();
                            sCustomerID = dt_temp.Rows[0]["CUSTOMER_ID"].ToString().Trim();

                            DataTable dt_FindOrderNo = App.Local_SQLServer.SelectDB("*", "BATCH_LIST", $"[BOX_ID] = '{sBoxID_}' AND [BATCH_NO] = '{sBatchNo}'");
                            if (dt_FindOrderNo.Rows.Count > 0)
                                sOrderNo = dt_FindOrderNo.Rows[0]["ORDER_NO"].ToString().Trim();
                            else
                                sOrderNo = "";

                            string sCommandID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            sSQL = $"INSERT INTO [TASK] ([BOX_ID], [BATCH_NO], [SRC_POS], [SRC_CELL_ID], [TAR_POS], [TAR_CELL_ID], [LINE_NO], [STATUS], [DIRECTION], [OUT_STOCKER], [OUT_LIFTER], [PRIORITY], [COMMAND_ID], [NG_REASON], [SOTERIA], [CUSTOMER_ID], [START_TIME], [END_TIME], [USE_TEMP_STATUS], [USER_ID], [ORDER_NO]) " +
                                $"VALUES ('{sBoxID_}','{sBatchNo}', '{sSrcPos}','{sSrcCell}','{sTarPos}','{sTarCell}','{iSrcLineNo}','{(int)eTASK_Flow.eSendToSTK}','OUT','0','0',{iPriority_},'{sCommandID}','','{sSoteria}', '{sCustomerID}','{sCommandID}','','0','{sUserID_}','{sOrderNo}')";
                            App.Local_SQLServer.NonQuery(sSQL);
                        }
                        else
                        {
                            string sCommandID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            sSQL = $"INSERT INTO [TASK] ([BOX_ID], [BATCH_NO], [SRC_POS], [SRC_CELL_ID], [TAR_POS], [TAR_CELL_ID], [LINE_NO], [STATUS], [DIRECTION], [OUT_STOCKER], [OUT_LIFTER], [PRIORITY], [COMMAND_ID], [NG_REASON], [SOTERIA], [CUSTOMER_ID], [START_TIME], [END_TIME], [USE_TEMP_STATUS], [USER_ID], [ORDER_NO]) " +
                                $"VALUES ('{sBoxID_}','', '','','{sTarPos}','{sTarCell}','{iSrcLineNo}','{(int)TaskCtrl.eTASK_Flow.NG}','OUT','0','0',{iPriority_},'{sCommandID}','CAN NOT FIND THIS BOX IN STK','', '','{sCommandID}','','0','{sUserID_}','{sOrderNo}')";
                            App.Local_SQLServer.NonQuery(sSQL);
                            return false;
                        }
                        break;
                    case "MOVE":
                        sSrcPos = sSrcCell = sTarPos = sTarCell = sBatchNo = sSoteria = sCustomerID = string.Empty;
                        dt_temp = App.STK_SQLServer.SelectDB("*", "[CELL_STATUS]", $"[BOX_ID] = '{sBoxID_}'"); //找儲格內這箱BOX
                        if (dt_temp.Rows.Count > 0)
                        {
                            sSrcPos = dt_temp.Rows[0]["CAROUSEL_ID"].ToString().Trim();
                            sSrcCell = dt_temp.Rows[0]["CELL_ID"].ToString().Trim();
                            iSrcLineNo = App.Bc.GetCarousel_LineNo(sSrcPos);
                            sBatchNo = dt_temp.Rows[0]["BATCH_NO"].ToString().Trim();
                            sSoteria = dt_temp.Rows[0]["SOTERIA"].ToString().Trim();
                            sCustomerID = dt_temp.Rows[0]["CUSTOMER_ID"].ToString().Trim();
                            string sCommandID = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                            if (Dispatch_TargetCarousel("MOVE", sCustomerID_, out sTarPos, out sTarCell, (List<string>)Obj_)) //指派目標Carousel & CELL
                            {
                                iTarLineNo = App.Bc.GetCarousel_LineNo(sTarPos);

                                sSQL = $"INSERT INTO [TASK] ([BOX_ID], [BATCH_NO], [SRC_POS], [SRC_CELL_ID], [TAR_POS], [TAR_CELL_ID], [LINE_NO], [STATUS], [DIRECTION], [OUT_STOCKER], [OUT_LIFTER], [PRIORITY], [COMMAND_ID], [NG_REASON], [SOTERIA], [CUSTOMER_ID], [START_TIME], [END_TIME], [USE_TEMP_STATUS], [USER_ID], [ORDER_NO]) " +
                                    $"VALUES ('{sBoxID_}','{sBatchNo}', '{sSrcPos}','{sSrcCell}','{sTarPos}','{sTarCell}','{iSrcLineNo}','{(int)TaskCtrl.eTASK_Flow.eSendToSTK}','MOVE','0','0',{iPriority_},'{sCommandID}','','{sSoteria}', '{sCustomerID}','{sCommandID}','','0','{sUserID_}','')";
                                App.Local_SQLServer.NonQuery(sSQL);
                            }
                            else
                            {
                                sSQL = $"INSERT INTO [TASK] ([BOX_ID], [BATCH_NO], [SRC_POS], [SRC_CELL_ID], [TAR_POS], [TAR_CELL_ID], [LINE_NO], [STATUS], [DIRECTION], [OUT_STOCKER], [OUT_LIFTER], [PRIORITY], [COMMAND_ID], [NG_REASON], [SOTERIA], [CUSTOMER_ID], [START_TIME], [END_TIME], [USE_TEMP_STATUS], [USER_ID], [ORDER_NO]) " +
                                    $"VALUES ('{sBoxID_}','{sBatchNo}', '{sSrcPos}','{sSrcCell}','{sTarPos}','{sTarCell}','{iSrcLineNo}','{(int)TaskCtrl.eTASK_Flow.NG}','MOVE','0','0',{iPriority_},'{sCommandID}','CAN NOT FIND TARGET CAROUSEL & CELL','{sSoteria}', '{sCustomerID}','{sCommandID}','','0','{sUserID_}','')";
                                App.Local_SQLServer.NonQuery(sSQL);
                                return false;
                            }
                        }
                        else
                        {
                            string sCommandID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            sSQL = $"INSERT INTO [TASK] ([BOX_ID], [BATCH_NO], [SRC_POS], [SRC_CELL_ID], [TAR_POS], [TAR_CELL_ID], [LINE_NO], [STATUS], [DIRECTION], [OUT_STOCKER], [OUT_LIFTER], [PRIORITY], [COMMAND_ID], [NG_REASON], [SOTERIA], [CUSTOMER_ID], [START_TIME], [END_TIME], [USE_TEMP_STATUS], [USER_ID], [ORDER_NO]) " +
                                $"VALUES ('{sBoxID_}','', '','','{sTarPos}','{sTarCell}','{iSrcLineNo}','{(int)TaskCtrl.eTASK_Flow.NG}','MOVE','0','0',{iPriority_},'{sCommandID}','CAN NOT FIND THIS BOX IN STK','', '','{sCommandID}','','0','{sUserID_}','')";
                            App.Local_SQLServer.NonQuery(sSQL);
                            return false;
                        }
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogExcept.LogException(ex);
            }
            return false;
        }

        public bool Dispatch_TargetCarousel(string sFrom_, string sCustomerID_, out string sTarget_Carousel_, out string sTargetCell_, List<string> TargetCarouselList_) //分配目標Carousel & CELL
        {
            sTarget_Carousel_ = string.Empty;
            sTargetCell_ = string.Empty;
            try
            {
                if (sFrom_ == "LIFTER_A" || sFrom_ == "LIFTER_B") //從LIFTER A/B進來的
                {
                    string sSort = string.Empty;
                    if (sFrom_ == "LIFTER_A") //是從Lifter A進來的
                    {
                        if (App.Bc.wAux["isOnlyDemo"].BinValue == 1)
                        {
                            sTarget_Carousel_ = "C-051";
                            sTargetCell_ = "001";
                            return true;
                        }
                        sSort = "DESC"; //從C-090開始找
                    }
                    else
                    {
                        if (App.Bc.wAux["isOnlyDemo"].BinValue == 1)
                        {
                            DataTable dt_temp = App.Local_SQLServer.SelectDB("*", "[BATCH_LIST]", "[CURN_BIN] LIKE 'C-009%'");
							if (dt_temp.Rows.Count > 0)
							{
                                //2022/02/24 FOR B-002,B-003 DEMO TEST SIAN EDITED
                                sTarget_Carousel_ = "C-019";
                                sTargetCell_ = "001";
                                return true;
                            }
                            sTarget_Carousel_ = "C-009";
                            sTargetCell_ = "001";
                            return true;
                        }
                        sSort = "ASC"; //從C-001開始找
                    }

                    //先找相同客戶碼的Carousel
                    string sSQL = $"SELECT DISTINCT cell_status.[CAROUSEL_ID] " +
                        $"FROM [CELL_STATUS] cell_status JOIN [CAROUSEL_STATUS] carousel_status ON cell_status.[CAROUSEL_ID] = carousel_status.[CAROUSEL_ID] " +
                        $"WHERE cell_status.[CUSTOMER_ID] = '{sCustomerID_}' AND carousel_status.[STATUS] = 1 ORDER BY cell_status.CAROUSEL_ID {sSort}"; //找[CAROUSEL_STATUS]的STATUS = 1 (Online)的

                    DataTable dt_SameCustomer = new DataTable();
                    App.STK_SQLServer.Query(sSQL, ref dt_SameCustomer);
                    DataTable dt_Temp;
                    if (dt_SameCustomer.Rows.Count != 0) //有找到相同客戶ID的
                    {
                        foreach (DataRow dr in dt_SameCustomer.Rows) //可能有多個Carousel都有相同產品，可能有些滿的有些有空位，找一個空位
                        {
                            dt_Temp = App.STK_SQLServer.SelectDB("[CELL_ID]", "[CELL_STATUS]", $"CAROUSEL_ID = '{dr["CAROUSEL_ID"].ToString().Trim()}' AND [BOX_ID] = '' AND [BATCH_NO] = '' AND [STATUS] = 0 ORDER BY [CELL_ID] ASC");
                            
                            foreach(DataRow dr_cell in dt_Temp.Rows)
                            {
                                DataTable dt_FindTaskTarget = App.Local_SQLServer.SelectDB("*", "[TASK]", $"[TAR_POS] = '{dr["CAROUSEL_ID"].ToString().Trim()}' AND [TAR_CELL_ID] = '{dr_cell["CELL_ID"].ToString().Trim()}'");
                                if (dt_FindTaskTarget.Rows.Count != 0) //這個Cell 已經有任務指定目標了
                                {
                                    break;
                                }
                                else
                                {
                                    sTarget_Carousel_ = dr["CAROUSEL_ID"].ToString().Trim();
                                    sTargetCell_ = dr_cell["CELL_ID"].ToString().Trim();
                                    return true;
                                }
                            }
                        }
                    }
                    //沒有相同客戶的，先找空倉
                    DataTable dt_EmptyCarousel = App.STK_SQLServer.SelectDB("[CAROUSEL_ID]", "[CAROUSEL_STATUS]", $"[STORE_STATUS] = 2 AND [STATUS] = 1  AND [USE_SPACE] = 0 ORDER BY CAROUSEL_ID {sSort}"); //找[CAROUSEL_STATUS]的STORE_STATUS = 2 (空倉)的
               
                    foreach (DataRow dr_EmptyCarousel in dt_EmptyCarousel.Rows) //找到空倉
                    {
                        dt_Temp = App.STK_SQLServer.SelectDB("[CELL_ID]", "[CELL_STATUS]", $"CAROUSEL_ID = '{dr_EmptyCarousel["CAROUSEL_ID"].ToString().Trim()}' AND [BOX_ID] = '' AND [BATCH_NO] = '' AND [STATUS] = 0 ORDER BY [CELL_ID] ASC");

                        foreach (DataRow dr_cell in dt_Temp.Rows)
                        {
                            DataTable dt_FindTaskTarget = App.Local_SQLServer.SelectDB("*", "[TASK]", $"[TAR_POS] = '{dr_EmptyCarousel["CAROUSEL_ID"].ToString().Trim()}' AND [TAR_CELL_ID] = '{dr_cell["CELL_ID"].ToString().Trim()}'");
                            if (dt_FindTaskTarget.Rows.Count != 0) //這個Cell 已經有任務指定目標了
                            {
                                continue; //找下一個CELL
                            }
                            else
                            {
                                sTarget_Carousel_ = dr_EmptyCarousel["CAROUSEL_ID"].ToString().Trim();
                                sTargetCell_ = dr_cell["CELL_ID"].ToString().Trim();
                                return true;
                            }
                        }
                        App.Alarm.Set(1, 1001, true, $"CAROUSEL={dt_EmptyCarousel.Rows[0]["CAROUSEL_ID"].ToString().Trim()} is Empty Carousel but no Empty CELL !"); //明明是空倉卻找不到空位???
                    }
                    //沒有空倉Carousel，找有空位的
                    
                    DataTable dt_HaveEmptyCellCarousel = App.STK_SQLServer.SelectDB("[CAROUSEL_ID]", "[CAROUSEL_STATUS]", $"[STORE_STATUS] = 0 AND [STATUS] = 1 AND [TOTAL_SPACE] - [USE_SPACE] > 0 ORDER BY CAROUSEL_ID {sSort}"); //找[CAROUSEL_STATUS]的STORE_STATUS = 0 (正常)的


                    foreach (DataRow dr_HaveEmptyCellCarousel in dt_HaveEmptyCellCarousel.Rows) //找正常倉
                    {
                        dt_Temp = App.STK_SQLServer.SelectDB("[CELL_ID]", "[CELL_STATUS]", $"CAROUSEL_ID = '{dt_EmptyCarousel.Rows[0]["CAROUSEL_ID"].ToString().Trim()}' AND [BOX_ID] = '' AND ([BATCH_NO] = '' OR [BATCH_NO] is NULL) AND [STATUS] = 0 ORDER BY [CELL_ID] ASC");
                        foreach (DataRow dr_cell in dt_Temp.Rows) //有空的CELL
                        { 
                            DataTable dt_FindTaskTarget = App.Local_SQLServer.SelectDB("*", "[TASK]", $"[TAR_POS] = '{dr_HaveEmptyCellCarousel["CAROUSEL_ID"].ToString().Trim()}' AND [TAR_CELL_ID] = '{dr_cell["CELL_ID"].ToString().Trim()}'");
                            if (dt_FindTaskTarget.Rows.Count != 0) //這個Cell 已經有任務指定目標了
                            {
                                continue; //找下一個CELL
                            }
                            else
                            {
                                sTarget_Carousel_ = dr_HaveEmptyCellCarousel["CAROUSEL_ID"].ToString().Trim();
                                sTargetCell_ = dr_cell["CELL_ID"].ToString().Trim();
                                return true;
                            }
                        }
                    }
                    App.Alarm.Set(1, 1002, true, $"CAROUSEL={dt_EmptyCarousel.Rows[0]["CAROUSEL_ID"].ToString().Trim()} is Normal Carousel but no Empty CELL !"); //明明是正常儲格卻找不到空位???
                    return false;
                }
                else if (sFrom_ == "MOVE") //調儲目標搜尋
                {
                    //先找相同客戶碼的Carousel
                    string sSQL = $"SELECT DISTINCT [CELL_STATUS].[CAROUSEL_ID] " +
                        $"FROM [CELL_STATUS] LEFT JOIN [CAROUSEL_STATUS] ON [CELL_STATUS].[CAROUSEL_ID] = [CAROUSEL_STATUS].[CAROUSEL_ID] " +
                        $"WHERE [CUSTOMER_ID] = '{sCustomerID_}' AND [CAROUSEL_STATUS].[STATUS] = 1 AND ("; //找[CAROUSEL_STATUS]的STATUS = 1 (Online)的
                    for(int i = 0; i < TargetCarouselList_.Count; i++)
                    {
                        sSQL += $" [CELL_STATUS].[CAROUSEL_ID] = '{TargetCarouselList_[i]}'";
                        if (i != TargetCarouselList_.Count - 1) //不是最後一個
                            sSQL += " OR";
                    }
                    sSQL += " )";
                    DataTable dt_SameCustomer = new DataTable(); ;
                    App.STK_SQLServer.Query(sSQL, ref dt_SameCustomer);
                    
                    foreach (DataRow dr in dt_SameCustomer.Rows)
                    {
                        
                        DataTable dt_Temp = App.STK_SQLServer.SelectDB("[CELL_ID]", "[CELL_STATUS]", $"CAROUSEL_ID = '{dr["CAROUSEL_ID"].ToString().Trim()}' AND [BOX_ID] = '' AND ([BATCH_NO] = '' OR [BATCH_NO] IS NULL) AND [STATUS] = 0 ORDER BY [CELL_ID] ASC");
                        
                        if (dt_Temp.Rows.Count != 0) //有空的CELL
                        {
                            for(int x=0;x< dt_Temp.Rows.Count; x++)
							{
                                DataTable dt_FindTaskTarget = App.Local_SQLServer.SelectDB("*", "[TASK]", $"[TAR_POS] = '{dr["CAROUSEL_ID"].ToString().Trim()}' AND [TAR_CELL_ID] = '{dt_Temp.Rows[x]["CELL_ID"].ToString().Trim()}'");
                                if (dt_FindTaskTarget.Rows.Count != 0)
                                    continue;
                                sTarget_Carousel_ = dr["CAROUSEL_ID"].ToString().Trim();
                                sTargetCell_ = dt_Temp.Rows[x]["CELL_ID"].ToString().Trim();
                                return true;
							}
                        }
                    }
                }
            }
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
            return false;
        }
        public bool Dispatch_TargetCarousel_RandomMove(string sFrom_, string sCustomerID_, out string sTarget_Carousel_, out string sTargetCell_, string TargetCarousel_)
        {
            sTarget_Carousel_ = string.Empty;
            sTargetCell_ = string.Empty;
            try
            {
                if (sFrom_ == "MOVE") //調儲目標搜尋
                {
                    //先找相同客戶碼的Carousel
                    string sSQL = $"SELECT DISTINCT [CELL_STATUS].[CAROUSEL_ID] " +
                        $"FROM [CELL_STATUS] LEFT JOIN [CAROUSEL_STATUS] ON [CELL_STATUS].[CAROUSEL_ID] = [CAROUSEL_STATUS].[CAROUSEL_ID] " +
                        $"WHERE [CUSTOMER_ID] = '{sCustomerID_}' AND [CAROUSEL_STATUS].[STATUS] = 1 AND ("; //找[CAROUSEL_STATUS]的STATUS = 1 (Online)的
                    sSQL += $" [CELL_STATUS].[CAROUSEL_ID] != '{TargetCarousel_}'";
                    sSQL += " )";
                    DataTable dt_SameCustomer = new DataTable(); ;
                    App.STK_SQLServer.Query(sSQL, ref dt_SameCustomer);

                    foreach (DataRow dr in dt_SameCustomer.Rows)
                    {

                        DataTable dt_Temp = App.STK_SQLServer.SelectDB("[CELL_ID]", "[CELL_STATUS]", $"CAROUSEL_ID = '{dr["CAROUSEL_ID"].ToString().Trim()}' AND [BOX_ID] = '' AND ([BATCH_NO] = '' OR [BATCH_NO] IS NULL) AND [STATUS] = 0 ORDER BY [CELL_ID] ASC");

                        if (dt_Temp.Rows.Count != 0) //有空的CELL
                        {
                            for (int x = 0; x < dt_Temp.Rows.Count; x++)
                            {
                                DataTable dt_FindTaskTarget = App.Local_SQLServer.SelectDB("*", "[TASK]", $"[TAR_POS] = '{dr["CAROUSEL_ID"].ToString().Trim()}' AND [TAR_CELL_ID] = '{dt_Temp.Rows[x]["CELL_ID"].ToString().Trim()}'");
                                if (dt_FindTaskTarget.Rows.Count != 0)
                                    continue;
                                sTarget_Carousel_ = dr["CAROUSEL_ID"].ToString().Trim();
                                sTargetCell_ = dt_Temp.Rows[x]["CELL_ID"].ToString().Trim();
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogExcept.LogException(ex);
            }
            return false;
        }
        public bool ChangeTargetCarousel(string sCommandID_, string sChangeType_)
        {
            DataTable dt_Task = App.Local_SQLServer.SelectDB("*", "[TASK]", $"[COMMAND_ID] = '{sCommandID_}'");
            if (dt_Task.Rows.Count > 0)
            {
                DataRow dr_Task = dt_Task.Rows[0];
                if (dr_Task["DIRECTION"].ToString().Trim() != "IN") //不是入庫任務
                    return true;
                string sSQL, sNewCarousel, sNewCell, sSort, sFindState = "0";
                if (sChangeType_ == "Secret") //是機密入庫，改找機密儲格
                    sFindState = "1";
                else if (sChangeType_ == "Error") //是異常入庫，改找暫存儲格
                    sFindState = "2";

                if (dr_Task["SRC_POS"].ToString().Trim() == "LIFTER_A") //是從Lifter A進來的
                {
                    if (App.Bc.wAux["isOnlyDemo"].BinValue == 1)
                    {
                        sNewCarousel = "C-051";
                        sNewCell = "001";
                        return true;
                    }
                    sSort = "DESC"; //從C-090開始找
                }
                else 
                {
                    if (App.Bc.wAux["isOnlyDemo"].BinValue == 1)
                    {
                        sNewCarousel = "C-009";
                        sNewCell = "001";
                        return true;
                    }
                    sSort = "ASC"; //其他都從C-001開始找
                }

                //先找相同客戶碼的Carousel
                sSQL = $"SELECT cell_status.[CAROUSEL_ID], cell_status.[CELL_ID] " +
                    $"FROM [CELL_STATUS] cell_status JOIN [CAROUSEL_STATUS] carousel_status ON cell_status.[CAROUSEL_ID] = carousel_status.[CAROUSEL_ID] " +
                    $"WHERE cell_status.[CUSTOMER_ID] = '{dr_Task["CUSTOMER_ID"].ToString().Trim()}' AND carousel_status.[STATUS] = 1 ORDER BY cell_status.CAROUSEL_ID {sSort}"; //找[CAROUSEL_STATUS]的STATUS = 1 (Online)的

                DataTable dt_SameCustomer = new DataTable();
                App.STK_SQLServer.Query(sSQL, ref dt_SameCustomer);

                DataTable dt_TypeMatch_Cells = App.Local_SQLServer.SelectDB("*", "CELL_STATUS", $"STATE = {sFindState}");
                dt_TypeMatch_Cells.PrimaryKey = new DataColumn[] { dt_TypeMatch_Cells.Columns[0], dt_TypeMatch_Cells.Columns[1] };
                if (dt_TypeMatch_Cells.Rows.Count > 0)
                {
                    string stypematch_carousel = dt_TypeMatch_Cells.Rows[0]["CAROUSEL_ID"].ToString();
                    string stypematch_cell = dt_TypeMatch_Cells.Rows[0]["CELL_ID"].ToString();
                }
                DataTable dt_Match = new DataTable();
                dt_Match.Columns.Add("CAROUSEL_ID", typeof(string));
                dt_Match.Columns.Add("CELL_ID", typeof(string));
                
                //將兩個資料表合併
                var userRolesInfo2 = (from u in dt_SameCustomer.AsEnumerable()
                                      join ur in dt_TypeMatch_Cells.AsEnumerable()
                                      on new 
                                      { 
                                          CAROUSEL_ID = u.Field<string>("CAROUSEL_ID").Trim(), 
                                          CELL_ID = u.Field<string>("CELL_ID").Trim()
                                      } equals  new 
                                      { 
                                          CAROUSEL_ID = ur.Field<string>("CAROUSEL_ID").Trim(), 
                                          CELL_ID = ur.Field<string>("CELL_ID").Trim()
                                      }  
                                      select dt_Match.LoadDataRow(
                                      new object[]
                                      {
                                        u.Field<string>("CAROUSEL_ID"),
                                        u.Field<string>("CELL_ID")
                                      }, true)).ToList();
                
                if (dt_Match.Rows.Count > 0)
                {
                    sNewCarousel = dt_Match.Rows[0]["CAROUSEL_ID"].ToString().Trim();
                    sNewCell = dt_Match.Rows[0]["CELL_ID"].ToString().Trim();
                    sSQL = $"UPDATE [TASK] SET [TAR_POS] = {sNewCarousel}, [TAR_CELL_ID] = '{sNewCell}' WHERE [COMMAND_ID] = '{sCommandID_}'";
                    App.Local_SQLServer.NonQuery(sSQL);
                }
                return true;
            }
            return false;
        }
        
        #endregion Function

    }
}
