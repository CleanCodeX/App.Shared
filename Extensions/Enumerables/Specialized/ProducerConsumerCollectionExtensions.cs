using System.Collections.Concurrent;

namespace Common.Shared.Extensions.Enumerables.Specialized
{
    public static class ProducerConsumerCollectionExtensions
    {
        public static void Shrink<T>(this IProducerConsumerCollection<T> list, int maxListSize)
        {
            for (var i = list.Count - 1; i > maxListSize; i--)
                list.TryTake(out _);
        }
    }
}