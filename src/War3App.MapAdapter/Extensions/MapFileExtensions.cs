namespace War3App.MapAdapter.Extensions
{
    public static class MapFileExtensions
    {
        public static bool CanUndoChanges(this MapFile mapFile)
        {
            return mapFile.Status == MapFileStatus.Removed
                || mapFile.AdaptResult.CanUndoChanges();
        }
    }
}