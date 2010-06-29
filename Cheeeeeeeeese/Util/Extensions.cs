using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Collections;

namespace Cheeeeeeeeese.Util
{
    public static class Extensions
    {
        public static List<byte[]> SplitBytes(this IEnumerable<byte> bytes, byte delimiter, int len)
        {
            return SplitBytes(bytes.ToArray(), delimiter, len);
        }
        public static List<byte[]> SplitBytes(this byte[] bytes, byte delimiter, int len)
        {
            var chunks = new List<byte[]>();

            using (MemoryStream stream = new MemoryStream(bytes, 0, len))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                var chunk = new List<Byte>();
                byte b;

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // add non-delimiter bytes to chunk
                    if ((b = reader.ReadByte()) != delimiter)
                        chunk.Add(b);
                    else // hit delimiter, add chunk to list
                    {
                        chunks.Add(chunk.ToArray());
                        chunk = new List<Byte>();
                    }
                }
            }
            return chunks;
        }

        public static byte[] ToByteArray(this string str)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            return encoding.GetBytes(str);
        }

        public static string ToHexString(this byte[] bytes)
        {
            SoapHexBinary hexBinary = new SoapHexBinary(bytes);
            return hexBinary.ToString();
        }

        public static string ToUTF8(this IEnumerable<byte> self)
        {
            return self.ToArray().ToUTF8();
        }

        public static string ToUTF8(this byte[] self)
        {
            // remove extra 0s
            return Encoding.UTF8.GetString(self.TakeWhile(b => b != 0).ToArray());
        }

        public static string ArrayToStringGeneric<T>(this IList<T> array, string delimeter)
        {
            string outputString = "";

            for (int i = 0; i < array.Count; i++)
            {
                if (array[i] is IList)
                {
                    //Recursively convert nested arrays to string
                    outputString += ArrayToStringGeneric<T>((IList<T>)array[i], delimeter);
                }
                else
                {
                    outputString += array[i];
                }

                if (i != array.Count - 1)
                    outputString += delimeter;
            }

            return outputString;
        }
    }
}
