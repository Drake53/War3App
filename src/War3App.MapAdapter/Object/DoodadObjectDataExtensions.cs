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
    }
}