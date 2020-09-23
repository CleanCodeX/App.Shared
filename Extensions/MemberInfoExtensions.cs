using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Common.Shared.Extensions
{
    public static class MemberInfoExtensions
    {
        public static Type? GetMetaDataType([NotNull] this MemberInfo source) => source.DeclaringType?.GetCustomAttribute<ModelMetadataTypeAttribute>()?.MetadataType;

        public static string? GetDisplayName([NotNull] this MemberInfo source, bool returnMemberNameIfNull = true)
        {
            var memberInfo = source;
            while (memberInfo is not null)
            {
                var displayName = memberInfo.TryGetAttributeFromMetaProvider<DisplayNameAttribute>(false)?.DisplayName;
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (displayName is null)
                {
                    // ReSharper disable once RedundantArgumentDefaultValue
                    displayName = memberInfo.TryGetAttributeFromMetaProvider<DisplayAttribute>(true)?.GetName();
                }

                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (displayName is null)
                    displayName = returnMemberNameIfNull ? source.Name : null;

                if (displayName is not null)
                    return displayName;

                var lastType = memberInfo.GetMetaDataType();
                if (lastType is null) break;

                memberInfo = lastType.GetMember(source.Name).FirstOrDefault();
            }

            return returnMemberNameIfNull ? source.Name : null;
        }

        public static TAttribute? TryGetAttributeFromMetaProvider<TAttribute>([NotNull] this MemberInfo source, bool inherit = true)
            where TAttribute : Attribute
        {
            var memberInfo = source;
            while (memberInfo is not null)
            {
                var attribute = memberInfo.GetCustomAttribute<TAttribute>(inherit);
                if (attribute is not null)
                    return attribute;

                var metaDataType = memberInfo.GetMetaDataType();
                if (metaDataType is null) break;

                memberInfo = metaDataType.GetMember(source.Name).FirstOrDefault();
            }

            return null;
        }

        public static bool TryGetIsAttributeDefinedFromMetaProvider<TAttribute>([NotNull] this MemberInfo source, bool inherit = true)
            where TAttribute : Attribute
        {
            var memberInfo = source;
            while (memberInfo is not null)
            {
                var isDefined = memberInfo.IsDefined<TAttribute>(inherit);
                if (isDefined)
                    return true;

                var metaDataType = memberInfo.GetMetaDataType();
                if (metaDataType is null) break;

                memberInfo = metaDataType.GetMember(source.Name).FirstOrDefault();
            }

            return false;
        }

        public static bool IsDefined<T>([NotNull] this MemberInfo source, bool inherit = true) where T : Attribute => Attribute.IsDefined(source, typeof(T), inherit);

        public static int GetMaxLength([NotNull] this MemberInfo source) => source.TryGetAttributeFromMetaProvider<StringLengthAttribute>()?.MaximumLength ?? 0;

        public static bool GetIsRequired([NotNull] this MemberInfo source) => source.TryGetIsAttributeDefinedFromMetaProvider<RequiredAttribute>();

        public static T GetCustomAttributeOrNew<T>([NotNull] this MemberInfo source) where T : Attribute, new() => (T?)source.GetCustomAttribute(typeof(T)) ?? new T();

        public static (T Min, T Max) GetRange<T>([NotNull] this MemberInfo source) where T : struct => ((T Min, T Max))source.GetRange();

        public static (object Min, object Max) GetRange([NotNull] this MemberInfo source)
        {
            var range = source.TryGetAttributeFromMetaProvider<RangeAttribute>();
            return range is null 
                ? (0, 0) 
                : (range.Minimum, range.Maximum);
        }

        public static Type GetUnderlyingType(this MemberInfo member) =>
            member.MemberType switch
            {
                MemberTypes.Event => ((EventInfo) member).EventHandlerType!,
                MemberTypes.Field => ((FieldInfo) member).FieldType,
                MemberTypes.Method => ((MethodInfo) member).ReturnType,
                MemberTypes.Property => ((PropertyInfo) member).PropertyType,
                _ => throw new ArgumentException(
                    "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo")
            };
    }
}
