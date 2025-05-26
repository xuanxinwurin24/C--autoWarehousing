﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CIM.Lib
{
	/// <summary>
	/// Interaction logic for Led.xaml
	/// </summary>
	public partial class Led : UserControl
	{
		#region Dependency properties

		/// <summary>Dependency property to Get/Set the current IsActive (True/False)</summary>
		public static readonly DependencyProperty IsActiveProperty =
			DependencyProperty.Register("IsActive", typeof(bool?), typeof(Led),
				new PropertyMetadata(null, new PropertyChangedCallback(Led.IsActivePropertyChanced)));

		/// <summary>Dependency property to Get/Set Color when IsActive is true</summary>
		public static readonly DependencyProperty ColorOnProperty =
			DependencyProperty.Register("ColorOn", typeof(Color), typeof(Led),
				new PropertyMetadata(Colors.Green, new PropertyChangedCallback(Led.OnColorOnPropertyChanged)));

		/// <summary>Dependency property to Get/Set Color when IsActive is false</summary>
		public static readonly DependencyProperty ColorOffProperty =
			DependencyProperty.Register("ColorOff", typeof(Color), typeof(Led),
				new PropertyMetadata(Colors.Red, new PropertyChangedCallback(Led.OnColorOffPropertyChanged)));

		/// <summary>Dependency property to Get/Set Color when IsActive is false</summary>
		public static readonly DependencyProperty ColorNullProperty =
			DependencyProperty.Register("ColorNull", typeof(Color), typeof(Led),
				new PropertyMetadata(Colors.Gray, new PropertyChangedCallback(Led.OnColorNullPropertyChanged)));

		/// <summary>Dependency property to Get/Set if led will flash</summary>
		public static readonly DependencyProperty FlashingProperty =
			DependencyProperty.Register("Flashing", typeof(bool), typeof(Led),
			new PropertyMetadata(false, new PropertyChangedCallback(Led.OnFlashingPropertyChanged)));

		/// <summary>Dependency property to Get/Set period of flash in milliseconds</summary>
		public static readonly DependencyProperty FlashingPeriodProperty =
			DependencyProperty.Register("FlashingPeriod", typeof(int), typeof(Led),
				new PropertyMetadata(500, new PropertyChangedCallback(Led.OnFlashingPeriodPropertyChanged)));

		#endregion

		#region Wrapper Properties

		/// <summary>Gets/Sets Value</summary>
		public bool? IsActive
		{
			get { return (bool?)GetValue(IsActiveProperty); }
			set
			{
				SetValue(IsActiveProperty, value);
			}
		}

		/// <summary>Gets/Sets Color when led is True</summary>
		public Color ColorOn
		{
			get
			{
				return (Color)GetValue(ColorOnProperty);
			}
			set
			{
				SetValue(ColorOnProperty, value);
			}
		}

		/// <summary>Gets/Sets Color when led is False</summary>
		public Color ColorOff
		{
			get
			{
				return (Color)GetValue(ColorOffProperty);
			}
			set
			{
				SetValue(ColorOffProperty, value);
			}
		}

		/// <summary>Gets/Sets Color when led is False</summary>
		public Color ColorNull
		{
			get
			{
				return (Color)GetValue(ColorNullProperty);
			}
			set
			{
				SetValue(ColorNullProperty, value);
			}
		}

		/// <summary>Gets/Sets Flashing beaviour on true or false</summary>
		public bool Flashing
		{
			get
			{
				return (bool)GetValue(FlashingProperty);
			}
			set
			{
				SetValue(FlashingProperty, value);
			}
		}

		/// <summary>Gets/Sets Flashing period in ms</summary>
		public int FlashingPeriod
		{
			get
			{
				return (int)GetValue(FlashingPeriodProperty);
			}
			set
			{
				SetValue(FlashingPeriodProperty, value);
			}
		}

		#endregion

		#region Private fields

		DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);

		#endregion

		#region Constructor

		public Led()
		{
			InitializeComponent();
			timer.Interval = TimeSpan.FromMilliseconds(FlashingPeriod);
			timer.Tick += new EventHandler(timer_Tick);
			if (this.IsActive == true)
				this.backgroundColor.Color = this.ColorOn;
			else if (this.IsActive == false)
				this.backgroundColor.Color = this.ColorOff;
			else
				this.backgroundColor.Color = this.ColorNull;

			//AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(gridBigLed_MouseLeftButtonDown), true);
		}

		#endregion

		#region Callbacks

		/// <summary> tick of flashing timer </summary>
		void timer_Tick(object sender, EventArgs e)
		{
			if (this.IsActive == true)
			{
				if (this.backgroundColor.Color == this.ColorOn)
					this.backgroundColor.Color = this.ColorNull;
				else
					this.backgroundColor.Color = this.ColorOn;
			}

			if (this.IsActive == false)
			{
				if (this.backgroundColor.Color == this.ColorOff)
					this.backgroundColor.Color = this.ColorNull;
				else
					this.backgroundColor.Color = this.ColorOff;
			}

			if (this.IsActive == null && this.timer.IsEnabled)
				timer.Stop();
		}

		private static void OnFlashingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Led led = (Led)d;
			if (led.timer.IsEnabled)
			{
				led.timer.Stop();
				if (led.backgroundColor.Color == led.ColorNull)
					led.timer_Tick(null, new EventArgs());
			}
			else
				led.timer.Start();
		}

		private static void OnFlashingPeriodPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Led led = (Led)d;
			led.timer.Interval = TimeSpan.FromMilliseconds((int)e.NewValue);
		}

		private static void IsActivePropertyChanced(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Led led = (Led)d;
			if (led.IsActive == null)
				led.backgroundColor.Color = led.ColorNull;
			else if (led.IsActive == true)
				led.backgroundColor.Color = led.ColorOn;
			else
				led.backgroundColor.Color = led.ColorOff;

		}

		private static void OnColorOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Led led = (Led)d;
			led.ColorOn = (Color)e.NewValue;
			if (led.IsActive == true)
				led.backgroundColor.Color = led.ColorOn;
		}

		private static void OnColorOffPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Led led = (Led)d;
			led.ColorOff = (Color)e.NewValue;
			if (led.IsActive == false)
				led.backgroundColor.Color = led.ColorOff;
		}

		private static void OnColorNullPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Led led = (Led)d;
			led.ColorOff = (Color)e.NewValue;
			if (led.IsActive == null)
				led.backgroundColor.Color = led.ColorNull;
		}


		#endregion

		private void gridBigLed_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (this.IsActive == true)
				this.IsActive = false;
			else
				this.IsActive = true;
		}
	}
	class WidthToBorderThickConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			double height = (double)value;
			return height * 10 / 100; // border is 8% of height
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	public class BoolTo01Converter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			uint val = (uint)value;
			if ((val) == 0)
			{ return false; }
			else
			{ return true; }
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool bl = (bool)value;
			return bl == true ? 1 : 0;
		}
	}
}
