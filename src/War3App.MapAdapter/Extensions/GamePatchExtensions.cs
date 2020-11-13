using System;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Extensions
{
    public static class GamePatchExtensions
    {
        public static int GetEditorVersion(this GamePatch gamePatch)
        {
            return gamePatch switch
            {
                GamePatch.v1_28 => 6059,
                GamePatch.v1_29_0 => 6060,
                GamePatch.v1_30_0 => 6061,
                GamePatch.v1_31_0 => 6072,

                _ => throw new NotSupportedException(),
            };
        }
    }
}