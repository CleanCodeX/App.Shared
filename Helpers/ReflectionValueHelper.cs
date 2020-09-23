using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Common.Shared.Extensions;
// ReSharper disable UnusedParameter.Global

namespace Common.Shared.Helpers
{
    public static class ReflectionValueHelper
    {
        [SuppressMessage("Style", "IDE0060:Nicht verwendete Parameter entfernen")]
        public static PropertyInfo GetProperty<TModel>(this TModel source, Expression<Func<TModel, object?>> propSelector) => (PropertyInfo)propSelector.GetMemberInfo();

        [SuppressMessage("Style", "IDE0060:Nicht verwendete Parameter entfernen")]
        public static string GetDisplayName<TModel>(this TModel source, Expression<Func<TModel, object?>> propSelector, bool returnDefault = true) => propSelector.GetMemberInfo().GetDisplayName(returnDefault) ?? string.Empty;

        public static object? GetPublicFieldValueByName(this object source, string name) => InternalGetField(source, name, BindingFlags.Instance | BindingFlags.Public);

        public static object? GetPublicStaticFieldValueByName(this object source, string name) => InternalGetField(source, name, BindingFlags.Static | BindingFlags.Public);

        public static object? GetNonPublicFieldValueByName(this object source, string name) => InternalGetField(source, name, BindingFlags.Instance | BindingFlags.NonPublic);

        public static object? GetNonPublicStaticFieldValueByName(this object source, string name) => InternalGetField(source, name, BindingFlags.Static | BindingFlags.NonPublic);

        private static object? InternalGetField(object source, string name, BindingFlags bindingFlags)
        {
            var member = source.GetType().GetField(name, bindingFlags)!;
            member.ThrowIfNull(nameof(member));

            return member.GetValue(source);
        }

        public static object? GetPropertyValueByName(this object source, string name) =>
            InternalGetPropertyValue(source, name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public static T GetPropertyValueByName<T>(this object source, string name) => (T)GetPropertyValueByName(source, name)!;

        public static object? GetPublicPropertyValueByName(this object source, string name) => InternalGetPropertyValue(source, name, BindingFlags.Instance | BindingFlags.Public);

        public static T GetPublicPropertyValueByName<T>(this object source, string name) => (T)GetPublicPropertyValueByName(source, name)!;

        public static object? GetPublicStaticPropertyValueByName(this object source, string name) => InternalGetPropertyValue(source, name, BindingFlags.Static | BindingFlags.Public);

        public static object? GetNonPublicPropertyValueByName(this object source, string name) => InternalGetPropertyValue(source, name, BindingFlags.Instance | BindingFlags.NonPublic);

        public static object? GetNonPublicStaticPropertyValueByName(this object source, string name) => InternalGetPropertyValue(source, name, BindingFlags.Static | BindingFlags.NonPublic);

        private static object? InternalGetPropertyValue(this object source, string name, BindingFlags bindingFlags)
        {
            var member = source.GetType().GetProperty(name, bindingFlags | BindingFlags.GetProperty)!;

            return member.GetValue(source);
        }

        public static void SetPublicFieldValueByName(this object source, string name, object? value) => InternalSetField(source, name, value, BindingFlags.Instance | BindingFlags.Public);

        public static void SetPublicStaticFieldValueByName(this object source, string name, object? value) => InternalSetField(source, name, value, BindingFlags.Static | BindingFlags.Public);

        public static void SetNonPublicFieldValueByName(this object source, string name, object? value) => InternalSetField(source, name, value, BindingFlags.Instance | BindingFlags.NonPublic);

        public static void SetNonPublicStaticFieldValueByName(this object source, string name, object? value) => InternalSetField(source, name, value, BindingFlags.Static | BindingFlags.NonPublic);

        private static void InternalSetField(this object source, string name, object? value, BindingFlags bindingFlags)
        {
            var member = source.GetType().GetField(name, bindingFlags);
            member.ThrowIfNull(nameof(member));

            var convertedValue = GetConvertedValue(member.FieldType, value);

            member.SetValue(source, convertedValue);
        }

        public static void SetPropertyValueByName(this object source, string name, object? value) =>
            InternalSetPropertyValue(source, name, value, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public static void SetPublicPropertyValueByName(this object source, string name, object? value) => InternalSetPropertyValue(source, name, value, BindingFlags.Instance | BindingFlags.Public);

        public static void SetPublicStaticPropertyValueByName(this object source, string name, object? value) => InternalSetPropertyValue(source, name, value, BindingFlags.Static | BindingFlags.Public);

        public static void SetNonPublicPropertyValueByName(this object source, string name, object? value) => InternalSetPropertyValue(source, name, value, BindingFlags.Instance | BindingFlags.NonPublic);

        public static void SetNonPublicStaticPropertyValueByName(this object source, string name, object? value) => InternalSetPropertyValue(source, name, value, BindingFlags.Static | BindingFlags.NonPublic);

        private static void InternalSetPropertyValue(this object source, string name, object? value, BindingFlags bindingFlags)
        {
            var member = source.GetType().GetProperty(name, bindingFlags | BindingFlags.SetProperty);
            member.ThrowIfNull(nameof(member));

            var convertedValue = GetConvertedValue(member.PropertyType, value);

            member.SetValue(source, convertedValue);
        }

        private static object? GetConvertedValue(Type valueType, object? value)
        {
            if (valueType.IsClass || valueType.IsInterface) return value;
            if (value is null) return null;

            var convertedValue = valueType.IsEnum ? Enum.Parse(valueType, value.ToString()!) : Convert.ChangeType(value, valueType);

            return convertedValue;
        }
    }
}