using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Script;

namespace War3App.MapAdapter.Script
{
    public static class MapTriggersExtensions
    {
        public static bool Adapt(this MapTriggers mapTriggers, AdaptFileContext context, out MapFileStatus status)
        {
            var triggerDataStream = context.TargetPatch.OpenGameDataFile(PathConstants.TriggerDataPath);
            if (triggerDataStream is null)
            {
                context.ReportDiagnostic(DiagnosticRule.General.ConfigFileNotFound, PathConstants.TriggerDataPath);
                status = MapFileStatus.Inconclusive;
                return false;
            }

            var triggerDataText = triggerDataStream.ReadAllText();
            var triggerDataReader = new StringReader(triggerDataText);
            var triggerData = triggerDataReader.ReadTriggerData();

            IEnumerable<TriggerFunction> GetUnknownFunctions(TriggerFunction function)
            {
                var isKnownFunction = function.Type switch
                {
                    TriggerFunctionType.Event => triggerData.TriggerEvents.ContainsKey(function.Name),
                    TriggerFunctionType.Condition => triggerData.TriggerConditions.ContainsKey(function.Name),
                    TriggerFunctionType.Action => triggerData.TriggerActions.ContainsKey(function.Name),
                    TriggerFunctionType.Call => triggerData.TriggerCalls.ContainsKey(function.Name),

                    _ => false,
                };

                if (!isKnownFunction)
                {
                    yield return function;
                }

                foreach (var childFunction in function.ChildFunctions)
                {
                    foreach (var unknownFunction in GetUnknownFunctions(childFunction))
                    {
                        yield return unknownFunction;
                    }
                }
            }

            var isIncompatible = false;

            foreach (var triggerItem in mapTriggers.TriggerItems)
            {
                if (triggerItem is TriggerDefinition triggerDefinition)
                {
                    foreach (var function in triggerDefinition.Functions)
                    {
                        foreach (var unknownFunction in GetUnknownFunctions(function))
                        {
                            context.ReportDiagnostic(DiagnosticRule.MapTriggers.UnsupportedTriggerFunction, unknownFunction.Type, unknownFunction.Name);
                            isIncompatible = true;
                        }
                    }
                }
            }

            if (isIncompatible)
            {
                status = MapFileStatus.Incompatible;
                return false;
            }

            var supportedVariableTypes = triggerData.TriggerTypes.Values
                .Where(triggerType => triggerType.UsableAsGlobalVariable)
                .Select(triggerType => triggerType.TypeName)
                .ToHashSet(StringComparer.Ordinal);

            var unsupportedVariableBaseTypes = TriggerData.Default.TriggerTypes.Values
                .Where(triggerType => !supportedVariableTypes.Contains(triggerType.TypeName))
                .Where(triggerType => !string.IsNullOrEmpty(triggerType.BaseType))
                .ToDictionary(
                    triggerType => triggerType.TypeName,
                    triggerType => triggerType.BaseType!,
                    StringComparer.Ordinal);

            var isAdapted = false;

            foreach (var variableDefinition in mapTriggers.Variables)
            {
                if (!supportedVariableTypes.Contains(variableDefinition.Type))
                {
                    if (unsupportedVariableBaseTypes.TryGetValue(variableDefinition.Type, out var baseType) &&
                        supportedVariableTypes.Contains(baseType))
                    {
                        context.ReportDiagnostic(DiagnosticRule.MapTriggers.VariableTypeChanged, variableDefinition.Name, variableDefinition.Type, baseType);
                        variableDefinition.Type = baseType;
                        isAdapted = true;
                    }
                    else
                    {
                        context.ReportDiagnostic(DiagnosticRule.MapTriggers.UnsupportedVariableType, variableDefinition.Name, variableDefinition.Type);
                        isIncompatible = true;
                    }
                }
            }

            if (isIncompatible)
            {
                status = MapFileStatus.Incompatible;
                return false;
            }

            var mustDowngrade = mapTriggers.GetMinimumPatch() > context.TargetPatch.Patch;
            if (!mustDowngrade && !isAdapted)
            {
                status = MapFileStatus.Compatible;
                return false;
            }

            if (mustDowngrade && !mapTriggers.TryDowngrade(context.TargetPatch.Patch))
            {
                status = MapFileStatus.Incompatible;
                return false;
            }

            status = MapFileStatus.Compatible;
            return true;
        }

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