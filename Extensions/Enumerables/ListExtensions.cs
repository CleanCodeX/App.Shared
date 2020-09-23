using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Common.Shared.Extensions.Enumerables
{
    public static class ListExtensions
    {
        public static void Shrink(this IList list, int maxListSize)
        {
            for (var i = list.Count - 1; i > maxListSize; i--)
                list.RemoveAt(i);
        }

        public static string Join(this IList source, string separator = ", ") => string.Join(separator, source);

        public static void CopyFrom([NotNull] this IList source, [NotNull] IEnumerable @from)
        {
            source.Clear();

            foreach (var item in @from)
                source.Add(item);
        }
    }
}