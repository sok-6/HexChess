using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    public class AttackManager
    {
        public const int EMPTY = int.MinValue;

        private int[,] _whiteSlidingAttackMatrix;
        private int[,] _blackSlidingAttackMatrix;

        private int[] _whiteAttackCounts;
        private int[] _blackAttackCounts;

        public AttackManager()
        {
            _whiteSlidingAttackMatrix = new int[91, 12];
            _blackSlidingAttackMatrix = new int[91, 12];

            _whiteAttackCounts = new int[91];
            _blackAttackCounts = new int[91];

            Clear();
        }

        private void Clear()
        {
            for (int i = 0; i < 91; i++)
            {
                _whiteAttackCounts[i] = 0;
                _blackAttackCounts[i] = 0;

                for (int j = 0; j < 12; j++)
                {
                    _whiteSlidingAttackMatrix[i, j] = EMPTY;
                    _blackSlidingAttackMatrix[i, j] = EMPTY;
                }
            }
        }

        public void Recalculate(BoardState boardState)
        {
            Clear();

            // Pawns
            foreach (var index in boardState.WhitePawnIndices) ProcessPawn(true, index, true);
            foreach (var index in boardState.BlackPawnIndices) ProcessPawn(true, index, false);

            // Knights
            foreach (var index in boardState.WhiteKnightIndices) ProcessKnight(true, index, true);
            foreach (var index in boardState.BlackKnightIndices) ProcessKnight(true, index, false);

            // Kings
            ProcessKing(true, boardState.WhiteKingIndex, true);
            ProcessKing(true, boardState.BlackKingIndex, false);

            // Bishops
            foreach (var index in boardState.WhiteBishopIndices)
            {
                ProcessSlidingPiece(true, index, boardState, false, true, true);
                BlockSlidingAttacks(index, boardState);
            }

            foreach (var index in boardState.BlackBishopIndices)
            {
                ProcessSlidingPiece(true, index, boardState, false, true, false);
                BlockSlidingAttacks(index, boardState);
            }

            // Rooks
            foreach (var index in boardState.WhiteRookIndices)
            {
                ProcessSlidingPiece(true, index, boardState, true, false, true);
                BlockSlidingAttacks(index, boardState);
            }

            foreach (var index in boardState.BlackRookIndices)
            {
                ProcessSlidingPiece(true, index, boardState, true, false, false);
                BlockSlidingAttacks(index, boardState);
            }

            // Queens
            foreach (var index in boardState.WhiteQueenIndices)
            {
                ProcessSlidingPiece(true, index, boardState, true, true, true);
                BlockSlidingAttacks(index, boardState);
            }

            foreach (var index in boardState.BlackQueenIndices)
            {
                ProcessSlidingPiece(true, index, boardState, true, true, false);
                BlockSlidingAttacks(index, boardState);
            }
        }

        public void AddPiece(int startIndex, Piece piece, BoardState boardState)
        {
            ProcessPiece(true, startIndex, piece, boardState);
        }

        public void RemovePiece(int startIndex, Piece piece, BoardState boardState)
        {
            ProcessPiece(false, startIndex, piece, boardState);
        }

        /// <summary>
        /// Adds or removes a piece from its side's attack maps - this should be called AFTER adding new pieces, but BEFORE removing old pieces
        /// </summary>
        /// <param name="isAdd">true if adding a piece, false if removing it</param>
        /// <param name="startIndex">The cell index of the piece to change</param>
        /// <param name="piece">The piece to change</param>
        /// <param name="boardState">The current state of the board</param>
        private void ProcessPiece(bool isAdd, int startIndex, Piece piece, BoardState boardState)
        {
            // If adding a piece here, it will block existing sliding attacks through this cell
            if (isAdd) BlockSlidingAttacks(startIndex, boardState);

            var isWhitePiece = piece.IsWhite();

            if (piece.IsSameTypeAs(Piece.Pawn)) ProcessPawn(isAdd, startIndex, isWhitePiece);
            else if (piece.IsSameTypeAs(Piece.Bishop)) ProcessSlidingPiece(isAdd, startIndex, boardState, false, true, isWhitePiece);
            else if (piece.IsSameTypeAs(Piece.Knight)) ProcessKnight(isAdd, startIndex, isWhitePiece);
            else if (piece.IsSameTypeAs(Piece.Rook)) ProcessSlidingPiece(isAdd, startIndex, boardState, true, false, isWhitePiece);
            else if (piece.IsSameTypeAs(Piece.Queen)) ProcessSlidingPiece(isAdd, startIndex, boardState, true, true, isWhitePiece);
            else ProcessKing(isAdd, startIndex, isWhitePiece);

            // If removing a piece, extend previously blocked sliding attacks
            if (isAdd == false) ExtendSlidingAttacks(startIndex, boardState);
        }

        private void ProcessPawn(bool isAdd, int startIndex, bool isWhitePiece)
        {
            if (isWhitePiece)
            {
                foreach (var attackIndex in MoveGeneration.MoveLibrary.WhitePawnCaptures[startIndex])
                {
                    _whiteAttackCounts[attackIndex] += (isAdd ? 1 : -1);
                }
            }
            else
            {
                foreach (var attackIndex in MoveGeneration.MoveLibrary.BlackPawnCaptures[startIndex])
                {
                    _blackAttackCounts[attackIndex] += (isAdd ? 1 : -1);
                }
            }
        }

        private void ProcessKnight(bool isAdd, int startIndex, bool isWhitePiece)
        {
            foreach (var attackIndex in MoveGeneration.MoveLibrary.KnightMoves[startIndex])
            {
                if (isWhitePiece)
                {
                    _whiteAttackCounts[attackIndex] += (isAdd ? 1 : -1); 
                }
                else
                {
                    _blackAttackCounts[attackIndex] += (isAdd ? 1 : -1);
                }
            }
        }

        private void ProcessKing(bool isAdd, int startIndex, bool isWhitePiece)
        {
            foreach (var attackIndex in MoveGeneration.MoveLibrary.KingMoves[startIndex])
            {
                if (isWhitePiece)
                {
                    _whiteAttackCounts[attackIndex] += (isAdd ? 1 : -1);
                }
                else
                {
                    _blackAttackCounts[attackIndex] += (isAdd ? 1 : -1);
                }
            }
        }

        private void ProcessSlidingPiece(bool isAdd, int startIndex, BoardState boardState, bool includeFiles, bool includeDiagonals, bool isWhitePiece)
        {
            var directionStartIndex = includeFiles ? 0 : 6;
            var directionEndIndex = includeDiagonals ? 12 : 6;

            // Add sliding attacks in all relevant directions
            for (int directionIndex = directionStartIndex; directionIndex < directionEndIndex; directionIndex++)
            {
                var direction = (MovementDirection)directionIndex;

                var currentCell = CubeCoordinate.FromArrayIndex(startIndex);

                while (true)
                {
                    currentCell = currentCell.Step(direction);

                    // End if reached edge of board
                    if (currentCell.IsOnBoard() == false)
                    {
                        break;
                    }

                    var currentIndex = currentCell.ToArrayIndex();

                    if (isWhitePiece == true)
                    {
                        _whiteAttackCounts[currentIndex] += (isAdd ? 1 : -1);

                        _whiteSlidingAttackMatrix[currentIndex, directionIndex] = (isAdd ? startIndex : EMPTY);
                    }
                    else
                    {
                        _blackAttackCounts[currentIndex] += (isAdd ? 1 : -1);

                        _blackSlidingAttackMatrix[currentIndex, directionIndex] = (isAdd ? startIndex : EMPTY);
                    }

                    // End if cell is occupied
                    if (boardState.GetPieceInCell(currentIndex) != Piece.None)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Putting a new piece in a cell will block all sliding attacks through that cell
        /// Clears the attack matrix in that direction and reduces the number of attackers on affected cells
        /// </summary>
        /// <param name="blockingCellIndex">The cell index of the new piece which will block attacks</param>
        /// <param name="boardState">The current state of the board</param>
        private void BlockSlidingAttacks(int blockingCellIndex, BoardState boardState)
        {
            // Check all 12 directions for both white and black
            for (int i = 0; i < 12; i++)
            {
                if (_whiteSlidingAttackMatrix[blockingCellIndex, i] != EMPTY)
                {
                    var currentCell = CubeCoordinate.FromArrayIndex(blockingCellIndex);

                    while (true)
                    {
                        currentCell = currentCell.Step((MovementDirection)i);

                        if (currentCell.IsOnBoard() == false)
                        {
                            break;
                        }

                        var index = currentCell.ToArrayIndex();

                        _whiteAttackCounts[index]--;
                        _whiteSlidingAttackMatrix[index, i] = EMPTY;

                        if (boardState.GetPieceInCell(index) != Piece.None)
                        {
                            break;
                        }
                    }
                }

                if (_blackSlidingAttackMatrix[blockingCellIndex, i] != EMPTY)
                {
                    var currentCell = CubeCoordinate.FromArrayIndex(blockingCellIndex);

                    while (true)
                    {
                        currentCell = currentCell.Step((MovementDirection)i);

                        if (currentCell.IsOnBoard() == false)
                        {
                            break;
                        }

                        var index = currentCell.ToArrayIndex();

                        _blackAttackCounts[index]--;
                        _blackSlidingAttackMatrix[index, i] = EMPTY;

                        if (boardState.GetPieceInCell(index) != Piece.None)
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removing a piece will allow previously blocked sliding piece attacks to pass through
        /// Extends sliding piece attacks in all directions through the given cell
        /// </summary>
        /// <param name="blockingCellIndex">The index of the cell which was previously blocked</param>
        /// <param name="boardState">The current state of the board</param>
        private void ExtendSlidingAttacks(int blockingCellIndex, BoardState boardState)
        {
            // Check all 12 directions for both white and black
            for (int i = 0; i < 12; i++)
            {
                if (_whiteSlidingAttackMatrix[blockingCellIndex, i] != EMPTY)
                {
                    var attackerCellIndex = _whiteSlidingAttackMatrix[blockingCellIndex, i];
                    var currentCell = CubeCoordinate.FromArrayIndex(blockingCellIndex);

                    while (true)
                    {
                        currentCell = currentCell.Step((MovementDirection)i);

                        if (currentCell.IsOnBoard() == false)
                        {
                            break;
                        }

                        var index = currentCell.ToArrayIndex();

                        _whiteAttackCounts[index]++;
                        _whiteSlidingAttackMatrix[index, i] = attackerCellIndex;

                        if (boardState.GetPieceInCell(index) != Piece.None)
                        {
                            break;
                        }
                    }
                }

                if (_blackSlidingAttackMatrix[blockingCellIndex, i] != EMPTY)
                {
                    var currentCell = CubeCoordinate.FromArrayIndex(blockingCellIndex);

                    while (true)
                    {
                        var attackerCellIndex = _blackSlidingAttackMatrix[blockingCellIndex, i];
                        currentCell = currentCell.Step((MovementDirection)i);

                        if (currentCell.IsOnBoard() == false)
                        {
                            break;
                        }

                        var index = currentCell.ToArrayIndex();

                        _blackAttackCounts[index]++;
                        _blackSlidingAttackMatrix[index, i] = attackerCellIndex;

                        if (boardState.GetPieceInCell(index) != Piece.None)
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets if a particular side is attacking a target cell
        /// </summary>
        /// <param name="cellIndex">The cell to check attacks on</param>
        /// <param name="IsWhiteAttack">If true, returns if white is attaking the cell, otherwise if black is attacking it</param>
        /// <returns></returns>
        public bool IsUnderAttack(int cellIndex, bool IsWhiteAttack)
        {
            if (IsWhiteAttack)
            {
                return _whiteAttackCounts[cellIndex] != 0;
            }
            else
            {
                return _blackAttackCounts[cellIndex] != 0;
            }
        }

        public bool IsSlidingPieceAttackingFromDirection(int cellIndex, bool isWhiteAttack, MovementDirection attackDirection)
        {
            if (isWhiteAttack)
            {
                return _whiteSlidingAttackMatrix[cellIndex, (int)attackDirection] != EMPTY;
            }
            else
            {
                return _blackSlidingAttackMatrix[cellIndex, (int)attackDirection] != EMPTY;
            }
        }

        public int? GetSlidingAttackIndexFromDirection(int cellIndex, bool isWhiteAttack, MovementDirection attackDirection)
        {
            int result;

            if (isWhiteAttack)
            {
                result = _whiteSlidingAttackMatrix[cellIndex, (int)attackDirection];
            }
            else
            {
                result = _blackSlidingAttackMatrix[cellIndex, (int)attackDirection];
            }

            return result == EMPTY ? null : result;
        }
    }
}
