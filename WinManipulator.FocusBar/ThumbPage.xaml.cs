using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace WinManipulator.FocusBar
{
    /// <summary>
    /// Interaction logic for ThumbPage.xaml
    /// </summary>
    public partial class ThumbPage : Page
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public Process Process { get => _process; }
        public ObservableCollection<KeyValuePair<string, IntPtr>> Windows { get; } = new();
        IntPtr _selectedWindow = IntPtr.Zero;
        IntPtr _targetHandle, _thumbHandle;
        Rect _targetRect;
        readonly WindowInteropHelper _wih;
        public string WindowToWatch { get; private set; }
        private Process processToLoadOn;
        private Process _process;
        private Rect rect;
        private double heightOfProcess;
        private double widthOfProcess;

        public double HeightOfProcess
        {
            get => heightOfProcess; set
            {
                heightOfProcess = value;
                OnPropertyChanged();
            }
        }
        public double WidthOfProcess
        {
            get => widthOfProcess; set
            {
                widthOfProcess = value;
                OnPropertyChanged();
            }
        }

        public ThumbPage(string ProcessName)
        {
            WindowToWatch = ProcessName;
            InitializeComponent();
            //_wih = new WindowInteropHelper(this);
            Loaded += (s, e) => Init(Process.GetCurrentProcess().MainWindowHandle, GetRect());
            SizeChanged += (s, e) => WindowSizeChanged(GetRect());
            _process = Process.GetProcessesByName(ProcessName).Where(x => !string.IsNullOrEmpty(x.MainWindowTitle)).FirstOrDefault();
        }

        public ThumbPage(int ProcessId)
        {
            InitializeComponent();
            //_wih = new WindowInteropHelper(this);
            Loaded += (s, e) => Init(Process.GetCurrentProcess().MainWindowHandle, GetRect());
            SizeChanged += (s, e) => WindowSizeChanged(GetRect());
            _process = Process.GetProcessById(ProcessId);
        }

        public ThumbPage(Process Process, Rect Rect, Process toLoadOn)
        {
            InitializeComponent();
            DataContext = this;
            processToLoadOn = toLoadOn;
            //_wih = new WindowInteropHelper(this);
            rect = Rect;
            Loaded += (s, e) => Init(toLoadOn.MainWindowHandle, GetRect());
            SizeChanged += (s, e) => WindowSizeChanged(GetRect());
            _process = Process;
            HeightOfProcess = Rect.Bottom - Rect.Top;
            WidthOfProcess = Rect.Right - Rect.Left;
            rect.Top += 5;
            rect.Left += 10;
            rect.Bottom -= 5;
            rect.Right -= 10;
        }

        public IntPtr SelectedWindow
        {
            get { return _selectedWindow; }
            set
            {
                if (value == IntPtr.Zero)
                    return;

                _selectedWindow = value;

                if (_thumbHandle != IntPtr.Zero)
                    DWMApi.DwmUnregisterThumbnail(_thumbHandle);

                if (DWMApi.DwmRegisterThumbnail(_targetHandle, SelectedWindow, out _thumbHandle) == 0)
                    Update();

                OnPropertyChanged();
            }
        }

        public void Update()
        {
            if (_thumbHandle == IntPtr.Zero)
                return;

            DWMApi.DwmQueryThumbnailSourceSize(_thumbHandle, out PSIZE size);

            var props = new DWM_THUMBNAIL_PROPERTIES
            {
                fVisible = true,
                dwFlags = DWMApi.DWM_TNP_VISIBLE | DWMApi.DWM_TNP_RECTDESTINATION | DWMApi.DWM_TNP_OPACITY,
                opacity = 255,
                rcDestination = _targetRect
            };

            if (size.x < _targetRect.Width)
                props.rcDestination.Right = props.rcDestination.Left + size.x;

            if (size.y < _targetRect.Height)
                props.rcDestination.Bottom = props.rcDestination.Top + size.y;

            DWMApi.DwmUpdateThumbnailProperties(_thumbHandle, ref props);
        }

        void RefreshWindows()
        {
            if (_thumbHandle != IntPtr.Zero)
                DWMApi.DwmUnregisterThumbnail(_thumbHandle);

            Windows.Clear();

            User32.EnumWindows((hwnd, e) =>
            {
                if (_targetHandle != hwnd && (User32.GetWindowLongA(hwnd, User32.GWL_STYLE) & User32.TARGETWINDOW) == User32.TARGETWINDOW)
                {
                    var sb = new StringBuilder(100);
                    User32.GetWindowText(hwnd, sb, sb.Capacity);

                    var text = sb.ToString();

                    if (!string.IsNullOrWhiteSpace(text))
                        Windows.Add(new KeyValuePair<string, IntPtr>(text, hwnd));
                }

                return true;
            }, 0);

            SelectedWindow = _process.MainWindowHandle;
            //if(Windows.Any(x => x.Value == _process.MainWindowHandle))
            //    SelectedWindow = Windows.First(x => x.Value == _process.MainWindowHandle).Value;
            //else if (Windows.Count > 0)
            //    SelectedWindow = Windows[0].Value;
        }

        Rect GetRect()
        {
            //return new Rect(0, 0, 800, 450);
            return rect;
        }

        void Init(IntPtr Target, Rect Location)
        {
            _targetHandle = Target;
            _targetRect = Location;

            RefreshWindows();
        }

        public void WindowSizeChanged(Rect Location)
        {
            _targetRect = Location;

            Update();
        }
    }
}
