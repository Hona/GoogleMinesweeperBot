using GoogleMinesweeperBotClean.Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GoogleMinesweeperBotClean.Services
{
    public class BombFlagService
    {
        private static List<Point> _flaggedPoints = new List<Point>();

        public void FlagBombs(IEnumerable<BombProbability> probabilities, MinesweeperState state)
        {
            if (state.IsNewState())
            {
                // Reset flagged points
                _flaggedPoints = new List<Point>();
                return;
            }

            var bombProbabilities = probabilities.Where(x => x.IsBomb).ToArray();
            foreach (var certainBomb in bombProbabilities)
            {
                // TODO: Flag bombs using cache (if not in cache flag)
                if (certainBomb.Position.X >= 0 && certainBomb.Position.X < state.HorizontalTiles && certainBomb.Position.Y >= 0 && certainBomb.Position.Y < state.VerticalTiles
                    && !_flaggedPoints.Any(x => x.X == certainBomb.Position.X && x.Y == certainBomb.Position.Y))
                {
                    var point = MouseControlService.GetScreenPosition(certainBomb.Position);
                    MouseControlService.DoMouseRightClick(point.X, point.Y);
                    _flaggedPoints.Add(certainBomb.Position);
                }
            }

            MouseControlService.TryDelay(150);
        }
    }
}
