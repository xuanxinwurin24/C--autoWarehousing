using System.Collections.Generic;
using System.Windows;
using Strong;
using CIM.Lib.Model;
using System.Windows.Data;
using System;

namespace CIM.Lib.View
{
    /// <summary>
    /// Interaction logic for fmRecipe.xaml
    /// </summary>
    public partial class fmRecipe : Window
    {
        List<RecipeBody> list = new List<RecipeBody>();
        bool bEdit = true;
        public fmRecipe(bool bEdit_ = true)
        {
            bEdit = bEdit_;

            list = Common.DeserializeXMLFileToObject<List<RecipeBody>>(Recipe.FileName);
            foreach (RecipeBody rcp in list)
            {
                while (rcp.Steps.Count < RecipeBody.MaxStepCount)
                {
                    rcp.Steps.Add(new RcpStep());
                }
            }
            InitializeComponent();
            
            cm = (como)Resources["comoData"];
            cm.Groups = App.Bc.Groups;
            lv.ItemsSource = list;
        }
        como cm;
     
    
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (bEdit == true)
            {
                Common.SerializeXMLObjToFile<List<RecipeBody>>(Recipe.FileName, list);//save to file 
                this.DialogResult = true;
            }
            //App.Rcp.ReLoadFromFile();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            //Close();
        }

        private void dataGrid_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
        }

        private void Lv_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
    public class como
    {
        public List<string> Groups { get; set; }

        public como()
        {
            //Groups = App.Bc.Groups;
        }
    }
    public class CvGroup : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = (string)value;
            return App.Bc.Groups.IndexOf(s);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                int idx = (int)value;
                return App.Bc.Groups[idx];
            }
            catch (Exception e_) { }
            return "null";
        }
    }
}
