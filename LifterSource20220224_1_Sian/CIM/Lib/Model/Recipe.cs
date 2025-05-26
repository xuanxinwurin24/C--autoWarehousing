using Strong;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace CIM.Lib.Model
{
    [XmlRoot("Root")]
    public class Recipe
    {
        public static string FileName = Environment.CurrentDirectory + @"\Ini\Recipe\Recipe.xml";
        public SortedList<int, RecipeBody> BodyList = new SortedList<int, RecipeBody>();
        public void ReLoadFromFile()
        {
            try
            {
                SortedList<int, RecipeBody> oldLis_ = new SortedList<int, RecipeBody>(BodyList);//back up current RcpList
                LoadFromFile();//load new BodyList
            }
            catch (Exception e_)
            { MessageBox.Show(FileName + " File read error," + e_.ToString()); }
        }
        public void LoadFromFile()
        {
            try
            {
                if (File.Exists(FileName) == false)
                {
                    MessageBox.Show(FileName + " no exist");
                    Environment.Exit(Environment.ExitCode);
                }
                BodyList.Clear();
                List<RecipeBody> list = Common.DeserializeXMLFileToObject<List<RecipeBody>>(FileName);
                foreach (RecipeBody rcp in list)
                {
                    while (rcp.Steps.Count < RecipeBody.MaxStepCount)
                    {
                        rcp.Steps.Add(new RcpStep());
                    }
                }

                foreach (RecipeBody obj in list)
                {
                    if (BodyList.ContainsKey(obj.ID) == false)
                    {
                        BodyList.Add(obj.ID, obj);
                    }
                }
            }
            catch (Exception e_)
            { MessageBox.Show(FileName + " File read error," + e_.ToString()); }
        }
        public void SaveFile()
        {
            try
            {
                Common.SerializeXMLObjToFile<List<RecipeBody>>(Recipe.FileName, BodyList.Values.ToList<RecipeBody>());

            }
            catch (Exception e_)
            { MessageBox.Show(FileName + " File read error," + e_.ToString()); }
        }
        #region GetFuncs

        public RecipeBody GetObj(int ID_)
        {
            RecipeBody obj = null;
            if (BodyList.TryGetValue(ID_, out obj) == true)
            { return obj; }
            else
            { return null; }
        }
        //public RecipeBody GetObj(string ID_)//RecipeNoCheck
        //{
        //    RecipeBody obj = null;
        //    if (BodyList.TryGetValue(ID_.Trim(), out obj) == true)
        //    { return obj; }
        //    else
        //    { return null; }
        //}
        //public bool isExist(string ID_)
        //{
        //    return (GetObj(ID_.ToString()) != null);
        //}
        //public string GetRcpVersion(string ID_)
        //{
        //    RecipeBody obj = GetObj(ID_);
        //    if (obj == null) return "";
        //    return obj.Version.Trim();
        //}
        #endregion GetFuncs
    }

    public class RecipeBody : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public const int MaxStepCount = 11;
        public RecipeBody()
        {
            ID = 0;
            Version = DateTime.Now.ToString("yyyyMMddHHmmss");
        }
        #region Xml Property   
        public int ID { get; set; }
        public bool PsetupEnable { get; set; }
        

        public List<int> PsetupTime { get; set; }

        public List<int> PsetEnable { get; set; }

        public List<RcpStep> Steps { get; set; }
        public string Version { get; set; }
        #endregion Xml Property  
    }
    public class RcpStep : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public RcpStep()
        {
            Group = "null";
        }
        public int No { get; set; }
        public string Group { get; set; }
        public int StayOverTime { get; set; }
    }
}
