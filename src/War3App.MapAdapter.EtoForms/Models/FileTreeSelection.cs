using System.Collections.Generic;

namespace War3App.MapAdapter.EtoForms.Models
{
    public class FileTreeSelection
    {
        public HashSet<FileTreeItem> Parents { get; set; }

        public HashSet<FileTreeItem> Leafs { get; set; }
    }
}