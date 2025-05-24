namespace War3App.MapAdapter
{
    public enum AdaptationLevel
    {
        /// <summary>No changes will be made, the adapter will only check if the file is compatible or not.</summary>
        None,

        /// <summary>The adapter will only make non-breaking changes.</summary>
        /// <remarks>This is the default adaptation level.</remarks>
        Normal,

        /// <summary>The adapter may make breaking changes in order to make the file compatible.</summary>
        Aggressive,
    }
}