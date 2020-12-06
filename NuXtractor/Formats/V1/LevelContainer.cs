using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace NuXtractor.Formats.V1
{
    using Scenes;
    using Models;
    using Materials;
    using Textures;

    public abstract class LevelContainer : Container, ISceneContainer
    {
        protected LevelContainer(string path) : base("nux_v1", path)
        {
        }

        protected override async Task<Model> GetNewModelAsync(int id)
        {
            var model = data.models.list.desc[id].model;
            var idx = model.elements.data.indicies;

            int[] indicies = new int[idx.size];
            for (int i = 0; i < indicies.Length; i++)
            {
                indicies[i] = idx[i];
            }
            var stream = new VertexStream(data.verticies.blocks[(int)model.vtx_block - 1].data);
            var material = await GetMaterialAsync((int)model.material);

            return new Mesh(id, material, indicies, stream);
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
            var objects = data.objects.desc;
            var groups = data.groups.roots;

            var sceneObjs = new List<SceneObject>();
            for (int i = 0; i < objects.size; i++)
            {
                var obj = objects[i].obj;
                var name = objects[i].name;

                var models = new List<Model>();

                if (obj.group < groups.size)
                {
                    var group = groups[(int)obj.group];
                    if (group != null)
                    {
                        foreach (var child in group.desc.children)
                        {
                            if (child == null) continue;
                            models.Add(await GetModelAsync((int)(obj.group + child.model)));

                            foreach (var grandchild in child.desc.children)
                            {
                                if (grandchild == null) continue;
                                models.Add(await GetModelAsync((int)(obj.group + child.model + grandchild.model)));
                            }
                        }
                    }
                }
                else
                {
                    models.Add(await GetModelAsync((int)obj.group));
                }

                var trans = obj.transform;
                var matrix = new Matrix4x4(
                    trans[0], trans[1], trans[2], trans[3],
                    trans[4], trans[5], trans[6], trans[7],
                    trans[8], trans[9], trans[10], trans[11],
                    trans[12], trans[13], trans[14], trans[15]
                    );

                var sceneObj = new SceneObject(i, name, models.ToArray(), matrix);
                sceneObjs.Add(sceneObj);
            }

            return new Scene(sceneObjs.ToArray());
        }
    }
}
