namespace HexChess.Core.AI.Data
{
    public record PieceCellTableData
    {
        public int[] PawnCellTable { get; set; }
        public int[] BishopCellTable { get; set; }
        public int[] KnightCellTable { get; set; }
        public int[] RookCellTable { get; set; }
        public int[] QueenCellTable { get; set; }
        public int[] KingCellTable { get; set; }
    }
}
