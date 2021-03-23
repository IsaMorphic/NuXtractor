using NuXtractor.Materials;
using NuXtractor.Models;
using NuXtractor.Textures;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NuXtractor.LSW1.PS2
{
    public class LevelTextures : FormattedStream, ITextureContainer
    {
        public LevelTextures(Stream stream) : base("games\\lsw1\\ps2\\lvl\\tst0", stream)
        {
        }

        public int TextureCount => (int)data.count.Value;

        public async Task<Texture> GetTextureAsync(int id)
        {
            var textures = data.textures;

            Stream stream = textures[id].data;
            stream.Seek(0, SeekOrigin.Begin);

            var info = new PNTInfo(stream);
            await info.LoadAsync();

            var texture = new PNTTexture(id, info, stream);
            return texture;
        }
    }

    public class LevelContainer : Container
    {
        private LevelTextures Textures { get; set; }

        public override int ModelCount => throw new NotImplementedException();
        public override int MaterialCount => throw new NotImplementedException();
        public override int TextureCount => Textures.TextureCount;

        public LevelContainer(string path) : base("games\\lsw1\\ps2\\lvl\\gsc", path)
        {
        }

        protected async override Task OnLoadAsync()
        {
            await base.OnLoadAsync();

            foreach (var section in data.sections)
            {
                var ccBytes = BitConverter.GetBytes(section.id);
                var ccCode = Encoding.ASCII.GetString(ccBytes);

                switch (ccCode)
                {
                    case "TST0":
                        Stream stream = section.data;
                        stream.Seek(0, SeekOrigin.Begin);
                        Textures = new LevelTextures(stream);
                        await Textures.LoadAsync();
                        break;
                }
            }
        }

        protected override Task<Material> GetNewMaterialAsync(int id)
        {
            throw new NotImplementedException();
        }

        protected override Task<Model> GetNewModelAsync(int id)
        {
            throw new NotImplementedException();
        }

        protected override Task<Texture> GetNewTextureAsync(int id)
        {
            return Textures.GetTextureAsync(id);
        }
    }
}
