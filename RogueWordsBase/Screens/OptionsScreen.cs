using MknGames.Rogue_Words;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MknGames;
using MknGames.Split_Screen_Dungeon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueWordsBase.Rogue_Words.Screens
{
    public class OptionsScreen : RogueWordsScreen
    {
        struct Button
        {
            Rectf bounds;
        }
        Rectf returnButton;
        Rectf filterButton;
        Rectf soundButton;
        Rectf resetHighScoreBtn;
        public MainMenuScreenClassic menu;

        public OptionsScreen(RogueWordsGame Game) : base(Game)
        {

        }

        public override void LoadContent()
        {
            base.LoadContent();
            returnButton = Backpack.percentagef(ViewportRect, 0, 0, 1, 0.1f);
            filterButton = Backpack.percentagef(ViewportRect, 0, 0.1f, 1, 0.1f);
        }

        public override void Update(GameTime gameTime, float et)
        {
            base.Update(gameTime, et);
            soundButton = Backpack.percentagef(ViewportRect, 0, 0.2f, 1, 0.1f);
            resetHighScoreBtn = Backpack.percentagef(ViewportRect, 0, 0.3f, 1, 0.1f);
            if (pointerRelease())
            {
                if(returnButton.ContainsPoint(pointer()))
                    rwg.SwitchToScreen(menu);
                if (filterButton.ContainsPoint(pointer()))
                    menu.board.ToggleDictionaryFilter();
                if(soundButton.ContainsPoint(pointer()))
                {
                    if (menu.board.volume == 0)
                        menu.board.volume = 1;
                    else
                        menu.board.volume = 0;
                }
                if(resetHighScoreBtn.ContainsPoint(pointer()))
                {
                    menu.board.scoreHigh = 0;
                }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            menu.DrawBanner("return", 0, (Rectangle)returnButton);
            menu.DrawBanner(string.Format("filter: {0}", menu.board.isDictionaryFiltered), 1, (Rectangle)filterButton);
            menu.DrawBanner(string.Format("sound: {0}", menu.board.volume), 2, (Rectangle)soundButton);
            menu.DrawBanner(string.Format("Reset High Score: {0}", menu.board.scoreHigh), 3, (Rectangle)resetHighScoreBtn);
        }

    }
}
