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
using Kaitai;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NuXtractor
{
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
        [Option('i', "input-file", Required = true, HelpText = "Path to a nux or nup file.")]
        public string InputFile { get; set; }

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
                NuxFile nux = NuxFile.FromFile(options.InputFile);


                string outputDir = options.InputFile + ".textures";
                Directory.CreateDirectory(outputDir);

                // Start reading textures one by one
                for (int i = 0; i < nux.Textures.Count; i++)
                {
                    using (var inputStream = new MemoryStream(nux.Textures[i].Data))
                    using (var inputReader = new BinaryReader(inputStream))
                    {
                        string outputPath = Path.Combine(outputDir, $"texture_{i}.{options.Mode}");

                        WriteLine($"Found texture #{i}; Dumping to file: {outputPath}");

                        using (var outputStream = File.Create(outputPath)) // Open file to write texture to
                        using (var outputWriter = new BinaryWriter(outputStream))
                        {
                            int paddingIndex;
                            do
                            {
                                byte[] chunk = inputReader.ReadBytes(256); // Data is padded to nearest multiple of 256, 
                                                                           // so only read that many bytes at a time.
                                if (chunk.Length == 0) break;

                                paddingIndex = Encoding.ASCII.GetString(chunk).IndexOf("padding");        // Find padding
                                outputWriter.Write(chunk, 0, paddingIndex == -1 ? chunk.Length : paddingIndex); // and exclude it
                            } while (paddingIndex == -1);
                        }

                        if (options.Mode != ExtractionMode.DXT1) continue;

                        try
                        {
                            WriteLine($"Trying to convert texture from file: {outputPath} to PNG...", OutputImportance.Verbose);

                            string convertedPath = Path.Combine(outputDir, $"texture_{i}.png");
                            using (var convertStream = File.OpenRead(outputPath))
                            using (var convertReader = new BinaryReader(convertStream))
                            using (var bitmap = DXTConvert.UncompressDXT1(convertReader, (int)nux.Textures[i].Width, (int)nux.Textures[i].Height))
                            using (var outputStream = new SKFileWStream(convertedPath))
                            {
                                WriteLine($"Conversion successful! Writing output to file: {convertedPath}", OutputImportance.Verbose);
                                SKPixmap.Encode(outputStream, bitmap, SKEncodedImageFormat.Png, 100);
                            }
                        }
                        catch (Exception)
                        {
                            WriteLine("Conversion Failed! Moving on to next texture...", OutputImportance.Error);
                        }
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
