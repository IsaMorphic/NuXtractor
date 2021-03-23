using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Textures
{
    public class PNTInfo : FormattedStream
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public RgbaVector[] Palette { get; private set; }

        public new Stream Stream { get; private set; }

        public PNTInfo(Stream stream) : base("textures\\pnt", stream)
        {
        }

        protected override Task OnLoadAsync()
        {
            Width = (int)data.width.Value;
            Height = (int)data.height.Value;

            Palette = new RgbaVector[data.palette.colors.size];
            for (int i = 0; i < data.palette.colors.size; i++)
            {
                var color = data.palette.colors[i];
                Palette[i] = new RgbaVector(color.r / 255.0f, color.g / 255.0f, color.b / 255.0f, color.a / 255.0f);
            }

            Stream = data.pixels.data;

            return Task.CompletedTask;
        }
    }

    public class PNTTexture : Texture
    {
        public PNTTexture(int id, PNTInfo info, Stream stream) : base(id, info.Width, info.Height, 1, stream)
        {
        }

        public override Task<Image<RgbaVector>> ReadImageAsync()
        {
            throw new NotImplementedException();
        }

        public override Task WriteImageAsync(Image<RgbaVector> pixels)
        {
            throw new NotImplementedException();
        }
    }
}
