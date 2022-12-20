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
    }
}