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

using MightyStruct.Serializers;
using NuXtractor.Textures;

using System.Collections.Generic;

namespace NuXtractor.Formats
{
    public partial class NUXv1 : FormattedFile, ITextureContainer<IDXTTexture>
    {
        public NUXv1(string path) : base("nux_v1", path)
        {
        }

        List<ITexture> ITextureContainer<IDXTTexture>.GetTextures()
        {
            var index = data.texture_index;

            List<ITexture> textures = new List<ITexture>();

            for (int i = 0; i < index.entries.size; i++)
            {
                var width = (int)index.entries[i].width;
                var height = (int)index.entries[i].height;
                var type = (int)index.entries[i].type;

                var stream = index.textures[i];

                if (type == 0x0C)
                    textures.Add(new DXT1Texture(width, height, stream, Endianness.LittleEndian));
                else
                    textures.Add(new UnknownTexture(width, height, stream));
            }

            return textures;
        }
    }
}
