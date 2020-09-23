using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Shared.Attributes;

namespace Common.Shared.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetTypes(this Assembly assembly, Func<Type, bool> condition) =>
            assembly.GetTypes().Where(condition).ToArray();

        public static Type? GetTypeCaseInsensitive(this Assembly assembly, string entityTypeName) =>
            assembly.GetTypes().SingleOrDefault(x => x.Name.Equals(entityTypeName));

        public static Type? GetType(this Assembly assembly, Func<Type, bool> condition) =>
            assembly.GetTypes().SingleOrDefault(condition);

        public static IEnumerable<Type> GetSubclassesOf<TType>(this Assembly assembly, bool includeAbstractClasses = false, bool includeIgnoredClasses = false) =>
            assembly.GetSubclassesOf(typeof(TType), includeAbstractClasses, includeIgnoredClasses);

        public static IEnumerable<Type> GetSubclassesOf(this Assembly assembly, Type type, bool includeAbstractClasses = false, bool includeIgnoredClasses = false)
        {
            return GetTypes(assembly, t =>
                (!t.IsAbstract || includeAbstractClasses) &&
                (includeIgnoredClasses || !t.IsDefined(typeof(IgnoredForReflectionTypeSearchAttribute))) &&
                t.IsSubclassOf(type));
        }

        public static IEnumerable<Type> GetClassesWithAttribute<TAttribute>(this Assembly assembly, bool inherit = true, bool includeAbstractClasses = false, bool includeIgnoredClasses = false)
            where TAttribute : Attribute
        {
            return GetTypes(assembly, t =>
                (!t.IsAbstract || includeAbstractClasses) &&
                (includeIgnoredClasses || !t.IsDefined(typeof(IgnoredForReflectionTypeSearchAttribute))) &&
                t.IsDefined(typeof(TAttribute), inherit));
        }

        public static IEnumerable<Type> GetImplementersOf<TInterface>(this Assembly assembly,
            bool includeAbstractClasses = false, bool includeIgnoredClasses = false) =>
            assembly.GetImplementersOf(typeof(TInterface), includeAbstractClasses, includeIgnoredClasses);

        public static IEnumerable<Type> GetImplementersOf(this Assembly assembly, Type interfaceType, bool includeAbstractClasses = false, bool includeIgnoredClasses = false)
        {
            interfaceType.IsInterface.ThrowIfFalse(nameof(interfaceType.IsInterface));

            return assembly.GetTypes().Where(t => !t.IsInterface).Where(t =>
                (includeAbstractClasses || !t.IsAbstract) &&
                (includeIgnoredClasses || !t.IsDefined(typeof(IgnoredForReflectionTypeSearchAttribute))) &&
                (interfaceType.IsAssignableFrom(t) ||
                 t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType ||
                t.GetInterfaces().Any(i => interfaceType.IsAssignableFrom(i) ||
                                           i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)));
        }
    }
}
