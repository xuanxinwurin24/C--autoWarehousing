using Strong;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CIM.Lib.Model
{
    #region USER
    public class Password
    {
        public delegate void LogInOutEventHandle(string sOldUserName_, string sNewUserName_);
        public static event LogInOutEventHandle LogInOutEvent;

        const string Admin_UserName = "STRONG";
        const string Admin_Password = "5999011";

        static string FileName = " ";

        static string curnUserName = "GUEST";
        public static string CurnUserName
        {
            get { return curnUserName; }
            set
            {
                if (curnUserName == value) { return; }
                string old = curnUserName;
                curnUserName = value;
                if (LogInOutEvent != null)
                {
                    LogInOutEvent(old, value);
                }
            }
        }
        public static string CurnPassword = "GUEST";
        //public static int CurnLevel = 0;

        static int curnLevel;
        public static int CurnLevel
        {
            get { return curnLevel; }
            set
            {
                curnLevel = value;
            }
        }
        public static string CurnMemo = "GUEST";

        public static void Initial(string filePath_, string fileName_)
        {
            FileName = filePath_ + "\\" + fileName_;
            try
            {
                if (Directory.Exists(filePath_) == false)
                {
                    if (Directory.CreateDirectory(filePath_) == null)
                    { MessageBox.Show("Can not create directory:" + filePath_); return; }
                }
                if (File.Exists(FileName) == false)
                {
                    CurnLevel = 9;
                    NewUser("Admin_UserName", "Admin_Password", 9);
                    NewUser("GUEST", "GUEST", 0);
                    CurnLevel = 0;
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
            Logout();
        }

        public static bool NewUser(string uName_, string pwd_, int level_)
        {
            uName_ = uName_.ToUpper();
            pwd_ = pwd_.ToUpper();
            return NewUser(uName_, pwd_, level_, "Memo");
        }

        public static bool NewUser(string uName_, string pwd_, int level_, string sMemo_)
        {
            uName_ = uName_.ToUpper();
            pwd_ = pwd_.ToUpper();
            if (level_ > CurnLevel)
            { MessageBox.Show("Can not new user that Level higher than your"); return false; }
            if (WriteRecord(false, uName_, pwd_, level_, sMemo_) == true)//false:over write
            { return true; }
            else { MessageBox.Show(uName_ + " Exist ,Please use another username"); }
            return false;
        }

        public static bool ChangePassword(string pwd_)
        {
            pwd_ = pwd_.ToUpper();
            if (WriteRecord(true, curnUserName, pwd_, CurnLevel, CurnMemo) == true)//true:change
            { return true; }
            else { MessageBox.Show(curnUserName + " no Exist ,Please use another username"); }
            return false;
        }

        public static bool DeleteUser(string uName_)
        {
            uName_ = uName_.ToUpper();
            TIniFile pIniFile = new TIniFile(FileName);
            if (pIniFile.Section_Exists(uName_) == true)
            {
                string levelHash = pIniFile.ReadString(uName_.ToUpper(), "Level", "0");
                int level = LevelHash2Int(levelHash);
                if (level >= CurnLevel)
                { MessageBox.Show("Can not delete Level higher than your's"); return false; }
                pIniFile.EraseSection_(uName_);
                return true;
            }
            return false;
        }

        public static void Administrator_Login()
        {
            Login(Admin_UserName, Admin_Password);
        }
        public static bool Login(string uName_, string inpPwd_)
        {
            uName_ = uName_.ToUpper();
            inpPwd_ = inpPwd_.ToUpper();
            //從下行開始
            TIniFile pIniFile = new TIniFile(FileName);
            try
            {
                string passwordHash = pIniFile.ReadString(uName_, "Password", "");
                if (passwordHash.Length == 0)
                { return false; }

                if (verifyMd5Hash(inpPwd_, passwordHash) == true)
                {
                    string levelHash = pIniFile.ReadString(uName_, "Level", "0");
                    CurnLevel = LevelHash2Int(levelHash);
                    //到這之前 要換成資料庫比對

                    if (CurnLevel <= 0 || CurnLevel > 9)
                    { CurnLevel = 0; }

                    CurnPassword = inpPwd_;
                    CurnUserName = uName_;
                    App.DS.OP.UserName = CurnUserName;
                    App.DS.OP.Level = "";// CurnLevel;
                    return true;
                }
            }
            catch (Exception e_) { LogWriter.LogException(e_); }
            return false;
        }

        public static void Logout()
        {
            CurnPassword = "GUEST";
            CurnLevel = 0;
            CurnUserName = "GUEST";
        }

        public static void UserNames(List<string> Lst)
        {
            TIniFile pIniFile = new TIniFile(FileName);
            pIniFile.ReadSection(Lst);
        }

        static bool WriteRecord(bool bExistWrite_, string uName_, string pwd_, int level_, string memo_)
        {
            uName_ = uName_.ToUpper();
            pwd_ = pwd_.ToUpper();
            TIniFile pIniFile = new TIniFile(FileName);
            if (pIniFile.Section_Exists(uName_) == bExistWrite_)
            {
                pIniFile.WriteString(uName_, "Password", GetMd5Hash(pwd_));
                pIniFile.WriteString(uName_, "Level", GetMd5Hash(level_.ToString()));
                pIniFile.WriteString(uName_, "Memo", memo_);
                return true;
            }
            return false;
        }

        static string GetMd5Hash(string src_)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                //Byte[] srcBys = UTF8Encoding.Default.GetBytes(src_);
                Byte[] srcBys = Encoding.UTF8.GetBytes(src_);
                Byte[] hashCode = md5.ComputeHash(srcBys);
                string str = BitConverter.ToString(hashCode);
                return str.Replace("-", "");
            }
        }

        static bool verifyMd5Hash(string src_, string hash_)
        {
            string srcHash = GetMd5Hash(src_);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(srcHash, hash_) == 0 ? true : false;
        }

        static int LevelHash2Int(string hashLevel_)
        {
            for (int i = 0; i <= 9; i++)
            {
                if (verifyMd5Hash(i.ToString(), hashLevel_) == true)
                { return i; }
            }
            return -1;
        }
    }
    #endregion TUSER

    #region IniFile
    public class TIniFile
    {
        [DllImport("kernel32")]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault_, Byte[] lpReturnedString, uint nSize, string lpFileName);
        [DllImport("kernel32")]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpValString, string lpFileName);

        public string Default_ValStr = " ";
        public string FileName;
        //---------------------------------------------------------------------------
        public TIniFile(string AFileName)
        {
            FileName = AFileName;
        }
        //---------------------------------------------------------------------------
        ~TIniFile()
        {
            UpdateFile();
        }
        //---------------------------------------------------------------------------
        public string ReadString(string Section_, string sKey_, string Default_)
        {
            Byte[] vBuffer = new byte[2048];
            uint vCount = GetPrivateProfileString(Section_, sKey_, Default_, vBuffer, (uint)vBuffer.Length, FileName);
            return Encoding.UTF8.GetString(vBuffer, 0, (int)vCount);
        }
        //---------------------------------------------------------------------------
        public bool WriteString(string Section_, string sKey_, string Value)
        {
            return WritePrivateProfileString(Section_, sKey_, Value, FileName);
        }
        //---------------------------------------------------------------------------
        public virtual bool Section_Exists(string Section_)
        {
            List<string> vStrings = new List<string>();
            ReadSection(vStrings);
            return vStrings.Contains(Section_);
        }
        //---------------------------------------------------------------------------
        public virtual bool ValueExists(string Section_, string sKey_)
        {
            List<string> vStrings = new List<string>();
            ReadSection(Section_, vStrings);
            return vStrings.Contains(sKey_);
        }
        //---------------------------------------------------------------------------
        public bool ReadSection_Values(string Section_, List<string> Strings)
        {
            Strings.Clear();
            List<string> vsKey_List = new List<string>();
            if (!ReadSection(Section_, vsKey_List)) return false;
            foreach (string vsKey_ in vsKey_List)
                Strings.Add(string.Format("{0}={1}", vsKey_, ReadString(Section_, vsKey_, "")));
            return true;
        }
        //---------------------------------------------------------------------------
        public bool ReadSection(string Section_, List<string> Strings)
        {
            Strings.Clear();
            Byte[] vBuffer = new byte[16384];
            uint vLength = GetPrivateProfileString(Section_, null, null, vBuffer, (uint)vBuffer.Length, FileName);

            int j = 0;
            for (int i = 0; i < vLength; i++)
            {
                if (vBuffer[i] == 0)
                {
                    Strings.Add(Encoding.UTF8.GetString(vBuffer, j, i - j));
                    j = i + 1;
                }
            }
            return true;
        }
        //---------------------------------------------------------------------------
        public bool ReadSection(List<string> Strings)
        {
            return ReadSection(null, Strings);
        }
        //---------------------------------------------------------------------------
        public bool EraseSection_(string Section_)
        {
            return WritePrivateProfileString(Section_, null, null, FileName);
        }
        //---------------------------------------------------------------------------
        public bool DeleteKey(string Section_, string sKey_)
        {
            return WritePrivateProfileString(Section_, sKey_, null, FileName);
        }
        //---------------------------------------------------------------------------
        public bool UpdateFile()
        {
            return WritePrivateProfileString(null, null, null, FileName);
        }
    }
    #endregion IniFile
}
