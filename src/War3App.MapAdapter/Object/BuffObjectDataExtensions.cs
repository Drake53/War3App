using War3Net.Build.Common;
using War3Net.Build.Object;

namespace War3App.MapAdapter.Object
{
    public static class BuffObjectDataExtensions
    {
        public static bool TryDowngrade(this BuffObjectData buffObjectData, GamePatch targetPatch)
        {
            try
            {
                while (buffObjectData.GetMinimumPatch() > targetPatch)
                {
                    buffObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this BuffObjectData buffObjectData)
        {
            switch (buffObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.V3:
                    buffObjectData.FormatVersion = ObjectDataFormatVersion.Normal;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this BuffObjectData buffObjectData)
        {
            return buffObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.V1 => GamePatch.v1_00,
                ObjectDataFormatVersion.Normal => GamePatch.v1_00,
                ObjectDataFormatVersion.V3 => GamePatch.v1_33_0,
            };
        }
    }
}