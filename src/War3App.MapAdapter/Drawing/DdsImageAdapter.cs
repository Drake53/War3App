using System.IO;

using War3App.MapAdapter.Diagnostics;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Drawing
{
    public sealed class DdsImageAdapter : IMapFileAdapter
    {
        private static readonly DdsImageAdapter _instance = new();

        private DdsImageAdapter()
        {
        }

        public static DdsImageAdapter Instance => _instance;

        public string MapFileDescription => "Image File (DDS)";

        public string DefaultFileName => "file.dds";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            if (context.TargetPatch.Patch < GamePatch.v1_32_0)
            {
                context.ReportDiagnostic(DiagnosticRule.DdsImage.NotSupported);

                return MapFileStatus.Incompatible;
            }

            return MapFileStatus.Compatible;
        }
    }
}