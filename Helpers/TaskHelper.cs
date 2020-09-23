using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Shared.Helpers
{
    public static class TaskHelper
    {
        public static void RunInNewThread(Action action, CancellationToken token = default) => Task.Run(action, token);
        public static void RunInNewThread(Func<Task> action, CancellationToken token = default) => Task.Run(action, token);
    }
}
