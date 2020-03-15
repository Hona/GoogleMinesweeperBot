using System;
using System.Runtime.InteropServices;

namespace GoogleMinesweeperBotClean.User32
{
    public static partial class User32Utils
    {

        [DllImport("USER32.DLL")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref WindowsRectangle rectangle);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void BringToFront(IntPtr handle)
        {
            // Verify that the handle is a running process.
            if (handle == IntPtr.Zero)
            {
                return;
            }

            // Make Calculator the foreground application
            SetForegroundWindow(handle);
        }
        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();
        public static void BringConsoleToFront()
        {
            var pointer = GetConsoleWindow();
            BringToFront(pointer);
        }
    }
}
