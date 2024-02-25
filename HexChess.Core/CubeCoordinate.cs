using System.Data.Common;

namespace HexChess.Core
{
    public class CubeCoordinate
    {
        public int Q { get; private set; }
        public int R { get; private set; }
        public int S { get; private set; }

        public CubeCoordinate(int q, int r, int s)
        {
            if (q + r + s != 0)
            {
                throw new Exception($"Cube coordinates must sum to 0: Q={q} R={r} S={s}");
            }

            Q = q;
            R = r;
            S = s;
        }

        public CubeCoordinate Step(MovementDirection direction, int steps = 1)
        {
            CubeCoordinate offset;

            switch (direction)
            {
                case MovementDirection.Up: offset = new CubeCoordinate(0, -steps, steps); break;
                case MovementDirection.UpRight: offset = new CubeCoordinate(steps, -steps, 0); break;
                case MovementDirection.DownRight: offset = new CubeCoordinate(steps, 0, -steps); break;
                case MovementDirection.Down: offset = new CubeCoordinate(0, steps, -steps); break;
                case MovementDirection.DownLeft: offset = new CubeCoordinate(-steps, steps, 0); break;
                case MovementDirection.UpLeft: offset = new CubeCoordinate(-steps, 0, steps); break;
                case MovementDirection.DiagonalUpRight: offset = new CubeCoordinate(steps, -2 * steps, steps); break;
                case MovementDirection.DiagonalRight: offset = new CubeCoordinate(2 * steps, -steps, -steps); break;
                case MovementDirection.DiagonalDownRight: offset = new CubeCoordinate(steps, steps, -2 * steps); break;
                case MovementDirection.DiagonalDownLeft: offset = new CubeCoordinate(-steps, 2 * steps, -steps); break;
                case MovementDirection.DiagonalLeft: offset = new CubeCoordinate(-2 * steps, steps, steps); break;
                case MovementDirection.DiagonalUpLeft: offset = new CubeCoordinate(-steps, -steps, 2 * steps); break;
                default: offset = new CubeCoordinate(0, 0, 0); break;
            }

            return this + offset;
        }

        public static CubeCoordinate FromArrayIndex(int index) => CoordinateHelpers.BOARD_COORDINATES[index];

        public static CubeCoordinate FromAlgebraic(string algebraic)
        {
            var q = algebraic[0] - 'a' - 5;

            if (q < 0)
            {
                var r = 6 - (algebraic[1] - '0');
                return new CubeCoordinate(q, r, -q - r);
            }
            else
            {
                var s = (algebraic[1] - '0') - 6;
                return new CubeCoordinate(q, -q - s, s);
            }
        }

        public static CubeCoordinate Zero => new CubeCoordinate(0, 0, 0);

        public static CubeCoordinate operator +(CubeCoordinate a, CubeCoordinate b)
        {
            return new CubeCoordinate(a.Q + b.Q, a.R + b.R, a.S + b.S);
        }

        public static bool operator ==(CubeCoordinate a, CubeCoordinate b)
        {
            if (ReferenceEquals(a, b)) return true;

            if (ReferenceEquals(b, null)) return false;
            if (ReferenceEquals(a, null)) return false;

            return a.Equals(b);
        }

        public static bool operator !=(CubeCoordinate a, CubeCoordinate b) => !(a == b);

        public override bool Equals(object? obj)
        {
            if (Object.ReferenceEquals(null, obj))
            {
                return false;
            }

            var that = obj as CubeCoordinate;

            if (Object.ReferenceEquals(null, that))
            {
                return false;
            }

            return this.Q == that.Q && this.R == that.R && this.S == that.S;
        }

        public override string ToString() => $"({Q},{R},{S})";
    }
}
