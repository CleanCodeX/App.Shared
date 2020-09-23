using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using Common.Shared.Helpers;

namespace Common.Shared.Extensions.Enumerables
{
    public static class EnumerableExtensions
    {
        public static void ThrowIfEmpty(this IEnumerable source, string argName, string? customErrorText = null)
            => Requires.NotDefault(source, argName, customErrorText);

        public static void ThrowIfNotEmpty(this IEnumerable source, string argName, string? customErrorText = null)
            => Requires.Default(source, argName, customErrorText);

        [Pure]
        public static bool IsEmpty([NotNullWhen(false)] this IEnumerable? source)
        {
            if (source is null) return true;

            var e = source.GetEnumerator();

            return e.MoveNext();
        }

        [Pure]
        public static bool IsNotEmpty([NotNullWhen(true)] this IEnumerable? source)
        {
            if (source is null) return false;

            foreach (var _ in source)
                return true;

            return false;
        }

        public static IEnumerable ToNotNull(this IEnumerable? source) => source ?? Enumerable.Empty<object>();

        public static T? FirstOrDefault<T>(this IEnumerable source) where T : class => source.Cast<T>().FirstOrDefault();
        public static T First<T>(this IEnumerable source) where T : class => source.Cast<T>().First()!;
    }
}
