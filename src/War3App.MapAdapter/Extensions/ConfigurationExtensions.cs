using System;
using System.Linq;

using Microsoft.Extensions.Configuration;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Extensions
{
    public static class ConfigurationExtensions
    {
        public static AppSettings LoadAppSettings(this IConfiguration configuration)
        {
            return new AppSettings
            {
                TargetPatches = configuration.GetSection(nameof(AppSettings.TargetPatches)).GetChildren().Select(targetPatch => new TargetPatch
                {
                    Patch = Enum.Parse<GamePatch>(targetPatch.GetSection(nameof(TargetPatch.Patch)).Value),
                    GameDataPath = targetPatch.GetSection(nameof(TargetPatch.GameDataPath)).Value,
                    GameDataContainerType = ContainerType.Directory,
                }).ToList(),
            };
        }
    }
}