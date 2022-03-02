using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowManipulator.Library;

namespace WinManipulator.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Process> allProcesses;
        private Process selectedProcess;
        private ObservableCollection<Process> selectedProcesses;
        private Process selectedProcessInUse;
        private string focusHeight = "800";
        private string focusWidth = "1600";
        private Settings setting;
        private Task task;
        private Thread thread;
        private CancellationTokenSource cancellationSource;
        private CancellationToken cancellationToken;
        private string isRunning;
        private string processNames;
        private bool scrollLockIsActive;

        public bool ScrollLockIsActive { get => scrollLockIsActive; set => scrollLockIsActive = value; }
        public string ProcessNames { get => processNames; set => processNames = value; }
        public string IsRunning { get => isRunning; set => isRunning = value; }
        public string FocusHeight
        {
            get => focusHeight; set
            {
                UpdateAll();
                focusHeight = value;
            }
        }
        public string FocusWidth
        {
            get => focusWidth; set
            {
                focusWidth = value;
                UpdateAll();
            }
        }
        public ObservableCollection<Process> SelectedProcesses { get => selectedProcesses; set => selectedProcesses = value; }
        public ObservableCollection<Process> AllProcesses { get => allProcesses; set => allProcesses = value; }
        public Process SelectedProcess { get => selectedProcess; set => selectedProcess = value; }
        public Process SelectedProcessInUse { get => selectedProcessInUse; set => selectedProcessInUse = value; }
        public MainWindow()
        {
            DataContext = this;
            setting = new();
            SelectedProcesses = new();
            AllProcesses = new(Process.GetProcesses());
            SelectedProcess = AllProcesses.First();
            InitializeComponent();
        }

        private void UpdateAll()
        {
            setting.SetFocusWindowSettings(int.Parse(FocusWidth), int.Parse(FocusHeight));
            setting.ClearProcess();
            foreach (var item in SelectedProcesses)
            {
                setting.AddProcess(item);
            }
        }

        private void BringToForeground(object sender, RoutedEventArgs e)
        {
            setting.BringProcessToForeground(SelectedProcessInUse);
        }

        private void AddProcess(object sender, RoutedEventArgs e)
        {
            SelectedProcesses.Add(SelectedProcess);
            foreach (var item in SelectedProcesses)
            {
                if (AllProcesses.Any(x => x == item))
                    AllProcesses.Remove(item);
            }
            UpdateAll();
        }

        private void RemoveProcess(object sender, RoutedEventArgs e)
        {
            AllProcesses = new(Process.GetProcesses());
            SelectedProcesses.Remove(SelectedProcessInUse);
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            if (thread is not null && thread.IsAlive)
            {
                cancellationSource.Cancel();
                task.Wait();
            }
            UpdateAll();
            cancellationSource = new();
            cancellationToken = cancellationSource.Token;

            task = Task.Factory.StartNew(() =>
            {
                thread = Thread.CurrentThread;
                while (true)
                {
                    if ((((ushort)GetKeyState(0x91)) & 0xffff) != 0 && !ScrollLockIsActive)
                        ScrollLockIsActive = true;
                    else if ((((ushort)GetKeyState(0x91)) & 0xffff) == 0 && ScrollLockIsActive)
                        scrollLockIsActive = false;

                    if (ScrollLockIsActive)
                        setting.UpdateWindows();
                    if (cancellationSource.IsCancellationRequested)
                    {
                        return;
                    }
                }

            }, cancellationToken);
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            if (thread is not null && thread.IsAlive)
                cancellationSource.Cancel();
        }

        private void AddAll(object sender, RoutedEventArgs e)
        {
            foreach (var item in Process.GetProcessesByName(ProcessNames))
            {
                if (!SelectedProcesses.Any(x => x.Id == item.Id))
                    SelectedProcesses.Add(item);
            }
            UpdateAll();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);
    }
}
