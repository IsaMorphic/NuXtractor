using MightyStruct.Serializers;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NuXtractor.Textures
{
    using DXT;

    public enum DXTType
    {
        DXT1,
        DXT3,
        DXT5,
    }

    public class DDSInfo : FormattedStream
    {
        public DDSInfo(Stream stream) : base("dds_texture", stream)
        {
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int Levels { get; private set; }

        public DXTType PixelFormat { get; private set; }

        public Stream Stream { get; private set; }

        protected override Task OnLoadAsync()
        {
            var header = data.header;

            Width = (int)header.width;
            Height = (int)header.height;

            Levels = (int)header.mipmapCount;

            var ccBytes = BitConverter.GetBytes(header.pixelFormat.fourCC);
            var ccCode = Encoding.ASCII.GetString(ccBytes);
            switch (ccCode)
            {
                case "DXT1":
                    PixelFormat = DXTType.DXT1;
                    break;
                case "DXT3":
                    PixelFormat = DXTType.DXT3;
                    break;
                case "DXT5":
                    PixelFormat = DXTType.DXT5;
                    break;
            }

            Stream = data.data;

            return Task.CompletedTask;
        }
    }

    public class DDSTexture : Texture
    {
        private DXT1Texture Inner { get; }

        public DDSTexture(int id, DDSInfo info, Stream stream) : base(id, info.Width, info.Height, info.Levels, stream)
        {
            switch (info.PixelFormat)
            {
                case DXTType.DXT1:
                    Inner = new DXT1Texture(Id, Width, Height, Levels, Endianness.LittleEndian, info.Stream);
                    break;
                case DXTType.DXT3:
                    Inner = new DXT3Texture(Id, Width, Height, Levels, Endianness.LittleEndian, info.Stream);
                    break;
                case DXTType.DXT5:
                    Inner = new DXT5Texture(Id, Width, Height, Levels, Endianness.LittleEndian, info.Stream);
                    break;
            }
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
