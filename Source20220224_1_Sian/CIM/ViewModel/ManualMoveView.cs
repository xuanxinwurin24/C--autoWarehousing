using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CIM.ViewModel
{
	public class Cell_Move : INotifyPropertyChanged
	{
		/// <INotifyPropertyChanged>
		public event PropertyChangedEventHandler PropertyChanged;
		void NotifyPropertyChanged([CallerMemberName] string propertyName_ = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName_));
		}
		/// </INotifyPropertyChanged>

		private bool _isSelected;

		public bool isSelected
		{
			get { return _isSelected; }
			set
			{
				if (_isSelected == value) return;
				_isSelected = value;
				NotifyPropertyChanged();
			}
		}


		private string carousel_ID;

		public string CAROUSEL_ID
		{
			get { return carousel_ID; }
			set
			{
				if (carousel_ID == value) return;
				carousel_ID = value;
				NotifyPropertyChanged();
			}
		}

		private string cell_ID;

		public string CELL_ID
		{
			get { return cell_ID; }
			set
			{
				if (cell_ID == value) return;
				cell_ID = value;
				NotifyPropertyChanged();
			}
		}
		private int status;

		public int STATUS
		{
			get { return status; }
			set
			{
				if (status == value) return;
				status = value;
				NotifyPropertyChanged();
			}
		}

		private string batch_NO;

		public string BATCH_NO
		{
			get { return batch_NO; }
			set
			{
				if (batch_NO == value) return;
				batch_NO = value;
				NotifyPropertyChanged();
			}
		}
		private string box_ID;

		public string BOX_ID
		{
			get { return box_ID; }
			set
			{
				if (box_ID == value) return;
				box_ID = value;
				NotifyPropertyChanged();
			}
		}

		private string group_NO;

		public string GROUP_NO
		{
			get { return group_NO; }
			set
			{
				if (group_NO == value) return;
				group_NO = value;
				NotifyPropertyChanged();
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
				NotifyPropertyChanged();
			}
		}

		private string customer_ID;

		public string CUSTOMER_ID
		{
			get { return customer_ID; }
			set
			{
				if (customer_ID == value) return;
				customer_ID = value;
				NotifyPropertyChanged();
			}
		}

		private DateTime store_time;

		public DateTime STORED_TIME
		{
			get { return store_time; }
			set
			{
				if (store_time == value) return;
				store_time = value;
				NotifyPropertyChanged();
			}
		}

		private int check_Result;

		public int CHECK_RESULT
		{
			get { return check_Result; }
			set
			{
				if (check_Result == value) return;
				check_Result = value;
				NotifyPropertyChanged();
			}
		}

	}
}
