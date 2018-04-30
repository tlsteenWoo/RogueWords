using MknGames.Rogue_Words;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MknGames;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MknGames.Split_Screen_Dungeon;

namespace RogueWordsBase.Rogue_Words.Screens
{
    public class DiscoveredWordsScreen : RogueWordsScreen
    {
        Rectf returnButton;
        bool returnButtonHover;
        bool returnButtonDown;
        public MainMenuScreenClassic parent;

        public DiscoveredWordsScreen(RogueWordsGame Game) : base(Game)
        {
            verticalScrollEnabled = true;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            if (!parent.board.loadContentComplete)
                parent.board.LoadContent();
        }

        public override void Update(GameTime gameTime, float et)
        {
            base.Update(gameTime, et);
            returnButton = Backpack.percentagef(CalculateTransformedViewportRectf(), 0, 0.00f, 1, 0.1f);
            returnButtonHover = false;
            returnButtonDown = false;
            if (returnButton.ContainsPoint(pointer()))
            {
                returnButtonHover = true;
                if(pointerDown())
                {
                    returnButtonDown = true;
                }
                if (pointerRelease())
                {
                    rwg.activeScreen = parent;
                }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);

            //super expensive on ram 70-300mb!!!!!!
            //draw string appears to be expensive
            //test for intersection with viewport puts work on cpu but the relative cost appears low on pc
            float countY = 0;
            float maxY = ViewportRect.Height;
            float maxX = ViewportRect.Width;
            Rectf drawingRect = CalculateTransformedViewportRectf();
            Rectf itemBaseRectf = Backpack.percentagef(ViewportRect, 0, 0, 0.5f, 0.05f);
            itemBaseRectf.Y += returnButton.Height;
            int startX = (int)Math.Floor(drawingRect.X / itemBaseRectf.Width);
            int startY = (int)Math.Floor(drawingRect.Y / itemBaseRectf.Height);
            var dict = parent.board.dictionary;
            for (int i = startX; i < dict.Keys.Count; ++i)
            {
                float x = i;
                var key = dict.Keys.ElementAt(i);
                var table = dict[key];
                //detect vertical overflow
                bool enteredDrawingBounds = false;
                bool exitDrawingBounds = false;
                foreach(var key2 in table.Keys.OrderBy(length => length))
                {
                    var list = table[key2];
                    for(int k = 0;  k < list.Count; ++k)
                    {
                        float y = countY;
                        countY++;
                        //if (countY < startY) continue;
                        Rectf rect  = itemBaseRectf;
                        rect.X += rect.Width * x;
                        rect.Y += rect.Height * y;
                        var word = list[k];
                        if (rect.Intersectsf(drawingRect))
                        {
                            enteredDrawingBounds = true;
                            Color textColor = monochrome(grey/2);
                            if (charIntStringsContains(parent.board.charIntStrings_discovery, word))
                                textColor = monochrome(white);
                            game1.drawStringf(game1.defaultLargerFont, word, rect, textColor, new Vector2(0), true, 1);
                        }
                        else if(enteredDrawingBounds)
                        {
                            //exited drawing bounds
                            exitDrawingBounds = true;
                        }
                        if(rect.GetBottom() > maxY)
                        {
                            maxY = rect.GetBottom();
                        }
                        if(rect.GetRight() > maxX)
                        {
                            maxX = rect.GetRight();
                        }
                        //spriteBatch.DrawString(game1.defaultLargerFont, word, new Vector2(0, countY), Color.White);
                        if(exitDrawingBounds)
                        {
                            break;
                        }
                    }
                    if(exitDrawingBounds)
                    {
                        break;
                    }
                }
                countY = 0;
                if(maxX > drawingRect.GetRight())
                {
                    break;
                }
            }
            scrollBounds.Width = maxX;
            scrollBounds.Height = maxY;
            DrawButton("return", returnButton, returnButtonHover, returnButtonDown);
        }

        float grey = 0.5f;
        float dark = 0.2f;
        float light = 0.8f;
        float white = 1.0f;
        public void DrawButton(string text, Rectf rectf, bool pointerInside, bool pointerPressing)
        {
            game1.drawSquare(rectf, monochrome(pointerPressing ? dark : pointerInside ? light : grey), 0);
            game1.drawStringf(game1.defaultLargerFont, text, rectf, monochrome(white), new Vector2(0.5f), true, 1);
        }
    }
}
