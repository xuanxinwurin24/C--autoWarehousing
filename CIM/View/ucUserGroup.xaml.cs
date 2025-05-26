using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CIM.Lib.Model;
using CIM.Lib.View;
using Newtonsoft.Json;


namespace CIM.View
{
	/// <summary>
	/// ucUserGroup.xaml 的互動邏輯
	/// </summary>
	public partial class ucUserGroup : UserControl
	{
		private string currentUserGroup;

		public ucUserGroup()
		{
			InitializeComponent();
#if DEBUG
			if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
#endif
			AddTagToCheckBox();
		}

		private void AddTagToCheckBox()
		{
		}

		public void InitializeParameter()
		{
			cbUserGroup.Items.Clear();
			foreach (UserGroup userGroup in Password.UserGroups)
			{
				if (userGroup.GroupName.ToUpper() == "GUEST") continue;

				if (userGroup.Level <= Password.CurnUser.Group.Level)
				{
					cbUserGroup.Items.Add(userGroup.GroupName);
				}
			}

			cbUserGroup.SelectedIndex = cbUserGroup.Items.IndexOf(Password.CurnUser.Group.GroupName);
		}

		private void CbUserGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cbUserGroup.SelectedIndex == -1) return;

			try
			{
				currentUserGroup = cbUserGroup.SelectedValue.ToString();

				btnSave.IsEnabled = currentUserGroup != "Administrator";

				UserGroup userGroup = Password.UserGroups.FirstOrDefault(x => x.GroupName == currentUserGroup);

				chkAGVRemote.IsChecked = userGroup.HasThisAuthority(Password.Authorities.MainAGVsRemote);
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				UserGroup userGroup = Password.UserGroups.FirstOrDefault(x => x.GroupName == currentUserGroup);

				string openedAuthorities = "開啟 {";
				string closedAuthorities = "關閉 {";

				foreach (GroupBox groupBox in stackPanelAuthorities.Children)
				{
					foreach (CheckBox checkBox in CommonMethods.FindVisualChildren<CheckBox>(groupBox))
					{
						Password.Authorities authority = (Password.Authorities)checkBox.Tag;

						//開啟權限
						if (checkBox.IsChecked == true)
						{
							//紀錄使用者修改哪個權限
							if (userGroup.HasThisAuthority(authority) == false)
							{
								openedAuthorities += $"{authority.ToChineseString()};";
							}
							userGroup.RegisterAuthority(authority);
						}
						//關閉權限
						else
						{
							//紀錄使用者修改哪個權限
							if (userGroup.HasThisAuthority(authority) == true)
							{
								closedAuthorities += $"{authority.ToChineseString()};";
							}
							userGroup.DeregisterAuthority(authority);
						}
					}
				}

				openedAuthorities += "}";
				closedAuthorities += "}";

				if (Password.SaveToFile() == true)
				{
					if (openedAuthorities != "開啟 {}" || closedAuthorities != "關閉 {}")
					{
						Password.LogUserOperation($"修改權限 {openedAuthorities} {closedAuthorities}");
					}
					MessageBox.Show(FindResource("SavedSuccessfully") as string);
				}
				else
				{
					MessageBox.Show(FindResource("FailedToSaveTheSetting") as string);
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void btnCreateUser_Click(object sender, RoutedEventArgs e)
		{
			if (Password.CheckAuthority(Password.Authorities.UserGroupCreateUser) == false) return;

			fmCreateUser fmCreateUser = new fmCreateUser(currentUserGroup);
			fmCreateUser.ShowDialog();
		}

		private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
		{
			if (Password.CheckAuthority(Password.Authorities.UserGroupDeleteUser) == false) return;

			fmDeleteUser fmDeleteUser = new fmDeleteUser();
			fmDeleteUser.ShowDialog();
		}
	}

	public class CheckBoxEnableConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string userGroup = value as string;
			if (string.IsNullOrWhiteSpace(userGroup)) return false;
			return userGroup != Password.CurnUser.Group.GroupName;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
