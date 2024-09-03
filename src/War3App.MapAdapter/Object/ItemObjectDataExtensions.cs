using System.Linq;

using War3Net.Build.Common;
using War3Net.Build.Object;

namespace War3App.MapAdapter.Object
{
    public static class ItemObjectDataExtensions
    {
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