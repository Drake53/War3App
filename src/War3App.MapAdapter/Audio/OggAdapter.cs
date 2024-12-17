using System.IO;

using War3App.MapAdapter.Diagnostics;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public sealed class OggAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Audio File (OGG)";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            if (context.TargetPatch.Patch < GamePatch.v1_30_0 || context.TargetPatch.Patch > GamePatch.v1_30_4)
            {
                context.ReportDiagnostic(DiagnosticRule.Ogg.NotSupported);

                return MapFileStatus.Incompatible;
            }

            return MapFileStatus.Compatible;
        }
    }
}