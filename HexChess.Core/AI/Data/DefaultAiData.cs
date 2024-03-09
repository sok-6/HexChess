namespace HexChess.Core.AI.Data
{
    public record DefaultAiData
    {
        public PieceValueData PieceValues { get; set; }
        public PieceCellTableData PieceCellTables { get; set; }
        public StrategyWeights StrategyWeights { get; set; }
    }
}
