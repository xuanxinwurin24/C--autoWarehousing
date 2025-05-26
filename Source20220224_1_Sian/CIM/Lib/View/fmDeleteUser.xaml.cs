using CIM.Lib.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CIM.Lib.View
{
	/// <summary>
	/// fmDeleteUser.xaml 的互動邏輯
	/// </summary>
	public partial class fmDeleteUser : Window
	{
		private bool _IsTopUser;
		private User currentUser;

		public fmDeleteUser()
		{
			InitializeComponent();
			currentUser = Password.CurnUser;
			_IsTopUser = currentUser.UserName.ToUpper() == "STRONG";

			InitList();
		}

		private void InitList()
		{
			lsbUsers.Items.Clear();

			Func<UserGroup, bool> predicate;

			//Strong 登入的話全部使用者都列出，其他使用者則指會列出比自己還低 Level 的使用者群組
			if (_IsTopUser)
			{
				predicate = new Func<UserGroup, bool>(x => true);
			}
			else
			{
				predicate = new Func<UserGroup, bool>(x => x.Level < currentUser.Group.Level);
			}

			//根據條件列出可以刪除的使用者
			foreach (UserGroup group in Password.UserGroups.Where(predicate))
			{
				foreach (KeyValuePair<string, User> pair in group.UserList.Where(x => x.Value.UserName.ToUpper().In("STRONG", "GUEST") == false))
				{
					lsbUsers.Items.Add($"{pair.Value.UserName} ({pair.Value.Group.GroupName})");
				}
			}
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (lsbUsers.SelectedIndex == -1) return;
			
			string username = lsbUsers.SelectedItem.ToString().Split(' ')[0];

			string message = Properties.Settings.Default.DefaultCulture.Name == "zh-TW" ? $"確定要刪除使用者 '{username}' 嗎?" : $"Do you want to delete user '{username}'?";
			string caption = Properties.Settings.Default.DefaultCulture.Name == "zh-TW" ? "確認" : "Confirm";

			if (MessageBoxResult.No == MessageBox.Show(this, message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question)) return;

			if (Password.DeleteUser(username))
			{
				Password.LogUserOperation($"刪除使用者 {lsbUsers.SelectedItem}");
				//MessageBox.Show(this, FindResource("UserDeletedSuccessfully") as string, TimeSpan.FromSeconds(3));
				lsbUsers.Items.RemoveAt(lsbUsers.SelectedIndex);
			}
		}
	}
}
