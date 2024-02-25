using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    public class Move
    {
        /*  PACKED MOVE INFO LAYOUT
         * 
         * posn: 3322 2222 2222 1111 1111 11
         *       1098 7654 3210 9876 5432 1098 7654 3210
         * data: pppp pppf ffff fccc ccdd dddd dsss ssss
         * 
         * s: start cell index
         * d: destination cell index
         * c: captured piece details
         * f: flags
         * p: en passant cell
         */

        private const int START_SHIFT = 0;
        private const int START_MASK = 0b1111111;
        private const int DESTINATION_SHIFT = 7;
        private const int DESTINATION_MASK = 0b1111111;
        private const int CAPTURED_PIECE_SHIFT = 14;
        private const int CAPTURED_PIECE_MASK = 0b11111;
        private const int FLAGS_SHIFT = 19;
        private const int FLAGS_MASK = 0b111111;
        private const int EN_PASSANT_SHIFT = 25;
        private const int EN_PASSANT_MASK = 0b1111111;

        public int Data { get; private set; }

        public int StartIndex => (Data >> START_SHIFT) & START_MASK;
        public CubeCoordinate StartCoordinate => CubeCoordinate.FromArrayIndex(StartIndex);

        public int DestinationIndex => (Data >> DESTINATION_SHIFT) & DESTINATION_MASK;
        public CubeCoordinate DestinationCoordinate => CubeCoordinate.FromArrayIndex(DestinationIndex);

        public Piece CapturedPiece => (Piece)((Data >> CAPTURED_PIECE_SHIFT) & CAPTURED_PIECE_MASK);

        public MoveFlags Flags => (MoveFlags)((Data >> FLAGS_SHIFT) & FLAGS_MASK);

        /// <summary>
        /// If move is a double pawn push, this is the stepped over cell
        /// If move is an en passant capture, this is the cell where the enemy pawn will be removed from
        /// </summary>
        public int EnPassantIndex => (Data >> EN_PASSANT_SHIFT) & EN_PASSANT_MASK;

        /// <summary>
        /// If move is a double pawn push, this is the stepped over cell
        /// If move is an en passant capture, this is the cell where the enemy pawn will be removed from
        /// </summary>
        public CubeCoordinate EnPassantCoordinate => CubeCoordinate.FromArrayIndex(EnPassantIndex);


        public Move(CubeCoordinate start, CubeCoordinate destination, Piece capturedPiece = Piece.None, MoveFlags flags = MoveFlags.None, CubeCoordinate enPassant = null)
        {
            Data = (start.ToArrayIndex() << START_SHIFT) 
                | (destination.ToArrayIndex() << DESTINATION_SHIFT) 
                | (((int)capturedPiece) << CAPTURED_PIECE_SHIFT) 
                | (((int)flags) << FLAGS_SHIFT)
                | ((enPassant?.ToArrayIndex() ?? 0) << EN_PASSANT_SHIFT);
        }

        public Move(int startIndex, int destinationIndex, Piece capturedPiece = Piece.None, MoveFlags flags = MoveFlags.None, int enPassantIndex = 0)
        {
            Data = (startIndex << START_SHIFT)
                | (destinationIndex << DESTINATION_SHIFT)
                | (((int)capturedPiece) << CAPTURED_PIECE_SHIFT)
                | (((int)flags) << FLAGS_SHIFT)
                | (enPassantIndex << EN_PASSANT_SHIFT);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append($"{StartCoordinate.ToAlgebraic()}{DestinationCoordinate.ToAlgebraic()}");

            if (CapturedPiece != Piece.None)
            {
                sb.Append($" - Captured {CapturedPiece.GetName()}");
            }

            if (Flags != MoveFlags.None)
            {
                sb.Append($" - {Flags.ToString()}");
            }

            if (Flags == MoveFlags.DoublePawnPush)
            {
                sb.Append($" - EnPassant {EnPassantCoordinate.ToAlgebraic()}");
            }

            if (Flags == MoveFlags.PromoteToBishop)
            {
                sb.Append($" - Promote to bishop");
            }

            if (Flags == MoveFlags.PromoteToKnight)
            {
                sb.Append($" - Promote to knight");
            }

            if (Flags == MoveFlags.PromoteToRook)
            {
                sb.Append($" - Promote to rook");
            }

            if (Flags == MoveFlags.PromoteToQueen)
            {
                sb.Append($" - Promote to queen");
            }

            return sb.ToString();
        }

        public enum MoveFlags : byte
        {
            None = 0,
            DoublePawnPush = 1,
            EnPassantCapture = 2,
            PromoteToBishop = 3,
            PromoteToKnight = 4,
            PromoteToRook = 5,
            PromoteToQueen = 6
        }
    }
}
