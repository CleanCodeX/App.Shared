using Microsoft.Extensions.Logging;
using Res = Common.Shared.Properties.Resources;

namespace Common.Shared.Extensions
{
    public static class LogLevelExtensions
    {
        public static string? ToDisplayName(this LogLevel enumeration, bool returnDefault = true)
        {
            var result = enumeration switch
            {
                LogLevel.Trace => Res.Trace,
                LogLevel.Debug => Res.Debug,
                LogLevel.Information => Res.Info,
                LogLevel.Warning => Res.Warning,
                LogLevel.Error => Res.Error,
                LogLevel.Critical => Res.Critical,
                _ => null
            };

            return returnDefault ? result : null;
        }
    }
}
