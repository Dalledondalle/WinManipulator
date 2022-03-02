using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace WindowManipulator.Library
{
    public class Settings
    {
        const int ENUM_CURRENT_SETTINGS = -1;
        int screenHeight = 0;
        int screenWidth = 0;
        private List<ProcessSettings> processes;
        private int focusWindowWidth = 0;
        private int focusWindowHeight = 0;
        private int focusWindowX = 0;
        private int focusWindowY = 0;
        private int secWindowHeight = 0;
        private int secWindowWidth = 0;

        public int ScreenHeight { get => screenHeight; private set => screenHeight = value; }
        public int ScreenWidth { get => screenWidth; private set => screenWidth = value; }
        public List<ProcessSettings> Processes { get => processes; private set => processes = value; }

        public Settings()
        {
            processes = new();
            DEVMODE devMode = default;
            devMode.dmSize = (short)Marshal.SizeOf(devMode);
            EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode);

            ScreenHeight = devMode.dmPelsHeight;
            ScreenWidth = devMode.dmPelsWidth;
        }

        public void BringProcessToForeground(Process process)
        {
            if (process is not null)
                if (processes.Any(p => p.Process.Id == process.Id))
                    processes.First(p => p.Process.Id == process.Id).BringProcessToFront();
        }

        public void SetFocusWindowSettings(int width, int height)
        {
            focusWindowWidth = width;
            focusWindowHeight = height;
            focusWindowX = ((ScreenWidth - focusWindowWidth) / 2);
            secWindowHeight = ScreenHeight - focusWindowHeight;
            UpdateSettings();
        }


        private void UpdateSettings()
        {

            if (processes.Count > 0)
                secWindowWidth = focusWindowWidth / processes.Count;
            else
                secWindowWidth = focusWindowWidth;

            TaskBarEdge taskbarEdge = TaskBarEdge.Bottom;
            int taskbarHeight = 0;
            GetTaskBarInfo(out taskbarEdge, out taskbarHeight);

            //Setup all processes
            for (int i = 0; i < Processes.Count; i++)
            {
                int x = focusWindowX + (secWindowWidth * i);
                int y = ScreenHeight - (ScreenHeight - focusWindowHeight);
                processes[i].SetOutOfFocusSettings(secWindowWidth, secWindowHeight - taskbarHeight, x, y);
                processes[i].SetInFocusSettings(focusWindowWidth, focusWindowHeight, focusWindowX, focusWindowY);
            }
            if (processes.Count > 0) processes[0].IsActive = true;

        }

        public void UpdateWindows()
        {
            var activeProcessId = GetActiveWindowProcessID();
            var activeSetting = processes.FirstOrDefault(x => x.IsActive);
            if (processes.Any(x => x.Process.Id == activeProcessId))
            {
                if (activeSetting is not null && activeProcessId != activeSetting.Process.Id)
                {
                    foreach (var item in processes)
                    {
                        if (item.Process.Id == activeProcessId)
                        {
                            item.InFocus();
                            SetCursorPos(ScreenWidth / 2, focusWindowHeight / 2);
                        }
                        else
                        {
                            item.OutOfFocus();
                        }
                    }
                }

            }
        }

        public void ClearProcess()
        {
            processes.Clear();
        }
        public void AddProcess(Process process)
        {
            if (!processes.Any(x => x.Process.Id == process.Id))
                processes.Add(new(process));
            UpdateSettings();
        }

        public void AddProcess(int id)
        {
            if (Process.GetProcesses().Any(p => p.Id == id))
                if (!processes.Any(x => x.Process.Id == id))
                    processes.Add(new(Process.GetProcessById(id)));
            UpdateSettings();
        }

        public void RemoveProcess(Process process)
        {
            processes.RemoveAll(x => x.Process.Id == process.Id);
            UpdateSettings();
        }

        public void RemoveProcesss(int id)
        {
            processes.RemoveAll(x => x.Process.Id == id);
            UpdateSettings();
        }

        public int GetActiveWindowProcessID()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return 0;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId;
        }

        public Process GetActiveWindowProcess()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return null;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return Process.GetProcessById(activeProcId);
        }

        [DllImport("user32.dll")]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [StructLayout(LayoutKind.Sequential)]
        struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        enum ABMsg : int
        {
            ABM_NEW = 0,
            ABM_REMOVE = 1,
            ABM_QUERYPOS = 2,
            ABM_SETPOS = 3,
            ABM_GETSTATE = 4,
            ABM_GETTASKBARPOS = 5,
            ABM_ACTIVATE = 6,
            ABM_GETAUTOHIDEBAR = 7,
            ABM_SETAUTOHIDEBAR = 8,
            ABM_WINDOWPOSCHANGED = 9,
            ABM_SETSTATE = 10
        }

        enum ABEdge : int
        {
            ABE_LEFT = 0,
            ABE_TOP,
            ABE_RIGHT,
            ABE_BOTTOM
        }

        enum ABState : int
        {
            ABS_MANUAL = 0,
            ABS_AUTOHIDE = 1,
            ABS_ALWAYSONTOP = 2,
            ABS_AUTOHIDEANDONTOP = 3,
        }

        private enum TaskBarEdge : int
        {
            Bottom,
            Top,
            Left,
            Right
        }

        private void GetTaskBarInfo(out TaskBarEdge taskBarEdge, out int height)
        {
            APPBARDATA abd = new APPBARDATA();

            height = 0;
            taskBarEdge = TaskBarEdge.Bottom;

            uint ret = SHAppBarMessage((int)ABMsg.ABM_GETTASKBARPOS, ref abd);
            switch (abd.uEdge)
            {
                case (int)ABEdge.ABE_BOTTOM:
                    taskBarEdge = TaskBarEdge.Bottom;
                    height = abd.rc.bottom - abd.rc.top;
                    break;
                case (int)ABEdge.ABE_TOP:
                    taskBarEdge = TaskBarEdge.Top;
                    height = abd.rc.bottom;
                    break;
                case (int)ABEdge.ABE_LEFT:
                    taskBarEdge = TaskBarEdge.Left;
                    height = abd.rc.right - abd.rc.left;
                    break;
                case (int)ABEdge.ABE_RIGHT:
                    taskBarEdge = TaskBarEdge.Right;
                    height = abd.rc.right - abd.rc.left;
                    break;
            }
        }

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);
    }
}