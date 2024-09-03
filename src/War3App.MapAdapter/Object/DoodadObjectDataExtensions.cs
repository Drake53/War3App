using System.Linq;

using War3Net.Build.Common;
using War3Net.Build.Object;

namespace War3App.MapAdapter.Object
{
    public static class DoodadObjectDataExtensions
    {
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