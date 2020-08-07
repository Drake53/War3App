using System.IO;

namespace War3App.MapAdapter
{
    public sealed class AdaptResult
    {
        public MapFileStatus Status { get; set; }

        public Stream AdaptedFileStream { get; set; }
    }
}