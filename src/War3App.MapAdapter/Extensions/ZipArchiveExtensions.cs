using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace War3App.MapAdapter.Extensions
{
    public static class ZipArchiveExtensions
    {
        public static ZipArchiveEntry? GetEntryNormalized(this ZipArchive zipArchive, string filePath)
        {
            return zipArchive.Entries.SingleOrDefault(entry => string.Equals(
                entry.FullName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar),
                filePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase));
        }
    }
}