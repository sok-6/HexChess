using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Screens.ChessScreen
{
    internal record InitialisationOptions
    {
        public PlayerType WhitePlayer { get; set; } = PlayerType.Human;
        public PlayerType BlackPlayer { get; set; } = PlayerType.Human;
        public string FenString { get; set; } = null;

        public enum PlayerType
        {
            Human,
            Ai
        }
    }
}
