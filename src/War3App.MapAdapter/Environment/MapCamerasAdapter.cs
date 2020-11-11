using System;
using System.IO;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Environment;

namespace War3App.MapAdapter.Environment
{
    public sealed class MapCamerasAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Cameras";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), MapCameras.FileName.GetFileExtension(), StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                var mapCameras = MapCameras.Parse(stream);
                if (mapCameras.GetMinimumPatch() <= targetPatch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (mapCameras.TryDowngrade(targetPatch))
                    {
                        var newMapCamerasFileStream = new MemoryStream();
                        mapCameras.SerializeTo(newMapCamerasFileStream, true);

                        return new AdaptResult
                        {
                            Status = MapFileStatus.Adapted,
                            AdaptedFileStream = newMapCamerasFileStream,
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
            catch (NotSupportedException)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.Unadaptable,
                };
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