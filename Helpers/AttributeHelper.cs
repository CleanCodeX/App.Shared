using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Common.Shared.Extensions;

namespace Common.Shared.Helpers
{
    public static class AttributeHelper
    {
        public static int GetMaxLength<T>(Expression<Func<T, string?>> propertyExpression) => GetPropertyAttributeValue<T, string?, StringLengthAttribute, int>(propertyExpression, attr => attr.MaximumLength);

        public static (int Minimum, int Maximum) GetRange<T, TProp>(Expression<Func<T, TProp>> propertyExpression) => GetPropertyAttributeValue<T, TProp, RangeAttribute, (int Minimum, int Maximum)>(propertyExpression, attr => ((int)attr.Minimum, (int)attr.Maximum));

        public static (int MinLength, int MaxLength) GetStringLength<T, TProp>(Expression<Func<T, TProp>> propertyExpression) => GetPropertyAttributeValue<T, TProp, StringLengthAttribute, (int MinLength, int MaxLength)>(propertyExpression, attr => (attr.MinimumLength, attr.MaximumLength));

        public static string GetEnumMember<T>(Expression<Func<T, string>> propertyExpression) => GetPropertyAttributeValue<T, string, EnumMemberAttribute, string>(propertyExpression, attr => attr.Value!);

        public static TValue GetPropertyAttributeValue<T, TOut, TAttribute, TValue>(Expression<Func<T, TOut>> propertyExpression, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var expression = (MemberExpression)propertyExpression.Body;
            var propertyInfo = (PropertyInfo)expression.Member;

            var attribute = propertyInfo.TryGetAttributeFromMetaProvider<TAttribute>();
            if (attribute is null)
                throw new MissingMemberException(typeof(T).Name + "." + propertyInfo.Name, typeof(TAttribute).Name);

            return valueSelector(attribute);
        }
    }
}
