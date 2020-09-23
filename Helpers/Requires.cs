using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Common.Shared.Extensions;
using Common.Shared.Extensions.Enumerables;
using Common.Shared.Helpers.Internal;
using Res = Common.Shared.Properties.Resources;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Common.Shared.Helpers
{
    /// <summary>
    /// Helper class name for common checks
    /// </summary>
    public static class Requires
    {
        public static void Implements<TInterface>([NotNull] object instance, string? customErrorText = null)
        {
            if (!(instance is TInterface))
                throw new ArgumentException(customErrorText ?? Res.ArgumentMustImplementTemplate.InsertArgs(instance.GetType().Name, typeof(TInterface).Name));
        }

        public static void NotTrue(bool arg, string argName, string? customErrorText = null) => False(arg, argName, customErrorText);
        public static void False(bool arg, string argName, string? customErrorText = null)
        {
            if (arg)
                throw new ArgumentException(customErrorText ?? Strings.ArgumentMustBeFalse, argName);
        }

        public static void NotFalse(bool arg, string argName, string? customErrorText = null) => True(arg, argName, customErrorText);
        public static void True(bool arg, string argName, string? customErrorText = null)
        {
            if (!arg)
                throw new ArgumentException(customErrorText ?? Strings.ArgumentMustBeTrue, argName);
        }

        public static void NotLessThan<T>(T actual, T expected, string argName, string? customErrorText = null)
            where T : struct, IComparable => GreaterThanOrEqual(actual, expected, argName, customErrorText);

        public static void GreaterThanOrEqual<T>(T actual, T expected, string argName, string? customErrorText = null)
            where T : struct, IComparable
        {
            if (actual.CompareTo(expected) < 0)
                throw new ArgumentException(customErrorText ?? Strings.ArgumentNotLessThanMaxCurrentTemplate.InsertArgs(actual, expected), argName);
        }

        public static void NotLessThanOrEqual<T>(T actual, T expected, string argName, string? customErrorText = null)
            where T : struct, IComparable => GreaterThan(actual, expected, argName, customErrorText);

        public static void GreaterThan<T>(T actual, T expected, string argName, string? customErrorText = null)
            where T : struct, IComparable
        {
            if (actual.CompareTo(expected) <= 0)
                throw new ArgumentException(customErrorText ?? Strings.ArgumentNotLessThanOrEqualMaxCurrentTemplate.InsertArgs(actual, expected), argName);
        }

        public static void NotGreaterThan<T>(T actual, T expected, string argName, string? customErrorText = null)
            where T : struct, IComparable => LessThanOrEqual(actual, expected, argName, customErrorText);
        public static void LessThanOrEqual<T>(T actual, T expected, string argName, string? customErrorText = null)
            where T : struct, IComparable
        {
            if (actual.CompareTo(expected) > 0)
                throw new ArgumentException(customErrorText ?? Strings.ArgumentNotGreaterThanMaxCurrentTemplate.InsertArgs(actual, expected), argName);
        }
        
        public static void NotGreaterThanOrEqual<T>(T actual, T expected, string argName, string? customErrorText = null)
            where T : struct, IComparable => LessThan(actual, expected, argName, customErrorText);

        public static void LessThan<T>(T actual, T expected, string argName, string? customErrorText = null)
            where T : struct, IComparable
        {
            if (actual.CompareTo(expected) >= 0)
                throw new ArgumentException(customErrorText ?? Strings.ArgumentNotGreaterThanOrEqualMaxCurrentTemplate.InsertArgs(actual, expected), argName);
        }

        public static void Null(object? arg, string argName, string? customErrorText = null)
        {
            if (arg is not null)
                throw new ArgumentException(customErrorText ?? Strings.ArgumentMustBeNull, argName);
        }

        public static void NotNull(object? arg, string argName, string? customErrorText = null)
        {
            if (arg is null)
                throw new ArgumentNullException(argName, customErrorText ?? Strings.ArgumentMustNotBeNull);
        }

        public static void NotDefault<T>(T arg, string argName, string? customErrorText = null)
        {
            if (EqualityComparer<T>.Default.Equals(arg, default!))
                throw new ArgumentException(customErrorText ?? Strings.ArgumentMustNotBeDefault, argName);
        }

        public static void NotNegative<T>(T arg, string argName, string? customErrorText = null) where T : struct, IConvertible, IComparable
        {
            var int64 = Convert.ToInt64(arg);
            if (int64 < 0)
                throw new ArgumentException(customErrorText ?? Strings.ArgumentMustBePositive, argName);
        }

        public static void NotPositive<T>(T arg, string argName, string? customErrorText = null) where T : struct, IConvertible, IComparable
        {
            var int64 = Convert.ToInt64(arg);
            if (int64 > 0)
                throw new ArgumentException(customErrorText ?? Strings.ArgumentMustBeNegative, argName);
        }

        public static void Default<T>(T arg, string argName, string? customErrorText = null)
        {
            if (!EqualityComparer<T>.Default.Equals(arg, default!))
                throw new ArgumentException(customErrorText ?? Strings.ArgumentMustNotBeDefault, argName);
        }

        public static void NotNullOrEmpty(string? arg, string argName, string? customErrorText = null)
        {
            NotNull(arg, argName);
            if (arg == string.Empty)
                throw new ArgumentException(customErrorText ?? Strings.ArgumentValueNotEmpty, argName);
        }

        public static void NotNullOrEmpty<T>(IEnumerable<T> arg, string argName, string? customErrorText = null)
        {
            NotNull(arg, argName);
            if (!arg.Any())
                throw new ArgumentException(customErrorText ?? Strings.ArgumentListNotEmptyMaxLength, argName);
        }

        public static void NotNullOrEmpty(IEnumerable arg, string argName, string? customErrorText = null)
        {
            NotNull(arg, argName);
            if (!arg.IsNotEmpty())
                throw new ArgumentException(customErrorText ?? Strings.ArgumentListNotEmptyMaxLength, argName);
        }

        public static void Equal<T>(T actual, T expected, string argName, string? customErrorText = null)
        {
            if (!Equals(actual, expected))
                throw new ArgumentException(customErrorText ?? Strings.ArgumentEqualCompareCurrentTemplate.InsertArgs(actual), argName);
        }

        public static void NotEqual<T>(T actual, T expected, string argName, string? customErrorText = null)
        {
            if (Equals(actual, expected))
                throw new ArgumentException(customErrorText ?? Strings.ArgumentNotEqualCurrentTemplate.InsertArgs(actual), argName);
        }

        public static void DirectoryExists(string? path, string argName, string? customErrorText = null)
        {
            NotNullOrEmpty(path, argName);

            if (!Directory.Exists(path))
                throw new ArgumentException(customErrorText ?? Strings.ArgumentDirectoryNotExistsTemplate.InsertArgs(path), argName);
        }

        public static void FileExists(string? path, string argName, string? customErrorText = null)
        {
            NotNullOrEmpty(path, argName);

            if (!File.Exists(path))
                throw new ArgumentException(customErrorText ?? Strings.ArgumentFileNotExistsTemplate.InsertArgs(path), argName);
        }

        public static void MaxFileSizeNotExceeded<T>(T actualSize, T maxSize, string argName)
            where T : struct, IConvertible, IComparable, IComparable<T>
        {
            if (actualSize.CompareTo(maxSize) > 0)
                throw ExceptionsHelper.MaxFileSizeExceeded(actualSize, maxSize, argName);
        }

        public static void MaxLength(string? value, int maxLength, string argName, string? customErrorText = null)
        {
            if (maxLength > 0 && value?.Length > maxLength)
                throw new ArgumentException(customErrorText ?? Strings.ArgumentMaxLengthTemplate.InsertArgs(maxLength), argName);
        }

        internal static class Strings
        {
            internal static readonly string ArgumentNotEqualCurrentTemplate = Res.ArgumentNotEqualCurrentTemplate;
            internal static readonly string ArgumentEqualCompareCurrentTemplate = Res.ArgumentEqualCompareCurrentTemplate;

            internal static readonly string ArgumentNotLessThanMaxCurrentTemplate = Res.ArgumentNotLessThanMaxCurrentTemplate;
            internal static readonly string ArgumentNotGreaterThanMaxCurrentTemplate = Res.ArgumentNotGreaterThanMaxCurrentTemplate;

            internal static readonly string ArgumentNotLessThanOrEqualMaxCurrentTemplate = Res.ArgumentNotLessThanOrEqualMaxCurrentTemplate;
            internal static readonly string ArgumentNotGreaterThanOrEqualMaxCurrentTemplate = Res.ArgumentNotGreaterThanOrEqualMaxCurrentTemplate;

            internal static readonly string ArgumentMustBeNull = Res.ArgumentMustBeNull;
            internal static readonly string ArgumentMustBeTrue = Res.ArgumentMustBeTrue;
            internal static readonly string ArgumentMustBeFalse = Res.ArgumentMustBeFalse;
            internal static readonly string ArgumentMustNotBeNull = Res.ArgumentMustNotBeNull;
            internal static readonly string ArgumentMustBeNegative = Res.ArgumentMustBeNegative;
            internal static readonly string ArgumentMustBePositive = Res.ArgumentMustBePositive;
            internal static readonly string ArgumentMustNotBeDefault = Res.ArgumentMustNotBeDefault;
            internal static readonly string ArgumentValueNotEmpty = Res.ArgumentValueNotEmpty;
            internal static readonly string ArgumentListNotEmptyMaxLength = Res.ArgumentListNotEmptyMaxLength;
            internal static readonly string ArgumentMaxLengthTemplate = Res.ArgumentMaxLengthTemplate;
            internal static readonly string ArgumentDirectoryNotExistsTemplate = Res.ArgumentDirectoryNotExistsTemplate;
            internal static readonly string ArgumentFileNotExistsTemplate = Res.ArgumentFileNotExistsTemplate;
            internal static readonly string ArgumentMaxFileSizeExceededSizeTemplate = Res.ArgumentMaxFileSizeExceededSizeTemplate;
            internal static readonly string ArgumentMustImplementTemplate = Res.ArgumentMustImplementTemplate;
        }
    }
}
