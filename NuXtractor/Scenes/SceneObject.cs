using System.Numerics;

namespace NuXtractor.Scenes
{
    using Models;

    public class SceneObject
    {
        public int Id { get; }
        public string Name { get; }

        public Model[] Models { get; }
        public Matrix4x4 Transform { get; }

        public SceneObject(int id, string name, Model[] models, Matrix4x4 transform)
        {
            Id = id;
            Name = name;

            Models = models;
            Transform = transform;
        }
    }
}
