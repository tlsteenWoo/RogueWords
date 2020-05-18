using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MknGames;
using MknGames.Rogue_Words;
using System;

namespace RogueWordsAndroid
{
    [Activity(Label = "Rogue Words"
        , MainLauncher = true
        , Icon = "@drawable/myicon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.FullUser
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout)]
    public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            BoardScreenClassic.RetrieveStream = (string path) =>
            {
                return this.Assets.Open(path);
            };
            var g = new GameMG();
            RogueWordsGame game = new RogueWordsGame(g, true);
            g.HandleBackButton = () =>
            {
                if(game.activeScreen == game.menuScreen)
                {
                    MoveTaskToBack(true);
                }else if(game.activeScreen == game.menuScreen.board)
                {
                    game.activeScreen = game.menuScreen.board.parentScreen;
                }
            };
            g.Components.Add(game);
            SetContentView((View)g.Services.GetService(typeof(View)));
            g.Run();
        }
    }
}

