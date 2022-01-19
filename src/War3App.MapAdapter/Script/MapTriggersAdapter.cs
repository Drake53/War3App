using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using War3Net.Build.Common;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Script
{
    public sealed class MapTriggersAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Triggers";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var mapTriggers = reader.ReadMapTriggers();

                try
                {
                    var triggerDataText = File.ReadAllText(Path.Combine(targetPatch.GameDataPath, PathConstants.TriggerDataPath));
                    var triggerDataReader = new StringReader(triggerDataText);
                    var triggerData = triggerDataReader.ReadTriggerData();

                    stream.Position = 0;
                    reader.ReadMapTriggers(triggerData);
                }
                catch (KeyNotFoundException e)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Incompatible,
                        Diagnostics = new[] { e.Message },
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

                if (mapTriggers.GetMinimumPatch() <= targetPatch.Patch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (mapTriggers.TryDowngrade(targetPatch.Patch))
                    {
                        var newMapTriggersFileStream = new MemoryStream();
                        using var writer = new BinaryWriter(newMapTriggersFileStream, new UTF8Encoding(false, true), true);
                        writer.Write(mapTriggers);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = newMapTriggersFileStream,
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
    }
}