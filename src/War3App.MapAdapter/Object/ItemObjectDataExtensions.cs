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
    public static class ItemObjectDataExtensions
    {
        public static MapFileStatus Adapt(this ItemObjectData itemObjectData, AdaptFileContext context)
        {
            var isSkinFileSupported = context.TargetPatch.Patch >= GamePatch.v1_33_0;
            if (!isSkinFileSupported)
            {
                if (context.FileName is null)
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.MissingFileName, ItemObjectData.MapFileName, ItemObjectData.MapSkinFileName);
                }
                else
                {
                    var isSkinFile = context.FileName.EndsWith($"Skin{ItemObjectData.FileExtension}");
                    if (isSkinFile)
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.RemovedSkinData, context.FileName.Replace($"Skin{ItemObjectData.FileExtension}", ItemObjectData.FileExtension));
                        return MapFileStatus.Removed;
                    }
                }
            }

            var missingDataFiles = false;

            var itemDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.ItemDataPath);
            if (!File.Exists(itemDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.ItemDataPath);
                missingDataFiles = true;
            }

            var itemMetaDataPath = Path.Combine(context.TargetPatch.GameDataPath, PathConstants.ItemMetaDataPath);
            if (!File.Exists(itemMetaDataPath))
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.ItemMetaDataPath);
                missingDataFiles = true;
            }

            if (missingDataFiles)
            {
                return MapFileStatus.ConfigError;
            }

            var isAdapted = false;

            if (itemObjectData.GetMinimumPatch() > context.TargetPatch.Patch)
            {
                if (!itemObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    return MapFileStatus.Incompatible;
                }

                isAdapted = true;
            }

            if (!isSkinFileSupported && context.FileName is not null)
            {
                var expectedSkinFileName = context.FileName.Replace(ItemObjectData.FileExtension, $"Skin{ItemObjectData.FileExtension}");

                if (context.Archive.TryOpenFile(expectedSkinFileName, out var skinStream))
                {
                    ItemObjectData? itemSkinObjectData;
                    try
                    {
                        using var reader = new BinaryReader(skinStream, Encoding.UTF8, true);
                        itemSkinObjectData = reader.ReadItemObjectData();
                    }
                    catch (Exception e)
                    {
                        _ = context.ReportParseError(e);
                        itemSkinObjectData = null;
                    }

                    if (itemSkinObjectData is not null &&
                        (itemSkinObjectData.BaseItems.Count > 0 ||
                         itemSkinObjectData.NewItems.Count > 0))
                    {
                        itemObjectData.MergeWith(itemSkinObjectData);

                        context.ReportDiagnostic(DiagnosticRule.ObjectData.MergedSkinData, expectedSkinFileName);
                        isAdapted = true;
                    }
                }
            }

            var knownIds = new HashSet<int>();
            knownIds.AddItemsFromSylkTable(itemDataPath, DataConstants.ItemDataKeyColumn);

            var knownProperties = new HashSet<int>();
            knownProperties.AddItemsFromSylkTable(itemMetaDataPath, DataConstants.MetaDataIdColumn);

            var baseItems = new List<SimpleObjectModification>();
            foreach (var item in itemObjectData.BaseItems)
            {
                if (!knownIds.Contains(item.OldId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseId, "item", item.OldId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                for (var i = 0; i < item.Modifications.Count; i++)
                {
                    var property = item.Modifications[i];
                    if (!knownProperties.Contains(property.Id))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                        isAdapted = true;
                        item.Modifications.RemoveAt(i--);
                    }
                }

                baseItems.Add(item);
            }

            var newItems = new List<SimpleObjectModification>();
            foreach (var item in itemObjectData.NewItems)
            {
                if (!knownIds.Contains(item.OldId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseIdNew, "item", item.NewId.ToRawcode(), item.OldId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                if (knownIds.Contains(item.NewId))
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.ConflictingId, "item", item.NewId.ToRawcode());
                    isAdapted = true;
                    continue;
                }

                for (var i = 0; i < item.Modifications.Count; i++)
                {
                    var property = item.Modifications[i];
                    if (!knownProperties.Contains(property.Id))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                        isAdapted = true;
                        item.Modifications.RemoveAt(i--);
                    }
                }

                newItems.Add(item);
            }

            if (!isAdapted)
            {
                return MapFileStatus.Compatible;
            }

            itemObjectData.BaseItems.Clear();
            itemObjectData.NewItems.Clear();

            itemObjectData.BaseItems.AddRange(baseItems);
            itemObjectData.NewItems.AddRange(newItems);

            return MapFileStatus.Adapted;
        }

        public static bool TryDowngrade(this ItemObjectData itemObjectData, GamePatch targetPatch)
        {
            try
            {
                while (itemObjectData.GetMinimumPatch() > targetPatch)
                {
                    itemObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this ItemObjectData itemObjectData)
        {
            switch (itemObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.v3:
                    itemObjectData.FormatVersion = ObjectDataFormatVersion.v2;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this ItemObjectData itemObjectData)
        {
            return itemObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.v1 => GamePatch.v1_00,
                ObjectDataFormatVersion.v2 => GamePatch.v1_00,
                ObjectDataFormatVersion.v3 => GamePatch.v1_33_0,
            };
        }

        public static void MergeWith(this ItemObjectData target, ItemObjectData source)
        {
            foreach (var sourceBaseItem in source.BaseItems)
            {
                var targetBaseItem = target.BaseItems.FirstOrDefault(item => item.OldId == sourceBaseItem.OldId);
                if (targetBaseItem is null)
                {
                    target.BaseItems.Add(sourceBaseItem);
                    continue;
                }

                foreach (var sourceItemModification in sourceBaseItem.Modifications)
                {
                    var targetItemModification = targetBaseItem.Modifications.FirstOrDefault(mod => mod.Id == sourceItemModification.Id);
                    if (targetItemModification is null)
                    {
                        targetBaseItem.Modifications.Add(sourceItemModification);
                        continue;
                    }

                    targetItemModification.Type = sourceItemModification.Type;
                    targetItemModification.Value = sourceItemModification.Value;
                }
            }

            foreach (var sourceNewItem in source.NewItems)
            {
                var targetNewItem = target.NewItems.FirstOrDefault(item => item.NewId == sourceNewItem.NewId);
                if (targetNewItem is null)
                {
                    target.NewItems.Add(sourceNewItem);
                    continue;
                }

                foreach (var sourceItemModification in sourceNewItem.Modifications)
                {
                    var targetItemModification = targetNewItem.Modifications.FirstOrDefault(mod => mod.Id == sourceItemModification.Id);
                    if (targetItemModification is null)
                    {
                        targetNewItem.Modifications.Add(sourceItemModification);
                        continue;
                    }

                    targetItemModification.Type = sourceItemModification.Type;
                    targetItemModification.Value = sourceItemModification.Value;
                }
            }
        }
    }
}