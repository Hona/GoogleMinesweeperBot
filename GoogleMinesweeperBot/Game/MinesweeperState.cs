using System;

namespace GoogleMinesweeperBotClean.Game
{
    public class MinesweeperState
    {
        public int HorizontalTiles => Grid.GetLength(0);
        public int VerticalTiles => Grid.GetLength(1);
        public int[,] Grid { get; set; }
        public MinesweeperState(int width, int height)
        {
            ResetGrid(width, height);
        }
        private void ResetGrid(int width, int height)
        {
            Grid = new int[width, height];
        }
        public void PrintToConsole()
        {
            var defaultForegroundColor = Console.ForegroundColor;
            for (var y = 0; y < VerticalTiles; y++)
            {
                for (var x = 0; x < HorizontalTiles; x++)
                {
                    var tileString = Grid[x, y] < 0 ? (Grid[x, y] == -2 ? "x" : " " ) : Grid[x, y].ToString();

                    Console.ForegroundColor = (Grid[x, y]) switch
                    {
                        -2 => ConsoleColor.Red,
                        0 => defaultForegroundColor,
                        1 => ConsoleColor.Blue,
                        2 => ConsoleColor.Green,
                        3 => ConsoleColor.Red,
                        4 => ConsoleColor.DarkMagenta,
                        5 => ConsoleColor.Yellow,
                        _ => defaultForegroundColor,
                    };

                    Console.Write(tileString + " ");
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = defaultForegroundColor;
        }
        public bool IsNewState()
        {
            for (var x = 0; x < HorizontalTiles; x++)
            {
                for (var y = 0; y < VerticalTiles; y++)
                {
                    if (Grid[x, y] != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

    }
}
