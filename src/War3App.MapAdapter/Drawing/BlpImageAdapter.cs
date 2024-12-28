using System.IO;

namespace War3App.MapAdapter.Drawing
{
    public sealed class BlpImageAdapter : IMapFileAdapter
    {
        private static readonly BlpImageAdapter _instance = new();

        private BlpImageAdapter()
        {
        }

        public static BlpImageAdapter Instance => _instance;

        public string MapFileDescription => "Image File (BLP)";

        public string DefaultFileName => "file.blp";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            // TODO
            // var blpFile = BlpFile.Parse(stream);
            return MapFileStatus.Compatible;
        }
    }
}