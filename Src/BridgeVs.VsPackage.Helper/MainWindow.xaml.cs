using System;
using System.Collections.Generic;
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
using Win32Interop.WinHandles;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private System.Windows.Forms.Panel _panel;
        private Process _process;
        System.Windows.Forms.Integration.WindowsFormsHost windowsFormsHost1 =
      new System.Windows.Forms.Integration.WindowsFormsHost();
        public Window1()
        {
            InitializeComponent(); 
            _panel = new System.Windows.Forms.Panel();
            _panel.Width = 1200;
            _panel.Height = 800;
            windowsFormsHost1.Child = _panel; 
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        private static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        [DllImport("user32")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        IntPtr hwnd;

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            button1.Visibility = Visibility.Hidden;
            this.grid1.Children.Add(windowsFormsHost1);

            ProcessStartInfo psi = new ProcessStartInfo(@"C:\Program Files (x86)\LINQPad5\LINQPad.exe");
            _process = Process.Start(psi);
            _process.WaitForInputIdle();

            var winHandle = TopLevelWindowUtils.FindWindow(a => a.GetWindowText().Equals("LINQPad 5"));
            while (winHandle.RawPtr == IntPtr.Zero)
            {
                Thread.Yield();
                winHandle = TopLevelWindowUtils.FindWindow(a => a.GetWindowText().Equals("LINQPad 5"));
            }
            hwnd = winHandle.RawPtr;
            SetParent(hwnd, _panel.Handle);

            // remove control box
            int style = GetWindowLong(hwnd, GWL_STYLE);
            style = style & ~WS_CAPTION & ~WS_THICKFRAME;
            SetWindowLong(hwnd, GWL_STYLE, style);

            // resize embedded application & refresh
            ResizeEmbeddedApp();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_process != null)
            {
                _process.Refresh();
                _process.Close();
            }
        }

        private void ResizeEmbeddedApp()
        { 
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, (int)_panel.ClientSize.Width, (int)_panel.ClientSize.Height, SWP_NOZORDER | SWP_NOACTIVATE);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = base.MeasureOverride(availableSize);
            ResizeEmbeddedApp();
            return size;
        } 
    }
}
