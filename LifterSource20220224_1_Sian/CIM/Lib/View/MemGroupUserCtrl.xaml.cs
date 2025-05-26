using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

using Strong;
using CIM.Lib.Model;

namespace CIM.Lib.View
{
    /// <summary>
    /// Interaction logic for MemGroupControl.xaml
    /// </summary> 

    partial class MemGroupControl : UserControl
    {
        public int PasswordLevel { get; set; }

        public bool RowDetailVisible
        { get; set; }

        public TagItemStrType StrShowType
        {
            get
            {
                return Mgm.StrTypeSetting;
            }
            set
            {
                Mgm.StrTypeSetting = value;
            }
        }

        public MemGroupModel Mgm;
        public MemGroupControl()
        { }
        public MemGroupControl(MemGroup mg_)
        {
            InitializeComponent();

            dataGrid.Columns[1].Header = string.Format("Addr({0},{1})", mg_.Bank.ToString(), mg_.AddHexDisp == true ? "16" : "10");
            Mgm = new MemGroupModel(mg_);
            dataGrid.ItemsSource = Mgm.ItemMs;
            this.DataContext = this;

            LogInOutEventCallBackFunc(null, null);
            Password.LogInOutEvent += LogInOutEventCallBackFunc;
        }
        void LogInOutEventCallBackFunc(string sOldUserName_, string sNewUserName_)
        {
            clValue.IsReadOnly = Password.CurnLevel < 9;
            cbAllCol.Visibility = Password.CurnLevel < 9 ? Visibility.Collapsed : Visibility.Visible;
        }
        private void UserControl_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void subVal_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox txt = e.Source as TextBox;
            if (txt != null)
            { txt.IsHitTestVisible = Password.CurnLevel >= 9; }
        }
    }
    public class EnumToRadioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
    public class RowDetailsVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool b = (bool)value;
            return b == true ? DataGridRowDetailsVisibilityMode.Visible : DataGridRowDetailsVisibilityMode.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DataGridRowDetailsVisibilityMode vm = (DataGridRowDetailsVisibilityMode)value;
            return vm == DataGridRowDetailsVisibilityMode.Visible ? true : false;
        }
    }
    public class ColumnVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool b = (bool)value;
            return b == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}