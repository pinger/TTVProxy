using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptoLibrary
{
    public class CryptoHelper
    {
        public static string Encrypt<T>(string value, string password, string salt) where T : SymmetricAlgorithm, new()
        {
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));
            SymmetricAlgorithm symmetricAlgorithm = (SymmetricAlgorithm)Activator.CreateInstance<T>();
            byte[] bytes1 = rfc2898DeriveBytes.GetBytes(symmetricAlgorithm.KeySize >> 3);
            byte[] bytes2 = rfc2898DeriveBytes.GetBytes(symmetricAlgorithm.BlockSize >> 3);
            ICryptoTransform encryptor = symmetricAlgorithm.CreateEncryptor(bytes1, bytes2);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream, Encoding.Unicode))
                        streamWriter.Write(value);
                }
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public static string Decrypt<T>(string text, string password, string salt) where T : SymmetricAlgorithm, new()
        {
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));
            SymmetricAlgorithm symmetricAlgorithm = (SymmetricAlgorithm)Activator.CreateInstance<T>();
            byte[] bytes1 = rfc2898DeriveBytes.GetBytes(symmetricAlgorithm.KeySize >> 3);
            byte[] bytes2 = rfc2898DeriveBytes.GetBytes(symmetricAlgorithm.BlockSize >> 3);
            ICryptoTransform decryptor = symmetricAlgorithm.CreateDecryptor(bytes1, bytes2);
            using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader((Stream)cryptoStream, Encoding.Unicode))
                        return streamReader.ReadToEnd();
                }
            }
        }

        public static string ComputeHash<T>(string text) where T : HashAlgorithm, new()
        {
            return Convert.ToBase64String(Activator.CreateInstance<T>().ComputeHash(Encoding.Unicode.GetBytes(text)));
        }
    }
}