using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Common.Shared.Extensions.Enumerables.Specialized;

namespace Common.Shared.Extensions
{
    public static partial class StringExtensions
    {
        public static string LineBreak(this string text) => $"{text}{Environment.NewLine}";

        public static string LineBreakAtBeginning(this string text) => EnumerableExtensions.IsNullOrEmpty(text)
            ? string.Empty
            : $"{Environment.NewLine}{text}";

        public static string Paragraph(this string text) => text.LineBreak().LineBreak();

        public static string ParagraphAtBeginning(this string text) => EnumerableExtensions.IsNullOrEmpty(text)
            ? string.Empty
            : text.LineBreakAtBeginning().LineBreakAtBeginning();

        public static string? RemoveQuotes(this string? source) => source!.IsNullOrEmpty()
            ? source
            : source!.Replace(@"""", string.Empty);

        public static string? EscapeSpaces(this string? source) =>
            source!.IsNullOrEmpty()
                ? source
                : source!.Replace(" ", "%20");

        public static string? RemoveSingleQuotes(this string? source) =>
            source!.IsNullOrEmpty()
                ? source
                : source!.Replace(@"'", string.Empty);

        public static string ToBase64(this string input) => Convert.ToBase64String(Encoding.ASCII.GetBytes(input));
        public static string FromBase64(this string input) => Encoding.ASCII.GetString(Convert.FromBase64String(input));

        [return: NotNull]
        public static string Quote(this string? str, string quoteCharacter) => $"{quoteCharacter}{str}{quoteCharacter}";
        [return: NotNull]
        public static string Quote(this string? str, char quoteCharacter = '"') => $"{quoteCharacter}{str}{quoteCharacter}";

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

        private static readonly int _charSize = sizeof(char);

        public static unsafe byte[] GetBytes(string str)
        {
            str.ThrowIfNull(nameof(str));
            if (str.Length == 0) return Array.Empty<byte>();

            fixed (char* p = str)
                return new Span<byte>(p, str.Length * _charSize).ToArray();
        }

        public static unsafe string GetString(byte[] bytes)
        {
            bytes.ThrowIfNull(nameof(bytes));
            if (bytes.Length % _charSize != 0) throw new ArgumentException($"Invalid {nameof(bytes)} length");
            if (bytes.Length == 0) return string.Empty;

            fixed (byte* p = bytes)
                return new string(new Span<char>(p, bytes.Length / _charSize));
        }
    }
}