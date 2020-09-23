using System.Collections.Generic;
using System.Linq;

namespace Common.Shared.Extensions.Enumerables.Specialized
{
    public static class CollectionExtensions
    {
        public static T RemoveByPrefixIf<T>(this T source, string prefix, bool condition) where T : ICollection<string>
            => condition ? source.RemoveByPrefix(prefix) : source;

        public static T RemoveByPrefix<T>(this T source, string prefix) where T : ICollection<string>
        {
            var elementsToRemove = new List<string>();

            foreach (var element in source)
            {
                if (!element.StartsWith(prefix)) continue;

                elementsToRemove.Add(element);
                elementsToRemove.Add(element.SubstringAfter(prefix));
            }

            foreach (var element in elementsToRemove)
                source.Remove(element);

            return source;
        }

        public static void CopyFrom<T>(this ICollection<T>? source, IEnumerable<T>? @from) where T : notnull
        {
            if (source is null || @from is null) return;

            source.Clear();
            @from.ForEach(source.Add);
        }

        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items) where T : notnull
        {
            source.ThrowIfNull(nameof(source));
            items.ThrowIfNull(nameof(items));

            if (source is List<T> list)
                list.AddRange(items);
            else
                foreach (var item in items!)
                    source!.Add(item);
        }

        public static bool EqualsAll<T>(this ICollection<T>? a, ICollection<T>? b) where T : notnull
        {
            if (a is null || b is null)
                return a is null && b is null;

            return a.Count == b.Count && a.SequenceEqual(b);
        }
    }
}