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
                case ObjectDataFormatVersion.V3:
                    upgradeObjectData.FormatVersion = ObjectDataFormatVersion.Normal;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this UpgradeObjectData upgradeObjectData)
        {
            return upgradeObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.V1 => GamePatch.v1_00,
                ObjectDataFormatVersion.Normal => GamePatch.v1_00,
                ObjectDataFormatVersion.V3 => GamePatch.v1_33_0,
            };
        }
    }
}