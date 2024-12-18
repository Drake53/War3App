﻿using System;
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
    public static class UpgradeObjectDataExtensions
    {
        public static bool Adapt(this UpgradeObjectData upgradeObjectData, AdaptFileContext context, out MapFileStatus status)
        {
            var isSkinFileSupported = context.TargetPatch.Patch >= GamePatch.v1_33_0;
            if (!isSkinFileSupported)
            {
                if (context.FileName is null)
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.MissingFileName, UpgradeObjectData.MapFileName, UpgradeObjectData.MapSkinFileName);
                }
                else
                {
                    var isSkinFile = context.FileName.EndsWith($"Skin{UpgradeObjectData.FileExtension}");
                    if (isSkinFile)
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.RemovedSkinData, context.FileName.Replace($"Skin{UpgradeObjectData.FileExtension}", UpgradeObjectData.FileExtension));
                        status = MapFileStatus.Removed;
                        return false;
                    }
                }
            }

            var missingDataFiles = false;

            var upgradeDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UpgradeDataPath);
            if (!File.Exists(upgradeDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.UpgradeDataPath);
                missingDataFiles = true;
            }

            var upgradeMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.UpgradeMetaDataPath);
            if (!File.Exists(upgradeMetaDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.UpgradeMetaDataPath);
                missingDataFiles = true;
            }

            if (missingDataFiles)
            {
                status = MapFileStatus.Inconclusive;
                return false;
            }

            var isAdapted = false;

            if (upgradeObjectData.GetMinimumPatch() > context.TargetPatch.Patch)
            {
                if (!upgradeObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    status = MapFileStatus.Incompatible;
                    return false;
                }

                isAdapted = true;
            }

            if (!isSkinFileSupported && context.FileName is not null)
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
                        _ = context.ReportParseError(e);
                        upgradeSkinObjectData = null;
                    }

                    if (upgradeSkinObjectData is not null &&
                        (upgradeSkinObjectData.BaseUpgrades.Count > 0 ||
                         upgradeSkinObjectData.NewUpgrades.Count > 0))
                    {
                        upgradeObjectData.MergeWith(upgradeSkinObjectData);

                        context.ReportDiagnostic(DiagnosticRule.ObjectData.MergedSkinData, expectedSkinFileName);
                        isAdapted = true;
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
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseId, "upgrade", upgrade.OldId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                for (var i = 0; i < upgrade.Modifications.Count; i++)
                {
                    var property = upgrade.Modifications[i];
                    if (!knownProperties.Contains(property.Id))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                        isAdapted = true;
                        upgrade.Modifications.RemoveAt(i--);
                    }
                }

                baseUpgrades.Add(upgrade);
            }

            var newUpgrades = new List<LevelObjectModification>();
            foreach (var upgrade in upgradeObjectData.NewUpgrades)
            {
                if (!knownIds.Contains(upgrade.OldId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseIdNew, "upgrade", upgrade.NewId.ToRawcode(), upgrade.OldId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                if (knownIds.Contains(upgrade.NewId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.ConflictingId, "upgrade", upgrade.NewId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                for (var i = 0; i < upgrade.Modifications.Count; i++)
                {
                    var property = upgrade.Modifications[i];
                    if (!knownProperties.Contains(property.Id))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                        isAdapted = true;
                        upgrade.Modifications.RemoveAt(i--);
                    }
                }

                newUpgrades.Add(upgrade);
            }

            status = MapFileStatus.Compatible;

            if (!isAdapted)
            {
                return false;
            }

            upgradeObjectData.BaseUpgrades.Clear();
            upgradeObjectData.NewUpgrades.Clear();

            upgradeObjectData.BaseUpgrades.AddRange(baseUpgrades);
            upgradeObjectData.NewUpgrades.AddRange(newUpgrades);

            return true;
        }

        public static bool TryDowngrade(this UpgradeObjectData upgradeObjectData, GamePatch targetPatch)
        {
            try
            {
                while (upgradeObjectData.GetMinimumPatch() > targetPatch)
                {
                    upgradeObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this UpgradeObjectData upgradeObjectData)
        {
            switch (upgradeObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.v3:
                    upgradeObjectData.FormatVersion = ObjectDataFormatVersion.v2;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this UpgradeObjectData upgradeObjectData)
        {
            return upgradeObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.v1 => GamePatch.v1_00,
                ObjectDataFormatVersion.v2 => GamePatch.v1_00,
                ObjectDataFormatVersion.v3 => GamePatch.v1_33_0,
            };
        }

        public static void MergeWith(this UpgradeObjectData target, UpgradeObjectData source)
        {
            foreach (var sourceBaseUpgrade in source.BaseUpgrades)
            {
                var targetBaseUpgrade = target.BaseUpgrades.FirstOrDefault(upgrade => upgrade.OldId == sourceBaseUpgrade.OldId);
                if (targetBaseUpgrade is null)
                {
                    target.BaseUpgrades.Add(sourceBaseUpgrade);
                    continue;
                }

                foreach (var sourceUpgradeModification in sourceBaseUpgrade.Modifications)
                {
                    var targetUpgradeModification = targetBaseUpgrade.Modifications.FirstOrDefault(mod
                        => mod.Id == sourceUpgradeModification.Id
                        && mod.Level == sourceUpgradeModification.Level
                        && mod.Pointer == sourceUpgradeModification.Pointer);

                    if (targetUpgradeModification is null)
                    {
                        targetBaseUpgrade.Modifications.Add(sourceUpgradeModification);
                        continue;
                    }

                    targetUpgradeModification.Type = sourceUpgradeModification.Type;
                    targetUpgradeModification.Value = sourceUpgradeModification.Value;
                }
            }

            foreach (var sourceNewUpgrade in source.NewUpgrades)
            {
                var targetNewUpgrade = target.NewUpgrades.FirstOrDefault(upgrade => upgrade.NewId == sourceNewUpgrade.NewId);
                if (targetNewUpgrade is null)
                {
                    target.NewUpgrades.Add(sourceNewUpgrade);
                    continue;
                }

                foreach (var sourceUpgradeModification in sourceNewUpgrade.Modifications)
                {
                    var targetUpgradeModification = targetNewUpgrade.Modifications.FirstOrDefault(mod
                        => mod.Id == sourceUpgradeModification.Id
                        && mod.Level == sourceUpgradeModification.Level
                        && mod.Pointer == sourceUpgradeModification.Pointer);

                    if (targetUpgradeModification is null)
                    {
                        targetNewUpgrade.Modifications.Add(sourceUpgradeModification);
                        continue;
                    }

                    targetUpgradeModification.Type = sourceUpgradeModification.Type;
                    targetUpgradeModification.Value = sourceUpgradeModification.Value;
                }
            }
        }
    }
}