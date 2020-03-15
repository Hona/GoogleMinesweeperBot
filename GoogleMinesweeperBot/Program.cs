//#define STEP
using GoogleMinesweeperBotClean.Services;
using System.Threading.Tasks;
using System;
using GoogleMinesweeperBotClean.User32;
using GoogleMinesweeperBotClean.Logging;
using System.Threading;

namespace GoogleMinesweeperBotClean
{
    internal class Program
    {
        internal static void Main() => MainAsync().GetAwaiter().GetResult();
        internal static async Task MainAsync()
        {
            Console.Clear();
            var browserImageCaptureService = new BrowserImageCaptureService(browserProcessName: "chrome"/*"MicrosoftEdgeCP"*/);
            var bombFlagService = new BombFlagService();

            while (true)
            {
                // TODO: Allow the browser to be manually specified

                var chromeBmp = await browserImageCaptureService.GetBrowserWindowScreenshotAsync();
                var minesweeperState = await BitmapToMinesweeperParser.ParseAsync(chromeBmp, browserImageCaptureService.ChromeTopLeft);

                if (minesweeperState != null)
                {
                    var probabilities = MinesweeperBotService.GetProbabilities(minesweeperState);

#if STEP
                    Console.ReadLine();
#endif

                    bombFlagService.FlagBombs(probabilities, minesweeperState);
                    
                    var choice = MinesweeperBotService.GetBestChoice(probabilities, minesweeperState);

                    foreach (var bestChoice in choice)
                    {
                        var clickPosition = MouseControlService.GetScreenPosition(bestChoice);

                        MouseControlService.DoMouseClick(clickPosition.X, clickPosition.Y);
                    }

                    User32Utils.BringConsoleToFront();
                }

#if STEP
                Console.ReadLine();
#endif

            }
        }
    }
}
