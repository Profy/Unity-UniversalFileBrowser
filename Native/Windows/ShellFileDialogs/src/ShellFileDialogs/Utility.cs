using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace ShellFileDialogs
{
    internal static class Utility
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "ExternalException")]
        public static IReadOnlyList<string> GetFileNames(IShellItemArray items)
        {
            HResult hresult = items.GetCount(out uint count);
            if (hresult != HResult.Ok)
            {
                throw new ExternalException("IShellItemArray.GetCount failed. HResult: " + hresult); // TODO: Will this ever happen?
            }
            else
            {
                List<string> list = new List<string>(capacity: (int)count);

                for (int i = 0; i < count; i++)
                {
                    IShellItem shellItem = Utility.GetShellItemAt(items, i);
                    string fileName = Utility.GetFileNameFromShellItem(shellItem);
                    if (fileName != null)
                    {
                        list.Add(fileName);
                    }
                }

                return list;
            }
        }

        private static readonly Guid _ishellItem2Guid = new Guid(ShellIIDGuid.IShellItem2);

        public static IShellItem2 ParseShellItem2Name(string value)
        {
            Guid ishellItem2GuidCopy = _ishellItem2Guid;

            HResult hresult = ShellNativeMethods.SHCreateItemFromParsingName(value, IntPtr.Zero, ref ishellItem2GuidCopy, out IShellItem2 shellItem);
            if (hresult == HResult.Ok)
            {
                return shellItem;
            }
            else
            {
                // TODO: Handle HRESULT error codes?
                return null;
            }
        }

        public static string GetFileNameFromShellItem(IShellItem item)
        {
            if (item is null)
            {
                return null;
            }
            else
            {
                HResult hr = item.GetDisplayName(ShellItemDesignNameOptions.DesktopAbsoluteParsing, out IntPtr pszString);
                if (hr == HResult.Ok && pszString != IntPtr.Zero)
                {
                    string fileName = Marshal.PtrToStringAuto(pszString);
                    Marshal.FreeCoTaskMem(pszString);
                    return fileName;
                }
                else
                {
                    return null;
                }
            }
        }

        public static IShellItem GetShellItemAt(IShellItemArray array, int i)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));

            HResult hr = array.GetItemAt((uint)i, out IShellItem result);
            if (hr == HResult.Ok)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>Sets the file extension filters on <paramref name="dialog"/>.</summary>
        /// <param name="dialog">Required. Cannot be <see langword="null"/>.</param>
        /// <param name="filters">If this is <see langword="null"/> or empty, then this method returns immediately (i.e. it does nothing).</param>
        /// <param name="selectedFilterZeroBasedIndex">0-based index of the filter in in <paramref name="filters"/> to use. If this value is out-of-range then this method does nothing.</param>
        public static void SetFilters(IFileDialog dialog, IReadOnlyCollection<Filter> filters, int selectedFilterZeroBasedIndex)
        {
            if (dialog is null) throw new ArgumentNullException(nameof(dialog));

            if (filters == null || filters.Count == 0) return;

            FilterSpec[] specs = Utility.CreateFilterSpec(filters);
            dialog.SetFileTypes((uint)specs.Length, specs);

            if (selectedFilterZeroBasedIndex > -1 && selectedFilterZeroBasedIndex < filters.Count)
            {
                dialog.SetFileTypeIndex(1 + (uint)selectedFilterZeroBasedIndex); // In the COM interface (like the other Windows OFD APIs), filter indexes are 1-based, not 0-based.
                dialog.SetDefaultExtension(specs[(uint)selectedFilterZeroBasedIndex].Spec != "*.*" ? specs[(uint)selectedFilterZeroBasedIndex].Spec : null);
            }
        }

        public static FilterSpec[] CreateFilterSpec(IReadOnlyCollection<Filter> filters)
        {
            FilterSpec[] specs = new FilterSpec[filters.Count];
            int i = 0;
            foreach (Filter filter in filters)
            {
                specs[i] = filter.ToFilterSpec();
                i++;
            }
            return specs;
        }

        /// <summary>Returns <see langword="false"/> if the user cancelled-out of the dialog. Returns <see langword="true"/> if the user completed the dialog. All other cases result in a thrown <see cref="Win32Exception"/> or <see cref="ExternalException"/> depending on the HRESULT returned from <see cref="IModalWindow.Show(IntPtr)"/>.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "ExternalException")]
        public static bool ValidateDialogShowHResult(this HResult dialogHResult)
        {
            if (dialogHResult.TryGetWin32ErrorCode(out Win32ErrorCodes win32Code))
            {
                if (win32Code == Win32ErrorCodes.Success)
                {
                    // OK.
                    return true;
                }
                else if (win32Code == Win32ErrorCodes.ErrorCancelled)
                {
                    // Cancelled
                    return false;
                }
                else
                {
                    // Other Win32 error:

                    string msg = string.Format(CultureInfo.CurrentCulture, "Unexpected Win32 error code 0x{0:X2} in HRESULT 0x{1:X4} returned from IModalWindow.Show(...).", (int)win32Code, (int)dialogHResult);
                    throw new Win32Exception(error: (int)win32Code, message: msg);
                }
            }
            else if (dialogHResult.IsValidHResult())
            {
                const ushort RPC_E_SERVERFAULT = 0x0105;

                if (dialogHResult.GetFacility() == HResultFacility.Rpc && dialogHResult.GetCode() == RPC_E_SERVERFAULT)
                {
                    // This error happens when calling `IModalWindow.Show` instead of using the `Show` method on a different interface, like `IFileOpenDialog.Show`.
                    string msg = string.Format(CultureInfo.CurrentCulture, "Unexpected RPC HRESULT: 0x{0:X4} (RPC Error {1:X2}) returned from IModalWindow.Show(...). This particular RPC error suggests the dialog was accessed via the wrong COM interface.", (int)dialogHResult, RPC_E_SERVERFAULT);
                    throw new ExternalException(msg, errorCode: (int)dialogHResult);
                }
                else
                {
                    // Fall-through to below:
                }
            }
            else
            {
                // Fall-through to below:
            }

            {
                // Other HRESULT (non-Win32 error):
                // https://stackoverflow.com/questions/11158379/how-can-i-throw-an-exception-with-a-certain-hresult

                string msg = string.Format(CultureInfo.CurrentCulture, "Unexpected HRESULT: 0x{0:X4} returned from IModalWindow.Show(...).", (int)dialogHResult);
                throw new ExternalException(msg, errorCode: (int)dialogHResult);
            }
        }
    }
}
