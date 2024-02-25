using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    public enum Piece : byte
    {
        None = 0,

        Pawn = 1, 
        Bishop = 2,
        Knight = 3,
        Rook = 4,
        Queen = 5,
        King = 6,

        White = 8,
        Black = 16
    }
}
