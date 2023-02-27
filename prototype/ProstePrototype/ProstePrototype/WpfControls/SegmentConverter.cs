using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace ProstePrototype.WpfControls
{
    public class SegmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string address && parameter is string segmentParam && int.TryParse(segmentParam, out int segment))
            {
                var segments = address.Split('.');
                if (segment > 0 && segment <= segments.Length)
                {
                    return segments[segment - 1];
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
