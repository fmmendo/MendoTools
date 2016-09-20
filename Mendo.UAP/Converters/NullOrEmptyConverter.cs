using Mendo.UAP.Extensions;
using System;
using System.Collections;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Mendo.UAP.Converters
{
    public enum NullOrEmptyConverterMode
    {
        Boolean,
        Visibility
    }

    public class NullOrEmptyConverter : IValueConverter
    {
        /// <summary>
        /// Default mode is boolean
        /// </summary>
        public NullOrEmptyConverterMode Mode { get; set; }

        /// <summary>
        /// Sets the visibility of an element to collapsed if the input value is null or "empty"
        /// If a parameter is passed, the returned visibility is inverted
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool result = true;

            // If null, set to collapsed
            if (value == null)
            {
                result = false;
            }
            else if (value is int)
            {
                int i = (int)value;
                result = i > 0;
            }
            else if (value is string)
            {
                var s = (string)value;
                if (string.IsNullOrWhiteSpace(s))
                    result = false;
            }
            else if (value is IList)
            {
                var l = (IList)value;
                if (l.Count == 0)
                    result = false;
            }
            else if (value is ICollection)
            {
                var c = (ICollection)value;
                if (c.Count == 0)
                    result = false;
            }
            else if (value is IEnumerable)
            {
                var i = (IEnumerable)value;
                var obj = i.Cast<object>();

                if (!obj.Any())
                    result = false;
            }
            else
            {
                if (value.IsNullOrDefault())
                    result = false;
            }

            // Reverse if a parameter was passed through
            if (parameter != null)
            {
                result = !result;
            }

            if (Mode == NullOrEmptyConverterMode.Boolean)
                return result;
            else
            {
                return (result) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
