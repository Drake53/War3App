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

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            try
            {
                if (context.FileName is null)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.AdapterError,
                        Diagnostics = new[] { "Invalid context: FileName should be known to run this adapter." },
                    };
                }

                var isSkinFile = context.FileName.EndsWith($"Skin{AbilityObjectData.FileExtension}");
                var isSkinFileSupported = context.TargetPatch.Patch >= GamePatch.v1_33_0;

                if (isSkinFile && !isSkinFileSupported)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Removed,
                    };
                }

                var abilityDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.AbilityDataPath);
                if (!File.Exists(abilityDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = abilityDataPath.GetFileNotFoundDiagnostics(),
                    };
                }
                
                var abilityMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.AbilityMetaDataPath);
                if (!File.Exists(abilityMetaDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = abilityMetaDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                AbilityObjectData abilityObjectData;
                try
                {
                    using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                    abilityObjectData = reader.ReadAbilityObjectData();
                }
                catch (Exception e)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ParseError,
                        Diagnostics = new[] { e.Message },
                    };
                }

                var shouldDowngrade = abilityObjectData.GetMinimumPatch() > context.TargetPatch.Patch;
                if (shouldDowngrade && !abilityObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Unadaptable,
                    };
                }

                var diagnostics = new List<string>();

                var mergedWithSkinObjectData = false;

                if (!isSkinFileSupported)
                {
                    var expectedSkinFileName = context.FileName.Replace(AbilityObjectData.FileExtension, $"Skin{AbilityObjectData.FileExtension}");

                    if (context.Archive.TryOpenFile(expectedSkinFileName, out var skinStream))
                    {
                        AbilityObjectData? abilitySkinObjectData;
                        try
                        {
                            using var reader = new BinaryReader(skinStream, Encoding.UTF8, true);
                            abilitySkinObjectData = reader.ReadAbilityObjectData();
                        }
                        catch (Exception e)
                        {
                            diagnostics.Add(e.Message);
                            abilitySkinObjectData = null;
                        }

                        if (abilitySkinObjectData is not null &&
                            (abilitySkinObjectData.BaseAbilities.Count > 0 ||
                             abilitySkinObjectData.NewAbilities.Count > 0))
                        {
                            abilityObjectData.MergeWith(abilitySkinObjectData);

                            mergedWithSkinObjectData = true;
                        }
                    }
                }

                var knownIds = new HashSet<int>();
                knownIds.AddItemsFromSylkTable(abilityDataPath, DataConstants.AbilityDataKeyColumn);

                var knownProperties = new HashSet<int>();
                knownProperties.AddItemsFromSylkTable(abilityMetaDataPath, DataConstants.MetaDataIdColumn);

                var baseAbilities = new List<LevelObjectModification>();
                foreach (var ability in abilityObjectData.BaseAbilities)
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
                foreach (var ability in abilityObjectData.NewAbilities)
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

                if (shouldDowngrade || mergedWithSkinObjectData || diagnostics.Count > 0)
                {
                    var memoryStream = new MemoryStream();
                    using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                    writer.Write(new AbilityObjectData(abilityObjectData.FormatVersion)
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