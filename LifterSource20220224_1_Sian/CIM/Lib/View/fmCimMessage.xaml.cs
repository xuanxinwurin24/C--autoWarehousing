using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CIM.Lib.View
{
	/// <summary>
	/// fmCimMessage.xaml 的互動邏輯
	/// </summary>
	public partial class fmCimMessage : Window
	{
		private static ObservableCollection<MessageBody> messageList = new ObservableCollection<MessageBody>();

		partial class MessageBody : INotifyPropertyChanged
		{
			private DateTime _time;
			public DateTime Time
			{
				get { return _time; }
				set
				{
					if (_time == value) return;
					_time = value;
					OnPropertyChanged();
				}
			}

			private string _message;
			public string Message
			{
				get { return _message; }
				set
				{
					if (_message == value) return;
					_message = value;
					OnPropertyChanged();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;
			void OnPropertyChanged([CallerMemberName] string name_ = "")
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name_));
			}
		}

		public fmCimMessage()
		{
			InitializeComponent();

			listViewMessage.ItemsSource = messageList;

			messageList.CollectionChanged += (s, e) =>
			{
				if (messageList.Count == 0)
				{
					Close();
				}
				else
				{
					Show();
				}
			};
		}

		public static void InsertMessage(string message)
		{
			MessageBody body = new MessageBody() { Time = DateTime.Now, Message = message };
			Application.Current.Dispatcher.Invoke(() => messageList.Add(body));
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
			Hide();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var body = ((sender as Button).Tag as ListViewItem).DataContext as MessageBody;
			messageList.Remove(body);
		}
	}
}
