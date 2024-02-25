using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    public class Pin
    {
        public int PinnedPieceIndex { get; private set; }
        public int[] PossibleDestinationIndices { get; private set; }

        public Pin(int pinnedPieceIndex, int[] possibleDestinationIndices)
        {
            PinnedPieceIndex = pinnedPieceIndex;
            PossibleDestinationIndices = possibleDestinationIndices;
        }
    }
}
