namespace War3App.MapAdapter.Diagnostics
{
    public static partial class DiagnosticRule
    {
        public static class CampaignInfo
        {
            public static readonly DiagnosticDescriptor NotSupported = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "The file format version {0} is not supported before {1}.",
            };
        }

        public static class MapInfo
        {
            public static readonly DiagnosticDescriptor NotSupported = new()
            {
                Severity = DiagnosticSeverity.Error,
                Description = "The file format version {0} is not supported before {1}.",
            };
        }
    }
}