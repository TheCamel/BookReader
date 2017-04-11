using System;
using System.Windows.Data;
using System.Windows.Controls;
using System.Drawing;

namespace BookReader
{
	[ValueConversion(typeof(string), typeof(System.Windows.Visibility))]
	public class BookmarkToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string val = value as string;
			if (!string.IsNullOrEmpty(val))
			{
				return System.Windows.Visibility.Visible;
			}
			else
			{
				return System.Windows.Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return string.Empty;
		}
	}

    [ValueConversion(typeof(bool), typeof(System.Windows.Media.SolidColorBrush))]
    public class ReadToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool val = (bool)value;
            if ( val )
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255,0,0));
            }
            else
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255,255,255));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
}
