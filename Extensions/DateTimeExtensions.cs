using System;
using System.Globalization;
using Common.Shared.Enums;
using Common.Shared.Helpers;

namespace Common.Shared.Extensions
{
    public static class DateTimeExtensions
    {
        public static string Format(this DateTime? dateTime, bool removeTime) =>
            dateTime is null ? string.Empty : dateTime.Value.Format(CultureInfo.CurrentUICulture, removeTime ? TimeFormatOption.ExcludeTime : TimeFormatOption.IncludeTimeIfNonZero);

        public static string Format(this DateTime? dateTime, TimeFormatOption option = TimeFormatOption.IncludeTimeIfNonZero) =>
            dateTime is null ? string.Empty : dateTime.Value.Format(CultureInfo.CurrentUICulture, option);

        public static string Format(this DateTime dateTime, bool removeTime) => 
            dateTime == default ? string.Empty : dateTime.Format(CultureInfo.CurrentUICulture, removeTime ? TimeFormatOption.ExcludeTime : TimeFormatOption.IncludeTimeIfNonZero);

        public static string Format(this DateTime dateTime, TimeFormatOption option = TimeFormatOption.IncludeTimeIfNonZero) =>
            dateTime.Format(CultureInfo.CurrentUICulture, option);

        public static string Format(this DateTimeOffset? dateTime, bool removeTime) => 
            dateTime is null ? string.Empty : dateTime.Value.Format(CultureInfo.CurrentUICulture, removeTime ? TimeFormatOption.ExcludeTime : TimeFormatOption.IncludeTimeIfNonZero);

        public static string Format(this DateTimeOffset? dateTime, TimeFormatOption option = TimeFormatOption.IncludeTimeIfNonZero) =>
            dateTime is null ? string.Empty : dateTime.Value.Format(CultureInfo.CurrentUICulture, option);

        public static string Format(this DateTimeOffset dateTime, bool removeTime) => 
            dateTime.Format(CultureInfo.CurrentUICulture, removeTime ? TimeFormatOption.ExcludeTime : TimeFormatOption.IncludeTimeIfNonZero);

        public static string Format(this DateTimeOffset dateTime, TimeFormatOption option = TimeFormatOption.IncludeTimeIfNonZero) =>
            dateTime.Format(CultureInfo.CurrentUICulture, option);

        public static string Format(this DateTimeOffset dateTime, CultureInfo cultureInfo, TimeFormatOption option = TimeFormatOption.IncludeTimeIfNonZero)
            => dateTime.ToLocalTime().ToDateTime().Format(cultureInfo, option);

        public static string Format(this DateTime dateTime, CultureInfo cultureInfo, TimeFormatOption option = TimeFormatOption.IncludeTimeIfNonZero)
        {
            if (dateTime == default)
                return string.Empty;

            var hasTimePart = dateTime.Second + dateTime.Minute + dateTime.Hour > 0;
            
            var dateOnly = dateTime.ToString("d", cultureInfo);
            var dateTimeTemplate = $"{dateOnly} {{0}}";

            return option switch
            {
                TimeFormatOption.ExcludeTime => dateOnly,
                TimeFormatOption.IncludeTimeIfNonZero => hasTimePart
                    ? GetDateWithTime("HH:mm")
                    : dateOnly,
                TimeFormatOption.ExcludeSeconds => GetDateWithTime("HH:mm"),
                TimeFormatOption.IncludeTime => GetDateWithTime("HH:mm:ss"),
                TimeFormatOption.IncludeMilliseconds => GetDateWithTime("HH:mm:ss.fff"),
                _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
            };

            string GetDateWithTime(string format) => dateTimeTemplate.InsertArgs(dateTime.ToString(format, cultureInfo));
        }

        public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime) => DateTime.SpecifyKind(dateTime, DateTimeKind.Local);

        public static DateTime ToDateTime(this DateTimeOffset dateTime)
        {
            if (dateTime.Offset == TimeSpan.Zero)
                return dateTime.UtcDateTime;
            
            return dateTime.Offset == TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime) 
                ? DateTime.SpecifyKind(dateTime.DateTime, DateTimeKind.Local) 
                : dateTime.DateTime;
        }

        public static string FormatSortable(this DateTimeOffset dateTime, bool includeTime = true) => dateTime.ToLocalTime().ToDateTime().FormatSortable(includeTime);

        public static string FormatSortable(this DateTime dateTime, bool includeTime = true) =>
            dateTime == default 
                ? string.Empty 
                : dateTime.ToString(includeTime ? "yyyy-MM-dd HH:mm:ss" : "yyyy-MM-dd");

        public static string FormatFileSortable(this DateTimeOffset dateTime, bool includeTime = true) => dateTime.ToLocalTime().ToDateTime().FormatFileSortable(includeTime);
        public static string FormatFileSortable(this DateTime dateTime, bool includeTime = true) =>
            dateTime == default
                ? string.Empty
                : dateTime.ToString(includeTime ? "yyyy-MM-dd_HH-mm-ss" : "yyyy-MM-dd");
        
        public static string? FormatSortable(this DateTime? dateTime, bool includeTime = true) => dateTime?.FormatSortable(includeTime);

        public static DateTime Truncate(this DateTime dateTime) =>
            dateTime == default 
                ? dateTime 
                : dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));

        public static DateTimeOffset Truncate(this DateTimeOffset dateTime) => 
            dateTime == default 
                ? dateTime 
                : dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));

        /// <summary>
        /// <para>Truncates a DateTime to a specified resolution.</para>
        /// <para>A convenient source for resolution is TimeSpan.TicksPerXXXX constants.</para>
        /// </summary>
        /// <param name="date">The DateTime object to truncate</param>
        /// <param name="resolution">e.g. to round to nearest second, TimeSpan.TicksPerSecond</param>
        /// <returns>Truncated DateTime</returns>
        public static DateTime Truncate(this DateTime date, long resolution) =>
            new DateTime(date.Ticks - date.Ticks % resolution, date.Kind);

        public static DateTimeOffset Truncate(this DateTimeOffset date, long resolution) =>
            new DateTimeOffset(date.Ticks - date.Ticks % resolution, date.Offset);

        public static TimeSpan TimeUntilNow(this DateTimeOffset start, bool truncateMillseconds = false) =>
            start.ToLocalTime().ToDateTime().TimeUntilNow(truncateMillseconds);
        public static TimeSpan TimeUntilNow(this DateTime start, bool truncateMillseconds = false)
        {
            if (truncateMillseconds)
                return DateTimeHelper.GetNow(NowPrecission.Seconds) - start.Truncate();

            return DateTimeHelper.Now - start;
        }

        public static TimeSpan TimeFromNow(this DateTimeOffset start, bool truncateMillseconds = false) =>
            start.ToLocalTime().ToDateTime().TimeFromNow(truncateMillseconds);
        public static TimeSpan TimeFromNow(this DateTime start, bool truncateMillseconds = false)
        {
            if (truncateMillseconds)
                return start.Truncate() - DateTimeHelper.GetNow(NowPrecission.Seconds);

            return start - DateTimeHelper.Now;
        }

        public static string TimeUntilNowString(this DateTimeOffset start, bool truncateMillseconds = true) =>
            start.ToLocalTime().ToDateTime().TimeUntilNowString(truncateMillseconds);
        public static string TimeUntilNowString(this DateTime start, bool truncateMillseconds = true) =>
            start.TimeUntilNow(truncateMillseconds).Format();
    }
}
