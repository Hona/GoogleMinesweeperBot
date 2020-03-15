using System.Runtime.InteropServices;

namespace GoogleMinesweeperBotClean.User32
{
    public static partial class User32Utils
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct WindowsRectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
