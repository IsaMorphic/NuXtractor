using MightyStruct;
using MightyStruct.Serializers;

using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Formats.V1
{
    public enum VertexAttribute
    {
        X = 0,
        Y = 4,
        Z = 8,
        U = 28,
        V = 32,
        Color = 24
    }

    public class VertexStream
    {
        private const uint STRIDE = 32;

        private static ISerializer<int> DWords { get; } =
            new SInt32Serializer(Endianness.LittleEndian);

        private Stream Stream { get; }

        public int Count { get; }

        public VertexStream(Stream stream)
        {
            Stream = stream;
            Count = (int)(Stream.Length / (STRIDE + 4));
        }

        public async Task<int[]> GetAttributeArray(VertexAttribute attr)
        {
            var attrs = new int[Count];

            Stream.Seek((long)attr, SeekOrigin.Begin);
            for (int i = 0; i < attrs.Length; i++)
            {
                attrs[i] = await DWords.ReadFromStreamAsync(Stream);
                Stream.Seek(STRIDE, SeekOrigin.Current);
            }

            return attrs;
        }
    }
}
