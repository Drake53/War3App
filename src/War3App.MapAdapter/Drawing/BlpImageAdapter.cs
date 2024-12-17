using System.IO;

namespace War3App.MapAdapter.Drawing
{
    public sealed class BlpImageAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Image File (BLP)";

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