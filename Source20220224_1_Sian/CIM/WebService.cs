using CIM.Lib.Model;
using Microsoft.CSharp;
using Strong;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;


namespace CIM
{
	#region AuthHeader
	public class AuthHeader : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private bool _headerEnabled;
		public bool HeaderEnabled
		{
			get { return _headerEnabled; }
			set
			{
				if (_headerEnabled == value) return;
				_headerEnabled = value;
				NotifyPropertyChanged();
			}
		}

		private string _userID;
		public string UserID
		{
			get { return _userID; }
			set
			{
				if (_userID == value) return;
				_userID = value;
				NotifyPropertyChanged();
			}
		}

		private string _password;
		public string Password
		{
			get { return _password; }
			set
			{
				if (_password == value) return;
				_password = value;
				NotifyPropertyChanged();
			}
		}

		public AuthHeader()
		{

		}
	}
	#endregion AuthHeader

	#region WebServiceSetting
	public class WebServiceSetting : INotifyPropertyChanged
	{

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private bool _webServiceUsed;
		public bool WebServiceUsed
		{
			get { return _webServiceUsed; }
			set
			{
				if (_webServiceUsed == value) return;
				_webServiceUsed = value;
				NotifyPropertyChanged();
			}
		}

		private string _uri;
		public string Uri
		{
			get { return _uri; }
			set
			{
				if (_uri == value) return;
				_uri = value;
				NotifyPropertyChanged();
			}
		}

		public AuthHeader AuthHeaderInfo { get; set; }

		public WebServiceSetting()
		{
			AuthHeaderInfo = new AuthHeader();
		}
	}


	#endregion WebServiceSetting

	public class WebService
	{
		public WebServiceSetting setting;// = new WebServiceSetting();
		public static ChannelFactory<ASE_WebService.IWmsAutoStkServiceChannel> channelFactory = null;

		public static ChannelFactory<ASE_CardID_WebService.ICardReviewServiceChannel> CardID_channelFactory = null;
		public WebService()
		{
		}

		/// <summary>
		/// 功能列表
		/// </summary>
		public enum FunctionType
		{
			///<summary>入庫/調儲/一般出庫-庫存檢查</summary>
			BinInReq,
			///<summary>入庫完成/調儲完成</summary>
			BinInWork,
			///<summary>出庫下架-一般出庫完成</summary>
			BankOutNormalWork,
			///<summary>出庫下架-詢問領料清單</summary>
			OrderIssueReq,
			///<summary>出庫下架-發料出庫完成</summary>
			BankOutIssueWork,
			///<summary>盤點完成</summary>
			InventoryWork,
		}

		public static async Task<ASE_WebService.OutParam> CallWebService(ASE_WebService.InParam inputParameter)
		{
			BasicHttpBinding binding = new BasicHttpBinding("BasicHttpsBinding_IWmsAutoStkService");
			EndpointAddress endpoint = new EndpointAddress("https://mybiztest/WaferCenterWeb/Services/WmsAutoStkService.svc");
			channelFactory = new ChannelFactory<ASE_WebService.IWmsAutoStkServiceChannel>(binding, endpoint);
			ASE_WebService.IWmsAutoStkServiceChannel channel = channelFactory.CreateChannel();
			return await channel.DoEventAsync(inputParameter);
		}
		public static async Task<string> Call_UserID_WebService1(string sCard_ID_)
		{
			using (ASE_CardID_WebService.CardReviewServiceClient _client = new ASE_CardID_WebService.CardReviewServiceClient())
			{
				var x = _client.GetCardInfoAsync("WAFER", sCard_ID_, false);
				
				return await x;
			}
		}
		public static async Task<string> Call_UserID_WebService(string sCard_ID_)
		{
			try
			{
				using (ASE_CardID_WebService.CardReviewServiceClient _client = new ASE_CardID_WebService.CardReviewServiceClient())
				{
					var x = _client.GetCardInfoAsync("WAFER", sCard_ID_, false);//123564改成sCard_ID_ HR改成WAFER
					return await x;
				}

				//BasicHttpBinding binding = new BasicHttpBinding("BasicHttpsBinding_ICardReviewService");
				//EndpointAddress endpoint = new EndpointAddress("https://hris/HRS/CardReviewService/CardReviewService.svc");


				//CardID_channelFactory = new ChannelFactory<ASE_CardID_WebService.ICardReviewServiceChannel>(binding, endpoint);

				//// step one - find and remove default endpoint behavior 
				//var defaultCredentials = CardID_channelFactory.Endpoint.Behaviors.Find<ClientCredentials>();
				//CardID_channelFactory.Endpoint.Behaviors.Remove(defaultCredentials);


				//// step two - instantiate your credentials
				////ClientCredentials loginCredentials = new ClientCredentials();
				////loginCredentials.Windows.ClientCredential.UserName = "KH\\PP4960";
				////loginCredentials.Windows.ClientCredential.Password = "K10WaferCenter";
				////loginCredentials.Windows.AllowNtlm = true;
				////loginCredentials.Windows.ClientCredential.Domain = "";
				////loginCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
				//// step three - set that as new endpoint behavior on factory
				////CardID_channelFactory.Endpoint.Behaviors.Add(loginCredentials); //add required ones

				//ASE_CardID_WebService.ICardReviewServiceChannel channel = CardID_channelFactory.CreateChannel();
				//return await channel.GetCardInfoAsync("WAFER", sCard_ID_, false);
			}
			catch (Exception ex_)
			{
				LogWriter.LogException(ex_.InnerException);
			}
			return null;
		}
		
	}
}
