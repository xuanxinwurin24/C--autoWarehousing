using CIM.BC;
using Strong;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;

namespace CIM.Lib.Model
{
    public class SysPara
    {
        public static string FileName = App.sSysDir + @"\Ini\SysPara.xml";
        public string SoftVersion { get; set; }
        public string ModelName { get; set; }
        public string EqID { get; set; }
        public string InLineID { get; set; }
        public bool Simulation { get; set; }
        public string SqlDB_data_source { get; set; }
        public string SqlDB_catalog { get; set; }
        public string SqlDB_uid { get; set; }
        public string SqlDB_password { get; set; }
        public string SqlDB_LifterUnit { get; set; }
        public int RFID_Retry_Times { get; set; }
        public int CleanITReplyTime { get; set; }

        public bool TEST { get; set; }
        public SysPara()
        {
        }
    }
}
