using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;

namespace CIM
{
	public static class CommonMethods
	{
		/// <summary>
		/// 將 MemGroup 的 ItemValChangeEvent 參數由 ushort 轉為 string 型態。
		/// </summary>
		public static string UShortArrayToStr(ushort[] input_)
		{
			byte[] bytes = new byte[input_.Length * 2];
			int index = 0;

			foreach (ushort us in input_)
			{
				bytes[index++] = (byte)us;
				bytes[index++] = (byte)(us >> 8);
			}

			return Encoding.ASCII.GetString(bytes);
		}

		/// <summary>
		/// 列舉出傳入物件底下的所有指定型態的物件。
		/// </summary>
		public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					foreach (T childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}

		/// <summary>
		/// 顯示桌面。
		/// </summary>
		public static void ShowDesktop()
		{
			Type shellType = Type.GetTypeFromProgID("Shell.Application");
			object shellObject = System.Activator.CreateInstance(shellType);
			shellType.InvokeMember("ToggleDesktop", System.Reflection.BindingFlags.InvokeMethod, null, shellObject, null);
		}

		/// <summary>
		/// 取得螢幕縮放倍率。
		/// </summary>
		public static double GetScreenScalingRatio()
		{
			return SystemParameters.PrimaryScreenWidth / 1920.0;
		}

		public static IntPtr GetWindowHandle(Window window)
		{
			return new WindowInteropHelper(window).Handle;
		}

		[DllImport("User32.dll", EntryPoint = "PostMessage")]
		public static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		[DllImport("User32.dll", EntryPoint = "FindWindow")]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("User32.dll", EntryPoint = "FindWindowEx")]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

		[DllImport("shell32.dll")]
		private static extern IntPtr SHAppBarMessage(int msg, ref APPBARDATA data);
		private struct RECT
		{
			public int left, top, right, bottom;
		}
		private struct APPBARDATA
		{
			public int cbSize;
			public IntPtr hWnd;
			public int uCallbackMessage;
			public int uEdge;
			public RECT rc;
			public IntPtr lParam;
		}

		public static int GetWindowsTaskbarHeight()
		{
			const int ABM_GETTASKBARPOS = 5;
			APPBARDATA data = new APPBARDATA();
			data.cbSize = Marshal.SizeOf(data);
			SHAppBarMessage(ABM_GETTASKBARPOS, ref data);
			return data.rc.bottom - data.rc.top;
		}


		public static void MGLog(LogWriter File_, TagItem Item_)
		{
			try
			{
				StringBuilder sbStr = new StringBuilder();
				sbStr.AppendFormat("\t\t {0}.{1} Event", Item_.Mg.Name, Item_.Name);
				foreach (TagItem item in Item_.Mg.ItemList)
				{
					if (item.Name == "DataLog") continue;
					sbStr.AppendFormat("\r\n\t\t {0}={1}", item.Name, item.StringValue);
				}
				File_.AddString(sbStr.ToString());
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public static void ItemLog(LogWriter File_, TagItem Item_)
		{
			try
			{
				File_.AddString(string.Format("\t\t {0}-{1}:{2}={3}",Item_.Mg.Owner, Item_.Mg.Name, Item_.Name, Item_.StringValue));
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
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
				LogExcept.LogException(ex);
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
				LogExcept.LogException(ex);
			}
			return null;
		}
	}

	public static class Extensions
	{
		/// <summary>
		/// 傳回是否包含在所列舉的項目中。
		/// </summary>
		/// <param name="items">列舉項目</param>
		public static bool In<T>(this T item, params T[] items)
		{
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}

			return items.Contains(item);
		}

		/// <summary>
		/// 傳回一個深層複製。
		/// </summary>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="SerializationException"></exception>
		public static T DeepClone<T>(this T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			if (item.GetType().IsSerializable == false)
			{
				throw new SerializationException("類別未設定成可序列化");
			}

			using (Stream objectStream = new MemoryStream())
			{
				//序列化物件格式
				IFormatter formatter = new BinaryFormatter();
				//將自己所有資料序列化
				formatter.Serialize(objectStream, item);
				//複寫資料流位置，返回最前端
				objectStream.Seek(0, SeekOrigin.Begin);
				//再將 objectStream 反序列化回去 
				return (T)formatter.Deserialize(objectStream);
			}
		}

		/// <summary>
		/// 傳回新字串，其中已刪除在目前執行個體中指定位置的字元。
		/// </summary>
		/// <param name="position">要刪除字元之以零為起始的位置</param>
		public static string RemoveAt(this string item, int position)
		{
			return item.Remove(position, 1);
		}

		/// <summary>
		/// 擷取符合指定之述詞所定義的條件之所有項目。
		/// </summary>
		/// <param name="match">定義要搜尋項目之條件的Predicate<in T>委派</param>
		public static List<T> FindAll<T>(this ObservableCollection<T> collection, Predicate<T> match)
		{
			List<T> list = new List<T>();

			foreach (T obj in collection)
			{
				if (match(obj))
				{
					list.Add(obj);
				}
			}

			return list;
		}

		/// <summary>
		/// 將權限轉換成中文字串。
		/// </summary>
		public static string ToChineseString(this Password.Authorities authority)
		{
			switch (authority)
			{
				case Password.Authorities.CloseProgram:
					return "關閉軟體";
				case Password.Authorities.SettingUserGroup:
					return "進入 Setting - User Group 畫面";
				case Password.Authorities.SettingSystemPara:
					return "修改 Setting - System Para 設定";
				case Password.Authorities.SettingSystemAGV_HSMS:
					return "修改 Setting - AGV HSMS 設定";
				case Password.Authorities.SettingSystemMES_HSMS:
					return "修改 Setting - MES HSMS 設定";
				case Password.Authorities.MainAGVsRemote:
					return "切換 AGV Remote";
				case Password.Authorities.MainAGVsOffline:
					return "切換 AGV Offline";
				case Password.Authorities.MainTaskAbort:
					return "Abort 任務";
				case Password.Authorities.MainTaskDelete:
					return "Delete 任務";
				case Password.Authorities.OHBDetailDispatch:
					return "OHB 派工";
				case Password.Authorities.OHBDetailTakeOut:
					return "OHB 取貨";
				case Password.Authorities.OHBDetailCreateData:
					return "OHB 建帳";
				case Password.Authorities.OHBDetailDeleteData:
					return "OHB 刪帳";
				case Password.Authorities.SECSQuery:
					return "查詢 SECS 紀錄";
				case Password.Authorities.SECSQueryDownload:
					return "匯出 SECS 紀錄";
				case Password.Authorities.UserGroupCreateUser:
					return "新增使用者";
				case Password.Authorities.UserGroupDeleteUser:
					return "刪除使用者";
				case Password.Authorities.TaskManualOperation:
					return "進入 Task 手動操作畫面";
			}
			return string.Empty;
		}

		/// <summary>
		/// 將 byte 陣列轉換成 HEX 字串格式。
		/// </summary>
		public static string ToHex(this byte[] bytes, bool upperCase)
		{
			StringBuilder result = new StringBuilder(bytes.Length * 2);

			for (int i = 0; i < bytes.Length; i++)
			{
				result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
			}

			return result.ToString();
		}

		/// <summary>
		/// 轉換為 int 並傳回，若轉換失敗則傳回預設值。
		/// </summary>
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
	#region Converters
	public class BoolStr2BoolValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string str = ((string)value).Trim();
			return str == "0" ? false : true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool bl = (bool)value;
			return bl == true ? "1" : "0";
		}
	}

	public class BoolRadioConverter : IValueConverter
	{
		public bool Inverse { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool boolValue = (bool)value;

			return this.Inverse ? !boolValue : boolValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool boolValue = (bool)value;

			if (!boolValue)
			{
				//We only care when the user clicks a radio button to select it.
				return null;
			}

			return !this.Inverse;
		}
	}

	public class ScreenWidthToFormWidthConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string sParameter = parameter as string;

			double dScalingRatio = (int.Parse(value.ToString()) / 1920.0);

			if (sParameter == "Form")
			{
				return int.Parse(value.ToString()) - 280 * dScalingRatio;
			}
			else if (sParameter == "SubForm")
			{
				return int.Parse(value.ToString()) - 280 * dScalingRatio * 2;
			}
			else
			{
				return value;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class MainWindowHeightConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return int.Parse(value.ToString()) - CommonMethods.GetWindowsTaskbarHeight();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ScreenHeightToFormHeightConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			double dScalingRatio = SystemParameters.PrimaryScreenWidth / 1920.0;

			double dStatusBarHeight = 50 * dScalingRatio;

			return int.Parse(value.ToString()) - dStatusBarHeight - CommonMethods.GetWindowsTaskbarHeight();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ScalingRatioConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			double dSize = double.TryParse(parameter.ToString(), out dSize) ? dSize : 0.0;

			if (dSize == 0.0) return parameter;

			double dFinalSize = (int.Parse(value.ToString()) / 1920.0) * dSize;

			return dFinalSize;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class MultiMarginConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return new Thickness(System.Convert.ToDouble(values[0]),
								 System.Convert.ToDouble(values[1]),
								 System.Convert.ToDouble(values[2]),
								 System.Convert.ToDouble(values[3]));
		}

		public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
	#endregion Converters

}
