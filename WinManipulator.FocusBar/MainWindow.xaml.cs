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
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading;

namespace WinManipulator.FocusBar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Process> allProcesses;
        private Process selectedProcess;
        private ObservableCollection<Process> selectedProcesses;
        private Process selectedProcessInUse;
        private int focusHeight = 800;
        private int focusWidth = 1600;
        private bool scrollLockIsActive;
        private string processNames;
        private bool transparent = true;
        private bool onTop = true;
        private Test test;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        public bool OnTop { get => onTop; set => SetField(ref onTop, value); }
        public bool Transparent { get => transparent; set => SetField(ref transparent, value); }
        public string FocusHeight
        {
            get => focusHeight.ToString(); set
            {
                if (!int.TryParse(value, out focusHeight))
                    focusHeight = 800;
                //UpdateBar();
            }
        }
        public string FocusWidth
        {
            get => focusWidth.ToString(); set
            {
                if (!int.TryParse(value, out focusWidth))
                    focusWidth = 1600;
                //UpdateBar();
            }
        }
        public string ProcessNames { get => processNames; set => processNames = value; }
        public bool ScrollLockIsActive { get => scrollLockIsActive; set => scrollLockIsActive = value; }
        public ObservableCollection<Process> SelectedProcesses { get => selectedProcesses; set => selectedProcesses = value; }
        public ObservableCollection<Process> AllProcesses { get => allProcesses; set => allProcesses = value; }
        public Process SelectedProcess { get => selectedProcess; set => SetField(ref selectedProcess, value); }
        public Process SelectedProcessInUse { get => selectedProcessInUse; set => SetField(ref selectedProcessInUse, value); }
        Bar Bar { get; set; } = new();

        public MainWindow()
        {
            DataContext = this;
            SelectedProcesses = new();
            AllProcesses = new(Process.GetProcesses().Where(x => x.MainWindowTitle.Any()));
            SelectedProcess = AllProcesses.First();
            InitializeComponent();
            this.Closed += new EventHandler(MainWindow_Closed);
        }

        private void OpenBar(object sender, RoutedEventArgs e)
        {
            if(Bar.IsOpen)
                Bar.Close();
            Bar = new();
            Bar.Show();
            PositionAndSize positionAndSize = new PositionAndSize();
            positionAndSize.height = focusHeight;
            positionAndSize.width = focusWidth;
            Bar.Transparent = Transparent;
            Bar.OnTop = OnTop;
            Bar.Setup(positionAndSize, SelectedProcesses.ToArray());
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void TextBox_GotFocus(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            if (tb != null)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }

        protected void MainWindow_Closed(object sender, EventArgs args)
        {
            App.Current.Shutdown();
        }

        private void CloseBar(object sender, RoutedEventArgs e)
        {
            foreach (var item in SelectedProcesses)
            {
            if (test is null)
                test = new(item, new PositionAndSize() { x= 0, y= 5, width = 200, height = 150});
            else
                test.Add(item, new PositionAndSize() { x = test.Pages.Count * 200, y = 5, width = 200, height = 150 });
            }
            
            test.Show();

            //Bar.Close();
        }

        private void BringToForeground(object sender, RoutedEventArgs e)
        {
            ProcessManagement.BringProcessToForeground(SelectedProcessInUse);
        }

        private void RemoveProcess(object sender, RoutedEventArgs e)
        {
            AllProcesses = new(Process.GetProcesses());
            SelectedProcesses.Remove(SelectedProcessInUse);
        }

        private void AddAll(object sender, RoutedEventArgs e)
        {
            foreach (var item in Process.GetProcessesByName(ProcessNames))
            {
                if (!SelectedProcesses.Any(x => x.Id == item.Id))
                    SelectedProcesses.Add(item);
            }
            //UpdateBar();
        }

        private void AddProcess(object sender, RoutedEventArgs e)
        {
            SelectedProcesses.Add(SelectedProcess);
            foreach (var item in SelectedProcesses)
            {
                if (AllProcesses.Any(x => x == item))
                    AllProcesses.Remove(item);
            }
            SelectedProcess = AllProcesses.FirstOrDefault();
            //UpdateBar();
        }

        private void AddProcess(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectedProcesses.Add(SelectedProcess);
                foreach (var item in SelectedProcesses)
                {
                    if (AllProcesses.Any(x => x == item))
                        AllProcesses.Remove(item);
                }
                SelectedProcess = AllProcesses.FirstOrDefault();
                //UpdateBar();
            }
        }

        private void RemoveProcess(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                AllProcesses = new(Process.GetProcesses());
                SelectedProcesses.Remove(SelectedProcessInUse);
            }
        }

        private void RemoveProcessFromCombo(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AllProcesses = new(Process.GetProcesses());
                SelectedProcesses.Remove(SelectedProcessInUse);
            }
        }

        private void AddAllProcesses(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                foreach (var item in Process.GetProcessesByName(ProcessNames))
                {
                    if (!SelectedProcesses.Any(x => x.Id == item.Id))
                        SelectedProcesses.Add(item);
                }
                //UpdateBar();
            }
        }

        private void RefreshProcesses(object sender, RoutedEventArgs e)
        {
            AllProcesses = new(Process.GetProcesses().Where(x => x.MainWindowTitle.Any()));
            foreach (var item in SelectedProcesses)
            {
                if (AllProcesses.Any(x => x.Id == item.Id))
                {
                    var t = AllProcesses.First(x => x.Id == item.Id);
                    AllProcesses.Remove(t);
                }
            }
        }

        private void CheckBoxHandler(object sender, RoutedEventArgs e)
        {
            Transparent = !Transparent;
        }


    }

    public static class ProcessManagement
    {
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        internal static void BringProcessToForeground(Process process)
        {
            if (process is not null)
                if (Process.GetProcesses().Any(p => p.Id == process.Id))
                    BringProcessToFront(Process.GetProcesses().First(p => p.Id == process.Id));
        }
        static void BringProcessToFront(Process process)
        {
            const int SW_RESTORE = 9;
            IntPtr handle = process.MainWindowHandle;
            if (IsIconic(handle))
            {
                ShowWindow(handle, SW_RESTORE);
            }

            SetForegroundWindow(handle);
        }
        [DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public static void SetWindowOfProcessPositionAndSize(Process process, PositionAndSize positionAndSize)
        {
            SetWindowPos(process.MainWindowHandle, HWND_TOP, positionAndSize.x, positionAndSize.y, positionAndSize.width, positionAndSize.height, 0);
        }
    }
}
