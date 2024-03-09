using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Core
{
    public static class Initialisation
    {
        public static void Initialise()
        {
            NLog.LogManager.GetCurrentClassLogger().Debug("Derpy doo");

            // Populate the list of moves for non-sliding pieces
            MoveGeneration.MoveLibrary.PopulateMoves();

            // Get the hash values for the zobrist hashing scheme
            ZobristHelper.GenerateZobristHashValues();

            //CreateDefaultAiJsonString();
        }

        private static void CreateDefaultAiJsonString()
        {
            var defaultAiData = new AI.Data.DefaultAiData()
            {
                PieceValues = new AI.Data.PieceValueData()
                {
                    PawnValue = 100,
                    BishopValue = 300,
                    KnightValue = 350,
                    RookValue = 550,
                    QueenValue = 1000
                },
                PieceCellTables = new AI.Data.PieceCellTableData()
                {
                    PawnCellTable = new int[91],
                    BishopCellTable = new int[91],
                    KnightCellTable = new int[91],
                    RookCellTable = new int[91],
                    QueenCellTable = new int[91],
                    KingCellTable = new int[91],
                }
            };

            var s = Newtonsoft.Json.JsonConvert.SerializeObject(defaultAiData, Newtonsoft.Json.Formatting.Indented);
        }
    }
}
