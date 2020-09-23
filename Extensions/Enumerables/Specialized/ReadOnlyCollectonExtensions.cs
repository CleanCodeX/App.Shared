using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Shared.Extensions.Enumerables.Specialized
{
    public static class ReadOnlyCollectonExtensions
    {
        public static string JoinIfNotEmpty<T>(this IReadOnlyCollection<T> source, string separator, string alternativeIfEmpty) where T : notnull => source?.Count > 0 ? string.Join(separator, source) : alternativeIfEmpty;

        public static string Join<T>(this IReadOnlyCollection<T> source, string separator = ", ") where T : notnull => source is null ? string.Empty : string.Join(separator, source);

        public static void ProcessBatch<T>(this IReadOnlyCollection<T> list, int batchSize,
            Action<IReadOnlyCollection<T>> funcToCall) where T : notnull
        {
            var startOffset = 0;
            while (startOffset < list.Count)
            {
                var batch = list.Skip(startOffset).Take(batchSize).ToArray();

                funcToCall(batch);

                startOffset += batchSize;
            }
        }
    }
}