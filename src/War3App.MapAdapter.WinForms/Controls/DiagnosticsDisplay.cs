using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using War3App.Common.WinForms.Extensions;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.WinForms.Extensions;

namespace War3App.MapAdapter.WinForms.Controls
{
    [DesignerCategory("")]
    public class DiagnosticsDisplay : RichTextBox
    {
        public DiagnosticsDisplay()
        {
            Dock = DockStyle.Fill;
            Multiline = true;
            ReadOnly = true;
            ScrollBars = RichTextBoxScrollBars.Both;
            WordWrap = false;
        }

        public void Update(ListView.SelectedListViewItemCollection selectedItems)
        {
            Update(selectedItems.Cast<ListViewItem>().Select(item => item.GetMapFile()));
        }

        public void Update(IEnumerable<MapFile> mapFiles)
        {
            var enumerator = mapFiles.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                Text = PlaceholderText.Diagnostics;
                return;
            }

            Text = string.Empty;
            WriteDiagnostics(enumerator.Current);

            while (enumerator.MoveNext())
            {
                this.WriteLine();
                this.WriteLine();
                WriteDiagnostics(enumerator.Current);
            }
        }

        private void WriteDiagnostics(MapFile mapFile)
        {
            this.Write($"// {mapFile.CurrentFileName}", Color.Green);

            var diagnostics = mapFile.AdaptResult?.Diagnostics;

            if (diagnostics is null || diagnostics.Length == 0)
            {
                this.WriteLine();
                this.Write(DiagnosticText.None, Color.Gray);
            }
            else
            {
                WriteDiagnostics(diagnostics, DiagnosticSeverity.Error, Color.Red, DiagnosticText.Error);
                WriteDiagnostics(diagnostics, DiagnosticSeverity.Warning, Color.Orange, DiagnosticText.Warning);
                WriteDiagnostics(diagnostics, DiagnosticSeverity.Info, Color.Blue, DiagnosticText.Info);
            }
        }

        private void WriteDiagnostics(Diagnostic[] diagnostics, DiagnosticSeverity severity, Color color, string prefix)
        {
            foreach (var grouping in diagnostics.Where(d => d.Descriptor.Severity == severity).Select(d => d.Message).GroupBy(m => m, StringComparer.Ordinal))
            {
                this.WriteLine();
                this.Write(prefix, color);

                var count = grouping.Count();
                AppendText(count > 1 ? $"{grouping.Key} ({count}x)" : grouping.Key);
            }
        }
    }
}