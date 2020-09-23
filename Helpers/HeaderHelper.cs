using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Shared.Attributes;
using Common.Shared.Extensions;

namespace Common.Shared.Helpers
{
    [SuppressMessage("Stil", "IDE0060:Nicht verwendete Parameter entfernen")]
    public static class HeaderHelper
    {
        // ReSharper disable once UnusedParameter.Global
        public static IReadOnlyDictionary<string, string> GetHeaders<TModel>([NotNull] this TModel source, bool inheritProperties = false) where TModel : class
            => GetHeaders(typeof(TModel), inheritProperties);

        public static IReadOnlyDictionary<string, string> GetHeaders<TModel>(bool inheritProperties = false)
            => GetHeaders(typeof(TModel), inheritProperties);

        public static IReadOnlyDictionary<string, string> GetHeaders([NotNull] this Type modelType, bool inheritProperties = true)
        {
            var properties = modelType.GetPublicPropertiesNotHavingAttribute<HideAttribute>(inheritProperties);

            return new ReadOnlyDictionary<string, string>(properties
                .Select(m => new { m.Name, DisplayName = m.GetDisplayName(false) })
                .Where(m => m.DisplayName is not null)
                .ToDictionary(k => k.Name, v => v.DisplayName)!);
        }
    }
}