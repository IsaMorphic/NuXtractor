using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Textures
{
    public class UnknownTexture : Texture
    {
        public UnknownTexture(int id, int width, int height, int levels, Stream stream) : base(id, width, height, levels, stream)
        {
        }

        public override Task<Image<RgbaVector>> ReadImageAsync()
        {
            throw new NotImplementedException();
        }

        public override Task WriteImageAsync(Image<RgbaVector> image)
        {
            throw new NotImplementedException();
        }
    }
}
