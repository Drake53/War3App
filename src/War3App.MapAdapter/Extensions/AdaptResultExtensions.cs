namespace War3App.MapAdapter.Extensions
{
    public static class AdaptResultExtensions
    {
        public static void Merge(this AdaptResult newAdaptResult, AdaptResult oldAdaptResult)
        {
            if (oldAdaptResult.AdaptedFileStream is not null)
            {
                if (newAdaptResult.AdaptedFileStream is null)
                {
                    newAdaptResult.AdaptedFileStream = oldAdaptResult.AdaptedFileStream;
                }
                else
                {
                    oldAdaptResult.Dispose();
                }
            }

            if (oldAdaptResult.NewFileName is not null)
            {
                if (newAdaptResult.NewFileName is null)
                {
                    newAdaptResult.NewFileName = oldAdaptResult.NewFileName;
                }
            }
        }
    }
}