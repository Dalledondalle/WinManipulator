using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinManipulator.FocusBar
{
    class User32
    {
        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        public const int GWL_STYLE = -16;

        public const ulong WS_VISIBLE = 0x10000000L,
            WS_BORDER = 0x00800000L,
            TARGETWINDOW = WS_BORDER | WS_VISIBLE;

        const string DllName = "user32";

        [DllImport(DllName)]
        public static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);
        [DllImport(DllName)]
        public static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);
        [DllImport(DllName)]
        public static extern void GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport(DllName)]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
        [DllImport(DllName)]
        public static extern bool IsIconic(IntPtr handle);
        [DllImport(DllName)]
        public static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [DllImport(DllName)]
        public static extern bool SetForegroundWindow(IntPtr handle);
        [DllImport(DllName)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport(DllName, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);
        [DllImport(DllName, SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hHook);
        [DllImport(DllName, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam);
        [DllImport(DllName, SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    }
}
