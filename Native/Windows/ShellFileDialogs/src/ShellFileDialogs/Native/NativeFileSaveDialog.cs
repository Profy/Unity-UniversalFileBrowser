using System;
using System.Runtime.InteropServices;

namespace ShellFileDialogs
{
    [ComImport]
    [Guid(ShellIIDGuid.IFileSaveDialog)]
    [CoClass(typeof(FileSaveDialogRCW))]
    internal interface INativeFileSaveDialog : IFileSaveDialog
    {
    }
}
