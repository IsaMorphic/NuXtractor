using System.Numerics;

namespace NuXtractor.Models
{
    public class Vertex
    {
        public int Id { get; }

        public Vector3 XYZ { get; }
        public UVCoord UV { get; }

        public Color Color { get; }

        public Vertex(int id, Vector3 xyz, UVCoord uv, Color color)
        {
            Id = id;

            XYZ = xyz;
            UV = uv;

            Color = color;
        }
    }
}
