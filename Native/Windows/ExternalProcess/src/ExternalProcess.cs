using System.ComponentModel;
using System.Runtime.InteropServices;

using static System.NativeMethods;

namespace System
{
    public static class ExternalProcess
    {
        /// <summary>
        /// Start a process
        /// </summary>
        /// <param name="commandLine">The command line to be executed</param>
        /// <param name="currentDirectory">The full path to the current directory for the process. If this parameter is NULL, the new process will have the same current drive and directory as the calling process. (This feature is provided primarily for shells that need to start an application and specify its initial drive and working directory.)</param>
        /// <param name="hidden">Is it a hidden process?</param>
        /// <returns>Process id</returns>
        public static uint Start(string commandLine, string currentDirectory, bool hidden = false)
        {
            ProcessCreationFlags flags = hidden ? ProcessCreationFlags.CREATE_NO_WINDOW : ProcessCreationFlags.NONE;
            STARTUPINFO startupinfo = new STARTUPINFO
            {
                cb = (uint)Marshal.SizeOf<STARTUPINFO>()
            };

            PROCESS_INFORMATION processinfo = new PROCESS_INFORMATION();
            if (!CreateProcessW(null, commandLine, IntPtr.Zero, IntPtr.Zero, false, flags, IntPtr.Zero, currentDirectory, ref startupinfo, ref processinfo))
            {
                throw new Win32Exception();
            }

            return processinfo.dwProcessId;
        }

        /// <summary>
        /// Kill a process
        /// </summary>
        /// <param name="pid">Process id to kill</param>
        /// <returns>Exit code process</returns>
        public static int Kill(uint pid)
        {
            IntPtr handle = OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, pid);

            if (handle == IntPtr.Zero)
            {
                return -1;
            }

            if (!TerminateProcess(handle, 0))
            {
                throw new Win32Exception();
            }

            return !CloseHandle(handle) ? throw new Win32Exception() : 0;
        }
    }
}