#define USE_KEY_CONTAINER

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Microsoft.Extensions.Configuration;

using War3App.Common.WinForms;
using War3App.Common.WinForms.Extensions;
using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Extensions;
using War3App.MapAdapter.WinForms.Constants;
using War3App.MapAdapter.WinForms.Controls;
using War3App.MapAdapter.WinForms.Helpers;

using War3Net.Build.Common;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.WinForms.Forms
{
    [DesignerCategory("")]
    internal partial class MainForm : Form
    {
        private static MainForm _mainForm;

        private readonly IConfiguration _configuration;
        private readonly BackgroundWorker _openArchiveWorker;
        private readonly BackgroundWorker _saveArchiveWorker;
        private readonly FileSystemWatcher _watcher;

        private readonly TextBox _archiveInput;
        private readonly Button _archiveInputBrowseButton;
        private readonly Button _openCloseArchiveButton;
        private readonly ComboBox _targetPatchesComboBox;
        private readonly Button _adaptAllButton;
        private readonly Button _saveAsButton;
        private readonly Button _getHelpButton;
        private readonly DiagnosticsDisplay _diagnosticsDisplay;
        private readonly FileListView _fileList;
        private readonly TextProgressBar _progressBar;

        private AppSettings _appSettings;
        private MpqArchive? _archive;
        private List<MpqArchive>? _nestedArchives;
        private Timer? _fileSelectionChangedEventTimer;
        private bool _isTargetPatchFromZipArchive;
        private TargetPatch? _targetPatch;
        private GamePatch? _originPatch;

        public MainForm(IConfiguration configuration)
        {
            if (_mainForm is not null)
            {
                throw new InvalidOperationException("Cannot create multiple main forms.");
            }

            _mainForm = this;

            Size = new Size(1280, 720);
            MinimumSize = new Size(400, 300);
            Text = string.Format(TitleText.Main, typeof(MainForm).Assembly.GetVersionString());

            _configuration = configuration;
            _openArchiveWorker = CreateOpenArchiveWorker();
            _saveArchiveWorker = CreateSaveArchiveWorker();
            _watcher = CreateWatcher();

            _archiveInput = ControlFactory.TextBox(PlaceholderText.Archive);
            _archiveInputBrowseButton = ControlFactory.Button(ButtonText.Browse, enabled: true);
            _openCloseArchiveButton = ControlFactory.Button(ButtonText.Open);
            _targetPatchesComboBox = ControlFactory.DropDownList(width: 120);
            _adaptAllButton = ControlFactory.Button(ButtonText.AdaptAll);
            _saveAsButton = ControlFactory.Button(ButtonText.SaveAs);
            _getHelpButton = ControlFactory.Button(ButtonText.GetHelp);
            _diagnosticsDisplay = ControlFactory.DiagnosticsDisplay();
            _fileList = ControlFactory.FileListView();
            _progressBar = ControlFactory.TextProgressBar();

            _appSettings = _configuration.LoadAppSettings();

            _targetPatchesComboBox.FormattingEnabled = true;
            _targetPatchesComboBox.Items.AddRange(_appSettings.TargetPatches.OrderByDescending(targetPatch => targetPatch.Patch).Select(targetPatch => (object)targetPatch.Patch).ToArray());
            if (_targetPatchesComboBox.Items.Count == 1)
            {
                _targetPatchesComboBox.SelectedIndex = 0;
            }

            var fileListContextMenu = new FileListContextMenuStrip(_fileList);
            _fileList.ContextMenuStrip = fileListContextMenu;

            _archiveInput.TextChanged += OnArchiveInputTextChanged;
            _archiveInputBrowseButton.Click += OnClickBrowseInputArchive;
            _openCloseArchiveButton.Click += OnClickOpenCloseArchive;
            _targetPatchesComboBox.Format += FormatTargetPatch;
            _targetPatchesComboBox.SelectedIndexChanged += OnSelectedTargetPatchChanged;
            _adaptAllButton.Click += OnClickAdaptAll;
            _saveAsButton.Click += OnClickSaveAs;
            _getHelpButton.Click += OnClickGetHelp;
            _fileList.ItemActivate += OnClickEditSelected;
            _fileList.KeyDown += OnFileKeyDown;
            _fileList.SelectedIndexChanged += OnFileSelectionChanged;

            fileListContextMenu.Adapt += OnClickAdaptSelected;
            fileListContextMenu.Edit += OnClickEditSelected;
            fileListContextMenu.Save += OnClickSaveSelected;
            fileListContextMenu.Diff += OnClickDiffSelected;
            fileListContextMenu.Undo += OnClickUndoChangesSelected;
            fileListContextMenu.Remove += OnClickRemoveSelected;
            fileListContextMenu.RegisterClickEvents();

            var targetPatchLabel = ControlFactory.Label(LabelText.TargetPatch);

            var inputArchiveFlowLayout = ControlFactory.HorizontalLayoutPanel(_archiveInput, _archiveInputBrowseButton, _openCloseArchiveButton);
            var buttonsFlowLayout = ControlFactory.HorizontalLayoutPanel(_adaptAllButton, _saveAsButton, targetPatchLabel, _targetPatchesComboBox, _getHelpButton);
            var flowLayout = ControlFactory.VerticalLayoutPanel(width: 640, inputArchiveFlowLayout, buttonsFlowLayout);

            var splitContainer = ControlFactory.SplitContainer();
            splitContainer.Panel1.AddControls(_diagnosticsDisplay, flowLayout);
            splitContainer.Panel2.AddControls(_fileList);
            this.AddControls(splitContainer, _progressBar);

            targetPatchLabel.Size = targetPatchLabel.PreferredSize;

            splitContainer.Panel1.SizeChanged += (s, e) =>
            {
                var width = splitContainer.Panel1.Width;
                _archiveInput.Width = (width > 360 ? 360 : width) - 10;

                inputArchiveFlowLayout.Size = inputArchiveFlowLayout.GetPreferredSize(new Size(width, 0));
                buttonsFlowLayout.Size = buttonsFlowLayout.GetPreferredSize(new Size(width, 0));
                flowLayout.Height
                    = inputArchiveFlowLayout.Height + inputArchiveFlowLayout.Margin.Vertical
                    + buttonsFlowLayout.Height + buttonsFlowLayout.Margin.Vertical;
            };

            splitContainer.SplitterDistance = 640 - splitContainer.SplitterWidth;
            splitContainer.Panel1MinSize = 200;

            FormClosing += DisposeOnClosing;
        }

        internal static MainForm Instance => _mainForm;

        [MemberNotNullWhen(true, nameof(_targetPatch))]
        internal bool CanAdapt => _targetPatch is not null;

        private static string GetMpqArchiveFileTypeFilter(bool isOpenFileDialog)
        {
            var filters = new List<string>
            {
                FilterStrings.ArchiveFile,
                FilterStrings.MapFile,
                FilterStrings.CampaignFile,
            };

#if USE_KEY_CONTAINER
            if (isOpenFileDialog)
            {
                filters.Add(FilterStrings.ZipArchive);
            }
#endif

            filters.Add(FilterStrings.AllFiles);

            return FilterStrings.Combine(filters.ToArray());
        }

        private FileSystemWatcher CreateWatcher()
        {
            var watcher = new FileSystemWatcher();

            watcher.Created += OnWatchedFileEvent;
            watcher.Renamed += OnWatchedFileEvent;
            watcher.Deleted += OnWatchedFileEvent;

            return watcher;
        }

        private void OnWatchedFileEvent(object? sender, EventArgs e)
        {
            SetOpenArchiveButtonEnabled(File.Exists(_archiveInput.Text));
        }

        private void SetOpenArchiveButtonEnabled(bool enabled)
        {
            if (_openCloseArchiveButton.InvokeRequired)
            {
                _openCloseArchiveButton.Invoke(new Action(() => _openCloseArchiveButton.Enabled = enabled));
            }
            else
            {
                _openCloseArchiveButton.Enabled = enabled;
            }
        }

        private void UpdateDiagnosticsDisplay()
        {
            _diagnosticsDisplay.Update(_fileList.SelectedItems);
        }

        private TargetPatch? GetTargetPatch(GamePatch? patch)
        {
            return patch.HasValue
                ? _appSettings.TargetPatches.FirstOrDefault(targetPatch => targetPatch.Patch == patch.Value)
                : null;
        }

        private AdaptResult? AdaptMapFile(MapFile mapFile)
        {
            var adapter = mapFile.Adapter;
            if (adapter is not null && mapFile.Status == MapFileStatus.Pending)
            {
                var context = new AdaptFileContext
                {
                    FileName = mapFile.CurrentFileName,
                    Archive = mapFile.MpqArchive,
                    TargetPatch = _targetPatch,
                    OriginPatch = mapFile.GetOriginPatch(_originPatch.Value),
                };

                mapFile.CurrentStream.Position = 0;
                var adaptResult = adapter.Run(mapFile.CurrentStream, context);
                mapFile.UpdateAdaptResult(adaptResult);
                _fileList.UpdateItemForMapFile(mapFile);

                return adaptResult;
            }

            return null;
        }
    }
}