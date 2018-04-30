using MknGames.Split_Screen_Dungeon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MknGames.Rogue_Words
{
    public abstract class RogueWordsScreen
    {
        protected RogueWordsGame rwg;
        public Matrix spritebatchMatrix = Matrix.Identity;
        public bool loadContentComplete;

        //inst scroll
        public bool verticalScrollEnabled;
        public bool horizontalScrollEnabled;
        public Vector2 scrollOffset;
        public Vector2 initialScrollOffset;
        public Rectf scrollBounds;

        //inst tap location
        public Vector2 pointerTapLocation;

        protected GameMG game1
        {
            get { return rwg.game1; }
        }
        public Rectangle ViewportRect
        {
            get { return game1.GraphicsDevice.Viewport.Bounds; }
        }
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return game1.GraphicsDevice;
            }
        }

        public RogueWordsScreen(RogueWordsGame Game)
        {
            rwg = Game;
        }

        public virtual void LoadContent()
        {
            loadContentComplete = true;
            scrollBounds = ViewportRect;
        }

        public Rectf CalculateTransformedViewportRectf()
        {
            Rectf rect = ViewportRect;
            Vector2 min = new Vector2(rect.X, rect.Y);
            Vector2 max = min + new Vector2(rect.Width, rect.Height);
            Matrix inv = Matrix.Invert(spritebatchMatrix);
            Vector2 minTransform = Vector2.Transform(min, inv);
            Vector2 maxTransform = Vector2.Transform(max, inv);
            Vector2 sizeTransform = maxTransform - minTransform;
            return new Rectf(minTransform.X, minTransform.Y, sizeTransform.X, sizeTransform.Y);
        }

        public virtual void Update(GameTime gameTime, float et)
        {

            // update input
            if (pointerTap())
            {
                pointerTapLocation = pointerRaw();
                initialScrollOffset = scrollOffset;
            }

            //update scroll
            if (verticalScrollEnabled && pointerDown())
            {
                Vector2 offset = pointerRaw() - pointerTapLocation;
                scrollOffset = initialScrollOffset - offset;
                Rectf viewRectf = ViewportRect;
                viewRectf.X += scrollOffset.X;
                viewRectf.Y += scrollOffset.Y;
                if (viewRectf.Y < scrollBounds.Y)
                    scrollOffset.Y += scrollBounds.Y - viewRectf.Y;
                if (viewRectf.GetBottom() > scrollBounds.GetBottom())
                    scrollOffset.Y += scrollBounds.GetBottom() - viewRectf.GetBottom();
                if (viewRectf.X < scrollBounds.X)
                    scrollOffset.X += scrollBounds.X - viewRectf.X;
                if (viewRectf.GetRight() > scrollBounds.GetRight())
                    scrollOffset.X += scrollBounds.GetRight() - viewRectf.GetRight();
            }
            
            //update spritebatch matrix
            spritebatchMatrix = Matrix.CreateTranslation(-scrollOffset.X, -scrollOffset.Y, 0);
        }

        public static bool charIntStringsContains(Dictionary<char, Dictionary<int, List<string>>> table, string word)
        {
            if (!table.ContainsKey(word[0]))
                return false;
            if (!table[word[0]].ContainsKey(word.Length))
                return false;
            return table[word[0]][word.Length].Contains(word);
        }
        public static void charIntStringsAdd(Dictionary<char, Dictionary<int, List<string>>> table, string word)
        {
            if (!table.ContainsKey(word[0]))
               table.Add(word[0], new Dictionary<int, List<string>>());
            var intStrings = table[word[0]];
            if (!intStrings.ContainsKey(word.Length))
                intStrings.Add(word.Length, new List<string>());
            intStrings[word.Length].Add(word);
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

        }

        public virtual void OnBackPressed()
        {

        }

        public Color monochrome(float lightness, float alpha = 1, float hue = 0, float saturation = 0)
        {
            return game1.monochrome(lightness, alpha, hue, saturation);
        }

        public Vector2 pointer()
        {
            return Vector2.Transform(pointerRaw(), Matrix.Invert(spritebatchMatrix));
        }

        public Vector2 pointerRaw()
        {
            Vector2 pos = game1.mousePosition;
            if (game1.touchesCurrent.Count > 0)
            {
                pos = game1.touchesCurrent[0].Position;
            }
            return pos;
        }

        public bool pointerTap()
        {
            if (game1.touchesCurrent.Count > 0)
            {
                return game1.touchesCurrent[0].State == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Pressed;
            }
            return pointerDown() && pointerUpOld();
        }

        public bool pointerRelease()
        {
            if (game1.touchesCurrent.Count > 0)
            {
                return game1.touchesCurrent[0].State == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Released;
            }
            return pointerUp() && pointerDownOld();
        }

        public bool pointerDown()
        {
            if (game1.touchesCurrent.Count > 0)
            {
                return game1.touchesCurrent[0].State != Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Released &&
                    game1.touchesCurrent[0].State != Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Invalid;
            }
            return game1.lmouse;
        }

        public bool pointerUp()
        {
            return !pointerDown();
        }

        public bool pointerDownOld()
        {
            if (game1.touchesOld.Count > 0)
            {
                return game1.touchesOld[0].State != Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Released &&
                    game1.touchesOld[0].State != Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Invalid;
            }
            return game1.lmouseOld;
        }

        public bool pointerUpOld()
        {
            return !pointerDownOld();
        }
    }
}
