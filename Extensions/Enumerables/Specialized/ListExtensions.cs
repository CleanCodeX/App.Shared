using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Shared.Helpers;

namespace Common.Shared.Extensions.Enumerables.Specialized
{
    public static class ListExtensions
    {
        public static IList<T> GetRange<T>([NotNull] this IList<T> source, Range range) where T : notnull => GetRange(source, range.Start, range.End);
        public static IList<T> GetRange<T>([NotNull] this IList<T> source, Index start, Index end) where T : notnull => GetRange(source, start.Value, end.GetOffset(source.Count) - start.Value);
        public static IList<T> GetRange<T>([NotNull] this IList<T> source, int index, int count) where T : notnull
        {
            source.ThrowIfNull(nameof(source));
            index.ThrowIfNegative(nameof(index));
            count.ThrowIfNegative(nameof(count));
            Requires.NotLessThan(source!.Count - index, count, $"{nameof(count)} - {nameof(index)}");

            var list = new T[count];
            for (var i = 0; i < count; i++)
                list[i] = source[i + index];

            return list;
        }

        public static List<T> Clone<T>(this IList<T> source) where T : ICloneable
            => source.Select(x => (T)x.Clone()).ToList();
    }
}