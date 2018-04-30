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
            returnButton = Backpack.percentagef(CalculateTransformedViewportRectf(), 0, 0.00f, 1, 0.05f);
            if (returnButton.ContainsPoint(pointer()))
            {
                returnButtonHover = true;
                if(pointerDown())
                {
                    returnButtonDown = true;
                    if (!pointerDownOld())
                    {
                        rwg.activeScreen = parent;
                    }
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
            float countX = 0;
            float maxY = ViewportRect.Height;
            float maxX = ViewportRect.Width;
            Rectf drawingRect = CalculateTransformedViewportRectf();
            foreach (var table in parent.board.dictionary)
            {
                foreach (var list in table.Value)
                {
                    foreach (var word in list.Value)
                    {
                        Rectf rect = Backpack.percentagef(ViewportRect, 0.5f * countX, 0.05f + 0.05f * countY, 0.5f, 0.05f);
                        rect.Y += returnButton.Height;
                        if (rect.Intersectsf(drawingRect))
                        {
                            game1.drawStringf(game1.defaultLargerFont, word, rect, monochrome(white), new Vector2(0), true, 1);
                        }
                        if(rect.GetBottom() > maxY)
                        {
                            maxY = rect.GetBottom();
                        }
                        if(rect.GetRight() > maxX)
                        {
                            maxX = rect.GetRight();
                        }
                        countY++;
                        //spriteBatch.DrawString(game1.defaultLargerFont, word, new Vector2(0, countY), Color.White);
                    }
                }
                countX++;
                countY = 0;
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
