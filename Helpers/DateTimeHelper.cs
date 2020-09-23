using System;
using System.Diagnostics.CodeAnalysis;
using Common.Shared.Extensions;

namespace Common.Shared.Helpers
{
    public enum NowPrecission
    {
        Seconds = 1,
        Milliseconds = 2,
        Nanoseconds = 3
    }
    
    public static class DateTimeHelper
    {
        private static readonly Func<DateTimeOffset> NowFactoryDefault = () => DateTimeOffset.Now;
        private static readonly Func<DateTimeOffset> UtcNowFactoryDefault = () => DateTimeOffset.UtcNow;

        private static Func<DateTimeOffset>? _nowFactory;
        private static Func<DateTimeOffset>? _utcNowFactory;

        public static void FreezeTime()
        {
            _nowFactory = () => DateTimeOffset.Now;
            _utcNowFactory = () => DateTimeOffset.UtcNow;
        }

        public static Func<DateTimeOffset> NowFactory
        {
            get => _nowFactory ?? NowFactoryDefault;
            [param: AllowNull]
            set => _nowFactory = value ?? NowFactoryDefault;
        }

        public static Func<DateTimeOffset> UtcNowFactory
        {
            get => _utcNowFactory ?? UtcNowFactoryDefault;
            [param: AllowNull]
            set => _utcNowFactory = value ?? UtcNowFactoryDefault;
        }

        public static DateTimeOffset Now => GetNow(NowPrecission.Milliseconds);
        public static DateTimeOffset UtcNow => GetUtcNow(NowPrecission.Milliseconds);
        
        public static DateTimeOffset GetNow(NowPrecission mode) => InternalGetNow(mode, NowFactory);
        public static DateTimeOffset GetUtcNow(NowPrecission mode) => InternalGetNow(mode, UtcNowFactory);
        
        private static DateTimeOffset InternalGetNow(NowPrecission mode, Func<DateTimeOffset> nowFactory)
        {
            return mode switch
            {
                NowPrecission.Milliseconds => nowFactory().Truncate(TimeSpan.TicksPerMillisecond),
                NowPrecission.Seconds => nowFactory().Truncate(),
                NowPrecission.Nanoseconds => nowFactory(),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        public static DateTimeOffset UtcToday => UtcNowFactory().Date;
        public static DateTimeOffset Today => NowFactory().Date;

        public static TimeSpan TimeOfToday => NowFactory().TimeOfDay;
        public static TimeSpan UtcTimeOfToday => UtcNowFactory().TimeOfDay;

        public static void ResetFactories()
        {
            NowFactory = default!;
            UtcNowFactory = default!;
        }
    }
}
