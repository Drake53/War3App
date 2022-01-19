﻿using System;
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
    public sealed class UnitObjectDataAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Object Data (Unit)";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var mapUnitObjectData = reader.ReadMapUnitObjectData();

                try
                {
                    var knownIds = new HashSet<int>();
                    knownIds.AddItemsFromSylkTable(Path.Combine(targetPatch.GameDataPath, PathConstants.UnitAbilityDataPath), DataConstants.UnitAbilityDataKeyColumn);
                    knownIds.AddItemsFromSylkTable(Path.Combine(targetPatch.GameDataPath, PathConstants.UnitBalanceDataPath), DataConstants.UnitBalanceDataKeyColumn);
                    knownIds.AddItemsFromSylkTable(Path.Combine(targetPatch.GameDataPath, PathConstants.UnitDataPath), DataConstants.UnitDataKeyColumn);
                    knownIds.AddItemsFromSylkTable(Path.Combine(targetPatch.GameDataPath, PathConstants.UnitUiDataPath), DataConstants.UnitUiDataKeyColumn);
                    knownIds.AddItemsFromSylkTable(Path.Combine(targetPatch.GameDataPath, PathConstants.UnitWeaponDataPath), DataConstants.UnitWeaponDataKeyColumn);

                    var knownProperties = new HashSet<int>();
                    knownProperties.AddItemsFromSylkTable(Path.Combine(targetPatch.GameDataPath, PathConstants.UnitMetaDataPath), DataConstants.MetaDataIdColumn);

                    var diagnostics = new List<string>();

                    var baseUnits = new List<SimpleObjectModification>();
                    foreach (var unit in mapUnitObjectData.BaseUnits)
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
                    foreach (var unit in mapUnitObjectData.NewUnits)
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

                    if (diagnostics.Count > 0)
                    {
                        var memoryStream = new MemoryStream();
                        using var writer = new BinaryWriter(memoryStream, new UTF8Encoding(false, true), true);
                        writer.Write(new MapUnitObjectData(mapUnitObjectData.FormatVersion)
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
                catch
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.AdapterError,
                    };
                }
            }
            catch (NotSupportedException)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.Unadaptable,
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
        }
    }
}