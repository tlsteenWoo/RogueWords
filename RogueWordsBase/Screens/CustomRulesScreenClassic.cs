using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueWordsBase.Rogue_Words.Screens;

namespace MknGames.Rogue_Words
{
    public class CustomRulesScreenClassic : RogueWordsScreen
    {
        MainMenuScreenClassic mainMenu;
        BoardScreenClassic board
        {
            get { return mainMenu.board; }
        }

        public CustomRulesScreenClassic(RogueWordsGame Game, MainMenuScreenClassic parent) : base(Game)
        {
            mainMenu = parent;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            board.LoadContent();
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            rwg.activeScreen = mainMenu;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            //draw title
            Rectangle titler = Split_Screen_Dungeon.Backpack.percentage(ViewportRect, 0, 0, 1, 1f / 20f);
            float grayv = 0.5f;
            game1.drawSquare(titler, monochrome(grayv), 0);
            float whitev = 1.0f;
            game1.drawString(game1.defaultLargerFont, "return", titler, monochrome(whitev), Vector2.Zero, true, 0.8f);
            if(titler.Contains(pointer()) && pointerRelease())
            {
                board.WriteSettings();
                rwg.activeScreen = mainMenu;
            }

            //draw sliders
            Rectangle slidersr = Split_Screen_Dungeon.Backpack.percentage(ViewportRect, 0, 0, 1, 0.4f);
            slidersr.Y += titler.Height;
                float darkv = 0.2f;
            for(float f = 0; f < 3; ++f)
            {
                Rectangle sliderr = Split_Screen_Dungeon.Backpack.percentage(slidersr, 0.1f, f / 3f, 1, 1f / 3f);
                Rectangle topr = Split_Screen_Dungeon.Backpack.percentage(sliderr, 0, 0.1f, 1, 0.3f);
                string text = f == 0 ? "Vowel Chance" : f == 1 ? "Assured Branches" : "Move Time Limit";
                game1.drawString(game1.defaultLargerFont, text, topr, monochrome(whitev), new Vector2(0, 0.5f), true);
                Rectangle bottomr = Split_Screen_Dungeon.Backpack.percentage(sliderr, 0, 0.5f, 0.4f, 0.5f);
                if(pointerDown() && bottomr.Contains(pointer()))
                {
                    float dx = pointer().X - bottomr.Left;
                    float xp = dx / (float)bottomr.Width;
                    if (f == 0)
                        board.vowelChance = (int)Math.Round(xp * 100);
                    if (f == 1)
                        board.assuredBranchLimit = (int)Math.Round(xp * 4);
                    if(f == 2)
                        board.playerDeadline = (float)Math.Round(xp * 10) / 2;
                }
                float percentage = 
                    f == 0 ? (float)board.vowelChance / 100f : 
                    f == 1 ? (float)board.assuredBranchLimit /4f : 
                    board.playerDeadline/5f;
                game1.drawSquare(bottomr, monochrome(grayv), 0);
                Rectangle bottomLeftr = bottomr;
                bottomLeftr.Width = (int)((float)bottomLeftr.Width * percentage);
                game1.drawSquare(bottomLeftr, Color.SteelBlue, 0);
                float lightv = 0.8f;
                game1.drawFrame(bottomr, monochrome(grayv), 1);
                //draw knob
                game1.drawSquare(new Vector2(bottomLeftr.Right, bottomLeftr.Center.Y), monochrome(lightv), 0,
                    Split_Screen_Dungeon.Backpack.percentageW(bottomr, 1f / 20f), bottomr.Height);
            }

            //draw play
            Rectangle playr = Split_Screen_Dungeon.Backpack.percentage(ViewportRect, 0, 0.5f, 1, 1 / 10f);
            game1.drawSquare(playr, monochrome(darkv), 0);
            Rectangle playrtxt = Split_Screen_Dungeon.Backpack.percentage(playr, 0, 0.3f, 1, 0.4f);
            game1.drawString(game1.defaultLargerFont, "Play Custom Rules!", playrtxt, monochrome(whitev), new Vector2(1, 0.5f), true);
            if(pointerTap() && playr.Contains(pointer()))
            {
                board.parentScreen = this;
                board.WriteSettings();
                mainMenu.ApplyBoardSize();
                if (board.loaded == false)
                    board.LoadContent();
                board.requestReset = true;
                rwg.activeScreen = board;
            }
        }
    }
}
