/*
 *  Copyright 2020 Chosen Few Software
 *  This file is part of NuXtractor.
 *
 *  NuXtractor is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  NuXtractor is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with NuXtractor.  If not, see <https://www.gnu.org/licenses/>.
 */

using NuXtractor.Textures;
using System.Collections.Generic;
using System.Linq;

namespace NuXtractor.Formats
{
    public partial class CscGc : ITextureContainer<GCTexture>
    {
        List<Texture> ITextureContainer<GCTexture>.GetTextures()
        {
            var section = Sections.Single(s => s.Type == "0TST");
            var textures = section.Data as TextureIndex;
            return textures.Data
                .Select<TextureData, Texture>(
                    tex => new GCTexture(
                        tex.Width, 
                        tex.Height, 
                        tex.Data,
                        tex.Type)
                    ).ToList();
        }
    }
}
