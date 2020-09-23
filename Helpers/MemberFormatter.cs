using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Common.Shared.Attributes;
using Common.Shared.Extensions;
using Common.Shared.Extensions.Enumerables;
using Common.Shared.Internal.Helpers;
using Res = Common.Shared.Properties.Resources;

namespace Common.Shared.Helpers
{
    public static class MemberFormatter
    {
        public static string GetMembersString<T>([NotNull] this T source, bool includePrivateAndProtected = false, BindingFlags bindingFlags = default) where T : class => GetMembersStringInternal(source.GetType(), FormatMembers(source, includePrivateAndProtected, bindingFlags));

        public static string GetMembersString([NotNull] this Type source, bool includePrivateAndProtected = false, BindingFlags bindingFlags = default) => GetMembersStringInternal(source, FormatMembers(source, includePrivateAndProtected, bindingFlags));

        private static string GetMembersStringInternal(Type type, Dictionary<string, string> settings)
        {
            var sb = new StringBuilder();
            sb.Append($"{type.GetDisplayName()}:{Environment.NewLine}");
            var sortedSettings = settings.OrderBy(x => x.Key).Select(x => x.Value);
            foreach (var sortedSetting in sortedSettings)
                sb.AppendLine(sortedSetting);

            return sb.ToString();
        }

        public static Dictionary<string, string> FormatMembers([NotNull] Type type, bool includePrivateAndProtected = false, BindingFlags bindingFlags = default) => FormatMembersInternal(type, null, includePrivateAndProtected, bindingFlags);

        public static Dictionary<string, string> FormatMembers<T>([NotNull] T instance,
            bool includePrivateAndProtected = false, BindingFlags bindingFlags = default)
            where T : class =>
            FormatMembersInternal(instance.GetType(), instance, includePrivateAndProtected, bindingFlags);

        private static Dictionary<string, string> FormatMembersInternal(Type type, object? instance, bool includePrivateAndProtected = false, BindingFlags bindingFlags = default)
        {
            if (bindingFlags == default)
            {
                bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;

                if (instance is not null)
                    bindingFlags |= BindingFlags.Instance | BindingFlags.DeclaredOnly;
                else
                    bindingFlags |= BindingFlags.Static;
            }

            var settings = new Dictionary<string, string>();

            #region Properties 

            var propertyInfos = type
                .GetProperties(bindingFlags)
                .Where(x =>
                            x.GetMethod is not null &&
                            x.GetIndexParameters().Length == 0 &&
                            !x.IsDefined<HideAttribute>() &&
                            (includePrivateAndProtected || !x.GetMethod.IsPrivate && !x.GetMethod.IsFamily)
                )
                .OrderBy(x => x.Name);

            foreach (var memberInfo in propertyInfos)
                AddMember(memberInfo, memberInfo.PropertyType, memberInfo.GetValue(instance)!);

            #endregion

            #region Fields

            var fieldInfos = type
                .GetFields(bindingFlags)
                .Where(x => !x.IsDefined<HideAttribute>() &&
                            (includePrivateAndProtected || !x.IsPrivate && !x.IsFamily) && !x.IsDefined(typeof(CompilerGeneratedAttribute)))
                .OrderBy(x => x.Name);

            foreach (var memberInfo in fieldInfos)
                AddMember(memberInfo, memberInfo.FieldType, memberInfo.GetValue(instance)!);

            #endregion

            return settings;

            string AddComplexType(object value, MemberInfo propType, string name, string? unit)
            {
                var sb2 = new StringBuilder();
                var sp = " ".Repeat(2);

                if (value is IDictionary dict)
                    AddDictionary();
                else if (value is IEnumerable list)
                    AddList(list);
                else if (propType.IsDefined<ProvidesSelfFormattingAttribute>())
                    AddSelfFormattedObject();
                else
                    AddAutoFormattedObject();

                sb2.AppendLine().AppendLine();

                return sb2.ToString();

                void AddDictionary()
                {
                    sb2.AppendLine($"{name}: ({Res.Dictionary})");
                    if (dict.IsNotEmpty())
                    {
                        sb2.AppendLine("{");
                        foreach (DictionaryEntry listEntry in dict)
                            sb2.AppendLine($"{sp}\u2023 {listEntry.Key}: {(listEntry.Value.FormatValue() + unit)}");
                        sb2.AppendLine("}");
                    }
                    else
                        sb2.AppendLine($"{{ {Res.None} }}");
                }

                void AddList(IEnumerable list)
                {
                    sb2.AppendLine($"{name}: ({Res.List})");
                    if (list.IsNotEmpty())
                    {
                        sb2.AppendLine("[");
                        foreach (var listEntry in list)
                            sb2.AppendLine($"{sp}\u2023 {listEntry.FormatValue() + unit}");
                        sb2.AppendLine("]");
                    }
                    else
                        sb2.AppendLine($"[ {Res.None} ]");
                }

                void AddSelfFormattedObject()
                {
                    sb2.AppendLine($"{name}: {{");
                    sb2.AppendLine($"{sp}\u2023 {value}}}");
                }

                void AddAutoFormattedObject()
                {
                    sb2.AppendLine($"{name}:{Environment.NewLine}{{");
                    sb2.AppendLine(
                        $"{sp}\u2023 {FormatHelper.FormatObject(value, inheritProperties: true, itemFunc: item => item.Value) + Environment.NewLine}}}");
                }
            }

            void AddMember(MemberInfo memberInfo, Type propType, object value)
            {
                var displayName = memberInfo.GetDisplayName(false);
                var name = displayName is not null ? $"{memberInfo.Name} {displayName.Quote()}" : memberInfo.Name;
                var unit = memberInfo.GetCustomAttribute<DisplayUnitAttribute>()?.DisplayUnit;

                string valueString;
                if (propType.IsComplexType())
                    valueString = AddComplexType(value, propType, name, unit);
                else
                {
                    var isPassword = memberInfo.IsDefined<PasswordPropertyTextAttribute>();
                    if (isPassword)
                        value = "*".Repeat(10);

                    valueString = $"{name}: {(value.FormatValue(memberInfo) + unit)}";
                }

                settings.Add(name, valueString);
            }
        }
    }
}