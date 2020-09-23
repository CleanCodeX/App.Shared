using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Common.Shared.Extensions.Enumerables.Specialized
{
    public static class ArrayExtensions
    {
        public static T[] Clone<T>([NotNull] this T[] source) where T : notnull, ICloneable => source.Select(x => (T)x.Clone()).ToArray();
    }
}