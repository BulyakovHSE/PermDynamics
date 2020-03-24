﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace WPFMVVMLib.Converters
{
    public class ObjectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? new object() : null;
            }

            return null;
        }
    }
}