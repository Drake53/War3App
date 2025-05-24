using System;
using System.Collections.Generic;
using System.IO;

using War3App.MapAdapter.Diagnostics;

namespace War3App.MapAdapter.Extensions
{
    public static class AdaptFileContextExtensions
    {
        public static HashSet<int>? GetKnownAbilityIdsFromSylkTables(this AdaptFileContext context)
        {
            var knownIds = new HashSet<int>();

            var ok = context.TryAddItemsFromSylkTable(knownIds, PathConstants.AbilityDataPath, DataConstants.AbilityDataKeyColumn);

            return ok ? knownIds : null;
        }

        public static HashSet<int>? GetKnownBuffIdsFromSylkTables(this AdaptFileContext context)
        {
            var knownIds = new HashSet<int>();

            var ok = context.TryAddItemsFromSylkTable(knownIds, PathConstants.BuffDataPath, DataConstants.BuffDataKeyColumn);

            return ok ? knownIds : null;
        }

        public static HashSet<int>? GetKnownDestructableIdsFromSylkTables(this AdaptFileContext context)
        {
            var knownIds = new HashSet<int>();

            var ok = context.TryAddItemsFromSylkTable(knownIds, PathConstants.DestructableDataPath, DataConstants.DestructableDataKeyColumn);

            return ok ? knownIds : null;
        }

        public static HashSet<int>? GetKnownDoodadIdsFromSylkTables(this AdaptFileContext context)
        {
            var knownIds = new HashSet<int>();

            var ok = context.TryAddItemsFromSylkTable(knownIds, PathConstants.DoodadDataPath, DataConstants.DoodadDataKeyColumn);

            return ok ? knownIds : null;
        }

        public static HashSet<int>? GetKnownItemIdsFromSylkTables(this AdaptFileContext context)
        {
            var knownIds = new HashSet<int>();

            var ok = context.TryAddItemsFromSylkTable(knownIds, PathConstants.ItemDataPath, DataConstants.ItemDataKeyColumn);

            return ok ? knownIds : null;
        }

        public static HashSet<int>? GetKnownUnitIdsFromSylkTables(this AdaptFileContext context)
        {
            var knownIds = new HashSet<int>();

            var ok = context.TryAddItemsFromSylkTable(knownIds, PathConstants.UnitAbilityDataPath, DataConstants.UnitAbilityDataKeyColumn)
                   & context.TryAddItemsFromSylkTable(knownIds, PathConstants.UnitBalanceDataPath, DataConstants.UnitBalanceDataKeyColumn)
                   & context.TryAddItemsFromSylkTable(knownIds, PathConstants.UnitDataPath, DataConstants.UnitDataKeyColumn)
                   & context.TryAddItemsFromSylkTable(knownIds, PathConstants.UnitUiDataPath, DataConstants.UnitUiDataKeyColumn)
                   & context.TryAddItemsFromSylkTable(knownIds, PathConstants.UnitWeaponDataPath, DataConstants.UnitWeaponDataKeyColumn, DataConstants.UnitWeaponDataKeyColumnOld);

            return ok ? knownIds : null;
        }

        public static HashSet<int>? GetKnownUpgradeIdsFromSylkTables(this AdaptFileContext context)
        {
            var knownIds = new HashSet<int>();

            var ok = context.TryAddItemsFromSylkTable(knownIds, PathConstants.UpgradeDataPath, DataConstants.UpgradeDataKeyColumn);

            return ok ? knownIds : null;
        }

        public static HashSet<int>? GetKnownPropertiesFromSylkTables(this AdaptFileContext context, string sylkTableDataPath)
        {
            var knownProperties = new HashSet<int>();

            var ok = context.TryAddItemsFromSylkTable(knownProperties, sylkTableDataPath, DataConstants.MetaDataIdColumn);

            return ok ? knownProperties : null;
        }

        private static bool TryAddItemsFromSylkTable(
            this AdaptFileContext context,
            HashSet<int> knownIds,
            string sylkTableDataPath,
            params string[] knownKeyColumnNames)
        {
            using var sylkTableStream = context.TargetPatch.OpenGameDataFile(sylkTableDataPath);
            if (sylkTableStream is not null)
            {
                try
                {
                    knownIds.AddItemsFromSylkTable(sylkTableStream, knownKeyColumnNames);
                    return true;
                }
                catch (Exception e)
                {
                    _ = context.ReportParseError(e);
                    return false;
                }
            }
            else
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, sylkTableDataPath);
                return false;
            }
        }
    }
}