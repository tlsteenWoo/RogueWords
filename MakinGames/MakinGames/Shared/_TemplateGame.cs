using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MknGames
{
    public class _TemplateGame : DrawableGameComponent
    {
        GameMG game1;
        public _TemplateGame(GameMG game) : base(game)
        {
            game1 = game;
        }
        protected override void LoadContent()
        {
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            float et = (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            game1.spriteBatch.Begin();
            game1.spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
