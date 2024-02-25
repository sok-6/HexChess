using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess
{
    internal static class SpriteLibrary
    {
        public static Texture2D WhitePawn { get; private set; }
        public static Texture2D WhiteBishop { get; private set; }
        public static Texture2D WhiteKnight { get; private set; }
        public static Texture2D WhiteRook { get; private set; }
        public static Texture2D WhiteQueen { get; private set; }
        public static Texture2D WhiteKing { get; private set; }
        public static Texture2D BlackPawn { get; private set; }
        public static Texture2D BlackBishop { get; private set; }
        public static Texture2D BlackKnight { get; private set; }
        public static Texture2D BlackRook { get; private set; }
        public static Texture2D BlackQueen { get; private set; }
        public static Texture2D BlackKing { get; private set; }

        public static void LoadSprites(ContentManager content)
        {
            WhitePawn = content.Load<Texture2D>("white-pawn");
            WhiteBishop = content.Load<Texture2D>("white-bishop");
            WhiteKnight = content.Load<Texture2D>("white-knight");
            WhiteRook = content.Load<Texture2D>("white-rook");
            WhiteQueen = content.Load<Texture2D>("white-queen");
            WhiteKing = content.Load<Texture2D>("white-king");
            BlackPawn = content.Load<Texture2D>("black-pawn");
            BlackBishop = content.Load<Texture2D>("black-bishop");
            BlackKnight = content.Load<Texture2D>("black-knight");
            BlackRook = content.Load<Texture2D>("black-rook");
            BlackQueen = content.Load<Texture2D>("black-queen");
            BlackKing = content.Load<Texture2D>("black-king");
        }
    }
}
