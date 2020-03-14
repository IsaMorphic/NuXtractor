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
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NuXtractor
{
    enum FileFormat
    {
        NUPv1,
        NUPv2
    }

    enum ExtractionMode
    {
        DDS,
        DXT1
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

        [Option('f', "format", Required = true, HelpText = "The format of the specified input file.  Can be either NUPv1 or NUPv2")]
        public FileFormat Format { get; set; }

        [Option('m', "mode", Required = true, HelpText = "The extraction mode to use. Can be either DDS or DXT1.")]
        public ExtractionMode Mode { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions);
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

        static void RunOptions(Options options)
        {
            if (!File.Exists(options.InputFile))
            {
                WriteLine("The specified input file does not exist. Check any paths or filenames to make sure they are correct.", OutputImportance.Error);
                return;
            }

            try
            {
                ITextureContainer container = null;
                switch (options.Format)
                {
                    case FileFormat.NUPv1:
                        container = NupV1.FromFile(options.InputFile);
                        break;
                    case FileFormat.NUPv2:
                        container = NupV2.FromFile(options.InputFile);
                        break;
                }

                List<Texture> textures = container.GetTextures();

                string outputDir = options.InputFile + ".textures";
                Directory.CreateDirectory(outputDir);

                // Start reading textures one by one
                for (int i = 0; i < textures.Count; i++)
                {
                    switch (options.Mode)
                    {
                        case ExtractionMode.DDS:
                            string dumpPath = Path.Combine(outputDir, $"texture_{i}.dds");
                            WriteLine($"Found texture #{i}; Dumping to file: {dumpPath}");
                            File.WriteAllBytes(dumpPath, textures[i].Data);
                            break;
                        case ExtractionMode.DXT1:
                            using (var inputStream = new MemoryStream(textures[i].Data))
                            using (var inputReader = new BinaryReader(inputStream))
                            {
                                try
                                {
                                    WriteLine($"Found texture #{i}; Converting to PNG...");

                                    string outputPath = Path.Combine(outputDir, $"texture_{i}.png");
                                    using (var bitmap = DXTConvert.UncompressDXT1(inputReader, textures[i].Width, textures[i].Height))
                                    using (var outputStream = new SKFileWStream(outputPath))
                                    {
                                        WriteLine($"Conversion successful! Writing output to file: {outputPath}", OutputImportance.Verbose);
                                        SKPixmap.Encode(outputStream, bitmap, SKEncodedImageFormat.Png, 100);
                                    }
                                }
                                catch (Exception)
                                {
                                    WriteLine("Conversion Failed! Moving on to next texture...", OutputImportance.Error);
                                }
                            }
                            break;
                    }
                }

                WriteLine("Thanks for using NuXtractor, a tool created by Yodadude2003.", OutputImportance.Highlight);
                WriteLine("Please make sure to credit the tool and creator for any public usage of the textures it extracts.", OutputImportance.Highlight);
                WriteLine("For more software from Chosen Few Software, visit https://www.chosenfewsoftware.com", OutputImportance.Highlight);
                WriteLine("Copyright (C) 2020 Chosen Few Software", OutputImportance.Highlight);
            }
            catch (Exception)
            {
                WriteLine("An error occured during the extraction process. File may be in use, format may be bad, or textures are corrupt.", OutputImportance.Error);
            }
        }
    }
}
