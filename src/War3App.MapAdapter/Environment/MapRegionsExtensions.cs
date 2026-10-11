using War3Net.Build.Common;
using War3Net.Build.Environment;

namespace War3App.MapAdapter.Environment
{
    public static class MapRegionsExtensions
    {
        public static bool Adapt(this MapRegions mapRegions, AdaptFileContext context, out MapFileStatus status)
        {
            if (mapRegions.GetMinimumPatch() <= context.TargetPatch.Patch)
            {
                status = MapFileStatus.Compatible;
                return false;
            }

            status = mapRegions.TryDowngrade(context)
                ? MapFileStatus.Compatible
                : MapFileStatus.Incompatible;

            return status == MapFileStatus.Compatible;
        }

        public static bool TryDowngrade(
            this MapRegions mapRegions,
            AdaptFileContext context)
        {
            try
            {
                while (mapRegions.GetMinimumPatch() > context.TargetPatch.Patch)
                {
                    mapRegions.DowngradeOnce(context);
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(
            this MapRegions mapRegions,
            AdaptFileContext context)
        {
            switch (mapRegions.FormatVersion)
            {
                case MapRegionsFormatVersion.v7:
                    mapRegions.FormatVersion = MapRegionsFormatVersion.v5;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this MapRegions mapRegions)
        {
            if (mapRegions.FormatVersion == MapRegionsFormatVersion.v7)
            {
                return GamePatch.v3_0_0;
            }

            return GamePatch.v1_00;
        }
    }
}