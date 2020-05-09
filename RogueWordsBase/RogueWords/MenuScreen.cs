using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MknGames.Rogue_Words
{
    public class MenuScreen : RogueWordsScreen
    {
        public MenuScreen(RogueWordsGame Game) : base(Game)
        {
        }

        public override void Update(GameTime gameTime, float et)
        {
            base.Update(gameTime, et);
            if(game1.kclick(Keys.Enter))
            {
                //rwg.activeScreen = rwg.boardScreen;
                return;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            spriteBatch.DrawString(game1.defaultFont, "Press Enter to Play",
                game1.GraphicsDevice.Viewport.Bounds.Center.ToVector2(),
                Color.White);
        }
    }
}
