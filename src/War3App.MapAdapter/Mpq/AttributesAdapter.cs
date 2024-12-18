using System.IO;

using War3Net.IO.Mpq;

namespace War3App.MapAdapter.Mpq
{
    public sealed class AttributesAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "MPQ Attributes";

        public string DefaultFileName => Attributes.FileName;

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            return MapFileStatus.Removed;
        }
    }
}