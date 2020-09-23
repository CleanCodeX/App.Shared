using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Common.Shared.Helpers;

namespace Common.Shared.Extensions.Enumerables.Specialized
{
    public static class ReadOnlyListExtensions
    {
        public static int IndexOf<T>([NotNull] this IReadOnlyList<T> source, T elementToFind) where T : notnull
        {
            source.ThrowIfNull(nameof(source));

            var i = 0;
            foreach (var element in source!)
            {
                if (Equals(element, elementToFind))
                    return i;
                ++i;
            }

            return -1;
        }

        public static IReadOnlyList<T> GetRange<T>([NotNull] this IReadOnlyList<T> source, Range range) where T : notnull => GetRange(source, range.Start, range.End);
        public static IReadOnlyList<T> GetRange<T>([NotNull] this IReadOnlyList<T> source, Index start, Index end) where T : notnull => GetRange(source, start.Value, end.GetOffset(source.Count) - start.Value);
        public static IReadOnlyList<T> GetRange<T>([NotNull] this IReadOnlyList<T> source, int index, int count) where T : notnull
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
    }
}