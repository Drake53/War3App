using System.Collections.Generic;
using System.Linq;
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
            var parents = new HashSet<FileTreeItem>();
            var leafs = new HashSet<FileTreeItem>();

            foreach (var selectedItem in selectedItems)
            {
                if (selectedItem.Count == 0)
                {
                    leafs.Add(selectedItem);
                }
                else
                {
                    parents.Add(selectedItem);
                    leafs.UnionWith(selectedItem.Children.Cast<FileTreeItem>());
                }
            }

            return new FileTreeSelection
            {
                Parents = parents,
                Leafs = leafs,
            };
        }
    }
}