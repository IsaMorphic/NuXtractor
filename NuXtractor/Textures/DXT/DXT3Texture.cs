using MightyStruct;
using MightyStruct.Serializers;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuXtractor.Textures.DXT
{
    public class DXT3Texture : DXT1Texture
    {
        private ISerializer<byte> Bytes { get; }

        public DXT3Texture(int width, int height, int levels, Endianness endianness, Stream stream) : base(width, height, levels, endianness, stream)
        {
            Bytes = new UInt8Serializer();
        }

        private async Task<float[]> ReadAlphaAsync()
        {
            float[] alphas = new float[16];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j += 2)
                {
                    var pair = await Bytes.ReadFromStreamAsync(Stream);

                    var left = pair & 0x0F;
                    var right = (pair & 0xF0) >> 4;

                    alphas[i * 4 + 3 - (j + 0)] = left / 15.0f;
                    alphas[i * 4 + 3 - (j + 1)] = right / 15.0f;
                }
            }

            return alphas;
        }

        protected override async Task<RgbaVector[]> ReadTileAsync()
        {
            float[] alphas = await ReadAlphaAsync();
            RgbaVector[] tile = await base.ReadTileAsync();

            return tile
                .Select((c, i) => new RgbaVector(c.R, c.G, c.B, alphas[i]))
                .ToArray();
        }

        private async Task WriteAlphaAsync(float[] alphas)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j += 2)
                {
                    var left = (byte)Math.Clamp(alphas[i * 4 + 3 - (j + 0)] * 15.0f, 0.0f, 15.0f);
                    var right = (byte)Math.Clamp(alphas[i * 4 + 3 - (j + 1)] * 15.0f, 0.0f, 15.0f);

                    var pair = (byte)(left | (right << 4));
                    await Bytes.WriteToStreamAsync(Stream, pair);
                }
            }
        }

        protected override async Task WriteTileAsync(RgbaVector[] tile)
        {
            float[] alphas = tile.Select(c => c.A).ToArray();
            await WriteAlphaAsync(alphas);

            var noAlpha = tile
                .Select(c => new RgbaVector(c.R, c.G, c.B))
                .ToArray();
            await base.WriteTileAsync(noAlpha);
        }
    }
}
