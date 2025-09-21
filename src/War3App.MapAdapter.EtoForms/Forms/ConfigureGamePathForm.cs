using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Eto.Drawing;
using Eto.Forms;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.EtoForms.Helpers;
using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;

namespace War3App.MapAdapter.EtoForms.Forms
{
    public sealed class ConfigureGamePathForm : Dialog<DialogResult>
    {
        private readonly TextBox _gameDirectoryInput;
        private readonly Button _gameDirectoryInputBrowseButton;

        private readonly ComboBox _targetPatchesComboBox;

        private readonly Button _saveButton;

        public ConfigureGamePathForm()
        {
            Title = TitleText.Setup;
            Size = new Size(600, 200);
            MinimumSize = new Size(400, 200);
            Resizable = true;

            _gameDirectoryInput = new TextBox
            {
                PlaceholderText = PlaceholderText.GameDirectory,
                TabIndex = 2,
                Size = new Size(360, -1),
            };

            _gameDirectoryInput.TextChanged += OnSettingChanged;

            _gameDirectoryInputBrowseButton = new Button
            {
                Text = ButtonText.Browse,
                TabIndex = 0,
            };

            _gameDirectoryInputBrowseButton.Click += (s, e) =>
            {
                var openDirectoryDialog = new SelectFolderDialog
                {
                };

                var openDirectoryDialogResult = openDirectoryDialog.ShowDialog(this);
                if (openDirectoryDialogResult == DialogResult.Ok)
                {
                    _gameDirectoryInput.Text = openDirectoryDialog.Directory;
                }
            };

            _saveButton = new Button
            {
                Text = ButtonText.Save,
                Enabled = false,
                TabIndex = 1,
            };

            _saveButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_gameDirectoryInput.Text))
                {
                    var dialogResult = MessageBox.Show(
                        this,
                        MessageText.ContinueWithoutGameFiles,
                        TitleText.ContinueWithoutGameFiles,
                        MessageBoxButtons.YesNo,
                        MessageBoxType.Warning);

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
                            this,
                            string.Join(System.Environment.NewLine, missingFiles.Prepend(MessageText.MissingFiles)),
                            TitleText.MissingFiles,
                            MessageBoxButtons.OK,
                            MessageBoxType.Error);

                        return;
                    }
                }

                Close(DialogResult.Ok);
            };

            var targetPatchLabel = new Label
            {
                Text = LabelText.TargetPatch,
                TextAlignment = TextAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
            };

            _targetPatchesComboBox = new ComboBox
            {
                ReadOnly = true,
                Size = new Size(120, -1),
            };

            foreach (var patch in Enum.GetValues<GamePatch>().OrderByDescending(patch => patch))
            {
                _targetPatchesComboBox.Items.Add(patch.PrettyPrint(), patch.ToString());
            }

            _targetPatchesComboBox.SelectedIndexChanged += OnSettingChanged;

            var inputGameDirectoryLayout = ControlFactory.HorizontalFlowLayout(
                _gameDirectoryInput,
                _gameDirectoryInputBrowseButton);

            var targetPatchLayout = ControlFactory.HorizontalFlowLayout(
                targetPatchLabel,
                _targetPatchesComboBox);

            var mainLayout = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Padding = 10,
                Spacing = 10,
                Items =
                {
                    inputGameDirectoryLayout,
                    targetPatchLayout,
                    new StackLayoutItem(_saveButton, HorizontalAlignment.Right),
                },
            };

            Content = mainLayout;
            DefaultButton = _saveButton;
        }

        public string GameDirectory => _gameDirectoryInput.Text;

        public GamePatch GamePatch =>  Enum.Parse<GamePatch>(_targetPatchesComboBox.SelectedKey);

        private void OnSettingChanged(object? sender, EventArgs e)
        {
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            if (_targetPatchesComboBox.SelectedIndex == -1)
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