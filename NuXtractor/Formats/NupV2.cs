using System.Collections.Generic;
using System.Linq;

namespace NuXtractor.Formats
{
    public partial class NupV2 : ITextureContainer
    {
        public List<Texture> GetTextures()
        {
            var section = Sections.Single(s => s.Type == "TST0");
            var textures = section.Data as TextureIndex;
            return textures.Data
                .Select(tex => new Texture(
                    (int)tex.Width,
                    (int)tex.Height,
                    tex.Data)
                ).ToList();
        }
    }
}
