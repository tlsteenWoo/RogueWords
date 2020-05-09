using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MknGames;
using MknGames.Rogue_Words;
using RogueWordsBase.Rogue_Words;
using System;

namespace RogueWordsAndroid
{
    [Activity(Label = "RogueWordsAndroid"
        , MainLauncher = true
        , Icon = "@drawable/icon"
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
            g.Components.Add(new RogueWordsGame(g));
            SetContentView((View)g.Services.GetService(typeof(View)));
            g.Run();
        }
    }
}

