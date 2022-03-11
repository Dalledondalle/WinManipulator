using System;
using System.Diagnostics;
using System.Linq;

namespace WinManipulator.FocusBar
{
    static class ProcessManagement
    {
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        internal static void BringProcessToForeground(Process process)
        {
            if (process is not null)
                if (Process.GetProcesses().Any(p => p.Id == process.Id))
                    BringProcessToFront(Process.GetProcesses().First(p => p.Id == process.Id));
        }
        static void BringProcessToFront(Process process)
        {
            const int SW_RESTORE = 9;
            IntPtr handle = process.MainWindowHandle;
            if (User32.IsIconic(handle))
            {
                User32.ShowWindow(handle, SW_RESTORE);
            }
            User32.SetForegroundWindow(handle);
        }
        public static void SetWindowOfProcessPositionAndSize(Process process, PositionAndSize positionAndSize) => User32.SetWindowPos(process.MainWindowHandle, HWND_TOP, positionAndSize.x, positionAndSize.y, positionAndSize.width, positionAndSize.height, 0);
    }
}
