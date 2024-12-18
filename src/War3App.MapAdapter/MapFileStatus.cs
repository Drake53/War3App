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
        /// The adapter found incompatibilities that cannot be (automatically) resolved.
        /// </summary>
        Incompatible,

        /// <summary>
        /// The mpq file can not be read.
        /// </summary>
        Locked,

        /// <summary>
        /// The file's adapter cannot run, because the appsettings.json configuration file is invalid.
        /// </summary>
        ConfigError,

        /// <summary>
        /// An exception occured when adapting the file.
        /// </summary>
        Error,
    }
}