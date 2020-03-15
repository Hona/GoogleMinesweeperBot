using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace GoogleMinesweeperBotClean.Services
{
    public static class BitmapService
    {
        public static BitmapData LockAllBits(this Bitmap bitmap) => bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, bitmap.PixelFormat);
        public static List<ColoredPoint> ToColoredPointList(this Bitmap bitmap)
        {
            var output = new List<ColoredPoint>();

            var data = bitmap.LockAllBits();

            // Get byte width per pixel depending on the format
            var pixelWidth = data.PixelFormat == PixelFormat.Format24bppRgb ? 3 :
                       data.PixelFormat == PixelFormat.Format32bppArgb ? 4 : 4;
            unsafe
            {
                for (var y = 0; y < data.Height; ++y)
                {
                    var currentPixelPointer = (byte*)data.Scan0.ToPointer() + y * data.Stride;
                    for (var x = 0; x < data.Width; ++x)
                    {

                        // windows stores images in BGR pixel order
                        var r = currentPixelPointer[2];
                        var g = currentPixelPointer[1];
                        var b = currentPixelPointer[0];

                        output.Add(new ColoredPoint
                        {
                            Point = new Point(x, y),
                            Color = Color.FromArgb(r, g, b)
                        });

                        // next pixel in the row
                        currentPixelPointer += pixelWidth;
                    }
                }
            }

            bitmap.UnlockBits(data);

            // TODO: Probably safe to remove the order by's
            return output.OrderBy(x => x.Point.Y).ThenBy(x => x.Point.X).ToList();
        }
    }
}
