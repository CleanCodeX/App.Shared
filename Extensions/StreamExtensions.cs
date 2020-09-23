using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace Common.Shared.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<MemoryStream> CopyAsMemoryStreamAsync([NotNull] this Stream stream)
        {
            var buffer = new byte[stream.Length];
            var ms = new MemoryStream(buffer);

            await stream.CopyToAsync(ms).KeepContext();

            ms.Position = 0;

            return ms;
        }
    }
}
