using MightyStruct;
using MightyStruct.Serializers;

using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Formats.V1
{
    public class ElementStream
    {
        private static ISerializer<ushort> Words { get; } =
            new UInt16Serializer(Endianness.LittleEndian);

        private Stream Stream { get; }

        public int Count { get; }

        public ElementStream(Stream stream)
        {
            Stream = stream;
            Count = (int)(stream.Length / 2);
        }

        public async Task<int[]> ReadElementsAsync()
        {
            var elements = new int[Count];
            Stream.Seek(0, SeekOrigin.Begin);

            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = await Words.ReadFromStreamAsync(Stream);
            }

            return elements;
        }
    }
}
