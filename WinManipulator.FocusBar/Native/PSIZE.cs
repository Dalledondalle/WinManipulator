using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinManipulator.FocusBar
{
    [StructLayout(LayoutKind.Sequential)]
    struct PSIZE
    {
        public int x;
        public int y;
    }
}
