using System.IO;
using System.IO.Compression;

namespace War3App.MapAdapter.Extensions
{
    public static class ZipArchiveEntryExtensions
    {
        public static void Extract(this ZipArchiveEntry entry, Stream stream)
        {
            using var sourceStream = entry.Open();

            sourceStream.CopyTo(stream);
        }
    }
}