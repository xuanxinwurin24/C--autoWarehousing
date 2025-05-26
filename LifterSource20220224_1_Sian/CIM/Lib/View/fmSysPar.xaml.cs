using CIM.Lib.Model;
using Strong;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace CIM.Lib.View
{
	/// <summary>
	/// Interaction logic for fmHamsPar.xaml
	/// </summary>
	public partial class fmSysPar : Window
	{
		Model.SysPara para;
		public fmSysPar()
		{
			InitializeComponent();
			para = Common.DeserializeXMLFileToObject<SysPara>(SysPara.FileName);
			//cbPreTake.Visibility = App.BcHs.BcPara.PreTakeLine == true ? Visibility.Visible : Visibility.Collapsed;
			DataContext = para;
			LogInOutEventCallBackFunc(null, null);
			Password.LogInOutEvent += LogInOutEventCallBackFunc;
		}
		void LogInOutEventCallBackFunc(string sOldUserName_, string sNewUserName_)
		{
			stackMain.IsHitTestVisible = Password.CurnLevel >8;
		}
		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			//if (InputCorrect() == false) return;
			Common.SerializeXMLObjToFile<SysPara>(SysPara.FileName, para);
			App.SysPara = para;//parameter reload
			//App.BcHs.TimerChange();
			Close();
		}
		private void btnDefault_Click(object sender, RoutedEventArgs e)
		{
			para.ModelName = "MachineName";
			para.InLineID = "LineID";

			DataContext = null;//refresh UI
			DataContext = para;
		}
		private void btnExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
