using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    public enum GameState
    {
        Normal,
        Check,
        Draw,
        Stalemate,
        Checkmate,
        ThreefoldRepetitionDraw
    }
}
