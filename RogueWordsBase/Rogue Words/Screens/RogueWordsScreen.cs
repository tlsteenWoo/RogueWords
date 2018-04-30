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
        public bool verticalScrollEnabled;

        //inst scroll
        public float scrollOffset;
        public float initialScrollOffset;
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
                scrollOffset = initialScrollOffset - offset.Y;
                float min = scrollBounds.Y;
                float max = scrollBounds.GetBottom();
                float top = min + scrollOffset;
                float bottom = ViewportRect.Height + scrollOffset;
                if (top < min)
                    scrollOffset += min - top;
                if (bottom > max)
                    scrollOffset += max - bottom;
            }
            
            //update spritebatch matrix
            spritebatchMatrix = Matrix.CreateTranslation(0, -scrollOffset, 0);
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
            if (game1.touchesCurrent.Count > 0)
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
