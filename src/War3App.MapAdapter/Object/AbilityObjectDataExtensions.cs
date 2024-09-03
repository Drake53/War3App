using System.Linq;

using War3Net.Build.Common;
using War3Net.Build.Object;

namespace War3App.MapAdapter.Object
{
    public static class AbilityObjectDataExtensions
    {
        public static bool TryDowngrade(this AbilityObjectData abilityObjectData, GamePatch targetPatch)
        {
            try
            {
                while (abilityObjectData.GetMinimumPatch() > targetPatch)
                {
                    abilityObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this AbilityObjectData abilityObjectData)
        {
            switch (abilityObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.v3:
                    abilityObjectData.FormatVersion = ObjectDataFormatVersion.v2;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this AbilityObjectData abilityObjectData)
        {
            return abilityObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.v1 => GamePatch.v1_00,
                ObjectDataFormatVersion.v2 => GamePatch.v1_00,
                ObjectDataFormatVersion.v3 => GamePatch.v1_33_0,
            };
        }

        public static void MergeWith(this AbilityObjectData target, AbilityObjectData source)
        {
            foreach (var sourceBaseAbility in source.BaseAbilities)
            {
                var targetBaseAbility = target.BaseAbilities.FirstOrDefault(ability => ability.OldId == sourceBaseAbility.OldId);
                if (targetBaseAbility is null)
                {
                    target.BaseAbilities.Add(sourceBaseAbility);
                    continue;
                }

                foreach (var sourceAbilityModification in sourceBaseAbility.Modifications)
                {
                    var targetAbilityModification = targetBaseAbility.Modifications.FirstOrDefault(mod
                        => mod.Id == sourceAbilityModification.Id
                        && mod.Level == sourceAbilityModification.Level
                        && mod.Pointer == sourceAbilityModification.Pointer);

                    if (targetAbilityModification is null)
                    {
                        targetBaseAbility.Modifications.Add(sourceAbilityModification);
                        continue;
                    }

                    targetAbilityModification.Type = sourceAbilityModification.Type;
                    targetAbilityModification.Value = sourceAbilityModification.Value;
                }
            }

            foreach (var sourceNewAbility in source.NewAbilities)
            {
                var targetNewAbility = target.NewAbilities.FirstOrDefault(ability => ability.NewId == sourceNewAbility.NewId);
                if (targetNewAbility is null)
                {
                    target.NewAbilities.Add(sourceNewAbility);
                    continue;
                }

                foreach (var sourceAbilityModification in sourceNewAbility.Modifications)
                {
                    var targetAbilityModification = targetNewAbility.Modifications.FirstOrDefault(mod
                        => mod.Id == sourceAbilityModification.Id
                        && mod.Level == sourceAbilityModification.Level
                        && mod.Pointer == sourceAbilityModification.Pointer);

                    if (targetAbilityModification is null)
                    {
                        targetNewAbility.Modifications.Add(sourceAbilityModification);
                        continue;
                    }

                    targetAbilityModification.Type = sourceAbilityModification.Type;
                    targetAbilityModification.Value = sourceAbilityModification.Value;
                }
            }
        }
    }
}