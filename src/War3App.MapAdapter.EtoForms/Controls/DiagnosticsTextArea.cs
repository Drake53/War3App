using System;
using System.Linq;

using Eto.Drawing;
using Eto.Forms;

using War3App.Common.EtoForms.Extensions;
using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Diagnostics;

namespace War3App.MapAdapter.EtoForms.Controls
{
    public class DiagnosticsTextArea : RichTextArea
    {
        public DiagnosticsTextArea()
        {
            Font = new Font(FontFamilies.Monospace, 10);
            ReadOnly = true;
            Wrap = false;
        }

        public bool WriteDiagnosticsFiltered(
            MapFile mapFile,
            bool isSelected,
            ref bool anyItemWritten,
            bool showError,
            bool showWarning,
            bool showInfo)
        {
            var originalCount = 0;
            var diagnostics = mapFile.AdaptResult?.Diagnostics;

            if (diagnostics is null)
            {
                diagnostics = Array.Empty<Diagnostic>();
            }
            else
            {
                originalCount = diagnostics.Length;
                diagnostics = diagnostics.Where(diagnostic
                    => (showError && diagnostic.Descriptor.Severity == DiagnosticSeverity.Error)
                    || (showWarning && diagnostic.Descriptor.Severity == DiagnosticSeverity.Warning)
                    || (showInfo && diagnostic.Descriptor.Severity == DiagnosticSeverity.Info)).ToArray();
            }

            if (diagnostics.Length == 0 && !isSelected)
            {
                return false;
            }

            if (anyItemWritten)
            {
                this.AppendLine();
                this.AppendLine();
            }

            if (!string.IsNullOrEmpty(mapFile.ArchiveName))
            {
                this.Append($"// {mapFile.ArchiveName} - {mapFile.CurrentFileName}", Colors.Green);
            }
            else
            {
                this.Append($"// {mapFile.CurrentFileName}", Colors.Green);
            }

            if (diagnostics.Length == 0)
            {
                this.AppendLine();

                if (originalCount == 0)
                {
                    this.Append(DiagnosticText.None, Colors.Gray);
                }
                else
                {
                    this.Append(DiagnosticText.NoResults, Colors.Purple);
                }
            }
            else
            {
                if (showError)
                {
                    WriteDiagnostics(diagnostics, DiagnosticSeverity.Error, Colors.Red, DiagnosticText.Error);
                }

                if (showWarning)
                {
                    WriteDiagnostics(diagnostics, DiagnosticSeverity.Warning, Colors.Orange, DiagnosticText.Warning);
                }

                if (showInfo)
                {
                    WriteDiagnostics(diagnostics, DiagnosticSeverity.Info, Colors.Blue, DiagnosticText.Info);
                }
            }

            anyItemWritten = true;
            return true;
        }

        private void WriteDiagnostics(Diagnostic[] diagnostics, DiagnosticSeverity severity, Color color, string prefix)
        {
            foreach (var grouping in diagnostics.Where(d => d.Descriptor.Severity == severity).Select(d => d.Message).GroupBy(m => m, StringComparer.Ordinal))
            {
                this.AppendLine();
                this.Append(prefix, color);

                var count = grouping.Count();
                Append(count > 1 ? $"{grouping.Key} ({count}x)" : grouping.Key);
            }
        }
    }
}