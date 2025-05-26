using CIM.Lib.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
	/// fmCreateUser.xaml 的互動邏輯
	/// </summary>
	public partial class fmCreateUser : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private string _currentUserGroup;
		public string CurrentUserGroup
		{
			get { return _currentUserGroup; }
			set
			{
				if (_currentUserGroup == value) return;
				_currentUserGroup = value;
				OnPropertyChanged();
			}
		}

		public fmCreateUser(string currentUserGroup)
		{
			InitializeComponent();
			lblCurrentUserGroup.DataContext = this;

			CurrentUserGroup = currentUserGroup;

			edUsername.Focus();
		}

		private void btnCreate_Click(object sender, RoutedEventArgs e)
		{
			if (edPasswordConfirm.Password != edPassword.Password)
			{
				//MessageBox.Show(this, FindResource("PasswordConfirmationDoesNotMatch") as string, TimeSpan.FromSeconds(3));
				return;
			}

			string username = edUsername.Text.Trim();
			string password = edPassword.Password.Trim();

			if (Password.NewUser(username, password, Password.UserGroups.FirstOrDefault(x => x.GroupName == CurrentUserGroup)))
			{
				Password.LogUserOperation($"新增使用者 {username} ({CurrentUserGroup})");
				//MessageBox.Show(this, FindResource("UserCreatedSuccessfully") as string, TimeSpan.FromSeconds(3));
				edUsername.Clear();
				edPassword.Clear();
				edPasswordConfirm.Clear();
				edUsername.Focus();
			}
		}

		private void edPasswordConfirm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				btnCreate_Click(null, null);
			}
		}
	}

	class CurrentUserGroupConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string userGroup = value.ToString();
			if (Properties.Settings.Default.DefaultCulture.Name == "zh-TW")
			{
				return $"將於 {userGroup} 中新增使用者";
			}
			else
			{
				return $"User will be created into {userGroup}";
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return DependencyProperty.UnsetValue;
		}
	}
}
