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
