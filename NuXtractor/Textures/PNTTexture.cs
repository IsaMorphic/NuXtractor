using NuXtractor.Textures.Indexed;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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

            int numColors = data.palette.colors.size;
            Palette = new RgbaVector[numColors];

            if (numColors == 16)
            {
                for (int i = 0; i < numColors; i++)
                {
                    var color = data.palette.colors[i];
                    Palette[i] = new RgbaVector(color.r / 255.0f, color.g / 255.0f, color.b / 255.0f, color.a / 255.0f);
                }
            }
            else if (numColors == 256)
            {
                // From Rainbow by Marco Calautti (GPLv2, Copyright 2014+)
                // https://github.com/marco-calautti/Rainbow/blob/master/Rainbow.ImgLib/ImgLib/Filters/TIM2PaletteFilter.cs
                int parts = numColors / 32;
                int stripes = 2;
                int colors = 8;
                int blocks = 2;

                int i = 0;
                for (int part = 0; part < parts; part++)
                {
                    for (int block = 0; block < blocks; block++)
                    {
                        for (int stripe = 0; stripe < stripes; stripe++)
                        {
                            for (int color = 0; color < colors; color++)
                            {
                                var c = data.palette.colors[part * colors * stripes * blocks + block * colors + stripe * stripes * colors + color];
                                Palette[i++] = new RgbaVector(c.r / 255.0f, c.g / 255.0f, c.b / 255.0f, c.a / 255.0f);
                            }
                        }
                    }
                }
            }

            Stream = data.pixels.data;

            return Task.CompletedTask;
        }
    }

    public class PNTTexture : Texture
    {
        private Texture Inner { get; }

        public PNTTexture(int id, PNTInfo info, Stream stream) : base(id, info.Width, info.Height, 1, stream)
        {
            if (info.Palette.Length == 16)
                Inner = new Indexed4BppTexture(id, Width, Height, info.Palette, info.Stream);
            else
                Inner = new Indexed8BppTexture(id, Width, Height, info.Palette, info.Stream);
        }

        public override Task<Image<RgbaVector>> ReadImageAsync()
        {
            return Inner.ReadImageAsync();
        }

        public override Task WriteImageAsync(Image<RgbaVector> pixels)
        {
            return Inner.WriteImageAsync(pixels);
        }
    }
}
