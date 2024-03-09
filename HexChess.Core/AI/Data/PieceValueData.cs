using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core.AI.Data
{
    public record PieceValueData
    {
        public int PawnValue { get; set; }
        public int BishopValue { get; set; }
        public int KnightValue { get; set; }
        public int RookValue { get; set; }
        public int QueenValue { get; set; }
    }
}
