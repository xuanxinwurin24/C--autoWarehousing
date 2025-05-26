using Strong;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Lib.Model
{
    static class LogExcept
    {
        static LogWriter logWriter = new LogWriter(Environment.CurrentDirectory + @"\LogFile\Exception", "Exception", 50000);

        public static void LogException(Exception e_)
        {
            logWriter.AddString("-----Exception-----\r\n" + e_.Message + "\r\n" + e_.StackTrace);
        }
    }
}
