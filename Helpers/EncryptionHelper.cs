using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common.Shared.Helpers
{
    public class EncryptionHelper
    {
        /// The salt value used to strengthen the encryption.
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionHelper(string encryptionPassphrase, byte[] encryptionSalt)
        {
            using var keyGenerator = new Rfc2898DeriveBytes(encryptionPassphrase, encryptionSalt);
            _key = keyGenerator.GetBytes(32);
            _iv = keyGenerator.GetBytes(16);
        }

        public string Encrypt(string inputText)
        {
            if (string.IsNullOrEmpty(inputText)) return inputText;

            //Create a new RijndaelManaged cipher for the symmetric algorithm from the key and iv
            using var rijndaelCipher = new RijndaelManaged { Key = _key, IV = _iv };

            var plainText = Encoding.Unicode.GetBytes(inputText);

            using var encryptor = rijndaelCipher.CreateEncryptor();
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainText, 0, plainText.Length);
            cryptoStream.FlushFinalBlock();

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public string Decrypt(string inputText)
        {
            if (string.IsNullOrEmpty(inputText)) return inputText;

            var rijndaelCipher = new RijndaelManaged();
            var encryptedData = Convert.FromBase64String(inputText);

            using var decryptor = rijndaelCipher.CreateDecryptor(_key, _iv);
            using var memoryStream = new MemoryStream(encryptedData);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            var plainText = new byte[encryptedData.Length];
            var decryptedCount = cryptoStream.Read(plainText, 0, plainText.Length);

            return Encoding.Unicode.GetString(plainText, 0, decryptedCount);
        }
    }
}