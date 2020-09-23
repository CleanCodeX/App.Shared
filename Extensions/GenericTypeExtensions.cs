using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Common.Shared.Extensions.Copy;
using Common.Shared.Helpers;
using Common.Shared.Internal.Helpers;

namespace Common.Shared.Extensions
{
    [SuppressMessage("Stil", "IDE0060:Nicht verwendete Parameter entfernen")]
    public static class GenericTypeExtensions
    {
        #region Throw

#pragma warning disable CS8777 
        public static void ThrowIfNull<T>([NotNull] this T source, string argName, string? customErrorText = null) => Requires.NotNull(source, argName, customErrorText);
#pragma warning restore CS8777 

        // ReSharper disable UnusedTypeParameter
        public struct StructTypeRequirement<T> where T : struct { }
        public struct ClassTypeRequirement<T> where T : class { }
        // ReSharper restore UnusedTypeParameter

        [return: NotNullIfNotNull("source")]
        public static T GetOrThrowIfDefault<T>([NotNull, NotNullIfNotNull("source")] this T source, string argName, string? customErrorText = null) where T : struct
        {
            if (Equals(source, default(T)))
                throw ExceptionsHelper.ArgumentDefault(argName, customErrorText);

            return source;
        }

        [return: NotNullIfNotNull("source")]
        public static T? GetOrThrowIfNull<T>([NotNull, NotNullIfNotNull("source")] this T? source, string argName, string? customErrorText = null, StructTypeRequirement<T> _ = default) where T : struct => source ?? throw ExceptionsHelper.ArgumentNull(argName, customErrorText);

        [return: NotNullIfNotNull("source")]
        public static T GetOrThrowIfNull<T>([NotNull, NotNullIfNotNull("source")] this T? source, string argName, string? customErrorText = null, ClassTypeRequirement<T> _ = default) where T : class => source ?? throw ExceptionsHelper.ArgumentNull(argName, customErrorText);

        public static void ThrowIfNegative<T>(this T source, string argName, string? customErrorText = null) where T : struct, IConvertible, IComparable
            => Requires.NotNegative(source, argName, customErrorText);

        public static void ThrowIfPositive<T>(this T source, string argName, string? customErrorText = null) where T : struct, IConvertible, IComparable
            => Requires.NotPositive(source, argName, customErrorText);

        public static void ThrowIfDefault<T>(this T source, string argName, string? customErrorText = null)
            => Requires.NotDefault(source, argName, customErrorText);

        public static void ThrowIfNotDefault<T>(this T source, string argName, string? customErrorText = null)
            => Requires.Default(source, argName, customErrorText);

        #endregion

        #region Casting, Conversion

        public static T[] AsArray<T>(this T source) where T : notnull => new[] {source};
        public static List<T> AsList<T>(this T source) where T : notnull => new List<T> {source};
        public static HashSet<T> AsHashSet<T>(this T source) where T : notnull => new HashSet<T> {source};

        public static IQueryable<T> ToQueryable<T>(this T? arg) where T : class
            => arg is null ? Array.Empty<T>().AsQueryable() : new[] { arg }.AsQueryable();

        #endregion

        #region Cloning, Copying

        public static TTarget CopyAs<TTarget>([NotNull] this object source)
            where TTarget : class, new()
        {
            var target = new TTarget();
            source.CopyValuesTo(target);

            return target;
        }

        public static T Copy<T>(this T sourceItem) => CopyExtensions.Copy(sourceItem);

        public static void CopyValuesTo<T>([NotNull] this T source, [NotNull]  object target, bool deepCopy = true)
            => PropertyCopier.CopyPropertyValuesTo(source!, target, deepCopy);

        public static void CopyValuesTo<T>([NotNull] this object source, [NotNull] object target, bool deepCopy = true)
            => PropertyCopier.CopyPropertyValuesTo<T>(source, target, deepCopy);

        #endregion

        #region Attributes

        public static int GetMaxLength<T>(this T source, Expression<Func<T, string?>> propertyExpression) where T : class
            => AttributeHelper.GetMaxLength(propertyExpression);

        public static (int MinLength, int MaxLength) GetStringLength<T, TProp>(this T source, Expression<Func<T, TProp>> propertyExpression) where T : class
            => AttributeHelper.GetStringLength(propertyExpression);

        public static (int Min, int Max) GetRange<T, TProp>(this T source, Expression<Func<T, TProp>> propertyExpression) where T : class
            => AttributeHelper.GetRange(propertyExpression);

        public static string? GetPropertyDisplayText<T>(this T source, Expression<Func<T, object?>> propertyExpression) =>
            typeof(T).GetPropertyDisplayText(propertyExpression);

        #endregion

        #region Json

        public static string ToJson<T>(this T source, bool formatIndented = false, int maxDepth = 10) where T : notnull =>
            ToJson(source, new JsonSerializerOptions
            {
                WriteIndented = formatIndented,
                IgnoreNullValues = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                IgnoreReadOnlyProperties = true,
                MaxDepth = maxDepth > 0 ? maxDepth : 10
            });
        public static string ToJson<T>(this T data, JsonSerializerOptions? options) where T : notnull =>
            options == default ? JsonSerializer.Serialize(data) : JsonSerializer.Serialize(data, options);

        #endregion
    }
}