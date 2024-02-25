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
}
