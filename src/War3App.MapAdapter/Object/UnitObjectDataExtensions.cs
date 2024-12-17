using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Object;
using War3Net.Common.Extensions;

namespace War3App.MapAdapter.Object
{
    public static class UnitObjectDataExtensions
    {
        public static MapFileStatus Adapt(this UnitObjectData unitObjectData, AdaptFileContext context)
        {
            var isSkinFileSupported = context.TargetPatch.Patch >= GamePatch.v1_33_0;
            if (!isSkinFileSupported)
            {
                if (context.FileName is null)
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.MissingFileName, UnitObjectData.MapFileName, UnitObjectData.MapSkinFileName);
                }
                else
                {
                    var isSkinFile = context.FileName.EndsWith($"Skin{UnitObjectData.FileExtension}");
                    if (isSkinFile)
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.RemovedSkinData, context.FileName.Replace($"Skin{UnitObjectData.FileExtension}", UnitObjectData.FileExtension));
                        return MapFileStatus.Removed;
                    }
                }
            }

            var missingDataFiles = false;

            var unitAbilityDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UnitAbilityDataPath);
            if (!File.Exists(unitAbilityDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.UnitAbilityDataPath);
                missingDataFiles = true;
            }

            var unitBalanceDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UnitBalanceDataPath);
            if (!File.Exists(unitBalanceDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.UnitBalanceDataPath);
                missingDataFiles = true;
            }

            var unitDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UnitDataPath);
            if (!File.Exists(unitDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.UnitDataPath);
                missingDataFiles = true;
            }

            var unitUiDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UnitUiDataPath);
            if (!File.Exists(unitUiDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.UnitUiDataPath);
                missingDataFiles = true;
            }

            var unitWeaponDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UnitWeaponDataPath);
            if (!File.Exists(unitWeaponDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.UnitWeaponDataPath);
                missingDataFiles = true;
            }

            var unitMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UnitMetaDataPath);
            if (!File.Exists(unitMetaDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.UnitMetaDataPath);
                missingDataFiles = true;
            }

            if (missingDataFiles)
            {
                return MapFileStatus.ConfigError;
            }

            var isAdapted = false;

            if (unitObjectData.GetMinimumPatch() > context.TargetPatch.Patch)
            {
                if (!unitObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    return MapFileStatus.Incompatible;
                }

                isAdapted = true;
            }

            if (!isSkinFileSupported && context.FileName is not null)
            {
                var expectedSkinFileName = context.FileName.Replace(UnitObjectData.FileExtension, $"Skin{UnitObjectData.FileExtension}");

                if (context.Archive.TryOpenFile(expectedSkinFileName, out var skinStream))
                {
                    UnitObjectData? unitSkinObjectData;
                    try
                    {
                        using var reader = new BinaryReader(skinStream, Encoding.UTF8, true);
                        unitSkinObjectData = reader.ReadUnitObjectData();
                    }
                    catch (Exception e)
                    {
                        _ = context.ReportParseError(e);
                        unitSkinObjectData = null;
                    }

                    if (unitSkinObjectData is not null &&
                        (unitSkinObjectData.BaseUnits.Count > 0 ||
                         unitSkinObjectData.NewUnits.Count > 0))
                    {
                        unitObjectData.MergeWith(unitSkinObjectData);

                        context.ReportDiagnostic(DiagnosticRule.ObjectData.MergedSkinData, expectedSkinFileName);
                        isAdapted = true;
                    }
                }
            }

            var knownIds = new HashSet<int>();
            knownIds.AddItemsFromSylkTable(unitAbilityDataPath, DataConstants.UnitAbilityDataKeyColumn);
            knownIds.AddItemsFromSylkTable(unitBalanceDataPath, DataConstants.UnitBalanceDataKeyColumn);
            knownIds.AddItemsFromSylkTable(unitDataPath, DataConstants.UnitDataKeyColumn);
            knownIds.AddItemsFromSylkTable(unitUiDataPath, DataConstants.UnitUiDataKeyColumn);
            knownIds.AddItemsFromSylkTable(unitWeaponDataPath, DataConstants.UnitWeaponDataKeyColumn, DataConstants.UnitWeaponDataKeyColumnOld);

            var knownProperties = new HashSet<int>();
            knownProperties.AddItemsFromSylkTable(unitMetaDataPath, DataConstants.MetaDataIdColumn);

            var baseUnits = new List<SimpleObjectModification>();
            foreach (var unit in unitObjectData.BaseUnits)
            {
                if (!knownIds.Contains(unit.OldId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseId, "unit", unit.OldId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                for (var i = 0; i < unit.Modifications.Count; i++)
                {
                    var property = unit.Modifications[i];
                    if (!knownProperties.Contains(property.Id))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                        isAdapted = true;
                        unit.Modifications.RemoveAt(i--);
                    }
                }

                baseUnits.Add(unit);
            }

            var newUnits = new List<SimpleObjectModification>();
            foreach (var unit in unitObjectData.NewUnits)
            {
                if (!knownIds.Contains(unit.OldId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseIdNew, "unit", unit.NewId.ToRawcode(), unit.OldId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                if (knownIds.Contains(unit.NewId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.ConflictingId, "unit", unit.NewId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                for (var i = 0; i < unit.Modifications.Count; i++)
                {
                    var property = unit.Modifications[i];
                    if (!knownProperties.Contains(property.Id))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                        isAdapted = true;
                        unit.Modifications.RemoveAt(i--);
                    }
                }

                newUnits.Add(unit);
            }

            if (!isAdapted)
            {
                return MapFileStatus.Compatible;
            }

            unitObjectData.BaseUnits.Clear();
            unitObjectData.NewUnits.Clear();

            unitObjectData.BaseUnits.AddRange(baseUnits);
            unitObjectData.NewUnits.AddRange(newUnits);

            return MapFileStatus.Adapted;
        }

        public static bool TryDowngrade(this UnitObjectData unitObjectData, GamePatch targetPatch)
        {
            try
            {
                while (unitObjectData.GetMinimumPatch() > targetPatch)
                {
                    unitObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this UnitObjectData unitObjectData)
        {
            switch (unitObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.v3:
                    unitObjectData.FormatVersion = ObjectDataFormatVersion.v2;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this UnitObjectData unitObjectData)
        {
            return unitObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.v1 => GamePatch.v1_00,
                ObjectDataFormatVersion.v2 => GamePatch.v1_00,
                ObjectDataFormatVersion.v3 => GamePatch.v1_33_0,
            };
        }

        public static void MergeWith(this UnitObjectData target, UnitObjectData source)
        {
            foreach (var sourceBaseUnit in source.BaseUnits)
            {
                var targetBaseUnit = target.BaseUnits.FirstOrDefault(unit => unit.OldId == sourceBaseUnit.OldId);
                if (targetBaseUnit is null)
                {
                    target.BaseUnits.Add(sourceBaseUnit);
                    continue;
                }

                foreach (var sourceUnitModification in sourceBaseUnit.Modifications)
                {
                    var targetUnitModification = targetBaseUnit.Modifications.FirstOrDefault(mod => mod.Id == sourceUnitModification.Id);
                    if (targetUnitModification is null)
                    {
                        targetBaseUnit.Modifications.Add(sourceUnitModification);
                        continue;
                    }

                    targetUnitModification.Type = sourceUnitModification.Type;
                    targetUnitModification.Value = sourceUnitModification.Value;
                }
            }

            foreach (var sourceNewUnit in source.NewUnits)
            {
                var targetNewUnit = target.NewUnits.FirstOrDefault(unit => unit.NewId == sourceNewUnit.NewId);
                if (targetNewUnit is null)
                {
                    target.NewUnits.Add(sourceNewUnit);
                    continue;
                }

                foreach (var sourceUnitModification in sourceNewUnit.Modifications)
                {
                    var targetUnitModification = targetNewUnit.Modifications.FirstOrDefault(mod => mod.Id == sourceUnitModification.Id);
                    if (targetUnitModification is null)
                    {
                        targetNewUnit.Modifications.Add(sourceUnitModification);
                        continue;
                    }

                    targetUnitModification.Type = sourceUnitModification.Type;
                    targetUnitModification.Value = sourceUnitModification.Value;
                }
            }
        }
    }
}