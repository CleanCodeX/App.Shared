using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using Common.Shared.Extensions.Enumerables.Specialized;

namespace Common.Shared.Extensions
{
    public static class EnumExtensions
    {
        public static int ToInt<T>(this T @enum) where T : struct, Enum => (int)(object)@enum;

        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example>string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;</DescriptionAttribute>></example>
        public static T? GetAttribute<T>(this Enum? enumVal) where T : Attribute
        {
            if (enumVal is null) return default;

            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            if (memInfo.Length == 0) return null;

            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);

            return attributes.Length > 0 ? (T)attributes[0] : null;
        }

        public static void ProcessBatch<T>(this IReadOnlyCollection<T> list, int batchSize,
            Action<IReadOnlyCollection<T>> funcToCall)
        {
            var startOffset = 0;
            while (startOffset < list.Count)
            {
                var batch = list.Skip(startOffset).Take(batchSize).ToArray();

                funcToCall(batch);

                startOffset += batchSize;
            }
        }

        public static string? GetEnumMember<TEnum>(this object? enumeration, bool returnDefault = true) where TEnum : struct, Enum => enumeration?.ToEnum<TEnum>().GetEnumMember(returnDefault);
        public static string? GetEnumMember(this object? enumeration, Type enumType, bool returnDefault = true) => enumeration?.ToEnum(enumType).GetEnumMember(returnDefault);
        public static string? GetEnumMember(this Enum? enumeration, bool returnDefault = true) => enumeration is null ? null : enumeration.GetAttribute<EnumMemberAttribute>()?.Value ?? (returnDefault ? enumeration.ToString() : string.Empty);

        public static string? GetDisplayName<TEnum>(this object? enumeration, bool returnDefault = true) where TEnum : struct, Enum => enumeration?.ToEnum<TEnum>().GetEnumMember(returnDefault);
        public static string? GetDisplayName(this object? enumeration, Type enumType, bool returnDefault = true) => enumeration?.ToEnum(enumType).GetDisplayName(returnDefault);
        public static string? GetDisplayName(this Enum? enumeration, bool returnDefault = true) => enumeration is null ? null :
            enumeration.GetAttribute<DisplayNameAttribute>()?.DisplayName ?? enumeration.GetAttribute<DisplayAttribute>()?.Name ?? (returnDefault ? enumeration.ToString() : string.Empty);

        public static IEnumerable<TEnum> GetSetFlags<TEnum>(this TEnum source) where TEnum : struct, Enum => source.GetFlags().Where(e => source.HasFlag(e));
        public static IEnumerable<Enum> GetSetFlags(this Enum source) => source.GetFlags().Where(source.HasFlag);

        public static TEnum GetHighestFlagSet<TEnum>(this TEnum source) where TEnum : struct, Enum
        {
            var flags = source.GetFlags().Where(x => source.HasFlag(x));

            return flags.IsEmpty() ? default : flags.Max()!;
        }

        public static TEnum GetLowestFlagSet<TEnum>(this TEnum source) where TEnum : struct, Enum
        {
            var flags = source.GetFlags().Where(x => source.HasFlag(x));

            return flags.IsEmpty() ? default : flags.Min()!;
        }

        // ReSharper disable once UnusedParameter.Global
        [SuppressMessage("Stil", "IDE0060:Nicht verwendete Parameter entfernen")]
        public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum source) where TEnum : struct, Enum
        {
            var flag = 0x1UL;
            foreach (var value in Enum.GetValues(typeof(TEnum)))
            {
                var bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    continue;

                while (flag < bits)
                    flag <<= 1;

                if (flag == bits)
                    yield return (TEnum)value;
            }
        }

        [SuppressMessage("Stil", "IDE0060:Nicht verwendete Parameter entfernen")]
        public static IEnumerable<Enum> GetFlags(this Enum source)
        {
            var flag = 0x1UL;
            foreach (var value in Enum.GetValues(source.GetType()))
            {
                var bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    continue;

                while (flag < bits)
                    flag <<= 1;

                if (flag == bits)
                    yield return (Enum)value;
            }
        }

        /// <summary>Enumerates get flags in this collection.</summary>
        /// <param name="source">The value.</param>
        /// <returns>An enumerator that allows foreach to be used to process get flags in this collection.</returns>
        // ReSharper disable once UnusedParameter.Global
        [SuppressMessage("Stil", "IDE0060:Nicht verwendete Parameter entfernen")]
        public static IEnumerable<TEnum> GetValues<TEnum>(this TEnum source) where TEnum : struct, Enum => Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
    }
}
