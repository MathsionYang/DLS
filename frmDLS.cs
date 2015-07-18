using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DLS
{
    public partial class frmDLS : Form
    {
        Process process = null;
        IntPtr appWin;
        private string exeName = "";
        private string DLSpath = null;

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.StdCall)]

        private static extern long GetWindowThreadProcessId(long hWnd, long lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        [DllImport("user32.dll", SetLastError = true)]
        private static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);


        [DllImport("user32.dll", EntryPoint = "GetWindowLongA", SetLastError = true)]
        private static extern long GetWindowLong(IntPtr hwnd, int nIndex);


        [DllImport("user32.dll", EntryPoint = "SetWindowLongA", SetLastError = true)]
        private static extern long SetWindowLong(IntPtr hwnd, int nIndex, long dwNewLong);

        //private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern long SetWindowPos(IntPtr hwnd, long hWndInsertAfter, long x, long y, long cx, long cy, long wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        [DllImport("user32.dll", EntryPoint = "PostMessageA", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hwnd, uint Msg, long wParam, long lParam);

        private const int SWP_NOOWNERZORDER = 0x200;
        private const int SWP_NOREDRAW = 0x8;
        private const int SWP_NOZORDER = 0x4;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int WS_EX_MDICHILD = 0x40;
        private const int SWP_FRAMECHANGED = 0x20;
        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_ASYNCWINDOWPOS = 0x4000;
        private const int SWP_NOMOVE = 0x2;
        private const int SWP_NOSIZE = 0x1;
        private const int GWL_STYLE = (-16);
        private const int WS_VISIBLE = 0x10000000;
        private const int WM_CLOSE = 0x10;
        private const int WS_CHILD = 0x40000000;

        public string ExeName
        {

            get
            {
                return exeName;
            }

            set
            {
                exeName = value;
            }
        }

        public frmDLS( string path)
        {
            DLSpath = path;
            InitializeComponent();
        }

        private void frmDLS_Load(object sender, EventArgs e)
        {


            this.exeName = DLSpath + "\\DLS\\bin\\bin.x86\\dls.exe";

            try
            {

                // Start the process 
                process = System.Diagnostics.Process.Start(this.exeName);

                // Wait for process to be created and enter idle condition 
                process.WaitForInputIdle();

                // Get the main handle
                appWin = process.MainWindowHandle;

            }

            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error");
            }

            // Put it into this form
            SetParent(appWin, this.Handle);

            // Remove border and whatnot
            //SetWindowLong(appWin, GWL_STYLE, WS_VISIBLE);
            // Move the window to overlay it on this window
            MoveWindow(appWin, -2, -45, 730,300, true);


        }

        private void frmDLS_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                process.Kill();
            }
            catch { }
        }

        private void frmDLS_Resize(object sender, EventArgs e)
        {
            if (this.appWin != IntPtr.Zero)
            {
                MoveWindow(appWin, -2, -45, 730, 300, true);
            }

            //base.OnResize(e);
        }
    }
}
