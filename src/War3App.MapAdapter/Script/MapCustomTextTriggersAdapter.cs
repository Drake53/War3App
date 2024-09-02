using System;
using System.IO;
using System.Text;

using War3Net.Build.Common;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Script
{
    public sealed class MapCustomTextTriggersAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Custom Text Triggers";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            var encoding = new UTF8Encoding(false, true);

            try
            {
                using var reader = new BinaryReader(stream);
                var mapCustomTextTriggers = reader.ReadMapCustomTextTriggers(encoding);
                if (mapCustomTextTriggers.GetMinimumPatch() <= context.TargetPatch.Patch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (mapCustomTextTriggers.TryDowngrade(context.TargetPatch.Patch))
                    {
                        var newMapCustomTextTriggersFileStream = new MemoryStream();
                        using var writer = new BinaryWriter(newMapCustomTextTriggersFileStream, encoding, true);
                        writer.Write(mapCustomTextTriggers, encoding);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = newMapCustomTextTriggersFileStream,
                        };
                    }
                    else
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Unadaptable,
                        };
                    }
                }
                catch
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.AdapterError,
                    };
                }
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