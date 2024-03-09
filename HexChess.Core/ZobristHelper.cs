using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    internal static class ZobristHelper
    {
        private const int SEED = 26061986; // TODO: Validate that this is an effective seed?
        private static readonly Random _rnd = new Random(SEED);

        /// <summary>
        /// Hash values associated with a cell/piece combination. <para/>
        /// Cell is cell index <para/>
        /// Piece is piece value subtract 9 (White pawn = 8+1, White King = 8+6, Black pawn = 16+1, Black King = 16+6)
        /// </summary>
        internal static ulong[,] CELL_PIECE_VALUES { get; private set; }
        internal static ulong WHITE_TO_MOVE_VALUE { get; private set; }
        internal static ulong[] EN_PASSANT_CELL_VALUES { get; private set; }

        internal static void GenerateZobristHashValues()
        {
            /* 0123456789012345678901234
             * xxxxxxxxxwwwwwwxxbbbbbb
             *          pbkrqkxxpbkrqk
             *          01234567890123
             */
            CELL_PIECE_VALUES = new ulong[91, 14];
            EN_PASSANT_CELL_VALUES = new ulong[91];

            for (int cellIndex = 0; cellIndex < 91; cellIndex++)
            {
                for (int pieceIndex = 0; pieceIndex < 14; pieceIndex++)
                {
                    CELL_PIECE_VALUES[cellIndex, pieceIndex] = GetRandomUlong();
                }

                EN_PASSANT_CELL_VALUES[cellIndex] = GetRandomUlong();
            }

            WHITE_TO_MOVE_VALUE = GetRandomUlong();
        }

        private static ulong GetRandomUlong()
        {
            var buffer = new byte[8];
            _rnd.NextBytes(buffer);
            
            return
                ((ulong)buffer[7] << (8 * 7)) |
                ((ulong)buffer[6] << (8 * 6)) |
                ((ulong)buffer[5] << (8 * 5)) |
                ((ulong)buffer[4] << (8 * 4)) |
                ((ulong)buffer[3] << (8 * 3)) |
                ((ulong)buffer[2] << (8 * 2)) |
                ((ulong)buffer[1] << (8 * 1)) |
                ((ulong)buffer[0] << (8 * 0));
        }
    }
}
