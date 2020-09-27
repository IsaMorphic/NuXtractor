using NuXtractor.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuXtractor.Formats
{
    public partial class HgpV1 : ITextureContainer<DDSTexture>, ITextureContainer<DXT1Texture>
    {
        List<Texture> ITextureContainer<DXT1Texture>.GetTextures()
        {
            return TexIndex.Data
                .Select<TextureData, Texture>(
                    tex => new DXT1Texture(
                        (int)tex.Width,
                        (int)tex.Height,
                        tex.Data)
                    ).ToList();
        }

        List<Texture> ITextureContainer<DDSTexture>.GetTextures()
        {
            return TexIndex.Data
                .Select<TextureData, Texture>(
                    tex => new DDSTexture(
                        (int)tex.Width,
                        (int)tex.Height,
                        tex.Data)
                    ).ToList();
        }
    }
}
