using MightyStruct;
using System.IO;
using System.Threading.Tasks;

namespace NuXtractor.Converters
{
    using Formats.V1;

    public class PrototypeNUXConverter
    {
        private string OriginalPath { get; }
        private string InputPath { get; }
        private string OutputPath { get; }

        public PrototypeNUXConverter(string path)
        {
            OriginalPath = path;
            var filePath = Path.ChangeExtension(path, null);

            InputPath = filePath + ".tmp.nux";
            OutputPath = filePath + ".converted.nux";
        }

        public async Task ConvertAsync()
        {
            File.Copy(OriginalPath, InputPath, true);
            using (var input = new NUXContainer("nux_v0", InputPath))
            using (var output = File.Create(OutputPath))
            {
                await input.LoadAsync();

                var data = input.data;
                var stream = input.Stream;

                var header = data.header;
                var objects = data.objects.desc;

                uint offsetAdjust = 4 * (uint)objects.size;

                header.material_offset -= offsetAdjust;
                header.model_offset -= offsetAdjust;
                header.dynamic_offset -= offsetAdjust;

                for (int i = 0; i < header.unk003.size; i++)
                {
                    header.unk003[i] -= offsetAdjust;
                }

                await header.UpdateAsync();

                var firstChunk = new SubStream(stream, 0);
                firstChunk.SetLength((long)data.header.object_offset + 64);
                firstChunk.Lock();

                await firstChunk.CopyToAsync(output);

                var dynamics = data.dynamics.desc;
                for (int i = 0; i < dynamics.size; i++)
                {
                    dynamics[i].obj.offset = 0U;
                    await dynamics[i].obj.UpdateAsync();
                }

                for (int i = 0; i < objects.size; i++)
                {
                    objects[i].offset = 0U;
                    await objects[i].UpdateAsync();

                    var objStream = objects[i].Context.Stream;

                    for (int j = 0; j < dynamics.size; j++)
                    {
                        if (dynamics[j].obj_offset > objStream.AbsoluteOffset - 64)
                        {
                            dynamics[j].obj_offset -= 4;
                            await dynamics[j].UpdateAsync();
                        }
                    }

                    var subStream = new SubStream(objStream, 0);
                    subStream.SetLength(80);
                    subStream.Lock();

                    await subStream.CopyToAsync(output);
                }

                long position = stream.Position + 4;

                var materials = data.materials;
                for (int i = 0; i < (int)materials.count; i++)
                {
                    materials.offsets[i] -= offsetAdjust;
                }
                await materials.UpdateAsync();

                var models = data.models;
                var modelHeader = models.header;

                modelHeader.list_offset -= offsetAdjust;
                modelHeader.locator_offset -= offsetAdjust;
                modelHeader.group_offset -= offsetAdjust;

                await modelHeader.UpdateAsync();

                var modelList = models.list;
                for (int i = 0; i < modelHeader.num_models; i++)
                {
                    modelList.offsets[i] -= offsetAdjust;

                    var desc = modelList.desc[i];
                    desc.offset -= offsetAdjust;

                    var model = desc.model;
                    model.elem_offset -= offsetAdjust;

                    var elements = model.elements;
                    elements.data_offset -= offsetAdjust;

                    while (model.next != null)
                    {
                        model.next_offset -= offsetAdjust;

                        model = model.next;
                        model.elem_offset -= offsetAdjust;

                        elements = model.elements;
                        elements.data_offset -= offsetAdjust;
                    }
                }

                await modelList.UpdateAsync();

                var locators = data.locators.desc;
                foreach (var locator in locators)
                {
                    locator.loc_offset -= offsetAdjust;
                }

                await locators.UpdateAsync();

                var groups = data.groups;
                for (int i = 0; i < modelHeader.num_groups; i++)
                {
                    if (groups.offsets[i] != 0)
                    {
                        groups.offsets[i] -= offsetAdjust;

                        var root = groups.root[i];
                        root.desc_offset -= offsetAdjust;

                        for (int j = 0; j < root.desc.offsets.size; j++)
                        {
                            if (root.desc.offsets[j] != 0)
                            {
                                root.desc.offsets[j] -= offsetAdjust;

                                var child = root.desc.children[j];
                                child.desc_offset -= offsetAdjust;

                                for (int k = 0; k < child.desc.offsets.size; k++)
                                {
                                    if (child.desc.offsets[k] != 0)
                                        child.desc.offsets[k] -= offsetAdjust;
                                }
                            }
                        }
                    }
                }
                await groups.UpdateAsync();

                //stream.Seek(position, SeekOrigin.Begin);
                var secondChunk = new SubStream(stream, position);
                secondChunk.SetLength((long)header.texture_offset - position + 64);
                secondChunk.Lock();

                await secondChunk.CopyToAsync(output);

                output.Seek((long)header.texture_offset + 64, SeekOrigin.Begin);
                await stream.CopyToAsync(output);
            }
        }
    }
}
