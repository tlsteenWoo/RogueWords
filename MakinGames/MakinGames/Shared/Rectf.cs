using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MknGames
{
    public struct Rectf
    {
        public float X, Y, Width, Height;

        public static Rectf CreateCentered(Vector2 position, float width, float height)
        {
            return new Rectf(position.X - width/2, position.Y - height/2, width, height);
        }
        public Rectf(float X, float Y, float Width, float height_)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            Height = height_;
        }

        public Vector2 GetCenter()
        {
            return new Vector2(X + Width/2, Y + Height/2);
        }

        public Vector2 GetLocation()
        {
            return new Vector2(X, Y);
        }

        public Vector2 GetSize()
        {
            return new Vector2(Width, Height);
        }

        /// <summary>
        /// Inside, on edge doesnt count
        /// </summary>
        /// <param name="rectf"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector2 point)
        {
            float difx = point.X -X;
            float dify = point.Y - Y;
            return difx > 0 && dify > 0 && difx < Width && dify < Height;
        }

        static public implicit operator Rectf(Rectangle rect)
        {
            return new Rectf(rect.X, rect.Y, rect.Width, rect.Height);
        }
        static public explicit operator Rectangle(Rectf rectf)
        {
            return new Rectangle((int)rectf.X, (int)rectf.Y, (int)rectf.Width, (int)rectf.Height);
        }

        public float GetRight()
        {
            return X + Width;
        }
        public float GetBottom()
        {
            return Y + Height;
        }

        public bool Intersects(Rectangle rectangle)
        {
            if (rectangle.Left > GetRight())
                return false;
            if (rectangle.Right < X)
                return false;
            if (rectangle.Top > GetBottom())
                return false;
            if (rectangle.Bottom < Y)
                return false;
            return true;
        }
        public bool Intersectsf(Rectf rectf)
        {
            if (rectf.X > GetRight())
                return false;
            if (rectf.GetRight() < X)
                return false;
            if (rectf.Y > GetBottom())
                return false;
            if (rectf.GetBottom() < Y)
                return false;
            return true;
        }
    }
}
