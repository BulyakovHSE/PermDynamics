using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Navigation;

namespace WPFMVVMLib.Converters
{
    public class NumberToColorConverter : IValueConverter
    {
        private object _value;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _value = value;
            if (value is IComparable val)
            {
                int result = 0;
                if(val is Decimal)
                    result = val.CompareTo(0m);
                else if(val is double)
                    result = val.CompareTo(0d);
                else if (val is int)
                    result = val.CompareTo(0);
                if (result > 0) return Brushes.Green;
                if (result == 0) return Brushes.Black;
                if (result < 0) return Brushes.Red;
            }

            return Brushes.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _value;
        }
    }
}