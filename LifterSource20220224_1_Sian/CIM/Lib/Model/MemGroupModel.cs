using System;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using Strong;
using System.Windows;
using System.Collections.Generic;

namespace CIM.Lib.Model
{
    public class MemGroupModel
    {
        public MemGroup Mg;

        public TagItemStrType strTypeSetting = TagItemStrType.Default;
        public TagItemStrType StrTypeSetting
        {
            get { return strTypeSetting; }
            set
            {
                if (strTypeSetting == value) { return; }
                strTypeSetting = value;
                //ICollectionView view = CollectionViewSource.GetDefaultView(itemMs);
                //if (view != null)
                //{ view.Refresh(); }
                foreach (ItemModel itm in itemMs)
                { itm.StrTypeSetting = strTypeSetting; }//will refresh grid
            }
        }

        List<ItemModel> itemMs = new List<ItemModel>();
        public List<ItemModel> ItemMs
        {
            get { return itemMs; }
        }
        public MemGroupModel(MemGroup mg_)
        {
            Mg = mg_;
            foreach (TagItem itm in mg_.ItemList)
            {
                itemMs.Add(new ItemModel(itm, false));
            }
        }
    }

    public class ItemModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public TagItem Item { get; set; }

        public TagItemStrType CurnShowType = TagItemStrType.Default;
        TagItemStrType strTypeSetting = TagItemStrType.Default;
        public TagItemStrType StrTypeSetting
        {
            get { return strTypeSetting; }
            set
            {
                if (strTypeSetting == value) { return; }
                strTypeSetting = value;

                TagItemStrType newSetting = strTypeSetting == TagItemStrType.Default ? Item.StrType : strTypeSetting;
                if (CurnShowType == newSetting) { return; }
                CurnShowType = newSetting;
                OnPropertyChanged(Item);
            }
        }
        //public string Name
        //{
        //    get
        //    {
        //        return Item.Name;
        //    }
        //}
        public string Addr
        {
            get
            {
                int absAdr = Item.AbsAddr();
                if(Item.Mg.Device.GetType().Name == "PRRecoderDev")
                { absAdr++; }
                return Item.Mg.AddHexDisp == true ? absAdr.ToString("X4") : absAdr.ToString("D4");
            }
        }
        //public bool BitUse
        //{
        //    get { return Item.BitUse; }
        //}
        //public int StBit
        //{
        //    get { return Item.StBit; }
        //}
        //public int Length
        //{
        //    get { return Item.Length; }
        //}
        //public TagItemStrType StrType
        //{
        //    get { return Item.StrType; }
        //}
        //public bool EventLog
        //{
        //    get { return Item.EventLog; }
        //}
        //public int BitOnTime
        //{
        //    get { return Item.BitOnTime; }
        //}
        //public TagItemTrigEdge BitTrigerEdge
        //{
        //    get { return Item.BitTrigerEdge; }
        //}
        //public bool FirstEventByPass
        //{
        //    get { return Item.FirstEventByPass; }
        //}
        //public string Hint
        //{
        //    get { return Item.Hint; }
        //}
        //public string Description
        //{
        //    get { return Item.Description; }
        //}    
        public string stringValue = "  ";
        public string StringValue
        {
            get { return stringValue; }
            set
            {
                string OriStr = stringValue;
                Item.Log("User Key in:" + OriStr + "->" + value);
                Item.SetString(value, CurnShowType);//Write to PLC
            }
        }
        public Visibility HasSubValues
        {
            get
            {
                if (Item.BitUse == false && Item.Length > 1)
                {
                    return Visibility.Visible;
                }
                else
                { return Visibility.Collapsed; }
            }
        }

        List<SubItem> subItems;
        public List<SubItem> SubItems
        {
            get { return subItems; }
        }
        public ItemModel(TagItem it_, bool bSimple_ = true)
        {
            Item = it_;
            CurnShowType = Item.StrType;
            subItems = new List<SubItem>();

            Item.UIUpdateEvent += new TagItem.UIUpdateEventHandler(OnPropertyChanged);

            if (bSimple_ == false && Item.BitUse == false && Item.Length > 1)
            {
                for (int i = 0; i < Item.Length; i++)
                {
                    subItems.Add(new SubItem(this, i));
                }
            }
            RefreshStrValue();
        }
        void OnPropertyChanged(TagItem item_)//on call from TagItem
        {
            RefreshStrValue();
            if (PropertyChanged != null)
            {
                //Item.Log("OnPropertyChanged F =" + stringValue);
                PropertyChanged(this, new PropertyChangedEventArgs("StringValue"));
                //Item.Log("OnPropertyChanged B =" + stringValue);
            }
        }
        void RefreshStrValue()
        {
            stringValue = Item.GetString(CurnShowType);
            foreach (SubItem sub in subItems)
            {
                sub.RefreshStrValue();
            }
        }
    }

    public class SubItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ItemModel ItemM;
        public int AdrOft = 0;
        public string Name { get; set; }
        public string Addr { get; set; }
        public string stringValue = "  ";
        public string StringValue
        {
            get
            {
                return stringValue;
            }
            set
            {
                WriteToDevMemory(value);
            }
        }

        public void RefreshStrValue()
        {
            try
            {
                string val;
                if (ItemM.CurnShowType == TagItemStrType.ASC)
                {
                    try
                    {
                        if (ItemM.stringValue.Length < ((AdrOft << 1) + 2))
                        { val = "  "; }
                        else
                        { val = ItemM.stringValue.Substring(AdrOft << 1, 2); }

                    }
                    catch (Exception e_) { MessageBox.Show("Value Error"); return; }
                }
                else
                {
                    string[] Strs = ItemM.stringValue.Split(new Char[] { ',', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (Strs.Length <= AdrOft)
                    { val = "00"; }
                    else
                    { val = Strs[AdrOft]; }
                }

                if (stringValue == val) return;
                stringValue = val;
                OnPropertyChanged();
            }
            catch (Exception e_) { MessageBox.Show("Value Error"); return; }
        }
        public SubItem(ItemModel itm_, int Ofs_)
        {
            ItemM = itm_;
            AdrOft = Ofs_;
            Name = ItemM.Item.Name + AdrOft.ToString();
            int absAdr = ItemM.Item.AbsAddr() + AdrOft;
            Addr = ItemM.Item.Mg.AddHexDisp == true ? absAdr.ToString("X8") : absAdr.ToString("D10");
        }
        private void WriteToDevMemory(string Str_)
        {
            try
            {
                string OriStr = ItemM.Item.GetString(ItemM.CurnShowType);
                string dstStr;

                if (ItemM.Item.BitUse == true || ItemM.Item.Length == 1)
                { dstStr = Str_; }
                else
                {
                    if (ItemM.CurnShowType == TagItemStrType.ASC)
                    {
                        StringBuilder sb = new StringBuilder(OriStr);
                        sb[AdrOft << 1] = Str_[0];
                        sb[(AdrOft << 1) + 1] = Str_[1];
                        dstStr = sb.ToString();
                    }
                    else
                    {
                        string[] Strs = OriStr.Split(new Char[] { ',', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Strs[AdrOft] = Str_;
                        dstStr = string.Join(",", Strs);
                    }
                }

                ItemM.Item.Log("User Key in:" + OriStr + "->" + dstStr);
                ItemM.Item.SetString(dstStr, ItemM.CurnShowType);
            }
            catch (Exception e_) { { MessageBox.Show("Value Error"); return; } }
        }
        public void OnPropertyChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("StringValue"));
            }
        }
    }

    public class StrToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //if (value is string)
            //{
            //    string str = value as string;
            //    return (str.Trim() == "1") ? true : false;
            //}
            //else
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //if (value is string)
            //{
            //    string str = value as string;
            //    return (str.Trim() == "True") ? "1" : "0";
            //}
            //else
            return value;
        }
    }
}
