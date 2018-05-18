using MknGames.Rogue_Words;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MknGames;

namespace RogueWordsBase.Rogue_Words.Screens
{
    public class BoardScreenClassic : BoardScreenBase
    {
        public BoardScreenClassic(RogueWordsGame Game, 
            MainMenuScreenClassic main, 
            RogueWordsScreen parent) : base(Game, main, parent)
        {
        }
    }
}
