using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Shared.Extensions.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        [return: NotNull]
        public static T GetServiceOrNew<T>(this IServiceProvider provider) where T : notnull, new() => GetService<T>(provider) ?? new T();

        [return: NotNull]
        public static T GetRequiredService<T>(this IServiceProvider provider) => (T)GetRequiredService(provider, typeof(T));

        [return: NotNull]
        public static object GetRequiredService(this IServiceProvider provider, Type serviceType)
        {
            var service = provider.GetService(serviceType);
            if (service is not null)
                return service;

            throw new InvalidOperationException($"Service not registered ({serviceType.Name})");
        }

        /// <summary>
        /// Get an enumeration of services of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T">The type of service object to get.</typeparam>
        /// <param name="provider">The <see cref="IServiceProvider"/> to retrieve the services from.</param>
        /// <returns>An enumeration of services of type <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> GetServices<T>(this IServiceProvider provider)
        {
            if (provider is null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return provider.GetRequiredService<IEnumerable<T>>();
        }

        public static T? GetService<T>(this IServiceProvider provider) => (T)provider.GetService(typeof(T));

        /// <summary>
        /// Creates a new <see cref="IServiceScope"/> that can be used to resolve scoped services.
        /// </summary>
        /// <param name="provider">The <see cref="IServiceProvider"/> to create the scope from.</param>
        /// <returns>A <see cref="IServiceScope"/> that can be used to resolve scoped services.</returns>
        public static IServiceScope CreateScope(this IServiceProvider provider) => provider.GetRequiredService<IServiceScopeFactory>().CreateScope();

        public static TInterface ResolveByName<TInterface>(this IServiceProvider serviceProvider, string typeName)
        {
            var allRegisteredTypes = serviceProvider.GetRequiredService<IEnumerable<TInterface>>();
            // ReSharper disable once PossibleNullReferenceException
            var resolvedService = allRegisteredTypes.FirstOrDefault(p => p!.GetType().FullName!.Contains(typeName));
            if (resolvedService is null)
                throw new Exception($"{typeName} type not found.");

            return resolvedService;
        }
    }
}
