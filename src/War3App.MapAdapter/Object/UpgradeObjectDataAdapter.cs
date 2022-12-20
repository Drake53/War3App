using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var upgradeDataPath = Path.Combine(targetPatch.GameDataPath, PathConstants.UpgradeDataPath);
                if (!File.Exists(upgradeDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = upgradeDataPath.GetFileNotFoundDiagnostics(),
                    };
                }
                
                var upgradeMetaDataPath = Path.Combine(targetPatch.GameDataPath, PathConstants.UpgradeMetaDataPath);
                if (!File.Exists(upgradeMetaDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = upgradeMetaDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                MapUpgradeObjectData mapUpgradeObjectData;
                try
                {
                    using var reader = new BinaryReader(stream);
                    mapUpgradeObjectData = reader.ReadMapUpgradeObjectData();
                }
                catch (Exception e)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ParseError,
                        Diagnostics = new[] { e.Message },
                    };
                }

                var shouldDowngrade = mapUpgradeObjectData.GetMinimumPatch() > targetPatch.Patch;
                if (shouldDowngrade && !mapUpgradeObjectData.TryDowngrade(targetPatch.Patch))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Unadaptable,
                    };
                }

                var knownIds = new HashSet<int>();
                knownIds.AddItemsFromSylkTable(upgradeDataPath, DataConstants.UpgradeDataKeyColumn);

                var knownProperties = new HashSet<int>();
                knownProperties.AddItemsFromSylkTable(upgradeMetaDataPath, DataConstants.MetaDataIdColumn);

                var diagnostics = new List<string>();

                var baseUpgrades = new List<LevelObjectModification>();
                foreach (var upgrade in mapUpgradeObjectData.BaseUpgrades)
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
                foreach (var upgrade in mapUpgradeObjectData.NewUpgrades)
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

                if (shouldDowngrade || diagnostics.Count > 0)
                {
                    var memoryStream = new MemoryStream();
                    using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                    writer.Write(new MapUpgradeObjectData(mapUpgradeObjectData.FormatVersion)
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
                using var reader = new BinaryReader(stream);
                var mapUpgradeObjectData = reader.ReadMapUpgradeObjectData();

                return System.Text.Json.JsonSerializer.Serialize(mapUpgradeObjectData);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}