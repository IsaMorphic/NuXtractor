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
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;

namespace NuXtractor.Formats
{
    public partial class GscPs2 : ITextureContainer<IndexedTexture>
    {
        List<Texture> ITextureContainer<IndexedTexture>.GetTextures()
        {
            var section = Sections.Single(s => s.Type == "TST0");
            var textures = section.Data as TextureIndex;
            return textures.Entries
                .Select<TextureEntry, Texture>(
                    entry => new IndexedTexture(
                        entry.Texture.Width,
                        entry.Texture.Height,
                        entry.Texture.Palette.Colors
                            .Select(c => new SKColor(c.R, c.G, c.B, (byte)(c.A * 2)))
                            .ToArray(),
                        entry.Texture.Pixels.Data)
                ).ToList();
        }
    }
}
