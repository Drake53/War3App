using War3Net.Build.Common;
using War3Net.Build.Object;

namespace War3App.MapAdapter.Object
{
    public static class AbilityObjectDataExtensions
    {
        public static bool TryDowngrade(this AbilityObjectData abilityObjectData, GamePatch targetPatch)
        {
            try
            {
                while (abilityObjectData.GetMinimumPatch() > targetPatch)
                {
                    abilityObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this AbilityObjectData abilityObjectData)
        {
            switch (abilityObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.V3:
                    abilityObjectData.FormatVersion = ObjectDataFormatVersion.Normal;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this AbilityObjectData abilityObjectData)
        {
            return abilityObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.V1 => GamePatch.v1_00,
                ObjectDataFormatVersion.Normal => GamePatch.v1_00,
                ObjectDataFormatVersion.V3 => GamePatch.v1_33_0,
            };
        }
    }
}