using System.Collections.Generic;
using System.Linq;

namespace NuXtractor.Scenes
{
    using Materials;
    using Models;
    using Textures;

    public class Scene
    {
        public List<SceneObject> Objects { get; }

        public Model[] Models => Objects
            .SelectMany(o => o.Model.SubModels)
            .Distinct().ToArray();

        public Material[] Materials => Models
            .Select(o => o.Material)
            .Distinct().ToArray();

        public Texture[] Textures => Materials
            .Select(m => m.Texture)
            .Where(t => t != null)
            .Distinct().ToArray();

        public Scene()
        {
            Objects = new List<SceneObject>();
        }
    }
}
