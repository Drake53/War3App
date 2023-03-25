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
    public sealed class AbilityObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Ability)";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;
        
        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var abilityDataPath = Path.Combine(targetPatch.GameDataPath, PathConstants.AbilityDataPath);
                if (!File.Exists(abilityDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = abilityDataPath.GetFileNotFoundDiagnostics(),
                    };
                }
                
                var abilityMetaDataPath = Path.Combine(targetPatch.GameDataPath, PathConstants.AbilityMetaDataPath);
                if (!File.Exists(abilityMetaDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = abilityMetaDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                MapAbilityObjectData mapAbilityObjectData;
                try
                {
                    using var reader = new BinaryReader(stream);
                    mapAbilityObjectData = reader.ReadMapAbilityObjectData();
                }
                catch (Exception e)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ParseError,
                        Diagnostics = new[] { e.Message },
                    };
                }

                var shouldDowngrade = mapAbilityObjectData.GetMinimumPatch() > targetPatch.Patch;
                if (shouldDowngrade && !mapAbilityObjectData.TryDowngrade(targetPatch.Patch))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Unadaptable,
                    };
                }

                var knownIds = new HashSet<int>();
                knownIds.AddItemsFromSylkTable(abilityDataPath, DataConstants.AbilityDataKeyColumn);

                var knownProperties = new HashSet<int>();
                knownProperties.AddItemsFromSylkTable(abilityMetaDataPath, DataConstants.MetaDataIdColumn);

                var diagnostics = new List<string>();

                var baseAbilities = new List<LevelObjectModification>();
                foreach (var ability in mapAbilityObjectData.BaseAbilities)
                {
                    if (!knownIds.Contains(ability.OldId))
                    {
                        diagnostics.Add($"Unknown base ability: '{ability.OldId.ToRawcode()}'");
                        continue;
                    }

                    if (ability.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                    {
                        diagnostics.AddRange(ability.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                        ability.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                    }

                    baseAbilities.Add(ability);
                }

                var newAbilities = new List<LevelObjectModification>();
                foreach (var ability in mapAbilityObjectData.NewAbilities)
                {
                    if (!knownIds.Contains(ability.OldId))
                    {
                        diagnostics.Add($"Unknown base ability: '{ability.OldId.ToRawcode()}'");
                        continue;
                    }

                    if (knownIds.Contains(ability.NewId))
                    {
                        diagnostics.Add($"Conflicting ability: '{ability.NewId.ToRawcode()}'");
                        continue;
                    }

                    if (ability.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                    {
                        diagnostics.AddRange(ability.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                        ability.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                    }

                    newAbilities.Add(ability);
                }

                if (shouldDowngrade || diagnostics.Count > 0)
                {
                    var memoryStream = new MemoryStream();
                    using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                    writer.Write(new MapAbilityObjectData(mapAbilityObjectData.FormatVersion)
                    {
                        BaseAbilities = baseAbilities,
                        NewAbilities = newAbilities,
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
                var abilityObjectData = reader.ReadAbilityObjectData();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                return JsonSerializer.Serialize(abilityObjectData, options);
            }
            catch (Exception e)
            {
                return $"{e.GetType().FullName}{System.Environment.NewLine}{e.Message}";
            }
        }
    }
}