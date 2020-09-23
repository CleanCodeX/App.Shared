using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Shared.Extensions.Enumerables
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterAllAssignableType<T>(this IServiceCollection services, string assemblyName)
        {
            var assembly = AppDomain.CurrentDomain.Load(assemblyName);
            var types = assembly.GetTypes(p => typeof(T).IsAssignableFrom(p)).ToArray();

            var methodInfo = typeof(ServiceCollectionServiceExtensions).GetMethods().FirstOrDefault(m =>
                m.Name == "AddTransient" &&
                m.IsGenericMethod &&
                m.GetGenericArguments().Length == 2);

            methodInfo.ThrowIfNull(nameof(methodInfo));

            foreach (var type in types)
            {
                if (type.IsInterface)
                    continue;

                var method = methodInfo!.MakeGenericMethod(typeof(T), type);
                method.Invoke(services, new object[] { services });
            }
        }
    }
}