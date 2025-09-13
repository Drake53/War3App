using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Windows.Forms;

using War3App.Common.WinForms.Extensions;
using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Extensions;
using War3App.MapAdapter.WinForms.Helpers;

namespace War3App.MapAdapter.WinForms.Forms
{
    [DesignerCategory("")]
    internal sealed class GetHelpForm : Form
    {
        private readonly string _mapFilePath;
        private readonly TargetPatch _targetPatch;

        private readonly RichTextBox _helpTextBox;
        private readonly CheckBox _encryptMapFileCheckBox;
        private readonly Button _saveOrNavigateToZipButton;

        private string? _savedZipFilePath;

        public GetHelpForm(string mapFilePath, TargetPatch targetPatch)
        {
            _mapFilePath = mapFilePath;
            _targetPatch = targetPatch;

            Size = new Size(400, 300);
            MinimumSize = new Size(400, 300);
            Text = TitleText.Help;

            _helpTextBox = new RichTextBox
            {
                Text = MessageText.GetHelp,
                Width = 380,
                Height = 200,
                ReadOnly = true,
                DetectUrls = true,
                TabIndex = 0,
            };

            _helpTextBox.LinkClicked += (s, e) =>
            {
                OpenUrl(e.LinkText);
            };

            var encryptMapFileLabel = new Label
            {
                Text = LabelText.Encrypt,
                TextAlign = ContentAlignment.BottomRight,
            };

            encryptMapFileLabel.Size = encryptMapFileLabel.PreferredSize;

            _encryptMapFileCheckBox = new CheckBox();
            _encryptMapFileCheckBox.CheckedChanged += (s, e) =>
            {
                _savedZipFilePath = null;
                _saveOrNavigateToZipButton.Text = ButtonText.SaveZip;
            };

            _saveOrNavigateToZipButton = new Button
            {
                Text = ButtonText.SaveZip,
                Dock = DockStyle.Bottom,
                TabIndex = 1,
            };

            _saveOrNavigateToZipButton.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(_savedZipFilePath))
                {
                    WindowsFileExplorerHelper.SelectFile(_savedZipFilePath);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    OverwritePrompt = true,
                    CreatePrompt = false,
                    AddExtension = true,
                    DefaultExt = FileExtension.Zip,
                    Filter = FilterStrings.ZipFileOrAllFiles,
                };

                var saveFileDialogResult = saveFileDialog.ShowDialog();
                if (saveFileDialogResult == DialogResult.OK)
                {
                    SaveZipFile(saveFileDialog.FileName);

                    _saveOrNavigateToZipButton.Text = ButtonText.ShowZip;
                    _savedZipFilePath = saveFileDialog.FileName;
                }
            };

            var horizontalFlowLayout = new FlowLayoutPanel();
            horizontalFlowLayout.FlowDirection = FlowDirection.LeftToRight;
            horizontalFlowLayout.AddControls(encryptMapFileLabel, _encryptMapFileCheckBox);
            horizontalFlowLayout.Size = horizontalFlowLayout.PreferredSize;

            var verticalFlowLayout = new FlowLayoutPanel();
            verticalFlowLayout.FlowDirection = FlowDirection.TopDown;
            verticalFlowLayout.AddControls(_helpTextBox, horizontalFlowLayout);
            verticalFlowLayout.Size = verticalFlowLayout.PreferredSize;

            this.AddControls(verticalFlowLayout, _saveOrNavigateToZipButton);
        }

        // https://stackoverflow.com/a/43232486
        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                url = url.Replace("&", "^&", StringComparison.Ordinal);
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }

        private void SaveZipFile(string zipFilePath)
        {
            using var fileStream = File.Create(zipFilePath);
            using var zipFile = new ZipArchive(fileStream, ZipArchiveMode.Create);

            if (_encryptMapFileCheckBox.Checked)
            {
                using var rsa = RSA.Create();
                rsa.ImportRSAPublicKey(Resources.Files.PublicKey, out _);

                using var aes = Aes.Create();
                aes.Padding = PaddingMode.PKCS7;

                var aesParameters = new byte[aes.Key.Length + aes.IV.Length];
                Array.Copy(aes.Key, aesParameters, aes.Key.Length);
                Array.Copy(aes.IV, 0, aesParameters, aes.Key.Length, aes.IV.Length);

                var encryptedAesParameters = rsa.Encrypt(aesParameters, RSAEncryptionPadding.Pkcs1);
                zipFile.AddEntry(FileName.AesParameters, encryptedAesParameters);

                using var memoryStream = new MemoryStream();
                using var aesEncryptor = aes.CreateEncryptor();
                using var cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);

                using var mapFileStream = File.OpenRead(_mapFilePath);
                mapFileStream.CopyTo(cryptoStream);
                cryptoStream.FlushFinalBlock();

                zipFile.AddEntry($"{Path.GetFileNameWithoutExtension(_mapFilePath)}{FileExtension.Aes}", memoryStream.ToArray());
            }
            else
            {
                zipFile.AddFile(_mapFilePath, string.Empty);
            }

            zipFile.AddEntry(FileName.ApplicationVersion, typeof(GetHelpForm).Assembly.GetFullVersionString());
            zipFile.AddEntry(FileName.TargetPatch, _targetPatch.Patch.ToString());

            foreach (var path in PathConstants.GetAllPaths())
            {
                using var gameDataFileStream = _targetPatch.OpenGameDataFile(path);
                if (gameDataFileStream is not null)
                {
                    zipFile.AddEntry(Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path)), gameDataFileStream);
                }
            }
        }
    }
}