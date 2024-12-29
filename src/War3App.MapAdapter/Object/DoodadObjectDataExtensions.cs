using System;
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
    public static class DoodadObjectDataExtensions
    {
        public static bool Adapt(this DoodadObjectData doodadObjectData, AdaptFileContext context, out MapFileStatus status)
        {
            status = MapFileStatus.Compatible;

            var isSkinFileSupported = context.TargetPatch.Patch >= GamePatch.v1_33_0;
            if (!isSkinFileSupported)
            {
                if (context.FileName is null)
                {
                    context.ReportDiagnostic(DiagnosticRule.ObjectData.MissingFileName, DoodadObjectData.MapFileName, DoodadObjectData.MapSkinFileName);
                }
                else
                {
                    var isSkinFile = context.FileName.EndsWith($"Skin{DoodadObjectData.FileExtension}");
                    if (isSkinFile)
                    {
                        var nonSkinFileName = context.FileName.Replace($"Skin{DoodadObjectData.FileExtension}", DoodadObjectData.FileExtension);

                        if (context.Archive.FileExists(nonSkinFileName))
                        {
                            context.ReportDiagnostic(DiagnosticRule.ObjectData.RemovedSkinData, nonSkinFileName);
                            status = MapFileStatus.Removed;
                            return false;
                        }

                        context.ReportDiagnostic(DiagnosticRule.ObjectData.RenamedSkinData, nonSkinFileName);
                        context.NewFileName = nonSkinFileName;
                    }
                }
            }

            var knownIds = context.GetKnownDoodadIdsFromSylkTables();
            var knownProperties = context.GetKnownPropertiesFromSylkTables(PathConstants.DoodadMetaDataPath);

            if (knownIds is null ||
                knownProperties is null)
            {
                status = MapFileStatus.Inconclusive;
            }

            var isAdapted = false;

            if (doodadObjectData.GetMinimumPatch() > context.TargetPatch.Patch)
            {
                if (doodadObjectData.TryDowngrade(context.TargetPatch.Patch))
                {
                    isAdapted = true;
                }
                else
                {
                    status = MapFileStatus.Incompatible;
                }
            }

            if (!isSkinFileSupported && context.FileName is not null && !context.FileName.EndsWith($"Skin{DoodadObjectData.FileExtension}"))
            {
                var expectedSkinFileName = context.FileName.Replace(DoodadObjectData.FileExtension, $"Skin{DoodadObjectData.FileExtension}");

                if (context.Archive.TryOpenFile(expectedSkinFileName, out var skinStream))
                {
                    DoodadObjectData? doodadSkinObjectData;
                    try
                    {
                        using var reader = new BinaryReader(skinStream, Encoding.UTF8, true);
                        doodadSkinObjectData = reader.ReadDoodadObjectData();
                    }
                    catch (Exception e)
                    {
                        _ = context.ReportParseError(e);
                        doodadSkinObjectData = null;
                    }

                    if (doodadSkinObjectData is not null &&
                        (doodadSkinObjectData.BaseDoodads.Count > 0 ||
                         doodadSkinObjectData.NewDoodads.Count > 0))
                    {
                        doodadObjectData.MergeWith(doodadSkinObjectData);

                        context.ReportDiagnostic(DiagnosticRule.ObjectData.MergedSkinData, expectedSkinFileName);
                        isAdapted = true;
                    }
                }
            }

            for (var i = 0; i < doodadObjectData.BaseDoodads.Count; i++)
            {
                var doodad = doodadObjectData.BaseDoodads[i];
                if (knownIds is not null)
                {
                    if (!knownIds.Contains(doodad.OldId))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseId, "doodad", doodad.OldId.ToRawcode());
                        isAdapted = true;
                        doodadObjectData.BaseDoodads.RemoveAt(i--);
                        continue;
                    }
                }

                if (knownProperties is not null)
                {
                    for (var j = 0; j < doodad.Modifications.Count; j++)
                    {
                        var property = doodad.Modifications[j];
                        if (!knownProperties.Contains(property.Id))
                        {
                            context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                            isAdapted = true;
                            doodad.Modifications.RemoveAt(j--);
                        }
                    }
                }
            }

            for (var i = 0; i < doodadObjectData.NewDoodads.Count; i++)
            {
                var doodad = doodadObjectData.NewDoodads[i];
                if (knownIds is not null)
                {
                    if (!knownIds.Contains(doodad.OldId))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownBaseIdNew, "doodad", doodad.NewId.ToRawcode(), doodad.OldId.ToRawcode());
                        isAdapted = true;
                        doodadObjectData.NewDoodads.RemoveAt(i--);
                        continue;
                    }

                    if (knownIds.Contains(doodad.NewId))
                    {
                        context.ReportDiagnostic(DiagnosticRule.ObjectData.ConflictingId, "doodad", doodad.NewId.ToRawcode());
                        isAdapted = true;
                        doodadObjectData.NewDoodads.RemoveAt(i--);
                        continue;
                    }
                }

                if (knownProperties is not null)
                {
                    for (var j = 0; j < doodad.Modifications.Count; j++)
                    {
                        var property = doodad.Modifications[j];
                        if (!knownProperties.Contains(property.Id))
                        {
                            context.ReportDiagnostic(DiagnosticRule.ObjectData.UnknownProperty, property.Id.ToRawcode());
                            isAdapted = true;
                            doodad.Modifications.RemoveAt(j--);
                        }
                    }
                }
            }

            return isAdapted;
        }

        public static bool TryDowngrade(this DoodadObjectData doodadObjectData, GamePatch targetPatch)
        {
            try
            {
                while (doodadObjectData.GetMinimumPatch() > targetPatch)
                {
                    doodadObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this DoodadObjectData doodadObjectData)
        {
            switch (doodadObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.v3:
                    doodadObjectData.FormatVersion = ObjectDataFormatVersion.v2;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this DoodadObjectData doodadObjectData)
        {
            return doodadObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.v1 => GamePatch.v1_00,
                ObjectDataFormatVersion.v2 => GamePatch.v1_00,
                ObjectDataFormatVersion.v3 => GamePatch.v1_33_0,
            };
        }

        public static void MergeWith(this DoodadObjectData target, DoodadObjectData source)
        {
            foreach (var sourceBaseDoodad in source.BaseDoodads)
            {
                var targetBaseDoodad = target.BaseDoodads.FirstOrDefault(doodad => doodad.OldId == sourceBaseDoodad.OldId);
                if (targetBaseDoodad is null)
                {
                    target.BaseDoodads.Add(sourceBaseDoodad);
                    continue;
                }

                foreach (var sourceDoodadModification in sourceBaseDoodad.Modifications)
                {
                    var targetDoodadModification = targetBaseDoodad.Modifications.FirstOrDefault(mod
                        => mod.Id == sourceDoodadModification.Id
                        && mod.Variation == sourceDoodadModification.Variation
                        && mod.Pointer == sourceDoodadModification.Pointer);

                    if (targetDoodadModification is null)
                    {
                        targetBaseDoodad.Modifications.Add(sourceDoodadModification);
                        continue;
                    }

                    targetDoodadModification.Type = sourceDoodadModification.Type;
                    targetDoodadModification.Value = sourceDoodadModification.Value;
                }
            }

            foreach (var sourceNewDoodad in source.NewDoodads)
            {
                var targetNewDoodad = target.NewDoodads.FirstOrDefault(doodad => doodad.NewId == sourceNewDoodad.NewId);
                if (targetNewDoodad is null)
                {
                    target.NewDoodads.Add(sourceNewDoodad);
                    continue;
                }

                foreach (var sourceDoodadModification in sourceNewDoodad.Modifications)
                {
                    var targetDoodadModification = targetNewDoodad.Modifications.FirstOrDefault(mod
                        => mod.Id == sourceDoodadModification.Id
                        && mod.Variation == sourceDoodadModification.Variation
                        && mod.Pointer == sourceDoodadModification.Pointer);

                    if (targetDoodadModification is null)
                    {
                        targetNewDoodad.Modifications.Add(sourceDoodadModification);
                        continue;
                    }

                    targetDoodadModification.Type = sourceDoodadModification.Type;
                    targetDoodadModification.Value = sourceDoodadModification.Value;
                }
            }
        }
    }
}