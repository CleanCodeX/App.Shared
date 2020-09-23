using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Common.Shared.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<PropertyInfo> GetProperties([NotNull] this Type source, params string[] propertyNames) => propertyNames.Select(source.GetProperty)!;

        public static PropertyInfo? FindProperty(this Type type, string propertyName)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            if (!type.IsInterface)
                return type.GetProperty(propertyName, bindingFlags);

            foreach (var @interface in type.GetInterfaces())
            {
                var propInfo = @interface.GetProperty(propertyName, bindingFlags);
                if (propInfo is not null) return propInfo;
            }

            return null;
        }

        public static TAttribute? FindAttribute<TAttribute>(this Type type, bool inherit = true)
            where TAttribute : Attribute =>
            (TAttribute?)FindAttribute(type, typeof(TAttribute), inherit);

        public static Attribute? FindAttribute(this Type type, Type attributeType, bool inherit = true)
        {
            var attribute = type.GetCustomAttribute(attributeType, inherit);
            if (attribute is not null) return attribute;
            if (!inherit) return null;

            foreach (var @interface in type.GetInterfaces())
            {
                attribute = @interface.GetCustomAttribute(attributeType, true);
                if (attribute is not null) return attribute;
            }

            return null;
        }

        public static Type? GetMetaDataType([NotNull] this Type type)
        {
            Type? lastType = type;
            Type? lastMetaData = null;

            while (lastType is not null)
            {
                lastType = lastType.GetCustomAttribute<ModelMetadataTypeAttribute>()?.MetadataType;
                if (lastType is not null)
                    lastMetaData = lastType;
            }

            return lastMetaData;
        }

        public static bool IsNullable(this Type type) => type.IsClass || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        public static bool IsNullableEnum(this Type t)
        {
            var u = Nullable.GetUnderlyingType(t);
            return u is not null && u.IsEnum;
        }

        [SuppressMessage("Style", "IDE0060:Nicht verwendete Parameter entfernen", Justification = "<Ausstehend>")]
        public static string? GetPropertyDisplayText<T>(this Type source, Expression<Func<T, object?>> propertyExpression) =>
            GetPropertyDisplayText(propertyExpression);

        public static string? GetPropertyDisplayText<T>(string propertyName) =>
            typeof(T).GetProperty(propertyName)!.GetDisplayName();

        public static string? GetPropertyDisplayText<T>(this Expression<Func<T, object?>> propertyExpression)
        {
            if (propertyExpression.Body is UnaryExpression) return null;

            var expression = (MemberExpression)propertyExpression.Body;
            var propertyInfo = (PropertyInfo)expression.Member;

            return propertyInfo.GetDisplayName();
        }

        public static bool IsSubclassOfGeneric(this Type typeToCheck, Type genericType)
        {
            while (true)
            {
                if (typeToCheck is null || typeToCheck == typeof(object)) return false;
                if (typeToCheck.IsGenericType && typeToCheck.GetGenericTypeDefinition() == genericType) return true;

                typeToCheck = typeToCheck.BaseType!;
            }
        }

        public static bool Implements<TInterface>(this Type source) where TInterface : notnull =>
            Implements(source, typeof(TInterface));

        public static bool Implements(this Type source, Type interfaceType)
        {
            if (!interfaceType.IsInterface)
                throw new ArgumentException($"Type {interfaceType.Name} is not an interface type.");

            return interfaceType.IsAssignableFrom(source);
        }

        public static bool IsSubclassOf<T>(this Type type) => type.IsSubclassOf(typeof(T));

        public static bool IsComplexType(this Type source) =>
            source.IsInterface || source.IsClass && source != typeof(string) && source != typeof(Uri);

        public static IEnumerable<PropertyInfo> GetPropertiesOfType<TType>([NotNull] this Type type,
            bool inheritProperties = true) =>
            type.GetProperties(inheritProperties).Where(p => p.PropertyType.IsSubclassOf<TType>());

        public static IEnumerable<PropertyInfo> GetPropertiesImplements<TInterface>([NotNull] this Type type,
            bool inheritProperties = true) where TInterface : notnull =>
            type.GetProperties(inheritProperties).Where(p => p.PropertyType.Implements<TInterface>());

        public static IEnumerable<PropertyInfo> GetPublicPropertiesOfType<TType>([NotNull] this Type type,
            bool inheritProperties = true) =>
            type.GetPublicProperties(inheritProperties).Where(p => p.PropertyType.IsSubclassOf<TType>());

        public static IEnumerable<PropertyInfo> GetPublicPropertiesImplements<TInterface>([NotNull] this Type type,
            bool inheritProperties = true) where TInterface : notnull =>
            type.GetPublicProperties(inheritProperties).Where(p => p.PropertyType.Implements<TInterface>());

        public static IEnumerable<PropertyInfo> GetPublicPropertiesHavingAttribute<TAttribute>([NotNull] this Type type,
            bool inheritProperties = true) where TAttribute : Attribute =>
            type.GetPublicProperties(inheritProperties).Where(p => p.IsDefined<TAttribute>());

        public static IEnumerable<PropertyInfo> GetPublicPropertiesNotHavingAttribute<TAttribute>([NotNull] this Type type,
            bool inheritProperties = true) where TAttribute : Attribute =>
            type.GetPublicProperties(inheritProperties).Where(p => !p.IsDefined<TAttribute>());

        public static IEnumerable<PropertyInfo> GetProperties([NotNull] this Type type, bool inheritProperties = true, bool publicOnly = false)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public |
                        (publicOnly ? BindingFlags.Default : BindingFlags.NonPublic) |
                        (inheritProperties ? BindingFlags.Default : BindingFlags.DeclaredOnly);

            if (!type.IsInterface)
                return type.GetProperties(bindingFlags);

            return new[] { type }
                .Concat(type.GetInterfaces())
                .SelectMany(i => i.GetProperties(bindingFlags));
        }

        public static IEnumerable<PropertyInfo> GetPropertiesHavingAttribute<TAttribute>([NotNull] this Type type,
            bool inheritProperties = true) where TAttribute : Attribute =>
            type.GetProperties(inheritProperties).Where(p => p.IsDefined<TAttribute>());

        public static IEnumerable<PropertyInfo> GetPropertiesNotHavingAttribute<TAttribute>([NotNull] this Type type,
            bool inheritProperties = true) where TAttribute : Attribute =>
            type.GetProperties(inheritProperties).Where(p => !p.IsDefined<TAttribute>());

        public static IEnumerable<PropertyInfo> GetPublicProperties([NotNull] this Type type, bool inheritProperties = true) => GetProperties(type, inheritProperties, true);

        public static IEnumerable<PropertyInfo> GetNonPrivateProperties([NotNull] this Type type, bool inheritProperties = true) =>
            type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                               (inheritProperties
                                   ? BindingFlags.Default
                                   : BindingFlags.DeclaredOnly)
                               )
                .Where(x => !x.GetMethod!.IsPrivate);
    }
}
