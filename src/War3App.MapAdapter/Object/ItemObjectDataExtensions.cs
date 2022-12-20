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
    }
}