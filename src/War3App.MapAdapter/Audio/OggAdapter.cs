using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public sealed class OggAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Audio File (OGG)";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            return new AdaptResult
            {
                Status = targetPatch.Patch >= GamePatch.v1_30_0 && targetPatch.Patch <= GamePatch.v1_30_4 ? MapFileStatus.Compatible : MapFileStatus.Unadaptable,
            };
        }
    }
}