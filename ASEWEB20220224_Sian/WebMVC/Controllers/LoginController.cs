using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASEWEB.Models;
using Microsoft.AspNetCore.Http;

namespace ASEWEB.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index(/*string UserID,string UserPass,string Authority*/)
        {
            //LoginModel data = new LoginModel(UserID,UserPass);
            //string Auth = data.Authority_string();

            //if (Auth != "0")
            //{
            //    TempData["Authority"] = Auth;
            //    return RedirectToAction("Index", "Frame");
            //}
            if (Startup.STK_Connect)
                TempData["STK_Connect_Message"] = "OK";
            else
                TempData["STK_Connect_Message"] = "Stocker資料庫連接失敗";
            return PartialView();
        }
        [HttpPost]
        public IActionResult Index(IFormCollection post)
        {
            if (Startup.STK_Connect)
                TempData["STK_Connect_Message"] = "OK";
			else
			{
                TempData["STK_Connect_Message"] = "Stocker資料庫連接失敗";
                //return PartialView();
            }
            LoginModel data = new LoginModel(post["UserID"],post["UserPass"]);
            string Auth = data.Authority_string();
            if (Auth != "0")
            {
                TempData["Authority"] = Auth;
                TempData["User"] = data.UserID;
                TempData["UserGroup"] = data.UserGroup;
                TempData["UserName"] = data.UserName;
                TempData["UserSystemSetting"] = data.UserSystemSetting;
                return RedirectToAction("Index", "Frame");
            }
            TempData["ToUser"]="Login Error:"+post["UserID"]+post["UserPass"];
            return RedirectToAction("Index", "Login");
        }
        public IActionResult LanguageForLayout(string lan)
        {
            if (lan != null)
            {
                TempData["Language"] = lan;
                TempData["LanTemp"] = lan;
            }
            if (lan == null)
                TempData["Language"] = TempData["LanTemp"];
            return PartialView();
        }
        public IActionResult Logout()
        {
            TempData["Authority"] = "0";
            return RedirectToAction("Index", "Login");
        }
    }
}
