using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Eto.Drawing;
using Eto.Forms;

using War3App.MapAdapter.Constants;
using War3App.MapAdapter.Extensions;
using War3App.MapAdapter.EtoForms.Enums;
using War3App.MapAdapter.EtoForms.Forms;
using War3App.MapAdapter.EtoForms.Models;
using War3App.MapAdapter.EtoForms.Helpers;

namespace War3App.MapAdapter.EtoForms.Controls
{
    public class FileTreeView : TreeGridView, IEnumerable<FileTreeItem>
    {
        private readonly MainForm _mainForm;
        private readonly TreeGridItemCollection _items = new();
        private FileTreeColumn _sortColumn;
        private SortDirection _sortDirection = SortDirection.None;

        public FileTreeView(MainForm mainForm)
        {
            _mainForm = mainForm;

            DataStore = _items;
            AllowMultipleSelection = true;
            ShowHeader = true;
            GridLines = GridLines.Both;

            SetupColumns();
            SetupEventHandlers();
            SetupContextMenu();
        }

        public bool HasItems => _items.Count > 0;

        public new IEnumerable<FileTreeItem> SelectedItems
        {
            get => base.SelectedItems.Cast<FileTreeItem>();
            set => SelectedRows = FileTreeHelper.GetSelectedRows(this, value);
        }

        public void SetItems(IEnumerable<FileTreeItem> items)
        {
            _items.AddRange(items);
            DataStore = _items;
        }

        private void SetupColumns()
        {
            Columns.Add(new GridColumn
            {
                HeaderText = HeaderText.Status,
                DataCell = new ImageTextCell
                {
                    TextBinding = Binding.Property(FileTreeItem.GetStatusTextExpression()),
                    ImageBinding = Binding.Property(FileTreeItem.GetStatusImageExpression()),
                },
                Width = 120,
                Sortable = true,
                DisplayIndex = (int)FileTreeColumn.Status,
            });

            Columns.Add(new GridColumn
            {
                HeaderText = HeaderText.FileName,
                DataCell = new TextBoxCell { Binding = Binding.Property(FileTreeItem.GetFileNameExpression()) },
                Width = 350,
                Sortable = true,
                DisplayIndex = (int)FileTreeColumn.FileName,
                Expand = true,
            });

            Columns.Add(new GridColumn
            {
                HeaderText = HeaderText.FileType,
                DataCell = new TextBoxCell { Binding = Binding.Property<FileTreeItem, string>(item => item.FileType) },
                Width = 150,
                Sortable = true,
                DisplayIndex = (int)FileTreeColumn.FileType,
            });

            Columns.Add(new GridColumn
            {
                HeaderText = HeaderText.Archive,
                DataCell = new TextBoxCell { Binding = Binding.Property<FileTreeItem, string>(item => item.Archive) },
                Width = 100,
                Sortable = true,
                DisplayIndex = (int)FileTreeColumn.Archive,
            });
        }

        private void SetupEventHandlers()
        {
            ColumnHeaderClick += OnColumnHeaderClick;
            CellFormatting += OnCellFormatting;
        }

        private void SetupContextMenu()
        {
            var contextMenu = new FileTreeContextMenu(_mainForm, this);
            ContextMenu = contextMenu;

            contextMenu.Adapt += (s, e) => AdaptRequested?.Invoke(this, EventArgs.Empty);
            contextMenu.Edit += (s, e) => EditRequested?.Invoke(this, EventArgs.Empty);
            contextMenu.Save += (s, e) => SaveRequested?.Invoke(this, EventArgs.Empty);
            contextMenu.Diff += (s, e) => DiffRequested?.Invoke(this, EventArgs.Empty);
            contextMenu.Undo += (s, e) => UndoRequested?.Invoke(this, EventArgs.Empty);
            contextMenu.Remove += (s, e) => RemoveRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? AdaptRequested;
        public event EventHandler? EditRequested;
        public event EventHandler? SaveRequested;
        public event EventHandler? DiffRequested;
        public event EventHandler? UndoRequested;
        public event EventHandler? RemoveRequested;

        public void Reset()
        {
            _sortDirection = SortDirection.None;

            foreach (var item in _items.Cast<FileTreeItem>())
            {
                item.MapFile.Dispose();
            }

            SelectedRows = Array.Empty<int>();
            _items.Clear();
        }

        private void OnColumnHeaderClick(object? sender, GridColumnEventArgs e)
        {
            var newSortColumn = GetHeaderType(e.Column);

            if (newSortColumn == _sortColumn)
            {
                _sortDirection = _sortDirection switch
                {
                    SortDirection.None => SortDirection.Ascending,
                    SortDirection.Ascending => SortDirection.Descending,
                    SortDirection.Descending => SortDirection.None,
                    _ => SortDirection.None,
                };
            }
            else
            {
                _sortColumn = newSortColumn;
                _sortDirection = SortDirection.Ascending;
            }

            SortItems();
        }

        private void SortItems()
        {
            var fileItems = _items.Cast<FileTreeItem>().ToList();

            List<FileTreeItem> sortedItems;
            if (_sortDirection == SortDirection.None)
            {
                sortedItems = fileItems.OrderBy(item => item.MapFile.OriginalIndex).ToList();
            }
            else
            {
                sortedItems = _sortColumn switch
                {
                    FileTreeColumn.Status => fileItems.OrderBy(item => item.MapFile.Status, _sortDirection == SortDirection.Ascending).ToList(),
                    FileTreeColumn.FileName => fileItems.OrderBy(item => item.FileName, _sortDirection == SortDirection.Ascending).ToList(),
                    FileTreeColumn.FileType => fileItems.OrderBy(item => item.FileType, _sortDirection == SortDirection.Ascending).ToList(),
                    FileTreeColumn.Archive => fileItems.OrderBy(item => item.Archive, _sortDirection == SortDirection.Ascending).ToList(),
                    _ => fileItems.ToList(),
                };
            }

            SuspendLayout();

            SelectedRows = Array.Empty<int>();
            _items.Clear();

            foreach (var item in sortedItems)
            {
                _items.Add(item);
            }

            ResumeLayout();
        }

        private void OnCellFormatting(object? sender, GridCellFormatEventArgs e)
        {
            if (GetHeaderType(e.Column) == FileTreeColumn.FileName &&
                e.Item is FileTreeItem item)
            {
                var adaptResult = item.MapFile.AdaptResult;
                if (adaptResult is null)
                {
                    return;
                }

                if (adaptResult.Status == MapFileStatus.Removed)
                {
                    e.ForegroundColor = Colors.LightGrey;
                }
                else if (adaptResult.NewFileName is not null)
                {
                    e.ForegroundColor = Colors.Blue;
                }
                else if (adaptResult.AdaptedFileStream is not null)
                {
                    e.ForegroundColor = Colors.Violet;
                }
            }
        }

        public FileTreeItem[] GetSelectedItems()
        {
            return SelectedItems.Cast<FileTreeItem>().ToArray();
        }

        public MapFile[] GetSelectedMapFiles()
        {
            return SelectedItems.Cast<FileTreeItem>().Select(item => item.MapFile).ToArray();
        }

        public bool TryGetSelectedMapFile([NotNullWhen(true)] out MapFile? mapFile)
        {
            var enumerator = SelectedItems.Cast<FileTreeItem>().GetEnumerator();
            if (!enumerator.MoveNext())
            {
                mapFile = null;
                return false;
            }

            mapFile = enumerator.Current.MapFile;
            return !enumerator.MoveNext();
        }

        private FileTreeColumn GetHeaderType(GridColumn column)
        {
            var columnIndex = Columns.IndexOf(column);
            return (FileTreeColumn)columnIndex;
        }

        public IEnumerator<FileTreeItem> GetEnumerator()
        {
            return _items.Cast<FileTreeItem>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}