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
    }
}
