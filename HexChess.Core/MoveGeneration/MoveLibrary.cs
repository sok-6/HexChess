using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core.MoveGeneration
{
    internal static class MoveLibrary
    {
        public static int[][] WhitePawnMoves { get; private set; }
        public static int[][] WhitePawnCaptures { get; private set; }
        public static int[] WhitePawnPromotionIndices { get; private set; }
        public static int[][] BlackPawnMoves { get; private set; }
        public static int[][] BlackPawnCaptures { get; private set; }
        public static int[] BlackPawnPromotionIndices { get; private set; }

        public static int[][] KnightMoves { get; private set; }
        public static int[][] KingMoves { get; private set; }

        public static void PopulateMoves()
        {
            // Initialise all sets
            WhitePawnMoves = new int[91][];
            BlackPawnMoves = new int[91][];

            WhitePawnCaptures = new int[91][];
            BlackPawnCaptures = new int[91][];

            KnightMoves = new int[91][];

            KingMoves = new int[91][];

            // Lookups
            var whitePawnDoubleMoveCells = new int[] { 30, 31, 32, 33, 34, 35, 25, 17, 10, 4 };

            // Helper variables
            var destinations = new List<int>();
            CubeCoordinate candidate;

            for (int i = 0; i < 91; i++)
            {
                var startCell = CubeCoordinate.FromArrayIndex(i);

                // *** Pawn moves ***
                destinations.Clear();

                // Single step
                candidate = startCell.Step(MovementDirection.Up);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                // Check for double step
                if (whitePawnDoubleMoveCells.Contains(i)) destinations.Add(startCell.Step(MovementDirection.Up, 2).ToArrayIndex());

                // Add moves for each - black moves are rotated versions of white moves
                WhitePawnMoves[i] = destinations.ToArray();
                BlackPawnMoves[RotateBoard(i)] = destinations.Select(d => RotateBoard(d)).ToArray();

                // *** Pawn captures ***
                destinations.Clear();

                candidate = startCell.Step(MovementDirection.UpLeft);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                candidate = startCell.Step(MovementDirection.UpRight);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                // Add captures for each - black captures are rotated versions
                WhitePawnCaptures[i] = destinations.ToArray();
                BlackPawnCaptures[RotateBoard(i)] = destinations.Select(d => RotateBoard(d)).ToArray();

                // *** Pawn promotions ***
                WhitePawnPromotionIndices = new int[] { 50, 60, 69, 77, 84, 85, 86, 87, 88, 89, 90 };
                BlackPawnPromotionIndices = new int[] { 0, 1, 2, 3, 4, 5, 6, 13, 21, 30, 40 };

                // *** Knight Moves ***
                destinations.Clear();

                candidate = startCell.Step(MovementDirection.Up, 2).Step(MovementDirection.UpLeft);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                candidate = startCell.Step(MovementDirection.Up, 2).Step(MovementDirection.UpRight);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                candidate = startCell.Step(MovementDirection.UpRight, 2).Step(MovementDirection.Up);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                candidate = startCell.Step(MovementDirection.UpRight, 2).Step(MovementDirection.DownRight);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                candidate = startCell.Step(MovementDirection.DownRight, 2).Step(MovementDirection.UpRight);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                candidate = startCell.Step(MovementDirection.DownRight, 2).Step(MovementDirection.Down);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                candidate = startCell.Step(MovementDirection.Down, 2).Step(MovementDirection.DownRight);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                candidate = startCell.Step(MovementDirection.Down, 2).Step(MovementDirection.DownLeft);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                candidate = startCell.Step(MovementDirection.DownLeft, 2).Step(MovementDirection.Down);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                candidate = startCell.Step(MovementDirection.DownLeft, 2).Step(MovementDirection.UpLeft);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                candidate = startCell.Step(MovementDirection.UpLeft, 2).Step(MovementDirection.DownLeft);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                candidate = startCell.Step(MovementDirection.UpLeft, 2).Step(MovementDirection.Up);
                if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());

                KnightMoves[i] = destinations.ToArray();

                // *** King Moves ***
                destinations.Clear();

                foreach (MovementDirection direction in Enum.GetValues(typeof(MovementDirection)))
                {
                    candidate = startCell.Step(direction);
                    if (candidate.IsOnBoard()) destinations.Add(candidate.ToArrayIndex());
                }

                KingMoves[i] = destinations.ToArray();
            }
        }

        private static int RotateBoard(int index) => 90 - index;
    }
}
