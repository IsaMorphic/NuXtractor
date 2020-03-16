using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuXtractor.Formats
{
    public class IndexedTexture : Texture
    {
        public SKColor[] Colors { get; }

        public IndexedTexture(int width, int height, SKColor[] colors, byte[] data) : base(width, height, data)
        {
            Colors = colors;
        }

        public SKBitmap ToBitmap()
        {
            SKBitmap bitmap = new SKBitmap(Width, Height);
            switch (Colors.Length)
            {
                case 16:
                    for (int x = 0; x < Width / 2; x++)
                    {
                        for (int y = 0; y < Height; y++) 
                        {
                            byte pair = Data[y * Width / 2 + x];
                            int index1 = pair & 15;
                            int index2 = pair >> 4;
                            bitmap.SetPixel(x * 2, y, Colors[index1]);
                            bitmap.SetPixel(x * 2 + 1, y, Colors[index2]);
                        }
                    }
                    break;
                case 256:
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            int index = Data[y * Width + x];
                            bitmap.SetPixel(x, y, Colors[index]);
                        }
                    }
                    break;
            }
            return bitmap;
        }
    }

    public partial class Gsc : ITextureContainer
    {
        public List<Texture> GetTextures()
        {
            var section = Sections.Single(s => s.Type == "TST0");
            var textures = section.Data as TextureIndex;
            return textures.Entries
                .Select<TextureEntry, Texture>(
                    entry => new IndexedTexture(
                        (int)entry.Texture.Width,
                        (int)entry.Texture.Height,
                        entry.Texture.Palette.Colors
                            .Select(c => new SKColor(c.R, c.G, c.B, (byte)(c.A * 2)))
                            .ToArray(),
                        entry.Texture.Pixels)
                ).ToList();
        }
    }
}
