using System;
using System.IO;
using System.Text;

using War3Net.Build.Common;
using War3Net.Build.Extensions;
using War3Net.Build.Script;

namespace War3App.MapAdapter.Script
{
    public static class MapCustomTextTriggersExtensions
    {
        public static MapFileStatus Adapt(this MapCustomTextTriggers mapCustomTextTriggers, AdaptFileContext context)
        {
            if (mapCustomTextTriggers.GetMinimumPatch() <= context.TargetPatch.Patch)
            {
                return MapFileStatus.Compatible;
            }

            MapTriggers? mapTriggers = null;
            if (context.Archive.TryOpenFile(MapTriggers.FileName, out var mapTriggersStream))
            {
                try
                {
                    using var mapTriggersReader = new BinaryReader(mapTriggersStream, Encoding.UTF8, true);
                    mapTriggers = mapTriggersReader.ReadMapTriggers();
                }
                catch
                {
                }
            }

            return mapCustomTextTriggers.TryDowngrade(mapTriggers, context.TargetPatch.Patch)
                ? MapFileStatus.Adapted
                : MapFileStatus.Incompatible;
        }

        public static bool TryDowngrade(this MapCustomTextTriggers mapCustomTextTriggers, MapTriggers? mapTriggers, GamePatch targetPatch)
        {
            try
            {
                while (mapCustomTextTriggers.GetMinimumPatch() > targetPatch)
                {
                    mapCustomTextTriggers.DowngradeOnce(mapTriggers);
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

        public static void DowngradeOnce(this MapCustomTextTriggers mapCustomTextTriggers, MapTriggers? mapTriggers)
        {
            if (mapCustomTextTriggers.SubVersion.HasValue)
            {
                mapCustomTextTriggers.SubVersion = null;

                if (mapTriggers is null)
                {
                    return;
                }

                var index = 0;

                for (var i = 0; i < mapTriggers.TriggerItems.Count; i++)
                {
                    if (mapTriggers.TriggerItems[i] is TriggerDefinition triggerDefinition)
                    {
                        if (triggerDefinition.Type == TriggerItemType.Comment)
                        {
                            mapCustomTextTriggers.CustomTextTriggers.Insert(index, new CustomTextTrigger { Code = string.Empty });
                        }

                        index++;
                    }
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static GamePatch GetMinimumPatch(this MapCustomTextTriggers mapCustomTextTriggers)
        {
            return mapCustomTextTriggers.SubVersion.HasValue
                ? GamePatch.v1_31_0
                : mapCustomTextTriggers.FormatVersion == MapCustomTextTriggersFormatVersion.v1
                    ? GamePatch.v1_07
                    : GamePatch.v1_00;
        }
    }
}