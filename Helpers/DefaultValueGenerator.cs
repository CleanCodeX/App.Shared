using System;
using System.Reflection;

namespace Common.Shared.Helpers
{
    public static class DefaultValueGenerator
    {
        public static object? GetDefaultOfType(this Type type) =>
            type.GetTypeInfo().IsValueType
                ? Activator.CreateInstance(type)
                : null;
    }
}