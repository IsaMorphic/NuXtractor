using NuXtractor.Materials;
using NuXtractor.Models;
using NuXtractor.Scenes;
using NuXtractor.Textures;

using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace NuXtractor.Formats.V1
{
    public abstract class LevelContainer : Container, ISceneContainer
    {
        protected LevelContainer(string format, string path) : base(format, path)
        {
        }

        public async Task<Stream> ResizeTexture(int id, int newWidth, int newHeight, int newLevels, long newSize)
        {
            var entry = data.textures.desc[id];
            var texture = data.textures.blocks.data[id];

            entry.width = newWidth;
            entry.height = newHeight;
            entry.levels = newLevels;

            await entry.UpdateAsync();

            var offset = texture.Context.Stream.AbsoluteOffset;
            var newEnd = offset + newSize;

            await texture.Context.Segment.ResizeAsync(newEnd / 4096 * 4096 + 4096 - offset);

            return data.textures.blocks.data[id];
        }

        public async Task<Stream> ResizeVertexBlock(int modelId, short deltaSize)
        {
            var model = data.models.list.desc[modelId].model;
            var vtxBlock = data.verticies.blocks[(int)model.vtx_block.Value - 1];

            data.verticies.header.data_size += deltaSize;
            await data.verticies.header.UpdateAsync();

            vtxBlock.size += deltaSize;
            await vtxBlock.UpdateAsync();

            var offset = vtxBlock.data.Context.Stream.AbsoluteOffset;

            var oldEnd = offset + vtxBlock.data.Context.Stream.Length;
            var newEnd = offset + vtxBlock.size;

            await vtxBlock.data.Context.Segment.ResizeAsync(newEnd / 16 * 16 + 16 - offset - (oldEnd / 16 * 16 + 16 - oldEnd));

            return vtxBlock.data;
        }

        public async Task<Stream> ResizeElementArray(int modelId, short deltaSize)
        {
            var elements = data.models.list.desc[modelId].model.elements;

            elements.count += deltaSize / 2;
            await elements.UpdateAsync();

            Stream stream = elements.data;

            await elements.data.Context.Segment.ResizeAsync(stream.Length + deltaSize);

            return stream;
        }

        private async Task<Model> ParseModelAsync(dynamic model)
        {
            var elemStream = new ElementStream(model.elements.data);
            var vtxStream = new VertexStream(data.verticies.blocks[(int)model.vtx_block - 1].data);

            var material = await GetMaterialAsync((int)model.material);

            Mesh next = null;
            long offset = (long)model.next_offset;
            if (model.next != null)
            {
                if (CachedSubModels.ContainsKey(offset))
                    next = CachedSubModels[offset];
                else
                {
                    next = await ParseModelAsync(model.next);
                    CachedSubModels.Add(offset, next);
                }
            }

            return new Mesh(material, next, elemStream, vtxStream);
        }

        protected override async Task<Model> GetNewModelAsync(int id)
        {
            var model = data.models.list.desc[id].model;
            return await ParseModelAsync(model);
        }

        protected override async Task<Material> GetNewMaterialAsync(int id)
        {
            var material = data.materials.desc[id];
            var matCol = material.color;

            var container = (this as ITextureContainer);
            var texture = material.texture < data.textures.desc.size ? await container.GetTextureAsync(material.texture) : null;

            var color = new Color(matCol.red, matCol.green, matCol.blue, material.alpha);

            return new Material(id, color, texture);
        }

        public async Task<Scene> GetSceneAsync()
        {
            var scene = new Scene();

            var objects = data.objects.desc;
            for (int i = 0; i < objects.size; i++)
            {
                var obj = objects[i];

                var model = await GetModelAsync((int)obj.model);

                var trans = obj.transform;
                var matrix = new Matrix4x4(
                    trans[0], trans[1], trans[2], trans[3],
                    trans[4], trans[5], trans[6], trans[7],
                    trans[8], trans[9], trans[10], trans[11],
                    trans[12], trans[13], trans[14], trans[15]
                    );

                var sceneObj = new SceneObject(i, $"object_{i}", model, matrix);
                scene.Objects.Add(sceneObj);
            }

            var dynamics = data.dynamics.desc;
            for (int i = 0; i < dynamics.size; i++)
            {
                var obj = dynamics[i].obj;
                var name = dynamics[i].name;

                var model = await GetModelAsync((int)obj.model);

                var trans = obj.transform;
                var matrix = new Matrix4x4(
                    trans[0], trans[1], trans[2], trans[3],
                    trans[4], trans[5], trans[6], trans[7],
                    trans[8], trans[9], trans[10], trans[11],
                    trans[12], trans[13], trans[14], trans[15]
                    );

                var sceneObj = new SceneObject(i, name, model, matrix);
                scene.Objects.Add(sceneObj);
            }

            return scene;
        }
    }
}
