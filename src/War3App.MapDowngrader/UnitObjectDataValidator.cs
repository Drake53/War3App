using System.IO;
using System.Linq;

using War3Net.Build.Common;

namespace War3App.MapDowngrader
{
    public static class UnitObjectDataValidator
    {
        public static bool TryValidate(Stream stream, GamePatch targetPatch)
        {
            try
            {
                return Validate(stream, targetPatch);
            }
            catch
            {
                return false;
            }
        }

        public static bool Validate(Stream stream, GamePatch targetPatch)
        {
            return ObjectDataValidator.Validate(
                stream,
                UnitObjectDataProvider.GetRawcodes(targetPatch).ToHashSet(),
                UnitObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet(),
                false);
        }

        public static void Downgrade(Stream input, Stream output, GamePatch targetPatch)
        {
            ObjectDataValidator.Downgrade(
                input,
                output,
                UnitObjectDataProvider.GetRawcodes(targetPatch).ToHashSet(),
                UnitObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet(),
                false);
        }
    }
}