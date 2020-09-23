using System.Collections;

namespace Common.Shared.Extensions.Enumerables
{
    public static class CollectonExtensions
    {
        public static string Join(this ICollection source, string separator = ", ") => source.IsEmpty() ? string.Empty : string.Join(separator, source);
    }
}