using System.Runtime.InteropServices;

namespace WinManipulator.FocusBar
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;

        public int Height => Bottom - Top;
    }
}
