using System;
using System.Linq;

using Eto.Drawing;
using Eto.Forms;

using War3App.Common.EtoForms.Extensions;
using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Diagnostics;
using War3App.MapAdapter.EtoForms.Enums;
using War3App.MapAdapter.EtoForms.Helpers;
using War3App.MapAdapter.EtoForms.Models;

namespace War3App.MapAdapter.EtoForms.Controls
{
    public class DiagnosticsDisplay : Panel
    {
        private readonly CheckBox _showInfo;
        private readonly CheckBox _showWarning;
        private readonly CheckBox _showError;
        private readonly DiagnosticsTextArea _textArea;
        private SelectedFileTreeItem[] _selectedItems = Array.Empty<SelectedFileTreeItem>();

        public DiagnosticsDisplay()
        {
            _showInfo = new CheckBox { Text = CheckBoxText.Info, Checked = true };
            _showWarning = new CheckBox { Text = CheckBoxText.Warning, Checked = true };
            _showError = new CheckBox { Text = CheckBoxText.Error, Checked = true };
            _textArea = new DiagnosticsTextArea();

            _showInfo.CheckedChanged += OnFilterChanged;
            _showWarning.CheckedChanged += OnFilterChanged;
            _showError.CheckedChanged += OnFilterChanged;

            var filterPanel = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Items = { _showError, _showWarning, _showInfo },
            };

            Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Items =
                {
                    new StackLayoutItem(filterPanel, HorizontalAlignment.Left),
                    new StackLayoutItem(_textArea, HorizontalAlignment.Stretch, true),
                },
            };
        }

        public void Clear()
        {
            _textArea.Text = string.Empty;
        }

        public void Update(FileTreeItem[] selectedItems)
        {
            _selectedItems = FileTreeHelper.GetSelectedFileTreeItems(selectedItems).ToArray();
            UpdateDisplay();
        }

        private void OnFilterChanged(object? sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_selectedItems.Length == 0)
            {
                _textArea.Text = PlaceholderText.Diagnostics;
                return;
            }

            _textArea.Text = string.Empty;

            var anyItemWritten = false;
            var showError = _showError.Checked ?? false;
            var showWarning = _showWarning.Checked ?? false;
            var showInfo = _showInfo.Checked ?? false;

            for (var i = 0; i < _selectedItems.Length; i++)
            {
                var selectedItem = _selectedItems[i];

                if (selectedItem.SelectionType != SelectionType.Parent)
                {
                    _textArea.WriteDiagnosticsFiltered(
                        selectedItem.Item.MapFile,
                        selectedItem.SelectionType != SelectionType.IndirectLeaf,
                        ref anyItemWritten,
                        showError,
                        showWarning,
                        showInfo);

                    continue;
                }

                var showParent = true;
                var hasDiagnostics = false;
                foreach (var childItem in _selectedItems)
                {
                    if (childItem.Item.Parent != selectedItem.Item)
                    {
                        continue;
                    }

                    if (childItem.SelectionType != SelectionType.IndirectLeaf)
                    {
                        showParent = false;
                        break;
                    }

                    var diagnostics = childItem.Item.MapFile.AdaptResult?.Diagnostics;
                    if (diagnostics is null)
                    {
                        continue;
                    }

                    hasDiagnostics = hasDiagnostics || diagnostics.Length > 0;

                    if (HasVisibleDiagnostics(childItem.Item.MapFile, showError, showWarning, showInfo))
                    {
                        showParent = false;
                        break;
                    }
                }

                if (!showParent)
                {
                    continue;
                }

                if (anyItemWritten)
                {
                    _textArea.AppendLine();
                    _textArea.AppendLine();
                }

                _textArea.AppendLine($"// {selectedItem.Item.MapFile.CurrentFileName}", Colors.Green);

                if (hasDiagnostics)
                {
                    _textArea.Append(DiagnosticText.NoResults, Colors.Purple);
                }
                else
                {
                    _textArea.Append(DiagnosticText.None, Colors.Gray);
                }

                anyItemWritten = true;
            }
        }

        private static bool HasVisibleDiagnostics(MapFile mapFile, bool showError, bool showWarning, bool showInfo)
        {
            var diagnostics = mapFile.AdaptResult?.Diagnostics;
            if (diagnostics is null)
            {
                return false;
            }

            return diagnostics.Any(diagnostic
                => (showError && diagnostic.Descriptor.Severity == DiagnosticSeverity.Error)
                || (showWarning && diagnostic.Descriptor.Severity == DiagnosticSeverity.Warning)
                || (showInfo && diagnostic.Descriptor.Severity == DiagnosticSeverity.Info));
        }
    }
}