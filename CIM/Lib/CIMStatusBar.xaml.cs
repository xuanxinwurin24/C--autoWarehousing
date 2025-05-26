using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using System.Windows.Threading;
using CIM.View;
using CIM.Language;
using CIM.Lib.Model;
using CIM.Lib.View;
using static CIM.CommonMethods;

namespace CIM.Lib
{
	/// <summary>
	/// CIMStatusBar.xaml 的互動邏輯
	/// </summary>
	public partial class CIMStatusBar : UserControl
	{
		public CIMStatusBar()
		{
			InitializeComponent();
			lbWebServiceConnectMode.DataContext = App.Bc;
			if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
			{
				return;
			}

			//txbUserName.DataContext = Password.CurnUserName;

			//DeviceMemoryView.MenuForMemStatus(MainMenu);
		}

		private void BtnLogin_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				fmLogin fmLogin = new fmLogin();
				fmLogin.ShowDialog();
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void BtnLogout_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Password.Logout();
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				fmChangePassword fmChangePassword = new fmChangePassword();
				fmChangePassword.ShowDialog();
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void BtnGoToDesktop_Click(object sender, RoutedEventArgs e)
		{
			ShowDesktop();
		}

		private void cbChangeLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox cb = sender as ComboBox;
			if (cb.SelectedIndex < 0) return;

			LanguageHelper.ChangeCulture((CultureInfo)cb.ItemContainerGenerator.Items[cb.SelectedIndex]);
		}
	}

	public class UserLevelConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class UserLoginConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				string userName = value.ToString().ToUpper();

				if ((string)parameter == "Login")
				{
					if (userName == "GUEST")
						return Visibility.Visible;
					else
						return Visibility.Collapsed;
				}
				else if ((string)parameter == "Logout")
				{
					if (userName == "GUEST")
						return Visibility.Collapsed;
					else
						return Visibility.Visible;
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
