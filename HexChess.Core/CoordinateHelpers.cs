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

        public static IReadOnlyList<CubeCoordinate> BOARD_COORDINATES => _board_coordinates.Value;
        private static readonly Lazy<IReadOnlyList<CubeCoordinate>> _board_coordinates =
            new Lazy<IReadOnlyList<CubeCoordinate>>(() =>
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
        });
    }
}
