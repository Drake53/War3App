using System;
using System.IO;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Script;

namespace War3App.MapAdapter.Script
{
    public sealed class MapCustomTextTriggersAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Custom Text Triggers";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), MapCustomTextTriggers.FileName.GetFileExtension(), StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var mapCustomTextTriggers = MapCustomTextTriggers.Parse(stream);
                if (mapCustomTextTriggers.GetMinimumPatch() <= targetPatch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (mapCustomTextTriggers.TryDowngrade(targetPatch))
                    {
                        var newMapCustomTextTriggersFileStream = new MemoryStream();
                        mapCustomTextTriggers.SerializeTo(newMapCustomTextTriggersFileStream, true);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = newMapCustomTextTriggersFileStream,
                        };
                    }
                    else
                    {
                        return new AdaptResult
                        {
                            Status = MapFileStatus.Unadaptable,
                        };
                    }
                }
                catch
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.AdapterError,
                    };
                }
            }
            catch (Exception e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                    Diagnostics = new[] { e.Message },
                };
            }
        }
    }
}