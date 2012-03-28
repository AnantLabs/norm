namespace NORM.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static string HtmlTrim(this string s)
        {
            return s.Replace('\u00A0', ' ').Trim();
        }
    }
}
