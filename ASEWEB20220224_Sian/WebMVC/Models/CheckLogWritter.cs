using System;
using System.Timers;
using System.Data;
namespace ASEWEB.Models
{
    public class CheckLogWritter
    {
        public static string TimeSet;
        public static string TimeTypeSet;
        public static System.Timers.Timer timer = new System.Timers.Timer();
        public static int t = 60000;
        public static void  TimerSet(string TimeType,string TSet)
        {
            timer.Enabled = true;
            if (TimeType == "min") { timer.Interval = t; TimeTypeSet = "min"; }//执行间隔时间,单位为毫秒;此时时间间隔为1分钟
            if (TimeType == "week") {timer.Interval = t * 60 * 24; TimeTypeSet = "week"; }
            if (TimeType == "day") { timer.Interval = t * 60; TimeTypeSet = "day"; }
            TimeSet = TSet;
            timer.Start();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerOn);
        }
        private static void TimerOn(object source, ElapsedEventArgs e)
        {
            if (TimeTypeSet == "min")
            {
                string[] T1 = TimeSet.Split(":");
                if (DateTime.Now.Hour == Int32.Parse(T1[0]) && DateTime.Now.Minute == Int32.Parse(T1[1])) //如果当前时间是X点O分
                {

                }
            }
            if (TimeTypeSet == "day")
            {
                if (DateTime.Now.Day == Int32.Parse(TimeSet))
                {

                }
            }
            if (TimeTypeSet == "week")
            {
                if ((int)DateTime.Now.DayOfWeek == Int32.Parse(TimeSet)) //DayofWeek 0=星期日 6=星期六
                {

                }
            }



        }
    }
}
