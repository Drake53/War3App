using System.Linq;

using War3Net.Build.Common;
using War3Net.Build.Object;

namespace War3App.MapAdapter.Object
{
    public static class UpgradeObjectDataExtensions
    {
        public static bool TryDowngrade(this UpgradeObjectData upgradeObjectData, GamePatch targetPatch)
        {
            try
            {
                while (upgradeObjectData.GetMinimumPatch() > targetPatch)
                {
                    upgradeObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this UpgradeObjectData upgradeObjectData)
        {
            switch (upgradeObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.v3:
                    upgradeObjectData.FormatVersion = ObjectDataFormatVersion.v2;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this UpgradeObjectData upgradeObjectData)
        {
            return upgradeObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.v1 => GamePatch.v1_00,
                ObjectDataFormatVersion.v2 => GamePatch.v1_00,
                ObjectDataFormatVersion.v3 => GamePatch.v1_33_0,
            };
        }

        public static void MergeWith(this UpgradeObjectData target, UpgradeObjectData source)
        {
            foreach (var sourceBaseUpgrade in source.BaseUpgrades)
            {
                var targetBaseUpgrade = target.BaseUpgrades.FirstOrDefault(upgrade => upgrade.OldId == sourceBaseUpgrade.OldId);
                if (targetBaseUpgrade is null)
                {
                    target.BaseUpgrades.Add(sourceBaseUpgrade);
                    continue;
                }

                foreach (var sourceUpgradeModification in sourceBaseUpgrade.Modifications)
                {
                    var targetUpgradeModification = targetBaseUpgrade.Modifications.FirstOrDefault(mod
                        => mod.Id == sourceUpgradeModification.Id
                        && mod.Level == sourceUpgradeModification.Level
                        && mod.Pointer == sourceUpgradeModification.Pointer);

                    if (targetUpgradeModification is null)
                    {
                        targetBaseUpgrade.Modifications.Add(sourceUpgradeModification);
                        continue;
                    }

                    targetUpgradeModification.Type = sourceUpgradeModification.Type;
                    targetUpgradeModification.Value = sourceUpgradeModification.Value;
                }
            }

            foreach (var sourceNewUpgrade in source.NewUpgrades)
            {
                var targetNewUpgrade = target.NewUpgrades.FirstOrDefault(upgrade => upgrade.NewId == sourceNewUpgrade.NewId);
                if (targetNewUpgrade is null)
                {
                    target.NewUpgrades.Add(sourceNewUpgrade);
                    continue;
                }

                foreach (var sourceUpgradeModification in sourceNewUpgrade.Modifications)
                {
                    var targetUpgradeModification = targetNewUpgrade.Modifications.FirstOrDefault(mod
                        => mod.Id == sourceUpgradeModification.Id
                        && mod.Level == sourceUpgradeModification.Level
                        && mod.Pointer == sourceUpgradeModification.Pointer);

                    if (targetUpgradeModification is null)
                    {
                        targetNewUpgrade.Modifications.Add(sourceUpgradeModification);
                        continue;
                    }

                    targetUpgradeModification.Type = sourceUpgradeModification.Type;
                    targetUpgradeModification.Value = sourceUpgradeModification.Value;
                }
            }
        }
    }
}