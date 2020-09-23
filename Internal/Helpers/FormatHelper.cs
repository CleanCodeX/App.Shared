using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Common.Shared.Attributes;
using Common.Shared.Extensions;
using Common.Shared.Extensions.Enumerables.Specialized;
using Res = Common.Shared.Properties.Resources;

// ReSharper disable UnusedVariable

namespace Common.Shared.Internal.Helpers
{
    internal static class FormatHelper
    {
        public static string FormatObject(object source, bool includeNames = true, Func<KeyValuePair<string, object>, object>? itemFunc = null, bool inheritProperties = false)
        {
            var items = new List<string?>();

            foreach (var propertyInfo in source.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | (inheritProperties ? 0 : BindingFlags.DeclaredOnly))
                .Where(x => !x.IsDefined(typeof(HideAttribute), false) &&
                            x.GetIndexParameters().Length == 0))
            {
                var memberType = propertyInfo.PropertyType;
                if (memberType.IsComplexType()) continue;

                var value = propertyInfo.GetValue(source)!;

                if (itemFunc is not null)
                    value = itemFunc(new KeyValuePair<string, object>(propertyInfo.Name, value));

                var isPassword = propertyInfo.IsDefined<PasswordPropertyTextAttribute>();
                if (isPassword)
                    value = "*".Repeat(10);

                var text = FormatValue(value, propertyInfo);

                if (includeNames)
                {
                    var name = propertyInfo.GetDisplayName();

                    text = $"{name}: {text}";
                }

                items.Add(text);
            }

            return EnumerableExtensions.Join(items, " | ");
        }

        public static string? FormatValue(this object? value, MemberInfo? memberInfo = null)
        {
            try
            {
                string? result;

                switch (value)
                {
                    case null:
                        result = null; 
                        break;
                    case string @string:
                        if (memberInfo?.IsDefined(typeof(StringLengthAttribute)) == true)
                        {
                            var maxLength = memberInfo.GetMaxLength();
                            if (maxLength < @string.Length)
                                @string = @string.TruncateAt(maxLength)!;
                        }

                        result = @string.IsNullOrEmpty() ? string.Empty : @string;
                        break;
                    case Uri uri:
                        result = Uri.UnescapeDataString(uri.OriginalString);
                        break;
                    case TimeSpan timespan:
                        result = timespan.Format();
                        break;
                    case DateTime dateTime:
                        result = memberInfo?.IsDefined(typeof(FormatAsTimespanSinceNowAttribute)) == true
                            ? dateTime.TimeUntilNowString()
                            : dateTime.Format();

                        break;
                    case bool @bool:
                        result = @bool.Format();
                        break;
                    case float @float:
                    case double @double:
                        result = Convert.ToDouble(value).Format();
                        break;
                    default:
                        var type = value.GetType();
                        if (type.IsClass || type.IsInterface) return string.Empty;

                        if (type.IsPrimitive)
                        {
                            var convertedValue = Convert.ToInt64(value);

                            var timeSpanUnit = memberInfo?.GetCustomAttribute<FormatAsTimespanAttribute>()?.Unit;
                            if (timeSpanUnit is not null)
                            {
                                var timeSpan = TimeSpan.FromMilliseconds(convertedValue * (int)timeSpanUnit.Value);
                                return timeSpan.Format();
                            }

                            result = convertedValue.Format();
                        }
                        else
                            result = value.ToString();

                        break;
                }

                if (memberInfo is null) return result;

                var prefix = memberInfo.GetCustomAttribute<MemberInfoNamePrefixAttribute>()?.NamePrefix;
                if (prefix is not null)
                    result = prefix + result;

                return result;
            }
            catch (Exception ex)
            {
                return $"{Res.Error}: {ex.Message}";
            }
        }
    }
}