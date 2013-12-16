using System;

namespace NORM35.Extensions
{
    /// <summary>
    /// Расширения для TimeSpan
    /// </summary>
    public static class TimeSpanExtention
    {
        public static string ToStr(this TimeSpan timeSpan)
        {
            var result = timeSpan.ToString();

            string[] splitted = result.Substring(0, result.Length - 8).Replace('.', ':').Split(new[] {':'});
            if (splitted.Length < 3) return result;
            return string.Format("{0} д. {1} ч. {2} мин.", splitted[0], splitted[1], splitted[2]);
        }
    }
}