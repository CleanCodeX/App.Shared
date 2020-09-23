using System.Diagnostics.CodeAnalysis;
using Common.Shared.Helpers;
using Res = Common.Shared.Properties.Resources;

namespace Common.Shared.Extensions
{
    public static class BoolExtensions
    {
        public static bool ToBool(this bool? value) => value ?? false;
        public static string ToLowerString(this bool value) => value.ToString().ToLower();

        public static string Format(this bool value) => value ? Res.Yes : Res.No;
        public static string Format(this bool? value) =>
            value switch
            {
                null => string.Empty,
                _ => value.Value.Format()
            };

        public static void ThrowIfFalse([DoesNotReturnIf(false)] this bool source, string argName, string? customErrorText = null)
            => Requires.NotFalse(source, argName, customErrorText);

        public static void ThrowIfTrue([DoesNotReturnIf(true)] this bool source, string argName, string? customErrorText = null)
            => Requires.NotTrue(source, argName, customErrorText);
    }
}
