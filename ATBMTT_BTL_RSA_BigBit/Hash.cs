using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSystem.Security.Cryptography;

namespace ATBMTT_BTL_RSA_BigBit
{
    class Hash
    {
        public static byte[] MD5Decimal(string path)
        {
            using (MD5CryptoServiceProvider cryptHandler = new MD5CryptoServiceProvider())
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    return cryptHandler.ComputeHash(stream);
                }
            }
        }

        public static string MD5Hexadecimal(string path)
        {
            using (MD5CryptoServiceProvider cryptHandler = new MD5CryptoServiceProvider())
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    return BitConverter.ToString(cryptHandler.ComputeHash(stream));
                }
            }
        }

        public static string Base64Encode(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
