using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    public enum MovementDirection : int
    {
        Up, 
        UpRight,
        DownRight,
        Down,
        DownLeft,
        UpLeft,
        DiagonalUpRight,
        DiagonalRight,
        DiagonalDownRight,
        DiagonalDownLeft,
        DiagonalLeft,
        DiagonalUpLeft
    }

    public static class MovementDirectionHelpers
    {
        public static bool IsInLineWith(this MovementDirection direction1, MovementDirection direction2)
        {
            switch (direction1)
            {
                case MovementDirection.Up:
                case MovementDirection.Down:
                    return (direction2 == MovementDirection.Up) || (direction2 == MovementDirection.Down);
                case MovementDirection.UpRight:
                case MovementDirection.DownLeft:
                    return (direction2 == MovementDirection.UpRight) || (direction2 == MovementDirection.DownLeft);
                case MovementDirection.UpLeft:
                case MovementDirection.DownRight:
                    return (direction2 == MovementDirection.UpLeft) || (direction2 == MovementDirection.DownRight);
                case MovementDirection.DiagonalUpRight:
                case MovementDirection.DiagonalDownLeft:
                    return (direction2 == MovementDirection.DiagonalUpRight) || (direction2 == MovementDirection.DiagonalDownLeft);
                case MovementDirection.DiagonalRight:
                case MovementDirection.DiagonalLeft:
                    return (direction2 == MovementDirection.DiagonalRight) || (direction2 == MovementDirection.DiagonalLeft);
                case MovementDirection.DiagonalDownRight:
                case MovementDirection.DiagonalUpLeft:
                    return (direction2 == MovementDirection.DiagonalDownRight) || (direction2 == MovementDirection.DiagonalUpLeft);
                default:
                    return false;
            }
        }

        public static MovementDirection GetReverse(this MovementDirection direction)
        {
            switch (direction)
            {
                case MovementDirection.Up: 
                    return MovementDirection.Down;
                case MovementDirection.UpRight: 
                    return MovementDirection.DownLeft;
                case MovementDirection.DownRight: 
                    return MovementDirection.UpLeft;
                case MovementDirection.Down: 
                    return MovementDirection.Up;
                case MovementDirection.DownLeft:
                    return MovementDirection.UpRight;
                case MovementDirection.UpLeft:
                    return MovementDirection.DownRight;
                case MovementDirection.DiagonalUpRight:
                    return MovementDirection.DiagonalDownLeft;
                case MovementDirection.DiagonalRight:
                    return MovementDirection.DiagonalLeft;
                case MovementDirection.DiagonalDownRight:
                    return MovementDirection.DiagonalUpLeft;
                case MovementDirection.DiagonalDownLeft:
                    return MovementDirection.DiagonalUpRight;
                case MovementDirection.DiagonalLeft:
                    return MovementDirection.DiagonalRight;
                case MovementDirection.DiagonalUpLeft:
                    return MovementDirection.DiagonalDownRight;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
