using System;
using System.Globalization;

namespace Mendo.UWP.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime SafeType(this DateTime? dt)
        {
            if (dt.HasValue)
            {
                return dt.Value;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime SafeType(this DateTime dt)
        {
            return dt;
        }

        public static string ToString(this DateTime? dt, DateTimeFormat format)
        {
            if (dt.HasValue)
            {
                return dt.Value.ToString(format);
            }
            else
            {
                return string.Empty;
            }
        }

        public static string ToString(this DateTime dt, DateTimeFormat format)
        {
            if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
            {
                return string.Empty;
            }
            switch (format)
            {
                case DateTimeFormat.ShortDate:
                    return dt.ToString("g");
                case DateTimeFormat.LongDate:
                    return dt.ToString("D");
                case DateTimeFormat.Date:
                    return dt.ToString("d");
                case DateTimeFormat.Time:
                    return dt.ToString("t");
                case DateTimeFormat.DayOfWeek:
                    return dt.GetLocalizedDayOfWeek();
                case DateTimeFormat.CardDate:
                    return dt.ToString("dd MMM");
                case DateTimeFormat.CardTime:
                    return dt.ToString("HH mm");
                default:
                    return string.Empty;
            }
        }

        public static string GetLocalizedDayOfWeek(this DateTime dt)
        {
            return CultureInfo.CurrentUICulture.DateTimeFormat.DayNames[(int)dt.DayOfWeek];
        }

        public static DateTime FromNow(this TimeSpan value)
        {
            return new DateTime((DateTime.Now + value).Ticks);
        }

        public static DateTime FromUnixTime(this long seconds)
        {
            var time = new DateTime(1970, 1, 1);
            time = time.AddSeconds(seconds);

            return time.ToLocalTime();
        }

        public static long ToUnixTime(this DateTime dateTime)
        {
            var timeSpan = (dateTime - new DateTime(1970, 1, 1));
            var timestamp = (long)timeSpan.TotalSeconds;

            return timestamp;
        }
    }

    public enum DateTimeFormat
    {
        ShortDate,
        LongDate,
        Date,
        Time,
        DayOfWeek,
        CardDate,
        CardTime
    }
}
