using GoogleMinesweeperBotClean.Game;
using GoogleMinesweeperBotClean.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GoogleMinesweeperBotClean.Services
{
    public static class MinesweeperBotService
    {
        private static readonly Random Random = new Random();
        public static Point[] GetBestChoice(BombProbability[] bombProbabilities, MinesweeperState state)
        {

            var bestChoice = bombProbabilities?.Where(x => x != null && !x.IsBomb && x.Probability != -1 && x.Position.X >= 0 && x.Position.Y >= 0 && x.Position.X < state.HorizontalTiles && x.Position.Y < state.VerticalTiles && state.Grid[x.Position.X, x.Position.Y] == 0).OrderBy(x => x.Probability).ToArray();

            if (bestChoice == null || bestChoice.Length == 0)
            {
                if (state.IsNewState())
                {
                    bestChoice = new[] { new BombProbability
                    (
                        new Point(state.HorizontalTiles / 2,
                        state.VerticalTiles / 2), -1
                    )};
                }
                else
                {
                    do
                    {
                        var width = state.HorizontalTiles;
                        var height = state.VerticalTiles;
                        bestChoice = new[] { new BombProbability
                    (
                        new Point(Random.Next(width),
                        Random.Next(height)), -1
                    )};
                    } while (state.Grid[bestChoice[0].Position.X, bestChoice[0].Position.Y] < 0);
                }
            }

            if (bestChoice.Length >= 1)
            {
                Logger.LogInfo($"Best choices: {bestChoice.Aggregate("", (currentString, nextChoice) => currentString + "(" + nextChoice.Position + "), ").TrimEnd(' ', ',')} ({(bestChoice.OrderByDescending(x => x.Probability).First().Probability == -1 ? "Unknown" : (bestChoice.OrderByDescending(x => x.Probability).First().Probability * 100).ToString())}% chance of bomb)");
            }
            else
            {
                Logger.LogWarning("No safe move found");
            }

            return bestChoice.Select(x => x.Position).ToArray();
        }
        public static BombProbability[] GetProbabilities(MinesweeperState state)
        {
            var probabilities = GetCertainBombProbabilities(state);
            probabilities = GetCertainlySafeProbabilities(state, probabilities.ToArray());
            return probabilities.DistinctBy(x => x.Position).OrderBy(x => x.Position.X).ThenBy(x => x.Position.Y).ToArray();
        }

        private static List<BombProbability> GetCertainBombProbabilities(MinesweeperState state)
        {
            var output = new List<BombProbability>();

            for (var x = 0; x < state.HorizontalTiles; x++)
            {
                for (var y = 0; y < state.VerticalTiles; y++)
                {

                    if (state.Grid[x, y] > 0)
                    {
                        // Flag certain bombs
                        var probabilities = GetCertainBombProbability(state, new Point(x, y));
                        if (probabilities != null)
                        {
                            output.AddRange(probabilities);
                        }
                    }
                }
            }

            output = output.Where(x => x.Position.X >= 0 && x.Position.X < state.HorizontalTiles
                                    && x.Position.Y >= 0 && x.Position.Y < state.VerticalTiles).DistinctBy(x => x.Position).ToList();

            Logger.LogInfo(output.Count + " bomb tiles.");
            return output;
        }
        private static List<BombProbability> GetCertainlySafeProbabilities(MinesweeperState state, BombProbability[] probabilitiesParam)
        {
            var output = new List<BombProbability>();
            output.AddRange(probabilitiesParam);

            for (var x = 0; x < state.HorizontalTiles; x++)
            {
                for (var y = 0; y < state.VerticalTiles; y++)
                {

                    if (state.Grid[x, y] > 0)
                    {
                        // Flag certain bombs
                        var probabilities = GetCertainlySafeProbability(state, new Point(x, y), probabilitiesParam);
                        if (probabilities != null)
                        {
                            output.AddRange(probabilities);
                        }
                    }
                }
            }

            output = output.ToList();

            Logger.LogInfo(output.Count + " safe tiles.");
            return output;
        }

        private static BombProbability[] GetCertainlySafeProbability(MinesweeperState state, Point point, BombProbability[] probabilities)
        {
            var surroundingTiles = GetSurroundingTiles(state, point);

            // Check for certain bomb
            var certainSurroundingBombs = new List<BombProbability>();
            var unknownSurroundingTiles = new List<BombProbability>();

            foreach (var surroundingPoint in surroundingTiles)
            {
                var probability = probabilities.FirstOrDefault(z => z.Position.X == surroundingPoint.X && z.Position.Y == surroundingPoint.Y);
                if (probability != null && probability.IsBomb)
                {
                    certainSurroundingBombs.Add(probability);
                }
                else
                {
                    unknownSurroundingTiles.Add(new BombProbability
                        (
                            new Point(surroundingPoint.X, surroundingPoint.Y),
                            -1
                        )
                    );
                }
            }

            if (certainSurroundingBombs.Count == state.Grid[point.X, point.Y])
            {
                for (var i = 0; i < unknownSurroundingTiles.Count; i++)
                {
                    unknownSurroundingTiles[i] = new BombProbability(unknownSurroundingTiles[i].Position,0);
                }
            }

            var output = unknownSurroundingTiles.Where(x => x.Probability != -1).ToArray();
            return output;
        }
        private static IEnumerable<Point> GetSurroundingTiles(MinesweeperState state, Point point)
        {
            var output = new List<Point>();
            for (var x = point.X - 1; x <= point.X + 1; x++)
            {
                for (var y = point.Y - 1; y <= point.Y + 1; y++)
                {
                    if ((point.X != x || point.Y != y) && x >= 0 && x < state.HorizontalTiles && y >= 0 && y < state.VerticalTiles)
                    {
                        output.Add(new Point(x, y));
                    }
                }
            }
            return output.ToArray();
        }
        private static BombProbability[] GetCertainBombProbability(MinesweeperState state, Point point)
        {
            var surroundingTiles = GetSurroundingTiles(state, point);

            // Check for certain bomb
            var hiddenSurroundingTiles = surroundingTiles.Where(x => state.Grid[x.X, x.Y] == 0).ToArray();

            if (hiddenSurroundingTiles.Length == state.Grid[point.X, point.Y])
            {
                var output = new List<BombProbability>();
                output.AddRange(hiddenSurroundingTiles.Select(x => new BombProbability
                (
                    x,
                    1)
                ).ToArray());
                return output.ToArray();
            }

            return null;
        }

    }
}
