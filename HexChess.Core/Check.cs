using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    public class Check
    {
        public Check(int attackingPieceIndex, int kingIndex, MovementDirection? slidingAttackerDirection)
        {
            AttackingPieceIndex = attackingPieceIndex;
            KingIndex = kingIndex;
            SlidingAttackerDirection = slidingAttackerDirection;
        }

        public int AttackingPieceIndex { get; private set; }
        public int KingIndex { get; private set; }
        public MovementDirection? SlidingAttackerDirection { get; private set; }

        public bool IsResolvedBy(Move move)
        {
            // Moving the king
            if (move.StartIndex == KingIndex)
            {
                // If a sliding pieceis attacking, the king's move can't move in the same direction of the attack
                if (SlidingAttackerDirection.HasValue)
                {
                    var kingMoveInAttackedDirection = move.StartCoordinate.Step(SlidingAttackerDirection.Value);

                    if (kingMoveInAttackedDirection == move.DestinationCoordinate)
                    {
                        return false;
                    }
                }

                return true;
            }

            // Moving in the path of the attacking piece
            if (SlidingAttackerDirection != null)
            {
                var kingToBlockerDirection = CubeCoordinate.GetSlidingMoveTypeBetween(KingIndex, move.DestinationIndex);
                var blockerToAttackerDirection = CubeCoordinate.GetSlidingMoveTypeBetween(move.DestinationIndex, AttackingPieceIndex);

                if (kingToBlockerDirection.HasValue && blockerToAttackerDirection.HasValue && kingToBlockerDirection == blockerToAttackerDirection)
                {
                    return true;
                }
            }

            // Capturing the attacking piece
            if (move.DestinationIndex == AttackingPieceIndex)
            {
                return true;
            }

            if (move.Flags == Move.MoveFlags.EnPassantCapture && move.EnPassantIndex == AttackingPieceIndex)
            {
                return true;
            }

            // No other option, the move doesn't resolve the check
            return false;
        }
    }
}
