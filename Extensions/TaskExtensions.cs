using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Common.Shared.Extensions
{
    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable KeepContext([NotNull] this Task source) => source.ConfigureAwait(false);
        public static ConfiguredTaskAwaitable<T> KeepContext<T>([NotNull] this Task<T> source) => source.ConfigureAwait(false);

        public static ConfiguredValueTaskAwaitable KeepContext([NotNull] this ValueTask source) => source.ConfigureAwait(false);
        public static ConfiguredValueTaskAwaitable<T> KeepContext<T>([NotNull] this ValueTask<T> source) => source.ConfigureAwait(false);
    }
}
