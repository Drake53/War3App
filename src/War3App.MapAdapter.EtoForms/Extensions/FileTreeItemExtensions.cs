using System;
using System.Linq;
using War3App.MapAdapter.Constants;
using War3App.MapAdapter.EtoForms.Enums;
using War3App.MapAdapter.EtoForms.Models;

namespace War3App.MapAdapter.EtoForms.Extensions
{
    public static class FileTreeItemExtensions
    {
        public static void UpdateStatus(this FileTreeItem item)
        {
            var mapFile = item.MapFile;
            if (mapFile.Children is not null)
            {
                mapFile.Status = mapFile.Children.Max(child => child.Status);
            }
        }

        public static void UpdateFileName(this FileTreeItem item, AdaptResult? adaptResult)
        {
            var mapFile = item.MapFile;

            if (adaptResult is null || adaptResult.Status == MapFileStatus.Removed)
            {
                item.FileName = mapFile.OriginalFileName ?? MiscStrings.UnknownFileName;
            }
            else if (adaptResult.NewFileName is not null)
            {
                item.FileName = adaptResult.NewFileName;
            }
        }

        public static int CompareTo(this FileTreeItem item, FileTreeItem other, FileTreeColumn column)
        {
            return column switch
            {
                FileTreeColumn.Status => other.CompareStatus(item),
                FileTreeColumn.FileName => CompareText(item.FileName, other.FileName),
                FileTreeColumn.FileType => CompareText(item.FileType, other.FileType),
                FileTreeColumn.Archive => CompareText(item.Archive, other.Archive),
                _ => item.MapFile.OriginalIndex.CompareTo(other.MapFile.OriginalIndex),
            };
        }

        private static int CompareText(string? text1, string? text2)
        {
            return string.IsNullOrWhiteSpace(text1) == string.IsNullOrWhiteSpace(text2)
                ? string.Compare(text1, text2, StringComparison.InvariantCulture)
                : string.IsNullOrWhiteSpace(text1) ? 1 : -1;
        }

        private static int CompareStatus(this FileTreeItem item, FileTreeItem other)
        {
            var mapFile1 = item.MapFile;
            var mapFile2 = other.MapFile;

            var result = mapFile1.Status.CompareTo(mapFile2.Status);
            if (result != 0)
            {
                return result;
            }

            var modified1 = mapFile1.AdaptResult?.AdaptedFileStream is not null;
            var modified2 = mapFile2.AdaptResult?.AdaptedFileStream is not null;

            return modified1.CompareTo(modified2);
        }
    }
}