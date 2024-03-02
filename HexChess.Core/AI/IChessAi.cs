namespace HexChess.Core.AI
{
    public interface IChessAi
    {
        Move ChooseMove(BoardState boardState);

        int Evaluate(BoardState boardState);

        ThreadSafeString ProgressString { get; } 
    }
}
