using System.Text.RegularExpressions;

namespace War3App.MapAdapter
{
    public sealed class RegexDiagnostic
    {
        public string DisplayText { get; set; }

        public int Matches { get; set; }

        public Regex Regex { get; set; }
    }
}