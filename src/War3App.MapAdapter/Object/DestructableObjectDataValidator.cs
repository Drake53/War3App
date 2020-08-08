using System.IO;
using System.Linq;

using War3Net.Build.Common;

namespace War3App.MapAdapter.Object
{
    public static class DestructableObjectDataValidator
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
                DestructableObjectDataProvider.GetRawcodes(targetPatch).ToHashSet(),
                DestructableObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet(),
                false);
        }

        public static bool Downgrade(Stream input, Stream output, GamePatch targetPatch)
        {
            return ObjectDataValidator.Downgrade(
                input,
                output,
                DestructableObjectDataProvider.GetRawcodes(targetPatch).ToHashSet(),
                DestructableObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet(),
                false);
        }

        public static AdaptResult Adapt(Stream input, GamePatch targetPatch)
        {
            return ObjectDataValidator.Adapt(
                input,
                DestructableObjectDataProvider.GetRawcodes(targetPatch).ToHashSet(),
                DestructableObjectDataProvider.GetPropertyRawcodes(targetPatch).ToHashSet(),
                false);
        }
    }
}