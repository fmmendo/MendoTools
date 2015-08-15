using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Mendo.UAP.Converters
{
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isReverse = parameter != null;

            if (!isReverse)
            {
                return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return (value is bool && (bool)value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => !(value is bool && (bool)value);
    }
}
