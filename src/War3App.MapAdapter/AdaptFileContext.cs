using War3Net.Build.Common;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter
{
    public class AdaptFileContext
    {
        public string? FileName { get; set; }

        public MpqArchive Archive { get; set; }

        public TargetPatch TargetPatch { get; set; }

        public GamePatch OriginPatch { get; set; }
    }
}