using Strong;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CIM.Lib.Model
{
    public class HSMSpar
    {
        public static string FileName = App.sSysDir + @"\Ini\HSMSsetting.xml";

        public int HT3 { get; set; }
        public int HT5 { get; set; }
        public int HT6 { get; set; }
        public int HT7 { get; set; }
        public int HT8 { get; set; }
        public int HT9 { get; set; }
        public int LinkTime { get; set; }

        public int LocalPort { get; set; }
        public int RemotePort { get; set; }
        public string LocalIP { get; set; }
        public string RemoteIP { get; set; }

        public bool Active { get; set; }
        public int DeviceID { get; set; }
        public int EDCTime { get; set; }

        //public static HSMSpar LoadFromFile()
        //{
        //    try
        //    {
        //        if (File.Exists(sFileName) == false)
        //        {
        //            MessageBox.Show(sFileName + " no exist");
        //            Environment.Exit(Environment.ExitCode);
        //        }
        //        HSMSpar my = Common.DeserializeXMLFileToObject<HSMSpar>(sFileName);
        //        return my;
        //    }
        //    catch (Exception e_)
        //    { MessageBox.Show(sFileName + " File read error," + e_.ToString()); }
        //    return null;
        //}
        //public static void SaveFile(HSMSpar obj_)
        //{
        //    try
        //    {
        //        Common.SerializeXMLObjToFile<HSMSpar>(sFileName, obj_);
        //    }
        //    catch (Exception e_)
        //    { MessageBox.Show(sFileName + " File read error," + e_.ToString()); }
        //}
    }
}