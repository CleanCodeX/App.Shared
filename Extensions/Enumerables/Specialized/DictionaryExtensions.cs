using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Shared.Helpers;
using Common.Shared.Internal.Helpers;

namespace Common.Shared.Extensions.Enumerables.Specialized
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this IDictionary<TKey, TValue> source)
            where TKey: notnull
            where TValue : ICloneable
            => source.ToDictionary(k => k.Key, v => (TValue)v.Value.Clone());

        [return: MaybeNull]
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) => dictionary.GetValueOrDefault(key, default!);

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            dictionary.ThrowIfNull(nameof(dictionary));

            return dictionary!.TryGetValue(key, out var value) ? value : defaultValue;
        }

        [return: NotNullIfNotNull("source")]
        public static TValue GetOrThrowIfDefault<TValue>([NotNull, NotNullIfNotNull("source")] this IDictionary<string, TValue> source, string argName, string? customErrorText = null) where TValue : struct
        {
            var value = source.GetValueOrDefault(argName);

            if (Equals(value, default(TValue)))
                throw ExceptionsHelper.ArgumentDefault(argName, customErrorText);

            return value;
        }

        [return: NotNullIfNotNull("source")]
        public static TValue GetOrThrowIfDefault<TValue>([NotNull, NotNullIfNotNull("source")] this IDictionary<string, object> source, string argName, string? customErrorText = null) where TValue : struct
        {
            var value = (TValue?)source.GetValueOrDefault(argName);

            if (value is null || Equals(value, default(TValue)))
                throw ExceptionsHelper.ArgumentDefault(argName, customErrorText);

            return (TValue)value;
        }
    }
}