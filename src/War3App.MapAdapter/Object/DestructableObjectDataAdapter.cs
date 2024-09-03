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
    public sealed class DestructableObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Destructable)";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            try
            {
                var destructableDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.DestructableDataPath);
                if (!File.Exists(destructableDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = destructableDataPath.GetFileNotFoundDiagnostics(),
                    };
                }
                
                var destructableMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.DestructableMetaDataPath);
                if (!File.Exists(destructableMetaDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = destructableMetaDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                DestructableObjectData destructableObjectData;
                try
                {
                    using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                    destructableObjectData = reader.ReadDestructableObjectData();
                }
                catch (Exception e)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ParseError,
                        Diagnostics = new[] { e.Message },
                    };
                }

                var shouldDowngrade = destructableObjectData.GetMinimumPatch() > context.TargetPatch.Patch;
                if (shouldDowngrade && !destructableObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Unadaptable,
                    };
                }

                var knownIds = new HashSet<int>();
                knownIds.AddItemsFromSylkTable(destructableDataPath, DataConstants.DestructableDataKeyColumn);

                var knownProperties = new HashSet<int>();
                knownProperties.AddItemsFromSylkTable(destructableMetaDataPath, DataConstants.MetaDataIdColumn);

                var diagnostics = new List<string>();

                var baseDestructables = new List<SimpleObjectModification>();
                foreach (var destructable in destructableObjectData.BaseDestructables)
                {
                    if (!knownIds.Contains(destructable.OldId))
                    {
                        diagnostics.Add($"Unknown base destructable: '{destructable.OldId.ToRawcode()}'");
                        continue;
                    }

                    if (destructable.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                    {
                        diagnostics.AddRange(destructable.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                        destructable.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                    }

                    baseDestructables.Add(destructable);
                }

                var newDestructables = new List<SimpleObjectModification>();
                foreach (var destructable in destructableObjectData.NewDestructables)
                {
                    if (!knownIds.Contains(destructable.OldId))
                    {
                        diagnostics.Add($"Unknown base destructable: '{destructable.OldId.ToRawcode()}'");
                        continue;
                    }

                    if (knownIds.Contains(destructable.NewId))
                    {
                        diagnostics.Add($"Conflicting destructable: '{destructable.NewId.ToRawcode()}'");
                        continue;
                    }

                    if (destructable.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                    {
                        diagnostics.AddRange(destructable.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                        destructable.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                    }

                    newDestructables.Add(destructable);
                }

                if (shouldDowngrade || diagnostics.Count > 0)
                {
                    var memoryStream = new MemoryStream();
                    using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                    writer.Write(new DestructableObjectData(destructableObjectData.FormatVersion)
                    {
                        BaseDestructables = baseDestructables,
                        NewDestructables = newDestructables,
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
                var destructableObjectData = reader.ReadDestructableObjectData();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                return JsonSerializer.Serialize(destructableObjectData, options);
            }
            catch (Exception e)
            {
                return $"{e.GetType().FullName}{System.Environment.NewLine}{e.Message}";
            }
        }
    }
}