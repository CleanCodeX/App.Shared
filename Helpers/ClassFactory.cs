using System;

namespace Common.Shared.Helpers
{
    public static class ClassFactory
    {
        public static T Create<T>(params object?[] args) =>
            Create<T>(typeof(T), args);

        public static T Create<T>(Type type, params object?[] args) =>
            args.Length == 0
                ? (T)Activator.CreateInstance(type)!
                : (T)Activator.CreateInstance(type, args)!;
    }
}