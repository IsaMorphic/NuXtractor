using MightyStruct.Serializers;
using System.IO;

namespace NuXtractor.Textures.DXT
{
    public abstract class DXTTexture : Texture
    {
        public Endianness Endianness { get; }

        public DXTTexture(int width, int height, Endianness endianness, Stream stream) : base(width, height, stream)
        {
            Endianness = endianness;
        }
    }
}
