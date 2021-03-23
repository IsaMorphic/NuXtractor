/*
 *  Copyright 2020 Chosen Few Software
 *  This file is part of NuXtractor.
 *
 *  NuXtractor is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  NuXtractor is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with NuXtractor.  If not, see <https://www.gnu.org/licenses/>.
 */

using NuXtractor.Materials;
using NuXtractor.Models;
using NuXtractor.Scenes;
using NuXtractor.Textures;

using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace NuXtractor.LSW1.PCXB
{
    public class LevelContainer : Container, ISceneContainer
    {
        public override int ModelCount => data.models.list.desc.size;
        public override int MaterialCount => data.materials.desc.size;
        public override int TextureCount => data.textures.desc.size;

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

        public async Task AddObjectAsync(int modelId, Matrix4x4 transform)
        {
            int lastIndex = data.objects.desc.size - 1;

            var obj = data.objects.desc[lastIndex];
            await obj.Context.Segment.ResizeAsync(4096 + 80);

            data.header.unk001.Value += 4096;
            await data.header.UpdateAsync();

            data.models.header.unkptr000.Value += 4096;
            data.models.header.unkptr001.Value += 4096;
            data.models.header.unkptr002.Value += 4096;
            data.models.header.unkptr003.Value += 4096;
            data.models.header.unkptr004.Value += 4096;
            data.models.header.unkptr005.Value += 4096;
            data.models.header.unkptr006.Value += 4096;
            data.models.header.unkptr007.Value += 4096;
            data.models.header.unkptr008.Value += 4096;
            data.models.header.unkptr009.Value += 4096;
            data.models.header.unkptr010.Value += 4096;

            data.models.header.num_objects.Value++;
            await data.models.header.UpdateAsync();

            await data.objects.desc.ParseAsync();

            lastIndex++;

            var newObj = data.objects.desc[lastIndex];
            var objTrans = newObj.transform;

            objTrans[0] = transform.M11;
            objTrans[1] = transform.M12;
            objTrans[2] = transform.M13;
            objTrans[3] = transform.M14;

            objTrans[4] = transform.M21;
            objTrans[5] = transform.M22;
            objTrans[6] = transform.M23;
            objTrans[7] = transform.M24;

            objTrans[8] = transform.M31;
            objTrans[9] = transform.M32;
            objTrans[10] = transform.M33;
            objTrans[11] = transform.M34;

            objTrans[12] = transform.M41;
            objTrans[13] = transform.M42;
            objTrans[14] = transform.M43;
            objTrans[15] = transform.M44;

            newObj.model = (ushort)modelId;
            await newObj.UpdateAsync();
        }

        public async Task<Texture> AddTextureAsync(int width, int height, int levels, Stream stream)
        {
            int lastIndex = (int)data.textures.header.count.Value - 1;

            var entries = data.textures.desc;
            var blocks = data.textures.blocks.data;

            var entry = entries[lastIndex];
            var block = blocks[lastIndex];

            long entryOffset = blocks.Context.Stream.AbsoluteOffset;

            long newEntryEnd = entryOffset + 20;
            long newEntrySize = newEntryEnd / 4096 * 4096 + 4096 - entryOffset + 20;

            await entry.Context.Segment.ResizeAsync(newEntrySize);

            data.textures.header.count.Value++;
            data.textures.header.data_offset += newEntrySize - 20;

            lastIndex++;

            await entries.ParseAsync();
            var newEntry = entries[lastIndex];

            newEntry.width = width;
            newEntry.height = height;
            newEntry.levels = levels;
            newEntry.type = 0x0E;

            newEntry.offset = block.Context.Stream.AbsoluteOffset - blocks.Context.Stream.AbsoluteOffset + block.Context.Stream.Length;

            await newEntry.UpdateAsync();

            long oldBlockSize = block.Context.Stream.Length;

            long blockOffset = block.Context.Stream.AbsoluteOffset;
            long newBlockEnd = blockOffset + oldBlockSize + stream.Length;

            long newBlockSize = newBlockEnd / 4096 * 4096 + 4096 - blockOffset;
            await block.Context.Segment.ResizeAsync(newBlockSize);

            data.textures.header.last_offset += (uint)(newBlockSize - oldBlockSize);
            await data.textures.header.UpdateAsync();

            await blocks.ParseAsync();

            var newBlock = blocks[lastIndex];
            newBlock.Context.Stream.Seek(0, SeekOrigin.Begin);

            stream.Seek(0, SeekOrigin.Begin);
            await stream.CopyToAsync(newBlock.Context.Stream);

            return await GetTextureAsync(lastIndex);
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
            var vtxStream = new VertexStream(data.verticies.blocks[(int)model.vtx_block.Value - 1].data);

            var material = await GetMaterialAsync((int)model.material.Value);

            Mesh next = null;
            if (model.next != null)            
                next = await ParseModelAsync(model.next);

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

        protected override Task<Texture> GetNewTextureAsync(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}
