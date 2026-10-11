using System.Collections.Generic;

using War3Net.Build.Common;
using War3Net.Build.Environment;
using War3Net.Build.Info;
using War3Net.Build.Providers;

namespace War3App.MapAdapter
{
    public static class EnumSupport
    {
        private static readonly Dictionary<Tileset, GamePatch> _requiredPatchByTileset = BuildRequiredPatchByTileset();
        private static readonly Dictionary<CliffType, GamePatch> _requiredPatchByCliffType = BuildRequiredPatchByCliffType();
        private static readonly Dictionary<TerrainType, GamePatch> _requiredPatchByTerrainType = BuildRequiredPatchByTerrainType();
        private static readonly Dictionary<WeatherType, GamePatch> _requiredPatchByWeatherType = BuildRequiredPatchByWeatherType();
        private static readonly Dictionary<CampaignRace, GamePatch> _requiredPatchByCampaignRace = BuildRequiredPatchByCampaignRace();
        private static readonly Dictionary<FogStyle, GamePatch> _requiredPatchByFogStyle = BuildRequiredPatchByFogStyle();

        public static GamePatch GetMinimumPatch(Tileset tileset)
        {
            return _requiredPatchByTileset.GetValueOrDefault(tileset, GamePatch.v1_00);
        }

        public static GamePatch GetMinimumPatch(CliffType cliffType)
        {
            return _requiredPatchByCliffType.GetValueOrDefault(cliffType, GamePatch.v1_00);
        }

        public static GamePatch GetMinimumPatch(TerrainType terrainType)
        {
            return _requiredPatchByTerrainType.GetValueOrDefault(terrainType, GamePatch.v1_00);
        }

        public static GamePatch GetMinimumPatch(WeatherType weatherType)
        {
            return _requiredPatchByWeatherType.GetValueOrDefault(weatherType, GamePatch.v1_00);
        }

        public static GamePatch GetMinimumPatch(CampaignRace campaignRace)
        {
            return _requiredPatchByCampaignRace.GetValueOrDefault(campaignRace, GamePatch.v1_00);
        }

        public static GamePatch GetMinimumPatch(FogStyle fogStyle)
        {
            return _requiredPatchByFogStyle.GetValueOrDefault(fogStyle, GamePatch.v1_00);
        }

        private static Dictionary<Tileset, GamePatch> BuildRequiredPatchByTileset()
        {
            var result = new Dictionary<Tileset, GamePatch>();
            result.Add(Tileset.CityscapeRuins, GamePatch.v3_0_0);
            result.Add(Tileset.LordaeronCapital, GamePatch.v3_0_0);
            result.Add(Tileset.LordaeronCapitalRuins, GamePatch.v3_0_0);
            result.Add(Tileset.Misc, GamePatch.v3_0_0);
            result.Add(Tileset.Undercity, GamePatch.v3_0_0);

            return result;
        }

        private static Dictionary<CliffType, GamePatch> BuildRequiredPatchByCliffType()
        {
            var result = new Dictionary<CliffType, GamePatch>();
            foreach (var kvp in _requiredPatchByTileset)
            {
                foreach (var cliffType in TerrainTypeProvider.GetCliffTypes(kvp.Key))
                {
                    result.Add(cliffType, kvp.Value);
                }
            }

            return result;
        }

        private static Dictionary<TerrainType, GamePatch> BuildRequiredPatchByTerrainType()
        {
            var result = new Dictionary<TerrainType, GamePatch>();
            foreach (var kvp in _requiredPatchByTileset)
            {
                foreach (var terrainType in TerrainTypeProvider.GetTerrainTypes(kvp.Key))
                {
                    result.Add(terrainType, kvp.Value);
                }
            }

            return result;
        }

        private static Dictionary<WeatherType, GamePatch> BuildRequiredPatchByWeatherType()
        {
            var result = new Dictionary<WeatherType, GamePatch>();
            result.Add(WeatherType.Breeze, GamePatch.v3_0_0);
            result.Add(WeatherType.Bubbles, GamePatch.v3_0_0);
            result.Add(WeatherType.EmberRain, GamePatch.v3_0_0);
            result.Add(WeatherType.EmberRainGold, GamePatch.v3_0_0);
            result.Add(WeatherType.EmberRainGreen, GamePatch.v3_0_0);
            result.Add(WeatherType.EmberRainOrange, GamePatch.v3_0_0);
            result.Add(WeatherType.EmberRainPurple, GamePatch.v3_0_0);
            result.Add(WeatherType.EnergyFieldBlue, GamePatch.v3_0_0);
            result.Add(WeatherType.EnergyFieldGreen, GamePatch.v3_0_0);
            result.Add(WeatherType.EnergyFieldHoly, GamePatch.v3_0_0);
            result.Add(WeatherType.EnergyFieldRed, GamePatch.v3_0_0);
            result.Add(WeatherType.LeavesFallingAutumn, GamePatch.v3_0_0);
            result.Add(WeatherType.LeavesFallingSummer, GamePatch.v3_0_0);
            result.Add(WeatherType.Plague, GamePatch.v3_0_0);
            result.Add(WeatherType.PlagueLight, GamePatch.v3_0_0);
            result.Add(WeatherType.RainAcid, GamePatch.v3_0_0);
            result.Add(WeatherType.RainAle, GamePatch.v3_0_0);
            result.Add(WeatherType.RainBlood, GamePatch.v3_0_0);
            result.Add(WeatherType.RainLight, GamePatch.v3_0_0);
            result.Add(WeatherType.RainOil, GamePatch.v3_0_0);
            result.Add(WeatherType.RainPurple, GamePatch.v3_0_0);
            result.Add(WeatherType.RainSmudge, GamePatch.v3_0_0);
            result.Add(WeatherType.Snowflakes, GamePatch.v3_0_0);

            return result;
        }

        private static Dictionary<CampaignRace, GamePatch> BuildRequiredPatchByCampaignRace()
        {
            var result = new Dictionary<CampaignRace, GamePatch>();
            result.Add(CampaignRace.Forsaken, GamePatch.v3_0_0);

            return result;
        }

        private static Dictionary<FogStyle, GamePatch> BuildRequiredPatchByFogStyle()
        {
            var result = new Dictionary<FogStyle, GamePatch>();
            result.Add(FogStyle.Height, GamePatch.v3_0_0);
            result.Add(FogStyle.NewExponential1, GamePatch.v3_0_0);
            result.Add(FogStyle.NewExponential2, GamePatch.v3_0_0);

            return result;
        }
    }
}