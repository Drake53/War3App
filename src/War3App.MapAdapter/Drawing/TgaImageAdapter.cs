using System.IO;

namespace War3App.MapAdapter.Drawing
{
    public sealed class TgaImageAdapter : IMapFileAdapter
    {
        private static readonly TgaImageAdapter _instance = new();

        private TgaImageAdapter()
        {
        }

        public static TgaImageAdapter Instance => _instance;

        public string MapFileDescription => "Image File (TGA)";

        public string DefaultFileName => "file.tga";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            return MapFileStatus.Compatible;
        }
    }
}