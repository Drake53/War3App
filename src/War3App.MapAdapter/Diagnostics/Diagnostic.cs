using System.Text.RegularExpressions;

namespace War3App.MapAdapter.Diagnostics
{
    public class Diagnostic
    {
        public DiagnosticDescriptor Descriptor { get; set; }

        public Regex? Regex { get; set; }

        public string? FileName { get; set; }

        public string Message { get; set; }
    }
}