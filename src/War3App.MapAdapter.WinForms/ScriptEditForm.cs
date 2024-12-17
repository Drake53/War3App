using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using War3App.Common.WinForms.Extensions;
using War3App.MapAdapter.Diagnostics;

namespace War3App.MapAdapter.WinForms
{
    internal sealed class ScriptEditForm
    {
        private readonly Form _window;
        private readonly RichTextBox _script;
        private readonly ListView _diagnosticsView;
        private readonly List<Regex> _regices;

        public ScriptEditForm(Diagnostic[] diagnostics)
        {
            _window = new Form();
            _window.WindowState = FormWindowState.Maximized;

            _script = new RichTextBox
            {
                Width = _window.Width - 500,
                Dock = DockStyle.Right,
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Both,
                ZoomFactor = 1f,
                Font = new Font("Consolas", 12f, FontStyle.Regular),
                DetectUrls = false,
                WordWrap = false,
            };

            _diagnosticsView = new ListView
            {
                Height = _window.Height - 150,
                Dock = DockStyle.Top,
                FullRowSelect = true,
                MultiSelect = false,
                HeaderStyle = ColumnHeaderStyle.Clickable,
            };

            _diagnosticsView.View = View.Details;
            _diagnosticsView.Columns.AddRange(new[]
            {
                new ColumnHeader { Text = "Diagnostic", Width = 300 },
                new ColumnHeader { Text = "Occurences", Width = 100 },
            });

            _diagnosticsView.ItemActivate += (s, e) =>
            {
                if (_diagnosticsView.SelectedIndices.Count == 1)
                {
                    var selectedIndex = _diagnosticsView.SelectedIndices[0];

                    GoToNextRegexMatch(selectedIndex);
                }
            };

            _regices = new List<Regex>();

            foreach (var diagnostic in diagnostics)
            {
                if (diagnostic.Regex is not null)
                {
                    var split = diagnostic.Message.Split('(', 2, StringSplitOptions.TrimEntries);
                    var message = split[0];
                    var occurences = split.Length == 2 ? split[1][..split[1].IndexOf('x')] : "1";

                    _diagnosticsView.Items.Add(new ListViewItem(new[] { message, occurences }));
                    _regices.Add(diagnostic.Regex);
                }
            }

            _window.Load += (s, e) =>
            {
                _script.AutoWordSelection = false;
            };

            _window.Layout += (s, e) =>
            {
                _script.Width = Math.Max(100, _window.Width - 500);
                _diagnosticsView.Height = Math.Max(100, _window.Height - 150);
            };

            var okButton = new Button { Text = "OK", };
            okButton.Size = okButton.PreferredSize;
            okButton.Click += (s, e) =>
            {
                _window.DialogResult = DialogResult.OK;
                _window.Close();
            };

            var cancelButton = new Button { Text = "Cancel", };
            cancelButton.Size = cancelButton.PreferredSize;
            cancelButton.Click += (s, e) =>
            {
                _window.DialogResult = DialogResult.Cancel;
                _window.Close();
            };

            var searchBox = new TextBox
            {
                AcceptsReturn = false,
                Width = 200,
            };

            var searchButton = new Button{ Text = "Find Next", };
            searchButton.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(searchBox.Text))
                {
                    Regex searchRegex;
                    try
                    {
                        searchRegex = new Regex(searchBox.Text);
                    }
                    catch (RegexParseException)
                    {
                        return;
                    }

                    GoToNextRegexMatch(searchRegex);
                }
            };

            var buttonsFlowLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
            };

            buttonsFlowLayout.AddControls(okButton, cancelButton, searchBox, searchButton);
            _window.AddControls(buttonsFlowLayout, _diagnosticsView, _script);
        }

        public string Text
        {
            get => _script.Text;
            set => _script.Text = value;
        }

        public DialogResult Show()
        {
            return _window.ShowDialog();
        }

        private void GoToNextRegexMatch(int selectedIndex)
        {
            var regex = _regices[selectedIndex];
            var matchCount = GoToNextRegexMatch(regex);

            _diagnosticsView.Items[selectedIndex].SubItems[1].Text = matchCount.ToString();
        }

        private int GoToNextRegexMatch(Regex regex)
        {
            var matches = regex.Matches(_script.Text);
            if (matches.Count > 0)
            {
                var nextMatch = matches.FirstOrDefault(match => match.Index > _script.SelectionStart + _script.SelectionLength) ?? matches.First();
                _script.Select(nextMatch.Index, nextMatch.Length);
                _script.ScrollToCaret();
                _script.Focus();
            }

            return matches.Count;
        }
    }
}