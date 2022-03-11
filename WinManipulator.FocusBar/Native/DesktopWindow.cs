using System.Runtime.InteropServices;

namespace WinManipulator.FocusBar
{
    public static class DesktopWindow
    {
        const int ENUM_CURRENT_SETTINGS = -1;

        public static void GetSize(ref int height, ref int width)
        {
            DEVMODE devMode = default;
            devMode.dmSize = (short)Marshal.SizeOf(devMode);
            User32.EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode);

            height = devMode.dmPelsHeight;
            width = devMode.dmPelsWidth;
        }

    }
}
