using GoogleMinesweeperBotClean.Logging;
using GoogleMinesweeperBotClean.User32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleMinesweeperBotClean.Services
{
    public class BrowserImageCaptureService
    {
        public Process Browser { get; set; }
        public Point ChromeTopLeft { get; set; }
        public BrowserImageCaptureService(string browserProcessName)
        {
            // Get the process reference of the intended browser
            Browser = GetWindowedProcessByName(browserProcessName);
        }

        /// <summary>
        /// Gets a Process instance that has the inputted process name, and has a window attached to the process.
        /// </summary>
        /// <param name="processName">The process name search string</param>
        private Process GetWindowedProcessByName(string processName)
        {
            Logger.LogInfo("Getting chrome window process");

            var processes = Process.GetProcessesByName(processName);
            Logger.LogInfo($"Found {processes.Length} '{processName}' processes");

            if (processes.Length == 0)
            {
                Logger.LogInfo("Starting a chrome process, then trying again in 1.5 seconds");
                var process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = processName;
                process.StartInfo.Arguments = Constants.GoogleMinesweeperUrl;
                process.Start();
                Thread.Sleep(1500);
                return GetWindowedProcessByName(processName);
            }

            // Get the first process that has window (excludes background processes)
            var windowedProcess = processes.FirstOrDefault(x => x.MainWindowHandle != IntPtr.Zero);

            if (windowedProcess == null)
            {
                Logger.LogError($"Could not find a '{processName}' process with a window");
                throw new Exception($"No '{processName}' process found");
            }
            else
            {
                Logger.LogInfo($"Found '{processName}' process with a window");
                return windowedProcess;
            }
        }
        private User32Utils.WindowsRectangle GetProcessRectangleRelativeToScreen()
        {
            Logger.LogInfo("Getting the windows current rectangle");
            var rectangle = new User32Utils.WindowsRectangle();
            User32Utils.GetWindowRect(Browser.MainWindowHandle, ref rectangle);
            Logger.LogInfo($"Rectangle found: {Environment.NewLine + " - "}Top{rectangle.Top}{Environment.NewLine + " - "}Bottom{rectangle.Bottom}{Environment.NewLine + " - "}Left{rectangle.Left}{Environment.NewLine + " - "}Right{rectangle.Right}");
            ChromeTopLeft = new Point(rectangle.Left, rectangle.Top);
            return rectangle;
        }
        public async Task<Bitmap> GetBrowserWindowScreenshotAsync()
        {
            return await Task.Run(() =>
            {
                MouseControlService.TryDelay(Constants.ClickAnimationDuration);
                var rectangle = GetProcessRectangleRelativeToScreen();

                var width = rectangle.Right - rectangle.Left;
                var height = rectangle.Bottom - rectangle.Top;

                var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                var graphics = Graphics.FromImage(bmp);

                Logger.LogInfo("Bringing browser to the front");
                User32Utils.BringToFront(Browser.MainWindowHandle);

                graphics.CopyFromScreen(rectangle.Left, rectangle.Top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
                bmp.Save($"{Environment.CurrentDirectory}\\browser.png");
                return bmp;
            });
        }
    }
}
