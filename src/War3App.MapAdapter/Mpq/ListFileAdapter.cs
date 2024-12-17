using System.IO;

namespace War3App.MapAdapter.Mpq
{
    public sealed class ListFileAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "MPQ ListFile";

        public bool IsTextFile => true;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            return MapFileStatus.Compatible;
        }
    }
}