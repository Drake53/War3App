using System.Collections.Generic;

using War3Net.Build.Common;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter
{
    public class OpenArchiveResult
    {
        public OpenArchiveResult()
        {
            Files = new();
            NestedArchives = new();
        }

        public List<MapFile> Files { get; set; }

        public List<MpqArchive> NestedArchives { get; set; }

        public GamePatch? OriginPatch { get; set; }
    }
}