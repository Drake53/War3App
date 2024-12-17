namespace War3App.MapAdapter.Diagnostics
{
    public static partial class DiagnosticRule
    {
        public static class MapScript
        {
            public static readonly DiagnosticDescriptor UnsupportedIdentifier = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "Found incompatible identifier: '{0}' ({1}x).",
            };

            public static readonly DiagnosticDescriptor UnsupportedAudioFormat = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Found incompatible audio format: '{0}' ({1}x).",
            };

            public static readonly DiagnosticDescriptor UnsupportedFrameName = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Found incompatible frame name: '{0}' ({1}x).",
            };
        }

        public static class MapTriggers
        {
            public static readonly DiagnosticDescriptor NotSupported = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "The file format version {0} is not supported before {1}.",
            };

            public static readonly DiagnosticDescriptor UnsupportedTriggerFunction = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "The trigger {0} '{1}' is not supported.",
            };

            public static readonly DiagnosticDescriptor UnsupportedVariableType = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Variable '{0}' is of unsupported type '{1}'.",
            };

            public static readonly DiagnosticDescriptor VariableTypeChanged = new()
            {
                Severity = DiagnosticSeverity.Info,
                Description = "Changed type of variable '{0}' from '{1}' to '{2}'.",
            };
        }
    }
}