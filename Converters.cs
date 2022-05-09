using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CardFilePBX
{
	public class DbStateToBoolean : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (((ConnectionState)value) != ConnectionState.Broken)
				return true;
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (Boolean)value ? ConnectionState.Closed : ConnectionState.Broken;
		}

	}
	public class TariffConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var val = value as Tariff;
			if (val.Name == "МегаТариф") return 0;
			if (val.Name == "Максимум") return 1;
			if (val.Name == "VIP") return 2;
			if (val.Name == "Премиум") return 3;
			return 0;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Console.WriteLine(targetType);
			return value;
		}
	}
	public class InverseBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof(bool))
				return false;
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}
	}
	public class CountToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Console.WriteLine((int)value);
			if (targetType != typeof(bool))
				return false;
			return (int)value > 0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !(bool)value;
		}
	}
	public static class TariffToInt
	{
		public static object Convert(object value)
		{
			string val;
			if (value is Tariff)
			{
				val = (value as Tariff).Name;
			}
			else val = value as string;
			if (val == "МегаТариф") return 0;
			if (val == "Максимум") return 1;
			if (val == "VIP") return 2;
			if (val == "Премиум") return 3;
			if (val == "0") return 0;
			if (val == "1") return 1;
			if (val == "2") return 2;
			if (val == "3") return 3;
			return 0;
		}
		public static object ConvertBack(object value)
		{
			return value;
		}
	}
	public class BooleanToVisibility : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool)value)
			{
				if (parameter != null)
				{
					return (string)parameter == "1" ? Visibility.Collapsed : Visibility.Visible;
				}
				return Visibility.Visible;
			}
			if (parameter != null)
			{
				return (string)parameter == "1" ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((Visibility)value == Visibility.Visible)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

	}
}
