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

using CommandLine;
using MightyStruct.Serializers;
using NuXtractor.Materials;
using NuXtractor.Models;
using NuXtractor.Scenes;
using NuXtractor.Textures;
using NuXtractor.Textures.DXT;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace NuXtractor
{
    enum LevelFormat
    {
        NUPv1,

        NUXv0,
        NUXv1,

        GSCps2,
    }

    enum TextureFormat
    {
        UNK,
        DXT1,
        DXT3,
        DXT5,
        DDS,
        PNT,
    }

    enum MultiToolMode
    {
        TEX,
    }

    enum ExtractionMode
    {
        DUMP,
        CONV,
        INJD,
        INJC,
    }

    enum OutputImportance
    {
        Verbose,
        Important,
        Highlight,
        Error,
    }

    class Options
    {
        [Option('i', "input-file", Required = true, HelpText = "Path to the input file.")]
        public string InputFile { get; set; }
    }

    class LevelOptions : Options
    {
        [Option('f', "file-format", Required = true, HelpText = "The format of the specified input file.  Can be NUXv0, NUXv1 or NUPv1")]
        public LevelFormat FileFormat { get; set; }
    }

    [Verb("scene", HelpText = "Extract all data from a TT Games data container")]
    class SceneOptions : LevelOptions
    {
    }

    [Verb("models", HelpText = "Extract model data from a TT Games data container")]
    class ModelOptions : LevelOptions
    {
        [Option('w', "write-materials", HelpText = "Flag indicating whether the extraction should include model materials/textures.")]
        public bool WriteMaterials { get; set; }
    }

    [Verb("textures", HelpText = "Extract and inject textures into and from a TT Games data container.")]
    class TextureOptions : LevelOptions
    {
        [Option('m', "mode", Required = true, HelpText = "The extraction mode to use. Can be DUMP, CONV(ert), INJ(ect)C(onvert), or INJ(ect)D(ump).")]
        public ExtractionMode Mode { get; set; }

        [Option('p', "write-patch", Required = false, HelpText = "Flag indicating whether to write a ModLoader compatible patch file with the relevant file changes. Only applies when using INJC or INJD modes.")]
        public bool WritePatch { get; set; }
    }

    [Verb("multitool", HelpText = "Operate on various asset file formats that NuXtractor supports")]
    class MultiToolOptions : Options
    {
        [Option('m', "tool-mode", Required = true, HelpText = "The tool to select from the multi-tool")]
        public MultiToolMode Mode { get; set; }

        [Option('t', "texture-format", Required = false, HelpText = "For TEX tool, the primary texture format to be operated over.")]
        public TextureFormat TextureFormat { get; set; }

        [Option('d', "dimensions", Required = false, HelpText = "For TEX tool, <width>x<height> of the texture")]
        public string Dimensions { get; set; }
    }

    [Verb("test", HelpText = "test")]
    class TestOptions : LevelOptions
    {
        [Option('t', "texture-file", Required = true, HelpText = "Path to new texture file.")]
        public string TextureFile { get; set; }

        //[Option('m', "model-file", Required = true, HelpText = "Path to input obj model file.")]
        //public string ModelFile { get; set; }

        //[Option('o', "output-file", Required = true, HelpText = "Path to stripped output obj model file.")]
        //public string OutputFile { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            await Parser.Default
                .ParseArguments<TextureOptions, ModelOptions, SceneOptions, MultiToolOptions, TestOptions>(args)
                .MapResult(
                    (TextureOptions opts) => RunTexturesAsync(opts),
                    (ModelOptions opts) => RunModelsAsync(opts),
                    (SceneOptions opts) => RunSceneAsync(opts),
                    (MultiToolOptions opts) => RunMultiToolAsync(opts),
                    (TestOptions opts) => RunTestAsync(opts),
                    err => Task.CompletedTask
                    );
        }

        static void WriteLine(string str = "", OutputImportance type = OutputImportance.Important)
        {
            switch (type)
            {
                case OutputImportance.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case OutputImportance.Highlight:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case OutputImportance.Important:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case OutputImportance.Verbose:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine(str);
            Console.ResetColor();
        }

        static async Task<FormattedFile> OpenContainerAsync(LevelOptions options)
        {
            if (!File.Exists(options.InputFile))
            {
                WriteLine("Error: The specified input file does not exist. Check any paths or filenames to make sure they are correct.", OutputImportance.Error);
                return null;
            }

            FormattedFile file = null;
            try
            {
                switch (options.FileFormat)
                {
                    case LevelFormat.NUXv0:
                        file = new LSW1.PCXB.XboxContainer("games\\lsw1\\xbox\\lvl\\nux_v0", options.InputFile);
                        break;
                    case LevelFormat.NUXv1:
                        file = new LSW1.PCXB.XboxContainer("games\\lsw1\\xbox\\lvl\\nux_v1", options.InputFile);
                        break;
                    case LevelFormat.NUPv1:
                        file = new LSW1.PCXB.PCContainer(options.InputFile);
                        break;
                    case LevelFormat.GSCps2:
                        file = new LSW1.PS2.LevelContainer(options.InputFile);
                        break;
                }

                WriteLine("Parsing file...");

                await file.LoadAsync();
                return file;
            }
            catch (Exception)
            {
                WriteLine("Error: The input file could not be parsed in the chosen format.", OutputImportance.Error);
                throw;
            }
        }

        static void Goodbye()
        {
            WriteLine("Thanks for using NuXtractor, a tool created by Yodadude2003.", OutputImportance.Highlight);
            WriteLine("Please make sure to credit the tool and creator for any public usage of the assets it extracts.", OutputImportance.Highlight);
            WriteLine("For more software from Chosen Few Software, visit https://www.chosenfewsoftware.com/", OutputImportance.Highlight);
            WriteLine("Copyright (C) 2021 Chosen Few Software", OutputImportance.Highlight);
        }

        static async Task RunTexturesAsync(TextureOptions options)
        {
            var file = await OpenContainerAsync(options) as ITextureContainer;

            string dir = options.InputFile + ".textures";
            Directory.CreateDirectory(dir);

            Stream patchFile = Stream.Null;

            if (options.WritePatch && (options.Mode == ExtractionMode.INJC || options.Mode == ExtractionMode.INJD))
                patchFile = File.Create(options.InputFile + ".patch");

            // Start reading textures one by one
            for (int i = 0; i < file.TextureCount; i++)
            {
                var texture = await file.GetTextureAsync(i);
                switch (options.Mode)
                {
                    case ExtractionMode.DUMP:
                        try
                        {
                            string dumpPath = Path.Combine(dir, $"texture_{i}.bin");
                            WriteLine($"Found texture #{i}; Dumping to file...");

                            using (var stream = File.Create(dumpPath))
                            {
                                await texture.CopyToStreamAsync(stream);
                            }

                            WriteLine($"Dumped to {dumpPath} successfully!", OutputImportance.Verbose);
                        }
                        catch (Exception)
                        {
                            WriteLine("Dump failed!!! This is a bug. Please notify the developers immediately with details about how the issue arose.", OutputImportance.Error);
                            WriteLine("Create an issue on github: https://www.github.com/yodadude2003/NuXtractor", OutputImportance.Verbose);
                            WriteLine("Write us an email: info@chosenfewsoftware.com", OutputImportance.Verbose);
                        }
                        break;
                    case ExtractionMode.CONV:
                        try
                        {
                            WriteLine($"Found texture #{i}; Converting to PNG...");

                            var image = await texture.ReadImageAsync();

                            string convPath = Path.Combine(dir, $"texture_{i}.png");
                            await image.SaveAsPngAsync(convPath);

                            WriteLine($"Conversion successful! Wrote output to file: {convPath}", OutputImportance.Verbose);
                        }
                        catch (Exception)
                        {
                            WriteLine("Conversion failed! Moving on to next texture...", OutputImportance.Error);
                        }
                        break;
                    case ExtractionMode.INJD:
                        try
                        {
                            string injdPath = Path.Combine(dir, $"texture_{i}.bin");
                            if (File.Exists(injdPath))
                            {
                                WriteLine($"Found replacement file {injdPath}; Injecting raw texture data...");

                                using (var stream = File.OpenRead(injdPath))
                                {
                                    await texture.CopyFromStreamAsync(stream);
                                }

                                await texture.WritePatchAsync(patchFile);

                                WriteLine($"Injected texture #{i} successfully!", OutputImportance.Verbose);
                            }
                        }
                        catch (Exception)
                        {
                            WriteLine("Injection failed!!! This is a bug. Please notify the developers immediately with details about how the issue arose.", OutputImportance.Error);
                            WriteLine("Create an issue on github: https://www.github.com/yodadude2003/NuXtractor", OutputImportance.Verbose);
                            WriteLine("Write us an email: info@chosenfewsoftware.com", OutputImportance.Verbose);
                        }
                        break;
                    case ExtractionMode.INJC:
                        try
                        {
                            string injcPath = Path.Combine(dir, $"texture_{i}.png");
                            if (File.Exists(injcPath))
                            {
                                WriteLine($"Found replacement image {injcPath}; Converting & injecting texture...");

                                var image = await Image.LoadAsync<RgbaVector>(injcPath);
                                await texture.WriteImageAsync(image);

                                await texture.WritePatchAsync(patchFile);

                                WriteLine($"Injected texture #{i} successfully!", OutputImportance.Verbose);
                            }
                        }
                        catch (Exception)
                        {
                            WriteLine($"Failed to inject texture #{i}!", OutputImportance.Error);
                            WriteLine("Create an issue on github: https://www.github.com/yodadude2003/NuXtractor", OutputImportance.Verbose);
                            WriteLine("Write us an email: info@chosenfewsoftware.com", OutputImportance.Verbose);
                        }
                        break;
                }
            }

            patchFile.Dispose();

            Goodbye();
        }

        static async Task RunModelsAsync(ModelOptions options)
        {
            var file = await OpenContainerAsync(options);

            var models = file as IModelContainer;
            var materials = file as IMaterialContainer;
            var textures = file as ITextureContainer;

            string dir = options.InputFile + ".models";
            Directory.CreateDirectory(dir);

            if (options.WriteMaterials)
            {
                WriteLine("Writing material library file...");

                string materialPath = Path.Combine(dir, "materials.mtl");
                using (var writer = File.CreateText(materialPath))
                {
                    for (int i = 0; i < materials.MaterialCount; i++)
                    {
                        var material = await materials.GetMaterialAsync(i);

                        WriteLine($"Extracting material with id: {material.Id}...");
                        await material.WriteToMTLAsync(writer);
                        WriteLine($"Wrote material with id: {material.Id} to {materialPath}.", OutputImportance.Verbose);
                    }
                }

                WriteLine("Extracting textures...");

                string textureDir = Path.Combine(dir, "textures");
                Directory.CreateDirectory(textureDir);

                for (int i = 0; i < textures.TextureCount; i++)
                {
                    var texture = await textures.GetTextureAsync(i);
                    if (texture != null)
                    {
                        WriteLine($"Extracting texture with id: {texture.Id}...");
                        string texturePath = Path.Combine(textureDir, $"texture_{texture.Id}.png");

                        var image = await texture.ReadImageAsync();
                        await image.SaveAsPngAsync(texturePath);
                        WriteLine($"Wrote texture with id: {texture.Id} to {texturePath}.", OutputImportance.Verbose);
                    }
                }
            }

            WriteLine($"Exporting models to OBJs...");

            for (int i = 0; i < models.ModelCount; i++)
            {
                var model = await models.GetModelAsync(i);

                WriteLine($"Extracting model with id: {i}...");
                string modelPath = Path.Combine(dir, $"model_{i}.obj");

                using (var modelWriter = File.CreateText(modelPath))
                {
                    if (options.WriteMaterials)
                        await modelWriter.WriteLineAsync("mtllib .\\materials.mtl");
                    await model.WriteToOBJAsync(modelWriter);
                }

                WriteLine($"Wrote model with id: {i} to {modelPath}", OutputImportance.Verbose);
            }

            Goodbye();
        }

        static async Task RunSceneAsync(SceneOptions options)
        {
            var file = await OpenContainerAsync(options) as ISceneContainer;
            var scene = await file.GetSceneAsync();

            string dir = options.InputFile + ".extracted";
            Directory.CreateDirectory(dir);

            WriteLine("Writing material library file...");

            var mtlPath = Path.Combine(dir, "materials.mtl");
            using (var writer = File.CreateText(mtlPath))
            {
                writer.AutoFlush = true;
                foreach (var material in scene.Materials)
                {
                    WriteLine($"Extracting material with id: {material.Id}...");
                    await material.WriteToMTLAsync(writer);
                    WriteLine($"Wrote material with id: {material.Id} to {mtlPath}.", OutputImportance.Verbose);
                }
            }

            WriteLine("Extracting textures...");

            var texDir = Path.Combine(dir, "textures");
            Directory.CreateDirectory(texDir);

            foreach (var texture in scene.Textures)
            {
                WriteLine($"Extracting texture with id: {texture.Id}...");
                var image = await texture.ReadImageAsync();

                var texPath = Path.Combine(texDir, $"texture_{texture.Id}.png");
                await image.SaveAsPngAsync(texPath);

                WriteLine($"Wrote texture with id: {texture.Id} to {texPath}.", OutputImportance.Verbose);
            }

            WriteLine("Exporting full scene to OBJ file...");

            var objPath = Path.Combine(dir, "scene.obj");
            using (var writer = File.CreateText(objPath))
            {
                writer.AutoFlush = true;

                await writer.WriteLineAsync("mtllib .\\materials.mtl");

                foreach (var obj in scene.Objects)
                {
                    WriteLine($"Extracting object with name: {obj.Name}...");
                    await obj.WriteToOBJAsync(writer);
                    WriteLine($"Wrote object with name: {obj.Name} to {objPath}.", OutputImportance.Verbose);
                }
            }

            Goodbye();
        }

        static async Task<Texture> OpenTextureFileAsync(MultiToolOptions options)
        {
            if (!File.Exists(options.InputFile))
            {
                WriteLine("Error: The specified input file does not exist. Check any paths or filenames to make sure they are correct.", OutputImportance.Error);
                return null;
            }

            Texture texture = null;
            try
            {
                var inputFile = File.OpenRead(options.InputFile);

                int width = -1;
                int height = -1;

                if (options.Dimensions != null)
                {
                    var tokens = options.Dimensions.Split('x');

                    width = int.Parse(tokens[0]);
                    height = int.Parse(tokens[1]);
                }

                switch (options.TextureFormat)
                {
                    case TextureFormat.DXT1:
                        texture = new DXT1Texture(-1, width, height, 1, Endianness.LittleEndian, inputFile);
                        break;
                    case TextureFormat.DXT3:
                        texture = new DXT3Texture(-1, width, height, 1, Endianness.LittleEndian, inputFile);
                        break;
                    case TextureFormat.DXT5:
                        texture = new DXT5Texture(-1, width, height, 1, Endianness.LittleEndian, inputFile);
                        break;
                    case TextureFormat.DDS:
                        var ddsInfo = new DDSInfo(inputFile);
                        await ddsInfo.LoadAsync();
                        texture = new DDSTexture(-1, ddsInfo);
                        break;
                    case TextureFormat.PNT:
                        var pntInfo = new PNTInfo(inputFile);
                        await pntInfo.LoadAsync();
                        texture = new PNTTexture(-1, pntInfo);
                        break;
                }

                return texture;
            }
            catch (Exception)
            {
                WriteLine("Error: The input file could not be parsed in the chosen format.", OutputImportance.Error);
                throw;
            }
        }

        static async Task RunMultiToolAsync(MultiToolOptions options)
        {
            WriteLine($"Running MultiTool, selected mode: {options.Mode}");
            switch (options.Mode)
            {
                case MultiToolMode.TEX:
                    var outputPath = options.InputFile + ".png";
                    var texture = await OpenTextureFileAsync(options);

                    var image = await texture.ReadImageAsync();
                    await image.SaveAsPngAsync(outputPath);

                    WriteLine($"Wrote output to {outputPath}");
                    break;
            }

            Goodbye();
        }

        static async Task RunTestAsync(TestOptions options)
        {
            //var obj = new OBJMesh(File.OpenText(options.ModelFile));
            //await obj.ParseAsync();

            //using (var writer = File.CreateText(options.OutputFile))
            //{
            //    int idx = 0;
            //    writer.AutoFlush = true;
            //    foreach (var strip in await obj.ToTriangleStripsAsync())
            //    {
            //        await writer.WriteLineAsync($"o strip_{idx++}");
            //        await strip.WriteToOBJAsync(writer);
            //    }
            //}

            var file = await OpenContainerAsync(options) as LSW1.PCXB.LevelContainer;

            // Texture resizing/injection
            var texture = new FormattedFile("textures\\dds", options.TextureFile);
            await texture.LoadAsync();

            await file.AddTextureAsync((int)texture.data.header.width.Value, (int)texture.data.header.height.Value, (int)texture.data.header.mipmapCount.Value, texture.Stream);

            await file.AddObjectAsync(603, Matrix4x4.CreateTranslation(-9, 0, 0));

            //var textureStream = await file.ResizeTexture(0, , texture.data.Context.Stream.Length);
            //texture.Stream.Seek(0, SeekOrigin.Begin);
            //await texture.Stream.CopyToAsync(textureStream);

            //// Vertex data resizing
            //var vtxStream = await file.ResizeVertexBlock(0, 1024);
            //var elemStream = await file.ResizeElementArray(0, 1024);
        }
    }
}
