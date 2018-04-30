using PsISEProjectExplorer.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PsISEProjectExplorer.UI.Helpers
{
    [ValueConversion(typeof(IndexingMode), typeof(string))]
    public class IndexingModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IndexingMode indexingMode = (IndexingMode)value;
            switch (indexingMode)
            {
                case IndexingMode.ALL_FILES: return true;
                case IndexingMode.NO_FILES: return false;
                default: return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? nullableValue = value as bool?;
            if (!nullableValue.HasValue)
            {
                return IndexingMode.LOCAL_FILES;
            }
            return nullableValue.Value ? IndexingMode.ALL_FILES : IndexingMode.NO_FILES;
        }
    }
}
