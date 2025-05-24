using System.IO;
using System.IO.Compression;

namespace War3App.MapAdapter.WinForms.Extensions
{
    public static class ZipArchiveExtensions
    {
        public static void AddEntry(this ZipArchive zipArchive, string entryName, byte[] content)
        {
            var entry = zipArchive.CreateEntry(entryName);

            using var stream = entry.Open();

            stream.Write(content);
        }

        public static void AddEntry(this ZipArchive zipArchive, string entryName, Stream content)
        {
            var entry = zipArchive.CreateEntry(entryName);

            using var stream = entry.Open();

            content.CopyTo(stream);
        }

        public static void AddEntry(this ZipArchive zipArchive, string entryName, string content)
        {
            var entry = zipArchive.CreateEntry(entryName);

            using var stream = entry.Open();
            using var writer = new StreamWriter(stream);

            writer.Write(content);
        }

        public static void AddFile(this ZipArchive zipArchive, string fileName, string directoryPathInArchive)
        {
            zipArchive.CreateEntryFromFile(fileName, Path.Combine(directoryPathInArchive, Path.GetFileName(fileName)));
        }
    }
}