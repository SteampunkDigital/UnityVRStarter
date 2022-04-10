using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MidiParser
{
    internal class Reader
    {
        public static int Read16(byte[] data, ref int i)
        {
            return (data[i++] << 8) | data[i++];
        }

        public static int Read32(byte[] data, ref int i)
        {
            return (data[i++] << 24) | (data[i++] << 16) | (data[i++] << 8) | data[i++];
        }

        public static byte Read8(byte[] data, ref int i)
        {
            return data[i++];
        }

        public static byte[] ReadAllBytesFromStream(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }

        public static string ReadString(byte[] data, ref int i, int length)
        {
            var result = Encoding.ASCII.GetString(data, i, length);
            i += length;
            return result;
        }

        public static int ReadVarInt(byte[] data, ref int i)
        {
            var result = (int)data[i++];

            if ((result & 0x80) == 0)
            {
                return result;
            }

            result &= 0x7F;

            for (var j = 0; j < 3; j++)
            {
                var value = (int)data[i++];

                result = (result << 7) | (value & 0x7F);

                if ((value & 0x80) == 0)
                {
                    break;
                }
            }

            return result;
        }
    }
}
