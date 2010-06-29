using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Cheeeeeeeeese.Util
{
    public class Crypto
    {
        public static byte[] SHA256(string str)
        {
            SHA256 sha = new SHA256Managed();
            sha.Initialize();
            //return sha.ComputeHash(str.ToByteArray());
            return sha.ComputeHash(Encoding.UTF8.GetBytes(str));
        }
        public static string SHA256String(string str)
        {
            /*
            StringBuilder sb = new StringBuilder();
            foreach (byte b in SHA256(str))
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();*/

            return SHA256(str).ToHexString().ToLower();

            //return Convert.ToBase64String(SHA256(str));
        }
    }
}
