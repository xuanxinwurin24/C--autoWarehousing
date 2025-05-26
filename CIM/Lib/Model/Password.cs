using Strong;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Newtonsoft.Json;
using static CIM.CommonMethods;

namespace CIM.Lib.Model
{
	public class Password
	{
		public delegate void LogInOutEventHandle(string sOldUserName_, string sNewUserName_);
		public static event LogInOutEventHandle LogInOutEvent;

		public static string FileName { get; set; } = $@"{App.sSysDir}\Ini\UserGroup.json";

		public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

		public enum Authorities : uint
		{
			CloseProgram = 0b10000000000000000000000000000000,
			SettingUserGroup = 0b01000000000000000000000000000000,
			SettingSystemPara = 0b00100000000000000000000000000000,
			SettingSystemAGV_HSMS = 0b00010000000000000000000000000000,
			SettingSystemMES_HSMS = 0b00001000000000000000000000000000,
			MainAGVsRemote = 0b00000100000000000000000000000000,
			MainAGVsOffline = 0b00000010000000000000000000000000,
			MainTaskAbort = 0b00000001000000000000000000000000,
			MainTaskDelete = 0b00000000100000000000000000000000,
			OHBDetailAvailable = 0b00000000010000000000000000000000,
			OHBDetailDispatch = 0b00000000001000000000000000000000,
			OHBDetailTakeOut = 0b00000000000100000000000000000000,
			OHBDetailCreateData = 0b00000000000010000000000000000000,
			OHBDetailDeleteData = 0b00000000000001000000000000000000,
			SECSQuery = 0b00000000000000100000000000000000,
			SECSQueryDownload = 0b00000000000000010000000000000000,
			UserGroupCreateUser = 0b00000000000000001000000000000000,
			UserGroupDeleteUser = 0b00000000000000000100000000000000,
			TaskHistoryQuery = 0b00000000000000000010000000000000,
			TaskHistoryDownload = 0b00000000000000000001000000000000,
			TaskManualOperation = 0b00000000000000000000000000000001
		};

		protected static void OnStaticPropertyChanged(PropertyChangedEventArgs e)
		{
			StaticPropertyChanged?.Invoke(null, e);

			//User curnUser = new Password().GetType().GetProperty(e.PropertyName).GetValue(null, null) as User;

			MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
			mainWindow?.CIMStatusBar.txbUserName.GetBindingExpression(System.Windows.Controls.TextBlock.TextProperty).UpdateTarget();
			mainWindow?.CIMStatusBar.txbUserGroup.GetBindingExpression(System.Windows.Controls.TextBlock.TextProperty).UpdateTarget();
		}

		public static ObservableCollection<UserGroup> UserGroups { get; set; }

		static Password()
		{
			StaticPropertyChanged += (sender, e) => { return; };

			UserGroups = new ObservableCollection<UserGroup>
			{
				new UserGroup() { GroupName = "Administrator", Level = 9 },
				new UserGroup() { GroupName = "Engineer", Level = 6 },
				new UserGroup() { GroupName = "Operator", Level = 3 }
			};
		}

		const string _Admin_UserName = "Strong";
		const string _Admin_Password = "5999011";

		static User _curnUser;
		public static User CurnUser
		{
			get { return _curnUser; }
			set
			{
				if (_curnUser == value) return;
				string old = _curnUser.UserName;
				_curnUser = value;
				LogInOutEvent?.Invoke(old, value.UserName);
				OnStaticPropertyChanged(new PropertyChangedEventArgs("CurnUser"));
			}
		}

		public static List<string> UserNames
		{
			get
			{
				List<string> userList = new List<string>();
				foreach (UserGroup group in UserGroups.FindAll(x => x.GroupName.ToUpper() != "GUEST"))  //UserGroups.Where(x => x.GroupName.ToUpper() != "GUEST"))
				{
					foreach (KeyValuePair<string, User> pair in group.UserList)
					{
						userList.Add(pair.Value.UserName);
					}
				}
				return userList;
			}
		}

		public static bool Initial()
		{
			FileInfo file = new FileInfo(FileName);
			try
			{
				if (file.Directory.Exists == false)
				{
					try
					{
						file.Directory.Create();
					}
					catch (Exception ex)
					{
						MessageBox.Show("無法建立路徑: " + file.Directory.FullName);
						LogExcept.LogException(ex);
						return false;
					}
				}

				if (File.Exists(FileName) == false)
				{
					if (NewUser(_Admin_UserName, _Admin_Password, UserGroups.FirstOrDefault(x => x.GroupName == "Administrator")) == false)
					{
						return false;
					}

					if (NewUser("Guest", "", UserGroups.FirstOrDefault(x => x.GroupName == "Operator")) == false)
					{
						return false;
					}

					SaveToFile();
				}
				else
				{
					LoadFromFile();
					LoadAuthorities();
				}

				_curnUser = GetUser("Guest");

				return true;
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
			return false;
		}

		public static bool SaveToFile()
		{
			try
			{
				string json = JSON.Serialize(UserGroups, true);

				if (json.Length > 0)
				{
					using (StreamWriter sw = new StreamWriter(FileName))
					{
						sw.Write(json);
					}

					//每次存檔寫入到資料庫做備份
					//App.Host_SQL_Server.NonQuery($"update LastData set BYTES = (CONVERT(VARBINARY(MAX), '0x{File.ReadAllBytes(FileName).ToHex(true)}', 1)) where FILENAME = 'UserGroup.json'");
					//App.Host_SQL_Server.NonQuery($"update LastData set LAST_MODIFIED_DATE = '{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff}' where FILENAME = 'UserGroup.json'");
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
			return false;
		}

		public static void LoadFromFile()
		{
			try
			{
				using (StreamReader sr = new StreamReader(FileName))
				{
					string str = sr.ReadToEnd();
					UserGroups = JSON.DeSerialize<ObservableCollection<UserGroup>>(str);
				}

				foreach (UserGroup group in UserGroups)
				{
					foreach (KeyValuePair<string, User> pair in group.UserList)
					{
						pair.Value.Group = group;   //因為反序列化不會將會造成迴圈的參數還原回來，所以要自己補上
					}
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public static void LoadAuthorities()
		{
			try
			{
				foreach (UserGroup group in UserGroups)
				{
					try
					{
						uint authorityBits = Convert.ToUInt32(group.AccessibleElement, 16);
						group.AccessibleElementBits = authorityBits;
					}
					catch
					{
						group.AccessibleElementBits = 0;
					}
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public static bool NewUser(string username, string password, UserGroup userGroup)
		{
			try
			{
				if (userGroup == null)
				{
					if (Properties.Settings.Default.DefaultCulture.Name == "zh-TW")
					{
						MessageBox.Show(Application.Current.MainWindow, $"找不到使用者群組 '{userGroup.GroupName}'");
					}
					else
					{
						MessageBox.Show(Application.Current.MainWindow, $"Cannot find user group '{userGroup.GroupName}'");
					}
					return false;
				}

				if (userGroup.UserList.ContainsKey(username.ToUpper()))
				{
					if (Properties.Settings.Default.DefaultCulture.Name == "zh-TW")
					{
						MessageBox.Show(Application.Current.MainWindow, $"使用者名稱 '{username}' 已經存在，請使用其他名稱");
					}
					else
					{
						MessageBox.Show(Application.Current.MainWindow, $"Username '{username}' already exists");
					}
					return false;
				}

				User user = new User()
				{
					UserName = username,
					PasswordHash = GetMd5Hash(password),
					Group = userGroup
				};

				userGroup.UserList.Add(user.UserName.ToUpper(), user);
				SaveToFile();
				return true;
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}

			return false;
		}

		public static bool ChangePassword(User user, string password)
		{
			try
			{
				if (user == null)
				{
					throw new ArgumentNullException("user");
				}

				user.PasswordHash = GetMd5Hash(password);

				SaveToFile();

				return true;
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}

			return false;
		}

		public static bool DeleteUser(string username)
		{
			try
			{
				User user = GetUser(username);

				if (user == null)
				{
					if (Properties.Settings.Default.DefaultCulture.Name == "zh-TW")
					{
						MessageBox.Show(Application.Current.MainWindow, $"找不到使用者 '{username}'");
					}
					else
					{
						MessageBox.Show(Application.Current.MainWindow, $"Cannot find user '{username}'");
					}
					return false;
				}

				//if (CurnUser.Group.Level <= user.Group.Level)
				//{
				//	MessageBox.Show(Application.Current.MainWindow, $"您無法刪除此使用者，因為目標的使用者群組 '{user.Group.GroupName}' 權限高於您");
				//	return false;
				//}

				user.Group.UserList.Remove(username.ToUpper());
				SaveToFile();
				return true;
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}

			return false;
		}

		public static bool Login(string username, string password)
		{
			try
			{
				User user = GetUser(username);

				if (user == null)
				{
					if (Properties.Settings.Default.DefaultCulture.Name == "zh-TW")
					{
						MessageBox.Show(Application.Current.MainWindow, $"找不到使用者 '{username}'");
					}
					else
					{
						MessageBox.Show(Application.Current.MainWindow, $"Cannot find user '{username}'");
					}
					return false;
				}

				if (VerifyMd5Hash(password, user.PasswordHash) == true)
				{
					CurnUser = user;

					LogUserOperation("Login");
					return true;
				}
				else
				{
					if (Properties.Settings.Default.DefaultCulture.Name == "zh-TW")
					{
						MessageBox.Show(Application.Current.MainWindow, "密碼錯誤");
					}
					else
					{
						MessageBox.Show(Application.Current.MainWindow, $"Password incorrect");
					}
					return false;
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
			return false;
		}

		public static void Logout()
		{
			if (IsCurnUserGuest == false)
			{
				LogUserOperation("Logout");
			}

			CurnUser = GetUser("Guest");
		}

		public static User GetUser(string userName_)
		{
			User user = null;
			foreach (UserGroup group in UserGroups)
			{
				if (group.UserList.TryGetValue(userName_.ToUpper(), out user) == true)
				{
					break;
				}
			}
			return user;
		}

		public static string GetMd5Hash(string src_)
		{
			using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
			{
				//byte[] srcBys = UTF8Encoding.Default.GetBytes(src_);
				byte[] srcBys = Encoding.UTF8.GetBytes(src_);
				byte[] hashCode = md5.ComputeHash(srcBys);
				string str = BitConverter.ToString(hashCode);
				return str.Replace("-", "");
			}
		}

		public static bool VerifyMd5Hash(string src_, string hash_)
		{
			string srcHash = GetMd5Hash(src_);
			StringComparer comparer = StringComparer.OrdinalIgnoreCase;
			return comparer.Compare(srcHash, hash_) == 0 ? true : false;
		}

		public static void LogUserOperation(string message)
		{
			MainWindow.ucUILog?.SystemLog.Log($"User: {CurnUser.UserName}, Message = {message}");

			string sql = $@"insert into USER_Log
							(Username, Message, LogTime) VALUES
							(
								'{CurnUser.UserName}',
								'{message.Replace("'", "''")}',
								'{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}'
							)";

			//App.Host_SQL_Server.NonQuery(sql);
		}

		public static bool IsCurnUserGuest { get { return _curnUser.UserName.ToUpper() == "GUEST"; } }

		public static bool CheckAuthority(Authorities authority)
		{
			//if (IsCurnUserGuest)
			//{
			//	MessageBox.Show(Application.Current.MainWindow, "請先登入使用者再進行操作", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
			//	return false;
			//}

			if (CurnUser.Group.HasThisAuthority(authority) == false)
			{
				if (Properties.Settings.Default.DefaultCulture.Name == "zh-TW")
				{
					MessageBox.Show(Application.Current.MainWindow, "您目前所屬的使用者群組無法進行此操作", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
				else
				{
					MessageBox.Show(Application.Current.MainWindow, "Current user group cannot perform this operation", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
				return false;
			}

			return true;
		}
	}

	public class UserGroup
	{
		public string GroupName { get; set; }
		public int Level { get; set; }
		public SortedList<string, User> UserList { get; set; }
		public string AccessibleElement;

		[JsonIgnore]
		private uint _AccessibleElementBits;
		[JsonIgnore]
		public uint AccessibleElementBits
		{
			get { return _AccessibleElementBits; }
			set
			{
				if (_AccessibleElementBits == value) return;
				_AccessibleElementBits = value;
				AccessibleElement = Convert.ToString(_AccessibleElementBits, 16).ToUpper();
			}
		}

		public UserGroup()
		{
			UserList = new SortedList<string, User>();
		}

		/// <summary>
		/// 註冊新權限
		/// </summary>
		public void RegisterAuthority(Password.Authorities authority)
		{
			AccessibleElementBits |= (uint)authority;
		}

		/// <summary>
		/// 取消註冊此權限
		/// </summary>
		public void DeregisterAuthority(Password.Authorities authority)
		{
			AccessibleElementBits &= uint.MaxValue ^ (uint)authority;
		}

		/// <summary>
		/// 傳回此使用者群組是否包含此權限
		/// </summary>
		public bool HasThisAuthority(Password.Authorities authority)
		{
			return (AccessibleElementBits & (uint)authority) == (uint)authority;
		}
	}

	public class User
	{
		public string UserName { get; set; }
		public string PasswordHash { get; set; }

		[JsonIgnore]
		public UserGroup Group { get; set; }
	}
}
