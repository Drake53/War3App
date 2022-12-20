using War3Net.Build.Common;
using War3Net.Build.Object;

namespace War3App.MapAdapter.Object
{
    public static class UpgradeObjectDataExtensions
    {
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
    }
}