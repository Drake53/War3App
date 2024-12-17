using System.IO;

namespace War3App.MapAdapter.Mpq
{
    public sealed class AttributesAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "MPQ Attributes";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            return MapFileStatus.Removed;
        }
    }
}