using HexChess.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.VectorDraw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Screens
{
    internal class HexCell
    {
        public enum ColourOverride
        {
            None,
            Violet,
            Purple,
            Blue,
            Red,
            Green
        }

        public const float HEX_SIZE = 45;
        private enum CellColour
        {
            Light,
            Medium,
            Dark
        }

        public CubeCoordinate Coordinate { get; private set; }
        public int ArrayIndex => Coordinate.ToArrayIndex();
        public string Data { get; set; }
        public ColourOverride Override { get; set; }

        private Vector2[] _vertices;
        private CellColour cellColour;

        public HexCell(CubeCoordinate coord)
        {
            Coordinate = coord;

            _vertices = new Vector2[6];

            for (int i = 0; i < 6; i++)
            {
                _vertices[i] = new Vector2((float)(HEX_SIZE * Math.Cos(Math.PI * i / 3)), (float)(HEX_SIZE * Math.Sin(Math.PI * i / 3)));
            }

            switch ((coord.R - coord.S + 10) % 3)
            {
                case 0: cellColour = CellColour.Dark; break;
                case 1: cellColour = CellColour.Medium; break;
                case 2: cellColour = CellColour.Light; break;
                default:
                    break;
            }

            Data = $"{Coordinate} - {Coordinate.ToArrayIndex()}";

            Override = ColourOverride.None;
        }

        public Vector2 GetHexCentre(Vector2 boardCentre, bool isWhitesPerspective)
        {
            var fettledCoordinate = isWhitesPerspective ? Coordinate : new CubeCoordinate(-Coordinate.Q, -Coordinate.R, -Coordinate.S);

            return boardCentre + new Vector2(fettledCoordinate.Q * 1.5f * HEX_SIZE, (fettledCoordinate.R - fettledCoordinate.S) * (float)Math.Sqrt(3) * HEX_SIZE / 2);
        }

        public void DrawFill(PrimitiveDrawing primitiveDrawing, Vector2 boardCentre, bool isWhitesPerspective)
        {
            Color colour;

            switch (Override)
            {
                case ColourOverride.Violet: colour = ColourHelper.Violet; break;
                case ColourOverride.Purple: colour = ColourHelper.Purple; break;
                case ColourOverride.Blue: colour = ColourHelper.Blue; break;
                case ColourOverride.Red: colour = ColourHelper.Red; break;
                case ColourOverride.Green: colour = ColourHelper.Green; break;
                default: colour = ColourHelper.Base; break;
            }

            switch (cellColour)
            {
                case CellColour.Light:
                    colour = ColourHelper.Lighten(colour);
                    break;
                case CellColour.Dark:
                    colour = ColourHelper.Darken(colour);
                    break;
                default:
                    break;
            }

            primitiveDrawing.DrawSolidPolygon(GetHexCentre(boardCentre, isWhitesPerspective), _vertices, colour);
        }

        public void DrawFillSpecificColour(PrimitiveDrawing primitiveDrawing, Vector2 boardCentre, Color colour)
        {
            primitiveDrawing.DrawSolidPolygon(GetHexCentre(boardCentre, true), _vertices, colour);
        }

        public void DrawEdge(SpriteBatch spriteBatch, Vector2 boardCentre, Color colour)
        {
            var centre = GetHexCentre(boardCentre, true); // TODO: Need white and black perspective?

            spriteBatch.DrawLine(centre + _vertices[0], centre + _vertices[1], colour, 2);
            spriteBatch.DrawLine(centre + _vertices[1], centre + _vertices[2], colour, 2);
            spriteBatch.DrawLine(centre + _vertices[2], centre + _vertices[3], colour, 2);
            spriteBatch.DrawLine(centre + _vertices[3], centre + _vertices[4], colour, 2);
            spriteBatch.DrawLine(centre + _vertices[4], centre + _vertices[5], colour, 2);
            spriteBatch.DrawLine(centre + _vertices[5], centre + _vertices[0], colour, 2);
        }

        private static class ColourHelper
        {
            public static Color Base => new Color(233, 173, 112);
            public static Color Violet => new Color(0xD6, 0x5D, 0xB1);
            public static Color Purple => new Color(0x84, 0x5E, 0xC2);
            public static Color Blue => new Color(0x2C, 0x73, 0xD2);
            public static Color Red => new Color(0xC3, 0x4A, 0x36);
            public static Color Green => new Color(0x00, 0xC9, 0xA7);

            public static Color Lighten(Color colour)
            {
                const float RATE = 0.4f;
                var r = (int)Math.Min(255, colour.R + (255 - colour.R) * RATE);
                var g = (int)Math.Min(255, colour.G + (255 - colour.G) * RATE);
                var b = (int)Math.Min(255, colour.B + (255 - colour.B) * RATE);

                return new Color(r, g, b);
            }

            public static Color Darken(Color colour)
            {
                const float RATE = 0.85f;
                var r = (int)(colour.R * RATE);
                var g = (int)(colour.G * RATE);
                var b = (int)(colour.B * RATE);

                return new Color(r, g, b);
            }
        }
    }
}
