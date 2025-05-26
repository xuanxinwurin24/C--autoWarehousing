using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CIM.BC
{
    public class Store
    {
        #region Property
        public LogWriter LogFile = new LogWriter();
        #endregion

        #region INIT
        public Store()
        {
            try
            {
                LogFile.PathName = App.sSysDir + @"\LogFile\Store\";
                LogFile.sHead = "Store";
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        ~Store() { }

        public bool checkFilled(string sESBID_, string sBatchNo_, string sWay_)
        {
            uint uCheckItemFilled = 0;
            bool bResult = false;
            if (sWay_ == "Manual")
            {
                string sMsgBox = "請填寫";
                if (sESBID_ == "")
                {
                    sMsgBox += "靜電箱ID、";
                    ++uCheckItemFilled;
                }
                if (sBatchNo_ == "")
                {
                    sMsgBox += sBatchNo_ + "Batch No、";
                    ++uCheckItemFilled;
                }
                bResult = uCheckItemFilled == 0 ? true : false;
                if (!bResult)
                {
                    MessageBox.Show(sMsgBox.Substring(0, sMsgBox.Length - 1));
                }
                return bResult;
            }

            else if (sWay_ == "Auto")
            {
                if (sESBID_ == "")
                {
                    ++uCheckItemFilled;
                }
                if (sBatchNo_ == "")
                {
                    ++uCheckItemFilled;
                }

                bResult = uCheckItemFilled == 0 ? true : false;

                if (!bResult)
                {
                    MessageBox.Show("沒有資料");
                }
                return bResult;
            }
            return bResult;
        }

        public void InsertBatchList(List<StoreItem> itemList_)
        {
            foreach (StoreItem st in itemList_)
            {
                string sql = $"INSERT INTO BATCH_LIST(BOX_ID,BATCH_NO,BATCH_NO_DETAIL)" +
                    $"VALUES('{st.sESB}', '{st.sBatchNo}','{st.sBatchNoDetail}')";
                App.Local_SQLServer.NonQuery(sql);
            }
        }
        public void DropBatchList(List<StoreItem> itemList_)
        {
            foreach (StoreItem st in itemList_)
            {
                string sql = $"DELETE FROM BATCH_LIST WHERE BOX_ID = '{st.sESB}' AND BATCH_NO = '{st.sBatchNo}'";
                int iR = App.Local_SQLServer.NonQuery(sql);
            }
        }


        #endregion
        public void Initial()
        {

        }
    }

    public class StoreItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name_ = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name_));
        }
        public StoreItem(string sESB_, string sBatchno_, string sBatchnodetail_, long Groupno_, string sSoteria, string sCustomerNo_, string sOperatorNo_, string sStatus_)
        {
            sESB = sESB_;
            sBatchNo = sBatchno_;
            sBatchNoDetail = sBatchnodetail_;
            GroupNo = Groupno_;
            sSOTERIA = sSoteria;
            sCUSTOMERNO = sCustomerNo_;
            sOPERATORNO = sOperatorNo_;
            sSTATUS = sStatus_;
        }

        public StoreItem() { }

        private string esb = "";

        public string sESB
        {
            get { return esb; }
            set
            {
                if (esb == value) return;
                esb = value;
                OnPropertyChanged();
            }
        }

        private string batchno = "";

        public string sBatchNo
        {
            get { return batchno; }
            set
            {
                if (batchno == value) return;
                batchno = value;
                OnPropertyChanged();
            }
        }

        private string batchnodetail = "";

        public string sBatchNoDetail
        {
            get { return batchnodetail; }
            set
            {
                if (batchnodetail == value) return;
                batchnodetail = value;
                OnPropertyChanged();
            }
        }

        private long groupno = 0;

        public long GroupNo
        {
            get { return groupno; }
            set
            {
                if (groupno == value) return;
                groupno = value;
                OnPropertyChanged();
            }
        }

        private string soteria = "";

        public string sSOTERIA
        {
            get { return soteria; }
            set
            {
                if (soteria == value) return;
                soteria = value;
                OnPropertyChanged();
            }
        }

        private string customerno = "";

        public string sCUSTOMERNO
        {
            get { return customerno; }
            set
            {
                if (customerno == value) return;
                customerno = value;
                OnPropertyChanged();
            }
        }

        private string operatorno = "";

        public string sOPERATORNO
        {
            get { return operatorno; }
            set
            {
                if (operatorno == value) return;
                operatorno = value;
                OnPropertyChanged();
            }
        }

        private string status = "";

        public string sSTATUS
        {
            get { return status; }
            set
            {
                if (status == value) return;
                status = value;
                OnPropertyChanged();
            }
        }
    }
}
