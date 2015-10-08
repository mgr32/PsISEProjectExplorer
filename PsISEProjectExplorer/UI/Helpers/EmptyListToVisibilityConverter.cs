using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace PsISEProjectExplorer.UI.Helpers
{
    [ValueConversion(typeof(IEnumerable<Object>), typeof(Visibility))]
    public class EmptyListToVisibilityConverter : IValueConverter
    {
        enum Parameters
        {
            VisibleOnEmpty, HiddenOnEmpty
        }

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var collection = (IEnumerable<Object>)value;
            Parameters direction = Parameters.HiddenOnEmpty;
            if (parameter != null)
            {
                direction = (Parameters)Enum.Parse(typeof(Parameters), (string)parameter);
            }
            if (direction == Parameters.VisibleOnEmpty)
            {
                return collection.Any() ? Visibility.Collapsed : Visibility.Visible;
            }
            return collection.Any() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
