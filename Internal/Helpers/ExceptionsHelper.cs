using System;
using Common.Shared.Extensions;
using Common.Shared.Helpers;

namespace Common.Shared.Internal.Helpers
{
    internal static class ExceptionsHelper
    {
        public static Exception ArgumentNull(string argName, string? customErrorText = null) => new ArgumentNullException(argName, customErrorText ?? Requires.Strings.ArgumentMustNotBeNull);

        public static Exception MaxFileSizeExceeded<T>(T actualSize, T maxSize, string argName) where T : struct, IConvertible, IComparable, IComparable<T>
        {
            var newMessage = Requires.Strings.ArgumentMaxFileSizeExceededSizeTemplate.InsertArgs(actualSize.FormatBinary(), maxSize.FormatBinary());

            return new ArgumentException(newMessage, argName);
        }

        public static Exception ArgumentDefault(string argName, string? customErrorText = null) => new ArgumentException(customErrorText ?? Requires.Strings.ArgumentMustNotBeDefault, argName);

        public static Exception InvalidOperation(string message) => new InvalidOperationException(message);
    }
}