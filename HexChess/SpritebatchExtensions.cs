using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace HexChess
{
    internal static class SpritebatchExtensions
    {
        public static void DrawCenteredString(this SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color colour)
        {
            var renderedSize = font.MeasureString(text);

            sb.DrawString(font, text, position - (renderedSize / 2), colour);
        }

        public static void DrawCenteredSprite(this SpriteBatch sb, Texture2D texture, Vector2 position, float scale) 
        {
            var origin = new Vector2(texture.Width / 2, texture.Height / 2);

            sb.Draw(texture, position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
