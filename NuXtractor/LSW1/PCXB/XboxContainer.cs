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

using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.LSW1.PCXB
{
    public class XboxContainer : LevelContainer, ITextureContainer
    {
        public XboxContainer(string format, string path) : base(format, path)
        {
        }

        protected override Task<Texture> GetNewTextureAsync(int id)
        {
            var textures = data.textures;

            var width = (int)textures.desc[id].width.Value;
            var height = (int)textures.desc[id].height.Value;

            var levels = (int)textures.desc[id].levels.Value;
            var type = (int)textures.desc[id].type.Value;

            Stream stream = textures.blocks.data[id];

            Texture texture;
            switch (type)
            {
                case 0x0C:
                    texture = new DXT1Texture(id, width, height, levels, Endianness.LittleEndian, stream);
                    break;
                case 0x0F:
                    texture = new DXT5Texture(id, width, height, levels, Endianness.LittleEndian, stream);
                    break;
                default:
                    texture = new UnknownTexture(id, width, height, levels, stream);
                    break;
            }
            return Task.FromResult(texture);
        }
    }
}
