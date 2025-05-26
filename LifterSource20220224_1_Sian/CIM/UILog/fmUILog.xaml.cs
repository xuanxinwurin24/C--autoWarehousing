using System.Windows;
using System;
using Strong;
using System.Collections.Generic;
//using CIM.BC;
using System.Windows.Controls;
using CIM.Lib.Model;

namespace CIM.UILog
{
    /// <summary>
    /// Interaction logic for fmLog.xaml
    /// </summary>

    public partial class fmUILog : Window
    {
        //public List<frmRobot_IFSigLog> frmRobotPosLogLst = new List<frmRobot_IFSigLog>();
        //internal object frmCimMsgLog;

        public fmUILog()
        {
            InitializeComponent();
            Topmost = true;

            //frmSecsLog.LogFile.PathName = Environment.CurrentDirectory + @"\LogFile\SecsLog";
            frmSysLog.LogFile.PathName = Environment.CurrentDirectory + @"\LogFile\SystemLog";
            frmAlarmLog.LogFile.PathName = Environment.CurrentDirectory + @"\LogFile\AlarmLog";
            frmAlarmHistoryLog.LogFile.PathName = Environment.CurrentDirectory + @"\LogFile\AlarmHistoryLog";

            //frmSecsLog.LogFile.sHead = "Secs";
            frmAlarmLog.LogFile.sHead = "Alarm";
            frmAlarmHistoryLog.LogFile.sHead = "AlarmHistory";

            //newRobotPosLog();
            //frmIFLog.lv.AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(MouseDoubleClickEventf));
        }

        private void MouseDoubleClickEventf(object sender, RoutedEventArgs e)
        {
            //ListView lv = sender as ListView;
            //if (lv == null) return;
            //if (lv.SelectedIndex == -1) return;
            //var body = (IFMsgBody)lv.SelectedItem;
            //if (frmIFSigDetail == null)
            //{ frmIFSigDetail = new frmIFSigDetail(MainWindow.bIF_Send, MainWindow.wIF_Send, MainWindow.bIF_Rcv, MainWindow.wIF_Rcv); }
            //frmIFSigDetail.MyUpdate(body.EqSig, body.EqData, body.RobotSig, body.RobotData);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        //public static void EventLog(frmEqLog frmlog_, TagItem item_)
        //{
        //	try
        //	{
        //		string TagStr = item_.Hint.Trim().Length != 0 ? item_.Hint : item_.Name;
        //		string GroupStr = item_.Mg.Hint.Trim().Length != 0 ? item_.Mg.Hint : item_.Mg.Name;
        //		frmlog_.Log(GroupStr + ":" + TagStr + "=" + item_.StringValue);
        //		//frmlog_.Log("\t\t\t\t" + GroupStr + ":" + TagStr + "=" + item_.StringValue);
        //	}
        //	catch (Exception ex)
        //	{
        //		LogExcept.LogException(ex);
        //	}
        //}
    }
}
