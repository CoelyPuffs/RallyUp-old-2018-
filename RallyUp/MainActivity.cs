using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace RallyUp
{
    [Activity(Label = "Menu", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            ActionBar.Hide();

            Button menuButton = FindViewById<Button>(Resource.Id.menuButton);
            Button firstItemButton = FindViewById<Button>(Resource.Id.firstItem);
            Button secondItemButton = FindViewById<Button>(Resource.Id.secondItem);

            firstItemButton.Click += delegate
            {
                StartActivity(typeof(PingActivity));
            };

            menuButton.Click += delegate
            {
                firstItemButton.Visibility = ViewStates.Visible;
                secondItemButton.Visibility = ViewStates.Visible;
                firstItemButton.Animate().TranslationY(menuButton.Height).SetDuration(500);
                secondItemButton.Animate().TranslationY(menuButton.Height + firstItemButton.Height).SetDuration(500);
            };
        }
    }
}