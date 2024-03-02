using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core.MoveGeneration
{
    public static class MoveGenerator
    {
        public static IEnumerable<Move> GetMoves(BoardState boardState, bool generateCapturesOnly = false)
        {
            var result = new List<Move>();

            // Get pins for the current player's pieces
            var pins = GetPins(boardState, boardState.IsWhitesTurn);

            // Pawn moves and captures
            var pawnMoveSet = boardState.IsWhitesTurn ? MoveLibrary.WhitePawnMoves : MoveLibrary.BlackPawnMoves;
            var pawnCaptureSet = boardState.IsWhitesTurn ? MoveLibrary.WhitePawnCaptures : MoveLibrary.BlackPawnCaptures;

            foreach (var pawnIndex in boardState.IsWhitesTurn ? boardState.WhitePawnIndices : boardState.BlackPawnIndices)
            {
                if (!generateCapturesOnly)
                {
                    // We're ok to assume that there will be at least one pawn move, as a pawn on the back rank will have been promoted
                    // Single step move
                    if (boardState.GetPieceInCell(pawnMoveSet[pawnIndex][0]) == Piece.None)
                    {
                        GetPawnMoveWithPromotions(result, boardState.IsWhitesTurn, pawnIndex, pawnMoveSet[pawnIndex][0]);

                        // If double move available, check if it can still be taken
                        if (pawnMoveSet[pawnIndex].Length > 1 && boardState.GetPieceInCell(pawnMoveSet[pawnIndex][1]) == Piece.None)
                        {
                            // Double moves can't reach promotion cell
                            result.Add(new Move(pawnIndex, pawnMoveSet[pawnIndex][1], flags: Move.MoveFlags.DoublePawnPush, enPassantIndex: pawnMoveSet[pawnIndex][0]));
                        }
                    } 
                }

                // Captures in available directions
                foreach (var destination in pawnCaptureSet[pawnIndex])
                {
                    // Normal capture
                    Piece destinationPiece = boardState.GetPieceInCell(destination);

                    if (IsCapturable(destinationPiece, boardState.IsWhitesTurn))
                    {
                        GetPawnMoveWithPromotions(result, boardState.IsWhitesTurn, pawnIndex, destination, destinationPiece);
                    }

                    // En passant
                    if (boardState.EnPassantIndex.HasValue && boardState.EnPassantIndex.Value == destination)
                    {
                        // En passant can't reach promotion cell
                        result.Add(new Move(
                            pawnIndex,
                            destination,
                            (boardState.IsWhitesTurn ? Piece.Black : Piece.White) | Piece.Pawn,
                            Move.MoveFlags.EnPassantCapture,
                            CubeCoordinate.FromArrayIndex(destination).Step(boardState.IsWhitesTurn ? MovementDirection.Down : MovementDirection.Up).ToArrayIndex()));
                    }
                }
            }

            // Knight moves
            foreach (var knightIndex in boardState.IsWhitesTurn ? boardState.WhiteKnightIndices : boardState.BlackKnightIndices)
            {
                foreach (var destinationIndex in MoveLibrary.KnightMoves[knightIndex])
                {
                    var destinationPiece = boardState.GetPieceInCell(destinationIndex);

                    if (!generateCapturesOnly && destinationPiece == Piece.None)
                    {
                        result.Add(new Move(knightIndex, destinationIndex));
                    }

                    if (IsCapturable(destinationPiece, boardState.IsWhitesTurn))
                    {
                        result.Add(new Move(knightIndex, destinationIndex, destinationPiece));
                    }
                }
            }

            // Bishops
            foreach (var bishopIndex in boardState.IsWhitesTurn ? boardState.WhiteBishopIndices : boardState.BlackBishopIndices)
            {
                result.AddRange(GetSlidingPieceMoves(boardState, bishopIndex, generateCapturesOnly, false, true));
            }

            // Rooks
            foreach (var rookIndex in boardState.IsWhitesTurn ? boardState.WhiteRookIndices : boardState.BlackRookIndices)
            {
                result.AddRange(GetSlidingPieceMoves(boardState, rookIndex, generateCapturesOnly, true, false));
            }

            // Queens
            foreach (var queenIndex in boardState.IsWhitesTurn ? boardState.WhiteQueenIndices : boardState.BlackQueenIndices)
            {
                result.AddRange(GetSlidingPieceMoves(boardState, queenIndex, generateCapturesOnly, true, true));
            }

            // King moves
            var kingIndex = boardState.IsWhitesTurn ? boardState.WhiteKingIndex : boardState.BlackKingIndex;

            foreach (var destinationIndex in MoveLibrary.KingMoves[kingIndex])
            {
                // If destination is under attack, ignore this move
                if (boardState.Attacks.IsUnderAttack(destinationIndex, !boardState.IsWhitesTurn)) continue;

                var destinationPiece = boardState.GetPieceInCell(destinationIndex);

                if (!generateCapturesOnly && destinationPiece == Piece.None)
                {
                    result.Add(new Move(kingIndex, destinationIndex));
                }

                if (IsCapturable(destinationPiece, boardState.IsWhitesTurn))
                {
                    result.Add(new Move(kingIndex, destinationIndex, destinationPiece));
                }
            }

            IEnumerable<Move> finalResults;

            // Filter out invalid moves due to active checks
            if (boardState.Attacks.IsUnderAttack(kingIndex, !boardState.IsWhitesTurn))
            {
                var checks = GetChecks(boardState, !boardState.IsWhitesTurn);

                finalResults = result.Where(m => checks.All(c => c.IsResolvedBy(m)));
            }
            else
            {
                finalResults = result;
            }

            // Filter out invalid pinned piece moves
            if (pins.Any())
            {
                var pinnedPieceMoves = finalResults.Where(m => pins.Any(p => p.PinnedPieceIndex == m.StartIndex));
                var invalidPinnedMoves = pinnedPieceMoves.Where(m =>
                {
                    // Get the direction of the move
                    var moveDirection = CubeCoordinate.GetSlidingMoveTypeBetween(m.StartIndex, m.DestinationIndex);

                    // If it's not a sliding move it must break the pin
                    if (moveDirection.HasValue == false) return true;

                    // If the movement direction isn't in line with the pin direction it must break the pin
                    return !pins
                        .First(p => p.PinnedPieceIndex == m.StartIndex)
                        .PinDirection
                        .IsInLineWith(moveDirection.Value);
                });


                finalResults = finalResults.Except(invalidPinnedMoves).ToList();
            }

            return finalResults;
        }

        private static void GetPawnMoveWithPromotions(List<Move> moves, bool isWhitesTurn, int startIndex, int destinationIndex, Piece capturedPiece = Piece.None)
        {
            if ((isWhitesTurn ? MoveLibrary.WhitePawnPromotionIndices : MoveLibrary.BlackPawnPromotionIndices).Contains(destinationIndex))
            {
                // Add all promotion options
                moves.Add(new Move(startIndex, destinationIndex, capturedPiece, Move.MoveFlags.PromoteToBishop));
                moves.Add(new Move(startIndex, destinationIndex, capturedPiece, Move.MoveFlags.PromoteToKnight));
                moves.Add(new Move(startIndex, destinationIndex, capturedPiece, Move.MoveFlags.PromoteToRook));
                moves.Add(new Move(startIndex, destinationIndex, capturedPiece, Move.MoveFlags.PromoteToQueen));
            }
            else
            {
                // Just add the move without any promotion
                moves.Add(new Move(startIndex, destinationIndex, capturedPiece));
            }
        }

        private static bool IsCapturable(Piece targetPiece, bool IsWhitesTurn)
        {
            // Only return true if there is a piece on the target square, and that piece's colour does not match the current player
            if (targetPiece == Piece.None) return false;

            if (IsWhitesTurn && (targetPiece & Piece.Black) == Piece.Black) return true;
            if (!IsWhitesTurn && (targetPiece & Piece.White) == Piece.White) return true;

            return false;
        }

        private static IEnumerable<Move> GetSlidingPieceMoves(BoardState boardState, int startIndex, bool generateCapturesOnly, bool includeFiles, bool includeDiagonals)
        {
            var result = new List<Move>();

            var directionStartIndex = includeFiles ? 0 : 6;
            var directionEndIndex = includeDiagonals ? 12 : 6;

            var startCell = CubeCoordinate.FromArrayIndex(startIndex);
            var startPiece = boardState.GetPieceInCell(startIndex);

            // Loop over all the directions required
            for (int i = directionStartIndex; i < directionEndIndex; i++)
            {
                var currentDirection = (MovementDirection)i;

                var currentCell = startCell;
                
                while(true)
                {
                    // Step in the current direction
                    currentCell = currentCell.Step(currentDirection);

                    // Check if reached edge of board
                    if (!currentCell.IsOnBoard()) break;

                    var cellContents = boardState.GetPieceInCell(currentCell.ToArrayIndex());

                    // If empty, add move
                    if (!generateCapturesOnly && cellContents == Piece.None)
                    {
                        result.Add(new Move(startIndex, currentCell.ToArrayIndex()));
                    }
                    else
                    {
                        // Cell is occupied, this will end current direction search

                        if (cellContents.IsEnemyOf(startPiece))
                        {
                            // Opponent's piece, add capture
                            result.Add(new Move(startIndex, currentCell.ToArrayIndex(), cellContents));
                        }

                        break;
                    }
                }
            }

            return result;
        }

        //private static void GetSlidingPieceAttacks(BoardState boardState, List<Check> checks, int startIndex, int enemyKingIndex, bool includeFiles, bool includeDiagonals)
        //{
        //    var directionStartIndex = includeFiles ? 0 : 6;
        //    var directionEndIndex = includeDiagonals ? 12 : 6;

        //    var startCell = CubeCoordinate.FromArrayIndex(startIndex);

        //    // Loop over all the directions required
        //    for (int i = directionStartIndex; i < directionEndIndex; i++)
        //    {
        //        var currentDirection = (MovementDirection)i;

        //        var currentCell = startCell;
        //        var searchedCells = new List<int>();

        //        while (true)
        //        {
        //            // Step in the current direction
        //            currentCell = currentCell.Step(currentDirection);

        //            // Check if reached edge of board
        //            if (!currentCell.IsOnBoard()) break;

        //            var currentCellIndex = currentCell.ToArrayIndex();

        //            searchedCells.Add(currentCellIndex);
        //            var cellContents = boardState.GetPieceInCell(currentCellIndex);

        //            // If empty, add to attacked cells
        //            if (cellContents != Piece.None)
        //            {
        //                // Check if putting enemy king in check
        //                if (currentCellIndex == enemyKingIndex)
        //                {
        //                    checks.Add(new Check(startIndex, enemyKingIndex, searchedCells.ToArray(), currentDirection));
        //                }

        //                // Cell is occupied, this will end current direction search
        //                break;
        //            }
        //        }
        //    }
        //}

        public static IEnumerable<Check> GetChecks(BoardState boardState, bool getWhiteAttacks)
        {
            var checks = new List<Check>();

            var enemyKingIndex = getWhiteAttacks ? boardState.BlackKingIndex : boardState.WhiteKingIndex;

            // Pawns
            var pawnAttackIndices = getWhiteAttacks ? MoveLibrary.WhitePawnCaptures : MoveLibrary.BlackPawnCaptures;
            foreach (var pawnIndex in getWhiteAttacks ? boardState.WhitePawnIndices : boardState.BlackPawnIndices)
            {
                foreach (var a in pawnAttackIndices[pawnIndex])
                {
                    if (a == enemyKingIndex) checks.Add(new Check(pawnIndex, enemyKingIndex, null));
                }
            }

            // Knights
            foreach (var knightIndex in getWhiteAttacks ? boardState.WhiteKnightIndices : boardState.BlackKnightIndices)
            {
                foreach (var a in MoveLibrary.KnightMoves[knightIndex])
                {
                    if (a == enemyKingIndex) checks.Add(new Check(knightIndex, enemyKingIndex, null));
                }
            }

            // Sliding pieces
            foreach (var direction in Enum.GetValues<MovementDirection>())
            {
                var attackerIndex = boardState.Attacks.GetSlidingAttackIndexFromDirection(enemyKingIndex, getWhiteAttacks, direction);

                if (attackerIndex.HasValue)
                {
                    checks.Add(new Check(attackerIndex.Value, enemyKingIndex, direction));
                }

            }

            return checks;
        }

        public static IEnumerable<Pin> GetPins(BoardState boardState, bool checkWhitePiecePins)
        {
            var result = new List<Pin>();

            var startCell = CubeCoordinate.FromArrayIndex(checkWhitePiecePins ? boardState.WhiteKingIndex : boardState.BlackKingIndex);
            var thisKing = (checkWhitePiecePins ? Piece.White : Piece.Black) | Piece.King;

            for (int i = 0; i < 12; i++)
            {
                var currentCell = startCell;
                var currentDirection = (MovementDirection)i;
                var oppositeDirection = currentDirection.GetReverse();

                while (true)
                {
                    currentCell = currentCell.Step(currentDirection);

                    if (currentCell.IsOnBoard() == false) break;

                    int currentCellIndex = currentCell.ToArrayIndex();

                    var currentPiece = boardState.GetPieceInCell(currentCellIndex);

                    if (currentPiece != Piece.None)
                    {
                        // Pins can only be held by friendly pieces
                        if (currentPiece.IsAllyOf(thisKing))
                        {
                            // Check if a sliding piece is attacking the friendly piece in the opposite direction
                            if (boardState.Attacks.IsSlidingPieceAttackingFromDirection(currentCellIndex, !checkWhitePiecePins, oppositeDirection))
                            {
                                // We have a pin!
                                result.Add(new Pin(currentCellIndex, oppositeDirection));
                            }
                        }

                        // Whether friendly or not, end search in this direction
                        break;
                    }
                }

                //CubeCoordinate friendlyPieceCoordinate = null;
                //var searchedCells = new List<CubeCoordinate>();

                //while (true)
                //{
                //    currentCell = currentCell.Step(currentDirection);

                //    // If reached edge of the board, stop
                //    if (currentCell.IsOnBoard() == false) break;

                //    var currentPiece = boardState.GetPieceInCell(currentCell.ToArrayIndex());

                //    if (currentPiece == Piece.None)
                //    {
                //        // Nothing here, it's an available location if there is a pin
                //        searchedCells.Add(currentCell);
                //    }
                //    else
                //    {
                //        if (currentPiece.IsAllyOf(thisKing))
                //        {
                //            if (friendlyPieceCoordinate == null)
                //            {
                //                // No friendly pieces found yet, so this could still be a pin
                //                friendlyPieceCoordinate = currentCell;
                //            }
                //            else
                //            {
                //                // Already 1 friendly piece found, no pin
                //                break;
                //            }
                //        }
                //        else
                //        {
                //            // Enemy piece found, check if a single friendly piece is in the way
                //            if (friendlyPieceCoordinate == null)
                //            {
                //                // No friendly pieces in the way, no pin
                //                break;
                //            }

                //            // See if enemy piece can attack along this vector
                //            if ((i < 6 && currentPiece.CanMoveAlongFiles()) || (i >= 6 && currentPiece.CanMoveAlongDiagonals()))
                //            {
                //                // Valid pin, add to list
                //                searchedCells.Add(currentCell); // Add the enemy cell to the list, a capture of the pinning piece is still valid

                //                result.Add(new Pin(friendlyPieceCoordinate.ToArrayIndex(), searchedCells.Select(x => x.ToArrayIndex()).ToArray()));
                //                break;
                //            }
                //            else
                //            {
                //                // Found piece can't attack along this direction, no pin
                //                break;
                //            }

                //        }
                //    }
                //}
            }

            return result;
        }
    }
}
