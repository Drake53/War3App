using System;
using System.IO;
using System.Runtime.InteropServices;

namespace War3App.MapAdapter.WinForms.Helpers
{
    public static class WindowsFileExplorerHelper
    {
        // https://stackoverflow.com/a/12262552/723299
        public static void SelectFile(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new ArgumentNullException(nameof(fullPath));
            }

            fullPath = Path.GetFullPath(fullPath);

            var pidlList = NativeMethods.ILCreateFromPathW(fullPath);
            if (pidlList != IntPtr.Zero)
            {
                try
                {
                    // Open parent folder and select item
                    Marshal.ThrowExceptionForHR(NativeMethods.SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
                }
                finally
                {
                    NativeMethods.ILFree(pidlList);
                }
            }
        }
    }
}