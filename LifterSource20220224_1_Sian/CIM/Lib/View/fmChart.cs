using CIM.Lib.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CIM.Lib.View
{
    public partial class fmChart: Form
    {
        public fmChart()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            DateTime StartTime = StartTimePicker.Value;
            DateTime EndTime = EndTimePicker.Value;
            //Check Time End > Start
            if (EndTime < StartTime)
            {
                MessageBox.Show("End Time < Start Time");
            }

            //Check Search Path Start Time ~ End Time 
            List<string> SearchFileList = new List<string>();
            List<string> SearchRecordList = new List<string>();
            while (StartTime <= EndTime)
            {
                SearchFileList.Add(App.sSysDir + @"\LogFile\RecordData\\" + StartTime.Year.ToString("0000") + "_RecordData\\" + StartTime.Month.ToString("00") + "_RecordData\\" + StartTime.Day.ToString("00") + "_RecordData\\");
                StartTime = StartTime.AddDays(1);
            }
            StartTime = StartTimePicker.Value;

            foreach (string sSearchFilePath in SearchFileList)
            {
                try
                {
                    System.IO.Directory.GetFiles(sSearchFilePath);
                }
                catch (Exception e_) { continue;  }
                foreach(string sFileName in System.IO.Directory.GetFiles(sSearchFilePath))
                {
                    System.IO.StreamReader file = new System.IO.StreamReader(sFileName);
                    string sTemp;
                    while ( (sTemp = file.ReadLine()) != null)
                    {
                        DateTime RecordTime;
                        if (!DateTime.TryParseExact(sTemp.Substring(0,17),"yyyyMMdd-HH:mm:ss",null,System.Globalization.DateTimeStyles.None, out RecordTime))
                        {
                            continue;
                        }
                        if (RecordTime < StartTime || RecordTime > EndTime) continue;
                        SearchRecordList.Add(sTemp);
                    }
                }
            }

            //Use SearchRecordList Draw Chart
            RecordChart.Series[0].Points.Clear();
            RecordChart.Series[1].Points.Clear();
            RecordChart.Series[2].Points.Clear();
            RecordChart.Series[3].Points.Clear();
            RecordChart.Series[4].Points.Clear();
            RecordChart.Series[5].Points.Clear();
            RecordChart.Series[6].Points.Clear();
            RecordChart.Series[7].Points.Clear();
            RecordChart.Series[8].Points.Clear();
            RecordChart.Series[9].Points.Clear();
            RecordChart.Series[10].Points.Clear();
            RecordChart.Series[11].Points.Clear();
            RecordChart.Series[12].Points.Clear();
            RecordChart.Series[13].Points.Clear();
            RecordChart.Series[14].Points.Clear();
            RecordChart.Series[15].Points.Clear();
            RecordChart.Series[16].Points.Clear();
            RecordChart.Series[17].Points.Clear();
            foreach (string sRecordData in SearchRecordList)
            {
                string[] sArray = sRecordData.Split(' ');
                string[] sData = sArray[2].Split(',');
                string sTime = sArray[0].Substring(0, 17);
                RecordChart.Series[0].Points.AddXY(sTime, sData[0]);
                RecordChart.Series[1].Points.AddXY(sTime, sData[1]);
                RecordChart.Series[2].Points.AddXY(sTime, sData[2]);
                RecordChart.Series[3].Points.AddXY(sTime, sData[3]);
                RecordChart.Series[4].Points.AddXY(sTime, sData[4]);
                RecordChart.Series[5].Points.AddXY(sTime, sData[5]);
                RecordChart.Series[6].Points.AddXY(sTime, sData[6]);
                RecordChart.Series[7].Points.AddXY(sTime, sData[7]);
                RecordChart.Series[8].Points.AddXY(sTime, sData[8]);
                RecordChart.Series[9].Points.AddXY(sTime, sData[9]);
                RecordChart.Series[10].Points.AddXY(sTime, sData[10]);
                RecordChart.Series[11].Points.AddXY(sTime, sData[11]);
                RecordChart.Series[12].Points.AddXY(sTime, sData[12]);
                RecordChart.Series[13].Points.AddXY(sTime, sData[13]);
                RecordChart.Series[14].Points.AddXY(sTime, sData[14]);
                RecordChart.Series[15].Points.AddXY(sTime, sData[15]);
                RecordChart.Series[16].Points.AddXY(sTime, sData[16]);
                RecordChart.Series[17].Points.AddXY(sTime, sData[17]);
            }
            RecordChart.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
            RecordChart.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            RecordChart.ChartAreas[0].AxisX.ScaleView.Size = 5;
            RecordChart.ChartAreas[0].AxisX.Interval = 1;

            RecordChart.Series[0].Enabled = checkBox1.Checked;
            RecordChart.Series[1].Enabled = checkBox2.Checked;
            RecordChart.Series[2].Enabled = checkBox3.Checked;
            RecordChart.Series[3].Enabled = checkBox4.Checked;
            RecordChart.Series[4].Enabled = checkBox5.Checked;
            RecordChart.Series[5].Enabled = checkBox6.Checked;
            RecordChart.Series[6].Enabled = checkBox7.Checked;
            RecordChart.Series[7].Enabled = checkBox8.Checked;
            RecordChart.Series[8].Enabled = checkBox9.Checked;
            RecordChart.Series[9].Enabled = checkBox10.Checked;
            RecordChart.Series[10].Enabled = checkBox11.Checked;
            RecordChart.Series[11].Enabled = checkBox12.Checked;
            RecordChart.Series[12].Enabled = checkBox13.Checked;
            RecordChart.Series[13].Enabled = checkBox14.Checked;
            RecordChart.Series[14].Enabled = checkBox15.Checked;
            RecordChart.Series[15].Enabled = checkBox16.Checked;
            RecordChart.Series[16].Enabled = checkBox17.Checked;
            RecordChart.Series[17].Enabled = checkBox18.Checked;

        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            int iSeriesIndex = int.Parse(cb.Tag.ToString()) - 1;
            RecordChart.Series[iSeriesIndex].Enabled = cb.Checked;
        }
    }
}
