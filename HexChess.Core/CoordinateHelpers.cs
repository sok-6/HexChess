using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    public static class CoordinateHelpers
    {
        private static readonly IReadOnlyList<int> FILE_OFFSETS = new int[] { 0, 7, 15, 24, 34, 45, 56, 66, 75, 83, 90 };

        public static int ToArrayIndex(this CubeCoordinate coord)
        {
            return FILE_OFFSETS[coord.S + 5] + coord.Q;
        }

        public static string ToAlgebraic(this CubeCoordinate coord)
        {
            return $"{(char)('a' + coord.Q + 5)}{6 - (coord.Q < 0 ? coord.R : -coord.S)}";
        }

        public static bool IsOnBoard(this CubeCoordinate coord) => 
            coord.Q > -6 && coord.Q < 6 && 
            coord.R > -6 && coord.R < 6 && 
            coord.S > -6 && coord.S < 6;

        public static IReadOnlyList<CubeCoordinate> BOARD_COORDINATES { get; } = GetBoardCoordinates();

        private static List<CubeCoordinate> GetBoardCoordinates()
        {
            var testBoard = new List<CubeCoordinate>();

            for (int i = -5; i <= 5; i++)
            {
                for (int j = -5; j <= 5; j++)
                {
                    var testCoord = new CubeCoordinate(i, j, -i - j);
                    if (testCoord.IsOnBoard())
                    {
                        testBoard.Add(testCoord);
                    }
                }
            }

            return testBoard.OrderBy(x => x.ToArrayIndex()).ToList();
        }

        /// <summary>
        /// Describes which direction would be used to get from a source cell to a destination cell. If it's not just one direction (e.g. knight move) lookup returns null
        /// </summary>
        public static IReadOnlyList<IReadOnlyList<MovementDirection?>> COORDINATE_PAIR_LINKS { get; } = GetCoordinatePairLinks();

        private static MovementDirection?[][] GetCoordinatePairLinks()
        {
            var result = new MovementDirection?[91][];

            for (int i = 0; i < 91; i++)
            {
                // Create empty set for this cell
                result[i] = new MovementDirection?[91];

                for (int j = 0; j < 91; j++)
                {
                    result[i][j] = null;
                }

                // Move in each of the 12 directions
                foreach (var direction in Enum.GetValues<MovementDirection>())
                {
                    var currentCell = CubeCoordinate.FromArrayIndex(i);

                    // Step continually in this direction until the edge of the board
                    while (true)
                    {
                        currentCell = currentCell.Step(direction);

                        if (currentCell.IsOnBoard() == false)
                        {
                            break;
                        }

                        // Update the lookup with the current direction
                        result[i][currentCell.ToArrayIndex()] = direction;
                    }
                }
            }

            return result;
        }
    }
}
