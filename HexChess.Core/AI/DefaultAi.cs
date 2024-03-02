using HexChess.Core.AI.Data;
using HexChess.Core.MoveGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core.AI
{
    public class DefaultAi : IChessAi
    {
        private Random _rnd = new Random();
        private ProcessedData _data;

        public ThreadSafeString ProgressString { get; }

        public DefaultAi()
        {
            var path = System.IO.Directory.GetCurrentDirectory() + @"\AiData\DefaultAi.json";
            var text = System.IO.File.ReadAllText(path);
            var rawData = Newtonsoft.Json.JsonConvert.DeserializeObject<DefaultAiData>(text);

            ProgressString = new ThreadSafeString();

            _data = new ProcessedData(rawData);
        }

        public Move ChooseMove(BoardState boardState)
        {
            var _boardStateCopy = new BoardState(boardState);

            Move bestMove = null;
            int bestScore = int.MinValue;

            var moves = MoveGeneration.MoveGenerator.GetMoves(_boardStateCopy);

            var moveCounter = 0;

            foreach (var move in moves)
            {
                moveCounter++;
                ProgressString.Write($"Searching move {moveCounter} of {moves.Count()}");

                _boardStateCopy.MakeMove(move);
                var evaluation = -Search(_boardStateCopy, 2, int.MinValue + 1, int.MaxValue - 1); // +1 and -1 to prevent weird overflow issues
                _boardStateCopy.UnmakeMove();

                if (bestMove == null || evaluation > bestScore)
                {
                    bestMove = move;
                    bestScore = evaluation;
                }
            }

            ProgressString.Write("Search complete");

            return bestMove;
        }

        private int Search(BoardState boardState, int depth, int alpha, int beta)
        {
            if (depth == 0)
            {
                return SearchOnlyCaptures(boardState, alpha, beta);
            }

            var moves = MoveGenerator.GetMoves(boardState);

            var gameState = boardState.UpdateGameState(moves.Count());

            if (gameState == GameState.Checkmate)
            {
                return int.MinValue;
            }
            else if (gameState == GameState.Draw)
            {
                return 0; // TODO: How to take into account a stalemate is 3/4s win?
            }

            foreach (var move in moves)
            {
                boardState.MakeMove(move);
                var evaluation = -Search(boardState, depth - 1, -beta, -alpha);
                boardState.UnmakeMove();

                if (evaluation >= beta)
                    return beta;

                alpha = Math.Max(alpha, evaluation);
            }

            return alpha;
        }

        private int SearchOnlyCaptures(BoardState boardState, int alpha, int beta)
        {
            var evaluation = Evaluate(boardState);
            if (evaluation >= beta)
            {
                return beta;
            }

            alpha = Math.Max(alpha, evaluation);

            var moves = MoveGenerator.GetMoves(boardState, true);

            foreach (var move in moves)
            {
                boardState.MakeMove(move);
                evaluation = -SearchOnlyCaptures(boardState, -beta, -alpha);
                boardState.UnmakeMove();

                if (evaluation >= beta)
                {
                    return beta;
                }
                alpha = Math.Max(alpha, evaluation);
            }

            return alpha;
        }

        public int Evaluate(BoardState boardState)
        {
            // Get white's piece scores
            var whiteScore = 0;
            whiteScore += boardState.WhitePawnIndices.Sum(i => _data.PieceValues.PawnValue * _data.PieceCellTables.WhitePawnCellTable[i]);
            whiteScore += boardState.WhiteBishopIndices.Sum(i => _data.PieceValues.BishopValue * _data.PieceCellTables.BishopCellTable[i]);
            whiteScore += boardState.WhiteKnightIndices.Sum(i => _data.PieceValues.KnightValue * _data.PieceCellTables.KnightCellTable[i]);
            whiteScore += boardState.WhiteRookIndices.Sum(i => _data.PieceValues.RookValue * _data.PieceCellTables.RookCellTable[i]);
            whiteScore += boardState.WhiteQueenIndices.Sum(i => _data.PieceValues.QueenValue * _data.PieceCellTables.QueenCellTable[i]);
            whiteScore += _data.PieceValues.KingValue * _data.PieceCellTables.KingCellTable[boardState.WhiteKingIndex];

            // Get black's piece scores
            var blackScore = 0;
            blackScore += boardState.BlackPawnIndices.Sum(i => _data.PieceValues.PawnValue * _data.PieceCellTables.BlackPawnCellTable[i]);
            blackScore += boardState.BlackBishopIndices.Sum(i => _data.PieceValues.BishopValue * _data.PieceCellTables.BishopCellTable[i]);
            blackScore += boardState.BlackKnightIndices.Sum(i => _data.PieceValues.KnightValue * _data.PieceCellTables.KnightCellTable[i]);
            blackScore += boardState.BlackRookIndices.Sum(i => _data.PieceValues.RookValue * _data.PieceCellTables.RookCellTable[i]);
            blackScore += boardState.BlackQueenIndices.Sum(i => _data.PieceValues.QueenValue * _data.PieceCellTables.QueenCellTable[i]);
            blackScore += _data.PieceValues.KingValue * _data.PieceCellTables.KingCellTable[boardState.BlackKingIndex];

            // Return side to play's score - opponent's score
            return boardState.IsWhitesTurn ? (whiteScore - blackScore) : (blackScore - whiteScore);
        }

        private class ProcessedData
        {
            public ProcessedPieceValueData PieceValues { get; set; }
            public ProcessedPieceCellTableData PieceCellTables { get; set; }

            public ProcessedData(DefaultAiData rawData)
            {
                PieceValues = new ProcessedPieceValueData
                {
                    PawnValue = rawData.PieceValues.PawnValue,
                    BishopValue = rawData.PieceValues.BishopValue,
                    KnightValue = rawData.PieceValues.KnightValue,
                    RookValue = rawData.PieceValues.RookValue,
                    QueenValue = rawData.PieceValues.QueenValue,
                    KingValue = rawData.PieceValues.KingValue,
                };

                PieceCellTables = new ProcessedPieceCellTableData
                {
                    WhitePawnCellTable = rawData.PieceCellTables.PawnCellTable,
                    BlackPawnCellTable = rawData.PieceCellTables.PawnCellTable.Reverse().ToArray(),
                    BishopCellTable = rawData.PieceCellTables.BishopCellTable,
                    KnightCellTable = rawData.PieceCellTables.KnightCellTable,
                    RookCellTable = rawData.PieceCellTables.RookCellTable,
                    QueenCellTable = rawData.PieceCellTables.QueenCellTable,
                    KingCellTable = rawData.PieceCellTables.KingCellTable
                };
            }
        }

        private class ProcessedPieceValueData
        {
            public int PawnValue { get; set; }
            public int BishopValue { get; set; }
            public int KnightValue { get; set; }
            public int RookValue { get; set; }
            public int QueenValue { get; set; }
            public int KingValue { get; set; }
        }

        private class ProcessedPieceCellTableData
        {
            public int[] WhitePawnCellTable { get; set; }
            public int[] BlackPawnCellTable { get; set; }
            public int[] BishopCellTable { get; set; }
            public int[] KnightCellTable { get; set; }
            public int[] RookCellTable { get; set; }
            public int[] QueenCellTable { get; set; }
            public int[] KingCellTable { get; set; }
        }
    }
}
