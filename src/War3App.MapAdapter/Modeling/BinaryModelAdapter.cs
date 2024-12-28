using System;
using System.IO;
using System.Text;

using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Modeling
{
    public sealed class BinaryModelAdapter : IMapFileAdapter
    {
        private static readonly BinaryModelAdapter _instance = new();

        private BinaryModelAdapter()
        {
        }

        public static BinaryModelAdapter Instance => _instance;

        public string MapFileDescription => "Binary Model";

        public string DefaultFileName => "file.mdx";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            try
            {
                // TODO: War3Net.Modeling.BinaryModelParser.Parse(stream)

                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                reader.ReadInt32();

                while (stream.Length - stream.Position > 8)
                {
                    // Find VERS chunk.
                    var chunkTag = new string(reader.ReadChars(4));
                    var chunkSize = reader.ReadInt32();
                    if (chunkTag == "VERS")
                    {
                        var version = reader.ReadUInt32();
                        if (version > 800 && context.TargetPatch.Patch < GamePatch.v1_32_0)
                        {
                            context.ReportDiagnostic(DiagnosticRule.BinaryModel.NotSupported, version, GamePatch.v1_32_0.PrettyPrint());

                            return MapFileStatus.Incompatible;
                        }
                        else
                        {
                            return MapFileStatus.Compatible;
                        }
                    }
                    else
                    {
                        reader.ReadBytes(chunkSize);
                    }
                }

                // Unable to find VERS chunk.
                return MapFileStatus.Error;
            }
            catch (Exception e)
            {
                return context.ReportParseError(e);
            }
        }
    }
}