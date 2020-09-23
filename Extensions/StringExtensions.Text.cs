using System;
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
    }
}