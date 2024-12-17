namespace War3App.MapAdapter.Diagnostics
{
    public enum DiagnosticSeverity
    {
        /// <summary>Message about a succesful adaptation or some other insignificant message, does not indicate an issue with compatibility.</summary>
        Info,

        /// <summary>Adaptation which may cause issues, or an incompatibility which could not be fixed and may cause unexpected behaviour, but does not prevent loading the map.</summary>
        Warning,

        /// <summary>Cannot adapt, either due to not being compatible, or an exception was thrown, might indicate that the game and/or world editor will be unable to load the map.</summary>
        Error,
    }
}