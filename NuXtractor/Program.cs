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
using NuXtractor.Formats;
using NuXtractor.Textures;
using NuXtractor.Textures.DXT;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NuXtractor
{
    enum FileFormat
    {
        CSCgc,
        GSCps2,
        NUXv1,
        NUXv2,
        HGXv1
    }

    enum TextureFormat
    {
        DDS,
        DXTn,
        CTX,
        PNT,
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

        [Option('f', "file-format", Required = true, HelpText = "The format of the specified input file.  Can be CSCgc, GSCps2, NUPv1, NUPv2 or HGPv1")]
        public FileFormat FileFormat { get; set; }

        [Option('t', "texture-format", Required = true, HelpText = "The format of the textures contained in the input file.  Can be DDS, DXTn, CTX, or PNT")]
        public TextureFormat TextureFormat { get; set; }

        [Option('m', "mode", Required = true, HelpText = "The extraction mode to use. Can be either DUMP or CONV.")]
        public ExtractionMode Mode { get; set; }

        [Option('p', "write-patch", Required = false, HelpText = "Flag indicating whether to write a ModLoader compatible patch file with the relevant file changes. Only applies when using INJC or INJD modes.")]
        public bool WritePatch { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args)
                .MapResult(opts => RunAsync(opts), err => Task.CompletedTask);
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

        static async Task RunAsync(Options options)
        {
            if (!File.Exists(options.InputFile))
            {
                WriteLine("Error: The specified input file does not exist. Check any paths or filenames to make sure they are correct.", OutputImportance.Error);
                return;
            }

            FormattedFile file = null;
            try
            {
                switch (options.FileFormat)
                {
                    case FileFormat.NUXv1:
                        file = new XboxContainerV1("nux_v1", options.InputFile);
                        break;
                    case FileFormat.HGXv1:
                        file = new XboxContainerV1("hgx_v1", options.InputFile);
                        break;
                }
                await file.LoadAsync();
            }
            catch (Exception)
            {
                WriteLine("Error: The input file could not be parsed in the chosen format.", OutputImportance.Error);
                return;
            }

            List<Texture> textures = null;
            try
            {

                switch (options.TextureFormat)
                {
                    case TextureFormat.DXTn:
                        textures = await (file as ITextureContainer<DXT1Texture>).GetTexturesAsync();
                        break;
                    case TextureFormat.DDS:
                        textures = await (file as ITextureContainer<DDSTexture>).GetTexturesAsync();
                        break;
                }
            }
            catch (Exception)
            {
                WriteLine("Error: The input file does not support the specified texture format.", OutputImportance.Error);
                return;
            }

            string dir = options.InputFile + ".textures";
            Directory.CreateDirectory(dir);


            Stream patchFile = Stream.Null;

            if(options.WritePatch && (options.Mode == ExtractionMode.INJC || options.Mode == ExtractionMode.INJD))
                patchFile = File.Create(options.InputFile + ".patch");
            
            // Start reading textures one by one
            for (int i = 0; i < textures.Count; i++)
            {
                using (var texture = textures[i])
                {
                    switch (options.Mode)
                    {
                        case ExtractionMode.DUMP:
                            try
                            {
                                string dumpPath = Path.Combine(dir, $"texture_{i}.{options.TextureFormat}");
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
                                string injdPath = Path.Combine(dir, $"texture_{i}.{options.TextureFormat}");
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

            WriteLine("Thanks for using NuXtractor, a tool created by Yodadude2003.", OutputImportance.Highlight);
            WriteLine("Please make sure to credit the tool and creator for any public usage of the textures it extracts.", OutputImportance.Highlight);
            WriteLine("For more software from Chosen Few Software, visit https://www.chosenfewsoftware.com", OutputImportance.Highlight);
            WriteLine("Copyright (C) 2020 Chosen Few Software", OutputImportance.Highlight);
        }
    }
}
