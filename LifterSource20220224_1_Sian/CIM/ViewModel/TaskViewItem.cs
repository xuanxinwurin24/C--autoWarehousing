using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CIM.ViewModel
{
    public class TaskViewItem : INotifyPropertyChanged
	{
		/// <INotifyPropertyChanged>
		public event PropertyChangedEventHandler PropertyChanged;
		void NotifyPropertyChanged([CallerMemberName] string propertyName_ = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName_));
		}

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

		private string _srcpos;
		public string SRCPOS
		{
			get { return _srcpos; }
			set
			{
				if (_srcpos == value) return;
				_srcpos = value;
				NotifyPropertyChanged();
			}
		}
		private string _srccell;
		public string SRCCELL
		{
			get { return _srccell; }
			set
			{
				if (_srccell == value) return;
				_srccell = value;
				NotifyPropertyChanged();
			}
		}
		private string _tarpos;
		public string TARPOS
		{
			get { return _tarpos; }
			set
			{
				if (_tarpos == value) return;
				_tarpos = value;
				NotifyPropertyChanged();
			}
		}
		private string _tarcell;
		public string TARCELL
		{
			get { return _tarcell; }
			set
			{
				if (_tarcell == value) return;
				_tarcell = value;
				NotifyPropertyChanged();
			}
		}

		private string _boxid;
		public string BOXID
		{
			get { return _boxid; }
			set
			{
				if (_boxid == value) return;
				_boxid = value;
				NotifyPropertyChanged();
			}
		}
		private string _batch_no;
		public string BATCH_NO
		{
			get { return _batch_no; }
			set
			{
				if (_batch_no == value) return;
				_batch_no = value;
				NotifyPropertyChanged();
			}
		}
		private string _status;
		public string STATUS
		{
			get { return _status; }
			set
			{
				if (_status == value) return;
				_status = value;
				NotifyPropertyChanged();
			}
		}
		private string _direction;
		public string DIRECTION
		{
			get { return _direction; }
			set
			{
				if (_direction == value) return;
				_direction = value;
				NotifyPropertyChanged();
			}
		}
		private int _priority;
		public int PRIORITY
		{
			get { return _priority; }
			set
			{
				if (_priority == value) return;
				_priority = value;
				NotifyPropertyChanged();
			}
		}
		private string _commandid;
		public string COMMANDID
		{
			get { return _commandid; }
			set
			{
				if (_commandid == value) return;
				_commandid = value;
				NotifyPropertyChanged();
			}
		}
		private string _starttime;
		public string STARTTIME
		{
			get { return _starttime; }
			set
			{
				if (_starttime == value) return;
				_starttime = value;
				NotifyPropertyChanged();
			}
		}
		private string _soteria;
		public string SOTERIA
		{
			get { return _soteria; }
			set
			{
				if (_soteria == value) return;
				_soteria = value;
				NotifyPropertyChanged();
			}
		}
		private string _customerid;
		public string CUSTOMER_ID
		{
			get { return _customerid; }
			set
			{
				if (_customerid == value) return;
				_customerid = value;
				NotifyPropertyChanged();
			}
		}
	}
}
