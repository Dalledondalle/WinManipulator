using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinManipulator.FocusBar
{
    public static class Taskbar
    {
        [StructLayout(LayoutKind.Sequential)]
        struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public Rect rc;
            public IntPtr lParam;
        }
        public static int GetHeight()
        {
            APPBARDATA abd = new APPBARDATA();

            int height = 0;

            uint ret = SHAppBarMessage((int)ABMsg.ABM_GETTASKBARPOS, ref abd);
            switch (abd.uEdge)
            {
                case (int)ABEdge.ABE_BOTTOM:
                    height = abd.rc.Bottom - abd.rc.Top;
                    break;
                case (int)ABEdge.ABE_TOP:
                    height = abd.rc.Bottom;
                    break;
                case (int)ABEdge.ABE_LEFT:
                    height = abd.rc.Right - abd.rc.Left;
                    break;
                case (int)ABEdge.ABE_RIGHT:
                    height = abd.rc.Right - abd.rc.Left;
                    break;
            }
            return height;
        }
        public static TaskBarEdge GetPosition()
        {
            APPBARDATA abd = new APPBARDATA();
            TaskBarEdge taskBarEdge = TaskBarEdge.Bottom;

            uint ret = SHAppBarMessage((int)ABMsg.ABM_GETTASKBARPOS, ref abd);
            switch (abd.uEdge)
            {
                case (int)ABEdge.ABE_BOTTOM:
                    taskBarEdge = TaskBarEdge.Bottom;
                    break;
                case (int)ABEdge.ABE_TOP:
                    taskBarEdge = TaskBarEdge.Top;
                    break;
                case (int)ABEdge.ABE_LEFT:
                    taskBarEdge = TaskBarEdge.Left;
                    break;
                case (int)ABEdge.ABE_RIGHT:
                    taskBarEdge = TaskBarEdge.Right;
                    break;
            }
            return taskBarEdge;
        }
        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);
        enum ABEdge : int
        {
            ABE_LEFT = 0,
            ABE_TOP,
            ABE_RIGHT,
            ABE_BOTTOM
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
    }
}
