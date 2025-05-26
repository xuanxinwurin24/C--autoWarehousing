using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CIM.ViewModel
{
	public class CustomMenuItem : ListViewItem
	{
		public CustomMenuItem(string header, UserControl screen = null/*, PackIconKind icon*/, ClickEventHandler clickhandler = null)
		{
			SetResourceReference(ContentControl.ContentProperty, $"lang_{header}");
			Header = header;
			Screen = screen;
			//Icon = icon;
			ClickEvent = clickhandler;
		}
		public string Header { get; private set; }
		public UserControl Screen { get; private set; }
		//public PackIconKind Icon { get; private set; }
		public delegate void ClickEventHandler();
		public ClickEventHandler ClickEvent;
	}
}
