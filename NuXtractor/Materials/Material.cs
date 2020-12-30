using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Materials
{
    using Models;
    using Textures;

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
