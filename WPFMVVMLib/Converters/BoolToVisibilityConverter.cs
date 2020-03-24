using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WPFMVVMLib.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            bool val = (bool) value;
            if (val) return Visibility.Visible;
            else return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            Visibility visibility = (Visibility) value;
            switch (visibility)
            {
                case Visibility.Collapsed: return false;
                case Visibility.Visible: return true;
            }
            return false;
        }
    }
}
