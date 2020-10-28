using System;
using System.IO;
using System.Text;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Script;
using War3Net.Common.Extensions;

namespace War3App.MapAdapter.Script
{
    public sealed class MapTriggersAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Triggers";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), MapTriggers.FileName.GetFileExtension(), StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            if (stream.Length < 4)
            {
                return false;
            }

            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            int header;
            try
            {
                header = reader.ReadInt32();
                stream.Position = 0;
            }
            catch (ArgumentException)
            {
                stream.Position = 0;
                return false;
            }

            return header == "WTG!".FromRawcode();
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var mapTriggers = MapTriggers.Parse(stream);
                if (mapTriggers.GetMinimumPatch() <= targetPatch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (mapTriggers.TryDowngrade(targetPatch))
                    {
                        var newMapTriggersFileStream = new MemoryStream();
                        mapTriggers.SerializeTo(newMapTriggersFileStream, true);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = newMapTriggersFileStream,
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