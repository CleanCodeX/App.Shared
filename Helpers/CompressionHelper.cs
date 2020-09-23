using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Common.Shared.Helpers
{
    public static class CompressionHelper
    {
        public static byte[] Compress(this string text)
        {
            var bytes = Encoding.Unicode.GetBytes(text);
            using var mso = new MemoryStream();
            using (var gs = new GZipStream(mso, CompressionMode.Compress))
                gs.Write(bytes, 0, bytes.Length);

            return mso.ToArray();
        }

        public static string? Decompress(this byte[]? data)
        {
            if (data is null || data.Length == 0) return null;

            // Read the last 4 bytes to get the length
            var lengthBuffer = new byte[4];
            Array.Copy(data, data.Length - 4, lengthBuffer, 0, 4);
            var uncompressedSize = BitConverter.ToInt32(lengthBuffer, 0);

            var buffer = new byte[uncompressedSize];
            using (var ms = new MemoryStream(data))
            using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                gzip.Read(buffer, 0, uncompressedSize);

            return Encoding.Unicode.GetString(buffer);
        }
    }
}
