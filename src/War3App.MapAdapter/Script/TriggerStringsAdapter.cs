using System.IO;

namespace War3App.MapAdapter.Script
{
    public sealed class TriggerStringsAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Trigger Strings";

        public bool IsTextFile => true;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            return MapFileStatus.Compatible;
        }
    }
}