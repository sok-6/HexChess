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
        public MovementDirection PinDirection { get; private set; }

        public Pin(int pinnedPieceIndex, MovementDirection pinDirection)
        {
            PinnedPieceIndex = pinnedPieceIndex;
            PinDirection = pinDirection;
        }
    }
}
