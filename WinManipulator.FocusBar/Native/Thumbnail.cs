using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;


namespace WinManipulator.FocusBar
{
    public class Thumbnail : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public Process Process { get; private set; }
        ObservableCollection<IntPtr> Windows { get; } = new();
        IntPtr _selectedWindow = IntPtr.Zero;
        IntPtr _targetHandle, _thumbHandle;
        Rect _targetRect;
        public int Height { get; set; }
        public int Width { get; set; }
        readonly WindowInteropHelper _wih;
        public Thumbnail(string ProcessName, int Height, int Width)
        {
            this.Height = Height;
            this.Width = Width;
            //_wih = new WindowInteropHelper(this);
            //Loaded += (s, e) => Init(_wih.Handle, GetRect());
            //SizeChanged += (s, e) => WindowSizeChanged(GetRect());
            Process = Process.GetProcessesByName(ProcessName)[0];
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
                        Windows.Add(hwnd);
                }

                return true;
            }, 0);

            if (Windows.Count > 0)
                SelectedWindow = Windows[0];
        }

        Rect GetRect()
        {
            return new Rect(5, 40, (int)Width - 20, (int)Height - 50);
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
