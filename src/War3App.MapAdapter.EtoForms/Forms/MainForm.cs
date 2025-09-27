#define USE_KEY_CONTAINER

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using Eto.Drawing;
using Eto.Forms;

using Microsoft.Extensions.Configuration;

using War3App.Common.EtoForms;
using War3App.MapAdapter.Constants;
using War3App.MapAdapter.EtoForms.Controls;
using War3App.MapAdapter.EtoForms.Helpers;
using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter.EtoForms.Forms
{
    public partial class MainForm : Form
    {
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
        private readonly FileTreeView _fileTree;
        private readonly TextProgressBar _progressBar;

        private AppSettings _appSettings;
        private MpqArchive? _archive;
        private List<MpqArchive>? _nestedArchives;
        private bool _isTargetPatchFromZipArchive;
        private TargetPatch? _targetPatch;
        private GamePatch? _originPatch;

        public MainForm(IConfiguration configuration)
        {
            Size = new Size(1280, 720);
            MinimumSize = new Size(400, 300);
            Title = string.Format(TitleText.Main, typeof(MainForm).Assembly.GetVersionString());

            _configuration = configuration;
            _openArchiveWorker = CreateOpenArchiveWorker();
            _saveArchiveWorker = CreateSaveArchiveWorker();
            _watcher = CreateWatcher();

            _archiveInput = ControlFactory.TextBox(PlaceholderText.Archive);
            _archiveInputBrowseButton = ControlFactory.Button(ButtonText.Browse, enabled: true);
            _openCloseArchiveButton = ControlFactory.Button(ButtonText.Open);
            _targetPatchesComboBox = ControlFactory.DropDownList(width: 210);
            _adaptAllButton = ControlFactory.Button(ButtonText.AdaptAll);
            _saveAsButton = ControlFactory.Button(ButtonText.SaveAs);
            _getHelpButton = ControlFactory.Button(ButtonText.GetHelp);
            _diagnosticsDisplay = ControlFactory.DiagnosticsDisplay();
            _fileTree = ControlFactory.FileTreeView(this);
            _progressBar = ControlFactory.TextProgressBar();

            _appSettings = _configuration.LoadAppSettings();

            var targetPatchItems = _appSettings.TargetPatches
                .OrderByDescending(targetPatch => targetPatch.Patch)
                .Select(targetPatch => new ListItem { Key = targetPatch.Patch.ToString(), Text = targetPatch.Patch.PrettyPrint() })
                .ToList();

            _targetPatchesComboBox.DataStore = targetPatchItems;

            if (targetPatchItems.Count == 1)
            {
                _targetPatchesComboBox.SelectedIndex = 0;
            }

            _archiveInput.TextChanged += OnArchiveInputTextChanged;
            _archiveInputBrowseButton.Click += OnClickBrowseInputArchive;
            _openCloseArchiveButton.Click += OnClickOpenCloseArchive;
            _targetPatchesComboBox.SelectedIndexChanged += OnSelectedTargetPatchChanged;
            _adaptAllButton.Click += OnClickAdaptAll;
            _saveAsButton.Click += OnClickSaveAs;
            _getHelpButton.Click += OnClickGetHelp;
            _fileTree.Activated += OnClickEditSelected;
            _fileTree.KeyDown += OnFileKeyDown;
            _fileTree.SelectionChanged += OnFileSelectionChanged;

            _fileTree.AdaptRequested += OnClickAdaptSelected;
            _fileTree.EditRequested += OnClickEditSelected;
            _fileTree.SaveRequested += OnClickSaveSelected;
            _fileTree.DiffRequested += OnClickDiffSelected;
            _fileTree.UndoRequested += OnClickUndoChangesSelected;
            _fileTree.RemoveRequested += OnClickRemoveSelected;

            var mainSplitter = ControlFactory.Splitter(450, 200);

            var flowLayout = ControlFactory.HorizontalFlowLayout(
                new FlowLayoutItem(_archiveInput, minimumSize: 400),
                _archiveInputBrowseButton,
                _openCloseArchiveButton,
                new FlowLayoutItem(null),
                _adaptAllButton,
                _saveAsButton,
                ControlFactory.Label(LabelText.TargetPatch + ':'),
                _targetPatchesComboBox,
                _getHelpButton);

            var headerPanel = new Panel
            {
                BackgroundColor = SystemColors.ControlBackground,
                Content = flowLayout,
            };

            mainSplitter.Panel1 = _diagnosticsDisplay;
            mainSplitter.Panel2 = _fileTree;

            var progressBarPanel = new Panel
            {
                BackgroundColor = SystemColors.ControlBackground,
                Content = _progressBar,
                Height = 30,
                Padding = 3,
            };

            var table = new TableLayout
            {
                Rows =
                {
                    new TableRow(new TableCell(headerPanel, scaleWidth: true)) { ScaleHeight = false },
                    new TableRow(new TableCell(mainSplitter, scaleWidth: true)) { ScaleHeight = true },
                    new TableRow(new TableCell(progressBarPanel, scaleWidth: true)) { ScaleHeight = false },
                },
            };

            Content = new Panel { Content = table };
        }

        [MemberNotNullWhen(true, nameof(_targetPatch))]
        internal bool CanAdapt => _targetPatch is not null;

        private static IEnumerable<FileFilter> GetMpqArchiveFileTypeFilters(bool isOpenFileDialog)
        {
            yield return new FileFilter(FileType.Archive, FileExtension.Map, FileExtension.MapEx, FileExtension.Campaign);
            yield return new FileFilter(FileType.Map, FileExtension.Map, FileExtension.MapEx);
            yield return new FileFilter(FileType.Campaign, FileExtension.Campaign);
#if USE_KEY_CONTAINER
            yield return new FileFilter(FileType.Zip, FileExtension.Zip);
#endif
            yield return new FileFilter(FileType.Any, FileExtension.AnyFile);
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
            _openCloseArchiveButton.Enabled = File.Exists(_archiveInput.Text);
        }

        private void UpdateDiagnosticsDisplay()
        {
            _diagnosticsDisplay.Update(_fileTree.GetSelectedItems());
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

                return adaptResult;
            }

            return null;
        }
    }
}