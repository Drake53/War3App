using System.IO;

namespace War3App.MapAdapter.Modeling
{
    public sealed class TextModelAdapter : IMapFileAdapter
    {
        private static readonly TextModelAdapter _instance = new();

        private TextModelAdapter()
        {
        }

        public static TextModelAdapter Instance => _instance;

        public string MapFileDescription => "Text Model";

        public string DefaultFileName => "file.mdl";

        public bool IsTextFile => true;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            // TODO
            return MapFileStatus.Inconclusive;
        }
    }
}