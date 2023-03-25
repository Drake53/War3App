using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Script
{
    public sealed class MapTriggersAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Triggers";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var triggerDataPath = Path.Combine(targetPatch.GameDataPath, PathConstants.TriggerDataPath);
                if (!File.Exists(triggerDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = triggerDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                using var reader = new BinaryReader(stream);
                var mapTriggers = reader.ReadMapTriggers();

                try
                {
                    var triggerDataText = File.ReadAllText(triggerDataPath);
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

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch)
        {
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                var mapTriggers = reader.ReadMapTriggers();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                return JsonSerializer.Serialize(mapTriggers, options);
            }
            catch (Exception e)
            {
                return $"{e.GetType().FullName}{System.Environment.NewLine}{e.Message}";
            }
        }
    }
}