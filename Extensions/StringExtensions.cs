using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Common.Shared.Extensions.Enumerables.Specialized;
using Common.Shared.Helpers;
using Microsoft.Extensions.Primitives;

namespace Common.Shared.Extensions
{
    public static partial class StringExtensions
    {
#pragma warning disable CA2211 
        public static Func<string, string>? StandardFormatter;
#pragma warning restore CA2211 

        public static string? PathJoin(this string? path1, string? path2)
        {
            if (path1!.IsNullOrEmpty()) return path2;
            if (path2!.IsNullOrEmpty()) return path1;

            return Path.Join(path1, path2);
        }

        public static string? UriJoin(this string? path1, string? path2) => PathJoin(path1, path2)?.Replace(@"\", "/");

        public static string? PathJoin(this string? path, params string?[] paths) =>
            path!.IsNullOrEmpty()
                ? path
                : paths.WhereNotNull().Aggregate(path, Path.Join);

        public static object? ParseEnum(this string? value, Type enumType,
            bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(value)) return null;
            if (Enum.TryParse(enumType, value, ignoreCase, out var lResult)) return (Enum)lResult!;

            return null;
        }

        public static TEnum ParseEnum<TEnum>(this string? value,
            bool ignoreCase = true,
            TEnum defaultValue = default)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;
            return Enum.TryParse(value, ignoreCase, out TEnum lResult)
                ? lResult
                : defaultValue;
        }

        public static bool TryParseEnum<TEnum>(this string value,
            out TEnum enumValue, bool ignoreCase = true)
            where TEnum : struct, Enum
        {
            enumValue = default;

            return !value.IsNullOrEmpty() && Enum.TryParse(value, ignoreCase, out enumValue);
        }

#pragma warning disable CS8777 // Der Parameter muss beim Beenden einen Wert ungleich NULL aufweisen.
        public static void ThrowIfNullOrEmpty([NotNull] this string? source, string argName, string? customErrorText = null) => Requires.NotNullOrEmpty(source, argName, customErrorText);
#pragma warning restore CS8777 // Der Parameter muss beim Beenden einen Wert ungleich NULL aufweisen.

        [return: NotNullIfNotNull("source")]
        public static string GetOrThrowIfNullOrEmpty([NotNull, NotNullIfNotNull("source")] this string? source, string argName, string? customErrorText = null)
        {
            if (source.IsNullOrEmpty())
                Requires.NotNullOrEmpty(source, argName, customErrorText);

            source ??= string.Empty;

            return source;
        }

        [return: NotNullIfNotNull("source")]
#pragma warning disable CS8777 // Der Parameter muss beim Beenden einen Wert ungleich NULL aufweisen.
        public static string GetIfNotNullOrEmpty([NotNull, NotNullIfNotNull("source")] this string? source, string alternative) => source.IsNullOrEmpty() ? alternative : source;
#pragma warning restore CS8777 // Der Parameter muss beim Beenden einen Wert ungleich NULL aufweisen.

        public static string? ToNullIfEmpty(this string? text) => text == Constants.EmptyString ? null : text;
        public static string? ToNullIfEmptyOrWhiteSpace(this string? text) => string.IsNullOrWhiteSpace(text) ? null : text;

        public static string ReplaceUntil(this string source, string separator, string find, string replace)
        {
            var strBefore = source.SubstringBefore(separator, true);
            var strAfter = source.SubstringAfter(separator);
            var newString = strBefore.Replace(find, replace, StringComparison.Ordinal) + strAfter;

            return newString;
        }

        public static string? RemoveQuotes(this string? source) => source!.IsNullOrEmpty() 
            ? source 
            : source!.Replace(@"""", string.Empty);

        public static string? Remove(this string? source, string? textToRemove)
        {
            if (source!.IsNullOrEmpty()) return source;
            
            return textToRemove!.IsNullOrEmpty() 
                ? source 
                : source!.Replace(textToRemove, string.Empty);
        }

        public static string? EscapeSpaces(this string? source) =>
            source!.IsNullOrEmpty()
                ? source
                : source!.Replace(" ", "%20");

        public static string? RemoveSingleQuotes(this string? source) =>
            source!.IsNullOrEmpty()
                ? source
                : source!.Replace(@"'", string.Empty);

        /// <summary>
        /// Returns the substring until the given <paramref name="occurrence"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="occurrence">The occurrence to search.</param>
        /// <param name="includeOccurrence">Whether include the occurrence or not.</param>
        /// <param name="returnEmptyIfNotFound">Whether to return an empty string if the occurrence could not be found.</param>
        /// <returns></returns>
        [return: NotNullIfNotNull("source")]
        public static string? SubstringBefore(this string? source, char occurrence, bool includeOccurrence = false, bool returnEmptyIfNotFound = false)
        {
            if (source.IsNullOrEmpty()) return source;

            var index = source.IndexOf(occurrence, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1)
                return returnEmptyIfNotFound ? string.Empty : source;

            var length = includeOccurrence ? index + 1 : index;
            return source.Substring(0, length);
        }

        /// <summary>
        /// Returns the substring until the given <paramref name="occurrence"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="occurrence">The occurrence to search.</param>
        /// <param name="includeOccurrence">Whether include the occurrence or not.</param>
        /// <param name="returnEmptyIfNotFound">Whether to return an empty string if the occurrence could not be found.</param>
        /// <returns></returns>
        [return: NotNullIfNotNull("source")]
        public static string? SubstringBefore(this string? source, string occurrence, bool includeOccurrence = false, bool returnEmptyIfNotFound = false)
        {
            if (source.IsNullOrEmpty()) return source;

            var index = source.IndexOf(occurrence, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1)
                return returnEmptyIfNotFound ? string.Empty : source;

            var length = includeOccurrence ? index + occurrence.Length : index;
            return source.Substring(0, length);
        }

        /// <summary>
        /// Returns the substring between the given <paramref name="startOccurrence" /> and <paramref name="endOccurrence"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startOccurrence">The occurrence to search.</param>
        /// <param name="endOccurrence">The occurrence to search.</param>
        /// <param name="includeOccurrence">Whether include the occurrence or not.</param>
        /// <param name="returnEmptyIfNotFound">Whether to return an empty string if the occurrence could not be found.</param>
        /// <returns></returns>
        [return: NotNullIfNotNull("source")]
        public static string? SubstringBetween(this string? source, string startOccurrence, string endOccurrence, bool includeOccurrence = false, bool returnEmptyIfNotFound = false)
        {
            if (source.IsNullOrEmpty()) return source;

            var startIndex = source.IndexOf(startOccurrence, StringComparison.InvariantCultureIgnoreCase);
            if (startIndex == -1)
                return returnEmptyIfNotFound ? string.Empty : source;

            startIndex = includeOccurrence ? startIndex : startIndex + startOccurrence.Length;

            var endIndex = source.IndexOf(endOccurrence, StringComparison.InvariantCultureIgnoreCase);
            if (endIndex == -1)
            {
                if (returnEmptyIfNotFound)
                    return string.Empty;

                endIndex = source.Length - 1;
            }
            else
                endIndex += includeOccurrence ? endOccurrence.Length : 0;

            return source[startIndex..endIndex];
        }

        /// <summary>
        /// Returns the substring between the given <paramref name="startOccurrence" /> and <paramref name="endOccurrence"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startOccurrence">The occurrence to search.</param>
        /// <param name="endOccurrence">The occurrence to search.</param>
        /// <param name="includeOccurrence">Whether include the occurrence or not.</param>
        /// <param name="returnEmptyIfNotFound">Whether to return an empty string if the occurrence could not be found.</param>
        /// <returns></returns>
        [return: NotNullIfNotNull("source")]
        public static string? SubstringBetween(this string? source, char startOccurrence, char endOccurrence, bool includeOccurrence = false, bool returnEmptyIfNotFound = false)
        {
            if (source.IsNullOrEmpty()) return source;

            var startIndex = source.IndexOf(startOccurrence, StringComparison.InvariantCultureIgnoreCase);
            if (startIndex == -1)
                return returnEmptyIfNotFound ? string.Empty : source;

            startIndex = includeOccurrence ? startIndex : startIndex + 1;

            var endIndex = source.IndexOf(endOccurrence, StringComparison.InvariantCultureIgnoreCase);
            if (endIndex == -1)
            {
                if (returnEmptyIfNotFound)
                    return string.Empty;

                endIndex = source.Length - 1;
            }
            else
                endIndex += includeOccurrence ? 1 : 0;

            return source[startIndex..endIndex];
        }

        /// <summary>
        /// Returns the substring until the given <paramref name="occurrence"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="occurrence">The occurrence to search.</param>
        /// <param name="includeOccurrence">Whether include the occurrence or not.</param>
        /// <param name="returnEmptyIfNotFound">Whether to return an empty string if the occurrence could not be found.</param>
        /// <returns></returns>
        [return: NotNullIfNotNull("source")]
        public static string? SubstringAfter(this string? source, char occurrence, bool includeOccurrence = false, bool returnEmptyIfNotFound = false)
        {
            if (source.IsNullOrEmpty()) return source;

            var index = source.IndexOf(occurrence, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1)
                return returnEmptyIfNotFound ? string.Empty : source;

            return source[(includeOccurrence ? index : index + 1)..];
        }

        /// <summary>
        /// Returns the substring until the given <paramref name="occurrence"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="occurrence">The occurrence to search.</param>
        /// <param name="includeOccurrence">Whether include the occurrence or not.</param>
        /// <param name="returnEmptyIfNotFound">Whether to return an empty string if the occurrence could not be found.</param>
        /// <returns></returns>
        [return: NotNullIfNotNull("source")]
        public static string? SubstringAfter(this string? source, string occurrence, bool includeOccurrence = false, bool returnEmptyIfNotFound = false)
        {
            if (source.IsNullOrEmpty()) return source;

            var index = source.IndexOf(occurrence, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1)
                return returnEmptyIfNotFound ? string.Empty : source;

            return source[(includeOccurrence ? index : index + occurrence.Length)..];
        }

        /// <summary>
        /// Returns the substring until the given <paramref name="occurrence"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="occurrence">The occurrence to search.</param>
        /// <param name="includeOccurrence">Whether include the occurrence or not.</param>
        /// <param name="returnEmptyIfNotFound">Whether to return an empty string if the occurrence could not be found.</param>
        /// <returns></returns>
        [return: NotNullIfNotNull("source")]
        public static string? SubstringBeforeLast(this string? source, string occurrence, bool includeOccurrence = false, bool returnEmptyIfNotFound = true)
        {
            if (source.IsNullOrEmpty()) return source;

            var index = source.LastIndexOf(occurrence, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1)
                return returnEmptyIfNotFound ? string.Empty : source;

            var length = includeOccurrence ? index + occurrence.Length : index;
            return source.Substring(0, length);
        }

        /// <summary>
        /// Returns the substring until the given <paramref name="occurrence"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="occurrence">The occurrence to search.</param>
        /// <param name="includeOccurrence">Whether include the occurrence or not.</param>
        /// <param name="returnEmptyIfNotFound">Whether to return an empty string if the occurrence could not be found.</param>
        /// <returns></returns>
        [return: NotNullIfNotNull("source")]
        public static string? SubstringBeforeLast(this string? source, char occurrence, bool includeOccurrence = false, bool returnEmptyIfNotFound = false)
        {
            if (source.IsNullOrEmpty()) return source;

            var index = source.LastIndexOf(occurrence);
            if (index == -1)
                return returnEmptyIfNotFound ? string.Empty : source;

            var length = includeOccurrence ? index + 1 : index;
            return source.Substring(0, length);
        }

        /// <summary>
        /// Returns the substring after the given <paramref name="occurrence"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="occurrence">The occurrence to search.</param>
        /// <param name="includeOccurrence">Whether include the occurrence or not.</param>
        /// <param name="returnEmptyIfNotFound">Whether to return an empty string if the occurrence could not be found.</param>
        /// <returns></returns>
        [return: NotNullIfNotNull("source")]
        public static string? SubstringAfterLast(this string? source, string occurrence, bool includeOccurrence = false, bool returnEmptyIfNotFound = true)
        {
            if (source.IsNullOrEmpty()) return source;

            var index = source.LastIndexOf(occurrence, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1)
                return returnEmptyIfNotFound ? string.Empty : source;

            return source[(includeOccurrence ? index : index + occurrence.Length)..];
        }

        /// <summary>
        /// Returns the substring after the given <paramref name="occurrence"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="occurrence">The occurrence to search.</param>
        /// <param name="includeOccurrence">Whether include the occurrence or not.</param>
        /// <param name="returnEmptyIfNotFound">Whether to return an empty string if the occurrence could not be found.</param>
        /// <returns></returns>
        [return: NotNullIfNotNull("source")]
        public static string? SubstringAfterLast(this string? source, char occurrence, bool includeOccurrence = false, bool returnEmptyIfNotFound = false)
        {
            if (source.IsNullOrEmpty()) return source;

            var index = source.LastIndexOf(occurrence);
            if (index == -1)
                return returnEmptyIfNotFound ? string.Empty : source;

            return source[(includeOccurrence ? index : index + 1)..];
        }

        [return: NotNull]
        public static string Quote(this string? str, string quoteCharacter) => $"{quoteCharacter}{str}{quoteCharacter}";
        [return: NotNull]
        public static string Quote(this string? str, char quoteCharacter = '"') => $"{quoteCharacter}{str}{quoteCharacter}";

        public static Guid ToGuid(this string? str)
        {
            if (str.IsNullOrEmpty())
                return Guid.Empty;

            Guid.TryParse(str, out var guid);

            return guid;
        }

        [Pure]
        public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? str) => !string.IsNullOrEmpty(str);
        [Pure]
        public static bool IsNotNullOrWhiteSpace([NotNullWhen(true)] this string? str) => !string.IsNullOrWhiteSpace(str);
        [Pure]
        public static bool IsNotNullOrEmpty([NotNullWhen(true)] this StringValues? str) => !string.IsNullOrEmpty(str);
        [Pure]
        public static bool IsNullOrEmpty([NotNullWhen(false)] this StringValues? str) => string.IsNullOrEmpty(str);
        [Pure]
        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str) => string.IsNullOrEmpty(str);
        [Pure]
        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str) => string.IsNullOrWhiteSpace(str);

        public static string InsertArgs(this string str, CultureInfo culture, params object[] args) => string.Format(culture, str, args);

        public static string InsertArgs(this string str, params object?[] args)
        {
            if (str.IsNullOrEmpty()) return str;
            if (args.Length == 0) return str;
            if (!str.Contains("}") || !str.Contains("{")) return str;

            return string.Format(str, args);
        }

        /// <param name="source">The string to truncate</param>
        /// <param name="maxLength">If greater than zero, the string will be truncated at specified length -1 and elipsis appended if longer than maxLengh</param>
        /// <param name="addElipsis">Whether the last allowed character will be replaced by an eplipsis, if text is longer than maxlength.</param>
        /// <returns></returns>
        public static string? TruncateAt(this string? source, int maxLength, bool addElipsis = true)
        {
            if (maxLength <= 0 || source.IsNullOrEmpty() || source.Length <= maxLength) return source;

            if (addElipsis)
                return source.Substring(0, maxLength - 1) + Constants.ElipsisCharacter;

            return source.Substring(0, maxLength);
        }

        public static string TrimEndAt([NotNull] this string source, string value) =>
            !source.Contains(value)
                ? source
                : source.Remove(source.LastIndexOf(value, StringComparison.Ordinal));

        public static string TrimEnd(this string source, string value) =>
            !source.EndsWith(value)
                ? source
                : source.Remove(source.LastIndexOf(value, StringComparison.Ordinal));

        public static string TrimStart([NotNull] this string source, string value) =>
            !source.StartsWith(value)
                ? source
                : source[value.Length..];

        public static bool Contains(this string? source, string toCheck, StringComparison comp) => source?.IndexOf(toCheck, comp) >= 0;

        public static string Combine(this string url1, string url2)
        {
            if (url1.IsNullOrEmpty())
                return url2;

            if (url2.IsNullOrEmpty())
                return url1;

            url1 = url1.TrimEnd('/', '\\');
            url2 = url2.TrimStart('/', '\\');

            return $"{url1}/{url2}";
        }

        public static Stream ToStream(this string str, Encoding? enc = null)
        {
            str.ThrowIfNull(nameof(str));

            enc ??= Encoding.UTF8;
            return new MemoryStream(enc.GetBytes(str));
        }

        public static bool EqualsInsensitive(this string text, string compareValue) => string.Equals(text, compareValue, StringComparison.OrdinalIgnoreCase);
        public static bool ContainsInsensitive(this string text, string containValue) => text.Contains(containValue, StringComparison.OrdinalIgnoreCase);

        public static bool IsBase64(this string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length % 4 != 0
                || value.Contains(" ") || value.Contains("\t") || value.Contains("\r") || value.Contains("\n"))
                return false;
            var index = value.Length - 1;
            if (value[index] == '=')
                index--;
            if (value[index] == '=')
                index--;
            for (var i = 0; i <= index; i++)
                if (IsInvalid(value[i]))
                    return false;
            return true;
        }

        public static string Repeat(this string input, int count)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var builder = new StringBuilder(input.Length * count);

            for (var i = 0; i < count; i++)
                builder.Append(input);

            return builder.ToString();

        }

        public static string ToBase64(this string input) => Convert.ToBase64String(Encoding.ASCII.GetBytes(input));
        public static string FromBase64(this string input) => Encoding.ASCII.GetString(Convert.FromBase64String(input));

        // Make it private as there is the name makes no sense for an outside caller
        private static bool IsInvalid(char value)
        {
            var intValue = (int)value;
            if (intValue >= 48 && intValue <= 57)
                return false;
            if (intValue >= 65 && intValue <= 90)
                return false;
            if (intValue >= 97 && intValue <= 122)
                return false;
            return intValue != 43 && intValue != 47;
        }

        public static string FormatJson(this string json)
        {
            if (json.IsNullOrEmpty()) return json;

            try
            {
                var t = json.FromJson<object>()!;
                return t.ToJson(new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception)
            {
                return json;
            }
        }

        public static T? FromJson<T>(this string json, JsonSerializerOptions? options = default)
        {
            var data = options == default ? JsonSerializer.Deserialize<T>(json) : JsonSerializer.Deserialize<T>(json, options);

            return data;
        }
    }
}
