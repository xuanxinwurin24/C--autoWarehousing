using CIM.Lib.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;

namespace CIM.Language
{
	public class LanguageHelper
	{
		private static bool _isFoundInstalledCultures = false;

		private static string _resourcePrefix = "LanguageDictionary";

		private static string _culturesFolder = "Language";

		private static List<CultureInfo> _supportedCultures = new List<CultureInfo>();

		public static List<CultureInfo> SupportedCultures
		{
			get
			{
				return _supportedCultures;
			}
		}

		public LanguageHelper()
		{
			try
			{
				if (!_isFoundInstalledCultures)
				{

					CultureInfo cultureInfo = new CultureInfo("");

					List<string> files = Directory.GetFiles(string.Format("{0}\\{1}", System.Windows.Forms.Application.StartupPath, _culturesFolder))
						.Where(s => s.Contains(_resourcePrefix) && s.ToLower().EndsWith("xaml")).ToList();

					foreach (string file in files)
					{
						try
						{
							string FileName = Path.GetFileName(file);
							string cultureName = FileName.Substring(FileName.IndexOf(".") + 1).Replace(".xaml", ""); //LanguangeDictionary.語系名稱.xaml , 獲得"語系名稱"

							cultureInfo = CultureInfo.GetCultureInfo(cultureName);

							if (cultureInfo != null)
							{
								_supportedCultures.Add(cultureInfo);
							}
						}
						catch (ArgumentException)
						{
						}
					}

					if (_supportedCultures.Count > 0 && Properties.Settings.Default.DefaultCulture != null)
					{
						ChangeCulture(Properties.Settings.Default.DefaultCulture);
					}

					_isFoundInstalledCultures = true;
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

		public static void ChangeCulture(CultureInfo culture)
		{
			try
			{
				if (_supportedCultures.Contains(culture))
				{
					ResourceDictionary resourceDictionary = Application.LoadComponent(new Uri(@"Language\LanguageDictionary." + culture.Name + ".xaml", UriKind.Relative)) as ResourceDictionary;

					if (Application.Current.Resources.MergedDictionaries.Count > 0)
						Application.Current.Resources.MergedDictionaries.Clear();

					Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
					Properties.Settings.Default.DefaultCulture = culture;
					Properties.Settings.Default.Save();
				}
			}
			catch (Exception ex)
			{
				LogExcept.LogException(ex);
			}
		}

	}
}
