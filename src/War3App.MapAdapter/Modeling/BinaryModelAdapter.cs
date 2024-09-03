using System;
using System.IO;
using System.Text;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Modeling
{
    public sealed class BinaryModelAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Binary Model";

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
                            return new AdaptResult
                            {
                                Status = MapFileStatus.Unadaptable,
                                Diagnostics = new[] { $"Model version not supported: v{version}" },
                            };
                        }
                        else
                        {
                            return new AdaptResult
                            {
                                Status = MapFileStatus.Compatible,
                            };
                        }
                    }
                    else
                    {
                        reader.ReadBytes(chunkSize);
                    }
                }

                // Unable to find VERS chunk.
                return new AdaptResult
                {
                    // Status = MapFileStatus.ParseError,
                    Status = MapFileStatus.Unknown,
                };
            }
            catch (Exception e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                    Diagnostics = new[] { e.Message },
                };
            }
        }

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch)
        {
            throw new NotSupportedException();
        }
    }
}