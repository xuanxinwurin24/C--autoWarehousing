using System;
using System.Windows;
using Strong;

namespace CIM.Lib.View
{
    /// <summary>
    /// Interaction logic for fmAlarmCode.xaml
    /// </summary>
    public partial class fmMemGroup
        : Window
    {
        //XElement xml;
        MemGroup mg;
        public fmMemGroup(string mgFileName_)
        {
            mg = Common.DeserializeXMLFileToObject<MemGroup>(mgFileName_);
            if (mg == null)
            {
                MessageBox.Show(mgFileName_ + " no exist");
                Environment.Exit(Environment.ExitCode);
            }
            InitializeComponent();
            DataContext = mg;
            dataGrid.ItemsSource = mg.ItemList;
            Title = mgFileName_;
        }      
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            mg.AddrBase = 101;
            //xml.Save(MainWindow.sAlarmFileName);
            Common.SerializeXMLObjToFile<MemGroup>("FileNmae", mg);
            this.DialogResult = true;
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }    
}
