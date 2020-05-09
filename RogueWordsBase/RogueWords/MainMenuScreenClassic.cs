using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MknGames.Split_Screen_Dungeon;

namespace MknGames.Rogue_Words
{
    public class MainMenuScreenClassic : RogueWordsScreen
    {
        public BoardScreenClassic board;
        CustomRulesScreenClassic custom;
        //public Viewport viewportFull;
        bool drawHowToPlay = false;
        bool drawCredits = false;
        public static bool ignoreRequestedSize = false;

        //inst options
        int difficulty = 2;
        string[] difficultyNames = new string[]
        {
            "Very Easy",
            "Easy",
            "Medium",
            "Hard",
            "Too Hard"
        };
        int boardSize = 1;
        //string[] boardNames = new string[]
        //{
        //    "Small",
        //    "Medium",
        //    "Large"
        //};

        //bool dialogDrawing = false;

        //inst scroll
        float scrollOffset;
        float initialScrollOffset;

        //inst tap location
        Vector2 pointerTapLocation;

        public MainMenuScreenClassic(RogueWordsGame Game) : base(Game)
        {
            custom = new CustomRulesScreenClassic(Game, this);
        }

        public override void LoadContent()
        {
            base.LoadContent();
#if true
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
                float desiredHeight = GraphicsDevice.DisplayMode.TitleSafeArea.Height - 200;
                //galaxy s6: 1440 x 2560
                //nexus 10: 2560 x 1600
                //64.5" sony KD-65ZD9: 3840x2160
                //Tagital t6 watch: 240x240
                float resX = 2560;
                float resY = 1600;
                float ratio = Math.Min(1, desiredHeight / resY);
                pbbw = (int)(resX * ratio);
                pbbh = (int)(resY * ratio);
            }
            game1.graphics.PreferredBackBufferHeight = pbbh;
            game1.graphics.PreferredBackBufferWidth = pbbw;
            game1.IsMouseVisible = true;
            game1.graphics.ApplyChanges();
#else
            game1.graphics.IsFullScreen = true;
            //graphics.PreferredBackBufferWidth = 800;
            //graphics.PreferredBackBufferHeight = 480;
            game1.graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight | DisplayOrientation.Portrait;
            game1.graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.TitleSafeArea.Width;
            game1.graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.TitleSafeArea.Height;
            game1.graphics.ApplyChanges();
#endif
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            game1.Exit();
        }

        public override void Update(GameTime gameTime, float et)
        {
            base.Update(gameTime, et);

            // update input
            if(pointerTap())
            {
                pointerTapLocation = pointerRaw();
                initialScrollOffset = scrollOffset;
            }

            //update scroll
            if(pointerDown())
            {
                Vector2 offset = pointerRaw() - pointerTapLocation;
                scrollOffset = initialScrollOffset + offset.Y;
                float min = 0;
                float max = ViewportRect.Height;
                float top = scrollOffset;
                float bottom = (float)ViewportRect.Height + scrollOffset;
                if (top < min)
                    scrollOffset += min - top;
                if (bottom > max)
                    scrollOffset += max - bottom;
            }

            // tap play
            if (Banner(1).Contains(pointer()) && pointerTap())
            {
                //using(StreamWriter writer = new StreamWriter("main-menu-settings.txt")_
                board = new BoardScreenClassic(rwg, this, this);
                board.requestReset = true;
                switch (difficulty)
                {
                    case 0: //too easy
                        board.assuredBranchLimit = 4;
                        board.vowelChance = 50;
                        board.playerDeadline = 10000;
                        break;
                    case 1: //easy
                        board.assuredBranchLimit = 2;
                        board.vowelChance = 50;
                        board.playerDeadline = 15;
                        break;
                    case 2: //medium
                        board.assuredBranchLimit = 1;
                        board.vowelChance = 50;
                        board.playerDeadline = 10;
                        break;
                    case 3: //hard
                        board.assuredBranchLimit = 1;
                        board.vowelChance = 30;
                        board.playerDeadline = 5;
                        break;
                    case 4: //very hard
                        board.assuredBranchLimit = 1;
                        board.vowelChance = 10;
                        board.playerDeadline = 2;
                        break;
                }
                if (!board.loaded)
                    board.LoadContent();
                ApplyBoardSize();
                ////////// rapid autoplay
                //board.playerDeadline = 0;
                rwg.activeScreen = board;
            }
            if (Banner(6).Contains(pointer()) && pointerTap())
            {
                custom.LoadContent();
                rwg.activeScreen = custom;
            }
            if (/*!dialogDrawing && */Banner(7).Contains(pointer()) && pointerTap())
            {
                drawCredits = true;
            }

            //update spritebatch matrix
            spritebatchMatrix = Matrix.CreateTranslation(0, scrollOffset, 0);
        }
        public void ApplyBoardSize()
        {
            if (ignoreRequestedSize) return;
            switch (boardSize)
            {
                case 0: //small
                    board.SetSize(5, 7);
                    break;
                case 1:
                    board.SetSize(10, 12);
                    break;
                case 2:
                    board.SetSize(15, 20);
                    break;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            //dialogDrawing = false;
            //draw title
            Rectangle titleRect = Banner(0);
            Rectangle titleRectA = Backpack.percentage(titleRect, 0, 0, 1f, 4f / 6f);
            game1.drawString(game1.defaultLargerFont, "Rogue Words", titleRectA, monochrome(1.0f), new Vector2(0.1f, 1), true);
            Rectangle titleRectB = Backpack.percentage(titleRect, 0, 4f/6f, 1f, 1f / 6f);
            game1.drawString(game1.defaultLargerFont, "Printer Friendly Version", titleRectB, monochrome(1.0f), new Vector2(0.5f,0), true);
            Rectangle titleRectC = Backpack.percentage(titleRect, 0, 5f/6f, 1f, 1f / 6f);
            game1.drawString(game1.defaultLargerFont, "The Only Version", titleRectC, monochrome(1.0f), new Vector2(0.9f,0), true);
            
            DrawBanner("Play", 1);

            //draw difficulty
            Rectangle difficultyRect = Banner(2);
            Rectangle dra = Backpack.percentage(difficultyRect, 0.05f, 0, 1, 1f / 3f);
            game1.drawString(game1.defaultLargerFont, "Difficulty", dra, monochrome(1.0f), Vector2.Zero, true);
            Rectangle drb = Backpack.percentage(difficultyRect, 0.10f, 1f / 3f, 1f/5f, 2f / 3f);
            drb.Width = drb.Height*2;
            game1.drawSquare(drb, monochrome(0.8f), 0);
            Rectangle drbtxt = Backpack.percentage(drb, 0.1f, 0.3f, 0.8f, 0.4f);
            game1.drawString(game1.defaultLargerFont, difficultyNames[difficulty], drbtxt, monochrome(0.0f), new Vector2(0.5f, 0.5f), true);
            if(pointerTap() && drb.Contains(pointer()))
            {
                difficulty = (difficulty + 1 ) % difficultyNames.Length;
            }

            //draw board sizes
            Rectangle boardRect = Banner(3);
            boardRect.Height *= 2;
            Rectangle bra = Backpack.percentage(boardRect, 0.05f, 0, 1, 1f / 6f);
            game1.drawString(game1.defaultLargerFont, "Board Size", bra, monochrome(1.0f), Vector2.Zero, true);
            Rectangle brb = Backpack.percentage(boardRect, 0.10f, 1f/6f, 1, 5f / 6f);
            float greyv = 0.5f;
            float darkv = 0.2f;
            for(float i = 0; i < 3; ++i)
            {
                Rectangle r = Backpack.percentage(brb, 0, i / 3f, 1, 1f / 3f);
                Rectf btn = Backpack.percentagef(r, 0, .1f, .8f, .8f);
                btn.Width = btn.Height;
                //Color btnc = i == selected ? Color.Green : monochrome(0.8f);
                Color btnc = monochrome(0.8f);
                if(i == boardSize)
                {
                    btnc = Color.Green;
                }
                Vector2 btnPos = new Vector2(btn.X, btn.Y);
                Vector2 btnCenter = btnPos + new Vector2(btn.Width, btn.Height) / 2;
                float btnRadius = btn.Width / 2;
                game1.drawNgon(btnCenter, monochrome(greyv, 0.9f), 0, 36, btnRadius, 1);
                Rectf btnInner = Backpack.percentagef(btn, 0.1f, 0.1f, 0.8f, 0.8f);
                Rectf btnInner2 = Backpack.percentagef(btnInner, 0.25f, 0.25f, 0.5f, 0.5f);
                game1.drawCircle(btnInner, monochrome(darkv));
                //game1.drawSquare(btnInner2, btnc, 0);
                game1.drawCircle(btnInner2, btnc);
                Vector2 btn2ptr = pointer() - btnCenter;
                if (pointerTap() && btn2ptr.LengthSquared() < btnRadius * btnRadius)
                {
                    boardSize = (int)i;
                }
                Rectf lbl = Backpack.percentagef(r, 0.3f, 0, .7f, 1);
                lbl.X = btn.X + btn.Width;
                string txt = i == 0 ? "Small" : i == 1 ? "Medium" : "Large";
                //string txt = boardNames[(int)i];
                game1.drawStringf(game1.defaultLargerFont,txt, lbl, monochrome(1), new Vector2(0.1f,0.5f), true, 0.6f);
            }
            DrawBanner("How To Play", 5);
            if(!drawHowToPlay && Banner(5).Contains(pointer()) && pointerTap())
            {
                drawHowToPlay = true;
            }
            DrawBanner("Customize", 6);
            DrawBanner("Credits", 7);
            if (drawHowToPlay && Dialog("How To Play",
@"
1. Make words 
by tapping letters.

The goal is to make 
interesting words.
You have to think 
on your feet,
try to make the word 
you are thinking of,
but be prepared for 
some unexpected letters.
Many words have plural 
so add 's' to the end,
also look for do'ing' 
and do'er's.

2. Swap letters by 
swiping (difficult).

Try very easy mode 
if you are having 
trouble.", "OK"))
            {
                drawHowToPlay = false;
            }
            if(drawCredits && Dialog("Credits",
@"Version 1.1
Made by Tim Steen
Thanks to friends, family, the internet,
freesound.org, sagetyrtle, corsica_s,
android, all developers of these tools.

Thank you for playing the game.

Words taken from the English Open Word 
list which built off the UK Advanced 
Cryptics Dictionary.

Copyright (c) J Ross Beresford 1993-1999. 
All Rights Reserved. 
The following restriction is placed on 
the use of this publication: 
if the UK Advanced Cryptics Dictionary 
is used in a software package or 
redistributed in any form, 
the copyright notice must be 
prominently displayed 
and the text of this document must be 
included 
verbatim.
", "ENOUGH!"))
            {
                drawCredits = false;
            }

            //draw cursor
            //game1.drawSquare(pointer(), Color.Red, 0, 10, 10);
        }
        public bool Dialog(string titleText, string bodyText, string buttonText)
        {
            //dialogDrawing = true;
            bool result = false;
            float edge = Backpack.percentageH(ViewportRect, 1f / 20f) / 2;
            Rectangle howToPlayRect = ViewportRect;
            howToPlayRect.Inflate(-edge, -edge);

            Rectangle hra = Backpack.percentage(howToPlayRect, 0, 0, 1, 9f / 10f);
            float blackv = 0.0f;
            game1.drawSquare(hra, monochrome(blackv), 0);

            //draw title
            Rectangle hrtitle = Backpack.percentage(hra, 0, 0, 1, 1 / 10f);
            float whitev = 1.0f;
            game1.drawString(game1.defaultLargerFont, titleText, hrtitle, monochrome(whitev), Vector2.Zero, true);

            //draw body
            Rectangle hrbody = Backpack.percentage(hra, 0, 1f/10f, 1, 9 / 10f);
            //TODO: Clip! Changing viewport or scissor does not work. May need to begin/end spritebatch.
            game1.drawString(game1.defaultLargerFont, bodyText, hrbody, monochrome(whitev), Vector2.Zero, true);

            //draw button
            Rectangle hrb = howToPlayRect;
            hrb.Y += hra.Height;
            hrb.Height = howToPlayRect.Height - hra.Height;
            float grayv = 0.5f;
            game1.drawSquare(hrb, monochrome(grayv), 0);
            float hrokw = Backpack.percentageW(hrb, 1f / 3f);
            Rectangle hrok = Backpack.percentage(hrb, 0.25f, 1f / 10f, 0.5f, 8f / 10f);
            float lightv = 0.8f;
            game1.drawSquare(hrok, monochrome(lightv), 0);
            game1.drawString(game1.defaultLargerFont, buttonText, hrok, monochrome(0), new Vector2(0.5f), true);
            if (hrok.Contains(pointer()) && pointerTap())
            {
                result = true;
            }

            //draw frame
            game1.drawFrame(howToPlayRect, monochrome(grayv), 1);

            return result;
        }
        public Rectangle Banner(float index)
        {
            return Backpack.percentage(ViewportRect, 0, index / 8f, 1, 1f / 8f);
        }
        public void DrawBanner(string text, float index)
        {
            Rectangle r = Banner(index);
            Color bannerc = monochrome(0.4f);
            if (index % 2 == 1)
                bannerc = monochrome(0.3f);
            game1.drawSquare(r, bannerc, 0);
            Rectangle txtr = Backpack.percentage(r, 0, 0.25f, 1, 0.5f);
            game1.drawString(game1.defaultLargerFont, text, txtr, monochrome(1.0f), new Vector2(0.9f, 0.5f), true, 0.8f);
        }
        public void DrawDebug()
        {

        }
    }
}
