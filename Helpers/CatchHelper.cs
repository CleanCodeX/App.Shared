using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Common.Shared.Extensions;

// ReSharper disable UnusedParameter.Global
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable MemberCanBePrivate.Global

namespace Common.Shared.Helpers
{
    [SuppressMessage("Stil", "IDE0060:Nicht verwendete Parameter entfernen")]
    public static class CatchHelper
    {
        public static bool CatchErrorBool([NotNull] this SyncLock syncLock, Action func)
            => syncLock.Lock(() => CatchErrorBool(func));
        public static bool CatchErrorBool([NotNull] this object source, Action func)
            => CatchErrorBool(func);
        public static bool CatchErrorBool(Action func)
            => CatchErrorBool(func, null);

        public static bool CatchErrorBool([NotNull] this SyncLock syncLock, Action func, Action<Exception> onError)
            => syncLock.Lock(() => CatchErrorBool(func, onError));
        public static bool CatchErrorBool([NotNull] this object source, Action func, Action<Exception> onError)
            => CatchErrorBool(func, onError);

        public static bool CatchErrorBool(Action func, Action<Exception>? onError)
        {
            try
            {
                func();
                return true;
            }
            catch (Exception ex)
            {
                InternalCatchError(onError, ex);

                return default;
            }
        }

        public static void CatchError([NotNull] this SyncLock syncLock, Action func)
            => syncLock.Lock(() => CatchError(func));
        public static void CatchError([NotNull] this object source, Action func)
            => CatchError(func);
        public static void CatchError(Action func)
            => CatchError(func, null);


        public static void CatchError([NotNull] this SyncLock syncLock, Action func, Action<Exception> onError)
            => syncLock.Lock(() => CatchError(func, onError));
        public static void CatchError([NotNull] this object source, Action func, Action<Exception> onError)
            => CatchError(func, onError);
        public static void CatchError(Action func, Action<Exception>? onError)
        {
            try
            {
                func();
            }
            catch (Exception ex)
            {
                InternalCatchError(onError, ex);
            }
        }

        public static T CatchError<T>([NotNull] this object source, Func<T> func) => CatchError(func, null);
        public static T CatchError<T>(Func<T> func) => CatchError(func, null);
        public static T CatchError<T>([NotNull] this SyncLock syncLock, Func<T> func) => syncLock.Lock(() 
            => CatchError(func, null));
        public static T CatchError<T>([NotNull] this SyncLock syncLock, Func<T> func, Action<Exception> onError) 
            => syncLock.Lock(() => CatchError(func, onError));
        public static T CatchError<T>(Func<T> func, Action<Exception> onError)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException) return default!;

                InternalCatchError(onError, ex);

                return default!;
            }
        }

        public static Task<T> CatchErrorAsync<T>([NotNull] this object source, Func<Task<T>> func) => source.CatchErrorAsync(func, null);
        public static Task<T> CatchErrorAsync<T>([NotNull] this object source, Func<Task<T>> func,
            Action<Exception>? onError) => CatchErrorAsync(func, onError);
        public static async Task<T> CatchErrorAsync<T>(Func<Task<T>> func, Action<Exception>? onError)
        {
            try
            {
                return await func().KeepContext();
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException) return default!;

                InternalCatchError(onError, ex);

                return default!;
            }
        }

        public static Task CatchErrorAsync([NotNull] this object source, Func<Task> func) => source.CatchErrorAsync(func, null);
        public static Task CatchErrorAsync([NotNull] this object source, Func<Task> func, Action<Exception>? onError) => CatchErrorAsync(func, onError);
        public static async Task CatchErrorAsync(Func<Task> func, Action<Exception>? onError)
        {
            try
            {
                await func().KeepContext();
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException) return;

                InternalCatchError(onError, ex);
            }
        }

        public static Task CatchErrorAsync<TArg>([NotNull] this object source, Func<TArg, Task> func, TArg arg) => source.CatchErrorAsync(func, arg, null);
        public static Task CatchErrorAsync<TArg>([NotNull] this object source, Func<TArg, Task> func, TArg arg, Action<Exception>? onError) => CatchErrorAsync(func, arg, onError);
        public static async Task CatchErrorAsync<TArg>(Func<TArg, Task> func, TArg arg, Action<Exception>? onError)
        {
            try
            {
                await func(arg).KeepContext();
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException) return;

                InternalCatchError(onError, ex);
            }
        }

        private static void InternalCatchError(Action<Exception>? onError, Exception ex)
        {
            //Debug.Assert(false);

            if (onError is null) return;
            if (ex is ObjectDisposedException) return;

            if (ex is TargetInvocationException targetEx)
                ex = targetEx.InnerException!;

            onError(ex);
        }

        public static T CatchError<T>([NotNull] this SyncLock syncLock, Func<T> func, Func<Exception, T> onError)
            => syncLock.Lock(() => CatchError(func, onError));
        public static T CatchError<T>([NotNull] this object source, Func<T> func, Func<Exception, T> onError) => CatchError(func, onError);
        public static T CatchError<T>(Func<T> func, Func<Exception, T>? onError)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                return InternalCatchError(onError, ex)!;
            }
        }

        [return: MaybeNull]
        private static TReturn InternalCatchError<TReturn>(Func<Exception, TReturn>? onError, Exception ex)
        {
            //Debug.Assert(false);

            if (onError is null) return default!;

            return ex switch
            {
                ObjectDisposedException _ => default!,
                TargetInvocationException targetEx => onError(targetEx.InnerException!),
                _ => onError(ex)
            };
        }
    }
}
