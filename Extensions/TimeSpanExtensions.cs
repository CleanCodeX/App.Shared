using System;
using System.Globalization;

namespace Common.Shared.Extensions
{
    public enum TimeSpanFormatOption
    {
        FirstTwoUnits,
        ExcludeMilliseconds,
        ExcludeSeconds,
        IncludeMilliseconds
    }

    public static class TimeSpanExtensions
    {
        public static string Format(this TimeSpan? timespan) => timespan is null ? string.Empty : Format(timespan.Value);

        public static string Format(this TimeSpan timespan) => Format(timespan, TimeSpanFormatOption.ExcludeSeconds);
        public static string Format(this TimeSpan timespan, bool showFractions) =>
            Format(timespan, showFractions ? TimeSpanFormatOption.IncludeMilliseconds : TimeSpanFormatOption.ExcludeMilliseconds);
        public static string Format(this TimeSpan timespan, TimeSpanFormatOption formatOption)
        {
            if (formatOption == TimeSpanFormatOption.ExcludeMilliseconds)
                return timespan.ToString("g", CultureInfo.CurrentUICulture).SubstringBeforeLast(CultureInfo.CurrentUICulture.NumberFormat.CurrencyDecimalSeparator);

            try
            {
                string format;
                int firstValue, secondValue = 0;

                if (timespan.Days > 0)
                {
                    format = "{0:d}T";
                    firstValue = timespan.Days;
                    secondValue = Math.Abs(timespan.Hours);
                    if (secondValue > 0)
                        format += ":{1:d}h";
                }
                else if (timespan.Hours > 0)
                {
                    format = "{0:d}h";
                    firstValue = timespan.Hours;
                    secondValue = Math.Abs(timespan.Minutes);
                    if (secondValue > 0)
                        format += ":{1:d}m";
                }
                else if (timespan.Minutes > 0)
                {
                    format = "{0:d}m";
                    firstValue = timespan.Minutes;
                    secondValue = Math.Abs(timespan.Seconds);
                    if (secondValue > 0 && formatOption != TimeSpanFormatOption.ExcludeSeconds)
                        format += ":{1:d}s";
                }
                else if (timespan.Seconds > 0)
                {
                    format = "{0:d}s";
                    firstValue = timespan.Seconds;
                    secondValue = timespan.Milliseconds;
                    if (secondValue > 0 && formatOption == TimeSpanFormatOption.IncludeMilliseconds)
                        format += ":{1:d}ms";
                }
                else if (formatOption == TimeSpanFormatOption.IncludeMilliseconds)
                {
                    format = "{0:d}ms";
                    firstValue = timespan.Milliseconds;
                }
                else
                    return string.Empty;

                if (secondValue == 0)
                    return format.InsertArgs(firstValue);

                return format.InsertArgs(firstValue, secondValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
