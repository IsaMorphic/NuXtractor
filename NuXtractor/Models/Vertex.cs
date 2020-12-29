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

using System.Numerics;

namespace NuXtractor.Models
{
    public struct Vertex
    {
        public Vector3 Pos { get; }
        public Vector2 Tex { get; }

        public Color Color { get; }

        public Vertex(Vector3 pos, Vector2 tex, Color color)
        {
            Pos = pos;
            Tex = tex;

            Color = color;
        }
    }
}
