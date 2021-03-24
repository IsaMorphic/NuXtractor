
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Textures.Indexed
{
    public class Indexed8BppTexture : Texture
    {
        private RgbaVector[] Palette { get; }

        public Indexed8BppTexture(int id, int width, int height, RgbaVector[] palette, Stream stream) : base(id, width, height, 1, stream)
        {
            Palette = palette;
        }

        public override Task<Image<RgbaVector>> ReadImageAsync()
        {
            Stream.Seek(0, SeekOrigin.Begin);
            var img = new Image<RgbaVector>(Width, Height);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int idx = Stream.ReadByte();
                    img[x, y] = Palette[idx];
                }
            }

            return Task.FromResult(img);
        }

        public override Task WriteImageAsync(Image<RgbaVector> pixels)
        {
            throw new NotImplementedException();
        }
    }
}
