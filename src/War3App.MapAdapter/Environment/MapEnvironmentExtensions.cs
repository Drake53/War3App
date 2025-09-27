using War3Net.Build.Common;
using War3Net.Build.Environment;

namespace War3App.MapAdapter.Environment
{
    public static class MapEnvironmentExtensions
    {
        public static bool Adapt(this MapEnvironment mapEnvironment, AdaptFileContext context, out MapFileStatus status)
        {
            if (mapEnvironment.GetMinimumPatch() <= context.TargetPatch.Patch)
            {
                status = MapFileStatus.Compatible;
                return false;
            }

            status = mapEnvironment.TryDowngrade(context.TargetPatch.Patch)
                ? MapFileStatus.Compatible
                : MapFileStatus.Incompatible;

            return status == MapFileStatus.Compatible;
        }

        public static bool TryDowngrade(this MapEnvironment mapEnvironment, GamePatch targetPatch)
        {
            try
            {
                while (mapEnvironment.GetMinimumPatch() > targetPatch)
                {
                    mapEnvironment.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this MapEnvironment mapEnvironment)
        {
            switch (mapEnvironment.FormatVersion)
            {
                case MapEnvironmentFormatVersion.v12:
                    mapEnvironment.FormatVersion = MapEnvironmentFormatVersion.v11;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this MapEnvironment mapEnvironment)
        {
            return mapEnvironment.FormatVersion switch
            {
                MapEnvironmentFormatVersion.v11 => GamePatch.v1_00,
                MapEnvironmentFormatVersion.v12 => GamePatch.v2_0_3,
            };
        }
    }
}