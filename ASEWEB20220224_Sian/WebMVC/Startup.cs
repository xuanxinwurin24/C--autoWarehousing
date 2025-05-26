using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASEWEB.Models;


namespace ASEWEB
{
    public class Startup
    {
        public static SysPara SysPara;
        public static SQL_DB STK_SQLServer;
        public static SQL_DB Local_SQLServer;
        public static bool STK_Connect;
        public static bool Local_Connect;
        public static string sSysDir = Environment.CurrentDirectory;
        //public static LogWriter LogFile;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            Sql_conn();

            //UseEndpoints URL 路由邏輯/[Controller]/[ActionName]/[Parameters]
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Login}/{action=Index}/{id?}");
            });
        }
        public static void Sql_conn()
        {
            try
            {
                //建立App Log
                //LogFile = new LogWriter(sSysDir + @"\LogFile\AppLog", "App", 500000);
                sSysDir = System.Environment.CurrentDirectory;
                //讀取參數
                //SysPara = Common.DeserializeXMLFileToObject<SysPara>(SysPara.FileName);
                //WS_Setting = Common.DeserializeXMLFileToObject<WebServiceSetting>(App.sSysDir + "\\Ini\\WebService.xml");
                //SQL_Parameter STK_SQL_Para = Common.DeserializeXMLFileToObject<SQL_Parameter>(sSysDir + "\\Ini\\STK_SQL_Parameter.xml");
                //SQL_Parameter Local_SQL_Para = Common.DeserializeXMLFileToObject<SQL_Parameter>(sSysDir + "\\Ini\\Local_SQL_Parameter.xml");
                //Alarm.LoadFromFile();
                //Password.Initial();
                
                SQL_Parameter STK_SQL_Para = new SQL_Parameter();
                //string[] lines = System.IO.File.ReadAllLines("~/Ini/STK_SQL_Parameter.txt");
                //STK_SQL_Para.DataSource = lines[0];
                //STK_SQL_Para.InitialCatalog = lines[1];
                //STK_SQL_Para.UserID = lines[2];
                //STK_SQL_Para.Password = lines[3];
                STK_SQL_Para.DataSource = "172.22.244.47,1433";
                STK_SQL_Para.InitialCatalog = "ASE";
                STK_SQL_Para.UserID = "CPC";
                STK_SQL_Para.Password = "CPC01";

                SQL_Parameter Local_SQL_Para = new SQL_Parameter();
                //lines = System.IO.File.ReadAllLines( "~/Ini/Local_SQL_Parameter.txt");
                //Local_SQL_Para.DataSource = lines[0];
                //Local_SQL_Para.InitialCatalog = lines[1];
                //Local_SQL_Para.UserID = lines[2];
                //Local_SQL_Para.Password = lines[3];
                Local_SQL_Para.DataSource = "172.22.244.41,1433";
                Local_SQL_Para.InitialCatalog = "WaferBank_DB";
                Local_SQL_Para.UserID = "strong";
                Local_SQL_Para.Password = "5999011";
                //建立Thread
                //MainThread = new ThreadComp("MainThreadComp");
                //MainThread.CycleRunEvent += MainThreadComp_CycleRunEvent;

                //DatabaseThread = new ThreadComp("DatabaseThread");
                //DatabaseThread.CycleRunEvent += DatabaseThread_CycleRunEvent;

                //建立SQL連線

                STK_SQLServer = new SQL_DB(STK_SQL_Para);
                Local_SQLServer = new SQL_DB(Local_SQL_Para);
                if (STK_SQLServer.ConnectToDB() == false)
                    STK_Connect = false;
                else
                    STK_Connect = true;
                //LogWriter.LogAndExit("STK DataBase連線失敗，請確認!");
                if (Local_SQLServer.ConnectToDB() == false)
                    Local_Connect = false;
                else
                    Local_Connect = true;
			}
            catch (Exception ex)
            {
                //LogExcept.LogException(ex);
            }
        }
    }
}
