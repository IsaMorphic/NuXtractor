using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace NuXtractor.Scenes
{
    using Materials;
    using Models;
    using Textures;

    public class Scene
    {
        public SceneObject[] Objects { get; }

        public Model[] Models => Objects
            .SelectMany(o => o.Models)
            .Distinct().ToArray();

        public Material[] Materials => Models
            .Select(o => o.Material)
            .Distinct().ToArray();

        public Texture[] Textures => Materials
            .Select(m => m.Texture)
            .Where(t => t != null)
            .Distinct().ToArray();

        public Scene(SceneObject[] objects)
        {
            Objects = objects;
        }

        public async Task ArchiveAsync(string dir)
        {
            Directory.CreateDirectory(dir);

            var mtlPath = Path.Combine(dir, "materials.mtl");

            using (var writer = File.CreateText(mtlPath))
            {
                writer.AutoFlush = true;
                foreach (var material in Materials)
                {
                    await material.WriteToMTLAsync(writer);
                }
            }

            var texDir = Path.Combine(dir, "textures");
            Directory.CreateDirectory(texDir);

            foreach (var texture in Textures)
            {
                var image = await texture.ReadImageAsync();

                var texPath = Path.Combine(texDir, $"texture_{texture.Id}.png");
                await image.SaveAsPngAsync(texPath);
            }

            var objPath = Path.Combine(dir, "scene.obj");
            using (var writer = File.CreateText(objPath))
            {
                writer.AutoFlush = true;

                await writer.WriteLineAsync("mtllib .\\materials.mtl");

                foreach (var obj in Objects)
                {
                    await writer.WriteLineAsync($"o {obj.Name}");
                    foreach (var model in obj.Models) 
                    {
                        await model.WriteToOBJAsync(writer, obj.Transform);
                    }
                }
            }
        }
    }
}
