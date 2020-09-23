using System.Security.Cryptography;
using System.Text;

namespace Common.Shared.Extensions
{
    public static partial class StringExtensions
    {
        public static string Md5Hash(this string text)
        {
            // Step 1, calculate MD5 hash from input
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(text);
            var hashBytes = md5.ComputeHash(inputBytes);

            // Step 2, convert byte array to hex string
            var sb = new StringBuilder();
            foreach (var @byte in hashBytes)
                sb.Append(@byte.ToString("X2"));
            
            return sb.ToString();
        }
    }
}
