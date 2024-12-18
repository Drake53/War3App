using System.IO;

namespace War3App.MapAdapter.Modeling
{
    public sealed class TextModelAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Text Model";

        public string DefaultFileName => "file.mdl";

        public bool IsTextFile => true;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            // TODO
            return MapFileStatus.Unknown;
        }
    }
}