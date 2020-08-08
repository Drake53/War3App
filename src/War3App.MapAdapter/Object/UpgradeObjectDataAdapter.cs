using System;
using System.IO;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Build.Object;

namespace War3App.MapAdapter.Object
{
    public sealed class UpgradeObjectDataAdapter : IMapFileAdapter
    {
        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), MapUpgradeObjectData.FileName.GetFileExtension(), StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            return false;
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch)
        {
            try
            {
                // TODO: use MapUpgradeObjectData as input
                // var mapUpgradeObjectData = MapUpgradeObjectData.Parse(stream);

                try
                {
                    return UpgradeObjectDataValidator.Adapt(stream, targetPatch);
                }
                catch
                {
                    return new AdaptResult
                    {
                        Status = MapFileStatus.AdapterError,
                    };
                }
            }
            catch
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                };
            }
        }
    }
}