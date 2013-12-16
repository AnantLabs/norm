using System.Collections.Generic;
using System.Linq;

namespace NORM35.Extensions
{
    public static class IEnumerableExtension
    {
        public static string ToDatabaseString(this int[] arr)
        {
            string result;
            if (arr != null && arr.Length > 0)
            {
                result = arr.
                    Select(x => x + ",").
                    Aggregate((x, y) => x + y).
                    TrimEnd(',');
            }
            else
            {
                result = "0";
            }

            return result;
        }

        public static List<T> ToListNotNull<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return new List<T>();
            }

            return enumerable.ToList() ?? new List<T>();
        }
    }
}
