using System;
using System.Diagnostics.CodeAnalysis;
using Common.Shared.Helpers;
using KellermanSoftware.CompareNetObjects;

// ReSharper disable UnusedParameter.Global

namespace Common.Shared.Extensions
{
    public static class ObjectExtensions
    {
        #region Casting, Conversion

        [return: NotNull]
        public static Enum ToEnum<TEnum>([NotNull]
            this object enumeration) where TEnum : struct, Enum => enumeration.ToEnum(typeof(TEnum));

        public static object? ParseEnum(this object? value, Type enumType,
            bool ignoreCase = true) =>
            value is not string str ? null : str.ParseEnum(enumType, ignoreCase);

        [return: NotNull]
        public static Enum ToEnum([NotNull] this object @enum, Type enumType)
        {
            @enum.ThrowIfNull(nameof(@enum));
            var type = @enum!.GetType();
            type.IsEnum.ThrowIfFalse(nameof(type.IsEnum));

            return (Enum)Enum.Parse(enumType, @enum.ToString()!);
        }

        #endregion Casting, Conversion

        public static bool DeepCompareAreEqual(this object sourceItem, object otherItem)
            => CompareHelper.Compare(sourceItem, otherItem).AreEqual;

        public static ComparisonResult DeepCompareResult(this object sourceItem, object otherItem)
            => CompareHelper.Compare(sourceItem, otherItem);
    }
}
