namespace War3App.MapAdapter
{
    public enum MapFileStatus
    {
        /// <summary>
        /// No adapter exists for the file.
        /// </summary>
        Unknown,

        /// <summary>
        /// The file is compatible without requiring changes.
        /// </summary>
        Compatible,

        /// <summary>
        /// The file has been made compatible with an adapter and/or manual user input.
        /// </summary>
        Adapted,

        /// <summary>
        /// An adapter exists for the file, but it has not been run yet.
        /// </summary>
        Pending,

        /// <summary>
        /// The adapter found incompatibilities that must be manually resolved.
        /// </summary>
        RequiresInput,

        /// <summary>
        /// The adapter found incompatibilities that cannot be resolved.
        /// </summary>
        Unadaptable,

        /// <summary>
        /// An exception occured when parsing the file.
        /// </summary>
        ParseError,

        /// <summary>
        /// An exception occured when adapting the file.
        /// </summary>
        AdapterError,
    }
}