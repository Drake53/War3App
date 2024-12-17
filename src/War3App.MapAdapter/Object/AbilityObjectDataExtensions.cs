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
    public static class AbilityObjectDataExtensions
    {
        public static MapFileStatus Adapt(this AbilityObjectData abilityObjectData, AdaptFileContext context)
        {
            var isSkinFileSupported = context.TargetPatch.Patch >= GamePatch.v1_33_0;
            if (!isSkinFileSupported)
            {
                if (context.FileName is null)
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.MissingFileName, AbilityObjectData.MapFileName, AbilityObjectData.MapSkinFileName);
                }
                else
                {
                    var isSkinFile = context.FileName.EndsWith($"Skin{AbilityObjectData.FileExtension}");
                    if (isSkinFile)
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.RemovedSkinData, context.FileName.Replace($"Skin{AbilityObjectData.FileExtension}", AbilityObjectData.FileExtension));
                        return MapFileStatus.Removed;
                    }
                }
            }

            var missingDataFiles = false;

            var abilityDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.AbilityDataPath);
            if (!File.Exists(abilityDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.AbilityDataPath);
                missingDataFiles = true;
            }

            var abilityMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.AbilityMetaDataPath);
            if (!File.Exists(abilityMetaDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.AbilityMetaDataPath);
                missingDataFiles = true;
            }

            if (missingDataFiles)
            {
                return MapFileStatus.ConfigError;
            }

            var isAdapted = false;

            if (abilityObjectData.GetMinimumPatch() > context.TargetPatch.Patch)
            {
                if (!abilityObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    return MapFileStatus.Incompatible;
                }

                isAdapted = true;
            }

            if (!isSkinFileSupported && context.FileName is not null)
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
                        _ = context.ReportParseError(e);
                        abilitySkinObjectData = null;
                    }

                    if (abilitySkinObjectData is not null &&
                        (abilitySkinObjectData.BaseAbilities.Count > 0 ||
                         abilitySkinObjectData.NewAbilities.Count > 0))
                    {
                        abilityObjectData.MergeWith(abilitySkinObjectData);

                        context.ReportDiagnostic(DiagnosticRule.ObjectData.MergedSkinData, expectedSkinFileName);
                        isAdapted = true;
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
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseId, "ability", ability.OldId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                for (var i = 0; i < ability.Modifications.Count; i++)
                {
                    var property = ability.Modifications[i];
                    if (!knownProperties.Contains(property.Id))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                        isAdapted = true;
                        ability.Modifications.RemoveAt(i--);
                    }
                }

                baseAbilities.Add(ability);
            }

            var newAbilities = new List<LevelObjectModification>();
            foreach (var ability in abilityObjectData.NewAbilities)
            {
                if (!knownIds.Contains(ability.OldId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseIdNew, "ability", ability.NewId.ToRawcode(), ability.OldId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                if (knownIds.Contains(ability.NewId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.ConflictingId, "ability", ability.NewId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                for (var i = 0; i < ability.Modifications.Count; i++)
                {
                    var property = ability.Modifications[i];
                    if (!knownProperties.Contains(property.Id))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                        isAdapted = true;
                        ability.Modifications.RemoveAt(i--);
                    }
                }

                newAbilities.Add(ability);
            }

            if (!isAdapted)
            {
                return MapFileStatus.Compatible;
            }

            abilityObjectData.BaseAbilities.Clear();
            abilityObjectData.NewAbilities.Clear();

            abilityObjectData.BaseAbilities.AddRange(baseAbilities);
            abilityObjectData.NewAbilities.AddRange(newAbilities);

            return MapFileStatus.Adapted;
        }

        public static bool TryDowngrade(this AbilityObjectData abilityObjectData, GamePatch targetPatch)
        {
            try
            {
                while (abilityObjectData.GetMinimumPatch() > targetPatch)
                {
                    abilityObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this AbilityObjectData abilityObjectData)
        {
            switch (abilityObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.v3:
                    abilityObjectData.FormatVersion = ObjectDataFormatVersion.v2;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this AbilityObjectData abilityObjectData)
        {
            return abilityObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.v1 => GamePatch.v1_00,
                ObjectDataFormatVersion.v2 => GamePatch.v1_00,
                ObjectDataFormatVersion.v3 => GamePatch.v1_33_0,
            };
        }

        public static void MergeWith(this AbilityObjectData target, AbilityObjectData source)
        {
            foreach (var sourceBaseAbility in source.BaseAbilities)
            {
                var targetBaseAbility = target.BaseAbilities.FirstOrDefault(ability => ability.OldId == sourceBaseAbility.OldId);
                if (targetBaseAbility is null)
                {
                    target.BaseAbilities.Add(sourceBaseAbility);
                    continue;
                }

                foreach (var sourceAbilityModification in sourceBaseAbility.Modifications)
                {
                    var targetAbilityModification = targetBaseAbility.Modifications.FirstOrDefault(mod
                        => mod.Id == sourceAbilityModification.Id
                        && mod.Level == sourceAbilityModification.Level
                        && mod.Pointer == sourceAbilityModification.Pointer);

                    if (targetAbilityModification is null)
                    {
                        targetBaseAbility.Modifications.Add(sourceAbilityModification);
                        continue;
                    }

                    targetAbilityModification.Type = sourceAbilityModification.Type;
                    targetAbilityModification.Value = sourceAbilityModification.Value;
                }
            }

            foreach (var sourceNewAbility in source.NewAbilities)
            {
                var targetNewAbility = target.NewAbilities.FirstOrDefault(ability => ability.NewId == sourceNewAbility.NewId);
                if (targetNewAbility is null)
                {
                    target.NewAbilities.Add(sourceNewAbility);
                    continue;
                }

                foreach (var sourceAbilityModification in sourceNewAbility.Modifications)
                {
                    var targetAbilityModification = targetNewAbility.Modifications.FirstOrDefault(mod
                        => mod.Id == sourceAbilityModification.Id
                        && mod.Level == sourceAbilityModification.Level
                        && mod.Pointer == sourceAbilityModification.Pointer);

                    if (targetAbilityModification is null)
                    {
                        targetNewAbility.Modifications.Add(sourceAbilityModification);
                        continue;
                    }

                    targetAbilityModification.Type = sourceAbilityModification.Type;
                    targetAbilityModification.Value = sourceAbilityModification.Value;
                }
            }
        }
    }
}