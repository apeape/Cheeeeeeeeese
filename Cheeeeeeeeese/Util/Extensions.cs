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
        public static List<string> SplitStrings(this IEnumerable<byte> bytes, byte delimiter, int len)
        {
            return SplitStrings(bytes.ToArray(), delimiter, len);
        }

        public static List<string> SplitStrings(this byte[] bytes, byte delimiter, int len)
        {
            List<string> strings = new List<string>();
            var chunks = SplitBytes(bytes, delimiter, len);

            chunks.ForEach(chunk => strings.Add(chunk.ToUTF8()));

            return strings;
        }

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

                while (reader.BaseStream.Position < len)
                {
                    // add non-delimiter bytes to chunk
                    if ((b = reader.ReadByte()) != delimiter)
                        chunk.Add(b);
                    else // hit delimiter, add chunk to list
                    {
                        chunks.Add(chunk.ToArray());
                        chunk = new List<Byte>();
                    }
                    if (reader.BaseStream.Position == len && chunk.Count > 0)
                    {
                        chunks.Add(chunk.ToArray()); // hit the end without finding the delimiter, add last chunk
                    }
                }
            }
            return chunks;
        }

        public static byte[] ToByteArray(this string str)
        {
            var res = Encoding.UTF8.GetBytes(str).ToList();
            //res.Add(0);
            return res.ToArray();
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
