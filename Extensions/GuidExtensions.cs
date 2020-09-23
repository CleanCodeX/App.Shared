using System;

namespace Common.Shared.Extensions
{
    public static class GuidExtensions
    {
        public static string? ToNullStringIfEmpty(this Guid guid) => guid != Guid.Empty ? guid.ToString() : null;

        public static string Quote(this Guid guid, string quoteCharacter = "'") => $"{quoteCharacter}{guid}{quoteCharacter}";
    }
}
