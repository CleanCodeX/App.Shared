using System;
using System.Runtime.ExceptionServices;

namespace Common.Shared.Extensions
{
    public static class ExceptionExtensions
    {
        public static string Format(this Exception source, bool addStackTrace = false)
        {
            var text = source.Message;

            var baseEx = source.GetBaseException();
            if (baseEx != source)
                text += $" | Base-Exception: {baseEx.Message}";

            if (addStackTrace)
                text += $" | {source.StackTrace}";
            
            return text;
        }

        public static void Rethrow(this Exception source) => ExceptionDispatchInfo.Throw(source);

        public static T CreateExceptionOfSameType<T>(this T innerException, string message)
            where T : Exception =>
            (T)Activator.CreateInstance(typeof(T), message, innerException)!;

        public static Exception CreateExceptionOfSameType(this Exception innerException, string message) => (Exception)Activator.CreateInstance(innerException.GetType(), message, innerException)!;
    }
}
