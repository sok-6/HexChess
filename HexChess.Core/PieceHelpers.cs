using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    public static class PieceHelpers
    {
        private const int PIECE_COLOUR_MASK = 0b11000;
        private const int PIECE_TYPE_MASK = 0b111;

        public static string GetName(this Piece piece)
        {
            var sb = new StringBuilder();

            sb.Append((piece & Piece.White) != Piece.None ? "White " : "Black ");
            sb.Append(((Piece)((int)piece & PIECE_TYPE_MASK)).ToString());

            return sb.ToString();
        }

        public static bool IsWhite(this Piece piece) => (piece & Piece.White) == Piece.White;

        public static bool IsKing(this Piece piece) => (Piece)((int)piece & PIECE_TYPE_MASK) == Piece.King;

        public static bool IsSameTypeAs(this Piece piece, Piece target) => ((int)piece & PIECE_TYPE_MASK) == ((int)target & PIECE_TYPE_MASK);

        public static bool IsAllyOf(this Piece a, Piece b)
        {
            if (a == Piece.None) return false;
            if (b == Piece.None) return false;

            return ((int)a & PIECE_COLOUR_MASK) == ((int)b & PIECE_COLOUR_MASK);
        }

        public static bool IsEnemyOf(this Piece a, Piece b)
        {
            if (a == Piece.None) return false;
            if (b == Piece.None) return false;

            return ((int)a & PIECE_COLOUR_MASK) != ((int)b & PIECE_COLOUR_MASK);
        }

        public static bool CanMoveAlongFiles(this Piece piece)
        {
            var pieceType = (Piece)((int)piece & PIECE_TYPE_MASK);

            return pieceType == Piece.Rook || pieceType == Piece.Queen;
        }

        public static bool CanMoveAlongDiagonals(this Piece piece)
        {
            var pieceType = (Piece)((int)piece & PIECE_TYPE_MASK);

            return pieceType == Piece.Bishop || pieceType == Piece.Queen;
        }
    }
}
