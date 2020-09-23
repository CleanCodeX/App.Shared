using System;
using System.Threading.Tasks;

namespace Common.Shared.Helpers
{
    public static class WaitHelper
    {
        public static void WaitIfNonZero(int waitMilliseconds)
        {
            if (waitMilliseconds > 0)
                Task.Delay(waitMilliseconds).Wait();
        }

        public static void WaitRandomTime(int maxMilliseconds) => WaitRandomTime(10, maxMilliseconds);
        public static void WaitRandomTime(int minMilliseconds, int maxMilliseconds)
        {
            var waitMilliseconds = new Random(DateTimeHelper.TimeOfToday.Milliseconds).Next(minMilliseconds, maxMilliseconds);

            Task.Delay(waitMilliseconds).Wait();
        }
    }
}
