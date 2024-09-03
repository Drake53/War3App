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
    public sealed class UpgradeObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Upgrade)";

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

                var isSkinFile = context.FileName.EndsWith($"Skin{UpgradeObjectData.FileExtension}");
                var isSkinFileSupported = context.TargetPatch.Patch >= GamePatch.v1_33_0;

                if (isSkinFile && !isSkinFileSupported)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Removed,
                    };
                }

                var upgradeDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UpgradeDataPath);
                if (!File.Exists(upgradeDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = upgradeDataPath.GetFileNotFoundDiagnostics(),
                    };
                }
                
                var upgradeMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UpgradeMetaDataPath);
                if (!File.Exists(upgradeMetaDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = upgradeMetaDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                UpgradeObjectData upgradeObjectData;
                try
                {
                    using var reader = new BinaryReader(stream, Encoding.UTF8, true);
                    upgradeObjectData = reader.ReadUpgradeObjectData();
                }
                catch (Exception e)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ParseError,
                        Diagnostics = new[] { e.Message },
                    };
                }

                var shouldDowngrade = upgradeObjectData.GetMinimumPatch() > context.TargetPatch.Patch;
                if (shouldDowngrade && !upgradeObjectData.TryDowngrade(context.TargetPatch.Patch))
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
                    var expectedSkinFileName = context.FileName.Replace(UpgradeObjectData.FileExtension, $"Skin{UpgradeObjectData.FileExtension}");

                    if (context.Archive.TryOpenFile(expectedSkinFileName, out var skinStream))
                    {
                        UpgradeObjectData? upgradeSkinObjectData;
                        try
                        {
                            using var reader = new BinaryReader(skinStream, Encoding.UTF8, true);
                            upgradeSkinObjectData = reader.ReadUpgradeObjectData();
                        }
                        catch (Exception e)
                        {
                            diagnostics.Add(e.Message);
                            upgradeSkinObjectData = null;
                        }

                        if (upgradeSkinObjectData is not null &&
                            (upgradeSkinObjectData.BaseUpgrades.Count > 0 ||
                             upgradeSkinObjectData.NewUpgrades.Count > 0))
                        {
                            upgradeObjectData.MergeWith(upgradeSkinObjectData);

                            mergedWithSkinObjectData = true;
                        }
                    }
                }

                var knownIds = new HashSet<int>();
                knownIds.AddItemsFromSylkTable(upgradeDataPath, DataConstants.UpgradeDataKeyColumn);

                var knownProperties = new HashSet<int>();
                knownProperties.AddItemsFromSylkTable(upgradeMetaDataPath, DataConstants.MetaDataIdColumn);

                var baseUpgrades = new List<LevelObjectModification>();
                foreach (var upgrade in upgradeObjectData.BaseUpgrades)
                {
                    if (!knownIds.Contains(upgrade.OldId))
                    {
                        diagnostics.Add($"Unknown base upgrade: '{upgrade.OldId.ToRawcode()}'");
                        continue;
                    }

                    if (upgrade.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                    {
                        diagnostics.AddRange(upgrade.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                        upgrade.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                    }

                    baseUpgrades.Add(upgrade);
                }

                var newUpgrades = new List<LevelObjectModification>();
                foreach (var upgrade in upgradeObjectData.NewUpgrades)
                {
                    if (!knownIds.Contains(upgrade.OldId))
                    {
                        diagnostics.Add($"Unknown base upgrade: '{upgrade.OldId.ToRawcode()}'");
                        continue;
                    }

                    if (knownIds.Contains(upgrade.NewId))
                    {
                        diagnostics.Add($"Conflicting upgrade: '{upgrade.NewId.ToRawcode()}'");
                        continue;
                    }

                    if (upgrade.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                    {
                        diagnostics.AddRange(upgrade.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                        upgrade.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                    }

                    newUpgrades.Add(upgrade);
                }

                if (shouldDowngrade || mergedWithSkinObjectData || diagnostics.Count > 0)
                {
                    var memoryStream = new MemoryStream();
                    using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                    writer.Write(new UpgradeObjectData(upgradeObjectData.FormatVersion)
                    {
                        BaseUpgrades = baseUpgrades,
                        NewUpgrades = newUpgrades,
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
                var upgradeObjectData = reader.ReadUpgradeObjectData();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                return JsonSerializer.Serialize(upgradeObjectData, options);
            }
            catch (Exception e)
            {
                return $"{e.GetType().FullName}{System.Environment.NewLine}{e.Message}";
            }
        }
    }
}