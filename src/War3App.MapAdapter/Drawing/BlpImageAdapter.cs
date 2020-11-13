using System;
using System.IO;
using System.Text;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Drawing
{
    public sealed class BlpImageAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "BLP Image";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), ".blp", StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            if (stream.Length < 4)
            {
                return false;
            }

            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            char[] chars;
            try
            {
                chars = reader.ReadChars(4);
                stream.Position = 0;
            }
            catch (ArgumentException)
            {
                stream.Position = 0;
                return false;
            }

            return new string(chars) == "BLP1";
        }

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