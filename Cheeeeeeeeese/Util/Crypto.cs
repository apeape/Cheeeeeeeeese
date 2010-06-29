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
            return sha.ComputeHash(str.ToByteArray());
        }
        public static string SHA256String(string str)
        {
            SHA256 sha = new SHA256Managed();
            return sha.ComputeHash(str.ToByteArray()).ToHexString();
        }
    }
}
