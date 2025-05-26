using CIM.Lib.View;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CIM.Lib.Model;
using CIM.BC;
using CIM.UILog;
using CIM.BC.View;
using System.Data;
namespace CIM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static fmUILog Log;
        public fmCimMessage fmCimMessage;
        public fmSecond fmSecond;
        public static 

        DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            App.Current.MainWindow = this;
            Log = new fmUILog();
            fmCimMessage = new fmCimMessage();
            fmSecond = new fmSecond();
            lbUserName.DataContext = App.DS.OP;
            lbLevel.DataContext = App.DS.OP;
            //DeviceMemoryView.MenuForMemStatus(MainMenu);
            Password.LogInOutEvent += LogInOutEventCallBackFunc;
            if (App.SysPara.Simulation)
            {
                Password.Administrator_Login();
                edLogin_UserName.Text = "STRONG"; //Simu
            }
            App.Start();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            fmSecond.lbScanTime.Text = App.MainThread.ScanTime.ToString("00");
            fmSecond.DateTimeValue = DateTime.Now.ToString("yy/MM/dd-HH:mm:ss");
            fmSecond.lbConnected.Tag = App.Eq.Eq_Alive == true ? "1" : "0";
            fmSecond.lbPLCStatus.Tag = App.Eq.bEqAuto == true ? "1" : "0";
            lbASEConnected.Tag = App.DS.bLifterIsOnline == true ? "1" : "0";
            fmSecond.lbASEConnected.Tag = App.DS.bLifterIsOnline == true ? "1" : "0";
            fmSecond.lbPLCMode.Tag = App.Eq.bPLCMode == true ? "1" : "0";
            //fmSecond.lv.ItemsSource = null;
            //fmSecond.lv.ItemsSource = App.DS.TASKList;
            if (App.DS.UpdateDelivery)
            {
                fmSecond.uDelivery.ucNormal.dataGrid1.DataContext = App.DS;
                fmSecond.uDelivery.ucIssue.dataGrid1.DataContext = App.DS;
                fmSecond.uDelivery.ucConf.dataGrid1.DataContext = App.DS;
                fmSecond.uDelivery.ucProb.dataGrid1.DataContext = App.DS;
            }

            Visibility = fmSecond.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            fmSecond.OfflineViewSet(App.DS.bLifterIsOnline);

            if (App.DS.NowReqFrame != DeliStore.ucFrame.None)
            {
                fmSecond.ReqIT_Event();
            }

            if (App.DS.NowActFrame != DeliStore.ucFrame.None)
            {
                fmSecond.ITAck_Event();
                if (App.DS.bAskOrderReplyNG)
                {
                    fmSecond.OrderNoReplyNG();
                }
                if (App.DS.UpdateDelivery)
                {
                    App.DS.NowActFrame = DeliStore.ucFrame.None;
                }
                App.DS.LastAckFrame = App.DS.NowActFrame;
                App.DS.NowActFrame = DeliStore.ucFrame.None;
            }
            //20211214 Ching Mark for No Card Input Test
            if (App.DS.bLifterIsOnline)
                {
                if (!App.DS.WebServiceDemo)
                {
                    if (App.DS.bBackToMainWindow)
                    {
                        fmSecond.Visibility = Visibility.Hidden;
                        App.DS.bBackToMainWindow = false;
                        edLogin_UserName.Text = "";
                        App.DS.OP.Logout();
                    }
                    //RFID讀到卡號 丟給MPC取得Operator的工號和Level [未做]
                    //取得的工號和Level填入App.DS.Operator中;
                    Visibility = Visibility.Visible;
                    lbUserName.DataContext = App.RFIDReader;
                    edLogin_UserName.Text = App.DS.OP.UserName;

                    edLogin_UserName.IsReadOnly = true;
                    if (App.SysPara.Simulation)
                    {
                        edLogin_UserName.Text = "Strong";
                    }
                    else
                    {
                        App.DS.OP.Logout();
                        edLogin_UserName.Text = App.DS.OP.UserName;
                    }
                    tbLogin_Password.Visibility = Visibility.Hidden;
                    edLogin_Password.Visibility = Visibility.Hidden;
                    btnLogin.Visibility = Visibility.Hidden;
                }
            }
                else
                {
                if (App.DS.bBackToMainWindow)
                {
                    fmSecond.Visibility = Visibility.Hidden;
                    edLogin_UserName.Text = "";
                    App.DS.bBackToMainWindow = false;
                }
                //離線時以離線帳密做登入
                //設定的帳密放入與Local資料庫相符 此資料庫尚未建立，[未做]
                //將其對應的工號和Level填入App.DS.Operator中
                lbUserName.DataContext = App.DS.OP;
                lbLevel.DataContext = App.DS.OP;
                edLogin_UserName.IsReadOnly = false;
                tbLogin_Password.Visibility = Visibility.Visible;
                edLogin_Password.Visibility = Visibility.Visible;
                btnLogin.Visibility = Visibility.Visible;
            }
            if (App.Eq.bStoreNGchangeToManual)
            {
                fmSecond.StoreNGchangeToManual();
            }
        }

        void LogInOutEventCallBackFunc(string sOldUserName_, string sNewUserName_)
        {
            App.LogFile.AddString("Log Out:" + sOldUserName_ + "  Log In:" + sNewUserName_);
            if (App.MainThread != null)
            { App.MainThread.PasswordLevel = Password.CurnLevel; }
            PasswordChanged(Password.CurnLevel);
        }
        public void PasswordChanged(int iLevel_)
        {
            //mnAlarmCode.Visibility = Password.CurnLevel >= 9 ? Visibility.Visible : Visibility.Collapsed;
            //mnRecipe.IsEnabled = Password.CurnLevel >= 3;

            //stackMain.IsHitTestVisible = Password.CurnLevel >= 9;
        }
        private void mnPassword_Click(object sender, RoutedEventArgs e)
        {
            fmPassword fmPassword = new fmPassword();
            //fmPassword.mnNewUser.IsEnabled = Password.CurnLevel >= 3;
            //fmPassword.mnDelete.IsEnabled = Password.CurnLevel >= 3;
            //fmPassword.BringToFront();
            fmPassword.ShowDialog();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.Log.frmAlarmHistoryLog.SaveToFile();
            Application.Current.Shutdown();
        }

        private void mnLog_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Log.Show();
        }

        private void mnSysPar_Click(object sender, RoutedEventArgs e)
        {
            fmSysPar fm = new fmSysPar();
            fm.ShowDialog();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (App.SysPara.Simulation)
            {
                if (edLogin_UserName.Text.Trim() == Password.CurnUserName)
                {
                    //fmSecond.Login(Password.CurnUserName, Password.CurnLevel.ToString());
                    fmSecond.Visibility = Visibility.Visible;
                    App.DS.OP.UserID = "Strong";
                    edLogin_Password.Password = "";
                    return;
                }
                else if (edLogin_UserName.Text.Trim().ToUpper() == "STRONG")
                {
                    fmSecond.Visibility = Visibility.Visible;
                    App.DS.OP.UserName = "STRONG";
                    App.DS.OP.UserID = "Strong";
                    App.DS.OP.Level = "Admin";
                    App.DS.OP.LogIn = true;
                    //App.DS.OP.Level = 9;
                    edLogin_Password.Password = "";
                    return;
                }
            }
            DataTable Result = new DataTable();
            string sql = $"SELECT USER_ID,USER_NAME,AUTHORITY_TYPE FROM [ACCOUNT] WHERE USER_ID='{edLogin_UserName.Text}' AND USER_PASS='{edLogin_Password.Password}'";
            App.Local_SQLServer.Query(sql, ref Result);
            foreach(DataRow dr in Result.Rows)
            {
                App.DS.OP.UserID = dr["USER_ID"].ToString().Trim();
                App.DS.OP.UserName = dr["USER_NAME"].ToString().Trim();
                App.DS.OP.Level = dr["AUTHORITY_TYPE"].ToString().Trim();
                edLogin_Password.Password = "";
                App.DS.OP.LogIn = true;
                fmSecond.Visibility = Visibility.Visible;
                return;
            }

            edLogin_Password.Password = "";
            App.DS.CIMMessage("LogIn Message", "No Account");
            return;
            
            //if (Password.Login(edLogin_UserName.Text.Trim(), edLogin_Password.Password.Trim()) == false)
            //{
            //    App.DS.CIMMessage("LogIn Message", "Login error");
            //    return;
            //}
            //else
            //{
            //    fmSecond.Visibility = Visibility.Visible;
            //    edLogin_Password.Password = "";
            //}
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Password.Logout();
            edLogin_UserName.Text = "";
            edLogin_Password.Password = "";
            App.DS.OP.UserName = Password.CurnUserName;
            App.DS.OP.Level = "";// Password.CurnLevel;
        }

    }
}
