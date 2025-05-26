using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

namespace CIM.Lib.View
{
    /// <summary>
    /// Interaction logic for ucChart.xaml
    /// </summary>
    public partial class ucStackedColumn : UserControl
    {
        System.Drawing.Color[] Colors = new System.Drawing.Color[] {
                //System.Drawing.Color.SeaGreen,
                System.Drawing.Color.FromArgb(240,255,0,64),
                System.Drawing.Color.FromArgb(240,0,128,255),
                System.Drawing.Color.Red,
                System.Drawing.Color.Blue,
                System.Drawing.Color.Yellow,
                System.Drawing.Color.Lime,
                System.Drawing.Color.Purple,
                System.Drawing.Color.Magenta,
                System.Drawing.Color.Orange,
                System.Drawing.Color.AliceBlue,
                System.Drawing.Color.BlanchedAlmond,
                System.Drawing.Color.DarkSlateGray,
                System.Drawing.Color.Magenta
        };
        DataTable dt;
        public ucStackedColumn()
        {
            InitializeComponent();
        }
        public void Draw(DataTable _dt_draw)
        {
            dt = _dt_draw;
            //SetDataTable(_DrawData1, _DrawData2);
            SetChart(dt);

            this.chart1.DataSource = dt;
            //this.chart1.DataBind();//這時候先DataBind()是為了顯示空白的圖表
        }
        private void SetDataTable(int [] _DrawData1, int[] _DrawData2)
        {
            dt = new DataTable();

            string[] names = new string[] { "8", "12" , "Total" };
            dt.Columns.Add(names[0]);
            dt.Columns.Add(names[1]);
            dt.Columns.Add(names[2]);

            for(int i=0; i < _DrawData1.Length; i++)
            {
                 DataRow dr = dt.NewRow();

                 dr[names[0]] = _DrawData1[i];
                 dr[names[1]] = _DrawData2[i];
                 dr[names[2]] = _DrawData1[i] + _DrawData2[i];
                 dt.Rows.Add(dr);
            }

            /*
            Random random = new Random();
            for (int i = 0; i < 12; i++)
            {
                DataRow dr = dt.NewRow();
                int v1= random.Next(1, 1000);
                int v2 = random.Next(1, 1000);
                dr[names[0]] = v1;// random.Next(1, 1000);
                dr[names[1]] = v2;// random.Next(1, 1000);
                dr[names[2]] = v1 + v2;// (int)dr[names[0]] + (int)dr[names[1]];
                dt.Rows.Add(dr);
            }
            */
        }
        private void SetChart(DataTable dt_)
        {
            this.chart1.ChartAreas.Clear();
            ChartArea Area = new ChartArea("ChartArea");
            this.chart1.ChartAreas.Add(Area);
            Area.AxisX.Interval = 1;//V
            Area.AxisX.IntervalType = DateTimeIntervalType.Days;
            Area.AxisX.LabelStyle.Format = "MM-dd";
            Area.AxisY.Interval = 20;
            //Area.Area3DStyle.Enable3D = true;
            //Area.Area3DStyle.PointDepth = 40;
            //Area.Area3DStyle.WallWidth = 0;
            //Area.Area3DStyle.LightStyle = LightStyle.Realistic;
            //Area.Area3DStyle.IsClustered = true;
            Area.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            Area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.NotSet;
            //Area.AxisX.MajorGrid.LineColor = System.Drawing.Color.FromArgb(150, 150, 150);
            Area.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(150, 150, 150);

            chart1.Legends.Clear();
            Legend lg = new Legend("aaa");
            lg.IsTextAutoFit = true;
            lg.Docking = Docking.Right;
            lg.Alignment = System.Drawing.StringAlignment.Center;
            lg.Font = new Font("Consolas", 20);
            chart1.Legends.Add(lg);

            this.chart1.Series.Clear();

            /*
            int i = 0;
            foreach (var col in dt_.Columns)
            {
                string barName = col.ToString();

                if (barName == "Date") continue;

                Series se = new Series(barName);

                se.Color = Colors[i++];
                se.ChartArea = "ChartArea";
                if (barName == "Total")
                {
                    se.ChartType = SeriesChartType.Point;
                    se.MarkerStyle = MarkerStyle.Diamond;
                    se.LabelBackColor = System.Drawing.Color.FromArgb(150, 255, 255, 255);
                    se.LabelForeColor = System.Drawing.Color.Black;
                }
                else
                {
                    se.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedColumn;
                    se.BorderWidth = 100;
                    se.LabelBackColor = System.Drawing.Color.FromArgb(150, 255, 255, 255);
                    se.LabelForeColor = Colors[i + 1];
                }
                se.YValueMembers = barName;
                se.IsValueShownAsLabel = true;

                se.Font = new Font("Consolas", 16);
                this.chart1.Series.Add(se);
            }
            */

            int icol_index = 0;
            int idt_date_index = dt_.Columns.Count - 1;

            foreach (var col in dt_.Columns)
            {
                string barName = col.ToString();
                Series se = new Series();

                if(barName != "Date")
                { 
                    foreach (DataRow row in dt_.Rows)
                    {
                        se.Points.AddXY(row[idt_date_index], row[icol_index]);
                    }

                    se.Color = Colors[icol_index % Colors.Length];
                    se.ChartArea = "ChartArea";
                    if (barName == "Total")
                    {
                        se.ChartType = SeriesChartType.Point;
                        se.MarkerStyle = MarkerStyle.Diamond;
                        se.MarkerColor = System.Drawing.Color.Black;
                        se.LabelBackColor = System.Drawing.Color.FromArgb(150, 255, 255, 255);
                        se.LabelForeColor = System.Drawing.Color.Black;
                    }
                    else
                    {
                        se.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.StackedColumn;
                        se.BorderWidth = 100;
                        se.LabelBackColor = System.Drawing.Color.FromArgb(150, 255, 255, 255);
                        se.LabelForeColor = Colors[icol_index % Colors.Length];
                    }

                    se.Name = barName;
                    se.IsValueShownAsLabel = true;

                    se.Font = new Font("Consolas", 16);
                    this.chart1.Series.Add(se);
                }

                icol_index++;
            }

            //this.chart1.Series.Add("Total");
            //this.chart1.Series["Total"].ChartType = SeriesChartType.Point;
            //this.chart1.Series["Total"].MarkerSize = 15;//  'change this to 0 if you don't want a marker at the top of the col.
            //this.chart1.Series["Total"].MarkerStyle = MarkerStyle.Diamond;
            //this.chart1.Series["Total"].IsValueShownAsLabel = true;
            //this.chart1.Series["Total"].Points.AddY(2000);


            //int iTotal = 0;
            //foreach(var val in chart1.Series)

            //For k As Integer = 0 To 1 'if there are 2 columns to add
            //    Dim total As Double = 0
            //    For j As Integer = 0 To 1
            //        total += Chart1.Series(j).Points(k).YValues(0)
            //    Next
            //    Chart1.Series("Total").Points.AddY(total)
            //Next
        }
    }
}
