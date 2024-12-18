namespace War3App.MapAdapter
{
    public enum MapFileStatus
    {
        /// <summary>
        /// File will not be saved in adapted archive.
        /// </summary>
        Removed,

        /// <summary>
        /// No adapter exists for the file.
        /// </summary>
        Unknown,

        /// <summary>
        /// The file is compatible with the target patch.
        /// </summary>
        Compatible,

        /// <summary>
        /// The adapter is unable to verify if the file is compatible.
        /// </summary>
        Inconclusive,

        /// <summary>
        /// An adapter exists for the file, but it has not been run yet.
        /// </summary>
        Pending,

        /// <summary>
        /// The adapter found incompatibilities that cannot be (automatically) resolved.
        /// </summary>
        Incompatible,

        /// <summary>
        /// The mpq file can not be read.
        /// </summary>
        Locked,

        /// <summary>
        /// An exception occured when adapting the file.
        /// </summary>
        Error,
    }
}