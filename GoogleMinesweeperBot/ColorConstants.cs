using System;
using System.Drawing;

namespace GoogleMinesweeperBotClean
{
    public static class ColorConstants
    {
        public static Color CanvasBottomRight => Color.FromArgb(170, 215, 81);
        public static Color LightSquare => Color.FromArgb(170, 215, 81);
        public static Color DarkSquare => Color.FromArgb(162, 209, 73);
        public static Color ClickedLightSquare => Color.FromArgb(229, 194, 159);
        public static Color ClickedDarkSquare => Color.FromArgb(215, 184, 153);
        public static Color FlagColor => Color.FromArgb(242, 54, 7);
        public static Color HoveringLightSquare => Color.FromArgb(191, 225, 125);
        public static Color HoveringDarkSquare => Color.FromArgb(185, 221, 119);
        public static Color HoveringClickedLightSqaure => Color.FromArgb(236, 209, 183);
        public static Color HoveringClickedDarkSquare => Color.FromArgb(225, 202, 179);
        public static Color[] NormalColorBorders => new Color[] 
        {
            Color.FromArgb(161, 205, 75),
            Color.FromArgb(144, 185, 64)
        };
        public static Color GetColorFromBombCount(int count) => count switch
        {
            1 => Color.FromArgb(25, 118, 210),
            2 => Color.FromArgb(56, 142, 60),
            3 => Color.FromArgb(211, 47, 47),
            4 => Color.FromArgb(123, 31, 162),
            5 => Color.FromArgb(255, 143, 0),
            6 => Color.FromArgb(0, 151, 167),
            _ => throw new ArgumentOutOfRangeException(nameof(count)),
        };
    }
}
