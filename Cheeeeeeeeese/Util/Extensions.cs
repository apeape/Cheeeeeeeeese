using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cheeeeeeeeese.Util
{
    public static class Extensions
    {
        public static List<byte[]> SplitBytes(this byte[] bytes)
        {
            var chunks = new List<byte[]>();

            using (MemoryStream stream = new MemoryStream(bytes, 0, bytes.Length))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                var chunk = new List<Byte>();
                byte b;

                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // add non-0 bytes to chunk
                    if ((b = reader.ReadByte()) != 0)
                        chunk.Add(b);
                    else // hit a 0, add chunk to list
                    {
                        chunks.Add(chunk.ToArray());
                        chunk = new List<Byte>();
                    }
                }
            }
            return chunks;
        }
    }
}
