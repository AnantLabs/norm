using System;
using System.Globalization;

namespace NORM35.Extensions
{
    /// <summary>
    /// Расширения для конвертации данных
    /// </summary>
    public static class ObjectParseExtention
    {
        public static T To<T>(this object obj)
        {
            return obj.To(default(T));
        }

        public static T To<T>(this object obj, T defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }
            if (obj is T)
            {
                return (T) obj;
            }
            Type type = typeof (T);
            if (type == typeof (string))
            {
                return (T) obj;
            }
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return To(obj, defaultValue, underlyingType);
            }
            return To(obj, defaultValue, type);
        }

        public static object To(this object obj, Type type)
        {
            if (obj == null)
            {
                return new object();
            }
            if (obj.GetType() == type)
            {
                return obj;
            }
            if (type == typeof(string))
            {
                return obj;
            }
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return To(obj, new object(), underlyingType);
            }
            return To(obj, new object(), type);
        }

        private static T To<T>(object obj, T defaultValue, Type type)
        {
            if (type.IsEnum)
            {
                if (obj is decimal)
                {
                    return (T) Enum.Parse(type, obj.ToString());
                }
                if (obj is string)
                {
                    return (T) Enum.Parse(type, (string) obj);
                }
                if (obj is long)
                {
                    return (T) Enum.Parse(type, obj.ToString());
                }
                if (Enum.IsDefined(type, obj))
                {
                    return (T) Enum.Parse(type, obj.ToString());
                }
                return defaultValue;
            }
            try
            {
                return (T) Convert.ChangeType(obj, type);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool ToBool(this object obj)
        {
            bool boolValue;
            int intValue;
            if (obj == null)
            {
                return false;
            }
            if (obj is bool)
            {
                return (bool) obj;
            }
            if (bool.TryParse(obj.ToString(), out boolValue))
            {
                return boolValue;
            }
            return (int.TryParse(obj.ToString(), out intValue) && (intValue != 0));
        }

        public static decimal ToDecimal(this object obj)
        {
            if (obj != null)
            {
                decimal decValue;
                if (obj is decimal)
                {
                    return (decimal) obj;
                }
                if (
                    decimal.TryParse(
                        obj.ToString().Replace(CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator,
                                               CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator),
                        out decValue))
                {
                    return decValue;
                }
            }
            return 0M;
        }

        public static float ToSingle(this object obj)
        {
            if (obj != null)
            {
                float sinValue;
                if (obj is float)
                {
                    return (float)obj;
                }
                if (
                    float.TryParse(
                        obj.ToString().Replace(CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator,
                                               CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator),
                        out sinValue))
                {
                    return sinValue;
                }
            }
            return 0f;
        }

        public static double ToDouble(this object obj)
        {
            if (obj != null)
            {
                double dbValue;
                if (obj is double)
                {
                    return (double) obj;
                }
                if (
                    double.TryParse(
                        obj.ToString().Replace(CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator,
                                               CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator),
                        out dbValue))
                {
                    return dbValue;
                }
            }
            return 0.0;
        }

        public static int ToInt(this object obj)
        {
            if ((obj != null) && (obj != DBNull.Value))
            {
                int intValue;
                if (obj is int)
                {
                    return (int) obj;
                }
                if (int.TryParse(obj.ToString(), out intValue))
                {
                    return intValue;
                }
            }
            return 0;
        }

        public static string ToStr(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            if (obj is string)
            {
                return (string) obj;
            }
            return obj.ToString();
        }
    }
}