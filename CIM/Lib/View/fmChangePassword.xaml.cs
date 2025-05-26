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
	/// PasswordChange.xaml 的互動邏輯
	/// </summary>
	public partial class fmChangePassword : Window
	{
		public fmChangePassword()
		{
			InitializeComponent();
			Initial();

			edOldPW_Change.Focus();
		}

		public void Initial()
		{
		}

		private void btnChange_Click(object sender, RoutedEventArgs e)
		{
			//if (Password.VerifyMd5Hash(edOldPW_Change.Password, Password.CurnUser.PasswordHash) == false)
			//{
				
			//	MessageBox.Show(this, FindResource("CurrentPasswordIsNotCorrect") as string, TimeSpan.FromSeconds(3));
			//	return;
			//}

			//if (string.IsNullOrEmpty(edNewPW1_Change.Password))
			//{
			//	MessageBox.Show(this, FindResource("PleaseEnterNewPassword") as string, TimeSpan.FromSeconds(3));
			//	return;
			//}

			//if (edNewPW1_Change.Password.Contains(' '))
			//{
			//	MessageBox.Show(this, FindResource("PasswordCannotContainBlankSpaces") as string, TimeSpan.FromSeconds(3));
			//	return;
			//}

			//if (edNewPW1_Change.Password != edNewPW2_Change.Password)
			//{
			//	MessageBox.Show(this, FindResource("PasswordConfirmationDoesNotMatch") as string, TimeSpan.FromSeconds(3));
			//	return;
			//}

			//if (Password.ChangePassword(Password.CurnUser, edNewPW1_Change.Password))
			//{
			//	MessageBox.Show(this, FindResource("PasswordChangedSuccessfully") as string, TimeSpan.FromSeconds(3));
			//	Close();
			//}
			//else
			//{
			//	MessageBox.Show(this, FindResource("FailedToChangePassword") as string, TimeSpan.FromSeconds(3));
			//	return;
			//}
		}

		private void btn_Cancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void edNewPW2_Change_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.Key == Key.Return)
				{
					btnChange_Click(sender, e);
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}
	}
}
