using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ASEWEB.Models
{
    public class Download_BatchListModel
    {
        public string BOX_ID  { get; set; }
        public string Batch_No  { get; set; }
        public string Order_No  { get; set; }
        public string Soteria  { get; set; }
        public string End_Time { get; set; }
        public string UserSystemSetting { get; set; }
        public List<string> TM_List { get; set; }
        public Download_BatchListModel()
        {
            BOX_ID = string.Empty;
            Batch_No = string.Empty;
            Order_No = string.Empty;
            Soteria  = string.Empty;
            End_Time = string.Empty;
            UserSystemSetting = string.Empty;
        }
        public Download_BatchListModel(string BOX_ID_,string Batch_No_,string Order_No_,string Soteria_,string End_Time_)
        {
            BOX_ID = BOX_ID_;
            Batch_No = Batch_No_;
            Order_No = Order_No_;
            Soteria  = Soteria_;
            End_Time = End_Time_;
        }
    }
}