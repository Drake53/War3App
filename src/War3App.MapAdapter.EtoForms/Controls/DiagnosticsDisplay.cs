using System;
using System.Linq;

using Eto.Drawing;
using Eto.Forms;

using War3App.Common.EtoForms.Extensions;
using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.EtoForms.Models;

namespace War3App.MapAdapter.EtoForms.Controls
{
    public class DiagnosticsDisplay : RichTextArea
    {
        public DiagnosticsDisplay()
        {
            Font = new Font(FontFamilies.Monospace, 10);
            ReadOnly = true;
            Wrap = false;
        }

        public void Update(FileListItem[] selectedItems)
        {
            if (selectedItems.Length == 0)
            {
                Text = PlaceholderText.Diagnostics;
                return;
            }

            Text = string.Empty;
            WriteDiagnostics(selectedItems[0].MapFile);

            for (var i = 1; i < selectedItems.Length; i++)
            {
                this.AppendLine();
                this.AppendLine();
                WriteDiagnostics(selectedItems[i].MapFile);
            }
        }

        private void WriteDiagnostics(MapFile mapFile)
        {
            this.Append($"// {mapFile.CurrentFileName}", Colors.Green);

            var diagnostics = mapFile.AdaptResult?.Diagnostics;

            if (diagnostics is null || diagnostics.Length == 0)
            {
                this.AppendLine();
                this.Append(DiagnosticText.None, Colors.Gray);
            }
            else
            {
                WriteDiagnostics(diagnostics, DiagnosticSeverity.Error, Colors.Red, DiagnosticText.Error);
                WriteDiagnostics(diagnostics, DiagnosticSeverity.Warning, Colors.Orange, DiagnosticText.Warning);
                WriteDiagnostics(diagnostics, DiagnosticSeverity.Info, Colors.Blue, DiagnosticText.Info);
            }
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