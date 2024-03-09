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
            whiteScore += boardState.WhitePawnIndices.Count() * _data.PieceValues.PawnValue;// .Sum(i => _data.PieceValues.PawnValue * _data.PieceCellTables.WhitePawnCellTable[i]);
            whiteScore += boardState.WhiteBishopIndices.Count() * _data.PieceValues.BishopValue;// .Sum(i => _data.PieceValues.BishopValue * _data.PieceCellTables.BishopCellTable[i]);
            whiteScore += boardState.WhiteKnightIndices.Count() * _data.PieceValues.KnightValue;// .Sum(i => _data.PieceValues.KnightValue * _data.PieceCellTables.KnightCellTable[i]);
            whiteScore += boardState.WhiteRookIndices.Count() * _data.PieceValues.RookValue;// .Sum(i => _data.PieceValues.RookValue * _data.PieceCellTables.RookCellTable[i]);
            whiteScore += boardState.WhiteQueenIndices.Count() * _data.PieceValues.QueenValue;// .Sum(i => _data.PieceValues.QueenValue * _data.PieceCellTables.QueenCellTable[i]);

            whiteScore += boardState.WhitePawnIndices.Sum(i => _data.PieceCellTables.WhitePawnCellTable[i]);
            whiteScore += boardState.WhiteBishopIndices.Sum(i => _data.PieceCellTables.WhiteBishopCellTable[i]);
            whiteScore += boardState.WhiteKnightIndices.Sum(i => _data.PieceCellTables.WhiteKnightCellTable[i]);
            whiteScore += boardState.WhiteRookIndices.Sum(i => _data.PieceCellTables.WhiteRookCellTable[i]);
            whiteScore += boardState.WhiteQueenIndices.Sum(i => _data.PieceCellTables.WhiteQueenCellTable[i]);
            whiteScore += _data.PieceCellTables.WhiteKingCellTable[boardState.WhiteKingIndex];

            // Get black's piece scores
            var blackScore = 0;
            blackScore += boardState.BlackPawnIndices.Count() * _data.PieceValues.PawnValue;
            blackScore += boardState.BlackBishopIndices.Count() * _data.PieceValues.BishopValue;
            blackScore += boardState.BlackKnightIndices.Count() * _data.PieceValues.KnightValue;
            blackScore += boardState.BlackRookIndices.Count() * _data.PieceValues.RookValue;
            blackScore += boardState.BlackQueenIndices.Count() * _data.PieceValues.QueenValue;

            blackScore += boardState.BlackPawnIndices.Sum(i => _data.PieceCellTables.BlackPawnCellTable[i]);
            blackScore += boardState.BlackBishopIndices.Sum(i => _data.PieceCellTables.BlackBishopCellTable[i]);
            blackScore += boardState.BlackKnightIndices.Sum(i => _data.PieceCellTables.BlackKnightCellTable[i]);
            blackScore += boardState.BlackRookIndices.Sum(i => _data.PieceCellTables.BlackRookCellTable[i]);
            blackScore += boardState.BlackQueenIndices.Sum(i => _data.PieceCellTables.BlackQueenCellTable[i]);
            blackScore += _data.PieceCellTables.BlackKingCellTable[boardState.BlackKingIndex];

            // Evaluation is side to play's score - opponent's score
            var evaluation = boardState.IsWhitesTurn ? (whiteScore - blackScore) : (blackScore - whiteScore);

            // Add mop up evaluation for end game strategy
            evaluation += MopUpEvaluation(boardState);

            return evaluation;
        }

        private int MopUpEvaluation(BoardState boardState)
        {
            var evaluation = 0;

            var friendlyKingCoordinate = boardState.IsWhitesTurn ? CubeCoordinate.FromArrayIndex(boardState.WhiteKingIndex) : CubeCoordinate.FromArrayIndex(boardState.BlackKingIndex);
            var enemyKingCoordinate = boardState.IsWhitesTurn ? CubeCoordinate.FromArrayIndex(boardState.BlackKingIndex) : CubeCoordinate.FromArrayIndex(boardState.WhiteKingIndex);

            var friendlyKingDistanceToCentre = CubeCoordinate.DistanceBetween(CubeCoordinate.Zero, friendlyKingCoordinate);
            var enemyKingDistanceToCentre = CubeCoordinate.DistanceBetween(CubeCoordinate.Zero, enemyKingCoordinate);

            // Incentivise moving the opponent's king to the edge of the board, fewer escape routes
            evaluation += enemyKingDistanceToCentre * _data.StrategyWeights.MopUpEnemyKingCentreDistanceWeight / 100;

            // Incentivise moving the friendly king towards the enemy king to assist with mate
            var distanceBetweenKings = CubeCoordinate.DistanceBetween(friendlyKingCoordinate, enemyKingCoordinate);

            // (11-distance) as 11 is the furthest away they could be, the closer they are the better
            evaluation += (11 - distanceBetweenKings) * _data.StrategyWeights.MopUpDistanceBetweenKingsWeight / 100;

            // Mop up evaluation is stronger when fewer enemy pieces on the board
            var enemyPieceCount = GetEnemyPieceCount(boardState);

            return evaluation * (18 - enemyPieceCount) * _data.StrategyWeights.MopUpEnemyPieceCountWeight / 100;
        }

        private int GetEnemyPieceCount(BoardState boardState)
        {
            if (boardState.IsWhitesTurn)
            {
                return
                    boardState.BlackPawnIndices.Count() +
                    boardState.BlackBishopIndices.Count() +
                    boardState.BlackKnightIndices.Count() +
                    boardState.BlackRookIndices.Count() +
                    boardState.BlackQueenIndices.Count() +
                    1; // King
            }
            else
            {
                return
                    boardState.WhitePawnIndices.Count() +
                    boardState.WhiteBishopIndices.Count() +
                    boardState.WhiteKnightIndices.Count() +
                    boardState.WhiteRookIndices.Count() +
                    boardState.WhiteQueenIndices.Count() +
                    1; // King
            }
        }

        private class ProcessedData
        {
            public ProcessedPieceValueData PieceValues { get; set; }
            public ProcessedPieceCellTableData PieceCellTables { get; set; }
            public ProcessedStrategyWeights StrategyWeights { get; set; }

            public ProcessedData(DefaultAiData rawData)
            {
                PieceValues = new ProcessedPieceValueData
                {
                    PawnValue = rawData.PieceValues.PawnValue,
                    BishopValue = rawData.PieceValues.BishopValue,
                    KnightValue = rawData.PieceValues.KnightValue,
                    RookValue = rawData.PieceValues.RookValue,
                    QueenValue = rawData.PieceValues.QueenValue
                };

                PieceCellTables = new ProcessedPieceCellTableData
                {
                    WhitePawnCellTable = rawData.PieceCellTables.PawnCellTable,
                    WhiteBishopCellTable = rawData.PieceCellTables.BishopCellTable,
                    WhiteKnightCellTable = rawData.PieceCellTables.KnightCellTable,
                    WhiteRookCellTable = rawData.PieceCellTables.RookCellTable,
                    WhiteQueenCellTable = rawData.PieceCellTables.QueenCellTable,
                    WhiteKingCellTable = rawData.PieceCellTables.KingCellTable,

                    BlackPawnCellTable = rawData.PieceCellTables.PawnCellTable.Reverse().ToArray(),
                    BlackBishopCellTable = rawData.PieceCellTables.BishopCellTable.Reverse().ToArray(),
                    BlackKnightCellTable = rawData.PieceCellTables.KnightCellTable.Reverse().ToArray(),
                    BlackRookCellTable = rawData.PieceCellTables.RookCellTable.Reverse().ToArray(),
                    BlackQueenCellTable = rawData.PieceCellTables.QueenCellTable.Reverse().ToArray(),
                    BlackKingCellTable = rawData.PieceCellTables.KingCellTable.Reverse().ToArray()
                };

                StrategyWeights = new ProcessedStrategyWeights
                {
                    MopUpDistanceBetweenKingsWeight = rawData.StrategyWeights.MopUpDistanceBetweenKingsWeight,
                    MopUpEnemyKingCentreDistanceWeight = rawData.StrategyWeights.MopUpEnemyKingCentreDistanceWeight,
                    MopUpEnemyPieceCountWeight = rawData.StrategyWeights.MopUpEnemyPieceCountWeight
                };
            }

            public class ProcessedPieceValueData
            {
                public int PawnValue { get; set; }
                public int BishopValue { get; set; }
                public int KnightValue { get; set; }
                public int RookValue { get; set; }
                public int QueenValue { get; set; }
            }

            public class ProcessedPieceCellTableData
            {
                public int[] WhitePawnCellTable { get; set; }
                public int[] BlackPawnCellTable { get; set; }
                public int[] WhiteBishopCellTable { get; set; }
                public int[] BlackBishopCellTable { get; set; }
                public int[] WhiteKnightCellTable { get; set; }
                public int[] BlackKnightCellTable { get; set; }
                public int[] WhiteRookCellTable { get; set; }
                public int[] BlackRookCellTable { get; set; }
                public int[] WhiteQueenCellTable { get; set; }
                public int[] BlackQueenCellTable { get; set; }
                public int[] WhiteKingCellTable { get; set; }
                public int[] BlackKingCellTable { get; set; }
            }

            public class ProcessedStrategyWeights
            {
                public int MopUpEnemyKingCentreDistanceWeight { get; set; }
                public int MopUpDistanceBetweenKingsWeight { get; set; }
                public int MopUpEnemyPieceCountWeight { get; set; }
            }
        }

    }
}
