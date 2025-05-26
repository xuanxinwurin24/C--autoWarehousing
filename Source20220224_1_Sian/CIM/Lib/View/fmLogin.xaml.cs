using CIM.Lib.Model;
using System;
using System.Collections.Generic;
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
	/// Login.xaml 的互動邏輯
	/// </summary>
	public partial class fmLogin : CustomWindow
	{
		public fmLogin()
		{
			InitializeComponent();

			edLogin_UserName.Focus();
		}

		private void Password_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.Key == Key.Return)
				{
					btnLogin_Click(sender, e);
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		private void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (Password.Login(edLogin_UserName.Text.Trim(), edLogin_Password.Password) == true)
				{
					MessageBox.Show("登入成功!");
					//MessageBox.Show(FindResource("LoggedInSuccessfully") as string);
					Close();
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
	}
}
