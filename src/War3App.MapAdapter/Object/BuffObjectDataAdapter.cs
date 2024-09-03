using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Object;
using War3Net.Common.Extensions;

namespace War3App.MapAdapter.Object
{
    public sealed class BuffObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Buff)";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            try
            {
                var buffDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.BuffDataPath);
                if (!File.Exists(buffDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = buffDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                var buffMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.BuffMetaDataPath);
                if (!File.Exists(buffMetaDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = buffMetaDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                BuffObjectData buffObjectData;
                try
                {
                    using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                    buffObjectData = reader.ReadBuffObjectData();
                }
                catch (Exception e)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ParseError,
                        Diagnostics = new[] { e.Message },
                    };
                }

                var shouldDowngrade = buffObjectData.GetMinimumPatch() > context.TargetPatch.Patch;
                if (shouldDowngrade && !buffObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Unadaptable,
                    };
                }

                var knownIds = new HashSet<int>();
                knownIds.AddItemsFromSylkTable(buffDataPath, DataConstants.BuffDataKeyColumn);

                var knownProperties = new HashSet<int>();
                knownProperties.AddItemsFromSylkTable(buffMetaDataPath, DataConstants.MetaDataIdColumn);

                var diagnostics = new List<string>();

                var baseBuffs = new List<SimpleObjectModification>();
                foreach (var buff in buffObjectData.BaseBuffs)
                {
                    if (!knownIds.Contains(buff.OldId))
                    {
                        diagnostics.Add($"Unknown base buff: '{buff.OldId.ToRawcode()}'");
                        continue;
                    }

                    if (buff.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                    {
                        diagnostics.AddRange(buff.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                        buff.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                    }

                    baseBuffs.Add(buff);
                }

                var newBuffs = new List<SimpleObjectModification>();
                foreach (var buff in buffObjectData.NewBuffs)
                {
                    if (!knownIds.Contains(buff.OldId))
                    {
                        diagnostics.Add($"Unknown base buff: '{buff.OldId.ToRawcode()}'");
                        continue;
                    }

                    if (knownIds.Contains(buff.NewId))
                    {
                        diagnostics.Add($"Conflicting buff: '{buff.NewId.ToRawcode()}'");
                        continue;
                    }

                    if (buff.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                    {
                        diagnostics.AddRange(buff.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                        buff.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                    }

                    newBuffs.Add(buff);
                }

                if (shouldDowngrade || diagnostics.Count > 0)
                {
                    var memoryStream = new MemoryStream();
                    using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                    writer.Write(new BuffObjectData(buffObjectData.FormatVersion)
                    {
                        BaseBuffs = baseBuffs,
                        NewBuffs = newBuffs,
                    });

                    return new AdaptResult
                    {
                        Status = MapFileStatus.Adapted,
                        Diagnostics = diagnostics.ToArray(),
                        AdaptedFileStream = memoryStream,
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
            catch (NotSupportedException e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.Unadaptable,
                    Diagnostics = new[] { e.Message },
                };
            }
            catch (Exception e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.AdapterError,
                    Diagnostics = new[] { e.Message },
                };
            }
        }

        public string SerializeFileToJson(Stream stream, GamePatch gamePatch)
        {
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                var buffObjectData = reader.ReadBuffObjectData();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                return JsonSerializer.Serialize(buffObjectData, options);
            }
            catch (Exception e)
            {
                return $"{e.GetType().FullName}{System.Environment.NewLine}{e.Message}";
            }
        }
    }
}