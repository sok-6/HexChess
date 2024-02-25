using HexChess.Core.AI.Data;
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

        public DefaultAi()
        {
            var path = System.IO.Directory.GetCurrentDirectory() + @"\AiData\DefaultAi.json";
            var text = System.IO.File.ReadAllText(path);
            var rawData = Newtonsoft.Json.JsonConvert.DeserializeObject<DefaultAiData>(text);

            _data = new ProcessedData(rawData);
        }

        public Move ChooseMove(BoardState boardState)
        {
            Move bestMove = null;
            int bestScore = int.MinValue;

            foreach (var move in boardState.CurrentMoves)
            {
                boardState.MakeMove(move, false);
                var evaluation = Evaluate(boardState);
                boardState.UnmakeMove(false);

                if (bestMove == null || evaluation > bestScore)
                {
                    bestMove = move;
                    bestScore = evaluation;
                }
            }

            return bestMove;
        }

        public int Evaluate(BoardState boardState)
        {
            if (boardState.GameState == GameState.Checkmate)
            {
                // Game over man, game over
                return int.MinValue;
            }

            if (boardState.GameState == GameState.Stalemate)
            {
                return 0; // TODO: How to represent the fact that stalemate is 3/4 of a win to the forcing side?
            }

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
