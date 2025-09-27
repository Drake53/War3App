using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using War3App.Common.WinForms.Extensions;
using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Extensions;

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
            Text = TitleText.Setup;
            Size = new Size(600, 200);
            MinimumSize = new Size(400, 200);

            _gameDirectoryInput = new TextBox
            {
                PlaceholderText = PlaceholderText.GameDirectory,
                TabIndex = 2,
                Width = 360,
            };

            _gameDirectoryInput.TextChanged += OnSettingChanged;

            _gameDirectoryInputBrowseButton = new Button
            {
                Text = ButtonText.Browse,
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

            _saveButton = new Button
            {
                Text = ButtonText.Save,
                Enabled = false,
                TabIndex = 1,
                Dock = DockStyle.Bottom,
            };

            _saveButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_gameDirectoryInput.Text))
                {
                    var dialogResult = MessageBox.Show(
                        MessageText.ContinueWithoutGameFiles,
                        TitleText.ContinueWithoutGameFiles,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (dialogResult != DialogResult.Yes)
                    {
                        return;
                    }
                }
                else
                {
                    var missingFiles = GetMissingFiles(_gameDirectoryInput.Text).ToList();
                    if (missingFiles.Count > 0)
                    {
                        MessageBox.Show(
                            string.Join(System.Environment.NewLine, missingFiles.Prepend(MessageText.MissingFiles)),
                            TitleText.MissingFiles,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            };

            var targetPatchLabel = new Label
            {
                Text = LabelText.TargetPatch,
                TextAlign = ContentAlignment.BottomRight,
            };

            targetPatchLabel.Size = targetPatchLabel.PreferredSize;

            _targetPatchesComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
            };

            _targetPatchesComboBox.Items.AddRange(Enum.GetValues<GamePatch>().OrderByDescending(patch => patch).Cast<object>().ToArray());

            _targetPatchesComboBox.SelectedIndexChanged += OnSettingChanged;

            _targetPatchesComboBox.FormattingEnabled = true;
            _targetPatchesComboBox.Format += (s, e) =>
            {
                if (e.ListItem is GamePatch gamePatch)
                {
                    e.Value = gamePatch.PrettyPrint();
                }
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

        private void OnSettingChanged(object? sender, EventArgs e)
        {
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            if (_targetPatchesComboBox.SelectedItem is null)
            {
                _saveButton.Enabled = false;
            }
            else if (string.IsNullOrWhiteSpace(_gameDirectoryInput.Text))
            {
                _saveButton.Enabled = true;
            }
            else
            {
                var directoryInfo = new DirectoryInfo(_gameDirectoryInput.Text);
                _saveButton.Enabled = directoryInfo.Exists;
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