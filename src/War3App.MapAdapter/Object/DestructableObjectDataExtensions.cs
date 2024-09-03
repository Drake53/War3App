using System.Linq;

using War3Net.Build.Common;
using War3Net.Build.Object;

namespace War3App.MapAdapter.Object
{
    public static class DestructableObjectDataExtensions
    {
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