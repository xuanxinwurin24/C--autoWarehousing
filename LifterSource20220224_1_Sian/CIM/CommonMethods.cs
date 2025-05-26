using CIM.Lib.Model;
using CIM.UILog;
using Strong;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CIM
{
    public static class CommonMethods
    {
        /// <summary>將 MemGroup 的 ItemValChangeEvent 參數由 ushort 轉為 string 型態。</summary>
        public static string UShortArrayToStr(ushort[] input_)
        {
            byte[] bytes = new byte[input_.Length * 2];
            Buffer.BlockCopy(input_, 0, bytes, 0, input_.Length * 2);

            //int index = 0;
            //foreach (ushort us in input_)
            //{
            //	bytes[index++] = (byte)us;
            //	bytes[index++] = (byte)(us >> 8);
            //}
            return Encoding.ASCII.GetString(bytes);
        }
        public static ushort[] StrToUShortArray(string input_, uint arraylength_)
        {
            string sVal = string.Empty;
            if (input_.Length > arraylength_ * 2)
                sVal = input_.Substring(0, (int)arraylength_ * 2);
            else
                sVal = input_.PadRight((int)arraylength_ * 2);
            byte[] bVal = Encoding.Default.GetBytes(sVal);
            ushort[] uVal = new ushort[arraylength_];
            Buffer.BlockCopy(bVal, 0, uVal, 0, bVal.Length);
            return uVal;
        }


        public static void MGLog(frmEqLog frmlog_, TagItem Item_)
        {
            try
            {
                StringBuilder sbStr = new StringBuilder();
                sbStr.AppendFormat("{0}.{1} Event", Item_.Mg.Name, Item_.Name);
                foreach (TagItem item in Item_.Mg.ItemList)
                {
                    if (item.Name == "DataLog") continue;
                    sbStr.AppendFormat("\r\n{0}={1}", item.Name, item.StringValue);
                }
                frmlog_.Log(sbStr.ToString());
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }

        public static void MGLog(LogWriter File_, TagItem Item_)
        {
            try
            {
                StringBuilder sbStr = new StringBuilder();
                sbStr.AppendFormat("{0}.{1} Event", Item_.Mg.Name, Item_.Name);
                foreach (TagItem item in Item_.Mg.ItemList)
                {
                    if (item.Name == "DataLog") continue;
                    sbStr.AppendFormat("\r\n{0}={1}", item.Name, item.StringValue);
                }
                File_.AddString(sbStr.ToString());
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }

        public static void Alarm_MGLog(frmEqLog frmlog_, TagItem Item_)
        {
            try
            {
                StringBuilder sbStr = new StringBuilder();
                sbStr.AppendFormat("{0}.{1} Event", Item_.Mg.Name, Item_.Name);
                foreach (TagItem item in Item_.Mg.ItemList)
                {
                    string[] str = { "Alarm1", "Alarm2", "Alarm3", "Alarm4", "Alarm5", "Alarm6", "Alarm7", "Alarm8", "Alarm9", "Alarm10", "Alarm11", "Index" };
                    if (!item.Name.In(str)) continue;
                    sbStr.AppendFormat("\r\n{0}={1}", item.Name, item.StringValue);
                }
                frmlog_.Log(sbStr.ToString());
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }

        public static void Alarm_MGLog(LogWriter File_, TagItem Item_)
        {
            try
            {
                StringBuilder sbStr = new StringBuilder();
                sbStr.AppendFormat("{0}.{1} Event", Item_.Mg.Name, Item_.Name);
                foreach (TagItem item in Item_.Mg.ItemList)
                {
                    string[] str = { "Alarm1", "Alarm2", "Alarm3", "Alarm4", "Alarm5", "Alarm6", "Alarm7", "Alarm8", "Alarm9", "Alarm10", "Alarm11", "Index" };
                    if (!item.Name.In(str)) continue;
                    sbStr.AppendFormat("\r\n{0}={1}", item.Name, item.StringValue);
                }
                File_.AddString(sbStr.ToString());
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }

        public static void ItemLog(frmEqLog frmlog_, TagItem Item_)
        {
            try
            {
                frmlog_.Log(string.Format("{0}-{1}:{2}={3}", Item_.Mg.Owner, Item_.Mg.Name, Item_.Name, Item_.StringValue));
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }

        public static void ItemLog(LogWriter File_, TagItem Item_)
        {
            try
            {
                File_.AddString(string.Format("{0}-{1}:{2}={3}", Item_.Mg.Owner, Item_.Mg.Name, Item_.Name, Item_.StringValue));
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }

        public static string HexStrReverse(string str_)
        {
            string sDestStr = "";
            string[] str = str_.Split(new Char[] { '-' });
            for (int i = str.Length - 1; i >= 0; i--)
            {
                sDestStr += (str[i] + " ");
            }
            return sDestStr.Trim();
        }

        /// <summary>將 A、B 物件交換。</summary>
        public static void Swap<T>(ref T objectA_, ref T objectB_)
        {
            T temp;
            temp = objectA_;
            objectA_ = objectB_;
            objectB_ = temp;
        }

        public static int String2_ToBCD(string str_)
        {
            try
            {
                int a = str_[0] - '0';
                int b = str_[1] - '0';
                int c = (a << 4) | b;
                return c;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
            return 0;
        }
        public static Stream LoadStreamResouce(string file_)           //new unit that included MemGroup
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(file_);
                return stream;
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
            return null;
        }
    }

    public static class Extensions
    {
        /// <summary>傳回是否包含在所列舉的項目中。</summary>
        /// <param name="items">列舉項目</param>
        public static bool In<T>(this T item, params T[] items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            return items.Contains(item);
        }
        public static int ToIntDef(this string str, int defaultValue = 0)
        {
            try
            {
                return int.Parse(str);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
    /// <summary>
    /// 轉換為 int 並傳回，若轉換失敗則傳回預設值。
    /// </summary>

}
