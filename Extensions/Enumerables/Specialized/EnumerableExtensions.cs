using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Common.Shared.Helpers;

namespace Common.Shared.Extensions.Enumerables.Specialized
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> TakeIfNotDefault<T>(this IEnumerable<T> source, int? count) => !count.HasValue ? source : source.Take(count.Value);

        public static IEnumerable<T> TakeIfNotDefault
            <T>(this IEnumerable<T> source, int count) => count == 0 ? source : source.Take(count);

        public static void ThrowIfEmpty<T>(this IEnumerable<T> source, string argName, string? customErrorText = null)
            => Requires.NotDefault(source, argName, customErrorText);

        public static void ThrowIfNotEmpty<T>(this IEnumerable<T> source, string argName, string? customErrorText = null)
            => Requires.Default(source, argName, customErrorText);

        public static IEnumerable<T> GetRange<T>([NotNull] this IEnumerable<T> items, int skip, int count)
            => items.Skip(skip).Take(count);
        public static IEnumerable<T> GetRange<T>([NotNull] this IEnumerable<T> items, Range range)
            => items.ToArray()[range];

        public static IEnumerable<T> OrderBySequence<T, TId>(
            this IEnumerable<T> source,
            IEnumerable<TId> order,
            Func<T, TId> idSelector)
        {
            var lookup = source.ToLookup(idSelector, t => t);
            foreach (var id in order)
                foreach (var t in lookup[id])
                    yield return t;
        }

        public static IEnumerable<T> Clone<T>(this IEnumerable<T> source) where T : ICloneable
            => source.Select(x => (T)x.Clone()).ToList();

        public static TItem GetMax<TItem, TU>(this IEnumerable<TItem> data, Func<TItem, TU> f) where TU : IComparable
            => data.Aggregate((i1, i2) => f(i1).CompareTo(f(i2)) > 0 ? i1 : i2);

        public static TItem GetMin<TItem, TU>(this IEnumerable<TItem> data, Func<TItem, TU> f) where TU : IComparable
            => data.Aggregate((i1, i2) => f(i1).CompareTo(f(i2)) < 0 ? i1 : i2);

        public static SortedSet<T> ToSorted<T>(this IEnumerable<T> source) => new SortedSet<T>(source);
        public static HashSet<TSource> ToSafeHashSet<TSource>(this IEnumerable<TSource> source)
            => source.IsEmpty() ? new HashSet<TSource>() : new HashSet<TSource>(source);

        public static bool IsEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source) => source is null || !source.Any();
        public static bool IsNotEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? source) => source is not null && source.Any();

        [return: NotNull]
        public static IEnumerable<T> ToNotNull<T>(this IEnumerable<T>? source) => source ?? Enumerable.Empty<T>();

        [return: NotNullIfNotNull("source")]
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T>? source) => source is null ? Enumerable.Empty<T>() : source.Where(x => x is not null);

        [return: NotNullIfNotNull("source")]
        public static IEnumerable<string> WhereNotNullOrEmpty(this IEnumerable<string?>? source)
        {
#nullable disable
            return source is null ? Enumerable.Empty<string>() : source.Where(x => StringExtensions.IsNotNullOrEmpty(x!));
#nullable restore
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source) => !source.Any();
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? source) => source is not null && source.Any();

        public static string Join<T>(this IEnumerable<T>? source, string separator = ", ") => source is null ? string.Empty : string.Join(separator, source);

        public static string JoinIfNotEmpty<T>(this IEnumerable<T> source, string separator, string alternativeIfEmpty) where T : notnull => source.IsNotEmpty() ? string.Join(separator, source) : alternativeIfEmpty;

        public static void ForEach<T>(this IEnumerable<T>? source, Action<T> action)
        {
            if (source is null) return;

            foreach (var item in source)
                action(item);
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body) =>
            Task.WhenAll(
                from item in source
                select Task.Run(() => body(item)));

        public static IEnumerable<string> ReplaceNonBreakSpaces(this IEnumerable<string> source) => source.Select(e => e.Replace(((char)160).ToString(), " ").Replace("&#160;", " ").Replace("&nbsp;", " "));
    }
}