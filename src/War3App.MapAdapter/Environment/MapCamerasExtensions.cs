using System;

using War3Net.Build.Common;
using War3Net.Build.Environment;

namespace War3App.MapAdapter.Environment
{
    public static class MapCamerasExtensions
    {
        public static bool Adapt(this MapCameras mapCameras, AdaptFileContext context, out MapFileStatus status)
        {
            if (mapCameras.GetMinimumPatch() <= context.TargetPatch.Patch)
            {
                status = MapFileStatus.Compatible;
                return false;
            }

            status = mapCameras.TryDowngrade(context)
                ? MapFileStatus.Compatible
                : MapFileStatus.Incompatible;

            return status == MapFileStatus.Compatible;
        }

        public static bool TryDowngrade(
            this MapCameras mapCameras,
            AdaptFileContext context)
        {
            try
            {
                while (mapCameras.GetMinimumPatch() > context.TargetPatch.Patch)
                {
                    mapCameras.DowngradeOnce(context);
                }

                return true;
            }
            catch (NotSupportedException)
            {
                return false;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(
            this MapCameras mapCameras,
            AdaptFileContext context)
        {
            if (mapCameras.UseNewFormat)
            {
                mapCameras.UseNewFormat = false;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static GamePatch GetMinimumPatch(this MapCameras mapCameras)
        {
            return mapCameras.UseNewFormat && mapCameras.Cameras.Count > 0 ? GamePatch.v1_31_0 : GamePatch.v1_00;
        }
    }
}