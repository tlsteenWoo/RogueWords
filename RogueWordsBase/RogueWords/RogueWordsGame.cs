using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MknGames.Rogue_Words;
using MknGames._2D;
using MknGames.Split_Screen_Dungeon;

namespace MknGames
{
    public class RogueWordsGame : DrawableGameComponentMG
    {
        public bool paused  = false;
        public bool isDesktop = false;
        public MainMenuScreenClassic menuScreen;
        //public RogueWordsScreen boardScreen;
        public RogueWordsScreen activeScreen;

        public RogueWordsGame(GameMG game, bool isMobile) : base(game)
        {
            menuScreen = new MainMenuScreenClassic(this);
            //menuScreen = new MenuScreen(this);
            //boardScreen = new BoardScreen(this);
            activeScreen = menuScreen;
            //activeScreen = boardScreen;
            game1.IsMouseVisible = false;
            //activeScreen = menuScreen;
            isDesktop = !isMobile;
            game1.Exiting += GameMG_Exiting;
        }

        private void GameMG_Exiting(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            //game1.graphics.ApplyChanges();
            menuScreen.LoadContent();
            //boardScreen.LoadContent();
            //WordListProcessor.DoWorkSon();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float et = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (game1.kclick(Keys.P)) paused = !paused ;
            if (paused) return;
            activeScreen.Update(gameTime, et);
            //game1.debugSquare(Rectf.CreateCentered(activeScreen.pointer(), 5, 5), Color.Green, 0);
            //game1.debugSquare(Rectf.CreateCentered(activeScreen.pointerRaw(), 5, 5), Color.Blue, 0);
        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied,null,null,null,null, activeScreen.spritebatchMatrix);
            activeScreen.Draw(gameTime, game1.spriteBatch);
            game1.DrawDebugSquares();
            if(paused)
                game1.drawString("paused", Backpack.percentage(game1.GraphicsDevice.Viewport.Bounds, 0.1f, 0.4f, 0.8f, 0.1f), Color.White,new Vector2(0.5f),true);
            game1.spriteBatch.End();

            game1.FlushDebugSquares();
        }
        public void OnBackPressed()
        {
            activeScreen.OnBackPressed();
        }
    }
}
