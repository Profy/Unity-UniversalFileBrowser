using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ShellFileDialogs
{
    public static class FileOpenDialog
    {
        /// <summary>Shows the file open dialog for multiple filename selections. Returns <see langword="null"/> if the user cancelled the dialog, otherwise returns all selected paths.</summary>
        /// <param name="selectedFilterZeroBasedIndex">0-based index of the filter to select.</param>
        public static IReadOnlyList<string> ShowMultiSelectDialog(IntPtr parentHWnd, string title, string initialDirectory, string defaultFileName, IReadOnlyCollection<Filter> filters, int? selectedFilterZeroBasedIndex)
        {
            return ShowDialog(parentHWnd, title, initialDirectory, defaultFileName, filters, selectedFilterZeroBasedIndex, FileOpenOptions.AllowMultiSelect);
        }

        /// <summary>Shows the file open dialog for a single filename selection. Returns <see langword="null"/> if the user cancelled the dialog, otherwise returns the selected path.</summary>
        /// <param name="selectedFilterZeroBasedIndex">0-based index of the filter to select.</param>
        public static string ShowSingleSelectDialog(IntPtr parentHWnd, string title, string initialDirectory, string defaultFileName, IReadOnlyCollection<Filter> filters, int? selectedFilterZeroBasedIndex)
        {
            IReadOnlyList<string> fileNames = ShowDialog(parentHWnd, title, initialDirectory, defaultFileName, filters, selectedFilterZeroBasedIndex, FileOpenOptions.None);
            if (fileNames != null && fileNames.Count > 0)
            {
                return fileNames[0];
            }
            else
            {
                return null;
            }
        }

        private static IReadOnlyList<string> ShowDialog(IntPtr parentHWnd, string title, string initialDirectory, string defaultFileName, IReadOnlyCollection<Filter> filters, int? selectedFilterZeroBasedIndex, FileOpenOptions flags)
        {
            NativeFileOpenDialog nfod = new NativeFileOpenDialog();
            try
            {
                return ShowDialogInner(nfod, parentHWnd, title, initialDirectory, defaultFileName, filters, selectedFilterZeroBasedIndex: selectedFilterZeroBasedIndex ?? (-1), flags);
            }
            finally
            {
                _ = Marshal.ReleaseComObject(nfod);
            }
        }

        private static IReadOnlyList<string> ShowDialogInner(IFileOpenDialog dialog, IntPtr parentHWnd, string title, string initialDirectory, string defaultFileName, IReadOnlyCollection<Filter> filters, int selectedFilterZeroBasedIndex, FileOpenOptions flags)
        {
            flags = flags |
                FileOpenOptions.NoTestFileCreate |
                FileOpenOptions.PathMustExist |
                FileOpenOptions.ForceFilesystem;

            dialog.SetOptions(flags);

            if (title != null)
            {
                dialog.SetTitle(title);
            }

            if (initialDirectory != null)
            {
                IShellItem2 initialDirectoryShellItem = Utility.ParseShellItem2Name(initialDirectory);
                if (initialDirectoryShellItem != null)
                {
                    dialog.SetFolder(initialDirectoryShellItem);
                }
            }

            if (defaultFileName != null)
            {
                dialog.SetFileName(defaultFileName);
            }

            Utility.SetFilters(dialog, filters, selectedFilterZeroBasedIndex);

            HResult hr = dialog.Show(parentHWnd);
            if (hr.ValidateDialogShowHResult())
            {
                dialog.GetResults(out IShellItemArray resultsArray);

                IReadOnlyList<string> fileNames = Utility.GetFileNames(resultsArray);
                return fileNames;
            }
            else
            {
                // User cancelled.
                return null;
            }
        }
    }
}
