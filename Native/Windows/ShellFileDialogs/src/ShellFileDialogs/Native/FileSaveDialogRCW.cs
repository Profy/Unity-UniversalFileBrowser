using System;
using System.Runtime.InteropServices;

namespace ShellFileDialogs
{
    [ComImport]
    [ClassInterface(ClassInterfaceType.None)]
    [TypeLibType(TypeLibTypeFlags.FCanCreate)]
    [Guid(ShellCLSIDGuid.FileSaveDialog)]
    internal class FileSaveDialogRCW
    {
    }
}
