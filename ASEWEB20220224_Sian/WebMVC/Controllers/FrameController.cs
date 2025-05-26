using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ASEWEB.Models;
using Microsoft.AspNetCore.Http;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Threading;
using System.Data;

namespace ASEWEB.Controllers
{
    public class FrameController : Controller
    {
        private readonly ILogger<FrameController> _logger;

        
        public FrameController(ILogger<FrameController> logger)
        {
            _logger = logger;
        }
        public IActionResult Error_Page()
		{
            return View();
		}
        #region Alarm
        public IActionResult Alarm()
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            List<AlarmModel> data = new List<AlarmModel>();
            ASEWEB.Models.SQLContext C = new ASEWEB.Models.SQLContext();
            List<string> Alarm_List = C.Alarm_List();
            List<string> data1 = C.Alarm_History_List();
            for (int i = 0; i < Alarm_List.Count; i = i + 4)
            {
                AlarmModel datatmp = new AlarmModel(Alarm_List[i], Alarm_List[i + 1], Alarm_List[i + 2], Alarm_List[i + 3]);
                data.Add(datatmp);
            }
            ViewBag.data = data;
            ViewBag.data1 = data1;
            return View();
        }
        [HttpPost]
        public IActionResult Alarm(IFormCollection post)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            var C = new ASEWEB.Models.SQLContext();
            C.Delete_Alarm(post["rID"],post["rRFtype"]);
            List<AlarmModel> data = new List<AlarmModel>();
            List<string> data1 = C.Alarm_History_List();
            List<string> Alarm_List = C.Alarm_List();
            for (int i = 0; i < Alarm_List.Count; i = i + 4)
            {
                AlarmModel datatmp = new AlarmModel(Alarm_List[i], Alarm_List[i + 1], Alarm_List[i + 2], Alarm_List[i + 3]);
                data.Add(datatmp);
            }
            ViewBag.data = data;
            ViewBag.data1 = data1;
            return View();
        }
        #endregion Alarm
        #region Index
        public IActionResult Index()
        {
			if (!Startup.STK_Connect)
			{
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
			else
			{
                TempData["STK_Connect_Message"] = "OK";
			}
            var C = new ASEWEB.Models.SQLContext();
            List<string> CSID_list = C.CSID_String();
            
            List<IndexModel> data = new List<IndexModel>();
            for (int i = 0; i < CSID_list.Count; i=i+1)
            {
                IndexModel datatemp = new IndexModel(CSID_list[i]);
                data.Add(datatemp);
            }
            ViewBag.data = data;
            return View(data);
        }
        #endregion Index
        #region Cell
        public IActionResult Cell(string rCSID, string rCSID_Show)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            var C = new ASEWEB.Models.SQLContext();
            List<string> CEID_list = C.CEID_String(rCSID);
            List<CellModel> data = new List<CellModel>();
            for (int i = 0; i < CEID_list.Count; i = i + 2)
            {
                CellModel datatemp = new CellModel(rCSID, rCSID_Show, CEID_list[i], CEID_list[i + 1]);
                data.Add(datatemp);
            }
            ViewBag.data = data;
            return View(data);
        }
        public IActionResult Cell_Detail(string rCSID, string rCEID)
        {
            rCSID = rCSID.Trim();
            Cell_DetailModel data = new Cell_DetailModel(rCSID, rCEID);
            ViewBag.data = data;
            ViewBag.list = data.Detail;
            return PartialView();
        }
        public IActionResult Cell_Utility(string rCSID)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            var C = new ASEWEB.Models.SQLContext();
            var datalist = C.Cell_Utility_String(rCSID);
            List<Cell_UtilityModel> data = new List<Cell_UtilityModel>();
            for (int i = 0; i < datalist.Count; i = i + 3)
            {
                Cell_UtilityModel datatmp = new Cell_UtilityModel(datalist[i], datalist[i + 1], datalist[i + 2]);
                data.Add(datatmp);
            }
            ViewBag.data = data;
            return PartialView();
        }
        [HttpPost]
        public IActionResult Cell_Utility(IFormCollection post)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            var C = new ASEWEB.Models.SQLContext();
            List<Cell_UtilityModel> data = new List<Cell_UtilityModel>();
            for (int i = 0; i < post["CSID_List"].Count; i++)
            {
                C.Update_Cell_Utility(post["CSID_List"][i], post["CEID_List"][i], post["State_List"][i]);
            }
            ViewBag.data = data;
            return PartialView();

        }
        #endregion Cell


        public IActionResult ShowRecover()
		{
            var C = new ASEWEB.Models.SQLContext();
            C.TransferRecover();
            return RedirectToAction("Index");
		}
        public IActionResult Test()
        {
            return View();
        }
        public IActionResult Log_record()
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                //return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            List<Log_RecordModel> data = new List<Log_RecordModel>();
            List<string> data_list = new List<string> {"Stocker", "Shuttle" };
            for(int i = 0; i < data_list.Count; i++)
            {
                Log_RecordModel datatmp = new Log_RecordModel(data_list[i]);
                data.Add(datatmp);
            }
            ViewBag.data = data;
            return View();
        }
        #region 盤點
        public IActionResult Check_inventory(string rBN1, string rBN2,string rCSID1,string rCSID2,string rSTR1,string rSTR2,string rCMID1,string rCMID2,string rDaysApart1,string rDaysApart2)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            Check_inventoryModel data = new Check_inventoryModel(rBN1,rBN2,rCSID1, rCSID2,rSTR1, rSTR2, rCMID1,rCMID2, rDaysApart1, rDaysApart2);
            List<Check_inventoryModel> list = new List<Check_inventoryModel>();
            List<string> user_select = data.Check_Inventory_string();
            for(int i = 0; i < user_select.Count; i = i + 8)
            {
                Check_inventoryModel datatemp = new Check_inventoryModel(user_select[i], user_select[i+1], user_select[i+2], user_select[i + 3], user_select[i + 4], user_select[i+5], user_select[i+6], user_select[i+7]);
                datatemp.id = (i / 8).ToString();
                list.Add(datatemp);
            }

            
            ViewBag.Check_Inventory = data;
            ViewBag.list = list;

            return View(data);
        }

        [HttpPost]
        public IActionResult Check_inventory(IFormCollection post)//IFormCollection在Chrome上不作用 注意
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            List<Check_inventoryModel> list = new List<Check_inventoryModel>();
            Check_inventoryModel data = new Check_inventoryModel();
            List<string> rT = new List<string>();
            List<string> rWS = new List<string>();
            List<string> rDS = new List<string>();
            string User = TempData.Peek("User").ToString();
            string rTTemp = post["rT"];
            rTTemp = rTTemp ?? string.Empty;
            if (rTTemp.Contains(",")) { rT = rTTemp.Split(',').ToList(); }

            string rWSTemp = post["rWS"];
            rWSTemp = rWSTemp ?? string.Empty;
            if (rWSTemp.Contains(","))
            {
                rWS = rWSTemp.Split(',').ToList();
            }
            
            string rDSTemp = post["rDS"];
            rDSTemp = rDSTemp ?? string.Empty;
            if (rDSTemp.Contains(","))
            {
                rDS = rDSTemp.Split(',').ToList();
            }
            Check_inventoryModel insert = new Check_inventoryModel(post["rNow"], rT, rWS, rDS, User);
            string comid=insert.Check_Inventory_insert();
            for (int i = 0; i < post["rInsert"].Count; i = i + 7)
            {
                string x = post["rInsert"][i];
                if (x == "") continue;
                insert.Set_Check_Inventory(post["rInsert"][i], post["rInsert"][i + 1], post["rInsert"][i + 2],post["rInsert"][i+3],post["rInsert"][i+4],post["rInsert"][i+5]);
                insert.Check_Inventory_insert_detail(comid);
            }

            ViewBag.Check_Inventory = data;
            ViewBag.list = list;
            return View(data);
        }
        public IActionResult Check_inventory_History()
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            Check_inventoryModel data = new Check_inventoryModel();
            ViewBag.Check_Inventory = data;
            ViewBag.history = data.Check_Inventory_History();
            return PartialView(data);
            
        }
        public IActionResult Check_inventory_History_List(string commandid)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            Check_inventoryModel data = new Check_inventoryModel();
            ViewBag.Check_Inventory = data;
            ViewBag.HistoryList = data.Check_Inventory_HistoryList(commandid);
            return PartialView();
        }
        public IActionResult Check_inventory_Task_List(string commandid)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            Check_inventoryModel data = new Check_inventoryModel();
            ViewBag.Check_Inventory = data;
            ViewBag.TaskList = data.Check_Inventory_TaskList(commandid);
            return PartialView(data);
        }
        public IActionResult Check_Inventory_Task()
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            Check_inventoryModel data = new Check_inventoryModel();
            ViewBag.Check_Inventory = data;
            ViewBag.Task = data.Check_Inevntory_Task();
            return PartialView();
        }
        #endregion 盤點
        #region 調儲
        public IActionResult Storage(string rBN1,string rBN2,string rCSID1,string rCSID2,string rSTR1,string rSTR2,string rCEID1,string rCEID2)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            StorageModel data = new StorageModel(rBN1, rBN2, rCSID1, rCSID2, rSTR1, rSTR2, rCEID1, rCEID2);
            List<StorageModel> list = new List<StorageModel>();
            List<string> user_select = data.Storage_string();
            for (int i = 0; i < user_select.Count; i = i + 7)
            {
                StorageModel datatemp = new StorageModel(user_select[i], user_select[i + 1], user_select[i + 2], user_select[i + 3], user_select[i + 4], user_select[i + 5], user_select[i + 6]);
                datatemp.id = (i / 7).ToString();
                list.Add(datatemp);
            }
            ViewBag.Storage = data;
            ViewBag.list = list;

            return View(data);
        }
        [HttpPost]
        public IActionResult Storage(IFormCollection post)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            StorageModel data = new StorageModel(post["rBN1"], post["rBN2"],post["rCSID1"],post["rCSID2"],post["rSTR1"],post["rSTR2"],post["rCMID1"],post["rCMID2"]);
            List<StorageModel> list = new List<StorageModel>();
            List<string> user_select = data.Storage_string();
            for (int i = 0; i < user_select.Count; i = i + 7)
            {
                StorageModel datatemp = new StorageModel(user_select[i], user_select[i + 1], user_select[i + 2], user_select[i + 3], user_select[i + 4], user_select[i + 5], user_select[i + 6]);
                datatemp.id = (i / 7).ToString();
                list.Add(datatemp);
            }
            ViewBag.Storage = data;
            ViewBag.list = list;
            
            return View(data);
        }
        public IActionResult StorageCarousel()
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            var C = new ASEWEB.Models.SQLContext();
            List<string> CSID_list = C.CSID_String();
            List<IndexModel> CSID = new List<IndexModel>();
            for (int i = 0; i < CSID_list.Count; i=i+1)
            {
                IndexModel CSIDtemp = new IndexModel(CSID_list[i]);
                CSID.Add(CSIDtemp);
            }
            ViewBag.data = CSID;
            return PartialView(CSID);
        }
        public IActionResult StorageCell(string tCSID,string tCSID_Show)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            var C = new ASEWEB.Models.SQLContext();
            List<CellModel> CEID = new List<CellModel>();
            List<string> CEID_list = C.CEID_String(tCSID);
            for (int i = 0; i < CEID_list.Count; i=i+2)
            {
                CellModel CEIDtemp = new CellModel(tCSID,tCSID_Show, CEID_list[i], CEID_list[i+1]);
                CEID.Add(CEIDtemp);
            }
            ViewBag.data = CEID;
            return PartialView(CEID);
        }
        [HttpPost]
        public IActionResult Update_Storage(IFormCollection post)
        {
            List<string> BoxIDList = new List<string>();
            List<string> TCarouselIDList = new List<string>();
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            for (int i = 0; i < post["Box_ID"].Count; i++)
            {
                UpdateStorageModel data;
                if (post["T_Carousel_ID"][i].Trim() != "")
                {
                    data = new UpdateStorageModel(post["Box_ID"][i].Trim(), post["T_Carousel_ID"][i].Trim(), post["T_Cell_ID"][i].Trim(),TempData.Peek("User").ToString().Trim());
                    BoxIDList.Add(post["Box_ID"][i].Trim());
                    TCarouselIDList.Add(post["T_Carousel_ID"][i].Trim());
                    //Thread.Sleep(3000);
                }
            }
            var C = new ASEWEB.Models.SQLContext();
            string x = string.Empty;
            for(int i = 0; i < BoxIDList.Count; i++)
			{
                if (i > 0)
                    x += ",";
                x += $"{BoxIDList[i].ToString().Trim()},{TempData.Peek("User").ToString().Trim()},{TCarouselIDList[i].ToString().Trim()}";
                
			}
            C.Update_Storage(x);
            return RedirectToAction("Storage", "Frame");
        }
		#endregion 調儲
		#region 出庫
		[HttpPost]
        public IActionResult Stock(IFormCollection post)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            var C = new ASEWEB.Models.SQLContext();
            List<string> Task_List = C.Stock_Task_List();
            List<StockModel> Stock_list = new List<StockModel>();
            
            StockModel Stock = new StockModel();
            string Box_List = string.Empty;
            for(int i = 0; i < post["Insert_Batch"].Count; i++)
            {
                if (i > 0)
                    Box_List += ",";
                Box_List+=(post["Insert_BOX"][i].ToString().Trim()+","+ TempData.Peek("User").ToString().Trim());
            }
            if (C.Lifter_Check() != "0")
            {
                C.Update_Stock(Box_List, C.Lifter_Select());
                TempData["Stock_Message"] = "任務創建成功";
            }
            else
                TempData["Stock_Message"] = "任務創建失敗 無LIFTER為出庫狀態";
            List<string> Stock_List_Temp = C.Stock_Select_List();
            for (int i = 0; i < Stock_List_Temp.Count; i += 5)
            {
                StockModel Stocktemp = new StockModel(Stock_List_Temp[i], Stock_List_Temp[i + 1], Stock_List_Temp[i + 2], Stock_List_Temp[i + 3], Stock_List_Temp[i + 4]);
                Stock_list.Add(Stocktemp);
            }
            ViewBag.Stock = Stock;
            ViewBag.Stock_list = Stock_list;
            ViewBag.List = Task_List;
            return View();
        }
        public IActionResult Stock(string rBN1,string rBN2,string rN1,string rN2)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            var C = new ASEWEB.Models.SQLContext();
            List<string> Task_List = C.Stock_Task_List();
            List<StockModel> Stock_list = new List<StockModel>();
            List<string> Stock_List_Temp = C.Stock_Select_List();
            StockModel Stock = new StockModel();
            if (rBN1 != null && rBN1!="")
                Stock_List_Temp=Stock.Set_BatchNO(rBN1, rBN2);
            else if (rN1 != null && rN1!="")
                Stock_List_Temp = Stock.Set_No(rN1, rN2);
            TempData["Stock_Message"] = "";
            for (int i = 0; i < Stock_List_Temp.Count; i+=5)
            {
                StockModel Stocktemp = new StockModel(Stock_List_Temp[i], Stock_List_Temp[i + 1], Stock_List_Temp[i + 2], Stock_List_Temp[i + 3], Stock_List_Temp[i + 4]);
                Stock_list.Add(Stocktemp);
            }
            ViewBag.Stock = Stock;
            ViewBag.Stock_list = Stock_list;
            ViewBag.List = Task_List;
            return View();
        }
		#endregion 出庫
		#region 任務列表
		public IActionResult Task(string Update_Message)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                //return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            TaskModel data = new TaskModel();
            var C = new ASEWEB.Models.SQLContext();
            List<string> Priority_Name = C.Task_Priority("Name");
            List<string> Priority_Value = C.Task_Priority("Value");
            ViewBag.Task = data;
            ViewBag.list = data.Task_string();
            ViewBag.P_Name = Priority_Name;
            ViewBag.P_Value = Priority_Value;

            return View(data);
        }
        [HttpPost]
        public IActionResult Task(IFormCollection post)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                //return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            TaskModel data = new TaskModel();
            var C = new ASEWEB.Models.SQLContext();
            for(int i = 0; i < post["PRIORITY_NAME"].Count; i++)
            {
                C.Update_Priority(post["PRIORITY_NAME"][i].Trim(), post["PRIORITY_VALUE"][i].Trim());
            }
            List<string> Priority_Name = C.Task_Priority("Name");
            List<string> Priority_Value = C.Task_Priority("Value");
            ViewBag.Task = data;
            ViewBag.list = data.Task_string();
            ViewBag.P_Name = Priority_Name;
            ViewBag.P_Value = Priority_Value;
            return View(data);
        }
        public IActionResult Task_List()
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                //return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            TaskModel data = new TaskModel();
            var C = new ASEWEB.Models.SQLContext();
            ViewBag.Task = data;
            ViewBag.list = data.Task_string();
            return PartialView();
        }
        public IActionResult Task_Priority(string CMD_ID_,string Update_Message)
        {
            var C = new ASEWEB.Models.SQLContext();
            ViewBag.List = C.Task_Priority_List(CMD_ID_);
            if (Update_Message == null)
                Update_Message = "";
            ViewBag.Update_Message = Update_Message;
            return PartialView();
        }
        [HttpPost]
        public IActionResult Update_Task_Priority(IFormCollection post)
        {
            var C = new ASEWEB.Models.SQLContext();
            string msg = null;
            for(int i = 0; i < post["CMD_ID"].Count;i++)
            {
                string CMD_ID = post["CMD_ID"][i].ToString().Trim();
                string priority = post["PRIORITY"][i].ToString().Trim();
                msg=C.Update_Task_Priority(CMD_ID, priority);
            }
            
            return RedirectToAction("Task_Priority","Frame",new { Update_Message=msg });
        }
        #endregion 任務列表
        #region 系統設定
        public IActionResult UcSystemSetting()
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            return View();
        }
        public IActionResult Carousel_Utility()
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            var C = new ASEWEB.Models.SQLContext();
            List<string> CSID_list = C.CU_CSID();
            List<Carousel_UtilityModel> data = new List<Carousel_UtilityModel>();
            for (int i = 0; i < CSID_list.Count; i++)
            {
                Carousel_UtilityModel datatemp = new Carousel_UtilityModel(CSID_list[i]);
                data.Add(datatemp);
            }
            ViewBag.list = data;
            return PartialView();
        }
        [HttpPost]
        public IActionResult Carousel_Utility(IFormCollection post)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            var C = new ASEWEB.Models.SQLContext();
            for (int i = 0; i < post["CSID_List"].Count; i++)
            {
                C.Update_ShowCSID(post["S_CSID_List"][i].Trim(), post["CSID_List"][i].Trim());
                C.Update_ShowCEID_ForCarousel(post["S_CEID_List"][i].Trim(), post["CSID_List"][i].Trim());
            }
            C.Update_Carousel_Utility(post["CSID_List"], post["TU_List"], post["TL_List"], post["HU_List"], post["HL_List"], post["TUON_List"], post["TUOFF_List"], post["UserID"][0].Trim());
            List<string> CSID_list = C.CU_CSID();
            List<Carousel_UtilityModel> data = new List<Carousel_UtilityModel>();
            for (int i = 0; i < CSID_list.Count; i++)
            {
                Carousel_UtilityModel datatemp = new Carousel_UtilityModel(CSID_list[i]);
                data.Add(datatemp);
            }
            ViewBag.list = data;
            return PartialView();
        }
        public IActionResult WebService_Setting()
		{
            var C = new ASEWEB.Models.SQLContext();
            string CPC_Status = C.CPC_Status();
            ViewBag.CPC_Status = CPC_Status;
            return PartialView();
		}
        [HttpPost]
        public IActionResult WebService_Setting(IFormCollection post)
		{
            var C = new ASEWEB.Models.SQLContext();
            C.Update_CPC_Status(post["CPC_Status"]);
            string CPC_Status = C.CPC_Status();
            ViewBag.CPC_Status = CPC_Status;
            return PartialView();
        }
        #endregion 系統設定
        #region 使用者設定
        public IActionResult UserSetting(string UG_Select)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }

            if (UG_Select == null || UG_Select == "")
                UG_Select = "Engineer";
            var C = new ASEWEB.Models.SQLContext();
            Modify_UserModel data = new Modify_UserModel(UG_Select);
            int test1 = data.UserAuthority.IndexOf("Mainwindow");
            ViewBag.UserData = data;
            return View();
        }
        [HttpPost]
        public IActionResult UserSetting(IFormCollection post)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            string UG = post["UG"];
            if (UG=="" || UG==null)
                UG = "Engineer";
            Modify_UserModel data = new Modify_UserModel(UG, post["UA"], post["USS"]);
            data.Update_Group_Authority();
            ViewBag.UserData = data;
            return View();
        }
        public IActionResult UserInsert(string rGroup)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            ViewBag.Group = rGroup;
            ViewBag.Message = "";
            return PartialView();
        }
        [HttpPost]
        public IActionResult UserInsert(IFormCollection post)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            Insert_UserModel data = new Insert_UserModel(post["UID"],post["UN"], post["UP"], post["UG"]);
            string message = string.Empty;
            if(post["UID"]!="" && post["UP"] != "")
			{
                message=data.Insert_User();
            }
            ViewBag.Message = message;
            ViewBag.Group = data.UserGroup;
            return PartialView();
        }
        public IActionResult UserDelete()
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            var C = new ASEWEB.Models.SQLContext();
            List<string> data = C.User_String();
            ViewBag.data = data;
            return PartialView();
        }
        [HttpPost]
        public IActionResult UserDelete(IFormCollection post)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            var C = new ASEWEB.Models.SQLContext();
            string[] UN = post["UN"].ToString().Split(',');
            for (int i = 0; i < UN.Length; i=i+2)
            {
                C.Delete_User(UN[i], UN[i+1]);
            }
            return PartialView();
        }
        #endregion 使用者設定

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Insert_Check()
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            return View();
        }
        #region 上傳&下載CSV
        public IActionResult Upload_Account()
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                //return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            ViewBag.test = "";
            return View();
        }
        [HttpPost]
        public IActionResult Upload_Account(List<IFormFile> postedFiles)
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                //return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            try
            {
                var reader = new StreamReader(postedFiles[0].OpenReadStream());
                while (!reader.EndOfStream)
                {
                    List<string> listA = new List<string>();
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    int i = 0;
                    while (i < values.Length)
                    {
                        listA.Add(values[i]);
                        i++;
                    }
                    var C = new SQLContext();
                    C.Insert_User_ForCSV(listA);
                }
                ViewBag.test = "上傳成功！";
            }
            catch(Exception ex)
            {
                ViewBag.test = "上傳失敗 請檢查CSV格式";
            }
            return View();
        }
        [HttpPost]
        public IActionResult Download_Account()
        {
            string folderPath = System.Environment.CurrentDirectory+"\\AccountCSV";
            FileStream fs;
            var C = new ASEWEB.Models.SQLContext();
            List<Download_CSVModel> data = new List<Download_CSVModel>();
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            List<string> DataList = C.Download_Account_List();
            for(int i = 0; i < DataList.Count; i += 6)
            {
                Download_CSVModel datatmp = new Download_CSVModel(DataList[i], DataList[i + 1], DataList[i + 2], DataList[i + 3], DataList[i + 4], DataList[i + 5]);
                data.Add(datatmp);
            }
            string CSV_Path = folderPath + "\\Account.csv";
            fs = new FileStream(CSV_Path, FileMode.Create);
            using (var file = new StreamWriter(fs,System.Text.Encoding.UTF8))
            {
                file.WriteLine("UserID,UserName,Password,UserGroup,Authority,SystemSetting");
                foreach (var item in data)
                {
                    file.WriteLine($"{item.ID},{item.Name},{item.Password},{item.Group},{item.Auth},{item.UserSystemSetting}");
                }
            }
            Stream iStream = new FileStream(CSV_Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(iStream, "application/octet-stream", $"Account{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.csv");
        }
        #endregion 上傳&下載CSV
        #region Log匯出
        public IActionResult Download_Log()
        {
            if (!Startup.STK_Connect)
            {
                TempData["STK_Connect_Message"] = "目前資料庫未連接 確認連接後再重新開啟網頁";
                return RedirectToAction("Error_Page", "Frame");
            }
            else
            {
                TempData["STK_Connect_Message"] = "OK";
            }
            return View();
        }
        [HttpPost]
        public IActionResult Download_BatchList(IFormCollection post)
        {
            string folderPath = System.Environment.CurrentDirectory + "\\BatchList_CSV";
            string Batch_Time1_=string.Empty;
            string Batch_Time2_=string.Empty;
            FileStream fs;
            var C = new ASEWEB.Models.SQLContext();
            List<Download_BatchListModel> data = new List<Download_BatchListModel>();
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            if (post["Batch_Time1"][0].ToString().Trim() != null && post["Batch_Time1"][0].ToString().Trim() != "")
            {
                Batch_Time1_ = post["Batch_Time1"][0].ToString().Trim();
                Batch_Time1_ = Batch_Time1_.Remove(4, 1).Remove(6, 1);
            }
            if (post["Batch_Time2"][0].ToString().Trim() != null && post["Batch_Time2"][0].ToString().Trim() != "")
            {
                Batch_Time2_ = post["Batch_Time2"][0].ToString().Trim();
                Batch_Time2_ = Batch_Time2_.Remove(4, 1).Remove(6, 1);
            }
                
            List<string> DataList = C.Download_BatchList(Batch_Time1_, Batch_Time2_);
            for (int i = 0; i < DataList.Count; i += 5)
            {
                Download_BatchListModel datatmp = new Download_BatchListModel(DataList[i], DataList[i + 1], DataList[i + 2], DataList[i + 3], DataList[i + 4]);
                data.Add(datatmp);
            }
            string CSV_Path = folderPath + "\\BatchNo.csv";
            fs = new FileStream(CSV_Path, FileMode.Create);
            using (var file = new StreamWriter(fs, System.Text.Encoding.UTF8))
            {
                file.WriteLine("BOX ID,Batch No,Order No,Soteria,End Time");
                foreach (var item in data)
                {
                    file.WriteLine($"{item.BOX_ID},{item.Batch_No},{item.Order_No},{item.Soteria},{item.End_Time}");
                }
            }
            Stream iStream = new FileStream(CSV_Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(iStream, "application/octet-stream", $"產品條碼{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.csv");
        }
        [HttpPost]
        public IActionResult Download_CarouselUtility_UserHistory(IFormCollection post)
        {
            string folderPath = System.Environment.CurrentDirectory + "\\CarouselUtility_UserModify_CSV";
            string Modify_Time1_ = string.Empty;
            string Modify_Time2_ = string.Empty;
            FileStream fs;
            var C = new ASEWEB.Models.SQLContext();
            List<Download_CarouselUtility_Modify> data = new List<Download_CarouselUtility_Modify>();
            List<Download_CarouselUtility_Modify_History> data_detail = new List<Download_CarouselUtility_Modify_History>();
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                if (post["Modify_Time1"][0].ToString().Trim() != null && post["Modify_Time1"][0].ToString().Trim() != "")
                {
                    Modify_Time1_ = post["Modify_Time1"][0].ToString().Trim();
                    Modify_Time1_ = Modify_Time1_.Remove(4, 1).Remove(6, 1);
                }
                if (post["Modify_Time2"][0].ToString().Trim() != null && post["Modify_Time2"][0].ToString().Trim() != "")
                {
                    Modify_Time2_ = post["Modify_Time2"][0].ToString().Trim();
                    Modify_Time2_ = Modify_Time2_.Remove(4, 1).Remove(6, 1);
                }
                List<string> DataList = C.Download_CarouselUtility_History(Modify_Time1_, Modify_Time2_);
                List<string> DataList1 = C.Download_CarouselUtility_History_Detail(DataList[0]);
                for (int i = 0; i < DataList.Count; i += 2)
                {
                    Download_CarouselUtility_Modify datatmp = new Download_CarouselUtility_Modify(DataList[i], DataList[i + 1]);
                    data.Add(datatmp);
                }
                for (int i = 0; i < DataList1.Count; i += 8)
                {
                    Download_CarouselUtility_Modify_History datatmp = new Download_CarouselUtility_Modify_History(DataList1[i], DataList1[i + 1], DataList1[i + 2], DataList1[i + 3], DataList1[i + 4], DataList1[i + 5], DataList1[i + 6], DataList1[i + 7]);
                    data_detail.Add(datatmp);
                }
            }
            catch(Exception ex)
            {

            }

            string CSV_Path = folderPath + "\\CarouselUtility_UserModify.csv";
            fs = new FileStream(CSV_Path, FileMode.Create);
            using (var file = new StreamWriter(fs, System.Text.Encoding.UTF8))
            {
                file.WriteLine("使用者,修改時間,儲櫃,監控溫度上限值,監控溫度下限值,監控濕度上限值,監控濕度下限值,N2充填開始濕度值,N2充填結束濕度值");
                foreach (var item in data)
                {
                    file.WriteLine($"{item.User},{item.Command_ID}");
                    foreach (var item1 in data_detail)
                    {
                        file.WriteLine($",,{item1.Show_Carousel_ID},{item1.Temp_UPPER},{item1.Temp_LOWER},{item1.Hum_UPPER},{item1.Hum_LOWER},{item1.TURN_ON_N2},{item1.TURN_OFF_N2}");
                    }
                }
            }
            Stream iStream = new FileStream(CSV_Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(iStream, "application/octet-stream", $"用戶修改歷史{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.csv");
        }
        [HttpPost]
        public IActionResult Serach_Check_History(IFormCollection post)
		{
            var C = new ASEWEB.Models.SQLContext();
            List<Download_Check_History> data = new List<Download_Check_History>();
            string Check_End_Time1_ = string.Empty;
            string Check_End_Time2_ = string.Empty;
            if (post["Check_End_Time1"][0].ToString().Trim() != null && post["Check_End_Time1"][0].ToString().Trim() != "")
            {
                Check_End_Time1_ = post["Check_End_Time1"][0].ToString().Trim();
                Check_End_Time1_ = Check_End_Time1_.Remove(4, 1).Remove(6, 1);
            }
            if (post["Check_End_Time2"][0].ToString().Trim() != null && post["Check_End_Time2"][0].ToString().Trim() != "")
            {
                Check_End_Time2_ = post["Check_End_Time2"][0].ToString().Trim();
                Check_End_Time2_ = Check_End_Time2_.Remove(4, 1).Remove(6, 1);
            }
            List<string> DataList = C.Download_Check_History(Check_End_Time1_, Check_End_Time2_);
            for (int i = 0; i < DataList.Count; i += 6)
            {
                Download_Check_History datatmp = new Download_Check_History(DataList[i], DataList[i + 1], DataList[i + 2], DataList[i + 3], DataList[i + 4], DataList[i + 5]);
                data.Add(datatmp);
            }
            ViewBag.List = data;
            return PartialView();
		}
        public IActionResult Serach_Check_History()
		{
            ViewBag.List = "";
            return PartialView();
		}
        [HttpPost]
        public IActionResult Download_Check_History_Detail(IFormCollection post)
        {
            var C = new ASEWEB.Models.SQLContext();
            List<Download_Check_History> data = new List<Download_Check_History>();
            string Check_End_Time1_ = string.Empty;
            string Check_End_Time2_ = string.Empty;
            if (post["Check_End_Time1"][0].ToString().Trim() != null && post["Check_End_Time1"][0].ToString().Trim() != "")
            {
                Check_End_Time1_ = post["Check_End_Time1"][0].ToString().Trim();
                Check_End_Time1_ = Check_End_Time1_.Remove(4, 1).Remove(6, 1);
            }
            if (post["Check_End_Time2"][0].ToString().Trim() != null && post["Check_End_Time2"][0].ToString().Trim() != "")
            {
                Check_End_Time2_ = post["Check_End_Time2"][0].ToString().Trim();
                Check_End_Time2_ = Check_End_Time2_.Remove(4, 1).Remove(6, 1);
            }
            List<string> DataList = C.Download_Check_History(Check_End_Time1_, Check_End_Time2_);
            for (int i = 0; i < DataList.Count; i += 6)
            {
                Download_Check_History datatmp = new Download_Check_History(DataList[i], DataList[i + 1], DataList[i + 2], DataList[i + 3], DataList[i + 4], DataList[i + 5]);
                data.Add(datatmp);
            }
            string folderPath = System.Environment.CurrentDirectory + "\\Check_History_Detail_CSV";
            string[] Command_ID = post["Command_Select"];
            string[] User_ID = post["User_Select"];
            FileStream fs;

            List<Download_Check_History_Detail> data_detail = new List<Download_Check_History_Detail>();
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                for(int j=0; j < data.Count; j++)
				{
                    List<string> DataList1 = C.Download_Check_History_Detail(data[j].Command_ID,data[j].User);//Command_ID,User_ID
                    for (int i = 0; i < DataList1.Count; i += 10)
                    {
                        Download_Check_History_Detail datatmp = new Download_Check_History_Detail(DataList1[i], DataList1[i + 1], DataList1[i + 2], DataList1[i + 3], DataList1[i + 4], DataList1[i + 5], DataList1[i + 6], DataList1[i + 7], DataList1[i + 8], DataList1[i + 9]);
                        data_detail.Add(datatmp);
                    }
                }

            }
            catch (Exception ex)
            {

            }

            string CSV_Path = folderPath + "\\Check_History_Detail.csv";
            fs = new FileStream(CSV_Path, FileMode.Create);
            using (var file = new StreamWriter(fs, System.Text.Encoding.UTF8))
            {
                file.WriteLine("使用者,任務ID,儲櫃ID,儲格ID,產品編號,靜電箱ID,Group No,Soteria,客戶編號,盤點結果");
                foreach (var item1 in data_detail)
                    {
                        file.WriteLine($"{item1.User_ID},{item1.Command_ID},{item1.Carousel_ID},{item1.Cell_ID},{item1.Batch_No},{item1.BOX_ID},{item1.Group_No},{item1.Soteria},{item1.Customer_ID},{item1.Check_Result}");
                    }
            }
            Stream iStream = new FileStream(CSV_Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(iStream, "application/octet-stream", $"盤點歷史({DateTime.Now.ToString("yyyyMMddHHmmssfff")}).csv");
        }
        [HttpPost]
        public IActionResult Download_Alarm_History(IFormCollection post)
        {
            string folderPath = System.Environment.CurrentDirectory + "\\Alarm_History_CSV";
            string Alarm_Time1_ = string.Empty;
            string Alarm_Time2_ = string.Empty;
            FileStream fs;
            var C = new ASEWEB.Models.SQLContext();
            List<Download_Alarm> data = new List<Download_Alarm>();
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                if (post["Alarm_Time1"][0].ToString().Trim() != null && post["Alarm_Time1"][0].ToString().Trim() != "")
                {
                    Alarm_Time1_ = post["Alarm_Time1"][0].ToString().Trim();
					Alarm_Time1_ = Alarm_Time1_.Replace('-','/');
				}
                if (post["Alarm_Time2"][0].ToString().Trim() != null && post["Alarm_Time2"][0].ToString().Trim() != "")
                {
                    Alarm_Time2_ = post["Alarm_Time2"][0].ToString().Trim();
                    Alarm_Time2_ = Alarm_Time2_.Replace('-', '/');
                }
                List<string> DataList = C.Download_Alarm_History(Alarm_Time1_, Alarm_Time2_);
                for (int i = 0; i < DataList.Count; i += 5)
                {
                    Download_Alarm datatmp = new Download_Alarm(DataList[i], DataList[i + 1], DataList[i + 2], DataList[i + 3], DataList[i + 4]);
                    data.Add(datatmp);
                }
            }
            catch (Exception ex)
            {

            }

            string CSV_Path = folderPath + "\\AlarmHistory.csv";
            fs = new FileStream(CSV_Path, FileMode.Create);
            using (var file = new StreamWriter(fs, System.Text.Encoding.UTF8))
            {
                file.WriteLine("ID,Unit Name,Occured_Time,Reset_Time,Message");
                foreach (var item in data)
                {
                    file.WriteLine($"{item.ID},{item.Unit_Name},{item.Occured_Time},{item.Reset_Time},{item.Message}");
                }
            }
            Stream iStream = new FileStream(CSV_Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(iStream, "application/octet-stream", $"警報歷史{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.csv");
        }
        [HttpPost]
        public IActionResult Download_Task_History(IFormCollection post)
        {
            string folderPath = System.Environment.CurrentDirectory + "\\Task_History_CSV";
            string Task_Time1_ = string.Empty;
            string Task_Time2_ = string.Empty;
            FileStream fs;
            var C = new ASEWEB.Models.SQLContext();
            List<Download_TaskHistory> data = new List<Download_TaskHistory>();
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            if (post["Task_Time1"][0].ToString().Trim() != null && post["Task_Time1"][0].ToString().Trim() != "")
            {
                Task_Time1_ = post["Task_Time1"][0].ToString().Trim();
                Task_Time1_ = Task_Time1_.Remove(4, 1).Remove(6, 1);
            }
            if (post["Task_Time2"][0].ToString().Trim() != null && post["Task_Time2"][0].ToString().Trim() != "")
            {
                Task_Time2_ = post["Task_Time2"][0].ToString().Trim();
                Task_Time2_ = Task_Time2_.Remove(4, 1).Remove(6, 1);
            }

            List<string> DataList = C.Download_Task_History(Task_Time1_, Task_Time2_);
            for (int i = 0; i < DataList.Count; i += 12)
            {
                Download_TaskHistory datatmp = new Download_TaskHistory(DataList[i], DataList[i + 1], DataList[i + 2], DataList[i + 3], DataList[i + 4], DataList[i + 5], DataList[i + 6], DataList[i + 7], DataList[i + 8], DataList[i + 9], DataList[i + 10], DataList[i + 11]);
                data.Add(datatmp);
            }
            string CSV_Path = folderPath + "\\TaskHistory.csv";
            fs = new FileStream(CSV_Path, FileMode.Create);
            using (var file = new StreamWriter(fs, System.Text.Encoding.UTF8))
            {
                file.WriteLine("BOX ID,Batch No,Src Pos,Src Cell,Tar Pos,Tar Cell,Status,Direction,NG Reason,Start Time,End Time,User ID");
                foreach (var item in data)
                {
					if (item.Status == "77")
					{
                        file.WriteLine($"{item.BOX_ID},{item.Batch_No},{item.SRC_POS},{item.SRC_CELL},{item.TAR_POS},{item.TAR_CELL},OK({item.Status}),{item.Direction},{item.NG_Reason},{item.Start_Time},{item.End_Time},{item.User_ID}");
                    }
                    else if (item.Status == "99")
					{
                        file.WriteLine($"{item.BOX_ID},{item.Batch_No},{item.SRC_POS},{item.SRC_CELL},{item.TAR_POS},{item.TAR_CELL},NG({item.Status}),{item.Direction},{item.NG_Reason},{item.Start_Time},{item.End_Time},{item.User_ID}");
                    }
                        
                }
            }
            Stream iStream = new FileStream(CSV_Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(iStream, "application/octet-stream", $"任務歷史{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.csv");
        }
        [HttpPost]
        public IActionResult Download_STK_Carousel_Daily_History(IFormCollection post)
		{
            string folderPath = System.Environment.CurrentDirectory + "\\STK_Carousel_Daily_History_CSV";
            string STK_Carousel_Time1_ = string.Empty;
            string STK_Carousel_Time2_ = string.Empty;
            FileStream fs;
            var C = new ASEWEB.Models.SQLContext();
            List<Download_STK_CarouselDaily_History> data = new List<Download_STK_CarouselDaily_History>();
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            if (post["STK_Carousel_Time1"][0].ToString().Trim() != null && post["STK_Carousel_Time1"][0].ToString().Trim() != "")
            {
                STK_Carousel_Time1_ = post["STK_Carousel_Time1"][0].ToString().Trim();
                STK_Carousel_Time1_ = STK_Carousel_Time1_.Replace('-','/');
            }
            if (post["STK_Carousel_Time2"][0].ToString().Trim() != null && post["STK_Carousel_Time2"][0].ToString().Trim() != "")
            {
                STK_Carousel_Time2_ = post["STK_Carousel_Time2"][0].ToString().Trim();
                STK_Carousel_Time2_ = STK_Carousel_Time2_.Replace('-', '/');
            }

            List<string> DataList = C.Download_STK_Carousel_Daily_History(STK_Carousel_Time1_, STK_Carousel_Time2_);
            for (int i = 0; i < DataList.Count; i += 10)
            {
                Download_STK_CarouselDaily_History datatmp = new Download_STK_CarouselDaily_History(DataList[i], DataList[i + 1], DataList[i + 2], DataList[i + 3], DataList[i + 4], DataList[i + 5], DataList[i + 6], DataList[i + 7], DataList[i + 8], DataList[i + 9]);
                data.Add(datatmp);
            }
            string CSV_Path = folderPath + "\\STK_Carousel_Daily_History.csv";
            fs = new FileStream(CSV_Path, FileMode.Create);
            using (var file = new StreamWriter(fs, System.Text.Encoding.UTF8))
            {
                file.WriteLine("儲櫃 ID,最高溫度,最低溫度,平均溫度,最高濕度,最低濕度,平均濕度,Log創建時間,開啟次數,總開啟時間");
                foreach (var item in data)
                {
                    file.WriteLine($"{item.Carousel_ID},{item.Max_T},{item.Min_T},{item.Avg_T},{item.Max_H},{item.Min_H},{item.Avg_H},{item.Creation_Date},{item.OP_Times},{item.Total_Sec}");
                }
            }
            Stream iStream = new FileStream(CSV_Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(iStream, "application/octet-stream", $"每日CarouselLog({DateTime.Now.ToString("yyyyMMddHHmmssfff")}).csv");
        }
        [HttpPost]
        public IActionResult Download_STK_Door_History(IFormCollection post)
        {
            string folderPath = System.Environment.CurrentDirectory + "\\STK_Door_History_CSV";
            string STK_Door_OP_Time1_ = string.Empty;
            string STK_Door_OP_Time2_ = string.Empty;
            FileStream fs;
            var C = new ASEWEB.Models.SQLContext();
            List<Download_STK_DoorHistory> data = new List<Download_STK_DoorHistory>();
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            if (post["STK_Door_OP_Time1"][0].ToString().Trim() != null && post["STK_Door_OP_Time1"][0].ToString().Trim() != "")
            {
                STK_Door_OP_Time1_ = post["STK_Door_OP_Time1"][0].ToString().Trim();
            }
            if (post["STK_Door_OP_Time2"][0].ToString().Trim() != null && post["STK_Door_OP_Time2"][0].ToString().Trim() != "")
            {
                STK_Door_OP_Time2_ = post["STK_Door_OP_Time2"][0].ToString().Trim();
            }

            List<string> DataList = C.Download_STK_Door_History(STK_Door_OP_Time1_, STK_Door_OP_Time2_);
            for (int i = 0; i < DataList.Count; i += 4)
            {
                Download_STK_DoorHistory datatmp = new Download_STK_DoorHistory(DataList[i], DataList[i + 1], DataList[i + 2], DataList[i + 3]);
                data.Add(datatmp);
            }
            string CSV_Path = folderPath + "\\STK_Door_History.csv";
            fs = new FileStream(CSV_Path, FileMode.Create);
            using (var file = new StreamWriter(fs, System.Text.Encoding.UTF8))
            {
                file.WriteLine("儲櫃 ID,開啟時間,關閉時間,開啟總時長(秒數)");
                foreach (var item in data)
                {
                    file.WriteLine($"{item.Carousel_ID},{item.OP_Time},{item.Close_Time},{item.OP_Sec}");
                }
            }
            Stream iStream = new FileStream(CSV_Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(iStream, "application/octet-stream", $"Carousel開關門Log({DateTime.Now.ToString("yyyyMMddHHmmssfff")}).csv");
        }
        //public IActionResult Download_TH_Log()
        //{
        //    string folderPath = System.Environment.CurrentDirectory + "\\TH_CSV";
        //    FileStream fs;
        //    var C = new ASEWEB.Models.SQLContext();
        //    //List<Download_CSVModel> data = new List<Download_CSVModel>();
        //    if (!Directory.Exists(folderPath))
        //    {
        //        Directory.CreateDirectory(folderPath);
        //    }
        //    List<string> DataList = C.Download_List();
        //    for (int i = 0; i < DataList.Count; i += 6)
        //    {
        //        //Download_CSVModel datatmp = new Download_CSVModel(DataList[i], DataList[i + 1], DataList[i + 2], DataList[i + 3], DataList[i + 4], DataList[i + 5]);
        //        data.Add(datatmp);
        //    }
        //    string CSV_Path = folderPath + "\\download.csv";
        //    fs = new FileStream(CSV_Path, FileMode.Create);
        //    using (var file = new StreamWriter(fs, System.Text.Encoding.UTF8))
        //    {
        //        foreach (var item in data)
        //        {
        //            //file.WriteLine($"{item.ID},{item.Name},{item.Password},{item.Group},{item.Auth},{item.UserSystemSetting}");
        //        }
        //    }
        //    Stream iStream = new FileStream(CSV_Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        //    return File(iStream, "application/octet-stream", "download.csv");
        //}
        #endregion
    }
}
