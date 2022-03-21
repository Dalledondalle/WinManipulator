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
        private int taskbarHeight, desktopHeight, desktopWidth;        
        private PositionAndSize barSizeAndPosition, workarea;
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
                    TransparentColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 1, 1, 1));
                else
                    TransparentColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                SetField(ref transparent, value);
            }
        }
        public bool IsOpen { get; set; } = false;
        public ObservableCollection<ThumbPage> ProcessesToWatch { get; set; } = new();
        public ThumbPage SelectedProcess
        {
            get => selectedProcess;
            set
            {
                if (value is not null)
                    SetField(ref selectedProcess, value);
            }
        }
        public PositionAndSize Workarea { get => workarea; private set => workarea = value; }
        public PositionAndSize BarSizeAndPosition { get => barSizeAndPosition; private set => barSizeAndPosition = value; }
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        private GlobalKeyboardHook _globalKeyboardHook;

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
            if ((Keyboard.GetKeyStates(Key.LWin) & KeyStates.Down) == 0)
                return;

            int keycode = e.KeyboardData.VirtualCode;
            if (!(keycode == (int)VKeys.SPACE || keycode == (int)VKeys.KEY_Z))
                return;
            if (e.KeyboardState != KeyboardState.KeyDown)
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
            //while(Keyboard.GetKeyStates(Key.LWin) != KeyStates.None)
            //{
            //    User32.keybd_event(0x5B, 0, 0, 0);
            //    //User32.keybd_event(0x5B, 0, 2, 0);
            //}            
        }

        void PreviousWindowInList()
        {
            Dispatcher.Invoke(() =>
            {
                if (ProcessesToWatch.Any())
                {

                    int i = ProcessesToWatch.IndexOf(SelectedProcess);
                    i--;
                    if (i < 0) i = ProcessesToWatch.Count - 1;
                    var t = ProcessesToWatch[i];
                    if (t is not null)
                    {
                        ProcessManagement.BringProcessToForeground(t.Process);
                        SelectedProcess = t;
                    }
                }
            });
        }

        void NextWindowInList()
        {
            Dispatcher.Invoke(() =>
            {
                if (ProcessesToWatch.Any())
                {

                    int i = ProcessesToWatch.IndexOf(SelectedProcess);
                    i++;
                    if (i >= ProcessesToWatch.Count) i = 0;
                    var t = ProcessesToWatch[i];
                    if (t is not null)
                    {
                        ProcessManagement.BringProcessToForeground(t.Process);
                        SelectedProcess = t;
                    }
                }
            });
        }

        public void Dispose()
        {
            _globalKeyboardHook?.Dispose();
        }

        private void SetVariables(PositionAndSize position)
        {
            taskbarHeight = Taskbar.GetHeight();
            DesktopWindow.GetSize(ref desktopHeight, ref desktopWidth);
            desktopHeight -= taskbarHeight;
            position.x = (desktopWidth - position.width) / 2;
            Workarea = position;
        }

        private void SetBarSizeAndPosition()
        {
            barSizeAndPosition.width = Workarea.width;
            barSizeAndPosition.height = desktopHeight - Workarea.height;
            barSizeAndPosition.y = Workarea.height;
            barSizeAndPosition.x = Workarea.x;
        }

        private void SetWindowProperties()
        {
            Left = BarSizeAndPosition.x;
            Top = Workarea.height;
            Width = BarSizeAndPosition.width;
            Height = BarSizeAndPosition.height;
        }

        private void ResizeAllWindowsToProcesses(Process[] processes)
        {
            foreach (var item in processes)
            {
                ProcessManagement.SetWindowOfProcessPositionAndSize(item, Workarea);
            }
        }

        private void MakeProcessWindows(Process[] processes)
        {
            int widthOfProcess = (int)(Width / processes.Length) - 20;
            for (int i = 0; i < processes.Length; i++)
            {
                Rect rect = new Rect() { Top = 0, Left = (widthOfProcess * i) + 10 + (10 * i) - i, Bottom = (int)Height, Right = ((widthOfProcess * i)) + widthOfProcess + 10 + (10 * i) - i };
                ProcessesToWatch.Add(new ThumbPage(processes[i], rect, thisProcess));
            }
        }

        private void StartSetup()
        {
            Show();
            Activate();
            ProcessesToWatch.Clear();
        }

        public void Setup(PositionAndSize position, Process[] processes)
        {
            StartSetup();
            SetVariables(position);
            SetBarSizeAndPosition();
            SetWindowProperties();
            ResizeAllWindowsToProcesses(processes);
            thisProcess = Process.GetCurrentProcess();
            MakeProcessWindows(processes);
            EndSetup();
        }

        private void EndSetup()
        {
            Height = ProcessesToWatch.First().WidthOfProcess / 1.778;
            Top = Workarea.height;
            Topmost = OnTop;
            IsOpen = true;
        }

        private void SelectProcess(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var t = (sender as ListBox).SelectedItem as ThumbPage;
            if (t is not null)
            {
                ProcessManagement.BringProcessToForeground(t.Process);
            }
            SelectedProcess = null;
        }
    }
}
