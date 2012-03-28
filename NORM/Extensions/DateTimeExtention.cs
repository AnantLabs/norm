using System;

namespace NORM.Extensions
{
    /// <summary>
    /// Расширения для DateTime
    /// </summary>
    public static class DateTimeExtention
    {
        public static DateTime EndOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 12, 31, 0, 0, 0);
        }

        public static DateTime EndOfMounth(this DateTime date)
        {
            return date.AddMonths(1).StartOfMounth().AddDays(-1).EndOfDay();
        }

        public static DateTime EndOfWeek(this DateTime date)
        {
            var delta = 7 - (int)date.DayOfWeek;
            return date.AddDays(delta).EndOfDay();
        }

        public static DateTime EndOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
        }

        public static DateTime StartOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 1, 1, 0, 0, 0);
        }

        public static DateTime StartOfMounth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, 0, 0, 0);
        }

        public static DateTime StartOfWeek(this DateTime date)
        {
            var delta = 1 - (int)date.DayOfWeek;
            return date.AddDays(delta).StartOfDay();
        }

        public static DateTime StartOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        }

        public static string ToDatabaseString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public static string ToShortDateString(this DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToShortDateString() : string.Empty;
        }

        public static string ToString(this DateTime? dateTime, string format)
        {
            return dateTime.HasValue ? dateTime.Value.ToString(format) : string.Empty;
        }

        public static string ToFilenameString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd_HH.mm.ss.fff");
        }
    }
}