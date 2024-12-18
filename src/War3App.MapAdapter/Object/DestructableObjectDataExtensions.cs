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
    public static class DestructableObjectDataExtensions
    {
        public static bool Adapt(this DestructableObjectData destructableObjectData, AdaptFileContext context, out MapFileStatus status)
        {
            var isSkinFileSupported = context.TargetPatch.Patch >= GamePatch.v1_33_0;
            if (!isSkinFileSupported)
            {
                if (context.FileName is null)
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.MissingFileName, DestructableObjectData.MapFileName, DestructableObjectData.MapSkinFileName);
                }
                else
                {
                    var isSkinFile = context.FileName.EndsWith($"Skin{DestructableObjectData.FileExtension}");
                    if (isSkinFile)
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.RemovedSkinData, context.FileName.Replace($"Skin{DestructableObjectData.FileExtension}", DestructableObjectData.FileExtension));
                        status = MapFileStatus.Removed;
                        return false;
                    }
                }
            }

            var missingDataFiles = false;

            var destructableDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.DestructableDataPath);
            if (!File.Exists(destructableDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.DestructableDataPath);
                missingDataFiles = true;
            }

            var destructableMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.DestructableMetaDataPath);
            if (!File.Exists(destructableMetaDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.DestructableMetaDataPath);
                missingDataFiles = true;
            }

            if (missingDataFiles)
            {
                status = MapFileStatus.Inconclusive;
                return false;
            }

            var isAdapted = false;

            if (destructableObjectData.GetMinimumPatch() > context.TargetPatch.Patch)
            {
                if (!destructableObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    status = MapFileStatus.Incompatible;
                    return false;
                }

                isAdapted = true;
            }

            if (!isSkinFileSupported && context.FileName is not null)
            {
                var expectedSkinFileName = context.FileName.Replace(DestructableObjectData.FileExtension, $"Skin{DestructableObjectData.FileExtension}");

                if (context.Archive.TryOpenFile(expectedSkinFileName, out var skinStream))
                {
                    DestructableObjectData? destructableSkinObjectData;
                    try
                    {
                        using var reader = new BinaryReader(skinStream, Encoding.UTF8, true);
                        destructableSkinObjectData = reader.ReadDestructableObjectData();
                    }
                    catch (Exception e)
                    {
                        _ = context.ReportParseError(e);
                        destructableSkinObjectData = null;
                    }

                    if (destructableSkinObjectData is not null &&
                        (destructableSkinObjectData.BaseDestructables.Count > 0 ||
                         destructableSkinObjectData.NewDestructables.Count > 0))
                    {
                        destructableObjectData.MergeWith(destructableSkinObjectData);

                        context.ReportDiagnostic(DiagnosticRule.ObjectData.MergedSkinData, expectedSkinFileName);
                        isAdapted = true;
                    }
                }
            }

            var knownIds = new HashSet<int>();
            knownIds.AddItemsFromSylkTable(destructableDataPath, DataConstants.DestructableDataKeyColumn);

            var knownProperties = new HashSet<int>();
            knownProperties.AddItemsFromSylkTable(destructableMetaDataPath, DataConstants.MetaDataIdColumn);

            var baseDestructables = new List<SimpleObjectModification>();
            foreach (var destructable in destructableObjectData.BaseDestructables)
            {
                if (!knownIds.Contains(destructable.OldId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseId, "destructable", destructable.OldId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                for (var i = 0; i < destructable.Modifications.Count; i++)
                {
                    var property = destructable.Modifications[i];
                    if (!knownProperties.Contains(property.Id))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                        isAdapted = true;
                        destructable.Modifications.RemoveAt(i--);
                    }
                }

                baseDestructables.Add(destructable);
            }

            var newDestructables = new List<SimpleObjectModification>();
            foreach (var destructable in destructableObjectData.NewDestructables)
            {
                if (!knownIds.Contains(destructable.OldId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseIdNew, "destructable", destructable.NewId.ToRawcode(), destructable.OldId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                if (knownIds.Contains(destructable.NewId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.ConflictingId, "destructable", destructable.NewId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                for (var i = 0; i < destructable.Modifications.Count; i++)
                {
                    var property = destructable.Modifications[i];
                    if (!knownProperties.Contains(property.Id))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                        isAdapted = true;
                        destructable.Modifications.RemoveAt(i--);
                    }
                }

                newDestructables.Add(destructable);
            }

            status = MapFileStatus.Compatible;

            if (!isAdapted)
            {
                return false;
            }

            destructableObjectData.BaseDestructables.Clear();
            destructableObjectData.NewDestructables.Clear();

            destructableObjectData.BaseDestructables.AddRange(baseDestructables);
            destructableObjectData.NewDestructables.AddRange(newDestructables);

            return true;
        }

        public static bool TryDowngrade(this DestructableObjectData destructableObjectData, GamePatch targetPatch)
        {
            try
            {
                while (destructableObjectData.GetMinimumPatch() > targetPatch)
                {
                    destructableObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this DestructableObjectData destructableObjectData)
        {
            switch (destructableObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.v3:
                    destructableObjectData.FormatVersion = ObjectDataFormatVersion.v2;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this DestructableObjectData destructableObjectData)
        {
            return destructableObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.v1 => GamePatch.v1_00,
                ObjectDataFormatVersion.v2 => GamePatch.v1_00,
                ObjectDataFormatVersion.v3 => GamePatch.v1_33_0,
            };
        }

        public static void MergeWith(this DestructableObjectData target, DestructableObjectData source)
        {
            foreach (var sourceBaseDestructable in source.BaseDestructables)
            {
                var targetBaseDestructable = target.BaseDestructables.FirstOrDefault(destructable => destructable.OldId == sourceBaseDestructable.OldId);
                if (targetBaseDestructable is null)
                {
                    target.BaseDestructables.Add(sourceBaseDestructable);
                    continue;
                }

                foreach (var sourceDestructableModification in sourceBaseDestructable.Modifications)
                {
                    var targetDestructableModification = targetBaseDestructable.Modifications.FirstOrDefault(mod => mod.Id == sourceDestructableModification.Id);
                    if (targetDestructableModification is null)
                    {
                        targetBaseDestructable.Modifications.Add(sourceDestructableModification);
                        continue;
                    }

                    targetDestructableModification.Type = sourceDestructableModification.Type;
                    targetDestructableModification.Value = sourceDestructableModification.Value;
                }
            }

            foreach (var sourceNewDestructable in source.NewDestructables)
            {
                var targetNewDestructable = target.NewDestructables.FirstOrDefault(destructable => destructable.NewId == sourceNewDestructable.NewId);
                if (targetNewDestructable is null)
                {
                    target.NewDestructables.Add(sourceNewDestructable);
                    continue;
                }

                foreach (var sourceDestructableModification in sourceNewDestructable.Modifications)
                {
                    var targetDestructableModification = targetNewDestructable.Modifications.FirstOrDefault(mod => mod.Id == sourceDestructableModification.Id);
                    if (targetDestructableModification is null)
                    {
                        targetNewDestructable.Modifications.Add(sourceDestructableModification);
                        continue;
                    }

                    targetDestructableModification.Type = sourceDestructableModification.Type;
                    targetDestructableModification.Value = sourceDestructableModification.Value;
                }
            }
        }
    }
}