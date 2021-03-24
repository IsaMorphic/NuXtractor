using MightyStruct;
using MightyStruct.Serializers;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Textures.Indexed
{
    public class Indexed4BppTexture : Texture
    {
        private RgbaVector[] Palette { get; }
        private ISerializer<byte> Bytes { get; }

        public Indexed4BppTexture(int id, int width, int height, RgbaVector[] palette, Stream stream) : base(id, width, height, 1, stream)
        {
            Palette = palette;
            Bytes = new UInt8Serializer();
        }

        public override async Task<Image<RgbaVector>> ReadImageAsync()
        {
            Stream.Seek(0, SeekOrigin.Begin);
            var img = new Image<RgbaVector>(Width, Height);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width / 2; x++)
                {
                    byte pair = await Bytes.ReadFromStreamAsync(Stream);

                    int hi = pair >> 4;
                    int lo = pair & 0x0F;

                    img[x * 2 + 0, y] = Palette[lo];
                    img[x * 2 + 1, y] = Palette[hi];
                }
            }

            return img;
        }

        public override Task WriteImageAsync(Image<RgbaVector> pixels)
        {
            throw new NotImplementedException();
        }
    }
}
