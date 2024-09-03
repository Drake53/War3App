using System.Linq;

using War3Net.Build.Common;
using War3Net.Build.Object;

namespace War3App.MapAdapter.Object
{
    public static class UnitObjectDataExtensions
    {
        public static bool TryDowngrade(this UnitObjectData unitObjectData, GamePatch targetPatch)
        {
            try
            {
                while (unitObjectData.GetMinimumPatch() > targetPatch)
                {
                    unitObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this UnitObjectData unitObjectData)
        {
            switch (unitObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.v3:
                    unitObjectData.FormatVersion = ObjectDataFormatVersion.v2;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this UnitObjectData unitObjectData)
        {
            return unitObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.v1 => GamePatch.v1_00,
                ObjectDataFormatVersion.v2 => GamePatch.v1_00,
                ObjectDataFormatVersion.v3 => GamePatch.v1_33_0,
            };
        }

        public static void MergeWith(this UnitObjectData target, UnitObjectData source)
        {
            foreach (var sourceBaseUnit in source.BaseUnits)
            {
                var targetBaseUnit = target.BaseUnits.FirstOrDefault(unit => unit.OldId == sourceBaseUnit.OldId);
                if (targetBaseUnit is null)
                {
                    target.BaseUnits.Add(sourceBaseUnit);
                    continue;
                }

                foreach (var sourceUnitModification in sourceBaseUnit.Modifications)
                {
                    var targetUnitModification = targetBaseUnit.Modifications.FirstOrDefault(mod => mod.Id == sourceUnitModification.Id);
                    if (targetUnitModification is null)
                    {
                        targetBaseUnit.Modifications.Add(sourceUnitModification);
                        continue;
                    }

                    targetUnitModification.Type = sourceUnitModification.Type;
                    targetUnitModification.Value = sourceUnitModification.Value;
                }
            }

            foreach (var sourceNewUnit in source.NewUnits)
            {
                var targetNewUnit = target.NewUnits.FirstOrDefault(unit => unit.NewId == sourceNewUnit.NewId);
                if (targetNewUnit is null)
                {
                    target.NewUnits.Add(sourceNewUnit);
                    continue;
                }

                foreach (var sourceUnitModification in sourceNewUnit.Modifications)
                {
                    var targetUnitModification = targetNewUnit.Modifications.FirstOrDefault(mod => mod.Id == sourceUnitModification.Id);
                    if (targetUnitModification is null)
                    {
                        targetNewUnit.Modifications.Add(sourceUnitModification);
                        continue;
                    }

                    targetUnitModification.Type = sourceUnitModification.Type;
                    targetUnitModification.Value = sourceUnitModification.Value;
                }
            }
        }
    }
}