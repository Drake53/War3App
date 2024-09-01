using System.Collections.Generic;

namespace War3App.MapAdapter
{
    public static class PathConstants
    {
        // Script
        public const string TriggerDataPath = @"ui\triggerdata.txt";
        public const string CommonJPath = @"scripts\common.j";
        public const string BlizzardJPath = @"scripts\blizzard.j";

        // Unit
        public const string UnitDataPath = @"units\unitdata.slk";
        public const string UnitAbilityDataPath = @"units\unitabilities.slk";
        public const string UnitBalanceDataPath = @"units\unitbalance.slk";
        public const string UnitUiDataPath = @"units\unitui.slk";
        public const string UnitWeaponDataPath = @"units\unitweapons.slk";

        // Item
        public const string ItemDataPath = @"units\itemdata.slk";

        // Destructable
        public const string DestructableDataPath = @"units\destructabledata.slk";

        // Doodad
        public const string DoodadDataPath = @"doodads\doodads.slk";

        // Ability
        public const string AbilityDataPath = @"units\abilitydata.slk";

        // Buff
        public const string BuffDataPath = @"units\abilitybuffdata.slk";

        // Upgrade
        public const string UpgradeDataPath = @"units\upgradedata.slk";

        // Metadata
        public const string UnitMetaDataPath = @"units\unitmetadata.slk";
        public const string ItemMetaDataPath = UnitMetaDataPath;
        public const string DestructableMetaDataPath = @"units\destructablemetadata.slk";
        public const string DoodadMetaDataPath = @"doodads\doodadmetadata.slk";
        public const string AbilityMetaDataPath = @"units\abilitymetadata.slk";
        public const string BuffMetaDataPath = @"units\abilitybuffmetadata.slk";
        public const string UpgradeMetaDataPath = @"units\upgrademetadata.slk";

        public static IEnumerable<string> GetAllPaths()
        {
            yield return TriggerDataPath;
            yield return CommonJPath;
            yield return BlizzardJPath;
            yield return UnitDataPath;
            yield return UnitAbilityDataPath;
            yield return UnitBalanceDataPath;
            yield return UnitUiDataPath;
            yield return UnitWeaponDataPath;
            yield return ItemDataPath;
            yield return DestructableDataPath;
            yield return DoodadDataPath;
            yield return AbilityDataPath;
            yield return BuffDataPath;
            yield return UpgradeDataPath;
            yield return UnitMetaDataPath;
            yield return DestructableMetaDataPath;
            yield return DoodadMetaDataPath;
            yield return AbilityMetaDataPath;
            yield return BuffMetaDataPath;
            yield return UpgradeMetaDataPath;
        }
    }
}