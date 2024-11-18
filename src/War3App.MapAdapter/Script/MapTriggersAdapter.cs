using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Script;

namespace War3App.MapAdapter.Script
{
    public sealed class MapTriggersAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Triggers";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            try
            {
                var triggerDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.TriggerDataPath);
                if (!File.Exists(triggerDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = triggerDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                var mapTriggers = reader.ReadMapTriggers();

                var triggerDataText = File.ReadAllText(triggerDataPath);
                var triggerDataReader = new StringReader(triggerDataText);
                var triggerData = triggerDataReader.ReadTriggerData();

                try
                {
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

                var supportedVariableTypes = triggerData.TriggerTypes.Values
                    .Where(triggerType => triggerType.UsableAsGlobalVariable)
                    .Select(triggerType => triggerType.TypeName)
                    .ToHashSet(StringComparer.Ordinal);

                var unsupportedVariableBaseTypes = TriggerData.Default.TriggerTypes.Values
                    .Where(triggerType => !supportedVariableTypes.Contains(triggerType.TypeName))
                    .Where(triggerType => !string.IsNullOrEmpty(triggerType.BaseType))
                    .ToDictionary(
                        triggerType => triggerType.TypeName,
                        triggerType => triggerType.BaseType!,
                        StringComparer.Ordinal);

                var variableTypeDiagnostics = new List<string>();
                var hasUnsupportedVariableTypes = false;

                foreach (var variableDefinition in mapTriggers.Variables)
                {
                    if (!supportedVariableTypes.Contains(variableDefinition.Type))
                    {
                        if (unsupportedVariableBaseTypes.TryGetValue(variableDefinition.Type, out var baseType) &&
                            supportedVariableTypes.Contains(baseType))
                        {
                            variableTypeDiagnostics.Add($"Changed type of variable '{variableDefinition.Name}' from '{variableDefinition.Type}' to '{baseType}'");
                            variableDefinition.Type = baseType;
                        }
                        else
                        {
                            variableTypeDiagnostics.Add($"Variable '{variableDefinition.Name}' is of unsupported type '{variableDefinition.Type}'");
                            hasUnsupportedVariableTypes = true;
                        }
                    }
                }

                if (hasUnsupportedVariableTypes)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Incompatible,
                        Diagnostics = variableTypeDiagnostics.ToArray(),
                    };
                }

                var mustDowngrade = mapTriggers.GetMinimumPatch() > context.TargetPatch.Patch;
                if (!mustDowngrade && variableTypeDiagnostics.Count == 0)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (!mustDowngrade || mapTriggers.TryDowngrade(context.TargetPatch.Patch))
                    {
                        var newMapTriggersFileStream = new MemoryStream();
                        using var writer = new BinaryWriter(newMapTriggersFileStream, new UTF8Encoding(false, true), true);
                        writer.Write(mapTriggers);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = newMapTriggersFileStream,
                            Diagnostics = variableTypeDiagnostics.Count > 0 ? variableTypeDiagnostics.ToArray() : null,
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