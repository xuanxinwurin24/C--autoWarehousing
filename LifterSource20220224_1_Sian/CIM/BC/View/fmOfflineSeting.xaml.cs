using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CIM.BC;
using Strong;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;

namespace CIM
{
    /// <summary>
    /// Interaction logic for fmOfflineSeting.xaml
    /// </summary>
    public partial class fmOfflineSeting : Window
    {
        SortedList<string, int> kvpDate = new SortedList<string, int> { };

        List<string> Date = new List<string> { "今天", "明天", "後天", "無" };
        List<string> Hour;
        List<string> Min = new List<string>();
        ObservableCollection<Offline> OfflineList;
        string sSetStartDate, sSetEndDate;

        public fmOfflineSeting()
        {
            InitializeComponent();
            MMIItemInit();
            RenewOfflineList();
            lvOffline.ItemsSource = OfflineList;
        }

        public void MMIItemInit()
        {
            Hour = new List<string>();
            Min = new List<string>();
            kvpDate = new SortedList<string, int>();
            for (uint h = 0; h < 24; h++)
            {
                Hour.Add(h.ToString());
            }
            for (int m = 0; m < 6; m++)
            {
                string min = (m * 10).ToString().PadLeft(2, '0');
                Min.Add(min);
            }
            int dcnt = 0;
            foreach (string sDate in Date)
            {
                kvpDate.Add(sDate, dcnt);
                ++dcnt;
            }
            cbStartDate.ItemsSource = Date;
            cbStartHour.ItemsSource = Hour;
            cbStartMin.ItemsSource = Min;
            cbEndDate.ItemsSource = Date;
            cbEndHour.ItemsSource = Hour;
            cbEndMin.ItemsSource = Min;
            cbStartDate.SelectedIndex = 0;
            cbEndDate.SelectedIndex = 3;
            cbStartHour.SelectedIndex = DateTime.Now.Hour + 1;
            cbStartMin.SelectedIndex = 0;
            cbEndHour.SelectedIndex = 0;
            cbEndMin.SelectedIndex = 0;
            cbStartDate.IsEnabled = App.DS.OP.bOfflineset;
            cbStartHour.IsEnabled = App.DS.OP.bOfflineset;
            cbStartMin.IsEnabled = App.DS.OP.bOfflineset;
            cbEndDate.IsEnabled = App.DS.OP.bOfflineset;
            cbEndHour.IsEnabled = App.DS.OP.bOfflineset;
            cbEndMin.IsEnabled = App.DS.OP.bOfflineset;
            btnSave.IsEnabled = App.DS.OP.bOfflineset;
            btnTurnOnline.IsEnabled = App.DS.OP.bOfflineset;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int iStartDate, iEndDate, iStartHour = 0, iEndHour = 0, iStartMin = 0, iEndMin = 0;
            bool bResult = true;
            kvpDate.TryGetValue(cbStartDate.SelectedItem.ToString(), out iStartDate);
            kvpDate.TryGetValue(cbEndDate.SelectedItem.ToString(), out iEndDate);
            int.TryParse(cbStartHour.SelectedItem.ToString(), out iStartHour);
            int.TryParse(cbEndHour.SelectedItem.ToString(), out iEndHour);
            int.TryParse(cbStartMin.SelectedItem.ToString(), out iStartMin);
            int.TryParse(cbEndMin.SelectedItem.ToString(), out iEndMin);

            if (iStartDate > iEndDate || iStartDate == 3)
            {
                bResult = false;
            }
            else if (iStartDate == iEndDate)
            {
                if (iStartHour > iEndHour)
                {
                    bResult = false;
                }
                else if (iStartHour == iEndHour)
                {
                    if (iStartMin >= iEndMin)
                    {
                        bResult = false;
                    }
                }
            }

            if (!bResult)
            {
                App.DS.CIMMessage("Set Offline Message", "時間設置錯誤");
            }
            else
            {
                sSetStartDate = TimeFormate(iStartDate, iStartHour, iStartMin);
                sSetEndDate = TimeFormate(iEndDate, iEndHour, iEndMin);
                string sql = $"INSERT INTO WEBSERVICE_CONNECTION(NAME,STATUS,START_TIME,END_TIME)" +
                    $"VALUES('SET', 'OFFLINE','{sSetStartDate}','{sSetEndDate}')";
                int line = App.Local_SQLServer.NonQuery(sql);
                RenewOfflineList();
                lvOffline.ItemsSource = OfflineList;
            }
        }

        private string TimeFormate(int iDate_, int iHour_, int iMin_)
        {
            string sTime = string.Empty;

            switch (iDate_)
            {
                case 0:
                    sTime = DateTime.Now.ToString("yyyyMMdd") + iHour_.ToString().PadLeft(2, '0') + iMin_.ToString().PadLeft(2, '0');
                    break;
                case 1:
                    sTime = DateTime.Today.AddDays(1).ToString("yyyyMMdd") + iHour_.ToString().PadLeft(2, '0') + iMin_.ToString().PadLeft(2, '0');
                    break;
                case 2:
                    sTime = DateTime.Today.AddDays(2).ToString("yyyyMMdd") + iHour_.ToString().PadLeft(2, '0') + iMin_.ToString().PadLeft(2, '0');
                    break;
                case 3:
                    sTime = string.Empty;
                    break;
            }
            return sTime;
        }

        private void Delet_Click(object sender, RoutedEventArgs e)
        {
            var body = ((sender as Button).Tag as ListViewItem).DataContext as Offline;
            string sql = string.Empty;
            string sStartTime = TextCangeFormat(body.StartTime.Trim(), false);
            string sEndTime = TextCangeFormat(body.EndTime.Trim(), false);

            if (body.EndTime == "")
            {
                sql = $"DELETE FROM WEBSERVICE_CONNECTION WHERE START_TIME LIKE '%{sStartTime}%' AND END_TIME = ''";
            }
            else
            {
                sql = $"DELETE FROM WEBSERVICE_CONNECTION WHERE START_TIME LIKE '%{sStartTime}%' AND END_TIME LIKE '%{sEndTime}%'";
            }
            int k = App.Local_SQLServer.NonQuery(sql);
            OfflineList.Remove(body);
        }

        private void RenewOfflineList()
        {
            bool bDeletEnable = true;
            if (!(App.DS.OP.Level.Contains("Offlineset")||App.DS.OP.Level.Contains("Admin")))//App.DS.OP.Level < App.SysPara.OfflineSetLevel
            {
                bDeletEnable = false;
            }
            string sql = string.Empty;
            try
            {
                DataTable Result = new DataTable();
                sql = $"SELECT* FROM WEBSERVICE_CONNECTION WHERE NAME LIKE '%SET%'";
                App.Local_SQLServer.Query(sql, ref Result, true);
                string sStartTime, sEndTime;
                OfflineList = new ObservableCollection<Offline>();
                int uNo = 0;
                int Len = Result.Rows.Count;
                foreach (DataRow dr in Result.Rows)
                {
                    sStartTime = dr["START_TIME"].ToString().Trim();
                    sEndTime = dr["END_TIME"].ToString().Trim();
                    ++uNo;
                    Offline offline = new Offline();
                    offline.No = uNo;
                    offline.StartTime = TextCangeFormat(sStartTime);
                    offline.EndTime = TextCangeFormat(sEndTime);
                    offline.bDeletBtn = bDeletEnable;

                    OfflineList.Add(offline);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }

        private void btnTurnOnline_Click(object sender, RoutedEventArgs e)
        {
            if (!App.DS.OP.bOfflineset)//App.DS.OP.Level < App.SysPara.SoteriaLevel
            {
                App.DS.CIMMessage("CIM Message", "權限不足");
            }
            else
            {
                string sql = string.Empty;
                string sql_Real = string.Empty;
                try
                {
                    DataTable Result = new DataTable();
                    DataTable Result_Real = new DataTable();

                    sql = $"SELECT * FROM WEBSERVICE_CONNECTION WHERE NAME LIKE '%SET%'";
                    sql_Real = $"SELECT * FROM WEBSERVICE_CONNECTION WHERE NAME LIKE '%REAL%'";
                    App.Local_SQLServer.Query(sql_Real, ref Result_Real, true);
                    string sStatus = string.Empty;
                    foreach (DataRow dr in Result_Real.Rows)
                    {
                        sStatus = dr["STATUS"].ToString().Trim();

                        if (sStatus.Trim() == "ONLINE")
                        {
                            App.DS.bLifterIsOnline = true;
                            App.bRemindOffline = false;
                        }
                        else
                        {
                            App.DS.bLifterIsOnline = false;
                        }
                    }

                    App.Local_SQLServer.Query(sql, ref Result, true);
                    string sStartTime;
                    OfflineList = new ObservableCollection<Offline>();
                    foreach (DataRow dr in Result.Rows)
                    {
                        sStartTime = dr["START_TIME"].ToString().Trim();
                        DateTime dtStart = DateTime.ParseExact(sStartTime, "yyyyMMddHHmm", CultureInfo.InvariantCulture);
                        TimeSpan tspan = dtStart - DateTime.Now;
                        if (tspan.TotalMinutes < 0)
                        {
                            sql = $"DELETE FROM WEBSERVICE_CONNECTION WHERE START_TIME LIKE '%{sStartTime}%'";
                            App.Local_SQLServer.NonQuery(sql);
                            RenewOfflineList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex);
                }
            }
        }

        private string TextCangeFormat(string sTime_, bool bToListView = true)
        {
            string sTime = string.Empty;
            if (sTime_ == "")
            {
                return sTime = "";
            }
            if (bToListView)
            {
                string year = sTime_.Substring(0, 4);
                string month = sTime_.Substring(4, 2);
                string date = sTime_.Substring(6, 2);
                string hour = sTime_.Substring(8, 2);
                string min = sTime_.Substring(10, 2);

                sTime = string.Format("{0}/{1}/{2} {3}:{4}", year, month, date, hour, min);
            }
            else
            {
                sTime = sTime_.Trim();
                sTime = sTime.Replace("/", "");
                sTime = sTime.Replace(" ", "");
                sTime = sTime.Replace(":", "");
            }
            return sTime;
        }
    }
}
