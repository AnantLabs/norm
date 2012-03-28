using System;
using System.Linq.Expressions;

namespace NORM35.Reflection
{
    public class ExpressionReflection
    {
        public static string GetPropertyName<T, TType>(Expression<Func<T, TType>> propertyExpression)
        {
            var body = (MemberExpression)propertyExpression.Body;
            return body.Member.Name;
        }
    }
}
