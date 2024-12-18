using System.IO;

namespace War3App.MapAdapter.Drawing
{
    public sealed class TgaImageAdapter : IMapFileAdapter
    {
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