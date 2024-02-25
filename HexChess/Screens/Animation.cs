using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexChess.Screens
{
    internal class Animation
    {
        public Texture2D Sprite { get; private set; }
        public Vector2 StartPosition { get; private set; }
        public Vector2 EndPosition { get; private set; }
        public int TotalMs { get; private set; }
        public int StartMs { get; private set; }

        public bool IsComplete { get; private set; }


        public Animation(Texture2D sprite, Vector2 startPosition, Vector2 endPosition, int totalMs, int startMs)
        {
            Sprite = sprite;
            StartPosition = startPosition;
            EndPosition = endPosition;
            TotalMs = totalMs;
            StartMs = startMs;

            IsComplete = false;
        }

        public void Draw(SpriteBatch spriteBatch, float scale, int currentMs)
        {
            if (currentMs - StartMs > TotalMs)
            {
                IsComplete = true;
            }

            var position = IsComplete ? EndPosition : Vector2.Lerp(StartPosition, EndPosition, (float)(currentMs - StartMs) / TotalMs);

            spriteBatch.DrawCenteredSprite(Sprite, position, scale);
        }
    }
}
