using System;
using System.Runtime.InteropServices;

namespace WinManipulator.FocusBar
{
    class Kernel32
    {
        const string DllName = "kernel32.dll";
        [DllImport(DllName)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport(DllName, CharSet = CharSet.Auto)]
        public static extern bool FreeLibrary(IntPtr hModule);
    }
}
