using System;
using System.IO;

using War3Net.Build.Common;
using War3Net.Build.Extensions;

namespace War3App.MapAdapter.Environment
{
    public sealed class MapShadowMapAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Shadow Map";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                using var reader = new BinaryReader(stream);
                var shadowMap = reader.ReadMapShadowMap();
                return new AdaptResult
                {
                    Status = MapFileStatus.Compatible,
                };
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