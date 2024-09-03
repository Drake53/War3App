using System.Linq;

using War3Net.Build.Common;
using War3Net.Build.Object;

namespace War3App.MapAdapter.Object
{
    public static class BuffObjectDataExtensions
    {
        public static bool TryDowngrade(this BuffObjectData buffObjectData, GamePatch targetPatch)
        {
            try
            {
                while (buffObjectData.GetMinimumPatch() > targetPatch)
                {
                    buffObjectData.DowngradeOnce();
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public static void DowngradeOnce(this BuffObjectData buffObjectData)
        {
            switch (buffObjectData.FormatVersion)
            {
                case ObjectDataFormatVersion.v3:
                    buffObjectData.FormatVersion = ObjectDataFormatVersion.v2;
                    break;

                default:
                    break;
            }
        }

        public static GamePatch GetMinimumPatch(this BuffObjectData buffObjectData)
        {
            return buffObjectData.FormatVersion switch
            {
                ObjectDataFormatVersion.v1 => GamePatch.v1_00,
                ObjectDataFormatVersion.v2 => GamePatch.v1_00,
                ObjectDataFormatVersion.v3 => GamePatch.v1_33_0,
            };
        }

        public static void MergeWith(this BuffObjectData target, BuffObjectData source)
        {
            foreach (var sourceBaseBuff in source.BaseBuffs)
            {
                var targetBaseBuff = target.BaseBuffs.FirstOrDefault(buff => buff.OldId == sourceBaseBuff.OldId);
                if (targetBaseBuff is null)
                {
                    target.BaseBuffs.Add(sourceBaseBuff);
                    continue;
                }

                foreach (var sourceBuffModification in sourceBaseBuff.Modifications)
                {
                    var targetBuffModification = targetBaseBuff.Modifications.FirstOrDefault(mod => mod.Id == sourceBuffModification.Id);
                    if (targetBuffModification is null)
                    {
                        targetBaseBuff.Modifications.Add(sourceBuffModification);
                        continue;
                    }

                    targetBuffModification.Type = sourceBuffModification.Type;
                    targetBuffModification.Value = sourceBuffModification.Value;
                }
            }

            foreach (var sourceNewBuff in source.NewBuffs)
            {
                var targetNewBuff = target.NewBuffs.FirstOrDefault(buff => buff.NewId == sourceNewBuff.NewId);
                if (targetNewBuff is null)
                {
                    target.NewBuffs.Add(sourceNewBuff);
                    continue;
                }

                foreach (var sourceBuffModification in sourceNewBuff.Modifications)
                {
                    var targetBuffModification = targetNewBuff.Modifications.FirstOrDefault(mod => mod.Id == sourceBuffModification.Id);
                    if (targetBuffModification is null)
                    {
                        targetNewBuff.Modifications.Add(sourceBuffModification);
                        continue;
                    }

                    targetBuffModification.Type = sourceBuffModification.Type;
                    targetBuffModification.Value = sourceBuffModification.Value;
                }
            }
        }
    }
}