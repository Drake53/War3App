using System.Collections.Generic;
using War3App.MapAdapter.EtoForms.Enums;

namespace War3App.MapAdapter.EtoForms.Models
{
    public class SelectedFileTreeItem
    {
        public SelectedFileTreeItem(KeyValuePair<FileTreeItem, SelectionType> kvp)
        {
            Item = kvp.Key;
            SelectionType = kvp.Value;
        }

        public FileTreeItem Item { get; set; }

        public SelectionType SelectionType { get; set; }
    }
}