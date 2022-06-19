using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ShellFileDialogs
{
    public static class FolderBrowserDialog
    {
        /// <summary>Shows the folder browser dialog. Returns <see langword="null"/> if the user cancelled the dialog. Otherwise returns the selected path.</summary>
        public static IReadOnlyList<string> ShowMultiSelectDialog(IntPtr parentHWnd, string title, string initialDirectory)
        {
            return ShowDialog(parentHWnd, title, initialDirectory, FileOpenOptions.AllowMultiSelect);
        }

        /// <summary>Shows the folder browser dialog. Returns <see langword="null"/> if the user cancelled the dialog. Otherwise returns the selected path.</summary>
        public static string ShowSingleSelectDialog(IntPtr parentHWnd, string title, string initialDirectory)
        {
            IReadOnlyList<string> fileNames = ShowDialog(parentHWnd, title, initialDirectory, FileOpenOptions.None);
            if (fileNames != null && fileNames.Count > 0)
            {
                return fileNames[0];
            }
            else
            {
                return null;
            }
        }

        private static IReadOnlyList<string> ShowDialog(IntPtr parentHWnd, string title, string initialDirectory, FileOpenOptions flags)
        {
            NativeFileOpenDialog nfod = new NativeFileOpenDialog();
            try
            {
                return ShowDialogInner(nfod, parentHWnd, title, initialDirectory, flags);
            }
            finally
            {
                _ = Marshal.ReleaseComObject(nfod);
            }
        }

        private static IReadOnlyList<string> ShowDialogInner(IFileOpenDialog dialog, IntPtr parentHWnd, string title, string initialDirectory, FileOpenOptions flags)
        {
            //IFileDialog ifd = dialog;
            flags = flags |
               FileOpenOptions.NoTestFileCreate |
               FileOpenOptions.PathMustExist |
               FileOpenOptions.PickFolders |
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
