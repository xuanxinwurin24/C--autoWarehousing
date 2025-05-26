using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Text.Unicode;
namespace ASEWEB.Controllers
{
    public class HelloWorldController : Controller
    {
        // GET: /HelloWorld/
        //https://localhost:44332/HelloWorld/
        public IActionResult Index() //預設呼叫的方法
        {
            //return "This is my default action...";
            return View();
            //View() 會抓到Views/HelloWold/index.cshtml
        }

        // GET: /HelloWorld/Welcome/ 
        //https://localhost:44332/HelloWorld/Welcome
        //public string Welcome()
        //{
        //    return "This is the Welcome action method...";
        //}

        //不能有同樣的方法名稱!!
        // GET: /HelloWorld/Welcome/ 
        //https://localhost:44332/HelloWorld/Welcome?name=Rick&numtimes=4
        // Requires using System.Text.Encodings.Web;
        //public string Welcome(string name, int numTimes = 1)
        //{
        //    return HtmlEncoder.Default.Encode($"Hello {name}, NumTimes is: {numTimes}");
        //}

        //https://localhost:44332/HelloWorld/Welcome/3?name=Rick
        //排列順序為3?name=Rick，因為UseEndpoints URL 路由邏輯/[Controller]/[ActionName]/[Parameters]
        //https:~3?name=Pick的?表示從尾端?開始查詢字串

        //URL 路由邏輯/[Controller]/[ActionName]/[Parameters]
        //{controller=Home}/{action=Index}/{id?}
        //id?的問號 代表此項變數可有可無
        public string Welcome(string name, int ID = 1)
        {
            return HtmlEncoder.Create(UnicodeRanges.All).Encode($"Hello {name}, ID: {ID}");
        }
    }
}
