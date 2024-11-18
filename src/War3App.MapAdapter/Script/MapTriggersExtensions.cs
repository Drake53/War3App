using System;
using System.Collections.Generic;
using System.Linq;

using War3Net.Build.Common;
using War3Net.Build.Script;

namespace War3App.MapAdapter.Script
{
    public static class MapTriggersExtensions
    {
        public static bool TryDowngrade(this MapTriggers mapTriggers, GamePatch targetPatch)
        {
            try
            {
                while (mapTriggers.GetMinimumPatch() > targetPatch)
                {
                    mapTriggers.DowngradeOnce();
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

        public static void DowngradeOnce(this MapTriggers mapTriggers)
        {
            if (mapTriggers.SubVersion.HasValue)
            {
                mapTriggers.SubVersion = null;
                mapTriggers.TriggerItemCounts.Clear();

                mapTriggers.OverrideIds();
                mapTriggers.UpdateNestedTriggerCategoryNames();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static GamePatch GetMinimumPatch(this MapTriggers mapTriggers)
        {
            return mapTriggers.SubVersion.HasValue
                ? GamePatch.v1_31_0
                : mapTriggers.FormatVersion == MapTriggersFormatVersion.v7
                    ? GamePatch.v1_07
                    : GamePatch.v1_00;
        }

        public static void OverrideIds(this MapTriggers mapTriggers)
        {
            var idRemapping = new Dictionary<int, int>();

            for (var i = 0; i < mapTriggers.TriggerItems.Count; i++)
            {
                var triggerItem = mapTriggers.TriggerItems[i];

                idRemapping[triggerItem.Id] = i;
                triggerItem.Id = i;

                if (idRemapping.TryGetValue(triggerItem.ParentId, out var newParentId))
                {
                    triggerItem.ParentId = newParentId;
                }
            }
        }

        public static void UpdateNestedTriggerCategoryNames(this MapTriggers mapTriggers)
        {
            var triggerCategories = mapTriggers.TriggerItems
                .Where(triggerItem => triggerItem is TriggerCategoryDefinition triggerCategory && triggerItem.Type != TriggerItemType.RootCategory)
                .ToDictionary(
                    triggerItem => triggerItem.Id,
                    triggerItem => (TriggerCategoryDefinition)triggerItem);

            var triggerCategoryPaths = new Dictionary<int, string>();

            IEnumerable<string> GetTriggerCategoryNames(TriggerCategoryDefinition triggerCategory)
            {
                if (triggerCategories.TryGetValue(triggerCategory.ParentId, out var parentTriggerCategory))
                {
                    return GetTriggerCategoryNames(parentTriggerCategory).Append(triggerCategory.Name);
                }

                return Enumerable.Repeat(triggerCategory.Name, 1);
            }

            foreach (var triggerCategory in triggerCategories)
            {
                triggerCategoryPaths.Add(triggerCategory.Key, string.Join("/", GetTriggerCategoryNames(triggerCategory.Value)));
            }

            foreach (var triggerCategory in triggerCategories)
            {
                if (triggerCategoryPaths.TryGetValue(triggerCategory.Key, out var triggerCategoryPath))
                {
                    triggerCategory.Value.Name = triggerCategoryPath;
                }
            }
        }
    }
}