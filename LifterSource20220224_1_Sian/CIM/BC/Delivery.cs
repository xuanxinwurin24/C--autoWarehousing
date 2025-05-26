using CIM.Lib.Model;
using Strong;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace CIM.BC
{
    public class Delivery
    {
        #region Property
        public LogWriter LogFile = new LogWriter();
        #endregion

        #region INIT
        public Delivery()
        {
            try
            {
                LogFile.PathName = App.sSysDir + @"\LogFile\Delivery\";
                LogFile.sHead = "Delivery";
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
        }
        ~Delivery() { }

        #endregion
        public void Initial()
        {

        }
    }
    public class DeliItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string name_ = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name_));
        }
        public DeliItem(string sOrderNo_, string sFactory_, string sBatchno_, long Groupno_, string sSoteria_, string sCustormerNo_, string sStatus_, string sOperatorNo_)
        {
            sOrderNo = sOrderNo_;
            sFACTORY = sFactory_;
            sBatchNo = sBatchno_;
            GroupNo = Groupno_;
            SOTERIA = sSoteria_;
            CUSTOMERNO = sCustormerNo_;
            sSTATUS = sStatus_;
            OPERATORNO = sOperatorNo_;
        }
        public DeliItem() { }

        private string orderno;

        public string sOrderNo
        {
            get { return orderno; }
            set
            {

                if (orderno == value) return;
                orderno = value;
                OnPropertyChanged();
            }
        }

        private string factory;

        public string sFACTORY
        {
            get { return factory; }
            set
            {
                if (factory == value) return;
                factory = value;
                OnPropertyChanged();
            }
        }


        private string batchno;

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

        private long groupno;

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

        private string soteria;

        public string SOTERIA
        {
            get { return soteria; }
            set
            {
                if (soteria == value) return;
                soteria = value;
                OnPropertyChanged();
            }
        }

        private string CustomerNo;

        public string CUSTOMERNO
        {
            get { return CustomerNo; }
            set
            {
                if (CustomerNo == value) return;
                CustomerNo = value;
                OnPropertyChanged();
            }
        }


        private string status;
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

        private string operatorno = "";

        public string OPERATORNO
        {
            get { return operatorno; }
            set
            {
                {
                    if (operatorno == value) return;
                    operatorno = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool isChecked;

        public bool bIsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked == value) return;
                isChecked = value;
                OnPropertyChanged();
            }
        }
    }
}
