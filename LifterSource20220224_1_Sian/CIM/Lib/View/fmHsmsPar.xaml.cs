using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CIM.Lib.View
{
    /// <summary>
    /// Interaction logic for fmHamsPar.xaml
    /// </summary>
    public partial class fmHsmsPar : Window
    {
        HSMSpar para;
        public fmHsmsPar()
        {
            para = Common.DeserializeXMLFileToObject<HSMSpar>(HSMSpar.FileName);
            InitializeComponent();
            DataContext = para;

            LogInOutEventCallBackFunc(null, null);
            Password.LogInOutEvent += LogInOutEventCallBackFunc;
            
        }
        void LogInOutEventCallBackFunc(string sOldUserName_, string sNewUserName_)
        {
            stackMain.IsHitTestVisible = Password.CurnLevel > 1;
            //gbIP.IsHitTestVisible = Password.CurnLevel > 1;
            //grid.IsHitTestVisible = Password.CurnLevel > 1;
        }    
        bool SaveToFile()
        {
            try
            {
                if (InputCorrect() == false) return false;
                Common.SerializeXMLObjToFile<HSMSpar>(HSMSpar.FileName, para);
                MessageBox.Show("You need to restart program for activing these changes!");
            }
            catch (Exception e_) { LogWriter.LogAndExit("Data input error!"); }
            return true;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveToFile() == false) return;
            //        sysPara.HSMSLoadFromFile();
            Close();
        }
        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            para.Active = true;
            para.LocalIP = "127.0.0.1";
            para.LocalPort = 5000;
            para.RemoteIP = "127.0.0.1";
            para.RemotePort = 5000;
            para.LinkTime = 60;
            para.HT3 = 45;
            para.HT5 = 10;
            para.HT6 = 5;
            para.HT7 = 10;
            para.HT8 = 5;
            para.HT9 = 60;

            DataContext = null;//refresh UI
            DataContext = para;
        }        
        bool InputCorrect()
        {
            //string Err = "";
            //if (!IsInteger(edLocalPort, 0, 32767)) Err = "LocalPort range is 0~32767 number";
            //else if (!IsInteger(edRemotePort, 0, 32767)) Err = "RemotePort range is 0~32767 number";
            //else if (!IsInteger(edLinkTestTimer, 5, 90)) Err = "LinkTestTimer range is 5~90 number";
            //else if (!IsInteger(edT3, 1, 120)) Err = "T3 range is 1~120 number";
            //else if (!IsInteger(edT5, 1, 240)) Err = "T5 range is 1~240 number";
            //else if (!IsInteger(edT6, 1, 240)) Err = "T6 range is 1~240 number";
            //else if (!IsInteger(edT7, 1, 240)) Err = "T7 range is 1~240 number";
            //else if (!IsInteger(edT8, 1, 120)) Err = "T8 range is 1~120 number";
            //else if (!IsInteger(edT9, 1, 999)) Err = "T9 range is 1~999 number";
            //else if (!IsIP(edLocalIP)) Err = "Local IP format is illlegal";
            //else if (!IsIP(edRemoteIP)) Err = "Remote IP IP format is illlegal";

            //else if (!IsInteger(edDeviceID, 0, 65535)) Err = "Device ID  range is 0~65535 number";

            //if (Err == "") { /*fmMain.frmAlarm.AlarmReset(1000); */return true; }
            //else
            //{
            //    MessageBox.Show(Err);
            //    return false;
            //}
            return true;
        }
        ////---------------------------------------------------------------------------
        //bool IsIP(Edit ed)
        //{
        //    string CorrectIP = IsIPAddress(ed.Text.Trim());
        //    if (CorrectIP == "")
        //    {
        //        ed.Color = clYellow;
        //        return false;
        //    }
        //    ed.Color = clWhite;
        //    ed.Text = CorrectIP;
        //    return true;
        //}
        //---------------------------------------------------------------------------
        //string IsIPAddress(string V)
        //{
        //    string CorrectIP = "";
        //    if (V.Length() < 7) return CorrectIP;
        //    V = V + string::StringOfChar(' ', 6);
        //    int p1 = V.Pos(".");
        //    if (p1 <= 1 || p1 > 4) return CorrectIP;
        //    string IP1 = V.SubString(1, p1 - 1).Trim();
        //    V = V.SubString(p1 + 1, V.Length() - p1);

        //    p1 = V.Pos(".");
        //    if (p1 <= 1 || p1 > 4) return CorrectIP;
        //    string IP2 = V.SubString(1, p1 - 1).Trim();
        //    V = V.SubString(p1 + 1, V.Length() - p1);

        //    p1 = V.Pos(".");
        //    if (p1 <= 1 || p1 > 4) return CorrectIP;
        //    string IP3 = V.SubString(1, p1 - 1).Trim();
        //    string IP4 = V.SubString(p1 + 1, V.Length() - p1).Trim();

        //    if (IP4.Pos(".") > 0) return CorrectIP;

        //    int i1 = IP1.ToIntDef(0);
        //    if (i1 == 0 && IP1 != "0") return CorrectIP;
        //    int i2 = IP2.ToIntDef(0);
        //    if (i2 == 0 && IP2 != "0") return CorrectIP;
        //    int i3 = IP3.ToIntDef(0);
        //    if (i3 == 0 && IP3 != "0") return CorrectIP;
        //    int i4 = IP4.ToIntDef(0);
        //    if (i4 == 0 && IP4 != "0") return CorrectIP;
        //    if (i1 < 0 || i2 < 0 || i3 < 0 || i4 < 0) return CorrectIP;
        //    if (i1 > 255 || i2 > 255 || i3 > 255 || i4 > 255) return CorrectIP;
        //    CorrectIP = IntToStr(IP1.ToIntDef(0)) + "." +
        //                IntToStr(IP2.ToIntDef(0)) + "." +
        //                IntToStr(IP3.ToIntDef(0)) + "." +
        //                IntToStr(IP4.ToIntDef(0));
        //    return CorrectIP;
        //}
        ////---------------------------------------------------------------------------
        //bool IsInteger(TEdit* ed, int Min, int Max)
        //{
        //    bool Sure = true;
        //    int i = ed.Text.ToIntDef(0);
        //    if (i == 0)
        //        if (ed.Text.Trim() != "0")
        //            Sure = false;

        //    if (i < Min) Sure = false;
        //    if (i > Max) Sure = false;
        //    ed.Color = (Sure ? clWhite : clYellow);
        //    return Sure;
        //}
    }
}
