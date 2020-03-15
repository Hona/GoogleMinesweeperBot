using GoogleMinesweeperBotClean.Logging;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace GoogleMinesweeperBotClean.Services
{
    public class MouseControlService
    {
        private static object _lock = new object();
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private static DateTime _lastClicked = DateTime.Now;
        public static int MouseClickDelay { get; set; } = 55;

        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);
        public static Point GetScreenPosition(Point minesweeperTile)
        {
            return new Point
            {
                X = (int)(BitmapToMinesweeperParser.ChromeTopLeft.X + BitmapToMinesweeperParser.MinesweeperTopLeft.X + minesweeperTile.X * BitmapToMinesweeperParser.TileWidth + (int)((decimal)0.5 * BitmapToMinesweeperParser.TileWidth)),
                Y = (int)(BitmapToMinesweeperParser.ChromeTopLeft.Y + BitmapToMinesweeperParser.MinesweeperTopLeft.Y + minesweeperTile.Y * BitmapToMinesweeperParser.TileWidth + (int)((decimal)0.5 * BitmapToMinesweeperParser.TileWidth))
            };
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }
        public static void TryDelay(int millis)
        {
            var timeSinceLastClick = DateTime.Now - _lastClicked;
            var attemptedDelay = millis - (int)timeSinceLastClick.TotalMilliseconds;
            var realDelay = attemptedDelay <= 1 ? millis : attemptedDelay;
            Logger.LogInfo($"Sleeping for {realDelay}ms");
            Thread.Sleep(realDelay);
            _lastClicked = DateTime.Now;
        }
        public static void TryDelay() => TryDelay(MouseClickDelay);
        public static void DoMouseClick(int x, int y)
        {
            lock (_lock)
            {
                SetCursorPos(x, y);
                TryDelay();
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
        }
        public static void DoMouseRightClick(int x, int y)
        {
            lock (_lock)
            {
                SetCursorPos(x, y);
                TryDelay();
                mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            }
        }
    }
}
