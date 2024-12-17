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
        /// The file has been modified by the user manually, but the adapter has not verified the changes yet.
        /// </summary>
        Modified,

        /// <summary>
        /// An adapter exists for the file, but it has not been run yet.
        /// </summary>
        Pending,

        /// <summary>
        /// The adapter found incompatibilities that must be manually resolved.
        /// </summary>
        Incompatible,

        /// <summary>
        /// The adapter found incompatibilities that cannot be resolved.
        /// </summary>
        Unadaptable,

        /// <summary>
        /// The mpq file can not be read.
        /// </summary>
        Locked,

        /// <summary>
        /// An exception occured when parsing the file.
        /// </summary>
        ParseError,

        /// <summary>
        /// The file's adapter cannot run, because the appsettings.json configuration file is invalid.
        /// </summary>
        ConfigError,

        /// <summary>
        /// An exception occured when adapting the file.
        /// </summary>
        AdapterError,

        /// <summary>
        /// An exception occured when serializing the adapted file.
        /// </summary>
        SerializeError,
    }
}