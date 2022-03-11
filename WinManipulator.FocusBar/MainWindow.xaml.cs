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
        private int focusHeight = 1080;
        private int focusWidth = 2440;
        private string processNames;
        private bool transparent = true;
        private bool onTop = true;
        private Bar bar;

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
                    focusHeight = 1080;
            }
        }
        public string FocusWidth
        {
            get => focusWidth.ToString(); set
            {
                if (!int.TryParse(value, out focusWidth))
                    focusWidth = 2440;
            }
        }
        public string ProcessNames { get => processNames; set => processNames = value; }
        public ObservableCollection<Process> SelectedProcesses { get => selectedProcesses; set => selectedProcesses = value; }
        public ObservableCollection<Process> AllProcesses { get => allProcesses; set => allProcesses = value; }
        public Process SelectedProcess { get => selectedProcess; set => SetField(ref selectedProcess, value); }
        public Process SelectedProcessInUse { get => selectedProcessInUse; set => SetField(ref selectedProcessInUse, value); }

        public MainWindow()
        {
            bar = new();
            SelectedProcesses = new();
            AllProcesses = new(Process.GetProcesses().Where(x => x.MainWindowTitle.Any()));
            SelectedProcess = AllProcesses.First();
            InitializeComponent();
            DataContext = this;
            this.Closed += new EventHandler(MainWindow_Closed);
        }

        private void OpenBar(object sender, RoutedEventArgs e)
        {
            if (!SelectedProcesses.Any())
                return;
            if(bar.IsOpen)
                bar.Close();
            bar = new();
            bar.Show();
            PositionAndSize positionAndSize = new PositionAndSize();
            positionAndSize.height = focusHeight;
            positionAndSize.width = focusWidth;
            bar.Transparent = Transparent;
            bar.OnTop = OnTop;
            bar.Setup(positionAndSize, SelectedProcesses.ToArray());
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) => (sender as TextBox).SelectAll();        

        private void TextBox_GotFocus(object sender, MouseButtonEventArgs e) => (sender as TextBox).SelectAll();        

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

        protected void MainWindow_Closed(object sender, EventArgs args) => App.Current.Shutdown();        

        private void CloseBar(object sender, RoutedEventArgs e) => bar.Close();        

        private void BringToForeground(object sender, RoutedEventArgs e) => ProcessManagement.BringProcessToForeground(SelectedProcessInUse);        

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
        }

        private void AddProcess(object sender, RoutedEventArgs e)
        {
            SelectedProcesses.Add(SelectedProcess);
            SyncProcesses();
            SelectedProcess = AllProcesses.FirstOrDefault();
        }

        private void SyncProcesses()
        {
            AllProcesses.Clear();
            foreach (var item in Process.GetProcesses().Where(x => x.MainWindowTitle.Any()))
            {
                if(!SelectedProcesses.Any(x => x.MainWindowTitle == item.MainWindowTitle))
                    AllProcesses.Add(item);
            }
            SelectedProcess = AllProcesses.FirstOrDefault();
        }

        private void AddProcess(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectedProcesses.Add(SelectedProcess);
                SyncProcesses();
                SelectedProcess = AllProcesses.FirstOrDefault();
            }
        }

        private void RemoveProcess(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {                
                SelectedProcesses.Remove(SelectedProcessInUse);
                SyncProcesses();
            }
        }

        private void RemoveProcessFromCombo(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectedProcesses.Remove(SelectedProcessInUse);
                SyncProcesses();
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
                SyncProcesses();
            }
        }

        private void RefreshProcesses(object sender, RoutedEventArgs e)
        {
            SyncProcesses();
        }
    }    
}
