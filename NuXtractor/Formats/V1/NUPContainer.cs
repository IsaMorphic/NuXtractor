using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Formats.V1
{
    using Textures;

    public class NUPContainer : LevelContainer, ITextureContainer
    {
        public NUPContainer(string path) : base(path)
        {
        }

        protected override async Task<Texture> GetNewTextureAsync(int id)
        {

            var textures = data.textures;

            var stream = textures.data[id];
            stream.Seek(0, SeekOrigin.Begin);

            var info = new DDSInfo(stream);
            await info.LoadAsync();

            var texture = new DDSTexture(id, info, stream);

            return texture;
        }
    }
}
