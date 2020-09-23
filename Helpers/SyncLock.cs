using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Common.Shared.Extensions;
using Res = Common.Shared.Properties.Resources;

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Common.Shared.Helpers
{
    
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class SyncLock
    {
        private static readonly ConcurrentDictionary<string, Lazy<SyncLock>> NamedSyncObjects = new ConcurrentDictionary<string, Lazy<SyncLock>>();

        internal static ISet<string> Blacklist = new HashSet<string>();
        internal static ISet<string> Whitelist = new HashSet<string>();

        public static bool Enabled = true;

        internal int LockCount;

        public string Name { get; }

        private SyncLock(string name) => Name = name;

        public void Lock(Action body) => LockInternal(this, body);
        public T Lock<T>(Func<T> body) => LockInternal(this, body);

        public static SyncLock Of<T>() where T : class => Of(typeof(T));
        public static SyncLock Of(object callerObj) => Of(callerObj.GetType());
        public static SyncLock Of(Type type) => Of(type.ToString());
        public static SyncLock Of(string name) => GetLockObj(name);
        public static IEnumerable<string> GetLockStatus()
        {
            yield return $"{{Lockname}}: {{{Res.Active}}} ({NamedSyncObjects.Count})";

            foreach (var syncObject in NamedSyncObjects.Values.Select(x => x.Value))
                yield return $"{syncObject.Name}: {ShouldLock(syncObject).Format()} ({syncObject.LockCount.Format()}x)";

            yield return string.Empty;
            yield return $"Whitelist: ({Whitelist.Count})";

            if (Whitelist.Count > 0)
                foreach (var name in Whitelist)
                    yield return name;
            else
                yield return Res.None;

            yield return string.Empty;
            yield return $"Blacklist: ({Blacklist.Count})";

            if (Blacklist.Count > 0)
                foreach (var name in Blacklist)
                    yield return name;
            else
                yield return Res.None;
        }

        private static void LockInternal(SyncLock lockObj, Action body)
        {
            if (!ShouldLock(lockObj))
            {
                body();
                return;
            }

            Interlocked.Increment(ref lockObj.LockCount);

            lock (lockObj)
                body();
        }

        private static T LockInternal<T>(SyncLock lockObj, Func<T> body)
        {
            if (!ShouldLock(lockObj))
                return body();

            Interlocked.Increment(ref lockObj.LockCount);

            lock (lockObj)
                return body();
        }

        private static bool ShouldLock(SyncLock lockObj)
        {
            var name = lockObj.Name;

            if (!Enabled && !Whitelist.Contains(name)) return false;
            if (Blacklist.Contains(name)) return false;

            return true;
        }

        private static SyncLock GetLockObj(string name)
        {
            Requires.NotNull(name, nameof(name));

            return NamedSyncObjects.GetOrAdd(name, new Lazy<SyncLock>(() => new SyncLock(name), true)).Value;
        }
    }
}