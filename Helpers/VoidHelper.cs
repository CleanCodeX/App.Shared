using System;
using System.Diagnostics.CodeAnalysis;

namespace Common.Shared.Helpers
{
    [SuppressMessage("Stil", "IDE0060:Nicht verwendete Parameter entfernen")]
    public static class VoidHelper
    {
        // ReSharper disable once UnusedParameter.Global
        public static void RunVoid([NotNull] this object source, bool condition, Action action) =>
            RunVoid(condition, action);

        public static void RunVoid(bool condition, Action action)
        {
            if (condition)
                action();
        }

        // ReSharper disable once UnusedParameter.Global
        public static void RunVoid<T>([NotNull] this object source, bool condition, Action<T> action, T arg) =>
            RunVoid(condition, action, arg);

        public static void RunVoid<T>(bool condition, Action<T> action, T arg)
        {
            if (condition)
                action(arg);
        }
    }
}