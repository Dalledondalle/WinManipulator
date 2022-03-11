using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace WinManipulator.FocusBar
{
    /// <summary>
    /// Interaction logic for Test.xaml
    /// </summary>
    public partial class Test : Window, INotifyPropertyChanged
    {
        private IntPtr selectedWindow;
        private Process thisProcess;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public ObservableCollection<ThumbPage> Pages { get; } = new();
        public ObservableCollection<IntPtr> Ints { get; } = new();

        public IntPtr SelectedWindow
        {
            get { return selectedWindow; }
            set
            {
                selectedWindow = value;
                OnPropertyChanged();
            }
        }



        public Test(Process process, PositionAndSize positionAndSize)
        {
            thisProcess = Process.GetCurrentProcess();
            InitializeComponent();
            DataContext = this;
            Rect rect = new() { Left = positionAndSize.x, Top = positionAndSize.y, Right = positionAndSize.x + positionAndSize.width, Bottom = positionAndSize.y + positionAndSize.height };
            var t = new ThumbPage(process, rect, thisProcess);
            //MainFrame.Content = t;
            Pages.Add(t);
            //SecFrame.Content = Pages[0];
            //Pages.Add(new ThumbPage("Notepad"));
            //_wih = new WindowInteropHelper(this);            
            //Loaded += (s, e) => Init(_wih.Handle, GetRect());
            //SizeChanged += (s, e) => WindowSizeChanged(GetRect());
            //Thumbnails.Add(new Thumbnail("Discord", 200, 150));
            //Thumbnails.Add(new Thumbnail("Notepad", 200, 150));
            //Thumbnails.Add(new Thumbnail("Messenger", 200, 150));
            //SelectedWindow = Thumbnails[0].SelectedWindow;
        }

        internal void Add(Process process, PositionAndSize positionAndSize)
        {
            Rect rect = new() { Left = positionAndSize.x, Top = positionAndSize.y, Right = positionAndSize.x + positionAndSize.width, Bottom = positionAndSize.y + positionAndSize.height };
            Pages.Add(new ThumbPage(process, rect, thisProcess));
        }

        private void SwitchFrame(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //MainFrame.Content = Pages[1];
        }
    }
}
