using MknGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueWordsBase.Rogue_Words
{
    public class RogueWordsGameMG : MknGames.GameMG
    {
        public RogueWordsGameMG():base()
        {
            Components.Add(new RogueWordsGame(this));
        }
    }
}
