﻿using System;
using System.IO;
using System.Text;

using War3Net.Build.Common;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Environment
{
    public sealed class MapCamerasAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Cameras";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, TargetPatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var mapCameras = reader.ReadMapCameras();
                if (mapCameras.GetMinimumPatch() <= targetPatch.Patch)
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.Compatible,
                    };
                }

                try
                {
                    if (mapCameras.TryDowngrade(targetPatch.Patch))
                    {
                        var newMapCamerasFileStream = new MemoryStream();
                        using var writer = new BinaryWriter(newMapCamerasFileStream, new UTF8Encoding(false, true), true);
                        writer.Write(mapCameras);

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