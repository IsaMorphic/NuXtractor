namespace NuXtractor.Models
{
    public class UVCoord
    {
        public int Id { get; }

        public float U { get; }
        public float V { get; }

        public UVCoord(int id, float u, float v)
        {
            Id = id;

            U = u;
            V = v;
        }
    }
}
