using War3Net.IO.Mpq;

namespace War3App.MapAdapter
{
    public class OpenZipArchiveResult
    {
        public MpqArchive Archive { get; set; }

        public TargetPatch TargetPatch { get; set; }
    }
}