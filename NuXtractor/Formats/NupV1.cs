using System.Collections.Generic;
using System.Linq;

namespace NuXtractor.Formats
{
    public partial class NupV1 : ITextureContainer
    {
        public List<Texture> GetTextures()
        {
            return Textures.Data
                .Select(tex => new Texture(
                    (int)tex.Width, 
                    (int)tex.Height, 
                    tex.Data)
                ).ToList();
        }
    }
}
