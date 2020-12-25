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
using NuXtractor.Converters;
using NuXtractor.Models;
using NuXtractor.Scenes;
using NuXtractor.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NuXtractor
{
    enum FileFormat
    {
        NUXv0,
        NUXv1,
        NUPv1,
        HGPv1,
        HGXv1
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

        [Option('f', "file-format", Required = true, HelpText = "The format of the specified input file.  Can be NUXv0, NUXv1 or NUPv1")]
        public FileFormat FileFormat { get; set; }
    }

    [Verb("test", HelpText = "test")]
    class TestOptions : Options
    {
        [Option('t', "texture-file", Required = true, HelpText = "Path to new texture file.")]
        public string TextureFile { get; set; }
    }

    [Verb("all", HelpText = "Extract all data from a TT Games data container")]
    class SceneOptions : Options
    {
    }

    [Verb("models", HelpText = "Extract model data from a TT Games data container")]
    class ModelOptions : Options
    {
    }

    [Verb("convert", HelpText = "Convert a TT Games data container to another format")]
    class ConvertOptions : Options
    {
    }

    [Verb("textures", HelpText = "Extract and inject textures into and from a TT Games data container.")]
    class TextureOptions : Options
    {
        [Option('m', "mode", Required = true, HelpText = "The extraction mode to use. Can be DUMP, CONV(ert), INJ(ect)C(onvert), or INJ(ect)D(ump).")]
        public ExtractionMode Mode { get; set; }

        [Option('p', "write-patch", Required = false, HelpText = "Flag indicating whether to write a ModLoader compatible patch file with the relevant file changes. Only applies when using INJC or INJD modes.")]
        public bool WritePatch { get; set; }
    }



    class Program
    {
        static async Task Main(string[] args)
        {
            await Parser.Default
                .ParseArguments<TextureOptions, ModelOptions, SceneOptions, ConvertOptions, TestOptions>(args)
                .MapResult(
                    (TextureOptions opts) => RunTexturesAsync(opts),
                    (ModelOptions opts) => RunModelsAsync(opts),
                    (SceneOptions opts) => RunAllAsync(opts),
                    (ConvertOptions opts) => RunConvertAsync(opts),
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

        static async Task<FormattedFile> OpenContainerAsync(Options options)
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
                    case FileFormat.NUXv0:
                        file = new Formats.V1.NUXContainer("nux_v0", options.InputFile);
                        break;
                    case FileFormat.NUXv1:
                        file = new Formats.V1.NUXContainer("nux_v1", options.InputFile);
                        break;
                    case FileFormat.NUPv1:
                        file = new Formats.V1.NUPContainer("nup_v1", options.InputFile);
                        break;
                    case FileFormat.HGPv1:
                        file = new Formats.V1.HGPContainer(options.InputFile);
                        break;
                    case FileFormat.HGXv1:
                        file = new Formats.V1.HGXContainer(options.InputFile);
                        break;
                }

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
            WriteLine("Please make sure to credit the tool and creator for any public usage of the textures it extracts.", OutputImportance.Highlight);
            WriteLine("For more software from Chosen Few Software, visit https://www.chosenfewsoftware.com", OutputImportance.Highlight);
            WriteLine("Copyright (C) 2020 Chosen Few Software", OutputImportance.Highlight);
        }

        static async Task RunTestAsync(TestOptions options) 
        {
            var file = await OpenContainerAsync(options) as Formats.V1.LevelContainer;

            // Texture resizing/injection
            var texture = new FormattedFile("dds_texture", options.TextureFile);
            await texture.LoadAsync();

            var textureStream = await file.ResizeTexture(0, (int)texture.data.header.width.Value, (int)texture.data.header.height.Value, (int)texture.data.header.mipmapCount.Value, texture.data.Context.Stream.Length);
            texture.Stream.Seek(0, SeekOrigin.Begin);
            await texture.Stream.CopyToAsync(textureStream);

            // Vertex data resizing
            var vtxStream = await file.ResizeVertexBlock(0, 1024);
        }

        static async Task RunTexturesAsync(TextureOptions options)
        {
            var file = await OpenContainerAsync(options) as ITextureContainer;
            await ProcessTexturesAsync(options, file);
            Goodbye();
        }

        static async Task RunModelsAsync(ModelOptions options)
        {
            var file = await OpenContainerAsync(options) as IModelContainer;
            await ProcessModelsAsync(options, file);
            Goodbye();
        }

        static async Task RunAllAsync(SceneOptions options)
        {
            var file = await OpenContainerAsync(options) as ISceneContainer;
            var scene = await file.GetSceneAsync();

            string dir = options.InputFile + ".extracted";
            Directory.CreateDirectory(dir);

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

        static Task RunConvertAsync(ConvertOptions options)
        {
            var converter = new PrototypeNUXConverter(options.InputFile);
            return converter.ConvertAsync();
        }

        static async Task ProcessModelsAsync(ModelOptions options, IModelContainer container)
        {
            string dir = options.InputFile + ".models";
            Directory.CreateDirectory(dir);

            for (int i = 0; i < container.ModelCount; i++)
            {
                var model = await container.GetModelAsync(i);
                WriteLine($"Found model #{i}; Extracting to OBJ...");

                string filePath = Path.Combine(dir, $"model_{i}.obj");
                using (var writer = File.CreateText(filePath))
                {
                    await model.WriteToOBJAsync(writer);
                }

                WriteLine($"Conversion successful! Wrote output to file: {filePath}", OutputImportance.Verbose);
            }
        }

        static async Task ProcessTexturesAsync(TextureOptions options, ITextureContainer container)
        {
            string dir = options.InputFile + ".textures";
            Directory.CreateDirectory(dir);

            Stream patchFile = Stream.Null;

            if (options.WritePatch && (options.Mode == ExtractionMode.INJC || options.Mode == ExtractionMode.INJD))
                patchFile = File.Create(options.InputFile + ".patch");

            // Start reading textures one by one
            for (int i = 0; i < container.TextureCount; i++)
            {
                using (var texture = await container.GetTextureAsync(i))
                {
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
            }

            patchFile.Dispose();
        }
    }
}
