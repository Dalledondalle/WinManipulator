

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WindowManipulator.Library
{
    public class ProcessSettings
    {
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        public Process Process { get; private set; }
        public bool IsActive { get; set; } = false;
        private int outOfFocusX = 0;
        private int outOfFocusY = 0;
        private int outOfFocusWidth = 0;
        private int outOfFocusHeight = 0;

        private int inFocusX = 0;
        private int inFocusY = 0;
        private int inFocusWidth = 0;
        private int inFocusHeight = 0;
        public ProcessSettings(Process process)
        {
            Process = process;
        }

        public void SetOutOfFocusSettings(int width, int height, int xPosition, int yPosition)
        {
            outOfFocusHeight = height;
            outOfFocusWidth = width;
            outOfFocusX = xPosition;
            outOfFocusY = yPosition;
        }
        public void SetInFocusSettings(int width, int height, int xPosition, int yPosition)
        {
            inFocusHeight = height;
            inFocusWidth = width;
            inFocusX = xPosition;
            inFocusY = yPosition;
        }
        public void InFocus()
        {
            SetWindowPos(Process.MainWindowHandle, HWND_TOP, inFocusX, inFocusY, inFocusWidth, inFocusHeight, 0);
            IsActive = true;
        }
        public void OutOfFocus()
        {
            SetWindowPos(Process.MainWindowHandle, HWND_TOP, outOfFocusX, outOfFocusY, outOfFocusWidth, outOfFocusHeight, 0);
            IsActive = false;
        }
        private void SetWindowSettings(int width, int height, int xPosition, int yPosition)
        {
            SetWindowPos(Process.MainWindowHandle, HWND_TOP, xPosition, yPosition, width, height, 0);
        }
        public void BringProcessToFront()
        {
            IntPtr handle = Process.MainWindowHandle;
            if (IsIconic(handle))
            {
                ShowWindow(handle, SW_RESTORE);
            }

            SetForegroundWindow(handle);
        }


        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        const int SW_RESTORE = 9;

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);
    }
}
