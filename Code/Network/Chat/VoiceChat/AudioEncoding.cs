using System;
using System.IO;
using System.IO.Compression;

namespace GrabCoin.Services.Chat.VoiceChat
{
    public class AudioEncoding
    {
        private const byte floatSize = sizeof(float); 

        public byte[] Encode(float[] audioData)
        {
           return Compress(EncodeToByteArray(audioData));
        }

        public float[] Decode(byte[] bytes)
        {
            return DecodeToFloatArray(Decompress(bytes));
        }

        private static byte[] Compress(byte[] src)
        {
            using (var ms = new MemoryStream())
            {
                using (var ds = new DeflateStream(ms, CompressionMode.Compress, true/*msは*/))
                {
                    ds.Write(src, 0, src.Length);
                }

                ms.Position = 0;
                byte[] comp = new byte[ms.Length];
                ms.Read(comp, 0, comp.Length);
                return comp;
            }
        }

        private static byte[] Decompress(byte[] src)
        {
            using (var ms = new MemoryStream(src))
            using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
            {
                using (var dest = new MemoryStream())
                {
                    ds.CopyTo(dest);

                    dest.Position = 0;
                    byte[] decomp = new byte[dest.Length];
                    dest.Read(decomp, 0, decomp.Length);
                    return decomp;
                }
            }
        }

        private static float[] DecodeToFloatArray(byte[] byteArray)
        {
            int len = byteArray.Length / floatSize;
            float[] floatArray = new float[len];
            for (int i = 0; i < byteArray.Length; i += floatSize)
            {
                floatArray[i / floatSize] = System.BitConverter.ToSingle(byteArray, i);
            }
            return floatArray;
        }

        private static byte[] EncodeToByteArray(float[] floatArray)
        {
            byte[] byteData = new byte[floatArray.Length * floatSize];
            Buffer.BlockCopy(floatArray, 0, byteData, 0, byteData.Length);
            return byteData;
        }
    }
}
