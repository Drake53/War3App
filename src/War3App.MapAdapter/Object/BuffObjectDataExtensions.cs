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
    public static class BuffObjectDataExtensions
    {
        public static bool Adapt(this BuffObjectData buffObjectData, AdaptFileContext context, out MapFileStatus status)
        {
            var isSkinFileSupported = context.TargetPatch.Patch >= GamePatch.v1_33_0;
            if (!isSkinFileSupported)
            {
                if (context.FileName is null)
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.MissingFileName, BuffObjectData.MapFileName, BuffObjectData.MapSkinFileName);
                }
                else
                {
                    var isSkinFile = context.FileName.EndsWith($"Skin{BuffObjectData.FileExtension}");
                    if (isSkinFile)
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.RemovedSkinData, context.FileName.Replace($"Skin{BuffObjectData.FileExtension}", BuffObjectData.FileExtension));
                        status = MapFileStatus.Removed;
                        return false;
                    }
                }
            }

            var missingDataFiles = false;

            var buffDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.BuffDataPath);
            if (!File.Exists(buffDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.BuffDataPath);
                missingDataFiles = true;
            }

            var buffMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.BuffMetaDataPath);
            if (!File.Exists(buffMetaDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.BuffMetaDataPath);
                missingDataFiles = true;
            }

            if (missingDataFiles)
            {
                status = MapFileStatus.Inconclusive;
                return false;
            }

            var isAdapted = false;

            if (buffObjectData.GetMinimumPatch() > context.TargetPatch.Patch)
            {
                if (!buffObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    status = MapFileStatus.Incompatible;
                    return false;
                }

                isAdapted = true;
            }

            if (!isSkinFileSupported && context.FileName is not null)
            {
                var expectedSkinFileName = context.FileName.Replace(BuffObjectData.FileExtension, $"Skin{BuffObjectData.FileExtension}");

                if (context.Archive.TryOpenFile(expectedSkinFileName, out var skinStream))
                {
                    BuffObjectData? buffSkinObjectData;
                    try
                    {
                        using var reader = new BinaryReader(skinStream, Encoding.UTF8, true);
                        buffSkinObjectData = reader.ReadBuffObjectData();
                    }
                    catch (Exception e)
                    {
                        _ = context.ReportParseError(e);
                        buffSkinObjectData = null;
                    }

                    if (buffSkinObjectData is not null &&
                        (buffSkinObjectData.BaseBuffs.Count > 0 ||
                         buffSkinObjectData.NewBuffs.Count > 0))
                    {
                        buffObjectData.MergeWith(buffSkinObjectData);

                        context.ReportDiagnostic(DiagnosticRule.ObjectData.MergedSkinData, expectedSkinFileName);
                        isAdapted = true;
                    }
                }
            }

            var knownIds = new HashSet<int>();
            knownIds.AddItemsFromSylkTable(buffDataPath, DataConstants.BuffDataKeyColumn);

            var knownProperties = new HashSet<int>();
            knownProperties.AddItemsFromSylkTable(buffMetaDataPath, DataConstants.MetaDataIdColumn);

            var baseBuffs = new List<SimpleObjectModification>();
            foreach (var buff in buffObjectData.BaseBuffs)
            {
                if (!knownIds.Contains(buff.OldId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseId, "buff", buff.OldId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                for (var i = 0; i < buff.Modifications.Count; i++)
                {
                    var property = buff.Modifications[i];
                    if (!knownProperties.Contains(property.Id))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                        isAdapted = true;
                        buff.Modifications.RemoveAt(i--);
                    }
                }

                baseBuffs.Add(buff);
            }

            var newBuffs = new List<SimpleObjectModification>();
            foreach (var buff in buffObjectData.NewBuffs)
            {
                if (!knownIds.Contains(buff.OldId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseIdNew, "buff", buff.NewId.ToRawcode(), buff.OldId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                if (knownIds.Contains(buff.NewId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.ConflictingId, "buff", buff.NewId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                for (var i = 0; i < buff.Modifications.Count; i++)
                {
                    var property = buff.Modifications[i];
                    if (!knownProperties.Contains(property.Id))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                        isAdapted = true;
                        buff.Modifications.RemoveAt(i--);
                    }
                }

                newBuffs.Add(buff);
            }

            status = MapFileStatus.Compatible;

            if (!isAdapted)
            {
                return false;
            }

            buffObjectData.BaseBuffs.Clear();
            buffObjectData.NewBuffs.Clear();

            buffObjectData.BaseBuffs.AddRange(baseBuffs);
            buffObjectData.NewBuffs.AddRange(newBuffs);

            return true;
        }

        public static bool TryDowngrade(this BuffObjectData buffObjectData, GamePatch targetPatch)
        {
            try
            {
                while (buffObjectData.GetMinimumPatch() > targetPatch)
                {
                    buffObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this BuffObjectData buffObjectData)
        {
            switch (buffObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.v3:
                    buffObjectData.FormatVersion = ObjectDataFormatVersion.v2;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this BuffObjectData buffObjectData)
        {
            return buffObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.v1 => GamePatch.v1_00,
                ObjectDataFormatVersion.v2 => GamePatch.v1_00,
                ObjectDataFormatVersion.v3 => GamePatch.v1_33_0,
            };
        }

        public static void MergeWith(this BuffObjectData target, BuffObjectData source)
        {
            foreach (var sourceBaseBuff in source.BaseBuffs)
            {
                var targetBaseBuff = target.BaseBuffs.FirstOrDefault(buff => buff.OldId == sourceBaseBuff.OldId);
                if (targetBaseBuff is null)
                {
                    target.BaseBuffs.Add(sourceBaseBuff);
                    continue;
                }

                foreach (var sourceBuffModification in sourceBaseBuff.Modifications)
                {
                    var targetBuffModification = targetBaseBuff.Modifications.FirstOrDefault(mod => mod.Id == sourceBuffModification.Id);
                    if (targetBuffModification is null)
                    {
                        targetBaseBuff.Modifications.Add(sourceBuffModification);
                        continue;
                    }

                    targetBuffModification.Type = sourceBuffModification.Type;
                    targetBuffModification.Value = sourceBuffModification.Value;
                }
            }

            foreach (var sourceNewBuff in source.NewBuffs)
            {
                var targetNewBuff = target.NewBuffs.FirstOrDefault(buff => buff.NewId == sourceNewBuff.NewId);
                if (targetNewBuff is null)
                {
                    target.NewBuffs.Add(sourceNewBuff);
                    continue;
                }

                foreach (var sourceBuffModification in sourceNewBuff.Modifications)
                {
                    var targetBuffModification = targetNewBuff.Modifications.FirstOrDefault(mod => mod.Id == sourceBuffModification.Id);
                    if (targetBuffModification is null)
                    {
                        targetNewBuff.Modifications.Add(sourceBuffModification);
                        continue;
                    }

                    targetBuffModification.Type = sourceBuffModification.Type;
                    targetBuffModification.Value = sourceBuffModification.Value;
                }
            }
        }
    }
}