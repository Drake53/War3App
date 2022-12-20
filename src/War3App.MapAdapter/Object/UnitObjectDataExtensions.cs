using War3Net.Build.Common;
using War3Net.Build.Object;

namespace War3App.MapAdapter.Object
{
    public static class UnitObjectDataExtensions
    {
        public static bool TryDowngrade(this UnitObjectData unitObjectData, GamePatch targetPatch)
        {
            try
            {
                while (unitObjectData.GetMinimumPatch() > targetPatch)
                {
                    unitObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this UnitObjectData unitObjectData)
        {
            switch (unitObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.v3:
                    unitObjectData.FormatVersion = ObjectDataFormatVersion.v2;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this UnitObjectData unitObjectData)
        {
            return unitObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.v1 => GamePatch.v1_00,
                ObjectDataFormatVersion.v2 => GamePatch.v1_00,
                ObjectDataFormatVersion.v3 => GamePatch.v1_33_0,
            };
        }
    }
}