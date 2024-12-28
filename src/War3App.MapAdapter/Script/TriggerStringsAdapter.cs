using System.IO;

using War3Net.Build.Script;

namespace War3App.MapAdapter.Script
{
    public sealed class TriggerStringsAdapter : IMapFileAdapter
    {
        private static readonly TriggerStringsAdapter _instance = new();

        private TriggerStringsAdapter()
        {
        }

        public static TriggerStringsAdapter Instance => _instance;

        public string MapFileDescription => "Trigger Strings";

        public string DefaultFileName => TriggerStrings.MapFileName;

        public bool IsTextFile => true;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            return MapFileStatus.Compatible;
        }
    }
}