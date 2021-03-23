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

using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.LSW1.PCXB
{
    public class PCContainer : LevelContainer, ITextureContainer
    {
        public PCContainer(string path) : base("games\\lsw1\\pc\\lvl\\nup", path)
        {
        }

        protected override async Task<Texture> GetNewTextureAsync(int id)
        {
            var textures = data.textures;

            Stream stream = textures.blocks.data[id];
            stream.Seek(0, SeekOrigin.Begin);

            var info = new DDSInfo(stream);
            await info.LoadAsync();

            var texture = new DDSTexture(id, info, stream);

            return texture;
        }
    }
}
