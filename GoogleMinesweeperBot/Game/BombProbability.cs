using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace GoogleMinesweeperBotClean.Game
{
    public class BombProbability : IEquatable<BombProbability>
    {
        public BombProbability(Point position, decimal probability)
        {
            Position = position;
            Probability = probability;
            TileState = probability == 0 ? TileState.Unclicked : TileState.Clicked;
        }
        public TileState TileState { get;  }
        public decimal Probability { get;  }
        public Point Position { get;  }
        public bool IsBomb => Probability == 1;
        public override bool Equals(object obj) =>
            obj switch
            {
                null => false,
                BombProbability bomb => (Position.X == bomb.Position.X && Position.Y == bomb.Position.Y &&
                                         Probability == bomb.Probability),
                _ => false
            };

        public override int GetHashCode()
        {
            return HashCode.Combine((int) TileState, Probability, Position);
        }

        public override string ToString()
        {
            return Position.ToString() + " " + Probability;
        }
        public bool Equals([AllowNull] BombProbability other)
        {
            return Equals((object)other);
        }
    }
}
