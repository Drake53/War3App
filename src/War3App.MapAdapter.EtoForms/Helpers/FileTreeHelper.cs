using System.Collections.Generic;
using System.Linq;
using War3App.MapAdapter.EtoForms.Controls;
using War3App.MapAdapter.EtoForms.Models;

namespace War3App.MapAdapter.EtoForms.Helpers
{
    public static class FileTreeHelper
    {
        public static IEnumerable<FileTreeItem> GetLeafItems(IEnumerable<FileTreeItem> selectedItems)
        {
            var leafs = new HashSet<FileTreeItem>();

            foreach (var selectedItem in selectedItems)
            {
                if (selectedItem.Count == 0)
                {
                    leafs.Add(selectedItem);
                }
                else
                {
                    leafs.UnionWith(selectedItem.Children.Cast<FileTreeItem>());
                }
            }

            return leafs;
        }

        public static FileTreeSelection GetSelection(IEnumerable<FileTreeItem> selectedItems)
        {
            var selection = new FileTreeSelection();

            foreach (var selectedItem in selectedItems)
            {
                if (selectedItem.Count == 0)
                {
                    selection.Leafs.Add(selectedItem);
                }
                else
                {
                    selection.Parents.Add(selectedItem);
                    selection.Leafs.UnionWith(selectedItem.Children.Cast<FileTreeItem>());
                }
            }

            return selection;
        }

        public static IEnumerable<int> GetSelectedRows(FileTreeView fileTree, HashSet<FileTreeItem> selectedItems)
        {
            var selectedRows = new List<int>();

            var row = 0;
            foreach (var item in fileTree)
            {
                if (item.Expandable)
                {
                    if (item.Expanded)
                    {
                        foreach (var childItem in item.Children.Cast<FileTreeItem>())
                        {
                            row++;

                            if (selectedItems.Remove(childItem))
                            {
                                selectedRows.Add(row);
                            }
                        }
                    }
                    else
                    {
                        var count = selectedItems.Count;
                        selectedItems.ExceptWith(item.Children.Cast<FileTreeItem>());
                        if (selectedItems.Count < count)
                        {
                            selectedRows.Add(row);
                        }
                    }
                }
                else
                {
                    if (selectedItems.Remove(item))
                    {
                        selectedRows.Add(row);
                    }
                }

                row++;
            }

            return selectedRows;
        }
    }
}