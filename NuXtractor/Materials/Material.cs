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

namespace NuXtractor.Materials
{
    public class Material
    {
        public int Id { get; }

        public Color Color { get; }
        public Texture Texture { get; }

        public Material(int id, Color color, Texture texture)
        {
            Id = id;

            Color = color;
            Texture = texture;
        }

        public async Task WriteToMTLAsync(StreamWriter writer)
        {
            await writer.WriteLineAsync($"newmtl material_{Id}");
            await writer.WriteLineAsync($"Kd {Color}");

            if (Texture != null)
                await writer.WriteLineAsync($"map_Kd .\\textures\\texture_{Texture.Id}.png");
        }
    }
}
