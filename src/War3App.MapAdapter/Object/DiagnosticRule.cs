namespace War3App.MapAdapter.Diagnostics
{
    public static partial class DiagnosticRule
    {
        public static class ObjectData
        {
            public static readonly DiagnosticDescriptor NotSupported = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "The file format version {0} is not supported before {1}.",
            };

            public static readonly DiagnosticDescriptor MissingFileName = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "This adapter requires the filename to be known, because {0} and {1} are handled differently.",
            };

            public static readonly DiagnosticDescriptor MergedSkinData = new()
            {
                Severity = DiagnosticSeverity.Info,
                Description = "The object data from {0} has been merged into this file.",
            };

            public static readonly DiagnosticDescriptor RemovedSkinData = new()
            {
                Severity = DiagnosticSeverity.Info,
                Description = "The object data from this file will be merged into {0}.",
            };

            public static readonly DiagnosticDescriptor RenamedSkinData = new()
            {
                Severity = DiagnosticSeverity.Info,
                Description = "The file has been renamed to {0}, because skin object data files are not supported.",
            };

            public static readonly DiagnosticDescriptor UnknownBaseId = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Deleted unknown base {0}: '{1}'.",
            };

            public static readonly DiagnosticDescriptor UnknownBaseIdNew = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Deleted {0} '{1}' with unknown base ID: '{2}'.",
            };

            public static readonly DiagnosticDescriptor ConflictingId = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Deleted conflicting {0}: '{1}'.",
            };

            public static readonly DiagnosticDescriptor UnknownProperty = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Deleted unknown property: '{0}'.",
            };
        }
    }
}