using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    internal record MoveRecord
    {
        public int PreviousHalfMoveClock { get; set; }
        public int NextHalfMoveClock { get; set; }
        public int PreviousTurnCounter { get; set; }
        public int NextTurnCounter { get; set; }
        public int? PreviousEnPassantIndex { get; set; }
        public int? NextEnPassantIndex { get; set; }
        public bool PreviousWhiteTurnEh { get; set; }
        public bool NextWhiteTurnEh { get; set; }
        public GameState PreviousGameState { get; set; }
        public GameState NextGameState { get; set; }
        public int LastMoveHighlightIndex1 { get; set; }
        public int LastMoveHighlightIndex2 { get; set; }
        public List<(int, Piece)> RemovedPieces { get; set; }
        public List<(int, Piece)> AddedPieces { get; set; }
        public Move Move { get; set; }
    }
}
