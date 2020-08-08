using System.IO;
using System.Linq;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Object
{
    public static class UpgradeObjectDataValidator
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
                UpgradeObjectDataProvider.GetRawcodes(targetPatch).ToHashSet(),
                UpgradeObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet(),
                true);
        }

        public static bool Downgrade(Stream input, Stream output, GamePatch targetPatch)
        {
            return ObjectDataValidator.Downgrade(
                input,
                output,
                UpgradeObjectDataProvider.GetRawcodes(targetPatch).ToHashSet(),
                UpgradeObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet(),
                true);
        }

        public static AdaptResult Adapt(Stream input, GamePatch targetPatch)
        {
            return ObjectDataValidator.Adapt(
                input,
                UpgradeObjectDataProvider.GetRawcodes(targetPatch).ToHashSet(),
                UpgradeObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet(),
                true);
        }
    }
}