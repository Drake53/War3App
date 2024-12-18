using System.IO;

using War3App.MapAdapter.Diagnostics;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Audio
{
    public sealed class FlacAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Audio File (FLAC)";

        public string DefaultFileName => "file.flac";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            if (context.TargetPatch.Patch < GamePatch.v1_32_0)
            {
                context.ReportDiagnostic(DiagnosticRule.Flac.NotSupported);

                return MapFileStatus.Incompatible;
            }

            return MapFileStatus.Compatible;
        }
    }
}