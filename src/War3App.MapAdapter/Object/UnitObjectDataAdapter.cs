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
    public sealed class UnitObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Unit)";

        public bool IsTextFile => false;

        public bool IsJsonSerializationSupported => true;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            try
            {
                var unitAbilityDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UnitAbilityDataPath);
                if (!File.Exists(unitAbilityDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = unitAbilityDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                var unitBalanceDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UnitBalanceDataPath);
                if (!File.Exists(unitBalanceDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = unitBalanceDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                var unitDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UnitDataPath);
                if (!File.Exists(unitDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = unitDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                var unitUiDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UnitUiDataPath);
                if (!File.Exists(unitUiDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = unitUiDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                var unitWeaponDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UnitWeaponDataPath);
                if (!File.Exists(unitWeaponDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = unitWeaponDataPath.GetFileNotFoundDiagnostics(),
                    };
                }
                
                var unitMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UnitMetaDataPath);
                if (!File.Exists(unitMetaDataPath))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ConfigError,
                        Diagnostics = unitMetaDataPath.GetFileNotFoundDiagnostics(),
                    };
                }

                UnitObjectData unitObjectData;
                try
                {
                    using var reader = new BinaryReader(stream);
                    unitObjectData = reader.ReadUnitObjectData();
                }
                catch (Exception e)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.ParseError,
                        Diagnostics = new[] { e.Message },
                    };
                }

                var shouldDowngrade = unitObjectData.GetMinimumPatch() > context.TargetPatch.Patch;
                if (shouldDowngrade && !unitObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Unadaptable,
                    };
                }

                var knownIds = new HashSet<int>();
                knownIds.AddItemsFromSylkTable(unitAbilityDataPath, DataConstants.UnitAbilityDataKeyColumn);
                knownIds.AddItemsFromSylkTable(unitBalanceDataPath, DataConstants.UnitBalanceDataKeyColumn);
                knownIds.AddItemsFromSylkTable(unitDataPath, DataConstants.UnitDataKeyColumn);
                knownIds.AddItemsFromSylkTable(unitUiDataPath, DataConstants.UnitUiDataKeyColumn);
                knownIds.AddItemsFromSylkTable(unitWeaponDataPath, DataConstants.UnitWeaponDataKeyColumn);

                var knownProperties = new HashSet<int>();
                knownProperties.AddItemsFromSylkTable(unitMetaDataPath, DataConstants.MetaDataIdColumn);

                var diagnostics = new List<string>();

                var baseUnits = new List<SimpleObjectModification>();
                foreach (var unit in unitObjectData.BaseUnits)
                {
                    if (!knownIds.Contains(unit.OldId))
                    {
                        diagnostics.Add($"Unknown base unit: '{unit.OldId.ToRawcode()}'");
                        continue;
                    }

                    if (unit.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                    {
                        diagnostics.AddRange(unit.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                        unit.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                    }

                    baseUnits.Add(unit);
                }

                var newUnits = new List<SimpleObjectModification>();
                foreach (var unit in unitObjectData.NewUnits)
                {
                    if (!knownIds.Contains(unit.OldId))
                    {
                        diagnostics.Add($"Unknown base unit: '{unit.OldId.ToRawcode()}'");
                        continue;
                    }

                    if (knownIds.Contains(unit.NewId))
                    {
                        diagnostics.Add($"Conflicting unit: '{unit.NewId.ToRawcode()}'");
                        continue;
                    }

                    if (unit.Modifications.Any(property => !knownProperties.Contains(property.Id)))
                    {
                        diagnostics.AddRange(unit.Modifications.Where(property => !knownProperties.Contains(property.Id)).Select(property => $"Unknown property: '{property.Id.ToRawcode()}'"));
                        unit.Modifications.RemoveAll(property => !knownProperties.Contains(property.Id));
                    }

                    newUnits.Add(unit);
                }

                if (shouldDowngrade || diagnostics.Count > 0)
                {
                    var memoryStream = new MemoryStream();
                    using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                    writer.Write(new UnitObjectData(unitObjectData.FormatVersion)
                    {
                        BaseUnits = baseUnits,
                        NewUnits = newUnits,
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
                var unitObjectData = reader.ReadUnitObjectData();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                return JsonSerializer.Serialize(unitObjectData, options);
            }
            catch (Exception e)
            {
                return $"{e.GetType().FullName}{System.Environment.NewLine}{e.Message}";
            }
        }
    }
}