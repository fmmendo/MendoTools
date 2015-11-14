using System;
using Windows.UI.Xaml.Data;

namespace Mendo.UAP.Converters
{
    public class BooleanReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
                return !(bool)value;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => !(bool)value;
    }
}
