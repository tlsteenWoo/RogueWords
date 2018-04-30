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
            base.LoadContent();
            //game1.graphics.PreferredBackBufferWidth = 425;
            //game1.graphics.PreferredBackBufferHeight = 450;
            //game1.graphics.ApplyChanges();
            menuScreen.LoadContent();
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
