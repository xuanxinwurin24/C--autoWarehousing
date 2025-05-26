using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using ASEWEB.Models;
namespace ASEWEB.Models
{
    public class Check_inventoryModel
    {
        public string id { get; set; }
        public string User { get; set; }
        public string Now { get; set; }
        public List<string> Time { get; set; }
        public List<string> Day { get; set; }
        public List<string> Week { get; set; }
        public string Batch_No1 { get; set; }
        public string Batch_No2 { get; set; }
        public string Carousel_ID1 { get; set; }
        public string Carousel_ID2 { get; set; }
        public string Soteria1 { get; set; }
        public string Soteria2 { get; set; }
        public string Customer_ID1 { get; set; }
        public string Customer_ID2 { get; set; }
        public string DaysApart1 { get; set; }
        public string DaysApart2 { get; set; }
        public string Carousel_ID { get; set; }
        public string Cell_ID { get; set; }
        public string BOX_ID { get; set; }
        public string Batch_No { get; set; }
        public string Check_Result { get; set; }
        public string Soteria { get; set; }
        public string Customer { get; set; }
        public string DaysApart { get; set; }
        public Check_inventoryModel()
        {
            Batch_No1 = string.Empty;
            Batch_No2 = string.Empty;
            Carousel_ID1 = string.Empty;
            Carousel_ID2 = string.Empty;
            Soteria1 = string.Empty;
            Soteria2 = string.Empty;
            Customer_ID1 = string.Empty;
            Customer_ID2 = string.Empty;
            Now = string.Empty;
            Time = new List<string>();
            Day = new List<string>();
            Week = new List<string>();
            Batch_No = string.Empty;
            Carousel_ID = string.Empty;
            Cell_ID = string.Empty;
            BOX_ID = string.Empty;
            Check_Result = string.Empty;
            Soteria = string.Empty;
            Customer = string.Empty;
            DaysApart = string.Empty;
        }
        public Check_inventoryModel(string _Batch_No1, string _Batch_No2, string _Carousel_ID1, string _Carousel_ID2, string _Soteria1, string _Soteria2, string _Customer_ID1, string _Customer_ID2, string _DaysApart1, string _DaysApart2)
        {

            Batch_No1 = _Batch_No1;
            Batch_No2 = _Batch_No2;
            Carousel_ID1 = _Carousel_ID1;
            Carousel_ID2 = _Carousel_ID2;
            Soteria1 = _Soteria1;
            Soteria2 = _Soteria2;
            Customer_ID1 = _Customer_ID1;
            Customer_ID2 = _Customer_ID2;
            DaysApart1 = _DaysApart1;
            DaysApart2 = _DaysApart2;

            Now = string.Empty;
            Time = new List<string>();
            Day = new List<string>();
            Week = new List<string>();
            Batch_No = string.Empty;
            Carousel_ID = string.Empty;
            Cell_ID = string.Empty;
            BOX_ID = string.Empty;
            Check_Result = string.Empty;
        }
        public Check_inventoryModel(string _Batch_No, string _Carousel_ID, string _Cell_ID, string _BOX_ID,string _Soteria,string _Customer,string _DaysApart, string _Check_Result)
        {
            Batch_No = _Batch_No;
            Carousel_ID = _Carousel_ID;
            Cell_ID = _Cell_ID;
            BOX_ID = _BOX_ID;
            Soteria = _Soteria;
            Customer = _Customer;
            DaysApart = _DaysApart;
            Check_Result = _Check_Result;
        }
        public Check_inventoryModel(string _Now, List<string> _Time, List<string> _Week, List<string> _Day, string _User)
        {
            Now = string.Empty;
            Time = new List<string>();
            Day = new List<string>();
            Week = new List<string>();
            Now = _Now;
            Time = _Time;
            Week = _Week;
            Day = _Day;
            User = _User;
        }
        public Check_inventoryModel(string rCSID, string rCEID, string rBOXID, string _Now, List<string> _Time, List<string> _Week, List<string> _Day, string _User)
        {
            Now = string.Empty;
            Time = new List<string>();
            Day = new List<string>();
            Week = new List<string>();
            Batch_No = string.Empty;
            Carousel_ID = string.Empty;
            Cell_ID = string.Empty;
            BOX_ID = string.Empty;
            Check_Result = string.Empty;
            Carousel_ID = rCSID;
            Cell_ID = rCEID;
            BOX_ID = rBOXID;
            Now = _Now;
            Time = _Time;
            Week = _Week;
            Day = _Day;
            User = _User;
        }
        public string Check_Inventory_insert()
        {
            var C = new ASEWEB.Models.SQLContext();
            return C.Insert_Check(Now, Time, Day, Week, User);
        }
        public List<string> Check_Inventory_string()
        {
            var C = new ASEWEB.Models.SQLContext();
            return C.Check_inventory_Context(Batch_No1, Batch_No2, Carousel_ID1, Carousel_ID2, Soteria1, Soteria2, Customer_ID1, Customer_ID2, DaysApart1, DaysApart2);
        }
        public List<string> Check_Inventory_History()
        {
            var C = new ASEWEB.Models.SQLContext();

            return C.Check_Inventory_History();
        }
        public List<string> Check_Inventory_HistoryList(string commandid)
        {
            var C = new ASEWEB.Models.SQLContext();
            return C.Check_Inventory_History_Detail(commandid);
        }
        public List<string> Check_Inventory_TaskList(string commandid)
        {
            var C = new ASEWEB.Models.SQLContext();
            return C.Check_Inventory_Task_Detail(commandid);
        }
        public List<string> Check_Inevntory_Task()
        {
            var C = new ASEWEB.Models.SQLContext();
            return C.Check_Inventory_Task();
        }
        public void Set_Check_Inventory(string rCSID, string rCEID, string rBOXID,string rBatch_No,string rSoteria,string rCustomer)
        {
            Batch_No = string.Empty;
            Carousel_ID = string.Empty;
            Cell_ID = string.Empty;
            BOX_ID = string.Empty;
            Check_Result = string.Empty;
            Carousel_ID = rCSID;
            Cell_ID = rCEID;
            BOX_ID = rBOXID;
            Batch_No = rBatch_No;
            Soteria = rSoteria;
            Customer = rCustomer;
        }
        public void Check_Inventory_insert_detail(string comid)
        {
            var C = new ASEWEB.Models.SQLContext();
            if((Now==""|| Now==null) && (Time.Count==0) && (Day.Count==0) && (Week.Count==0))
                return ;
            if(Day.Count!=0||Week.Count!=0)
                C.Insert_Check_Schedule_Detail(Carousel_ID,Cell_ID,BOX_ID);
            if (Time.Count != 0 || (Now != "" && Now != null))
                C.Insert_Check_List_Detail(Carousel_ID, Cell_ID, BOX_ID,Batch_No,Soteria,Customer,comid);
            return ;
        }
    }
}