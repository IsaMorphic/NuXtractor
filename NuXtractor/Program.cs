using CommandLine;
using SkiaSharp;
using System;
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

    class Options
    {
        [Option('i', "input-file", Required = true, HelpText = "The input file.")]
        public string InputFile { get; set; }

        [Option('m', "mode", Required = true, HelpText = "The extraction mode to use.")]
        public ExtractionMode Mode { get; set; }

        [Option('o', "offset", Required = false, HelpText = "Adjust this if texture sizes are incorrect. Should be between 0 and 3.", Default = 0)]
        public int Offset { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions);
        }

        static void RunOptions(Options options)
        {
            using (var inputStream = File.OpenRead(options.InputFile))
            using (var inputReader = new BinaryReader(inputStream))
            {
                string outputDir = options.InputFile + ".textures";
                Directory.CreateDirectory(outputDir);

                byte[] header = inputReader.ReadBytes(64); // Read header containing offsets to certain sections of data in the file

                uint offset = BitConverter.ToUInt32(header.Skip(8).Take(4).ToArray()); // Extract texture data offset

                inputReader.BaseStream.Seek(offset, SeekOrigin.Current); // Navigate to that offset

                byte[] textureIndexRaw = inputReader.ReadBytes(4096); // Read index of texture data (contains several structured entries 
                                                                      // that consist of an offset and an array of texture dimensions)

                uint[] textureIndex = textureIndexRaw
                    .Select((v, i) => new { Index = i, Value = v })
                    .GroupBy(x => x.Index / 4)
                    .Select(x => x.Select(v => v.Value).ToArray())
                    .Select(arr => BitConverter.ToUInt32(arr))
                    .Where(num => num <= 1024).Skip(2)
                    .Where((num, i) => i % 4 == options.Offset)
                    .ToArray();

                // Start reading textures one by one
                for (int i = 0; i < textureIndex.Length; i++)
                {
                    string outputPath = Path.Combine(outputDir, $"texture_{i}.{options.Mode}");
                    using (var outputStream = File.Create(outputPath)) // Open file to write texture to
                    using (var outputWriter = new BinaryWriter(outputStream))
                    {
                        int paddingIndex;
                        do
                        {
                            byte[] chunk = inputReader.ReadBytes(256); // Data is padded to nearest multiple of 256, 
                                                                       // so only read that many bytes at a time.
                            paddingIndex = Encoding.ASCII.GetString(chunk).IndexOf("padding");        // Find padding
                            outputWriter.Write(chunk, 0, paddingIndex == -1 ? chunk.Length : paddingIndex); // and exclude it
                        } while (paddingIndex == -1);
                    }

                    if (options.Mode != ExtractionMode.DXT1) continue;

                    using (var convertStream = File.OpenRead(outputPath))
                    using (var convertReader = new BinaryReader(convertStream))
                    using (var bitmap = DXTConvert.UncompressDXT1(convertReader, (int)textureIndex[i]))
                    using (var outputStream = new SKFileWStream(Path.Combine(outputDir, $"texture_{i}.png")))
                    {
                        SKPixmap.Encode(outputStream, bitmap, SKEncodedImageFormat.Png, 100);
                    }
                }
            }
        }
    }
}
