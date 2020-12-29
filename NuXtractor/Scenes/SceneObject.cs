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

using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuXtractor.Scenes
{
    using Models;

    public class SceneObject
    {
        public int Id { get; }
        public string Name { get; }

        public Model Model { get; }
        public Matrix4x4 Transform { get; }

        public List<SceneObject> Children { get; }

        public SceneObject(int id, string name, Model models, Matrix4x4 transform)
        {
            Id = id;
            Name = name;

            Model = models;
            Transform = transform;

            Children = new List<SceneObject>();
        }

        public async Task WriteToOBJAsync(StreamWriter writer)
        {
            await writer.WriteLineAsync($"o {Name}");

            await Model.WriteToOBJAsync(writer, Transform);

            foreach (var child in Children)
            {
                await child.WriteToOBJAsync(writer);
            }
        }
    }
}
