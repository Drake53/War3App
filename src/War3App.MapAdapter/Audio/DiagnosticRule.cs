namespace War3App.MapAdapter.Diagnostics
{
    public static partial class DiagnosticRule
    {
        public static class MapSounds
        {
            public static readonly DiagnosticDescriptor NotSupported = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "The file format version {0} is not supported before {1}.",
            };
        }

        public static class Flac
        {
            public static readonly DiagnosticDescriptor NotSupported = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Audio files in the .flac file format are not supported before v1.32.",
            };
        }

        public static class Ogg
        {
            public static readonly DiagnosticDescriptor NotSupported = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Audio files in the .ogg file format are only supported in v1.30.x.",
            };
        }
    }
}