using System;
using System.Collections.Generic;
using System.Text;

namespace NuXtractor
{
    public class Texture
    {
        public int Width { get; }
        public int Height { get; }

        public byte[] Data { get; }

        public Texture(int width, int height, byte[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }
    }
    public interface ITextureContainer
    {
        List<Texture> GetTextures();
    }
}
