namespace War3App.MapAdapter.Diagnostics
{
    public static partial class DiagnosticRule
    {
        public static class DdsImage
        {
            public static readonly DiagnosticDescriptor NotSupported = new()
            {
                Severity = DiagnosticSeverity.Warning,
                Description = "Image files in the .dds file format are not supported before v1.32.",
            };
        }
    }
}