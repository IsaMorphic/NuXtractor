namespace NuXtractor.Models
{
    public class Face
    {
        public int Id { get; }
        public Vertex[] Vertices { get; }

        public Face(int id, params Vertex[] vertices)
        {
            Id = id;
            Vertices = vertices;
        }
    }
}
