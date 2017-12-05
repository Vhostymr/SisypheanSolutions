using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SisypheanSolutions.Utilities
{
    public class EncryptionExtensions
    {
        /// <summary>
        /// Encrypts a file from a given byte[] with the provided password.
        /// </summary>
        /// <param name="bytesToBeEncrypted"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static byte[] EncryptFile(byte[] bytesToBeEncrypted, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            return AES_Encrypt(bytesToBeEncrypted, passwordBytes);
        }

        /// <summary>
        /// Decrypts a file from a given byte[] with the provided password.
        /// </summary>
        /// <param name="bytesToBeDecrypted"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static byte[] DecryptFile(byte[] bytesToBeDecrypted, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            return AES_Decrypt(bytesToBeDecrypted, passwordBytes);
        }

        /// <summary>
        /// Encrypts a string with the given password.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static string EncryptString(string input, string password)
        {
            // Get the bytes of the string
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            string encryptedString = Convert.ToBase64String(bytesEncrypted);

            return encryptedString;
        }

        /// <summary>
        /// Decrypts a string with the appropriate password.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static string DecryptString(string input, string password)
        {
            // Get the bytes of the string
            byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            string decryptedString = Encoding.UTF8.GetString(bytesDecrypted);

            return decryptedString;
        }

        /// <summary>
        /// Encrypts byte[] with 256 AES Encryption.
        /// </summary>
        /// <param name="bytesToBeEncrypted"></param>
        /// <param name="passwordBytes"></param>
        /// <returns></returns>
        private static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes;
            byte[] saltBytes = GetRandomBytes();

            using (MemoryStream memorystream = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cryptostream = new CryptoStream(memorystream, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptostream.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cryptostream.Close();
                    }

                    encryptedBytes = memorystream.ToArray();
                }
            }

            return encryptedBytes;
        }

        /// <summary>
        /// Decrypts AES encrypted byte[].
        /// </summary>
        /// <param name="bytesToBeDecrypted"></param>
        /// <param name="passwordBytes"></param>
        /// <returns></returns>
        private static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes;
            byte[] saltBytes = GetRandomBytes();

            using (MemoryStream memorystream = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cryptostream = new CryptoStream(memorystream, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptostream.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cryptostream.Close();
                    }

                    decryptedBytes = memorystream.ToArray();
                }
            }

            return decryptedBytes;
        }

        private static byte[] GetRandomBytes()
        {
            //int size = 16;
            //byte[] bytes = new byte[size];

            //RNGCryptoServiceProvider.Create().GetBytes(bytes);

            //Not Random Currently.
            byte[] bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8 };

            return bytes;
        }


        /// <summary>
        /// UTF8 encodes plain text.
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        protected internal static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Converts base 64 encoded data to UTF8 string.
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        protected internal static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}