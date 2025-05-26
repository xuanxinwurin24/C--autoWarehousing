using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CIM.Lib.View
{
    /// <summary>
    /// Interaction logic for Plc.xaml
    /// </summary>
    public partial class DeviceMemoryView : Window
    {
        MemGroupControl mgUserCtrl;

        string mgOwner = null;
        public DeviceMemoryView(Device Dev_, string Owner_, string Header_)
        {
            mgOwner = Owner_;
            InitializeComponent();

            Title = Header_ + ":" + Dev_.Name;
            NewTabPages_ForDevMems(Dev_);
        }
        void NewTabPages_ForDevMems(Device Dev_)
        {
            try
            {
                MgTabCtrl.Items.Clear();
                foreach (MemGroup mg in Dev_.MemGroupList)
                {
                    if (mg.Owner != mgOwner) continue;
                    mgUserCtrl = new MemGroupControl(mg);

                    TabItem tabPage = new TabItem();
                    tabPage.Content = new Grid();
                    tabPage.Header = mg.Name;
                    tabPage.Content = mgUserCtrl;
                    MgTabCtrl.Items.Add(tabPage);
                }
            }
            catch (Exception ex)
            {
                LogExcept.LogException(ex);
            }
        }       
        //---------static part--------------------------------------------------------------
        public static MenuItem MenuForMemStatus(Menu mainManu_)
        {
            MenuItem MemoryMainMenu = new MenuItem();
            MemoryMainMenu.Header = "Memory Status";
            MemoryMainMenu.ToolTip = "Memory Status";
            mainManu_.Items.Add(MemoryMainMenu);

            foreach (ThreadComp thr in ThreadComp.Members)
            {
                //-----------add thread layer into mainMenu
                MenuItem thrMenuItem = new MenuItem();
                thrMenuItem.Header = thr.Name;
                thrMenuItem.Tag = thr;
                MemoryMainMenu.Items.Add(thrMenuItem);
                //-----------add device layer into thread's layer
                foreach (Device dev in thr.DeviceList)
                {
                    MenuItem devMenuItem = new MenuItem();
                    devMenuItem.Header = dev.Name;
                    devMenuItem.Tag = dev;
                    thrMenuItem.Items.Add(devMenuItem);

                    aDevMemoryAddToMenu(devMenuItem, dev);//--------add module layer into device's layer
                }
            }
            return MemoryMainMenu;
        }

        //--------add module layer into device's layer  
        static void aDevMemoryAddToMenu(MenuItem devMemu, Device dev_)
        {
            List<string> mgs = new List<string>();
            string NullOwner = "";
            mgs.Clear();

            //search memGroups in same module(Owner)
            foreach (MemGroup mg in dev_.MemGroupList)
            {
                if (mg.Owner == null)
                { mg.Owner = NullOwner; }
                if (mgs.Contains(mg.Owner) == true) continue;
                mgs.Add(mg.Owner);

                MenuItem mgItem = new MenuItem();
                devMemu.Items.Add(mgItem);
                mgItem.Click += new RoutedEventHandler(aMemGroupShow_ClickEvent);
                //Type owner = mg.Owner.GetType();
                //mgItem.Header = owner.Name;
                mgItem.Header = mg.Owner;
                mgItem.Tag = mg;
            }
        }
        static void aMemGroupShow_ClickEvent(object sender, RoutedEventArgs e_)
        {
            MenuItem menu = sender as MenuItem;
            MemGroup mg = (MemGroup)(menu.Tag);
            DeviceMemoryView win = new DeviceMemoryView(mg.Device, mg.Owner, (string)menu.Header);
            win.Show();
        }
    }
}
