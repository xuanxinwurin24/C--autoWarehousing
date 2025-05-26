using System;
using System.Collections.Generic;
using System.Windows;

using CIM.Lib.Model;

namespace CIM.Lib.View
{
    #region PASSWORD
    /// <summary>
    /// Interaction logic for fmPassword.xaml
    /// </summary>
    public partial class fmPassword : Window
    {
        public fmPassword()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UserNameListChanged();
            PasswordChanged();
        }
        //---------------------------------------------------------------------------
        private void PasswordChanged()
        {
            btnNewUserOk.IsEnabled = Password.CurnLevel >= 3;
            btnDelete.IsEnabled = Password.CurnLevel >= 3;
            //lbUserName.Text = User.CurnUserName;
            //lbLevel.Text = User.CurnLevel.ToString();
        }
        //---------------------------------------------------------------------------
        private void UserNameListChanged()
        {
            List<string> list = new List<string>();
            Password.UserNames(list);
            listBox1.ItemsSource = list;
            //listBox1.Items.Clear();
            //listBox1.Items..AddRange(list.ToArray());
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Password.Logout();
            PasswordChanged();
            Close();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (Password.Login(edLogin_UserName.Text.Trim(), edLogin_Password.Text.Trim()) == false)
            { MessageBox.Show("Login error"); return; }
            else
            {
                PasswordChanged();
                Close();
            }
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (Password.Login(Password.CurnUserName, edOldPW_Change.Text) == false)
            { MessageBox.Show("OldPassword error"); return; }
            if (edNewPW1_Change.Text.Trim() != edNewPW2_Change.Text.Trim())
            { MessageBox.Show("NewPassword1 no equal NewPassword2"); return; }
            Password.ChangePassword(edNewPW1_Change.Text.Trim());
            UserNameListChanged();
            PasswordChanged();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            //string text = listBox1.GetItemText(listBox1.SelectedItem);
            string text = listBox1.SelectedItem as string;
            if (text.Trim() == "GUEST" || text.Trim() == "STRONG")
            { return; }
            if (Password.DeleteUser(text) == true)
            {
                List<string> list = new List<string>();
                Password.UserNames(list);
                listBox1.ItemsSource = list;
            }
        }

        private void btnNewUserOk_Click(object sender, RoutedEventArgs e)
        {
            int iLevel;
            iLevel = int.TryParse(edLevel_New.Text.Trim(), out iLevel) == true ? iLevel : 0;
            Password.NewUser(edUserName_New.Text.Trim(), edPW_New.Text.Trim(), iLevel);
            UserNameListChanged();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Close();
        }
    }
    #endregion PASSWORD
}
