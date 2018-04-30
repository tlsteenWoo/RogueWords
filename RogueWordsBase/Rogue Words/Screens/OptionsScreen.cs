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
        public MainMenuScreenClassic menu;

        public OptionsScreen(RogueWordsGame Game) : base(Game)
        {

        }

        public override void LoadContent()
        {
            base.LoadContent();
            returnButton = Backpack.percentagef(ViewportRect, 0, 0, 1, 0.05f);
            filterButton = Backpack.percentagef(ViewportRect, 0, 0.05f, 1, 0.05f);
        }

        public override void Update(GameTime gameTime, float et)
        {
            base.Update(gameTime, et);
            if(pointerTap() && returnButton.ContainsPoint(pointer()))
            {
                rwg.SwitchToScreen(menu);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            menu.DrawBanner("return", 0, (Rectangle)returnButton);
            menu.DrawBanner(string.Format("filter: {0}", false), 1, (Rectangle)filterButton);
        }

    }
}
