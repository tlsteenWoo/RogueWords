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
        }

        public virtual void Update(GameTime gameTime, float et)
        {

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
