using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.ComponentModel;
using System.IO;
using System.Windows.Interop;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Input;

namespace WinManipulator.FocusBar
{
    /// <summary>
    /// Interaction logic for Bar.xaml
    /// </summary>
    public partial class Bar : Window, INotifyPropertyChanged
    {
        private int taskbarHeight = 0;
        private int desktopHeight = 0;
        private int desktopWidth = 0;
        private PositionAndSize barSizeAndPosition;
        private PositionAndSize workarea;
        private int windowHeight;
        private int windowWidth;
        private int imageHeight;
        private ThumbPage selectedProcess;
        private Process thisProcess;
        private bool transparent;
        private System.Windows.Media.Brush transparentColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
        private bool onTop = true;

        public System.Windows.Media.Brush TransparentColor { get => transparentColor; set => SetField(ref transparentColor, value); }
        public event PropertyChangedEventHandler PropertyChanged;
        public bool OnTop { get => onTop; set => onTop = value; }
        public bool Transparent
        {
            get { return transparent; }
            set
            {
                if (value == true)
                    TransparentColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                else
                    TransparentColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                SetField(ref transparent, value);
            }
        }
        public bool IsOpen { get; set; } = false;
        public double ListHeight
        {
            get => listHeight; set
            {
                SetField(ref listHeight, value);
            }
        }
        public double ListWidth
        {
            get => listWidth; set
            {
                SetField(ref listWidth, value);
            }
        }
        public ObservableCollection<ThumbPage> ProcessesToWatch { get; set; } = new();
        public ThumbPage SelectedProcess
        {
            get => selectedProcess;
            set
            {
                if (value is not null)
                {
                    SetField(ref selectedProcess, value);
                    ProcessManagement.BringProcessToForeground(value.Process);
                }
            }
        }
        public int ImageHeight { get => imageHeight; set => SetField(ref imageHeight, value); }
        public ProcessToWatch ActiveProcess { get; set; }
        public PositionAndSize Workarea { get => workarea; private set => workarea = value; }
        public PositionAndSize BarSizeAndPosition { get => barSizeAndPosition; private set => barSizeAndPosition = value; }
        public int WindowHeight { get => windowHeight; set => windowHeight = value; }
        public int WindowWidth { get => windowWidth; set => windowWidth = value; }
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        Thread Thread { get; set; }
        private GlobalKeyboardHook _globalKeyboardHook;
        private double listWidth;
        private double listHeight;

        public Bar()
        {
            InitializeComponent();
            DataContext = this;
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            SetupKeyboardHooks();
        }

        public void SetupKeyboardHooks()
        {
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
        }

        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            int keycode = e.KeyboardData.VirtualCode;
            if (!(keycode == (int)VKeys.SPACE || keycode == (int)VKeys.KEY_Z))
                return;
            if (e.KeyboardState != GlobalKeyboardHook.KeyboardState.KeyDown)
                return;

            var winKey = Keyboard.GetKeyStates(Key.LWin);
            if (winKey == KeyStates.None)
                return;

            switch (keycode)
            {
                case (int)VKeys.SPACE:
                    NextWindowInList();
                    break;
                case (int)VKeys.KEY_Z:
                    PreviousWindowInList();
                    break;
                default:
                    break;
            }
            e.Handled = true;
        }

        void PreviousWindowInList()
        {
            //Dispatcher.Invoke(() =>
            //{
            //    if (ProcessesToWatch.Any())
            //    {

            //        int i = ProcessesToWatch.IndexOf(ActiveProcess);
            //        i--;
            //        if (i < 0) i = ProcessesToWatch.Count - 1;
            //        var t = ProcessesToWatch[i];
            //        if (t is not null)
            //        {
            //            ActiveProcess.ImageSource = CaptureScreenshot.OfProcess(ActiveProcess.Process);
            //            ProcessManagement.BringProcessToForeground(t.Process);
            //            ActiveProcess = t;
            //        }
            //        SelectedProcess = null;
            //    }
            //});
        }

        void NextWindowInList()
        {
            //Dispatcher.Invoke(() =>
            //{
            //    if (ProcessesToWatch.Any())
            //    {

            //        int i = ProcessesToWatch.IndexOf(ActiveProcess);
            //        i++;
            //        if (i >= ProcessesToWatch.Count) i = 0;
            //        var t = ProcessesToWatch[i];
            //        if (t is not null)
            //        {
            //            ActiveProcess.ImageSource = CaptureScreenshot.OfProcess(ActiveProcess.Process);
            //            ProcessManagement.BringProcessToForeground(t.Process);
            //            ActiveProcess = t;
            //        }
            //        SelectedProcess = null;
            //    }
            //});
        }


        public void Dispose()
        {
            _globalKeyboardHook?.Dispose();
        }

        public void Setup(PositionAndSize position, Process[] processes)
        {
            Show();
            Activate();
            ProcessesToWatch.Clear();
            taskbarHeight = Taskbar.GetHeight();
            DesktopWindow.GetSize(ref desktopHeight, ref desktopWidth);
            desktopHeight -= taskbarHeight;
            position.x = (desktopWidth - position.width) / 2;
            Workarea = position;

            barSizeAndPosition.width = position.width;
            barSizeAndPosition.height = desktopHeight - Workarea.height;
            barSizeAndPosition.y = Workarea.height;
            barSizeAndPosition.x = Workarea.x;
            this.Left = BarSizeAndPosition.x;
            this.Top = BarSizeAndPosition.y;
            this.Width = BarSizeAndPosition.width;
            this.Height = BarSizeAndPosition.height;
            ListHeight = Height;
            ListWidth = Width;
            foreach (var item in processes)
            {
                //ProcessManagement.BringProcessToForeground(item);
                ProcessManagement.SetWindowOfProcessPositionAndSize(item, Workarea);
            }
            var asda = Process.GetProcesses().Where(x => !string.IsNullOrEmpty(x.MainWindowTitle));
            thisProcess = Process.GetCurrentProcess();
            int widthOfProcess = (int)(Width / processes.Length) - 20;
            for (int i = 0; i < processes.Length; i++)
            {
                //var bm = CaptureScreenshot.OfProcess(item);
                var rect = new Rect() { Top = 0, Left = (widthOfProcess * i), Bottom = (int)Height, Right = ((widthOfProcess * i)) + widthOfProcess };
                var t = new ThumbPage(processes[i], rect, thisProcess);
                ProcessesToWatch.Add(t);
            }
            Topmost = OnTop;
            IsOpen = true;
            //ActiveProcess = ProcessesToWatch.Last();
        }
        private BitmapSource DefaultBitmap()
        {
            FileStream fileStream = new FileStream("example.png", FileMode.Open, FileAccess.Read);
            var img = new System.Windows.Media.Imaging.BitmapImage();

            img.BeginInit();
            img.StreamSource = fileStream;
            img.EndInit();

            return img;
        }

        private void SelectProcess(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var t = (sender as ListBox).SelectedItem as ProcessToWatch;
            if (t is not null)
            {
                ActiveProcess.ImageSource = CaptureScreenshot.OfProcess(ActiveProcess.Process);
                ProcessManagement.BringProcessToForeground(t.Process);
                ActiveProcess = t;
            }
            SelectedProcess = null;
        }


    }

    public struct PositionAndSize
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }

    public static class CaptureScreenshot
    {
        const int SRCCOPY = 0x00CC0020;
        const int CAPTUREBLT = 0x40000000;
        public static Bitmap CaptureWindow(IntPtr hWnd)
        {
            RECT region;

            GetWindowRect(hWnd, out region);

            return CaptureRegion(Rectangle.FromLTRB(region.left, region.top, region.right, region.bottom));
        }
        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int nxDest, int nyDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hdc);
        public static Bitmap CaptureRegion(Rectangle region)
        {
            IntPtr desktophWnd;
            IntPtr desktopDc;
            IntPtr memoryDc;
            IntPtr bitmap;
            IntPtr oldBitmap;
            bool success;
            Bitmap result;

            desktophWnd = GetDesktopWindow();
            desktopDc = GetWindowDC(desktophWnd);
            memoryDc = CreateCompatibleDC(desktopDc);
            bitmap = CreateCompatibleBitmap(desktopDc, region.Width, region.Height);
            oldBitmap = SelectObject(memoryDc, bitmap);

            success = BitBlt(memoryDc, 0, 0, region.Width, region.Height, desktopDc, region.Left, region.Top, SRCCOPY | CAPTUREBLT);

            try
            {
                if (!success)
                {
                    throw new Exception();
                }

                result = System.Drawing.Image.FromHbitmap(bitmap);
            }
            finally
            {
                SelectObject(memoryDc, oldBitmap);
                DeleteObject(bitmap);
                DeleteDC(memoryDc);
                ReleaseDC(desktophWnd, desktopDc);
            }

            return result;
        }
        public static BitmapSource OfProcess(Process process)
        {
            var bm = CaptureScreenshot.CaptureWindow(process.MainWindowHandle);
            bm.SetResolution(640, 480);
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bm.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
            return source;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    public class ProcessToWatch : INotifyPropertyChanged
    {
        public int Height { get => height; set => SetField(ref height, value); }
        public int Width { get => width; set => SetField(ref width, value); }
        private BitmapSource imageSource;
        private int width;
        private int height;

        public BitmapSource ImageSource
        {
            get => imageSource;
            set => SetField(ref imageSource, value);
        }
        public Process Process { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public enum TaskBarEdge : int
    {
        Bottom,
        Top,
        Left,
        Right
    }

    public static class Taskbar
    {
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
        public static int GetHeight()
        {
            APPBARDATA abd = new APPBARDATA();

            int height = 0;

            uint ret = SHAppBarMessage((int)ABMsg.ABM_GETTASKBARPOS, ref abd);
            switch (abd.uEdge)
            {
                case (int)ABEdge.ABE_BOTTOM:
                    height = abd.rc.bottom - abd.rc.top;
                    break;
                case (int)ABEdge.ABE_TOP:
                    height = abd.rc.bottom;
                    break;
                case (int)ABEdge.ABE_LEFT:
                    height = abd.rc.right - abd.rc.left;
                    break;
                case (int)ABEdge.ABE_RIGHT:
                    height = abd.rc.right - abd.rc.left;
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

    public static class DesktopWindow
    {
        const int ENUM_CURRENT_SETTINGS = -1;
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
        public static void GetSize(ref int height, ref int width)
        {
            DEVMODE devMode = default;
            devMode.dmSize = (short)Marshal.SizeOf(devMode);
            EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode);

            height = devMode.dmPelsHeight;
            width = devMode.dmPelsWidth;
        }
        [DllImport("user32.dll")]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
    }
}
