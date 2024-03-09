namespace HexChess.Core.AI.Data
{
    public record StrategyWeights
    {
        public int MopUpEnemyKingCentreDistanceWeight { get; set; }
        public int MopUpDistanceBetweenKingsWeight { get; set; }
        public int MopUpEnemyPieceCountWeight { get; set; }
    }
}
