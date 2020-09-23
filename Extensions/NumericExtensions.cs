using System;
using System.Globalization;
using System.Linq;
using Common.Shared.Enums;

namespace Common.Shared.Extensions
{
    public static class NumericExtensions
    {
        private const string NumberDefaultString = "0";
        private const string NumberFormatWithoutFractions = "{0:N0}";
        private const string NumberFormatWithFractions = "{{0:N{0}}}";

        public static string ToHex(this int value) => ((long)value).ToHex();
        public static string ToHex(this long value) => $"0x{value:X}";

        public static int FromHex(string value)
        {
            // strip the leading 0x
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                value = value[2..];

            return int.Parse(value, NumberStyles.HexNumber);
        }

        public static int IfDefaultUse(this int number, params int[] values) => (number > 0 ? number : values.FirstOrDefault(value => value > 0))!;

        public static string FormatTime(this int number, TimeUnit unit = TimeUnit.Seconds)
        {
            double value = number;

            value *= (int)unit;

            return TimeSpan.FromMilliseconds(value).Format();
        }

        public static string Format(this double number, int decimalPlaces, string defaultValue = NumberDefaultString) => number > 0 ? NumberFormatWithFractions.InsertArgs(decimalPlaces).InsertArgs(CultureInfo.CurrentUICulture, number) : defaultValue;
        public static string Format(this int? number, string defaultValue = NumberDefaultString) => number.HasValue ? number.Value.Format() : defaultValue;
        public static string Format(this long number, string defaultValue = NumberDefaultString) => number > 0 ? NumberFormatWithoutFractions.InsertArgs(CultureInfo.CurrentUICulture, number) : defaultValue;
        public static string Format(this int number, string defaultValue = NumberDefaultString) => Convert.ToInt64(number).Format(defaultValue);
        public static string Format<T>(this T number, string defaultValue = NumberDefaultString)
            where T : struct =>
            Convert.ToInt64(number).Format(defaultValue);

        public static string FormatRounded(this double number, int decimalPlaces = 2) => NumberFormatWithFractions.InsertArgs(decimalPlaces).InsertArgs(CultureInfo.CurrentUICulture, Round(number, decimalPlaces));
        public static double Round(this double number, int decimalPlaces = 2) => Math.Round(number, decimalPlaces);

        public static string FormatBinary<T>(this T number) where T : struct, IComparable<T>, IComparable => Convert.ToDouble(number).FormatBinary();
        public static string FormatBinary(this double number)
        {
            var unitSuffix = " B";

            if (number > 1024 * 1024)
            {
                number = Math.Round(number / 1024 / 1024, 2);
                unitSuffix = " MiB";
            }
            else if (number > 1024)
            {
                number = Math.Round(number / 1024, 2);
                unitSuffix = " KiB";
            }

            return number.Format() + unitSuffix;
        }
    }
}
