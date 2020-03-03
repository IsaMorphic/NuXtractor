using System;
using System.IO;
using System.Linq;
using System.Text;

namespace NuXtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var inputStream = File.OpenRead(args[0]))
            using (var reader = new BinaryReader(inputStream))
            {
                byte[] header = reader.ReadBytes(64); // Read header containing offsets to certain sections of data in the file

                uint offset = BitConverter.ToUInt32(header.Skip(8).Take(4).ToArray()); // Extract texture data offset

                reader.BaseStream.Seek(offset, SeekOrigin.Current); // Navigate to that offset

                byte[] textureIndex = reader.ReadBytes(4096); // Read index of texture data (contains several structured entries 
                                                              // that consist of an offset and an array of texture dimensions)

                int textureCount = textureIndex
                    .Select((v, i) => new { Index = i, Value = v })
                    .GroupBy(x => x.Index / 4)
                    .Select(x => x.Select(v => v.Value).ToArray())
                    .Select(arr => BitConverter.ToUInt32(arr))
                    .Where(num => num <= 1024)
                    .Count();  // Only interested in how many textures there are, although this method may be inaccurate.

                // Start reading textures one by one
                for (int i = 0; i < textureCount; i++)
                {
                    using (var outputStream = File.Create($"texture_{i}.data")) // Open file to write texture to
                    using (var writer = new BinaryWriter(outputStream))
                    {
                        int paddingIndex;
                        do
                        {
                            byte[] chunk = reader.ReadBytes(256); // Data is padded to nearest multiple of 256, 
                                                                  // so only read that many bytes at a time.
                            paddingIndex = Encoding.ASCII.GetString(chunk).IndexOf("padding");        // Find padding
                            writer.Write(chunk, 0, paddingIndex == -1 ? chunk.Length : paddingIndex); // and exclude it
                        } while (paddingIndex == -1);
                    }
                }
            }
        }
    }
}
