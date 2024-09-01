using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using War3App.Common.WinForms.Extensions;

using War3Net.Build.Common;

namespace War3App.MapAdapter.WinForms.Forms
{
    [DesignerCategory("")]
    public sealed class ConfigureGamePathForm : Form
    {
        private readonly TextBox _gameDirectoryInput;
        private readonly Button _gameDirectoryInputBrowseButton;

        private readonly ComboBox _targetPatchesComboBox;

        private readonly Button _saveButton;

        public ConfigureGamePathForm()
        {
            Text = "Initial setup";
            Size = new Size(600, 200);
            MinimumSize = new Size(400, 200);

            _gameDirectoryInput = new TextBox
            {
                PlaceholderText = "Input path to warcraft III installation...",
                TabIndex = 2,
                Width = 360,
            };

            _gameDirectoryInput.TextChanged += OnGameDirectoryInputTextChanged;

            _gameDirectoryInputBrowseButton = new Button
            {
                Text = "Browse",
                TabIndex = 0,
            };

            _gameDirectoryInputBrowseButton.Size = _gameDirectoryInputBrowseButton.PreferredSize;
            _gameDirectoryInputBrowseButton.Click += (s, e) =>
            {
                var openDirectoryDialog = new FolderBrowserDialog
                {
                };

                var openDirectoryDialogResult = openDirectoryDialog.ShowDialog();
                if (openDirectoryDialogResult == DialogResult.OK)
                {
                    _gameDirectoryInput.Text = openDirectoryDialog.SelectedPath;
                }
            };

            var targetPatchLabel = new Label
            {
                Text = "Target patch",
                TextAlign = ContentAlignment.BottomRight,
            };

            targetPatchLabel.Size = targetPatchLabel.PreferredSize;

            _targetPatchesComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
            };

            _targetPatchesComboBox.Items.AddRange(Enum.GetValues<GamePatch>().OrderByDescending(patch => patch).Cast<object>().ToArray());

            _targetPatchesComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(_gameDirectoryInput.Text))
                {
                    var directoryInfo = new DirectoryInfo(_gameDirectoryInput.Text);
                    _saveButton.Enabled = directoryInfo.Exists && _targetPatchesComboBox.SelectedItem is not null;
                }
            };

            _targetPatchesComboBox.FormattingEnabled = true;
            _targetPatchesComboBox.Format += (s, e) =>
            {
                if (e.ListItem is GamePatch gamePatch)
                {
                    e.Value = gamePatch.ToString().Replace('_', '.');
                }
            };

            _saveButton = new Button
            {
                Text = "Save",
                Enabled = false,
                TabIndex = 1,
                Dock = DockStyle.Bottom,
            };

            _saveButton.Click += (s, e) =>
            {
                var missingFiles = GetMissingFiles(_gameDirectoryInput.Text).ToList();
                if (missingFiles.Count > 0)
                {
                    MessageBox.Show(
                        string.Join(System.Environment.NewLine, missingFiles.Prepend("Directory does not contain all files required for adapting. The following files could not be found:")),
                        "Missing files",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            };

            var inputGameDirectoryFlowLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
            };

            var targetPatchFlowLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
            };

            var flowLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
            };

            inputGameDirectoryFlowLayout.AddControls(_gameDirectoryInput, _gameDirectoryInputBrowseButton);
            targetPatchFlowLayout.AddControls(targetPatchLabel, _targetPatchesComboBox);

            inputGameDirectoryFlowLayout.Size = inputGameDirectoryFlowLayout.PreferredSize;
            targetPatchFlowLayout.Size = targetPatchFlowLayout.PreferredSize;

            flowLayout.AddControls(inputGameDirectoryFlowLayout, targetPatchFlowLayout);

            this.AddControls(flowLayout, _saveButton);
        }

        public string GameDirectory => _gameDirectoryInput.Text;

        public GamePatch GamePatch => (GamePatch)_targetPatchesComboBox.SelectedItem;

        private void OnGameDirectoryInputTextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_gameDirectoryInput.Text))
            {
                _saveButton.Enabled = false;
            }
            else
            {
                var directoryInfo = new DirectoryInfo(_gameDirectoryInput.Text);
                _saveButton.Enabled = directoryInfo.Exists && _targetPatchesComboBox.SelectedItem is not null;
            }
        }

        private static IEnumerable<string> GetMissingFiles(string directory)
        {
            return PathConstants.GetAllPaths()
                .Select(path => Path.Combine(directory, path))
                .Where(filePath => !File.Exists(filePath));
        }
    }
}