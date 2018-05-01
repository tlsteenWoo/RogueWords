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
using System.IO;

namespace MknGames
{
    public class RogueWordsGame : fuckwhit_no_cursing
    {
        public MainMenuScreenClassic menuScreen;
        //public RogueWordsScreen boardScreen;
        public RogueWordsScreen activeScreen;
        public List<string> curseWords = new List<string>();

        public RogueWordsGame(GameMG game) : base(game)
        {
            menuScreen = new MainMenuScreenClassic(this);
            //menuScreen = new MenuScreen(this);
            //boardScreen = new BoardScreen(this);
            activeScreen = menuScreen;
            //activeScreen = boardScreen;
            game1.IsMouseVisible = false;
            //activeScreen = menuScreen;
            game1.Exiting += GameMG_Exiting;
        }

        public void SwitchToScreen(RogueWordsScreen screen)
        {
            if (screen.loadContentComplete == false)
                screen.LoadContent();
            activeScreen = screen;
        }

        public string GetDiscoveryPath()
        {
            return Path.Combine(menuScreen.board.GetGameDirectory(), "Discovery.txt");
        }

        private void GameMG_Exiting(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            menuScreen.board.WriteSettings();
            if (menuScreen.board.loadContentComplete)
            {
                string path = GetDiscoveryPath();
                using (StreamWriter writer = new StreamWriter(File.Create(path)))
                {
                    Console.WriteLine("Discovery: Writing words to {0}.", path);
                    foreach (var charintStrings in menuScreen.board.charIntStrings_discovery)
                    {
                        foreach (var intStrings in charintStrings.Value)
                        {
                            foreach (var str in intStrings.Value)
                            {
                                    writer.WriteLine(str);
                            }
                        }
                    }
                    Console.WriteLine("Discovery: Write complete.");
                }
            }
        }

        protected override void LoadContent()
        {
            //initialize
#if false
            //viewportFull = new Viewport(0,0,395, 702);
            //game1.graphics.PreferredBackBufferWidth = viewportFull.Width;
            //game1.graphics.PreferredBackBufferHeight = viewportFull.Height;
            game1.Window.AllowUserResizing = true;
            //game1.graphics.PreferredBackBufferWidth = 395;
            //game1.graphics.PreferredBackBufferHeight = 702;
            //game1.graphics.PreferredBackBufferWidth = 1200;
            //game1.graphics.PreferredBackBufferHeight = 2500;
            int pbbw = 395;
            int pbbh = 702;
            game1.graphics.IsFullScreen = false;
            {
                float desiredHeight = GraphicsDevice.DisplayMode.TitleSafeArea.Height - 100;
                //galaxy s6: 1440 x 2560
                //nexus 10: 2560 x 1600
                //64.5" sony KD-65ZD9: 3840x2160
                //Tagital t6 watch: 240x240
                //float resX = 1440;
                //float resY = 2560;
                float resX = 2560;
                float resY = 1600;
                float ratio = Math.Min(1, desiredHeight / resY);
                pbbw = (int)(resX * ratio);
                pbbh = (int)(resY * ratio);
            }
            game1.graphics.PreferredBackBufferHeight = pbbh;
            game1.graphics.PreferredBackBufferWidth = pbbw;
            game1.IsMouseVisible = true;
#else
            game1.graphics.IsFullScreen = true;
            //graphics.PreferredBackBufferWidth = 800;
            //graphics.PreferredBackBufferHeight = 480;
            game1.graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight | DisplayOrientation.Portrait;
            game1.graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.TitleSafeArea.Width;
            game1.graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.TitleSafeArea.Height;
#endif
            game1.graphics.ApplyChanges();
            base.LoadContent();
            //game1.graphics.PreferredBackBufferWidth = 425;
            //game1.graphics.PreferredBackBufferHeight = 450;
            //game1.graphics.ApplyChanges();
            menuScreen.LoadContent();
            menuScreen.board.LoadContent();
            //boardScreen.LoadContent();
            //WordListProcessor.DoWorkSon();
            string path = "filter-processed.txt";
            using (StreamReader reader = new StreamReader(BoardScreenClassic.RetrieveStream(path)))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    string[] tokens = line.Split(' ');
                    string word = tokens[0].Trim().ToUpper();
                    curseWords.Add(word);
                }
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float et = (float)gameTime.ElapsedGameTime.TotalSeconds;
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
            game1.spriteBatch.End();

            game1.FlushDebugSquares();
        }
        public void OnBackPressed()
        {
            activeScreen.OnBackPressed();
        }
    }
}
