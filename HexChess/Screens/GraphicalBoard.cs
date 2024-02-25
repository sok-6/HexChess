using HexChess.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.VectorDraw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Screens
{
    internal class GraphicalBoard
    {
        public IReadOnlyList<HexCell> Cells => _cells;
        private HexCell[] _cells;

        public bool IsWhitesPerspective { get; private set; }
        public Vector2 BoardCentre { get; private set; }

        private const int MOVE_ANIMATION_TOTAL_MS = 250;
        private Animation _animation;
        private CubeCoordinate _animationEndCell;

        public GraphicalBoard(Vector2 boardCentre)
        {
            _cells = CoordinateHelpers.BOARD_COORDINATES.Select(x => new HexCell(x)).ToArray();
            IsWhitesPerspective = true;

            BoardCentre = boardCentre;

            _animation = null;
            _animationEndCell = null;
        }

        public CubeCoordinate GetClickedCell(Vector2 mousePosition)
        {
            var q = 2.0 / 3 * (mousePosition.X - BoardCentre.X) / HexCell.HEX_SIZE;
            var r = (-1.0 / 3 * (mousePosition.X - BoardCentre.X) + Math.Sqrt(3) / 3 * (mousePosition.Y - BoardCentre.Y)) / HexCell.HEX_SIZE;
            var s = -q - r;

            var roundedQ = Math.Round(q);
            var roundedR = Math.Round(r);
            var roundedS = Math.Round(s);

            var qDiff = Math.Abs(roundedQ - q);
            var rDiff = Math.Abs(roundedR - r);
            var sDiff = Math.Abs(roundedS - s);

            if (qDiff > rDiff && qDiff > sDiff)
                roundedQ = -roundedR - roundedS;
            else if (rDiff > sDiff)
                roundedR = -roundedQ - roundedS;
            else
                roundedS = -roundedQ - roundedR;

            // If from black's perspective, invert all coordinates
            if (!IsWhitesPerspective)
            {
                roundedQ = -roundedQ;
                roundedR = -roundedR;
                roundedS = -roundedS;
            }

            var result = new CubeCoordinate((int)roundedQ, (int)roundedR, (int)roundedS);

            return result.IsOnBoard() ? result : null;
        }

        public void SetCellColourOverride(CubeCoordinate coordinate, HexCell.ColourOverride colourOverride)
        {
            _cells[coordinate.ToArrayIndex()].Override = colourOverride;
        }

        public void SetCellColourOverride(int cellIndex, HexCell.ColourOverride colourOverride)
        {
            _cells[cellIndex].Override = colourOverride;
        }

        public void ClearAllCellColourOverrides()
        {
            foreach (var cell in _cells)
            {
                cell.Override = HexCell.ColourOverride.None;
            }
        }

        public void DrawCells(PrimitiveDrawing primitiveDrawing)
        {
            foreach (var cell in _cells)
            {
                cell.DrawFill(primitiveDrawing, BoardCentre, IsWhitesPerspective);
            }
        }

        public void DrawData(SpriteBatch spriteBatch, SpriteFont font)
        {
            foreach (var cell in _cells.Where(d => d.Data != null))
            {
                spriteBatch.DrawCenteredString(font, cell.Data, cell.GetHexCentre(BoardCentre, IsWhitesPerspective), Color.Black);
            }
        }

        public void DrawPieces(SpriteBatch spriteBatch, BoardState boardState, CubeCoordinate dragStartCell, Point mousePosition, int currentMs)
        {
            const float SCALE = 0.5f;

            foreach (var cell in _cells)
            {
                // Skip dragged piece, it will be drawn at the end
                if (cell.Coordinate == dragStartCell) continue;

                // Skip any animating pieces too
                if (cell.Coordinate == _animationEndCell) continue;

                var sprite = PieceToSprite(boardState.GetPieceInCell(cell.Coordinate.ToArrayIndex()));

                if (sprite != null)
                {
                    spriteBatch.DrawCenteredSprite(sprite, cell.GetHexCentre(BoardCentre, IsWhitesPerspective), SCALE);
                }
            }

            if (dragStartCell != null)
            {
                spriteBatch.DrawCenteredSprite(PieceToSprite(boardState.GetPieceInCell(dragStartCell.ToArrayIndex())), mousePosition.ToVector2(), SCALE);
            }

            if (_animation != null)
            {
                _animation.Draw(spriteBatch, SCALE, currentMs);

                if (_animation.IsComplete)
                {
                    _animation = null;
                    _animationEndCell = null;
                }
            }
        }

        private static Texture2D PieceToSprite(Piece piece)
        {
            Texture2D sprite = null;

            if (piece == (Piece.White | Piece.Pawn)) sprite = SpriteLibrary.WhitePawn;
            if (piece == (Piece.White | Piece.Bishop)) sprite = SpriteLibrary.WhiteBishop;
            if (piece == (Piece.White | Piece.Knight)) sprite = SpriteLibrary.WhiteKnight;
            if (piece == (Piece.White | Piece.Rook)) sprite = SpriteLibrary.WhiteRook;
            if (piece == (Piece.White | Piece.Queen)) sprite = SpriteLibrary.WhiteQueen;
            if (piece == (Piece.White | Piece.King)) sprite = SpriteLibrary.WhiteKing;
            if (piece == (Piece.Black | Piece.Pawn)) sprite = SpriteLibrary.BlackPawn;
            if (piece == (Piece.Black | Piece.Bishop)) sprite = SpriteLibrary.BlackBishop;
            if (piece == (Piece.Black | Piece.Knight)) sprite = SpriteLibrary.BlackKnight;
            if (piece == (Piece.Black | Piece.Rook)) sprite = SpriteLibrary.BlackRook;
            if (piece == (Piece.Black | Piece.Queen)) sprite = SpriteLibrary.BlackQueen;
            if (piece == (Piece.Black | Piece.King)) sprite = SpriteLibrary.BlackKing;

            return sprite;
        }

        public void SetPerspective(bool isWhitesPerspective)
        {
            IsWhitesPerspective = isWhitesPerspective;
        }

        internal void InitialiseAnimation(BoardState boardState, Move move, int startMs)
        {
            var sprite = PieceToSprite(boardState.GetPieceInCell(move.StartIndex));
            var startPosition = _cells.First(c => c.Coordinate == move.StartCoordinate).GetHexCentre(BoardCentre, IsWhitesPerspective);
            var endPosition = _cells.First(c => c.Coordinate == move.DestinationCoordinate).GetHexCentre(BoardCentre, IsWhitesPerspective);

            _animation = new Animation(sprite, startPosition, endPosition, MOVE_ANIMATION_TOTAL_MS, startMs);
            _animationEndCell = move.DestinationCoordinate;
        }
    }
}
