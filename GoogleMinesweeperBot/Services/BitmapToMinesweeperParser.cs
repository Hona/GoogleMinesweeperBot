using GoogleMinesweeperBotClean.Game;
using GoogleMinesweeperBotClean.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace GoogleMinesweeperBotClean.Services
{
    public static class BitmapToMinesweeperParser
    {
        public static Point ChromeTopLeft { get; set; }
        public static Point MinesweeperTopLeft { get; set; }
        public static decimal TileWidth { get; set; }
        private static MinesweeperState _previousState { get; set; } = null;
        public static async Task<MinesweeperState> ParseAsync(Bitmap browserScreenshot, Point chromeTopLeft)
        {

            return await Task.Run(() =>
            {
                ChromeTopLeft = chromeTopLeft;

                // If this fails then try:
                // Check if the chrome://flags/#force-color-profile setting is set to Default, if not, setting to sRGB is one way to fix it.

                try
                {
                    var canvas = browserScreenshot.GetMinesweeperCanvas();

                    canvas.Save($"{Environment.CurrentDirectory}\\minesweeper.png", ImageFormat.Png);

                    var canvasPixels = canvas.ToColoredPointList();

                    var state = GetEmptyMinesweeperState(canvasPixels);
                    state.ParseCanvasPixels(canvasPixels);

                    if (_previousState != null && !state.IsNewState() && (state.HorizontalTiles != _previousState.HorizontalTiles || state.VerticalTiles != _previousState.VerticalTiles))
                    {
                        Logger.LogError("Minesweeper grid changed mid game. Press enter to ignore...");
                        Console.ReadLine();
                    }

                    Logger.LogInfo("Detected Board State:");
                    state.PrintToConsole();

                    browserScreenshot.Dispose();
                    canvas.Dispose();

                    _previousState = state;

                    return state;
                }
                catch (Exception e)
                {
                    Logger.LogWarning("Could not parse minesweeper canvas. Waiting 1.5 seconds before trying again.");
                    Logger.LogException(e);
                    Thread.Sleep(1500);
                    return null;
                }                
            });
        }
        private static void ParseCanvasPixels(this MinesweeperState state, List<ColoredPoint> pixels)
        {
            // Set all clicked tiles
            foreach (var pixel in pixels.Where(x => x.Color == ColorConstants.ClickedDarkSquare || x.Color == ColorConstants.ClickedLightSquare).ToArray())
            {
                state.TryAddPixel(pixel.Point, -1);
            }


            // Set all flagged tiles
            foreach (var pixel in pixels.Where(x => x.Color == ColorConstants.FlagColor).ToArray())
            {
                //state.TryAddPixel(pixel.Point, -2);
            }

            // Add surrounding bomb number tiles
            for (var i = 1; i <= 6; i++)
            {
                var color = ColorConstants.GetColorFromBombCount(i);
                var bombPixels = pixels.Where(x => x.Color == color).ToArray();
                Logger.LogInfo($"Found {bombPixels.Length} pixels that have {i} bombs");

                foreach (var pixel in bombPixels)
                {
                    state.TryAddPixel(pixel.Point, i);
                }
            }

        }
        private static bool TryAddPixel(this MinesweeperState state, Point point, int value)
        {
            var actualX = (int)Math.Floor(point.X / TileWidth);
            var actualY = (int)Math.Floor(point.Y / TileWidth);

            if (actualX >= 0 && actualX < state.HorizontalTiles && actualY >= 0 && actualY < state.VerticalTiles)
            {
                state.Grid[actualX, actualY] = value;
                return true;
            }

            return false;
        }
        //private static decimal GetTilePixelWidth(List<ColoredPoint> canvasPixels)
        //{
        //    var firstColor = canvasPixels.First();
        //    var nextTileFirstPixel = canvasPixels.First(x => x.Color != firstColor.Color);
        //    var tileLength = canvasPixels.IndexOf(nextTileFirstPixel);

        //    // Since arrays start at 0, the index includes the first pixel of the new tile, but is correct
        //    return tileLength;
        //}
        private static MinesweeperState GetEmptyMinesweeperState(List<ColoredPoint> canvasPixels)
        {
            // Get top row
            var topRow = canvasPixels.Where(x => x.Point.Y == 0);
            var topRowColors = topRow.Select(x => x.Color).ToArray();

            // Get the left column
            var leftColumn = canvasPixels.Where(x => x.Point.X == 0);
            var leftColumnColors = leftColumn.Select(x => x.Color).ToArray();

            var horizontalTiles = GetColorGroupsCount(topRowColors);
            var verticalTiles = GetColorGroupsCount(leftColumnColors);

            TileWidth = (decimal)topRow.Count() / horizontalTiles;


            Logger.LogInfo("Horizontal Tiles: " + horizontalTiles);
            Logger.LogInfo("Vertical Tiles: " + verticalTiles);

            return new MinesweeperState(horizontalTiles, verticalTiles);
        }
        private static int GetColorGroupsCount(Color[] color)
        {
            // Only count tile colors
            var colors = color.Where(x => x == ColorConstants.LightSquare
                                       || x == ColorConstants.ClickedLightSquare
                                       || x == ColorConstants.HoveringLightSquare
                                       || x == ColorConstants.HoveringClickedLightSqaure
                                       || x == ColorConstants.DarkSquare
                                       || x == ColorConstants.ClickedDarkSquare
                                       || x == ColorConstants.HoveringDarkSquare
                                       || x == ColorConstants.HoveringClickedDarkSquare).ToArray();

            // Set to 1 since we select the first color
            var output = 1;
            var currentColor = colors.First();

            for (var i = 1; i < colors.Length; i++)
            {
                if (colors[i] != currentColor)
                {
                    output++;
                    currentColor = colors[i];
                }
            }

            return output;
        }

        private static Bitmap GetMinesweeperCanvas(this Bitmap browserScreenshot)
        {
            var coloredPoints = browserScreenshot.ToColoredPointList();

            var potentialTopLeftPoints = new List<ColoredPoint>();
            coloredPoints.TryFindColor(ColorConstants.LightSquare, out var topLeftUnclicked);
            coloredPoints.TryFindColor(ColorConstants.ClickedLightSquare, out var topLeftClicked);
            coloredPoints.TryFindColor(ColorConstants.HoveringLightSquare, out var topLeftHoveringUnclicked);
            coloredPoints.TryFindColor(ColorConstants.HoveringClickedLightSqaure, out var topLeftHoveringClicked);

            potentialTopLeftPoints.Add(topLeftUnclicked);
            potentialTopLeftPoints.Add(topLeftClicked);
            potentialTopLeftPoints.Add(topLeftHoveringClicked);
            potentialTopLeftPoints.Add(topLeftHoveringUnclicked);

            potentialTopLeftPoints = potentialTopLeftPoints
                .Where(x => x != null)
                .OrderBy(x => x.Point.Y)
                .ThenBy(x => x.Point.X)
                .ToList();

            var topLeft = potentialTopLeftPoints.First();

            var potentialBottomRightPoints = new List<ColoredPoint>();
            
            coloredPoints.TryFindColor(ColorConstants.LightSquare, reverseSearch: true, out var bottomRightUnclicked);
            coloredPoints.TryFindColor(ColorConstants.ClickedLightSquare, reverseSearch: true, out var bottomRightClicked);
            coloredPoints.TryFindColor(ColorConstants.HoveringLightSquare, reverseSearch: true, out var bottomRightHoveringUnclicked);
            coloredPoints.TryFindColor(ColorConstants.HoveringClickedLightSqaure, reverseSearch: true, out var bottomRightHoveringClicked);

            potentialBottomRightPoints.Add(bottomRightUnclicked);
            potentialBottomRightPoints.Add(bottomRightClicked);
            potentialBottomRightPoints.Add(bottomRightHoveringUnclicked);
            potentialBottomRightPoints.Add(bottomRightHoveringClicked);

            potentialBottomRightPoints = potentialBottomRightPoints
                .Where(x => x != null)
                .OrderByDescending(x => x.Point.Y)
                .ThenByDescending(x => x.Point.X)
                .ToList();

            var bottomRight = potentialBottomRightPoints.First();

            MinesweeperTopLeft = (topLeft ?? throw new Exception("Could not find minesweeper canvas")).Point;

            var minesweeperCanvas = browserScreenshot.Clone(new Rectangle(topLeft.Point, new Size(width: bottomRight.Point.X - topLeft.Point.X, height: bottomRight.Point.Y - topLeft.Point.Y)), browserScreenshot.PixelFormat);
            Logger.LogInfo($"Minesweeper canvas: TopLeft: {topLeft.Point.ToString()}, BottomRight: {bottomRight.Point.ToString()}");

            return minesweeperCanvas;
        }
        private static bool TryFindColor(this List<ColoredPoint> coloredPoints, Color color, out ColoredPoint colorPosition)
        {
            var output = coloredPoints.TryFindColor(color, reverseSearch: false, out var foundPoint);
            colorPosition = foundPoint;
            return output;
        }

        private static bool TryFindColor(this List<ColoredPoint> coloredPoints, Color color, bool reverseSearch, out ColoredPoint colorPosition)
        {
            var points = coloredPoints.ToArray();

            // Ignore alpha
            var foundPixels = points.Where(x => x.Color.R == color.R
                                                     && x.Color.G == color.G
                                                     && x.Color.B == color.B).ToList();

            foundPixels = foundPixels.OrderBy(x => x.Point.Y).ThenBy(x => x.Point.X).ToList();

            if (reverseSearch)
            {
                foundPixels.Reverse();
            }

            var foundPixel = foundPixels.FirstOrDefault();

            if (foundPixel != null)
            {
                colorPosition = foundPixel;
                return true;
            }

            colorPosition = null;
            return false;
        }
    }
}
