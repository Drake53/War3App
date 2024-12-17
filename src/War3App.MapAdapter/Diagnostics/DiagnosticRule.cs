namespace War3App.MapAdapter.Diagnostics
{
    public static partial class DiagnosticRule
    {
        public static class General
        {
            public static readonly DiagnosticDescriptor ConfigFileNotFound = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Required file '{0}' could not be found in the game data files.",
            };

            public static readonly DiagnosticDescriptor ParseError = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Could not parse the file: {0}",
            };

            public static readonly DiagnosticDescriptor AdapterError = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "The adapter threw an exception: {0}",
            };

            public static readonly DiagnosticDescriptor SerializerError = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Could not serialize the adapted file: {0}",
            };
        }
    }
}