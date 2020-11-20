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
using NuXtractor.Textures.DXT;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuXtractor.Formats
{
    public partial class XboxContainerV1 : FormattedFile, ITextureContainer<DXT1Texture>, ITextureContainer<DDSTexture>
    {
        public XboxContainerV1(string format, string path) : base(format, path)
        {
        }

        Task<List<Texture>> ITextureContainer<DXT1Texture>.GetTexturesAsync()
        {
            var index = data.texture_index;

            List<Texture> textures = new List<Texture>();

            for (int i = 0; i < index.entries.size; i++)
            {
                var width = (int)index.entries[i].width;
                var height = (int)index.entries[i].height;

                var levels = (int)index.entries[i].levels;
                var type = (int)index.entries[i].type;

                var stream = index.textures[i];

                if (type == 0x0C)
                    textures.Add(new DXT1Texture(width, height, levels, Endianness.LittleEndian, stream));
                else if (type == 0x0F)
                    textures.Add(new DXT5Texture(width, height, levels, Endianness.LittleEndian, stream));
                else
                    textures.Add(new UnknownTexture(width, height, levels, stream));
            }

            return Task.FromResult(textures);
        }

        async Task<List<Texture>> ITextureContainer<DDSTexture>.GetTexturesAsync()
        {
            var index = data.texture_index;

            List<Texture> textures = new List<Texture>();

            for (int i = 0; i < index.textures.size; i++)
            {
                var stream = index.textures[i];
                stream.Seek(0, SeekOrigin.Begin);

                var info = new DDSInfo(stream);
                await info.LoadAsync();

                var texture = new DDSTexture(info, stream);
                textures.Add(texture);
            }

            return textures;
        }
    }
}
