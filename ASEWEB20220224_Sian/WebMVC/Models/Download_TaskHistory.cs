using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ASEWEB.Models
{
    public class Download_TaskHistory
    {
        public string BOX_ID  { get; set; }
        public string Batch_No { get; set; }
        public string Status { get; set; }
        public string User_ID { get; set; }
        public string SRC_POS { get; set; }
        public string SRC_CELL { get; set; }
        public string TAR_POS { get; set; }
        public string TAR_CELL { get; set; }
        public string Direction  { get; set; }
        public string Start_Time { get; set; }
        public string End_Time { get; set; }
        public string NG_Reason { get; set; }

        public Download_TaskHistory()
        {
            BOX_ID = string.Empty;
            Batch_No = string.Empty;
            Status = string.Empty;
            User_ID = string.Empty;
            SRC_POS = string.Empty;
            SRC_CELL = string.Empty;
            TAR_POS = string.Empty;
            TAR_CELL = string.Empty;
            Direction = string.Empty;
            Start_Time = string.Empty;
            End_Time = string.Empty;
            NG_Reason = string.Empty;

        }
        public Download_TaskHistory(string BOX_ID_, string Batch_No_, string SRC_POS_, string SRC_CELL_, string TAR_POS_, string TAR_CELL_, string Status_, string Direction_, string NG_Reason_, string Start_Time_,string End_Time_, string User_ID_)
        {
            BOX_ID = BOX_ID_;
            Batch_No = Batch_No_;
            SRC_POS = SRC_POS_;
            SRC_CELL = SRC_CELL_;
            TAR_POS = TAR_POS_;
            TAR_CELL = TAR_CELL_;
            Status = Status_;
            User_ID = User_ID_;
            Direction = Direction_;
            Start_Time = Start_Time_;
            End_Time = End_Time_;
            NG_Reason = NG_Reason_;
        }
    }
}