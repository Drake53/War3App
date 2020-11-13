using System;
using System.IO;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Drawing
{
    public sealed class BlpImageAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "BLP Image";

        public bool IsTextFile => false;

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                // TODO
                // var blpFile = BlpFile.Parse(stream);
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