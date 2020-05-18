using MknGames;
using System;

namespace RogueWords
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new GameMG())
            {
                game.Components.Add(new RogueWordsGame(game, false));
                game.Run();
            }
        }
    }
}
