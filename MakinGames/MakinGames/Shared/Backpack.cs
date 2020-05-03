using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MknGames.Split_Screen_Dungeon
{
    public class Backpack
    {
        public struct Frame
        {
            public Rectangle bounds;

            public Frame(Rectangle basis)
            {
                bounds = basis;
            }

            public float relativeWidth(float percentage)
            {
                return percentage * (float)bounds.Width;
            }
            public float relativeHeight(float percentage)
            {
                return percentage * (float)bounds.Height;
            }
            public Rectangle relativeRect(float minX, float minY, float maxX, float maxY)
            {
                Rectangle result;
                Vector2 min = new Vector2(relativeWidth(minX), relativeHeight(minY));
                Vector2 max = new Vector2(relativeWidth(maxX), relativeHeight(maxY));
                result.X = (int)(bounds.X + min.X);
                result.Y = (int)(bounds.Y + min.Y);
                result.Width = (int)(max.X - min.X);
                result.Height = (int)(max.Y - min.Y);
                return result;
            }
            public Vector2 relativeLocation(float x, float y)
            {
                return new Vector2(relativeWidth(x) + bounds.X, relativeHeight(y) + bounds.Y);
            }
        }
        
        public static float percentageW(Rectangle basis, float percentage)
        {
            return percentage * (float)basis.Width;
        }
        public static float percentageH(Rectangle basis, float percentage)
        {
            return percentage * (float)basis.Height;
        }
        public static float percentageW(Rectf basis, float percentage)
        {
            return percentage * basis.Width;
        }
        public static float percentageH(Rectf basis, float percentage)
        {
            return percentage * basis.Height;
        }
        public static Rectangle percentage(Rectangle basis, float x, float y, float width, float height)
        {
            return new Rectangle(
                (int)((float)basis.X + percentageW(basis, x)),
                (int)((float)basis.Y + percentageH(basis, y)),
                (int)percentageW(basis, width),
                (int)percentageH(basis, height));
        }
        public static Rectf percentagef(Rectangle basis, float x, float y, float width, float height)
        {
            return new Rectf(
                ((float)basis.X + percentageW(basis, x)),
                ((float)basis.Y + percentageH(basis, y)),
                percentageW(basis, width),
                percentageH(basis, height));
        }
        public static Rectf percentagef(Rectf basis, float x, float y, float width, float height)
        {
            return new Rectf(
                (basis.X + percentageW(basis, x)),
                (basis.Y + percentageH(basis, y)),
                percentageW(basis, width),
                percentageH(basis, height));
        }
        public static Rectangle percentageEdges(Rectangle basis, float minX, float minY, float maxX, float maxY)
        {
            return percentage(basis, minX, minY, maxX - minX, maxY - minY);
        }
        public static Vector2 percentageLocation(Rectangle bounds, float x, float y)
        {
            return new Vector2(percentageW(bounds, x) + bounds.X, percentageH(bounds, y) + bounds.Y);
        }
        public static bool CircleContainsPoint(Vector2 position, float radius, Vector2 point)
        {
            Vector2 size = new Vector2(radius, radius);
            Vector2 center = position + size/2;
            return (point - position).LengthSquared() < radius * radius;
        }
    }
}