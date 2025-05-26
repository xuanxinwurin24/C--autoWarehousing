using System.Collections.Generic;
using System.Windows;
using CIM.Lib.Model;
using Strong;

namespace CIM.Lib.View
{
    /// <summary>
    /// Interaction logic for fmAlarmCode.xaml
    /// </summary>
    public partial class fmAlarm : Window
    {
        //XElement xml;
        List<AlarmBody> list = new List<AlarmBody>();
        public fmAlarm()
        {
            list = Common.DeserializeXMLFileToObject<List<AlarmBody>>(Alarm.FileName);
            InitializeComponent();
            dataGrid.ItemsSource = list;

            //ObservableCollection<AlarmCode> objList = new ObservableCollection<AlarmCode>(list);
            //dataGrid.IsSynchronizedWithCurrentItem = true;
            //dataGrid.ItemsSource = objList;

            //xml = XDocument.Load(MainWindow.sFileName).Root;
            //InitializeComponent();
            //dataGrid.DataContext = xml;
            grid.IsHitTestVisible = Password.CurnLevel >= 9;
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //xml.Save(MainWindow.sAlarmFileName);
            Common.SerializeXMLObjToFile<List<AlarmBody>>(Alarm.FileName, list);
            this.DialogResult = true;
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
